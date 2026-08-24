using System.Drawing;

namespace Playground.NES
{
    public class GameRender
    {
        Emulator NES;

        Graphics graphics;
        public int screenWidth = 256, screenHeight = 240;

        public unsafe void WriteBitmap(byte[] byteToWrite, Color XColor)
        {
            lock (this)
            {
                int w = 0;
                int h = 0;

                int baseX = (int)((graphics.VisibleClipBounds.Width / 2) - (screenWidth / 2));
                int baseY = (int)((graphics.VisibleClipBounds.Height / 2) - (screenHeight / 2));

                for (int i = 0; i < byteToWrite.Length; i += 4)
                {
                    Color color = Color.FromArgb(byteToWrite[i + 3], byteToWrite[i + 2], byteToWrite[i + 1], byteToWrite[i + 0]);
                    graphics.DrawPoint(baseX + w, baseY + h, color.A != 0 ? color : XColor);
                    //
                    w++;
                    //256*240
                    if (w == screenWidth)
                    {
                        w = 0;
                        h++;
                    }
                }
            }
        }

        public GameRender(Emulator formObject)
        {
            NES = formObject;
            graphics = CreateGraphics();
        }
    }
}
