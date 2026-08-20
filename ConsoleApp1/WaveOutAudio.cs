using System;
using System.Collections.Generic;

#if !AUDIO_UNIT_TESTS
using Internal.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#endif

#if !AUDIO_UNIT_TESTS
// OpenCore AudioDxe publishes this versioned protocol from
// OpenCorePkg/Include/Acidanthera/Protocol/AudioIo.h.
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct EFI_AUDIO_IO_PROTOCOL_PORT
{
    public uint Type;
    public uint SupportedBits;
    public uint SupportedFreqs;
    public uint Device;
    public uint Location;
    public uint Surface;
}

internal unsafe struct EFI_AUDIO_IO_PROTOCOL
{
    public readonly ulong Revision;
    public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, EFI_AUDIO_IO_PROTOCOL_PORT**, ulong*, EFI_STATUS> GetOutputs;
    public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, ulong, byte, sbyte*, EFI_STATUS> RawGainToDecibels;
    public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, ulong, sbyte, uint, uint, byte, ulong, EFI_STATUS> SetupPlayback;
    public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, void*, ulong, ulong, EFI_STATUS> StartPlayback;
    public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, void*, ulong, ulong, delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, void*, void>, void*, EFI_STATUS> StartPlaybackAsync;
    public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, EFI_STATUS> StopPlayback;
}

#endif

internal interface IPcmSampleSink
{
    bool TryWriteSample(short left, short right);
}

