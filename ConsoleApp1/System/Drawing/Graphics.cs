#pragma warning disable
global using static System.Drawing.Graphics;
#pragma warning restore

namespace System.Drawing
{
    public unsafe sealed class Graphics : IDisposable
    {
        private EFI_GRAPHICS_OUTPUT_PROTOCOL* _graphics;
        private Color[] _cache;

        internal Graphics(EFI_GRAPHICS_OUTPUT_PROTOCOL* graphics)
        {
            if (graphics == null || graphics->Mode == null || graphics->Mode->Info == null || graphics->Blt == null)
                throw new ArgumentException("The graphics protocol is incomplete or unavailable.");
            _graphics = graphics;
            _cache = new Color[_graphics->Mode->Info->HorizontalResolution * _graphics->Mode->Info->VerticalResolution];
        }

        public RectangleF VisibleClipBounds => GetDisplayBounds();

        public void DrawPoint(int x, int y, Color color)
        {
            EnsureOpen();
            if (x < 0 || y < 0 ||
                (uint)x >= _graphics->Mode->Info->HorizontalResolution ||
                (uint)y >= _graphics->Mode->Info->VerticalResolution)
                return;

            if (_cache[_graphics->Mode->Info->HorizontalResolution * y + x] == color) return;
            _cache[_graphics->Mode->Info->HorizontalResolution * y + x] = color;

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

        public void Dispose()
        {
            _graphics = null;
        }

        private void EnsureOpen()
        {
            if (_graphics == null || _graphics->Mode == null || _graphics->Mode->Info == null)
                throw new InvalidOperationException("The graphics object has been disposed or is unavailable.");
        }

        private RectangleF GetDisplayBounds()
        {
            EnsureOpen();
            return new RectangleF(
                0,
                0,
                _graphics->Mode->Info->HorizontalResolution,
                _graphics->Mode->Info->VerticalResolution);
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
