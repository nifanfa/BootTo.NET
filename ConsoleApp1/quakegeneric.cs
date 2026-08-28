using System;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static unsafe class quakegeneric
{
    [DllImport("*")]
    private static extern void QG_Create(int argc, char** argv);

    [DllImport("*")]
    private static extern void QG_Tick(double duration);

    private const int QuakeWidth = 640;
    private const int QuakeHeight = 480;
    private const int QuakeFrameRate = 60;
    private const int QuakeMouseMultiplier = 32;
    private const int QuakeAudioRate = 11025;
    private const int QuakeAudioChannels = 2;
    private const int QuakeAudioSubmitFrames = 2048;
    private static readonly SoundPlayer SoundOutput = new SoundPlayer(QuakeAudioChannels, QuakeAudioRate);
    private static readonly byte[] AudioBuffer = new byte[
        QuakeAudioSubmitFrames * QuakeAudioChannels * sizeof(short)];

    private static Bitmap s_screen;
    private static Graphics s_graphics;
    private static Stopwatch s_clock;
    private static uint s_frameMillisecondRemainder;
    private static bool s_quitRequested;

    public static void Run()
    {
        s_screen = new Bitmap(QuakeWidth, QuakeHeight);
        s_graphics = CreateGraphics();
        s_clock = Stopwatch.StartNew();
        s_quitRequested = false;

        byte[] program = "quakegeneric\0"u8;
        fixed (byte* programPointer = program)
        {
            char** arguments = stackalloc char*[1];
            arguments[0] = (char*)programPointer;
            QG_Create(1, arguments);
        }

        Program.PrintFrame();
        while (!s_quitRequested)
        {
            Control.PumpMouseState();
            QG_Tick(1.0 / QuakeFrameRate);
            WaitForNextFrame();
        }
    }

    [RuntimeExport("QG_PresentFrame")]
    public static void PresentFrame(byte* pixels, byte* palette, int width, int height)
    {
        if (pixels == null || palette == null || width != QuakeWidth || height != QuakeHeight ||
            s_screen == null || s_graphics == null)
            return;

        Color* colors = stackalloc Color[256];
        for (int i = 0; i < 256; i++)
        {
            int paletteOffset = i * 3;
            colors[i] = Color.FromArgb(
                palette[paletteOffset],
                palette[paletteOffset + 1],
                palette[paletteOffset + 2]);
        }

        for (int i = 0; i < QuakeWidth * QuakeHeight; i++)
            s_screen.pixels[i] = colors[pixels[i]];

        Rectangle bounds = s_graphics.VisibleClipBounds;
        int x = (bounds.Width - QuakeWidth) / 2;
        int y = (bounds.Height - QuakeHeight) / 2;
        s_graphics.DrawImage(s_screen, x, y);
    }

    [RuntimeExport("QG_PollKey")]
    public static int PollKey(int* pressed, int* key)
    {
        if (pressed == null || key == null || !Console.TryReadKeyEvent(out ConsoleKeyEvent keyEvent))
            return 0;

        int value = keyEvent.KeyChar;
        if (value == 0)
            value = (int)keyEvent.Key;
        *pressed = keyEvent.IsKeyDown ? 1 : 0;
        *key = value;
        return 1;
    }

    [RuntimeExport("QG_PollMouse")]
    public static int PollMouse(int* buttons, int* deltaX, int* deltaY)
    {
        if (buttons == null || deltaX == null || deltaY == null ||
            !Control.TryReadMouseState(out int relativeX, out int relativeY, out MouseButtons state))
            return 0;

        int quakeButtons = 0;
        if ((state & MouseButtons.Left) != 0)
            quakeButtons |= 1;
        if ((state & MouseButtons.Right) != 0)
            quakeButtons |= 2;
        if ((state & MouseButtons.Middle) != 0)
            quakeButtons |= 4;

        *buttons = quakeButtons;
        *deltaX = ScaleMouseDelta(relativeX);
        *deltaY = ScaleMouseDelta(relativeY);
        return 1;
    }

    [RuntimeExport("QG_AudioWrite")]
    public static int AudioWrite(short* samples, int frameCount)
    {
        if (samples == null || frameCount <= 0 || frameCount > QuakeAudioSubmitFrames)
            return 0;

        int sampleCount = frameCount * QuakeAudioChannels;
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = samples[i];
            AudioBuffer[i * 2] = (byte)sample;
            AudioBuffer[i * 2 + 1] = (byte)(sample >> 8);
        }

        int byteCount = sampleCount * sizeof(short);
        return SoundOutput.Play(AudioBuffer, 0, byteCount) == byteCount
            ? frameCount
            : 0;
    }

    [RuntimeExport("BTDN_GetMilliseconds")]
    public static uint GetMilliseconds()
    {
        if (s_clock == null)
            return 0;
        long milliseconds = s_clock.ElapsedMilliseconds;
        return milliseconds <= 0 ? 0 :
            milliseconds >= uint.MaxValue ? uint.MaxValue : (uint)milliseconds;
    }

    [RuntimeExport("BTDN_RequestQuit")]
    public static void RequestQuit()
    {
        s_quitRequested = true;
    }

    [RuntimeExport("BTDN_MathSin")]
    public static double MathSin(double value) => Math.Sin(value);

    [RuntimeExport("BTDN_MathCos")]
    public static double MathCos(double value) => Math.Cos(value);

    [RuntimeExport("BTDN_MathTan")]
    public static double MathTan(double value) => Math.Tan(value);

    [RuntimeExport("BTDN_MathAtan")]
    public static double MathAtan(double value) => Math.Atan(value);

    [RuntimeExport("BTDN_MathAtan2")]
    public static double MathAtan2(double y, double x) => Math.Atan2(y, x);

    [RuntimeExport("BTDN_MathSqrt")]
    public static double MathSqrt(double value) => Math.Sqrt(value);

    [RuntimeExport("BTDN_MathPow")]
    public static double MathPow(double x, double y) => Math.Pow(x, y);

    [RuntimeExport("BTDN_MathFloor")]
    public static double MathFloor(double value) => Math.Floor(value);

    [RuntimeExport("BTDN_MathCeiling")]
    public static double MathCeiling(double value) => Math.Ceiling(value);

    private static int ScaleMouseDelta(int delta)
    {
        long scaled = (long)delta * QuakeMouseMultiplier;
        if (scaled > int.MaxValue)
            return int.MaxValue;
        if (scaled < int.MinValue)
            return int.MinValue;
        return (int)scaled;
    }

    private static void WaitForNextFrame()
    {
        s_frameMillisecondRemainder += 1000;
        uint milliseconds = s_frameMillisecondRemainder / QuakeFrameRate;
        s_frameMillisecondRemainder %= QuakeFrameRate;
        if (milliseconds != 0)
            gBS->Stall((ulong)milliseconds * 1000);
    }
}
