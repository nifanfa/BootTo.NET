using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static unsafe class re3generic
{
    private const int Width = 320;
    private const int Height = 240;
    private const int FrameRate = 60;
    private const int MaximumOpenFiles = 64;
    private const int AudioRate = 32000;
    private const int AudioChannels = 2;
    private const int AudioBatchFrames = 512;
    private const int AudioBufferFrames = 8192;
    private const int AudioMinimumSubmitFrames = 1024;
    private const int AudioSubmitFrames = 2048;
    private const ulong GameClockPeriodMilliseconds = 10;
    private const int AudioReadCacheBytes = 32768;

    [StructLayout(LayoutKind.Sequential)]
    public struct InputEvent
    {
        public int Type;
        public int Code;
        public int Value;
        public int ValueY;
    }

    [DllImport("*")]
    private static extern int RG_BootBind();

    [DllImport("*")]
    private static extern int BTDN_ChangeDirectory(byte* path);

    [DllImport("*")]
    private static extern int rg_game_init(uint width, uint height);

    [DllImport("*")]
    private static extern int rg_game_step();

    [DllImport("*")]
    private static extern void rg_game_shutdown();

    private static readonly FileStream[] s_files = new FileStream[MaximumOpenFiles];
    private static readonly byte[][] s_audioReadCache = new byte[MaximumOpenFiles][];
    private static readonly ulong[] s_audioCacheOffset = new ulong[MaximumOpenFiles];
    private static readonly int[] s_audioCacheLength = new int[MaximumOpenFiles];
    private static readonly SoundPlayer s_soundOutput = new SoundPlayer(AudioChannels, AudioRate);
    private static readonly byte[] s_audioBuffer = new byte[AudioBufferFrames * AudioChannels * sizeof(short)];
    private static int s_audioBufferedFrames;
    private static Bitmap s_screen;
    private static Graphics s_graphics;
    private static Stopwatch s_clock;
    private static EFI_EVENT s_gameClockEvent;
    private static ulong s_gameClockMilliseconds;
    private static readonly Color[] s_rgb565Colors = CreateRgb565Colors();
    private static int s_mouseX;
    private static int s_mouseY;
    private static int s_mouseButtons;
    private static int s_pendingMouseX;
    private static int s_pendingMouseY;
    private static int s_pendingMouseButton;
    private static int s_pendingMouseButtonType;
    private static bool s_pendingMouseMove;

    public static void Run()
    {
        if (!File.Exists(@"\Grand Theft Auto 3\models\gta3.img") ||
            !File.Exists(@"\Grand Theft Auto 3\data\gta3.dat"))
        {
            Console.WriteLine(@"GTA III files are missing in \Grand Theft Auto 3.");
            return;
        }

        int directoryStatus;
        fixed (byte* directory = "\\Grand Theft Auto 3"u8)
            directoryStatus = BTDN_ChangeDirectory(directory);
        if (directoryStatus != 0)
        {
            Console.WriteLine("GTA III directory could not be opened.");
            return;
        }

        s_screen = new Bitmap(Width, Height);
        s_graphics = CreateGraphics();
        s_clock = Stopwatch.StartNew();
        StartGameClock();
        s_mouseX = Width / 2;
        s_mouseY = Height / 2;
        s_mouseButtons = 0;
        s_audioBufferedFrames = 0;
        Program.PrintFrame();

        if (RG_BootBind() == 0 || rg_game_init(Width, Height) == 0)
        {
            Console.WriteLine("GTA III failed to initialize.");
            CloseFiles();
            StopGameClock();
            return;
        }

        int gameRunning;
        do
        {
            gameRunning = rg_game_step();
            FlushAudio();
        } while (gameRunning != 0);
        rg_game_shutdown();
        FlushAudio();
        CloseFiles();
        StopGameClock();
    }

    private static void StartGameClock()
    {
        s_gameClockMilliseconds = 0;
        EFI_EVENT timerEvent = null;
        EFI_STATUS status = gBS->CreateEvent(
            (uint)EVT_TIMER | EVT_NOTIFY_SIGNAL,
            TPL_CALLBACK,
            &GameClockTick,
            null,
            &timerEvent);
        if ((ulong)status != EFI_SUCCESS)
            return;

        status = gBS->SetTimer(timerEvent, TimerPeriodic, GameClockPeriodMilliseconds * 10000);
        if ((ulong)status != EFI_SUCCESS)
        {
            gBS->CloseEvent(timerEvent);
            return;
        }

        s_gameClockEvent = timerEvent;
    }

    private static void StopGameClock()
    {
        if ((void*)s_gameClockEvent == null)
            return;

        gBS->SetTimer(s_gameClockEvent, TimerCancel, 0);
        gBS->CloseEvent(s_gameClockEvent);
        s_gameClockEvent = null;
    }

    [UnmanagedCallersOnly]
    private static void GameClockTick(EFI_EVENT timerEvent, void* context)
        => s_gameClockMilliseconds += GameClockPeriodMilliseconds;

    [RuntimeExport("RG_BootTicks")]
    public static ulong Ticks(void* userdata)
    {
        if ((void*)s_gameClockEvent != null)
            return s_gameClockMilliseconds;

        long milliseconds = s_clock.ElapsedMilliseconds;
        return milliseconds > 0 ? (ulong)milliseconds : 0;
    }

    [RuntimeExport("RG_BootAudioSubmit")]
    public static void AudioSubmit(void* userdata, short* samples, uint frames, uint sampleRate)
    {
        if (samples == null || frames == 0 || frames > AudioBatchFrames || sampleRate != AudioRate)
            return;

        if (s_audioBufferedFrames + frames > AudioBufferFrames)
        {
            int retainedFrames = AudioBufferFrames - (int)frames;
            int discardedBytes = (s_audioBufferedFrames - retainedFrames) * AudioChannels * sizeof(short);
            int retainedBytes = retainedFrames * AudioChannels * sizeof(short);
            Array.Copy(s_audioBuffer, discardedBytes, s_audioBuffer, 0, retainedBytes);
            s_audioBufferedFrames = retainedFrames;
        }

        int sampleCount = (int)frames * AudioChannels;
        int offset = s_audioBufferedFrames * AudioChannels * sizeof(short);
        for (int index = 0; index < sampleCount; index++)
        {
            short sample = samples[index];
            s_audioBuffer[offset + index * 2] = (byte)sample;
            s_audioBuffer[offset + index * 2 + 1] = (byte)(sample >> 8);
        }

        s_audioBufferedFrames += (int)frames;
    }

    private static void FlushAudio()
    {
        if (s_audioBufferedFrames < AudioMinimumSubmitFrames)
            return;

        int submittedFrames = Math.Min(s_audioBufferedFrames, AudioSubmitFrames);
        int byteCount = submittedFrames * AudioChannels * sizeof(short);
        if (s_soundOutput.Play(s_audioBuffer, 0, byteCount, false) == byteCount)
        {
            s_audioBufferedFrames -= submittedFrames;
            if (s_audioBufferedFrames != 0)
                Array.Copy(s_audioBuffer, byteCount, s_audioBuffer, 0,
                    s_audioBufferedFrames * AudioChannels * sizeof(short));
        }
    }

    private static Color[] CreateRgb565Colors()
    {
        Color[] colors = new Color[65536];
        for (int pixel = 0; pixel < colors.Length; pixel++)
            colors[pixel] = Color.FromArgb(
                (byte)(((pixel >> 11) & 31) * 255 / 31),
                (byte)(((pixel >> 5) & 63) * 255 / 63),
                (byte)((pixel & 31) * 255 / 31));
        return colors;
    }

    [RuntimeExport("RG_BootPresent")]
    public static void Present(void* userdata, ushort* pixels, uint width, uint height, uint pitch, int format)
    {
        if (pixels == null || format != 1 || width != Width || height != Height || pitch != Width * 2 ||
            s_screen == null || s_graphics == null)
            return;

        for (int row = 0; row < Height; row++)
        {
            for (int column = 0; column < Width; column++)
            {
                int pixel = pixels[(Height - 1 - row) * Width + column];
                s_screen.pixels[row * Width + column] = s_rgb565Colors[pixel];
            }
        }

        Rectangle bounds = s_graphics.VisibleClipBounds;
        s_graphics.DrawImage(s_screen, (bounds.Width - Width) / 2, (bounds.Height - Height) / 2);
    }

    [RuntimeExport("RG_BootPollInput")]
    public static int PollInput(void* userdata, InputEvent* input)
    {
        if (input == null)
            return 0;

        if (Console.TryReadKeyEvent(out ConsoleKeyEvent keyEvent))
        {
            int code = MapKey(keyEvent);
            if (code != 0)
            {
                *input = new InputEvent { Type = keyEvent.IsKeyDown ? 1 : 2, Code = code };
                return 1;
            }
        }

        if (s_pendingMouseButtonType != 0)
        {
            *input = new InputEvent { Type = s_pendingMouseButtonType, Code = s_pendingMouseButton };
            s_pendingMouseButtonType = 0;
            return 1;
        }

        if (s_pendingMouseX != 0 || s_pendingMouseY != 0)
        {
            *input = new InputEvent { Type = 9, Value = s_pendingMouseX, ValueY = s_pendingMouseY };
            s_pendingMouseX = s_pendingMouseY = 0;
            return 1;
        }

        if (s_pendingMouseMove)
        {
            *input = new InputEvent { Type = 3, Value = s_mouseX, ValueY = s_mouseY };
            s_pendingMouseMove = false;
            return 1;
        }

        if (!Control.TryReadMouseState(out int deltaX, out int deltaY, out MouseButtons buttons))
            return 0;

        int buttonMask = 0;
        if ((buttons & MouseButtons.Left) != 0)
            buttonMask |= 1;
        if ((buttons & MouseButtons.Right) != 0)
            buttonMask |= 2;
        if ((buttons & MouseButtons.Middle) != 0)
            buttonMask |= 4;

        int changes = buttonMask ^ s_mouseButtons;
        if (changes != 0)
        {
            int bit = (changes & 1) != 0 ? 1 : (changes & 2) != 0 ? 2 : 4;
            s_pendingMouseButton = bit == 1 ? 0 : bit == 2 ? 1 : 2;
            s_pendingMouseButtonType = (buttonMask & bit) != 0 ? 4 : 5;
        }
        s_mouseButtons = buttonMask;
        s_pendingMouseX = deltaX;
        s_pendingMouseY = -deltaY;
        if (deltaX != 0 || deltaY != 0)
        {
            s_mouseX = Math.Clamp(s_mouseX + deltaX, 0, Width - 1);
            s_mouseY = Math.Clamp(s_mouseY + deltaY, 0, Height - 1);
            s_pendingMouseMove = true;
        }
        return PollInput(userdata, input);
    }

    [RuntimeExport("RG_BootFileOpen")]
    public static void* FileOpen(void* userdata, byte* path)
    {
        if (path == null)
            return null;

        int slot = 0;
        while (slot < MaximumOpenFiles && s_files[slot] != null)
            slot++;
        if (slot == MaximumOpenFiles)
            return null;

        int length = 0;
        while (path[length] != 0)
            length++;
        const string gameRoot = @"\Grand Theft Auto 3\";
        char[] characters = new char[gameRoot.Length + length];
        for (int position = 0; position < gameRoot.Length; position++)
            characters[position] = gameRoot[position];
        for (int position = 0; position < length; position++)
            characters[gameRoot.Length + position] = path[position] == '/' ? '\\' : (char)path[position];

        try
        {
            string filePath = new string(characters);
            byte[] cache = filePath.StartsWith(@"\Grand Theft Auto 3\AUDIO\")
                ? new byte[AudioReadCacheBytes]
                : null;
            s_files[slot] = File.OpenRead(filePath);
            s_audioReadCache[slot] = cache;
            s_audioCacheLength[slot] = 0;
            return (void*)(slot + 1);
        }
        catch
        {
            return null;
        }
    }

    [RuntimeExport("RG_BootFileSize")]
    public static ulong FileSize(void* userdata, void* file)
    {
        int slot = (int)(ulong)file - 1;
        return (uint)slot < MaximumOpenFiles && s_files[slot] != null ? (ulong)s_files[slot].Length : 0;
    }

    [RuntimeExport("RG_BootFileReadAt")]
    public static ulong FileReadAt(void* userdata, void* file, ulong offset, byte* buffer, ulong bytes)
    {
        int slot = (int)(ulong)file - 1;
        if ((uint)slot >= MaximumOpenFiles || s_files[slot] == null || buffer == null || offset > long.MaxValue)
            return 0;

        try
        {
            FileStream stream = s_files[slot];
            byte[] cache = s_audioReadCache[slot];
            if (cache != null && bytes <= AudioReadCacheBytes / 2)
            {
                ulong cacheOffset = s_audioCacheOffset[slot];
                int cacheLength = s_audioCacheLength[slot];
                if (offset < cacheOffset || offset - cacheOffset > (ulong)cacheLength ||
                    bytes > (ulong)cacheLength - (offset - cacheOffset))
                {
                    cacheOffset = offset & ~((ulong)(AudioReadCacheBytes / 2) - 1);
                    stream.Seek((long)cacheOffset, SeekOrigin.Begin);
                    fixed (byte* cached = cache)
                        cacheLength = stream.Read(cached, AudioReadCacheBytes);
                    s_audioCacheOffset[slot] = cacheOffset;
                    s_audioCacheLength[slot] = cacheLength;
                }

                ulong available = offset - cacheOffset;
                ulong copied = available < (ulong)cacheLength
                    ? Math.Min(bytes, (ulong)cacheLength - available)
                    : 0;
                fixed (byte* cached = cache)
                    System.Runtime.CompilerServices.Unsafe.CopyBlock(buffer, cached + available, (uint)copied);
                return copied;
            }

            stream.Seek((long)offset, SeekOrigin.Begin);
            ulong total = 0;
            while (total < bytes)
            {
                int count = (int)Math.Min(65536UL, bytes - total);
                int read = stream.Read(buffer + total, count);
                total += (ulong)read;
                if (read != count)
                    break;
            }
            return total;
        }
        catch
        {
            return 0;
        }
    }

    [RuntimeExport("RG_BootFileClose")]
    public static void FileClose(void* userdata, void* file)
    {
        int slot = (int)(ulong)file - 1;
        if ((uint)slot < MaximumOpenFiles && s_files[slot] != null)
        {
            s_files[slot].Dispose();
            s_files[slot] = null;
            s_audioReadCache[slot] = null;
            s_audioCacheLength[slot] = 0;
        }
    }

    private static void CloseFiles()
    {
        for (int slot = 0; slot < MaximumOpenFiles; slot++)
            FileClose(null, (void*)(slot + 1));
    }

    private static int MapKey(ConsoleKeyEvent keyEvent)
    {
        return keyEvent.Key switch
        {
            ConsoleKey.Escape => 1000,
            ConsoleKey.UpArrow => 1019,
            ConsoleKey.DownArrow => 1020,
            ConsoleKey.LeftArrow => 1021,
            ConsoleKey.RightArrow => 1022,
            ConsoleKey.Backspace => 1042,
            ConsoleKey.Tab => 1043,
            ConsoleKey.Enter => 1045,
            ConsoleKey.LeftShift => 1046,
            ConsoleKey.LeftControl => 1049,
            _ => keyEvent.KeyChar >= 'a' && keyEvent.KeyChar <= 'z'
                ? keyEvent.KeyChar - ('a' - 'A') : keyEvent.KeyChar
        };
    }
}
