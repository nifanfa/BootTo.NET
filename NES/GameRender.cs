using Internal.Runtime.CompilerServices;
using System;
using System.Drawing;

namespace NES
{
    public class GameRender
    {
        Emulator NES;
        Bitmap textureScreenBuffer = new Bitmap(256, 240);
        Graphics graphics = CreateGraphics();

        public void WriteBitmap(byte[] byteToWrite, Color XColor)
        {
            if (byteToWrite == null)
                throw new ArgumentNullException(nameof(byteToWrite));
            if ((byteToWrite.Length & 3) != 0)
                throw new ArgumentException("Bitmap data must contain complete 4-byte pixels.");

            int pixelCount = byteToWrite.Length / 4;
            if (pixelCount > textureScreenBuffer.Width * textureScreenBuffer.Height)
                throw new ArgumentException("Bitmap data is larger than the target bitmap.");

            unsafe
            {
                fixed (byte* src = byteToWrite)
                {
                    fixed (Color* dst = textureScreenBuffer.pixels)
                    {
                        Unsafe.CopyBlock(dst, src, (ulong)byteToWrite.Length);

                        // Color is a packed 32-bit ARGB value. Avoid the
                        // per-pixel SetPixel call and coordinate arithmetic.
                        uint* pixels = (uint*)dst;
                        uint replacement = XColor.ARGB;
                        for (int pixel = 0; pixel < pixelCount; pixel++)
                        {
                            if ((pixels[pixel] & 0xFF000000U) == 0)
                                pixels[pixel] = replacement;
                        }
                    }
                }
            }

            Rectangle bounds = graphics.VisibleClipBounds;
            int baseX = (bounds.Width / 2) - (textureScreenBuffer.Width / 2);
            int baseY = (bounds.Height / 2) - (textureScreenBuffer.Height / 2);
            graphics.DrawImage(textureScreenBuffer, baseX, baseY);
        }
    }
}
