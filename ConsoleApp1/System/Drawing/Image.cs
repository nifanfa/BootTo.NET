namespace System.Drawing
{
    public abstract class Image : IDisposable
    {
        internal Color[] pixels;

        public int Width { get; }
        public int Height { get; }

        protected Image(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Image dimensions must be positive.");

            ulong pixelCount = (ulong)(uint)width * (uint)height;
            if (pixelCount > int.MaxValue)
                throw new ArgumentException("Image dimensions are too large.");

            Width = width;
            Height = height;
            pixels = new Color[(int)pixelCount];
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            pixels = null;
        }

        protected int GetOffset(int x, int y)
        {
            if (pixels == null)
                throw new InvalidOperationException("The image has been disposed.");
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
                throw new ArgumentException("The pixel coordinates are outside the image bounds.");

            return y * Width + x;
        }
    }
}
