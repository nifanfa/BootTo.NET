using System.Drawing;

namespace System.Windows.Forms
{
    public static unsafe class Control
    {
        private static EFI_SIMPLE_POINTER_PROTOCOL* s_pointer;
        private static int s_x;
        private static int s_y;
        private static int s_pendingX;
        private static int s_pendingY;
        private static int s_remainderX;
        private static int s_remainderY;
        private static MouseButtons s_buttons;
        private static bool s_stateChanged;
        private const int PointerFixedPointScale = 65535;

        public static Point MousePosition
        {
            get
            {
                PollMouse();
                return new Point(s_x, s_y);
            }
        }

        public static MouseButtons MouseButtons
        {
            get
            {
                PollMouse();
                return s_buttons;
            }
        }

        internal static bool TryReadMouseState(out int deltaX, out int deltaY, out MouseButtons buttons)
        {
            PollMouse();

            deltaX = s_pendingX;
            deltaY = s_pendingY;
            buttons = s_buttons;
            if (!s_stateChanged)
                return false;

            s_pendingX = 0;
            s_pendingY = 0;
            s_stateChanged = false;
            return true;
        }

        internal static void PumpMouseState()
        {
            PollMouse();
        }

        private static void PollMouse()
        {
            if (!TryGetPointer())
                return;

            EFI_SIMPLE_POINTER_STATE state;
            while ((ulong)s_pointer->GetState(s_pointer, &state) == EFI_SUCCESS)
            {
                int deltaX = NormalizePointerMovement(state.RelativeMovementX, ref s_remainderX);
                int deltaY = NormalizePointerMovement(state.RelativeMovementY, ref s_remainderY);
                MouseButtons buttons = GetButtons(state);
                if (deltaX != 0 || deltaY != 0 || buttons != s_buttons)
                {
                    s_x = AddClamped(s_x, deltaX);
                    s_y = AddClamped(s_y, deltaY);
                    s_pendingX = AddClamped(s_pendingX, deltaX);
                    s_pendingY = AddClamped(s_pendingY, deltaY);
                    s_buttons = buttons;
                    s_stateChanged = true;
                }
            }
        }

        private static bool TryGetPointer()
        {
            if (s_pointer != null)
                return true;
            if (gBS == null)
                return false;

            EFI_SIMPLE_POINTER_PROTOCOL* pointer = null;
            EFI_GUID protocol = EFI_SIMPLE_POINTER_PROTOCOL_GUID;
            EFI_STATUS status = gBS->LocateProtocol((EFI_GUID*)protocol, null, (void**)&pointer);
            if ((ulong)status != EFI_SUCCESS || pointer == null || pointer->GetState == null)
                return false;

            s_pointer = pointer;
            if (s_pointer->Reset != null)
                s_pointer->Reset(s_pointer, false);
            return true;
        }

        private static MouseButtons GetButtons(EFI_SIMPLE_POINTER_STATE state)
        {
            MouseButtons buttons = MouseButtons.None;
            if (state.LeftButton)
                buttons |= MouseButtons.Left;
            if (state.RightButton)
                buttons |= MouseButtons.Right;
            return buttons;
        }

        private static int NormalizePointerMovement(int movement, ref int remainder)
        {
            long fixedPointMovement = (long)remainder + movement;
            int normalized = (int)(fixedPointMovement / PointerFixedPointScale);
            remainder = (int)(fixedPointMovement % PointerFixedPointScale);
            return normalized;
        }

        private static int AddClamped(int value, int delta)
        {
            long sum = (long)value + delta;
            if (sum > int.MaxValue)
                return int.MaxValue;
            if (sum < int.MinValue)
                return int.MinValue;
            return (int)sum;
        }

    }
}