#if AUDIO_UNIT_TESTS
internal sealed partial class WaveOutAudio
#else
internal sealed unsafe partial class WaveOutAudio : IPcmSampleSink
#endif
{
    internal sealed class PcmBlockFifo<T>
    {
        private readonly Queue<Entry> _items = new Queue<Entry>();
        private int _byteCount;

        private struct Entry
        {
            internal T Value;
            internal int Length;
        }

        internal int Count => _items.Count;
        internal int ByteCount => _byteCount;

        internal void Enqueue(T value, int length)
        {
            if (length <= 0)
                throw new ArgumentException();

            _items.Enqueue(new Entry
            {
                Value = value,
                Length = length
            });
            _byteCount += length;
        }

        internal int GetLengthAt(int index)
        {
            if (index < 0 || index >= _items.Count)
                throw new ArgumentException();

            int current = 0;
            foreach (Entry item in _items)
            {
                if (current++ == index)
                    return item.Length;
            }

            throw new ArgumentException();
        }

        internal bool TryDequeue(out T value, out int length)
        {
            if (!_items.TryDequeue(out Entry item))
            {
                value = default(T);
                length = 0;
                return false;
            }

            value = item.Value;
            length = item.Length;
            _byteCount -= length;
            return true;
        }

        internal void Clear(Action<T> release)
        {
            while (TryDequeue(out T value, out _))
                release(value);
        }
    }

    internal sealed class PcmInputConverter
    {
        private const int InputBytesPerSample = 2;

        private int _sourceChannels;
        private int _sourceSampleRate;
        private bool _hasPrevious;
        private short _previousLeft;
        private short _previousRight;
        private double _phase;
        private double _step;
        private long _inputIndex;

        internal bool HasPendingInput => _hasPrevious;

        internal int Append(
            byte[] buffer,
            int offset,
            int count,
            int channels,
            int sampleRate,
            IPcmSampleSink sink)
        {
            Configure(channels, sampleRate);

            int inputBytesPerFrame = channels * InputBytesPerSample;
            count -= count % inputBytesPerFrame;
            if (count == 0)
                return 0;

            int sourceOffset = offset;
            int sourceEnd = offset + count;
            while (sourceOffset < sourceEnd)
            {
                short currentLeft = ReadSample(buffer, sourceOffset);
                short currentRight = channels == 1
                    ? currentLeft
                    : ReadSample(buffer, sourceOffset + InputBytesPerSample);

                if (!_hasPrevious)
                {
                    if (!sink.TryWriteSample(currentLeft, currentRight))
                        break;

                    _previousLeft = currentLeft;
                    _previousRight = currentRight;
                    _hasPrevious = true;
                    _inputIndex = 0;
                    _phase = _step;
                    sourceOffset += inputBytesPerFrame;
                    continue;
                }

                long currentIndex = _inputIndex + 1;
                bool stopped = false;
                while (_phase <= currentIndex)
                {
                    double phase = _phase - _inputIndex;
                    short left = Interpolate(_previousLeft, currentLeft, phase);
                    short right = Interpolate(_previousRight, currentRight, phase);
                    if (!sink.TryWriteSample(left, right))
                    {
                        stopped = true;
                        break;
                    }

                    _phase += _step;
                }

                if (stopped)
                    break;

                _previousLeft = currentLeft;
                _previousRight = currentRight;
                _inputIndex = currentIndex;
                sourceOffset += inputBytesPerFrame;
            }

            return sourceOffset - offset;
        }

        internal void Flush(IPcmSampleSink sink)
        {
            if (!_hasPrevious)
                return;

            long endIndex = _inputIndex + 1;
            while (_phase < endIndex)
            {
                if (!sink.TryWriteSample(_previousLeft, _previousRight))
                    return;
                _phase += _step;
            }

            if (_phase >= endIndex)
                _hasPrevious = false;
        }

        private void Configure(int channels, int sampleRate)
        {
            if (_sourceChannels == channels && _sourceSampleRate == sampleRate)
                return;

            _sourceChannels = channels;
            _sourceSampleRate = sampleRate;
            _hasPrevious = false;
            _phase = 0;
            _step = sampleRate / 44100.0;
            _inputIndex = -1;
        }

        private static short ReadSample(byte[] buffer, int offset)
        {
            return (short)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        private static short Interpolate(short previous, short current, double phase)
        {
            double value = previous + (current - previous) * phase;
            if (value > short.MaxValue)
                value = short.MaxValue;
            else if (value < short.MinValue)
                value = short.MinValue;
            return (short)value;
        }
    }

    internal sealed class PcmRingState
    {
        // AudioDxe's HDA output stream is divided into two 256 KiB BDL blocks.
        // WaveOutAudio submits one aligned block at a time and advances the read
        // pointer from its PCM-duration timer, never from an IOC interrupt.
        internal const int DefaultSubmissionBytes = 256 * 1024;

        private readonly int _capacity;
        private readonly int _submissionBytes;
        private readonly int _maximumSubmissionBytes;
        private int _activeLength;

        internal PcmRingState(int capacity, int submissionBytes, int maximumSubmissionBytes)
        {
            _capacity = capacity;
            _submissionBytes = submissionBytes;
            _maximumSubmissionBytes = maximumSubmissionBytes;
        }

        internal int ReadOffset { get; private set; }
        internal int WriteOffset { get; private set; }
        internal int BufferedByteCount { get; private set; }
        internal int WritableByteCount => _capacity - BufferedByteCount;
        internal int ActiveLength => _activeLength;

        internal bool TryCommitWrite(int length)
        {
            if (length <= 0 || length > WritableByteCount)
                return false;

            WriteOffset += length;
            if (WriteOffset >= _capacity)
                WriteOffset -= _capacity;
            BufferedByteCount += length;
            return true;
        }

        internal bool TryStartNext(bool allowShortBuffer, out int offset, out int length)
        {
            offset = 0;
            length = 0;
            if (_activeLength != 0 || BufferedByteCount <= 0)
                return false;
            if (!allowShortBuffer && BufferedByteCount < _submissionBytes)
                return false;

            int contiguousLength;
            if (BufferedByteCount == _capacity)
            {
                contiguousLength = _capacity - ReadOffset;
            }
            else if (WriteOffset >= ReadOffset)
            {
                contiguousLength = WriteOffset - ReadOffset;
            }
            else
            {
                contiguousLength = _capacity - ReadOffset;
            }

            if (contiguousLength > BufferedByteCount)
                contiguousLength = BufferedByteCount;

            length = contiguousLength;
            if (!allowShortBuffer)
            {
                if (length >= _submissionBytes)
                    length -= length % _submissionBytes;
                // A short contiguous tail is valid when more queued bytes already
                // exist after the physical ring boundary. Without this exception
                // a wrapped FIFO would never advance to offset zero. A short
                // non-wrapped queue, however, must wait for a complete block.
                else if (BufferedByteCount == length)
                    return false;
            }

            if (length > _maximumSubmissionBytes)
                length = _maximumSubmissionBytes;
            if (length <= 0 || ReadOffset + length > _capacity)
                return false;

            offset = ReadOffset;
            _activeLength = length;
            return true;
        }

        internal bool CompleteCurrent(out int length)
        {
            length = _activeLength;
            if (length <= 0 || length > BufferedByteCount)
            {
                length = 0;
                return false;
            }

            ReadOffset += length;
            if (ReadOffset >= _capacity)
                ReadOffset -= _capacity;
            BufferedByteCount -= length;
            _activeLength = 0;
            return true;
        }

        internal void CancelCurrent() => _activeLength = 0;

        internal void Reset()
        {
            ReadOffset = 0;
            WriteOffset = 0;
            BufferedByteCount = 0;
            _activeLength = 0;
        }
    }

#if AUDIO_UNIT_TESTS
}
#else
    // Keep no more than about 91 ms of 44.1 kHz stereo PCM queued in the
    // producer-side FIFO.  The HDA DMA ring remains larger, but only this
    // amount can be ahead of the playback cursor.
    private const int RingCapacity = 1024 * 16;
    // 8 KiB is about 46 ms at 44.1 kHz stereo.  This leaves enough PCM for
    // the callback hand-off while keeping the total FIFO below 100 ms.
    private const int SubmissionBytes = 1024 * 8;
    private const int OutputBytesPerFrame = 4;
    private const uint AudioIoBits16 = 1u << 1;
    private const uint AudioIoFreq44Khz = 1u << 5;
    private const byte AudioIoChannelStereo = 2;
    private const uint AudioIoDeviceLine = 0;
    private const uint AudioIoDeviceSpeaker = 1;
    private const uint AudioIoDeviceHeadphones = 2;
    private const uint AudioIoDeviceSpdif = 3;
    private const uint AudioIoDeviceMic = 4;
    private const uint AudioIoDeviceHdmi = 5;

    // The asynchronous callback receives only a native context pointer. Keep
    // the managed owner rooted for the lifetime of the outstanding DMA.
    private static WaveOutAudio s_activeInstance;

    private EFI_AUDIO_IO_PROTOCOL* audio;
    private readonly byte[] m_ringBuffer = new byte[RingCapacity];
    private readonly PcmRingState m_ring = new PcmRingState(
        RingCapacity,
        SubmissionBytes,
        SubmissionBytes);
    private readonly PcmInputConverter m_converter = new PcmInputConverter();
    private bool m_playbackActive;
    private bool m_failed;

    private static bool TryGetOutput(
        EFI_AUDIO_IO_PROTOCOL* candidate,
        out byte outputIndex,
        out int outputScore)
    {
        outputIndex = byte.MaxValue;
        outputScore = int.MinValue;
        if (candidate == null || candidate->GetOutputs == null)
            return false;

        EFI_AUDIO_IO_PROTOCOL_PORT* ports = null;
        ulong portCount = 0;
        EFI_STATUS status = candidate->GetOutputs(candidate, &ports, &portCount);
        if ((ulong)status != EFI_SUCCESS || ports == null || portCount == 0)
        {
            if (ports != null)
                EFI.GC.FreeNative(ports);
            return false;
        }

        byte bestAnalogIndex = byte.MaxValue;
        int bestAnalogScore = int.MinValue;
        byte bestAnyIndex = byte.MaxValue;

        for (ulong index = 0; index < portCount; index++)
        {
            EFI_AUDIO_IO_PROTOCOL_PORT port = ports[index];
            if (port.Type != 0 ||
                (port.SupportedBits & AudioIoBits16) == 0 ||
                (port.SupportedFreqs & AudioIoFreq44Khz) == 0)
            {
                continue;
            }

            if (bestAnyIndex == byte.MaxValue)
                bestAnyIndex = (byte)index;

            int score = port.Device switch
            {
                AudioIoDeviceLine => 300,
                AudioIoDeviceSpeaker => 200,
                AudioIoDeviceHeadphones => 100,
                AudioIoDeviceSpdif => -1,
                AudioIoDeviceMic => -1,
                AudioIoDeviceHdmi => -1,
                _ => -1
            };

            if (score < 0)
                continue;

            // Prefer a rear external line-out when several analog ports are
            // advertised. This matches the usual motherboard jack wiring.
            if (port.Location == 1)
                score += 10;
            if (port.Surface == 0)
                score += 5;

            if (score > bestAnalogScore)
            {
                bestAnalogScore = score;
                bestAnalogIndex = (byte)index;
            }
        }

        EFI.GC.FreeNative(ports);
        // A codec can expose digital and unconnected pins before the physical
        // analog line-out. Prefer analog, but retain a generic fallback for
        // codecs (such as QEMU) that only report "Other".
        outputIndex = bestAnalogIndex != byte.MaxValue ? bestAnalogIndex : bestAnyIndex;
        outputScore = bestAnalogIndex != byte.MaxValue ? bestAnalogScore : 0;
        return outputIndex != byte.MaxValue;
    }

    public WaveOutAudio(int sampleRate)
    {
        EFI_GUID protocolGuid = new EFI_GUID(
            0xA6C4E42D, 0x5F77, 0x4F37, 0xB4, 0x16, 0xD3, 0xA2, 0x9C, 0xE8, 0x67, 0x51);

        EFI_HANDLE* handles = null;
        ulong handleCount = 0;
        EFI_STATUS status = gBS->LocateHandleBuffer(
            ByProtocol,
            (EFI_GUID*)protocolGuid,
            null,
            &handleCount,
            &handles);
        if ((ulong)status != EFI_SUCCESS || handles == null || handleCount == 0)
            return;

        EFI_AUDIO_IO_PROTOCOL* selectedAudio = null;
        byte selectedOutputIndex = byte.MaxValue;
        int selectedOutputScore = int.MinValue;

        for (ulong handleIndex = 0; handleIndex < handleCount; handleIndex++)
        {
            EFI_AUDIO_IO_PROTOCOL* candidate = null;
            status = gBS->HandleProtocol(
                handles[handleIndex],
                (EFI_GUID*)protocolGuid,
                (void**)&candidate);
            if ((ulong)status != EFI_SUCCESS ||
                candidate == null ||
                candidate->SetupPlayback == null ||
                candidate->StartPlaybackAsync == null ||
                candidate->StopPlayback == null)
                continue;

            if (!TryGetOutput(candidate, out byte candidateOutputIndex, out int candidateOutputScore))
                continue;

            if (selectedAudio == null || candidateOutputScore > selectedOutputScore)
            {
                selectedAudio = candidate;
                selectedOutputIndex = candidateOutputIndex;
                selectedOutputScore = candidateOutputScore;
            }
        }

        gBS->FreePool(handles);

        audio = selectedAudio;
        byte outputIndex = selectedOutputIndex;
        if (audio == null || outputIndex == byte.MaxValue)
            return;

        status = audio->SetupPlayback(audio, 1UL << outputIndex, 0, AudioIoFreq44Khz, AudioIoBits16, AudioIoChannelStereo, 0);
        if ((ulong)status != EFI_SUCCESS)
            return;

        s_activeInstance = this;
    }

    private void StartNext(bool allowShortBuffer)
    {
        if (m_failed || m_playbackActive ||
            !m_ring.TryStartNext(allowShortBuffer, out int offset, out int length))
            return;

        m_playbackActive = true;
        WaveOutAudio owner = this;
        IntPtr context = Unsafe.As<WaveOutAudio, IntPtr>(ref owner);
        EFI_STATUS status;
        fixed (byte* ptr = m_ringBuffer)
        {
            status = audio->StartPlaybackAsync(
                audio,
                ptr + offset,
                (ulong)length,
                0,
                &PlaybackTransferStopped,
                (void*)context);
        }

        if ((ulong)status != EFI_SUCCESS)
        {
            m_ring.CancelCurrent();
            m_playbackActive = false;
            m_failed = true;
        }
    }

    [UnmanagedCallersOnly]
    static void PlaybackTransferStopped(EFI_AUDIO_IO_PROTOCOL* audio, void* context)
    {
        IntPtr pointer = (IntPtr)context;
        WaveOutAudio owner = Unsafe.As<IntPtr, WaveOutAudio>(ref pointer);

        if (!owner.m_playbackActive ||
            !owner.m_ring.CompleteCurrent(out _))
            return;

        owner.m_playbackActive = false;
        // The driver has consumed the callback before entering here. Submit
        // the next queued segment synchronously so no managed timer can add
        // an audible gap or let the hardware lap the software write tail.
        owner.StartNext(false);
    }

    public bool TryWriteSample(short left, short right)
    {
        if (m_failed || m_ring.WritableByteCount < OutputBytesPerFrame)
            return false;

        int offset = m_ring.WriteOffset;
        fixed (byte* dst = m_ringBuffer)
        {
            dst[offset] = (byte)left;
            dst[offset + 1] = (byte)(left >> 8);
            dst[offset + 2] = (byte)right;
            dst[offset + 3] = (byte)(right >> 8);
        }

        return m_ring.TryCommitWrite(OutputBytesPerFrame);
    }

    internal int WritePcm(byte[] buffer, int offset, int count, int channels, int sampleRate)
    {
        if (m_failed || buffer == null || offset < 0 || count < 0 ||
            offset > buffer.Length - count || (channels != 1 && channels != 2) ||
            sampleRate <= 0)
            return 0;

        int inputBytesPerFrame = channels * 2;
        if (count % inputBytesPerFrame != 0)
            return 0;

        int consumedTotal = 0;
        while (consumedTotal < count && !m_failed)
        {
            int consumed = m_converter.Append(
                buffer,
                offset + consumedTotal,
                count - consumedTotal,
                channels,
                sampleRate,
                this);
            consumedTotal += consumed;

            // Do not start a short block from the producer path. This gives
            // the emulator a real FIFO prebuffer and prevents tiny writes
            // from becoming audible gaps. The completion callback drains the
            // final short tail after a full block has finished.
            StartNext(false);

            if (consumedTotal == count)
                break;

            // The ring is full. A Boot Services stall yields to the timer
            // event that invokes the playback-completed callback; it is not a
            // playback delay or a second scheduling mechanism.
            if (consumed == 0)
                Task.Yield();
        }

        return consumedTotal;
    }
}
#endif
