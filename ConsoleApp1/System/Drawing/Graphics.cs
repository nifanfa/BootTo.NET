#pragma warning disable
global using static System.Drawing.Graphics;
#pragma warning restore

namespace System.Drawing
{
    public unsafe sealed class Graphics : IDisposable
    {
        private EFI_GRAPHICS_OUTPUT_PROTOCOL* _graphics;

        internal Graphics(EFI_GRAPHICS_OUTPUT_PROTOCOL* graphics)
        {
            if (graphics == null || graphics->Mode == null || graphics->Mode->Info == null || graphics->Blt == null)
                throw new ArgumentException("The graphics protocol is incomplete or unavailable.");
            _graphics = graphics;
        }

        public Rectangle VisibleClipBounds => GetDisplayBounds();

        public void DrawPoint(int x, int y, Color color)
        {
            EnsureOpen();
            if (x < 0 || y < 0 ||
                x >= VisibleClipBounds.Width ||
                y >= VisibleClipBounds.Height)
                return;

            EFI_GRAPHICS_OUTPUT_BLT_PIXEL pixel = new EFI_GRAPHICS_OUTPUT_BLT_PIXEL
            {
                Blue = color.B,
                Green = color.G,
                Red = color.R,
                Reserved = 0
            };

            EFI_STATUS status = _graphics->Blt(
                _graphics,
                &pixel,
                EfiBltBufferToVideo,
                0,
                0,
                (ulong)x,
                (ulong)y,
                1,
                1,
                0);
            if ((ulong)status != EFI_SUCCESS)
                throw new InvalidOperationException("The graphics protocol could not draw the pixel.");
        }

        public void DrawImage(Image image, int x, int y)
        {
            EnsureOpen();
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (image.pixels == null)
                throw new InvalidOperationException("The image has been disposed.");

            Rectangle visibleClipBounds = VisibleClipBounds;
            int displayWidth = visibleClipBounds.Width;
            int displayHeight = visibleClipBounds.Height;
            if (x >= displayWidth || y >= displayHeight || x <= -image.Width || y <= -image.Height)
                return;

            int sourceX = x < 0 ? -x : 0;
            int sourceY = y < 0 ? -y : 0;
            int destinationX = x < 0 ? 0 : x;
            int destinationY = y < 0 ? 0 : y;
            int width = image.Width - sourceX;
            int height = image.Height - sourceY;

            if (width <= 0 || height <= 0)
                return;

            if (width > displayWidth - destinationX)
                width = displayWidth - destinationX;
            if (height > displayHeight - destinationY)
                height = displayHeight - destinationY;

            fixed (Color* buffer = image.pixels)
            {
                EFI_STATUS status = _graphics->Blt(
                    _graphics,
                    (EFI_GRAPHICS_OUTPUT_BLT_PIXEL*)buffer,
                    EfiBltBufferToVideo,
                    (ulong)sourceX,
                    (ulong)sourceY,
                    (ulong)destinationX,
                    (ulong)destinationY,
                    (ulong)width,
                    (ulong)height,
                    (ulong)image.Width * (ulong)sizeof(EFI_GRAPHICS_OUTPUT_BLT_PIXEL));
                if ((ulong)status != EFI_SUCCESS)
                    throw new InvalidOperationException("The graphics protocol could not draw the pixel.");
            }
        }

        public void Dispose()
        {
            _graphics = null;
        }

        private void EnsureOpen()
        {
            if (_graphics == null || _graphics->Mode == null || _graphics->Mode->Info == null)
                throw new InvalidOperationException("The graphics object has been disposed or is unavailable.");

        }

        private Rectangle GetDisplayBounds()
        {
            EnsureOpen();
            return new Rectangle(
                0,
                0,
                (int)_graphics->Mode->Info->HorizontalResolution,
                (int)_graphics->Mode->Info->VerticalResolution);
        }

        public static unsafe Graphics CreateGraphics()
        {
            EFI_GRAPHICS_OUTPUT_PROTOCOL* gop = null;
            EFI_STATUS GraphicsStatus = gBS->LocateProtocol(
                (EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID,
                null,
                (void**)&gop);
            if ((ulong)GraphicsStatus != EFI_SUCCESS ||
                gop == null ||
                gop->Mode == null ||
                gop->Mode->Info == null)
            {
                throw new NotSupportedException("The UEFI graphics output protocol is unavailable.");
            }
            return new Graphics(gop);
        }
    }
}
