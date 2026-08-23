using System;
using System.IO;

internal sealed unsafe class WavPlayer
{
    private const int PcmFormat = 1;
    private const int BitsPerSample = 16;
    private const int OutputSampleRate = 44100;
    // Match APU's normal PCM block size so this diagnostic follows the same
    // producer granularity as the emulator.
    private const int InputChunkBytes = 16384;

    private struct WavFormat
    {
        internal int Channels;
        internal int SampleRate;
        internal int BlockAlign;
        internal int DataLength;
    }

    internal static bool Play(string path)
    {
        FileStream stream = null;
        try
        {
            stream = new FileStream(path, FileMode.Open);
        }
        catch (Exception)
        {
            Console.WriteLine("WAV: cannot read file.");
            return false;
        }

        WavFormat format;
        if (!TryParse(stream, out format))
        {
            stream.Close();
            Console.WriteLine("WAV: unsupported PCM format.");
            return false;
        }

        WaveOutAudio audio = new WaveOutAudio(OutputSampleRate);

        try
        {
            byte[] buffer = new byte[InputChunkBytes];
            int remaining = format.DataLength;
            while (remaining > 0)
            {
                int request = remaining < buffer.Length ? remaining : buffer.Length;
                request -= request % format.BlockAlign;
                if (request <= 0)
                    break;

                int bytesRead = ReadFully(stream, buffer, request);
                bytesRead -= bytesRead % format.BlockAlign;
                if (bytesRead <= 0)
                    break;

                int consumed = audio.WritePcm(
                    buffer,
                    0,
                    bytesRead,
                    format.Channels,
                    format.SampleRate);
                if (consumed != bytesRead)
                    return false;
                remaining -= consumed;
            }

            if (remaining != 0)
                return false;

            return true;
        }
        finally
        {
            stream.Close();
        }
    }

    private static int ReadFully(FileStream stream, byte[] buffer, int count)
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

    private static bool TryParse(FileStream stream, out WavFormat format)
    {
        format = default;
        byte[] header = new byte[12];
        if (ReadFully(stream, header, header.Length) != header.Length ||
            !Tag(header, 0, (byte)'R', (byte)'I', (byte)'F', (byte)'F') ||
            !Tag(header, 8, (byte)'W', (byte)'A', (byte)'V', (byte)'E'))
            return false;

        bool haveFormat = false;
        bool haveData = false;
        byte[] chunkHeader = new byte[8];
        byte[] scratch = new byte[256];
        while (!haveData)
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
                haveData = true;
            }
            else if (!Skip(stream, size, scratch))
            {
                return false;
            }

            if (!haveData && (size & 1) != 0 && !Skip(stream, 1, scratch))
                return false;
        }

        return haveFormat && haveData;
    }

    private static bool Tag(byte[] data, int offset, byte a, byte b, byte c, byte d)
    {
        return offset >= 0 && offset + 4 <= data.Length &&
               data[offset] == a && data[offset + 1] == b &&
               data[offset + 2] == c && data[offset + 3] == d;
    }

    private static bool Skip(FileStream stream, int count, byte[] scratch)
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
    {
        return data[offset] | (data[offset + 1] << 8);
    }

    private static int ReadInt32(byte[] data, int offset)
    {
        return data[offset] |
               (data[offset + 1] << 8) |
               (data[offset + 2] << 16) |
               (data[offset + 3] << 24);
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        return (uint)(data[offset] |
               (data[offset + 1] << 8) |
               (data[offset + 2] << 16) |
               (data[offset + 3] << 24));
    }
}
