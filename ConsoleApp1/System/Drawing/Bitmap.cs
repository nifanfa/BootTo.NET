using System.IO;
using System.Runtime.InteropServices;

namespace System.Drawing
{
    public unsafe sealed class Bitmap : Image
    {
        public Bitmap(int width, int height) : base(width, height) { }

        public Bitmap(string filename) : this(DecodeImage(filename)) { }

        private Bitmap(DecodedImage image) : base(image.Width, image.Height, image.Pixels) { }

        public Color GetPixel(int x, int y) => pixels[GetOffset(x, y)];

        public void SetPixel(int x, int y, Color color) => pixels[GetOffset(x, y)] = color;

        [DllImport("*")]
        private static extern uint lodepng_decode32(
            out byte* output,
            out uint width,
            out uint height,
            byte* input,
            ulong inputSize);

        private static DecodedImage DecodeImage(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            if (Path.GetExtension(filename) == ".png")
                return DecodePng(File.ReadAllBytes(filename));

            throw new ArgumentException("The image format is not supported.");
        }

        private static DecodedImage DecodePng(byte[] pngData)
        {

            byte* output = null;
            uint width;
            uint height;
            uint error;
            fixed (byte* input = pngData)
                error = lodepng_decode32(out output, out width, out height, input, (ulong)pngData.Length);

            if (error != 0)
            {
                GarbageCollector.FreeNative(output);
                throw new ArgumentException("The PNG data could not be decoded. LodePNG error " + error + ".");
            }

            try
            {
                ulong pixelCount = (ulong)width * height;
                if (output == null || width == 0 || height == 0 || pixelCount > int.MaxValue)
                    throw new ArgumentException("The PNG dimensions are invalid.");

                Color[] decodedPixels = new Color[(int)pixelCount];
                byte* source = output;
                for (int i = 0; i < decodedPixels.Length; i++, source += 4)
                    decodedPixels[i] = Color.FromArgb(source[3], source[0], source[1], source[2]);

                return new DecodedImage((int)width, (int)height, decodedPixels);
            }
            finally
            {
                GarbageCollector.FreeNative(output);
            }
        }

        private readonly struct DecodedImage
        {
            public readonly int Width;
            public readonly int Height;
            public readonly Color[] Pixels;

            public DecodedImage(int width, int height, Color[] pixels)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
            }
        }
    }
}
