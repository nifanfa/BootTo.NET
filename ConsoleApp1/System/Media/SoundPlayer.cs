using Internal.Runtime.CompilerServices;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Media
{
    public unsafe class SoundPlayer
    {
        private const int PcmFormat = 1;
        private const int BitsPerSample = 16;
        private const int OutputSampleRate = 44100;
        private const int OutputChannels = 2;
        // Keep enough PCM in AudioDxe to absorb frame-time jitter.
        private const ulong MinimumStreamingBufferBytes = 96UL * 1024UL;
        private const int SynchronousChunkBytes = 4096;

        private string _soundLocation = string.Empty;
        private Stream _stream;
        private readonly int _pcmChannels;
        private readonly int _pcmSampleRate;
        private ulong _streamInputFrames;
        private ulong _streamOutputFrames;
        private bool _hasStreamLastFrame;
        private short _streamLastLeft;
        private short _streamLastRight;

        private struct WavFormat
        {
            internal int Channels;
            internal int SampleRate;
            internal int BlockAlign;
            internal int DataLength;
        }

        public SoundPlayer()
        {
        }

        public SoundPlayer(string soundLocation)
        {
            SoundLocation = soundLocation;
        }

        public SoundPlayer(Stream stream)
        {
            Stream = stream;
        }

        public SoundPlayer(int channels, int sampleRate)
        {
            if (channels < 1 || channels > 2)
                throw new ArgumentException("PCM channels must be one or two.");
            if (sampleRate <= 0)
                throw new ArgumentException("PCM sample rate must be positive.");

            _pcmChannels = channels;
            _pcmSampleRate = sampleRate;
        }

        public string SoundLocation
        {
            get => _soundLocation;
            set
            {
                _soundLocation = value ?? string.Empty;
                _stream = null;
            }
        }

        public Stream Stream
        {
            get => _stream;
            set
            {
                _stream = value;
                _soundLocation = string.Empty;
            }
        }

        public void PlaySync()
        {
            Stream input = _stream;
            bool ownsStream = false;
            if (input == null)
            {
                if (string.IsNullOrEmpty(_soundLocation))
                    throw new InvalidOperationException("No sound location or stream has been configured.");

                input = new FileStream(_soundLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
                ownsStream = true;
            }
            else if (!input.CanRead)
            {
                throw new InvalidOperationException("The sound stream is not readable.");
            }

            try
            {
                if (input.CanSeek)
                    input.Position = 0;

                WavFormat format;
                if (!TryParse(input, out format))
                    throw new InvalidOperationException("The sound data is not a supported PCM WAV file.");

                if (!EnsurePcmAudio(OutputSampleRate))
                    throw new InvalidOperationException("No UEFI audio output is available.");

                byte[] buffer = new byte[format.DataLength];
                if (ReadFully(input, buffer, buffer.Length) != buffer.Length ||
                    WritePcm(buffer, 0, buffer.Length, format.Channels, format.SampleRate) != buffer.Length)
                {
                    throw new InvalidOperationException("Audio playback did not accept the complete WAV data stream.");
                }

                if (!CompletePlayback())
                    throw new InvalidOperationException("Audio playback did not consume the complete WAV data stream.");
            }
            finally
            {
                if (ownsStream)
                    input.Close();
            }
        }

        // Appends signed 16-bit PCM. AudioDxe owns the streaming cache; this
        // call applies backpressure when producers outrun playback.
        public int Play(byte[] buffer, int offset, int count)
        {
            if (_pcmChannels == 0)
                throw new InvalidOperationException("This SoundPlayer was not configured for PCM playback.");
            if (buffer == null || offset < 0 || count <= 0 || offset > buffer.Length - count)
                return 0;

            int frameBytes = _pcmChannels * 2;
            if (count % frameBytes != 0)
                return 0;

            return AppendPcm(buffer, offset, count, _pcmChannels, _pcmSampleRate);
        }

        internal ulong GetBufferedInputFrameCount(int inputSampleRate)
        {
            if (inputSampleRate <= 0 || (void*)s_playbackEvent == null)
                return 0;

            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            ulong outputFrames = s_remainingBytes / (OutputChannels * sizeof(short));
            gBS->RestoreTPL(oldTpl);

            return (outputFrames * (ulong)inputSampleRate + OutputSampleRate - 1) /
                OutputSampleRate;
        }

        private static int ReadFully(Stream stream, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int bytesRead = stream.Read(buffer, offset, count - offset);
                if (bytesRead <= 0)
                    break;
                offset += bytesRead;
            }
            return offset;
        }

        private static bool TryParse(Stream stream, out WavFormat format)
        {
            format = default;
            byte[] header = new byte[12];
            if (ReadFully(stream, header, header.Length) != header.Length ||
                !Tag(header, 0, (byte)'R', (byte)'I', (byte)'F', (byte)'F') ||
                !Tag(header, 8, (byte)'W', (byte)'A', (byte)'V', (byte)'E'))
                return false;

            bool haveFormat = false;
            byte[] chunkHeader = new byte[8];
            byte[] scratch = new byte[256];
            while (true)
            {
                if (ReadFully(stream, chunkHeader, chunkHeader.Length) != chunkHeader.Length)
                    return false;

                uint chunkSize = ReadUInt32(chunkHeader, 4);
                if (chunkSize > int.MaxValue)
                    return false;
                int size = (int)chunkSize;

                if (Tag(chunkHeader, 0, (byte)'f', (byte)'m', (byte)'t', (byte)' '))
                {
                    if (size < 16)
                        return false;

                    byte[] fmt = new byte[16];
                    if (ReadFully(stream, fmt, fmt.Length) != fmt.Length)
                        return false;

                    int encoding = ReadUInt16(fmt, 0);
                    int channels = ReadUInt16(fmt, 2);
                    int sampleRate = ReadInt32(fmt, 4);
                    int blockAlign = ReadUInt16(fmt, 12);
                    int bits = ReadUInt16(fmt, 14);
                    if (encoding != PcmFormat || channels < 1 || channels > 2 ||
                        sampleRate <= 0 || blockAlign != channels * 2 || bits != BitsPerSample)
                        return false;

                    format.Channels = channels;
                    format.SampleRate = sampleRate;
                    format.BlockAlign = blockAlign;
                    haveFormat = true;
                    if (!Skip(stream, size - fmt.Length, scratch))
                        return false;
                }
                else if (Tag(chunkHeader, 0, (byte)'d', (byte)'a', (byte)'t', (byte)'a'))
                {
                    if (!haveFormat || size < format.BlockAlign || size % format.BlockAlign != 0)
                        return false;
                    format.DataLength = size;
                    return true;
                }
                else if (!Skip(stream, size, scratch))
                {
                    return false;
                }

                if ((size & 1) != 0 && !Skip(stream, 1, scratch))
                    return false;
            }
        }

        private static bool Tag(byte[] data, int offset, byte a, byte b, byte c, byte d)
            => offset >= 0 && offset + 4 <= data.Length && data[offset] == a &&
                data[offset + 1] == b && data[offset + 2] == c && data[offset + 3] == d;

        private static bool Skip(Stream stream, int count, byte[] scratch)
        {
            while (count > 0)
            {
                int request = count < scratch.Length ? count : scratch.Length;
                if (ReadFully(stream, scratch, request) != request)
                    return false;
                count -= request;
            }
            return true;
        }

        private static int ReadUInt16(byte[] data, int offset)
            => data[offset] | (data[offset + 1] << 8);

        private static int ReadInt32(byte[] data, int offset)
            => data[offset] | (data[offset + 1] << 8) |
                (data[offset + 2] << 16) | (data[offset + 3] << 24);

        private static uint ReadUInt32(byte[] data, int offset)
            => (uint)ReadInt32(data, offset);

        // OpenCorePkg/Include/Acidanthera/Protocol/AudioIo.h
        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_AUDIO_IO_PROTOCOL_PORT
        {
            public uint Type;
            public uint SupportedBits;
            public uint SupportedFreqs;
            public uint Device;
            public uint Location;
            public uint Surface;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_AUDIO_IO_PROTOCOL
        {
            public readonly ulong Revision;
            public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, EFI_AUDIO_IO_PROTOCOL_PORT**, ulong*, EFI_STATUS> GetOutputs;
            public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, ulong, byte, sbyte*, EFI_STATUS> RawGainToDecibels;
            public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, ulong, sbyte, uint, uint, byte, ulong, EFI_STATUS> SetupPlayback;
            public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, void*, ulong, ulong, EFI_STATUS> StartPlayback;
            public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, void*, ulong, ulong, delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, void*, void>, void*, EFI_STATUS> StartPlaybackAsync;
            public readonly delegate* unmanaged<EFI_AUDIO_IO_PROTOCOL*, EFI_STATUS> StopPlayback;
        }

        private const ulong AudioIoRevision = 4;
        private const uint AudioIoTypeOutput = 0;
        private const uint AudioIoBits16 = 1u << 1;

        private const uint AudioIoFreq8Khz = 1u << 0;
        private const uint AudioIoFreq11Khz = 1u << 1;
        private const uint AudioIoFreq16Khz = 1u << 2;
        private const uint AudioIoFreq22Khz = 1u << 3;
        private const uint AudioIoFreq32Khz = 1u << 4;
        private const uint AudioIoFreq44Khz = 1u << 5;
        private const uint AudioIoFreq48Khz = 1u << 6;
        private const uint AudioIoFreq88Khz = 1u << 7;
        private const uint AudioIoFreq96Khz = 1u << 8;
        private const uint AudioIoFreq192Khz = 1u << 9;

        private const uint AudioIoDeviceLine = 0;
        private const uint AudioIoDeviceSpeaker = 1;
        // 25% volume
        private const sbyte DefaultGain = -12;

        private static EFI_AUDIO_IO_PROTOCOL* s_audioIo;
        private static EFI_EVENT s_playbackEvent;
        private static ulong s_outputIndexMask;
        private static uint s_outputFrequency;
        private static uint s_outputFrequencies;
        private const ulong PlaybackDelay = 0;
        private static sbyte s_gain = DefaultGain;
        private static bool s_failed;
        private static ulong s_remainingBytes;
        private static bool s_streamingPlaybackActive;

        private static bool IsAvailable =>
            s_audioIo != null && (void*)s_playbackEvent != null && !s_failed;

        private static void Initialize(int sampleRate)
        {
            if (!TryGetFrequency(sampleRate, out uint initialFrequency))
            {
                s_failed = true;
                return;
            }

            if (s_audioIo != null)
                StopPlayback(true);

            if ((void*)s_playbackEvent == null)
            {
                EFI_EVENT playbackEvent = null;
                EFI_STATUS eventStatus = gBS->CreateEvent(
                    0,
                    TPL_NOTIFY,
                    null,
                    null,
                    &playbackEvent);
                if ((ulong)eventStatus != EFI_SUCCESS)
                {
                    s_audioIo = null;
                    s_failed = true;
                    return;
                }
                s_playbackEvent = playbackEvent;
            }

            EFI_AUDIO_IO_PROTOCOL* audioIo = null;
            ulong outputIndexMask = 0;
            uint outputFrequencies = 0;
            EFI_STATUS status = LocateSpeakerAudioIo(
                &audioIo,
                &outputIndexMask,
                &outputFrequencies,
                AudioIoBits16,
                initialFrequency);
            if ((ulong)status != EFI_SUCCESS || audioIo == null || outputIndexMask == 0)
            {
                s_audioIo = null;
                s_failed = true;
                return;
            }

            s_audioIo = audioIo;
            s_outputIndexMask = outputIndexMask;
            s_outputFrequency = initialFrequency;
            s_outputFrequencies = outputFrequencies;
            s_failed = false;
        }

        private static bool EnsurePcmAudio(int sampleRate)
        {
            if (!TryGetFrequency(sampleRate, out uint frequency))
                return false;

            if (s_audioIo == null || s_outputFrequency != frequency)
                Initialize(sampleRate);

            return IsAvailable;
        }

        private static EFI_STATUS LocateSpeakerAudioIo(
            EFI_AUDIO_IO_PROTOCOL** audioIo,
            ulong* outputIndexMask,
            uint* outputFrequencies,
            uint bits,
            uint frequency)
        {
            *audioIo = null;
            *outputIndexMask = 0;
            *outputFrequencies = 0;

            EFI_GUID audioIoGuid = new EFI_GUID(
                0xA6C4E42D, 0x5F77, 0x4F37, 0xB4, 0x16, 0xD3, 0xA2, 0x9C, 0xE8, 0x67, 0x51);
            EFI_STATUS status = FindSpeakerAudioIo(
                (EFI_GUID*)audioIoGuid,
                audioIo,
                outputIndexMask,
                outputFrequencies,
                bits,
                frequency);
            if ((ulong)status == EFI_SUCCESS)
                return status;

            // Some revision-4 builds publish the updated protocol GUID.
            audioIoGuid = new EFI_GUID(
                0x22266891, 0x2032, 0x4BAE, 0xB7, 0xB5, 0x43, 0x74, 0xE7, 0x32, 0x09, 0x49);
            return FindSpeakerAudioIo(
                (EFI_GUID*)audioIoGuid,
                audioIo,
                outputIndexMask,
                outputFrequencies,
                bits,
                frequency);
        }

        private static EFI_STATUS FindSpeakerAudioIo(
            EFI_GUID* audioIoGuid,
            EFI_AUDIO_IO_PROTOCOL** selectedAudioIo,
            ulong* selectedOutputIndexMask,
            uint* selectedOutputFrequencies,
            uint bits,
            uint frequency)
        {
            EFI_HANDLE* handles = null;
            ulong handleCount = 0;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                ByProtocol,
                audioIoGuid,
                null,
                &handleCount,
                &handles);
            if ((ulong)status != EFI_SUCCESS)
                return status;

            EFI_STATUS result = (EFI_STATUS)EFI_NOT_FOUND;
            for (ulong index = 0; index < handleCount; index++)
            {
                EFI_AUDIO_IO_PROTOCOL* candidate = null;
                status = gBS->HandleProtocol(
                    handles[index],
                    audioIoGuid,
                    (void**)&candidate);
                if ((ulong)status != EFI_SUCCESS || !IsUsableAudioIo(candidate))
                    continue;

                ulong outputIndexMask = GetConnectedSpeakerOutputMask(
                    candidate,
                    bits,
                    frequency,
                    out uint outputFrequencies);
                if (outputIndexMask == 0)
                    continue;

                *selectedAudioIo = candidate;
                *selectedOutputIndexMask = outputIndexMask;
                *selectedOutputFrequencies = outputFrequencies;
                result = (EFI_STATUS)EFI_SUCCESS;
                break;
            }

            if (handles != null)
                gBS->FreePool(handles);
            return result;
        }

        private static bool IsUsableAudioIo(EFI_AUDIO_IO_PROTOCOL* audioIo)
        {
            return audioIo != null &&
                audioIo->Revision == AudioIoRevision &&
                audioIo->GetOutputs != null &&
                audioIo->SetupPlayback != null &&
                audioIo->StartPlayback != null &&
                audioIo->StartPlaybackAsync != null &&
                audioIo->StopPlayback != null;
        }

        private static int WritePcm(
            byte[] buffer,
            int offset,
            int count,
            int channels,
            int sampleRate)
        {
            if (buffer == null || offset < 0 || count <= 0 ||
                offset > buffer.Length - count || channels <= 0 || channels > 2 ||
                sampleRate <= 0)
                return 0;

            int frameBytes = channels * 2;
            if (count % frameBytes != 0)
                return 0;

            byte[] output = ConvertToOutputPcm(buffer, offset, count, channels, sampleRate);
            if (output == null || !EnsurePcmAudio(OutputSampleRate))
                return 0;

            return PlayOutputPcmChunks(output) ? count : 0;
        }

        private static bool PlayOutputPcmChunks(byte[] output)
        {
            if (output == null || output.Length == 0 ||
                output.Length % (OutputChannels * 2) != 0)
                return false;

            // This is a complete synchronous clip. Do not append it to a
            // previous asynchronous stream, but keep all of its chunks in one
            // continuous HDA stream.
            StopPlayback(false);

            int offset = 0;
            while (offset < output.Length)
            {
                int count = output.Length - offset;
                if (count > SynchronousChunkBytes)
                    count = SynchronousChunkBytes;

                if (!QueueOutputPcmChunk(output, offset, count))
                {
                    StopPlayback(false);
                    return false;
                }
                offset += count;
            }

            if (!WaitForStreamingCompletion())
            {
                StopPlayback(false);
                return false;
            }

            StopPlayback(false);
            return true;
        }

        private static bool QueueOutputPcmChunk(byte[] output, int offset, int count)
        {
            if (output == null || offset < 0 || count <= 0 ||
                offset > output.Length - count || count % (OutputChannels * 2) != 0)
                return false;

            ulong outputLength = (ulong)count;
            if (!ReserveStreamingCapacity(outputLength))
                return false;

            void* rawBuffer = GarbageCollector.AllocateNative(outputLength);
            if (rawBuffer == null)
            {
                ReleaseStreamingCapacity(outputLength);
                return false;
            }

            fixed (byte* source = output)
                Unsafe.CopyBlock(rawBuffer, source + offset, outputLength);

            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            EFI_STATUS status = s_audioIo->SetupPlayback(
                s_audioIo,
                s_outputIndexMask,
                s_gain,
                s_outputFrequency,
                AudioIoBits16,
                OutputChannels,
                PlaybackDelay);
            if ((ulong)status == EFI_SUCCESS)
            {
                status = s_audioIo->StartPlaybackAsync(
                    s_audioIo,
                    rawBuffer,
                    outputLength,
                    0,
                    &StreamingPlaybackDone,
                    (void*)outputLength);
            }
            gBS->RestoreTPL(oldTpl);

            gBS->FreePool(rawBuffer);
            if ((ulong)status == EFI_SUCCESS)
            {
                s_streamingPlaybackActive = true;
                return true;
            }

            ReleaseStreamingCapacity(outputLength);
            s_failed = true;
            return false;
        }

        private static bool WaitForStreamingCompletion()
        {
            while (true)
            {
                EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
                bool complete = s_remainingBytes == 0;
                gBS->RestoreTPL(oldTpl);

                if (complete)
                    return IsAvailable;
                if (!IsAvailable)
                    return false;

                // Let AudioDxe's periodic HDA poll callback release completed
                // transfers and advance the streaming queue.
                gBS->Stall(1000);
            }
        }

        private int AppendPcm(
            byte[] buffer,
            int offset,
            int count,
            int channels,
            int sampleRate)
        {
            if (buffer == null || offset < 0 || count <= 0 ||
                offset > buffer.Length - count || channels <= 0 || channels > 2 ||
                sampleRate <= 0)
                return 0;

            int frameBytes = channels * 2;
            if (count % frameBytes != 0)
                return 0;

            byte[] output = ConvertStreamingPcm(
                buffer,
                offset,
                count,
                channels,
                sampleRate,
                out ulong outputFrames);
            if (output == null || !EnsurePcmAudio(OutputSampleRate))
                return 0;

            ulong outputLength = (ulong)output.Length;
            if (outputLength == 0)
            {
                CommitStreamingPcmState(buffer, offset, count, channels, outputFrames);
                return count;
            }

            if (!ReserveStreamingCapacity(outputLength))
                return 0;

            void* rawBuffer = GarbageCollector.AllocateNative(outputLength);
            if (rawBuffer == null)
            {
                ReleaseStreamingCapacity(outputLength);
                return 0;
            }

            fixed (byte* source = output)
                Unsafe.CopyBlock(rawBuffer, source, outputLength);

            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            EFI_STATUS status = s_audioIo->SetupPlayback(
                s_audioIo,
                s_outputIndexMask,
                s_gain,
                s_outputFrequency,
                AudioIoBits16,
                OutputChannels,
                PlaybackDelay);
            if ((ulong)status == EFI_SUCCESS)
            {
                status = s_audioIo->StartPlaybackAsync(
                    s_audioIo,
                    rawBuffer,
                    outputLength,
                    0,
                    &StreamingPlaybackDone,
                    (void*)outputLength);
            }
            gBS->RestoreTPL(oldTpl);

            gBS->FreePool(rawBuffer);
            if ((ulong)status == EFI_SUCCESS)
            {
                s_streamingPlaybackActive = true;
                CommitStreamingPcmState(buffer, offset, count, channels, outputFrames);
                return count;
            }

            ReleaseStreamingCapacity(outputLength);
            s_failed = true;
            return 0;
        }

        private byte[] ConvertStreamingPcm(
            byte[] input,
            int offset,
            int count,
            int inputChannels,
            int inputSampleRate,
            out ulong outputFrames)
        {
            outputFrames = 0;
            int inputFrames = count / (inputChannels * 2);
            if (inputFrames == 0)
                return null;

            ulong totalInputFrames = _streamInputFrames + (ulong)inputFrames;
            ulong lastInputPosition = (totalInputFrames - 1) * (ulong)OutputSampleRate;
            ulong lastOutputFrame = lastInputPosition / (ulong)inputSampleRate;
            if (lastOutputFrame < _streamOutputFrames)
                return new byte[0];

            outputFrames = lastOutputFrame - _streamOutputFrames + 1;
            if (outputFrames > int.MaxValue / (OutputChannels * 2))
            {
                outputFrames = 0;
                return null;
            }

            byte[] output = new byte[(int)outputFrames * OutputChannels * 2];
            ulong previousInputFrames = _streamInputFrames;
            ulong combinedBaseFrame = previousInputFrames == 0 ? 0 : previousInputFrames - 1;

            for (ulong outputFrame = 0; outputFrame < outputFrames; outputFrame++)
            {
                ulong globalOutputFrame = _streamOutputFrames + outputFrame;
                ulong sourcePosition = globalOutputFrame * (ulong)inputSampleRate;
                ulong sourceFrame = sourcePosition / (ulong)OutputSampleRate;
                int fraction = (int)(sourcePosition % (ulong)OutputSampleRate);
                ulong nextFrame = sourceFrame + 1 < totalInputFrames
                    ? sourceFrame + 1
                    : sourceFrame;

                int sourceLocalFrame = checked((int)(sourceFrame - combinedBaseFrame));
                int nextLocalFrame = checked((int)(nextFrame - combinedBaseFrame));
                short left = InterpolateStreamingSample(
                    input,
                    offset,
                    inputChannels,
                    previousInputFrames,
                    sourceLocalFrame,
                    nextLocalFrame,
                    0,
                    fraction);
                short right = inputChannels == 1
                    ? left
                    : InterpolateStreamingSample(
                        input,
                        offset,
                        inputChannels,
                        previousInputFrames,
                        sourceLocalFrame,
                        nextLocalFrame,
                        1,
                        fraction);

                int outputOffset = checked((int)outputFrame * OutputChannels * 2);
                output[outputOffset] = (byte)left;
                output[outputOffset + 1] = (byte)(left >> 8);
                output[outputOffset + 2] = (byte)right;
                output[outputOffset + 3] = (byte)(right >> 8);
            }

            return output;
        }

        private short InterpolateStreamingSample(
            byte[] input,
            int offset,
            int channels,
            ulong previousInputFrames,
            int sourceLocalFrame,
            int nextLocalFrame,
            int channel,
            int fraction)
        {
            int source = ReadStreamingSample(
                input,
                offset,
                channels,
                previousInputFrames,
                sourceLocalFrame,
                channel);
            int next = ReadStreamingSample(
                input,
                offset,
                channels,
                previousInputFrames,
                nextLocalFrame,
                channel);
            long difference = next - source;
            return (short)(source + (difference * fraction + OutputSampleRate / 2) / OutputSampleRate);
        }

        private int ReadStreamingSample(
            byte[] input,
            int offset,
            int channels,
            ulong previousInputFrames,
            int localFrame,
            int channel)
        {
            if (_hasStreamLastFrame && localFrame == 0 && previousInputFrames != 0)
                return channel == 0 ? _streamLastLeft : _streamLastRight;

            int inputFrame = localFrame - (previousInputFrames == 0 ? 0 : 1);
            int sampleOffset = offset + (inputFrame * channels + channel) * 2;
            return (short)(input[sampleOffset] | (input[sampleOffset + 1] << 8));
        }

        private void CommitStreamingPcmState(
            byte[] input,
            int offset,
            int count,
            int channels,
            ulong outputFrames)
        {
            int inputFrames = count / (channels * 2);
            int lastOffset = offset + (inputFrames - 1) * channels * 2;
            _streamLastLeft = (short)(input[lastOffset] | (input[lastOffset + 1] << 8));
            _streamLastRight = channels == 1
                ? _streamLastLeft
                : (short)(input[lastOffset + 2] | (input[lastOffset + 3] << 8));
            _hasStreamLastFrame = true;
            _streamInputFrames += (ulong)inputFrames;
            _streamOutputFrames += outputFrames;
        }

        private static bool ReserveStreamingCapacity(ulong byteCount)
        {
            if (byteCount == 0)
                return false;

            ulong bufferLimit = MinimumStreamingBufferBytes;
            if (byteCount > bufferLimit)
                return false;

            if (!IsAvailable)
                return false;

            while (IsAvailable)
            {
                EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
                bool hasCapacity = s_remainingBytes <= bufferLimit - byteCount;
                if (hasCapacity)
                    s_remainingBytes += byteCount;
                gBS->RestoreTPL(oldTpl);

                if (hasCapacity)
                    return true;

                gBS->Stall(1000);
            }

            return false;
        }

        private static void ReleaseStreamingCapacity(ulong byteCount)
        {
            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            if (byteCount >= s_remainingBytes)
                s_remainingBytes = 0;
            else
                s_remainingBytes -= byteCount;
            gBS->RestoreTPL(oldTpl);
        }

        // AudioDxe is fed a single hardware-safe format. This keeps stream
        // setup independent of the producer's source layout and sample rate.
        private static byte[] ConvertToOutputPcm(
            byte[] input,
            int offset,
            int count,
            int inputChannels,
            int inputSampleRate)
        {
            int inputFrames = count / (inputChannels * 2);
            if (inputFrames == 0)
                return null;

            long outputFramesLong = ((long)inputFrames * OutputSampleRate + inputSampleRate - 1) / inputSampleRate;
            if (outputFramesLong <= 0 || outputFramesLong > int.MaxValue / (OutputChannels * 2))
                return null;

            int outputFrames = (int)outputFramesLong;
            byte[] output = new byte[outputFrames * OutputChannels * 2];
            for (int outputFrame = 0; outputFrame < outputFrames; outputFrame++)
            {
                long sourcePosition = (long)outputFrame * inputSampleRate;
                int sourceFrame = (int)(sourcePosition / OutputSampleRate);
                int fraction = (int)(sourcePosition % OutputSampleRate);
                if (sourceFrame >= inputFrames)
                    sourceFrame = inputFrames - 1;

                int nextFrame = sourceFrame + 1 < inputFrames ? sourceFrame + 1 : sourceFrame;
                short left = InterpolateSample(input, offset, inputChannels, sourceFrame, nextFrame, 0, fraction);
                short right = inputChannels == 1
                    ? left
                    : InterpolateSample(input, offset, inputChannels, sourceFrame, nextFrame, 1, fraction);

                int outputOffset = outputFrame * 4;
                output[outputOffset] = (byte)left;
                output[outputOffset + 1] = (byte)(left >> 8);
                output[outputOffset + 2] = (byte)right;
                output[outputOffset + 3] = (byte)(right >> 8);
            }

            return output;
        }

        private static short InterpolateSample(
            byte[] input,
            int offset,
            int channels,
            int sourceFrame,
            int nextFrame,
            int channel,
            int fraction)
        {
            int sourceOffset = offset + (sourceFrame * channels + channel) * 2;
            int nextOffset = offset + (nextFrame * channels + channel) * 2;
            int source = (short)(input[sourceOffset] | (input[sourceOffset + 1] << 8));
            int next = (short)(input[nextOffset] | (input[nextOffset + 1] << 8));
            long difference = next - source;
            return (short)(source + (difference * fraction + OutputSampleRate / 2) / OutputSampleRate);
        }

        private static bool CompletePlayback()
        {
            if (!IsAvailable)
                return false;

            StopPlayback(true);
            return !s_failed;
        }

        private static void StopPlayback(bool wait)
        {
            if (s_audioIo == null || (void*)s_playbackEvent == null)
                return;

            bool checkEvent = true;
            _ = wait;

            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            if (s_streamingPlaybackActive)
            {
                // AudioIo StopPlayback deliberately does not invoke the callback.
                s_audioIo->StopPlayback(s_audioIo);
                s_streamingPlaybackActive = false;
            }

            // StopPlayback does not invoke queued transfer callbacks.
            s_remainingBytes = 0;

            if (checkEvent)
                gBS->CheckEvent(s_playbackEvent);
            gBS->RestoreTPL(oldTpl);
        }

        [UnmanagedCallersOnly]
        private static void StreamingPlaybackDone(EFI_AUDIO_IO_PROTOCOL* audioIo, void* context)
        {
            ulong completed = (ulong)context;
            if (completed >= s_remainingBytes)
                s_remainingBytes = 0;
            else
                s_remainingBytes -= completed;
        }

        private static ulong GetConnectedSpeakerOutputMask(
            EFI_AUDIO_IO_PROTOCOL* audioIo,
            uint bits,
            uint frequency,
            out uint outputFrequencies)
        {
            outputFrequencies = 0;
            EFI_AUDIO_IO_PROTOCOL_PORT* ports = null;
            ulong portCount = 0;
            EFI_STATUS status = audioIo->GetOutputs(audioIo, &ports, &portCount);
            if ((ulong)status != EFI_SUCCESS || ports == null || portCount == 0)
            {
                if (ports != null)
                    gBS->FreePool(ports);
                return 0;
            }

            ulong speakerMask = 0;
            uint commonFrequencies = uint.MaxValue;
            for (ulong index = 0; index < portCount && index < 64; index++)
            {
                EFI_AUDIO_IO_PROTOCOL_PORT port = ports[index];

                if (port.Type != AudioIoTypeOutput ||
                    (port.SupportedBits & bits) == 0 ||
                    (port.SupportedFreqs & frequency) == 0)
                    continue;

                // AudioDxe exposes pins without a physical connection as Other.
                // Keep only analog speaker endpoints and their external line-out.
                ulong bit = 1UL << (int)index;
                if (port.Device == AudioIoDeviceLine || port.Device == AudioIoDeviceSpeaker)
                {
                    speakerMask |= bit;
                    commonFrequencies &= port.SupportedFreqs;
                }
            }

            gBS->FreePool(ports);
            if (speakerMask != 0)
                outputFrequencies = commonFrequencies;
            return speakerMask;
        }

        private static uint GetAlternateFrequency(uint supportedFrequencies, uint frequency)
        {
            uint alternatives = supportedFrequencies & ~frequency;
            if ((alternatives & AudioIoFreq48Khz) != 0)
                return AudioIoFreq48Khz;
            if ((alternatives & AudioIoFreq44Khz) != 0)
                return AudioIoFreq44Khz;
            if ((alternatives & AudioIoFreq32Khz) != 0)
                return AudioIoFreq32Khz;
            if ((alternatives & AudioIoFreq96Khz) != 0)
                return AudioIoFreq96Khz;
            if ((alternatives & AudioIoFreq22Khz) != 0)
                return AudioIoFreq22Khz;
            if ((alternatives & AudioIoFreq16Khz) != 0)
                return AudioIoFreq16Khz;
            if ((alternatives & AudioIoFreq88Khz) != 0)
                return AudioIoFreq88Khz;
            if ((alternatives & AudioIoFreq192Khz) != 0)
                return AudioIoFreq192Khz;
            if ((alternatives & AudioIoFreq11Khz) != 0)
                return AudioIoFreq11Khz;
            if ((alternatives & AudioIoFreq8Khz) != 0)
                return AudioIoFreq8Khz;
            return 0;
        }

        private static bool TryGetFrequency(int sampleRate, out uint frequency)
        {
            frequency = sampleRate switch
            {
                8000 => AudioIoFreq8Khz,
                11025 => AudioIoFreq11Khz,
                16000 => AudioIoFreq16Khz,
                22050 => AudioIoFreq22Khz,
                32000 => AudioIoFreq32Khz,
                44100 => AudioIoFreq44Khz,
                48000 => AudioIoFreq48Khz,
                88200 => AudioIoFreq88Khz,
                96000 => AudioIoFreq96Khz,
                192000 => AudioIoFreq192Khz,
                _ => 0
            };
            return frequency != 0;
        }
    }
}
