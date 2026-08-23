using System.Drawing;

namespace Playground.NES
{
    public class GameRender
    {
        Emulator NES;

        public uint[] displayBuffer;
        public bool screenUpdated = false;
        public int screenWidth = 256, screenHeight = 240;

        public void InitializeGame()
        {
            displayBuffer = new uint[screenWidth * screenHeight];
        }

        public unsafe void WriteBitmap(byte[] byteToWrite, Color XColor)
        {
            lock (this)
            {
                for (int i = 0; i < displayBuffer.Length; i++) displayBuffer[i] = XColor.ToArgb();

                int w = 0;
                int h = 0;

                for (int i = 0; i < byteToWrite.Length; i += 4)
                {
                    Color color = Color.FromArgb(byteToWrite[i + 3], byteToWrite[i + 2], byteToWrite[i + 1], byteToWrite[i + 0]);
                    if (color.A != 0)
                    {
                        displayBuffer[screenWidth * h + w] = color.ToArgb();
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

        public GameRender(Emulator formObject)
        {
            NES = formObject;
            InitializeGame();
        }
    }
}
