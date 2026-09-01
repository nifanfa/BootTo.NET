namespace System.Drawing
{
    public abstract class Image : IDisposable
    {
        internal Color[] pixels;

        public int Width { get; }
        public int Height { get; }

        protected Image(int width, int height)
        {
            int pixelCount = ValidateDimensions(width, height);
            Width = width;
            Height = height;
            pixels = new Color[pixelCount];
        }

        protected Image(int width, int height, Color[] pixels)
        {
            int pixelCount = ValidateDimensions(width, height);
            if (pixels == null || pixels.Length != pixelCount)
                throw new ArgumentException("The pixel buffer does not match the image dimensions.");

            Width = width;
            Height = height;
            this.pixels = pixels;
        }

        private static int ValidateDimensions(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Image dimensions must be positive.");

            ulong pixelCount = (ulong)(uint)width * (uint)height;
            if (pixelCount > int.MaxValue)
                throw new ArgumentException("Image dimensions are too large.");

            return (int)pixelCount;
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
