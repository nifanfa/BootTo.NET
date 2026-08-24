using System.IO;

namespace System.Media
{
    public class SoundPlayer
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

                Audio.Initialize(format.SampleRate);
                if (!Audio.IsAvailable)
                    throw new InvalidOperationException("No UEFI audio output is available.");

                byte[] buffer = new byte[format.DataLength];
                if (ReadFully(input, buffer, buffer.Length) != buffer.Length ||
                    Audio.WritePcm(buffer, 0, buffer.Length, format.Channels, format.SampleRate) != buffer.Length)
                {
                    throw new InvalidOperationException("Audio playback did not accept the complete WAV data stream.");
                }

                if (!Audio.CompletePlayback())
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
    }
}
