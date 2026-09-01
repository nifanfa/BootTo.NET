using System;
using System.Drawing;
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
    private const int DoomMouseTurnMultiplier = 32;
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

        byte[] program = "doomgeneric"u8;
        byte[] iwadArgument = "-iwad"u8;
        byte[] wadName = "DOOM1.WAD"u8;

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

}
