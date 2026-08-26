using System.Drawing;

namespace Playground.NES
{
    public class GameRender
    {
        Graphics graphics = CreateGraphics();
        Bitmap bitmap = new Bitmap(256, 240);

        public unsafe void WriteBitmap(byte[] byteToWrite, Color XColor)
        {
            lock (this)
            {
                int w = 0;
                int h = 0;

                int baseX = (int)((graphics.VisibleClipBounds.Width / 2) - (bitmap.Width / 2));
                int baseY = (int)((graphics.VisibleClipBounds.Height / 2) - (bitmap.Height / 2));

                for (int i = 0; i < byteToWrite.Length; i += 4)
                {
                    Color color = Color.FromArgb(byteToWrite[i + 3], byteToWrite[i + 2], byteToWrite[i + 1], byteToWrite[i + 0]);
                    bitmap.SetPixel(w, h, color.A != 0 ? color : XColor);
                    //
                    w++;
                    if (w == bitmap.Width)
                    {
                        w = 0;
                        h++;
                    }
                }

                graphics.DrawImage(bitmap, baseX, baseY);
            }
        }
    }
}
