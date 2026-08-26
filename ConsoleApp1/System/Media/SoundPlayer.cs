using System.IO;
using System.Runtime.InteropServices;

namespace System.Media
{
    public unsafe class SoundPlayer
    {
        private const int PcmFormat = 1;
        private const int BitsPerSample = 16;

        private string _soundLocation = string.Empty;
        private Stream _stream;

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

                Initialize(format.SampleRate);
                if (!IsAvailable)
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
        private static void* s_currentBuffer;
        private static ulong s_outputIndexMask;
        private static uint s_outputFrequency;
        private static uint s_outputFrequencies;
        private static bool s_hasPlayed;
        private const ulong PlaybackDelay = 0;
        private static sbyte s_gain = DefaultGain;
        private static bool s_failed;

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

            if (s_audioIo != audioIo)
                s_hasPlayed = false;
            s_audioIo = audioIo;
            s_outputIndexMask = outputIndexMask;
            s_outputFrequency = initialFrequency;
            s_outputFrequencies = outputFrequencies;
            s_failed = false;
            Console.WriteLine("Audio output mask: " + s_outputIndexMask.ToString());
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
            if (!IsAvailable || buffer == null || offset < 0 || count <= 0 ||
                offset > buffer.Length - count || channels <= 0 || channels > 16 ||
                !TryGetFrequency(sampleRate, out uint frequency))
                return 0;

            int frameBytes = channels * 2;
            if (count % frameBytes != 0)
                return 0;

            uint outputFrequencies;
            ulong outputMask;
            if (frequency == s_outputFrequency)
            {
                outputMask = s_outputIndexMask;
                outputFrequencies = s_outputFrequencies;
            }
            else
            {
                outputMask = GetConnectedSpeakerOutputMask(
                    s_audioIo,
                    AudioIoBits16,
                    frequency,
                    out outputFrequencies);
            }
            if (outputMask == 0)
                return 0;

            void* rawBuffer = GarbageCollector.AllocateNative((ulong)count);
            if (rawBuffer == null)
                return 0;

            fixed (byte* source = buffer)
                memcpy(rawBuffer, source + offset, (ulong)count);

            EFI_STATUS status = PlayBuffer(
                rawBuffer,
                (ulong)count,
                outputMask,
                outputFrequencies,
                frequency,
                AudioIoBits16,
                (byte)channels);
            return (ulong)status == EFI_SUCCESS ? count : 0;
        }

        private static bool CompletePlayback()
        {
            if (!IsAvailable)
                return false;

            StopPlayback(true);
            return !s_failed;
        }

        private static EFI_STATUS PlayBuffer(
            void* rawBuffer,
            ulong rawBufferSize,
            ulong outputIndexMask,
            uint outputFrequencies,
            uint frequency,
            uint bits,
            byte channels)
        {
            if (s_audioIo == null || rawBuffer == null || rawBufferSize == 0)
            {
                if (rawBuffer != null)
                    gBS->FreePool(rawBuffer);
                return (EFI_STATUS)EFI_ABORTED;
            }

            // OcAudio owns only one provider buffer. A new play request first
            // finishes the previous request, then replaces CurrentBuffer.
            StopPlayback(true);

            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            if (s_hasPlayed)
            {
                // AudioDxe keeps the HDA DMA cursor when a stream is stopped and
                // only resets it when the stream format changes. Toggle through
                // another supported rate so each independent clip starts at zero.
                uint resetFrequency = GetAlternateFrequency(outputFrequencies, frequency);
                if (resetFrequency != 0)
                {
                    s_audioIo->SetupPlayback(
                        s_audioIo,
                        outputIndexMask,
                        s_gain,
                        resetFrequency,
                        bits,
                        channels,
                        PlaybackDelay);
                }
            }

            EFI_STATUS status = s_audioIo->SetupPlayback(
                s_audioIo,
                outputIndexMask,
                s_gain,
                frequency,
                bits,
                channels,
                PlaybackDelay);
            if ((ulong)status == EFI_SUCCESS)
            {
                s_currentBuffer = rawBuffer;
                status = s_audioIo->StartPlaybackAsync(
                    s_audioIo,
                    rawBuffer,
                    rawBufferSize,
                    0,
                    &PlaybackDone,
                    null);
                if ((ulong)status != EFI_SUCCESS)
                {
                    s_currentBuffer = null;
                    gBS->FreePool(rawBuffer);
                }
                else
                {
                    s_hasPlayed = true;
                }
            }
            else
            {
                gBS->FreePool(rawBuffer);
            }
            gBS->RestoreTPL(oldTpl);

            if ((ulong)status != EFI_SUCCESS)
                s_failed = true;
            return status;
        }

        private static void StopPlayback(bool wait)
        {
            if (s_audioIo == null || (void*)s_playbackEvent == null)
                return;

            bool checkEvent = true;
            if (wait)
            {
                if (s_currentBuffer != null && WaitForPlaybackCompletion())
                    checkEvent = false;
            }

            EFI_TPL oldTpl = gBS->RaiseTPL(TPL_NOTIFY);
            if (s_currentBuffer != null)
            {
                // AudioIo StopPlayback deliberately does not invoke the callback.
                s_audioIo->StopPlayback(s_audioIo);
                gBS->FreePool(s_currentBuffer);
                s_currentBuffer = null;
            }

            if (checkEvent)
                gBS->CheckEvent(s_playbackEvent);
            gBS->RestoreTPL(oldTpl);
        }

        private static bool WaitForPlaybackCompletion()
        {
            ulong index = 0;
            EFI_EVENT playbackEvent = s_playbackEvent;
            EFI_STATUS status = gBS->WaitForEvent(1, &playbackEvent, &index);
            return (ulong)status == EFI_SUCCESS;
        }

        [UnmanagedCallersOnly]
        private static void PlaybackDone(EFI_AUDIO_IO_PROTOCOL* audioIo, void* context)
        {
            if (s_currentBuffer != null)
            {
                gBS->FreePool(s_currentBuffer);
                s_currentBuffer = null;
            }
            gBS->SignalEvent(s_playbackEvent);
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
                Console.WriteLine(
                    "Audio output " + index.ToString() +
                    ": device=" + port.Device.ToString() +
                    ", location=" + port.Location.ToString() +
                    ", surface=" + port.Surface.ToString());

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
