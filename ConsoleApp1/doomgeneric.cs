using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static unsafe class doomgeneric
{
    [DllImport("*")]
    public static unsafe extern void doomgeneric_Create(int argc, char** argv);

    [DllImport("*")]
    public static unsafe extern void doomgeneric_Tick();

    private const int DoomWidth = 640;
    private const int DoomHeight = 400;
    private const int DoomAudioRate = 11025;
    private const int DoomAudioFrames = 315;
    private const uint DoomTickRate = 35;
    private const ulong DoomAudioOutputBytesPerBlock = DoomAudioFrames * 4UL * 2UL * sizeof(short);
    private const ulong MaximumQueuedAudioBytes = DoomAudioOutputBytesPerBlock * 3;
    private const int DoomMouseTurnMultiplier = 32;
    private const int MaximumOpenFiles = 32;

    private static readonly FileStream[] OpenFiles = new FileStream[MaximumOpenFiles];
    private static readonly SoundPlayer SoundOutput = new SoundPlayer(2, DoomAudioRate);
    private static readonly byte[] AudioBuffer = new byte[DoomAudioFrames * 2 * sizeof(short)];
    private static Bitmap s_screen;
    private static Graphics s_graphics;
    private static uint s_ticks;
    private static uint s_tickMillisecondRemainder;

    public static void Run()
    {
        s_screen = new Bitmap(DoomWidth, DoomHeight);
        s_graphics = CreateGraphics();

        byte[] program = "doomgeneric\0"u8;
        byte[] iwadArgument = "-iwad\0"u8;
        byte[] wadName = "DOOM1.WAD\0"u8;

        fixed (byte* programPointer = program)
        fixed (byte* iwadPointer = iwadArgument)
        fixed (byte* wadPointer = wadName)
        {
            char** arguments = stackalloc char*[3];
            arguments[0] = (char*)programPointer;
            arguments[1] = (char*)iwadPointer;
            arguments[2] = (char*)wadPointer;
            doomgeneric_Create(3, arguments);
        }

        Program.PrintFrame();
        while (true)
        {
            Control.PumpMouseState();
            doomgeneric_Tick();
            WaitForNextTick();
        }
    }

    [RuntimeExport("malloc")]
    public static nint Malloc(ulong size) => (nint)GarbageCollector.AllocateNative(size == 0 ? 1UL : size);

    [RuntimeExport("free")]
    public static void Free(nint pointer) => GarbageCollector.FreeNative((void*)pointer);

    [RuntimeExport("DG_PresentFrame")]
    public static void PresentFrame(uint* pixels, int width, int height)
    {
        if (pixels == null || width != DoomWidth || height != DoomHeight || s_screen == null)
            return;

        for (int i = 0; i < DoomWidth * DoomHeight; i++)
            s_screen.pixels[i] = Color.FromArgb(pixels[i]);

        Rectangle bounds = s_graphics.VisibleClipBounds;
        int x = (bounds.Width - DoomWidth) / 2;
        int y = (bounds.Height - DoomHeight) / 2;
        s_graphics.DrawImage(s_screen, x, y);
    }

    [RuntimeExport("DG_Sleep")]
    public static void Sleep(uint milliseconds)
    {
        if (milliseconds != 0)
        {
            gBS->Stall((ulong)milliseconds * 1000);
            s_ticks = unchecked(s_ticks + milliseconds);
        }
    }

    [RuntimeExport("DG_GetTicks")]
    public static uint GetTicks() => s_ticks;

    private static void WaitForNextTick()
    {
        s_tickMillisecondRemainder += 1000;
        uint milliseconds = s_tickMillisecondRemainder / DoomTickRate;
        s_tickMillisecondRemainder %= DoomTickRate;
        Sleep(milliseconds);
    }

    [RuntimeExport("DG_AudioWrite")]
    public static int AudioWrite(short* samples, int frameCount)
    {
        if (samples == null || frameCount <= 0 || frameCount > DoomAudioFrames)
            return 0;

        if (SoundOutput.RemainingBytes > MaximumQueuedAudioBytes - DoomAudioOutputBytesPerBlock)
            return frameCount;

        for (int i = 0; i < frameCount * 2; i++)
        {
            short sample = samples[i];
            AudioBuffer[i * 2] = (byte)sample;
            AudioBuffer[i * 2 + 1] = (byte)(sample >> 8);
        }

        int byteCount = frameCount * 2 * sizeof(short);
        return SoundOutput.Play(AudioBuffer, 0, byteCount) == byteCount ? frameCount : 0;
    }

    [RuntimeExport("DG_PollMouse")]
    public static int PollMouse(int* buttons, int* deltaX, int* deltaY)
    {
        if (buttons == null || deltaX == null || deltaY == null ||
            !Control.TryReadMouseState(out int relativeX, out _, out MouseButtons state))
            return 0;

        int doomButtons = 0;
        if ((state & MouseButtons.Left) != 0)
            doomButtons |= 1;
        *buttons = doomButtons;
        *deltaX = ScaleMouseDelta(relativeX);
        *deltaY = 0;
        return 1;
    }

    private static int ScaleMouseDelta(int delta)
    {
        long scaledValue = (long)delta * DoomMouseTurnMultiplier;
        int scaled = scaledValue > int.MaxValue ? int.MaxValue
            : scaledValue < int.MinValue ? int.MinValue
            : (int)scaledValue;
        return scaled;
    }

    [RuntimeExport("DG_PollKey")]
    public static int PollKey(int* pressed, byte* key)
    {
        if (pressed == null || key == null || !Console.TryReadKeyEvent(out ConsoleKeyEvent keyEvent))
            return 0;

        *pressed = keyEvent.IsKeyDown ? 1 : 0;
        *key = (byte)keyEvent.Key;
        return 1;
    }

    [RuntimeExport("DG_FileOpen")]
    public static int FileOpen(byte* path, byte* mode)
    {
        string filePath = NormalizeFilePath(DecodeAscii(path));
        string openMode = DecodeAscii(mode);
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(openMode))
            return 0;

        int slot = FindFreeFileSlot();
        if (slot < 0)
            return 0;

        try
        {
            bool write = openMode[0] == 'w' || openMode[0] == 'a';
            bool readWrite = false;
            for (int i = 0; i < openMode.Length; i++)
                readWrite |= openMode[i] == '+';

            FileMode fileMode = openMode[0] == 'r'
                ? FileMode.Open
                : openMode[0] == 'a' ? FileMode.Append : FileMode.Create;
            FileAccess access = readWrite
                ? FileAccess.ReadWrite
                : write ? FileAccess.Write : FileAccess.Read;
            if (write)
                EnsureParentDirectory(filePath);
            OpenFiles[slot] = new FileStream(filePath, fileMode, access);
            return slot + 1;
        }
        catch
        {
            return 0;
        }
    }

    [RuntimeExport("DG_FileRead")]
    public static int FileRead(int handle, byte* destination, int length)
    {
        FileStream stream = GetFile(handle);
        if (stream == null || destination == null || length < 0)
            return -1;

        try
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;
            while (totalRead < length)
            {
                int read = stream.Read(buffer, totalRead, length - totalRead);
                if (read <= 0)
                    break;
                totalRead += read;
            }
            for (int i = 0; i < totalRead; i++)
                destination[i] = buffer[i];
            return totalRead;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileWrite")]
    public static int FileWrite(int handle, byte* source, int length)
    {
        FileStream stream = GetFile(handle);
        if (stream == null || source == null || length < 0)
            return -1;

        try
        {
            byte[] buffer = new byte[length];
            for (int i = 0; i < length; i++)
                buffer[i] = source[i];

            int totalWritten = 0;
            while (totalWritten < length)
            {
                int written = stream.Write(buffer, totalWritten, length - totalWritten);
                if (written <= 0)
                    break;
                totalWritten += written;
            }
            return totalWritten;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileSeek")]
    public static long FileSeek(int handle, long offset, int origin)
    {
        FileStream stream = GetFile(handle);
        if (stream == null)
            return -1;

        try
        {
            return stream.Seek(offset, (SeekOrigin)origin);
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileTell")]
    public static long FileTell(int handle)
    {
        FileStream stream = GetFile(handle);
        if (stream == null)
            return -1;

        try
        {
            return stream.Position;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileClose")]
    public static int FileClose(int handle)
    {
        int index = handle - 1;
        if ((uint)index >= MaximumOpenFiles || OpenFiles[index] == null)
            return -1;

        try
        {
            OpenFiles[index].Flush();
            OpenFiles[index].Close();
            OpenFiles[index] = null;
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileFlush")]
    public static int FileFlush(int handle)
    {
        FileStream stream = GetFile(handle);
        if (stream == null)
            return -1;

        try
        {
            stream.Flush();
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileCreateDirectory")]
    public static int FileCreateDirectory(byte* path)
    {
        try
        {
            string directory = NormalizeFilePath(DecodeAscii(path));
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileRemove")]
    public static int FileRemove(byte* path)
    {
        try
        {
            File.Delete(NormalizeFilePath(DecodeAscii(path)));
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("DG_FileRename")]
    public static int FileRename(byte* oldPath, byte* newPath)
    {
        try
        {
            string sourcePath = NormalizeFilePath(DecodeAscii(oldPath));
            string destinationPath = NormalizeFilePath(DecodeAscii(newPath));
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
                return -1;

            EnsureParentDirectory(destinationPath);
            File.Move(sourcePath, destinationPath);
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    private static FileStream GetFile(int handle)
    {
        int index = handle - 1;
        return (uint)index < MaximumOpenFiles ? OpenFiles[index] : null;
    }

    private static int FindFreeFileSlot()
    {
        for (int i = 0; i < MaximumOpenFiles; i++)
        {
            if (OpenFiles[i] == null)
                return i;
        }
        return -1;
    }

    private static string DecodeAscii(byte* value)
    {
        if (value == null)
            return null;

        int length = 0;
        while (value[length] != 0)
            length++;

        char[] characters = new char[length];
        for (int i = 0; i < length; i++)
            characters[i] = (char)value[i];
        return new string(characters);
    }

    private static string NormalizeFilePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        int start = 0;
        while (start + 1 < path.Length && path[start] == '.' &&
               (path[start + 1] == '\\' || path[start + 1] == '/'))
            start += 2;

        int end = path.Length;
        while (end > start && (path[end - 1] == '\\' || path[end - 1] == '/'))
            end--;
        if (end - start == 1 && path[start] == '.')
            return string.Empty;

        char[] normalized = new char[end - start];
        for (int i = 0; i < normalized.Length; i++)
        {
            char value = path[start + i];
            normalized[i] = value == '/' ? '\\' : value;
        }
        return new string(normalized);
    }

    private static void EnsureParentDirectory(string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }
}
