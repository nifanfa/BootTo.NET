namespace System.Drawing
{
    public sealed class Bitmap : Image
    {
        public Bitmap(int width, int height) : base(width, height) { }

        public Color GetPixel(int x, int y) => pixels[GetOffset(x, y)];

        public void SetPixel(int x, int y, Color color) => pixels[GetOffset(x, y)] = color;
    }
}
