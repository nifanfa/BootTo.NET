using System.Drawing;

namespace Playground.NES
{
    public class GameRender
    {
        // Setup background color to use with Alpha
        Color colorBG;

        public const int screenWidth = 256;
        public const int screenHeight = 240;
        public uint[] screen = new uint[screenWidth * screenHeight];
        public volatile bool screenUpdated = false;

        public void InitializeGame()
        {
            colorBG = Color.Blue;
        }

        public unsafe void WriteBitmap(byte[] byteToWrite, Color XColor)
        {
            lock (this)
            {
                for (int i = 0; i < screen.Length; i++) screen[i] = XColor.ToArgb();

                int w = 0;
                int h = 0;

                for (int i = 0; i < byteToWrite.Length; i += 4)
                {
                    Color color = Color.FromArgb(byteToWrite[i + 3], byteToWrite[i + 2], byteToWrite[i + 1], byteToWrite[i + 0]);
                    if (color.A != 0)
                    {
                        screen[screenWidth * h + w] = color.ToArgb();
                    }
                    //
                    w++;
                    //256*240
                    if (w == screenWidth)
                    {
                        w = 0;
                        h++;
                    }
                }

                screenUpdated = true;
            }
        }

        public GameRender()
        {
            InitializeGame();
        }
    }
}