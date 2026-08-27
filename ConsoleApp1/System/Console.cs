using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace System
{
    public static unsafe class Console
    {
        private sealed class ReadKeyOperation : TaskPoller
        {
            private Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> _completion;

            internal Threading.Tasks.Task<ConsoleKeyInfo> Start()
            {
                Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion =
                    new Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo>();
                _completion = completion;
                TaskScheduler.Register(this);
                Poll();
                return completion.Task;
            }

            internal override void Poll()
            {
                if ((void*)gST->ConIn == null)
                {
                    CompleteException();
                    return;
                }

                if ((ulong)gBS->CheckEvent(gST->ConIn->WaitForKey) != EFI_SUCCESS)
                    return;

                EFI_INPUT_KEY key;
                EFI_STATUS status = gST->ConIn->ReadKeyStroke(gST->ConIn, &key);

                if ((ulong)status == EFI_NOT_READY)
                    return;
                if ((ulong)status != EFI_SUCCESS)
                {
                    CompleteException();
                    return;
                }

                Complete(CreateKeyInfo(key));
            }

            private void Complete(ConsoleKeyInfo key)
            {
                Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion = _completion;
                _completion = null;
                TaskScheduler.Unregister(this);
                if (completion != null)
                    completion.TrySetResult(key);
            }

            private void CompleteException()
            {
                Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion = _completion;
                _completion = null;
                TaskScheduler.Unregister(this);
                if (completion != null)
                    completion.TrySetException(new Exception("The console input protocol is unavailable."));
            }
        }

        private sealed class ReadKeyEventOperation : TaskPoller
        {
            private Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent> _completion;

            internal Threading.Tasks.Task<ConsoleKeyEvent> Start()
            {
                Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent> completion =
                    new Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent>();
                _completion = completion;
                TaskScheduler.Register(this);
                Poll();
                return completion.Task;
            }

            internal override void Poll()
            {
                if (!TryDequeueUsbKey(out ConsoleKeyEvent keyEvent))
                    return;

                Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent> completion = _completion;
                _completion = null;
                TaskScheduler.Unregister(this);
                if (completion != null)
                    completion.TrySetResult(keyEvent);
            }
        }
        private static readonly object s_syncRoot = new object();
        static Console()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.Gray;
            gST->ConOut->EnableCursor(gST->ConOut, true);
            Clear();
        }

        static ulong EfiBackgroundColor, EfiForegroundColor;
        private static bool s_lineWrapped;

        public static int BufferWidth
        {
            get
            {
                lock (s_syncRoot)
                {
                    int width;
                    int height;
                    QueryBufferDimensions(out width, out height);
                    return width;
                }
            }
            set => SetBufferDimensions(value, false);
        }

        public static int BufferHeight
        {
            get
            {
                lock (s_syncRoot)
                {
                    int width;
                    int height;
                    QueryBufferDimensions(out width, out height);
                    return height;
                }
            }
            set => SetBufferDimensions(value, true);
        }

        // UEFI simple text output has no separate window and screen buffer.
        public static int WindowWidth
        {
            get => BufferWidth;
            set => BufferWidth = value;
        }

        public static int WindowHeight
        {
            get => BufferHeight;
            set => BufferHeight = value;
        }

        private static void QueryBufferDimensions(out int width, out int height)
        {
            SIMPLE_TEXT_OUTPUT_INTERFACE* consoleOut = GetConsoleOutput();
            ulong columns = 0;
            ulong rows = 0;
            EFI_STATUS status = consoleOut->QueryMode(
                consoleOut,
                (ulong)consoleOut->Mode->Mode,
                &columns,
                &rows);

            if ((ulong)status != EFI_SUCCESS)
                throw new IO.IOException("The console output mode could not be queried.");
            if (columns > (ulong)int.MaxValue || rows > (ulong)int.MaxValue)
                throw new OverflowException("The console dimensions exceed the supported integer range.");

            width = (int)columns;
            height = (int)rows;
        }

        private static void SetBufferDimensions(int value, bool height)
        {
            if (value <= 0)
                throw new ArgumentException("The console dimensions must be positive.");

            lock (s_syncRoot)
            {
                SIMPLE_TEXT_OUTPUT_INTERFACE* consoleOut = GetConsoleOutput();
                int currentWidth;
                int currentHeight;
                QueryBufferDimensions(out currentWidth, out currentHeight);

                int requestedWidth = height ? currentWidth : value;
                int requestedHeight = height ? value : currentHeight;
                if (requestedWidth == currentWidth && requestedHeight == currentHeight)
                    return;

                int maxMode = consoleOut->Mode->MaxMode;
                for (int mode = 0; mode < maxMode; mode++)
                {
                    ulong modeWidth = 0;
                    ulong modeHeight = 0;
                    EFI_STATUS status = consoleOut->QueryMode(
                        consoleOut,
                        (ulong)mode,
                        &modeWidth,
                        &modeHeight);
                    if ((ulong)status != EFI_SUCCESS ||
                        modeWidth != (ulong)requestedWidth ||
                        modeHeight != (ulong)requestedHeight)
                        continue;

                    status = consoleOut->SetMode(consoleOut, (ulong)mode);
                    if ((ulong)status != EFI_SUCCESS)
                        throw new IO.IOException("The console output mode could not be selected.");
                    s_lineWrapped = false;
                    return;
                }
            }

            throw new ArgumentException("The requested console dimensions are not supported.");
        }

        private static SIMPLE_TEXT_OUTPUT_INTERFACE* GetConsoleOutput()
        {
            if ((void*)gST == null || (void*)gST->ConOut == null ||
                (void*)gST->ConOut->Mode == null)
                throw new InvalidOperationException("The UEFI console output protocol is unavailable.");

            return gST->ConOut;
        }

        public static ConsoleColor BackgroundColor
        {
            set
            {
                lock (s_syncRoot)
                {
                    EfiBackgroundColor = value switch
                    {
                        ConsoleColor.Black => EFI_BACKGROUND_BLACK,
                        ConsoleColor.DarkBlue => EFI_BACKGROUND_BLUE,
                        ConsoleColor.DarkGreen => EFI_BACKGROUND_GREEN,
                        ConsoleColor.DarkCyan => EFI_BACKGROUND_CYAN,
                        ConsoleColor.DarkRed => EFI_BACKGROUND_RED,
                        ConsoleColor.DarkMagenta => EFI_BACKGROUND_MAGENTA,
                        ConsoleColor.DarkYellow => EFI_BACKGROUND_BROWN,
                        ConsoleColor.Gray => EFI_BACKGROUND_LIGHTGRAY,
                        ConsoleColor.DarkGray => EFI_BACKGROUND_BLACK,
                        ConsoleColor.Blue => EFI_BACKGROUND_BLUE,
                        ConsoleColor.Green => EFI_BACKGROUND_GREEN,
                        ConsoleColor.Cyan => EFI_BACKGROUND_CYAN,
                        ConsoleColor.Red => EFI_BACKGROUND_RED,
                        ConsoleColor.Magenta => EFI_BACKGROUND_MAGENTA,
                        ConsoleColor.Yellow => EFI_BACKGROUND_BROWN,
                        ConsoleColor.White => EFI_BACKGROUND_LIGHTGRAY,
                        _ => EFI_BACKGROUND_BLACK
                    };
                    gST->ConOut->SetAttribute(gST->ConOut, EfiBackgroundColor | EfiForegroundColor);
                }
            }
        }

        public static ConsoleColor ForegroundColor
        {
            set
            {
                lock (s_syncRoot)
                {
                    EfiForegroundColor = value switch
                    {
                        ConsoleColor.Black => EFI_BLACK,
                        ConsoleColor.DarkBlue => EFI_BLUE,
                        ConsoleColor.DarkGreen => EFI_GREEN,
                        ConsoleColor.DarkCyan => EFI_CYAN,
                        ConsoleColor.DarkRed => EFI_RED,
                        ConsoleColor.DarkMagenta => EFI_MAGENTA,
                        ConsoleColor.DarkYellow => EFI_BROWN,
                        ConsoleColor.Gray => EFI_LIGHTGRAY,
                        ConsoleColor.DarkGray => EFI_DARKGRAY,
                        ConsoleColor.Blue => EFI_LIGHTBLUE,
                        ConsoleColor.Green => EFI_LIGHTGREEN,
                        ConsoleColor.Cyan => EFI_LIGHTCYAN,
                        ConsoleColor.Red => EFI_LIGHTRED,
                        ConsoleColor.Magenta => EFI_LIGHTMAGENTA,
                        ConsoleColor.Yellow => EFI_YELLOW,
                        ConsoleColor.White => EFI_WHITE,
                        _ => EFI_LIGHTGRAY
                    };
                    gST->ConOut->SetAttribute(gST->ConOut, EfiBackgroundColor | EfiForegroundColor);
                }
            }
        }

        public static void Clear()
        {
            lock (s_syncRoot)
            {
                gST->ConOut->ClearScreen(gST->ConOut);
                s_lineWrapped = false;
            }
        }

        public static void SetCursorPosition(int left, int top)
        {
            if (left < 0 || top < 0)
                throw new ArgumentException("The cursor position cannot be negative.");

            lock (s_syncRoot)
            {
                SIMPLE_TEXT_OUTPUT_INTERFACE* consoleOut = GetConsoleOutput();
                int width;
                int height;
                QueryBufferDimensions(out width, out height);
                if (left >= width || top >= height)
                    throw new ArgumentException("The cursor position is outside the console buffer.");

                EFI_STATUS status = consoleOut->SetCursorPosition(
                    consoleOut,
                    (ulong)left,
                    (ulong)top);
                if ((ulong)status != EFI_SUCCESS)
                    throw new IO.IOException("The console cursor position could not be changed.");

                s_lineWrapped = false;
            }
        }

        public static void Write(char c)
        {
            lock (s_syncRoot)
                WriteImpl(c);
        }

        private static void WriteImpl(char c)
        {
            SIMPLE_TEXT_OUTPUT_INTERFACE* consoleOut = GetConsoleOutput();
            char* chr = stackalloc char[2];
            chr[0] = c;
            chr[1] = '\0';
            consoleOut->OutputString(consoleOut, chr);

            // UEFI advances to the next row after writing in the last column.
            // Remember that implicit wrap so WriteLine does not add another row.
            s_lineWrapped = c != '\r' && c != '\n' && c != '\b' &&
                consoleOut->Mode->CursorColumn == 0;
        }

        public static void Write(string s)
        {
            lock (s_syncRoot)
                WriteStringImpl(s);
        }

        public static void WriteLine(string s)
        {
            lock (s_syncRoot)
            {
                WriteStringImpl(s);
                WriteLineImpl();
            }
        }

        public static void Write(bool value) => Write(value ? "True" : "False");
        public static void Write(byte value) => Write(value.ToString());
        public static void Write(sbyte value) => Write(value.ToString());
        public static void Write(short value) => Write(value.ToString());
        public static void Write(ushort value) => Write(value.ToString());
        public static void Write(int value) => Write(value.ToString());
        public static void Write(uint value) => Write(value.ToString());
        public static void Write(long value) => Write(value.ToString());
        public static void Write(ulong value) => Write(value.ToString());
        public static void Write(float value) => Write(value.ToString());
        public static void Write(double value) => Write(value.ToString());
        public static void Write(object value) => Write(value?.ToString() ?? string.Empty);

        public static void Write(char[] buffer)
        {
            if (buffer == null)
                return;

            Write(buffer, 0, buffer.Length);
        }

        public static void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The character buffer cannot be null.");
            ValidateCharArrayRange(buffer, index, count);

            lock (s_syncRoot)
            {
                for (int i = 0; i < count; i++)
                    WriteImpl(buffer[index + i]);
            }
        }

        public static void Write(string format, object arg0)
            => Write(String.Format(format, arg0));

        public static void Write(string format, object arg0, object arg1)
            => Write(String.Format(format, arg0, arg1));

        public static void Write(string format, object arg0, object arg1, object arg2)
            => Write(String.Format(format, arg0, arg1, arg2));

        public static void Write(string format, params object[] args)
            => Write(String.Format(format, args));

        public static void WriteLine()
        {
            lock (s_syncRoot)
                WriteLineImpl();
        }

        public static void WriteLine(char c)
        {
            lock (s_syncRoot)
            {
                WriteImpl(c);
                WriteLineImpl();
            }
        }

        public static void WriteLine(bool value) => WriteLine(value ? "True" : "False");
        public static void WriteLine(byte value) => WriteLine(value.ToString());
        public static void WriteLine(sbyte value) => WriteLine(value.ToString());
        public static void WriteLine(short value) => WriteLine(value.ToString());
        public static void WriteLine(ushort value) => WriteLine(value.ToString());
        public static void WriteLine(int value) => WriteLine(value.ToString());
        public static void WriteLine(uint value) => WriteLine(value.ToString());
        public static void WriteLine(long value) => WriteLine(value.ToString());
        public static void WriteLine(ulong value) => WriteLine(value.ToString());
        public static void WriteLine(float value) => WriteLine(value.ToString());
        public static void WriteLine(double value) => WriteLine(value.ToString());
        public static void WriteLine(object value) => WriteLine(value?.ToString() ?? string.Empty);

        public static void WriteLine(char[] buffer)
        {
            if (buffer == null)
            {
                WriteLine();
                return;
            }

            WriteLine(buffer, 0, buffer.Length);
        }

        public static void WriteLine(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The character buffer cannot be null.");
            ValidateCharArrayRange(buffer, index, count);

            lock (s_syncRoot)
            {
                for (int i = 0; i < count; i++)
                    WriteImpl(buffer[index + i]);
                WriteLineImpl();
            }
        }

        public static void WriteLine(string format, object arg0)
            => WriteLine(String.Format(format, arg0));

        public static void WriteLine(string format, object arg0, object arg1)
            => WriteLine(String.Format(format, arg0, arg1));

        public static void WriteLine(string format, object arg0, object arg1, object arg2)
            => WriteLine(String.Format(format, arg0, arg1, arg2));

        public static void WriteLine(string format, params object[] args)
            => WriteLine(String.Format(format, args));

        private static void WriteStringImpl(string s)
        {
            if (s == null)
                return;

            for (int i = 0; i < s.Length; i++)
                WriteImpl(s[i]);
        }

        private static void ValidateCharArrayRange(char[] buffer, int index, int count)
        {
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The character buffer offset and count do not describe a valid range.");
        }

        private static void WriteLineImpl()
        {
            if (s_lineWrapped)
            {
                s_lineWrapped = false;
                return;
            }

            char* chr = stackalloc char[3];
            chr[0] = '\r';
            chr[1] = '\n';
            chr[2] = '\0';
            gST->ConOut->OutputString(gST->ConOut, chr);
            s_lineWrapped = false;
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return ReadKeyAsync().GetAwaiter().GetResult();
        }

        public static Threading.Tasks.Task<ConsoleKeyInfo> ReadKeyAsync()
            => new ReadKeyOperation().Start();

        public static string ReadLine()
        {
            Text.StringBuilder line = new Text.StringBuilder();
            while (true)
            {
                ConsoleKeyInfo key = ReadKey();
                if (key.Key == ConsoleKey.Enter || key.KeyChar == '\r' || key.KeyChar == '\n')
                {
                    WriteLine();
                    return line.ToString();
                }

                if (key.Key == ConsoleKey.Backspace || key.KeyChar == '\b')
                {
                    if (line.Length > 0)
                    {
                        line.Length--;
                        Write("\b \b");
                    }
                    continue;
                }

                char character = key.KeyChar;
                if (character < ' ' || character == '\x7F')
                    continue;

                line.Append(character);
                Write(character);
            }
        }

        public static Threading.Tasks.Task<ConsoleKeyEvent> ReadKeyEventAsync()
        {
            if (!TryStartUsbKeyboard())
                throw new Exception("A USB HID boot keyboard is unavailable.");
            return new ReadKeyEventOperation().Start();
        }

        private static ConsoleKeyInfo CreateKeyInfo(EFI_INPUT_KEY key)
        {
            char character = key.UnicodeChar;
            bool control = character >= 1 && character <= 26;
            return new ConsoleKeyInfo(character, MapKey(key), false, false, control);
        }

        private static ConsoleKey MapKey(EFI_INPUT_KEY key)
        {
            switch ((ulong)key.ScanCode)
            {
                case SCAN_UP: return ConsoleKey.UpArrow;
                case SCAN_DOWN: return ConsoleKey.DownArrow;
                case SCAN_RIGHT: return ConsoleKey.RightArrow;
                case SCAN_LEFT: return ConsoleKey.LeftArrow;
                case SCAN_HOME: return ConsoleKey.Home;
                case SCAN_END: return ConsoleKey.End;
                case SCAN_INSERT: return ConsoleKey.Insert;
                case SCAN_DELETE: return ConsoleKey.Delete;
                case SCAN_PAGE_UP: return ConsoleKey.PageUp;
                case SCAN_PAGE_DOWN: return ConsoleKey.PageDown;
                case SCAN_F1: return ConsoleKey.F1;
                case SCAN_F2: return ConsoleKey.F2;
                case SCAN_F3: return ConsoleKey.F3;
                case SCAN_F4: return ConsoleKey.F4;
                case SCAN_F5: return ConsoleKey.F5;
                case SCAN_F6: return ConsoleKey.F6;
                case SCAN_F7: return ConsoleKey.F7;
                case SCAN_F8: return ConsoleKey.F8;
                case SCAN_F9: return ConsoleKey.F9;
                case SCAN_F10: return ConsoleKey.F10;
                case SCAN_F11: return ConsoleKey.F11;
                case SCAN_F12: return ConsoleKey.F12;
                case SCAN_ESC: return ConsoleKey.Escape;
            }

            char character = key.UnicodeChar;
            if (character >= 'a' && character <= 'z')
                character = (char)(character - ('a' - 'A'));

            // Simple Text Input reports Ctrl+A through Ctrl+Z as control
            // characters. ConsoleKey identifies the letter; modifier state is
            // unavailable through EFI_SIMPLE_TEXT_INPUT_PROTOCOL.
            if (character >= 1 && character <= 26)
                return (ConsoleKey)('A' + character - 1);

            switch (character)
            {
                case '\b': return ConsoleKey.Backspace;
                case '\t': return ConsoleKey.Tab;
                case '\r': return ConsoleKey.Enter;
                case ' ': return ConsoleKey.Spacebar;
                case '\x1B': return ConsoleKey.Escape;
                case ';': return ConsoleKey.Oem1;
                case '+': return ConsoleKey.OemPlus;
                case ',': return ConsoleKey.OemComma;
                case '-': return ConsoleKey.OemMinus;
                case '.': return ConsoleKey.OemPeriod;
                case '/': return ConsoleKey.Oem2;
                case '`': return ConsoleKey.Oem3;
                case '[': return ConsoleKey.Oem4;
                case '\\': return ConsoleKey.Oem5;
                case ']': return ConsoleKey.Oem6;
                case '\'': return ConsoleKey.Oem7;
                default: return (ConsoleKey)character;
            }
        }

        private enum EFI_USB_DATA_DIRECTION : uint
        {
            EfiUsbDataIn,
            EfiUsbDataOut,
            EfiUsbNoData
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct EFI_USB_DEVICE_REQUEST
        {
            public byte RequestType;
            public byte Request;
            public ushort Value;
            public ushort Index;
            public ushort Length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct EFI_USB_INTERFACE_DESCRIPTOR
        {
            public byte Length;
            public byte DescriptorType;
            public byte InterfaceNumber;
            public byte AlternateSetting;
            public byte NumEndpoints;
            public byte InterfaceClass;
            public byte InterfaceSubClass;
            public byte InterfaceProtocol;
            public byte Interface;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct EFI_USB_ENDPOINT_DESCRIPTOR
        {
            public byte Length;
            public byte DescriptorType;
            public byte EndpointAddress;
            public byte Attributes;
            public ushort MaxPacketSize;
            public byte Interval;
        }

        private struct EFI_USB_IO_PROTOCOL
        {
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, EFI_USB_DEVICE_REQUEST*, EFI_USB_DATA_DIRECTION, uint, void*, ulong, uint*, EFI_STATUS> UsbControlTransfer;
            public readonly void* UsbBulkTransfer;
            public readonly void* UsbAsyncInterruptTransfer;
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, byte, void*, ulong*, ulong, uint*, EFI_STATUS> UsbSyncInterruptTransfer;
            public readonly void* UsbIsochronousTransfer;
            public readonly void* UsbAsyncIsochronousTransfer;
            public readonly void* UsbGetDeviceDescriptor;
            public readonly void* UsbGetConfigDescriptor;
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, EFI_USB_INTERFACE_DESCRIPTOR*, EFI_STATUS> UsbGetInterfaceDescriptor;
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, byte, EFI_USB_ENDPOINT_DESCRIPTOR*, EFI_STATUS> UsbGetEndpointDescriptor;
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, ushort, byte, char**, EFI_STATUS> UsbGetStringDescriptor;
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, ushort**, ushort*, EFI_STATUS> UsbGetSupportedLanguages;
            public readonly delegate* unmanaged<EFI_USB_IO_PROTOCOL*, EFI_STATUS> UsbPortReset;
        }

        private const byte UsbClassHid = 0x03;
        private const byte UsbSubclassBoot = 0x01;
        private const byte UsbProtocolKeyboard = 0x01;
        private const byte UsbEndpointInterrupt = 0x03;
        private const byte UsbEndpointDirectionIn = 0x80;
        private const byte UsbRequestTypeClassInterfaceOut = 0x21;
        private const byte UsbSetProtocol = 0x0B;

        private static EFI_GUID EFI_USB_IO_PROTOCOL_GUID => new EFI_GUID(0x2b2f68d6, 0x0cd2, 0x44cf, 0x8e, 0x8b, 0xbb, 0xa2, 0x0b, 0x1b, 0x5b, 0x75);

        private static byte s_usbEndpoint;
        private static ulong s_usbReportLength;
        private static readonly byte[] s_usbPreviousKeys = new byte[6];
        private static byte s_usbPreviousModifiers;
        private static readonly Queue<ConsoleKeyEvent> s_usbEvents = new Queue<ConsoleKeyEvent>();
        private static bool s_usbStartAttempted;
        private static bool s_usbStarted;

        private static bool TryStartUsbKeyboard()
        {
            return StartUsbKeyboard();
        }

        private static bool TryDequeueUsbKey(out ConsoleKeyEvent keyEvent)
        {
            return DequeueUsbKey(out keyEvent);
        }

        internal static bool TryReadKeyEvent(out ConsoleKeyEvent keyEvent)
        {
            if (!s_usbStarted)
                TryStartUsbKeyboard();

            return DequeueUsbKey(out keyEvent);
        }

        private static bool StartUsbKeyboard()
        {
            if (s_usbStarted)
                return true;
            if (s_usbStartAttempted)
                return false;

            s_usbStartAttempted = true;

            EFI_HANDLE* handles = null;
            ulong handleCount = 0;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                ByProtocol,
                (EFI_GUID*)EFI_USB_IO_PROTOCOL_GUID,
                null,
                &handleCount,
                &handles);
            if ((ulong)status != EFI_SUCCESS)
                return false;

            bool started = false;
            for (ulong i = 0; i < handleCount && !started; i++)
            {
                EFI_USB_IO_PROTOCOL* usb = null;
                status = gBS->HandleProtocol(
                    handles[i],
                    (EFI_GUID*)EFI_USB_IO_PROTOCOL_GUID,
                    (void**)&usb);
                if ((ulong)status != EFI_SUCCESS || usb == null)
                    continue;

                EFI_USB_INTERFACE_DESCRIPTOR interfaceDescriptor = default;
                status = usb->UsbGetInterfaceDescriptor(usb, &interfaceDescriptor);
                if ((ulong)status != EFI_SUCCESS ||
                    interfaceDescriptor.InterfaceClass != UsbClassHid ||
                    interfaceDescriptor.InterfaceSubClass != UsbSubclassBoot ||
                    interfaceDescriptor.InterfaceProtocol != UsbProtocolKeyboard)
                {
                    continue;
                }

                EFI_USB_ENDPOINT_DESCRIPTOR endpoint = default;
                byte endpointAddress = 0;
                for (byte endpointIndex = 0; endpointIndex < interfaceDescriptor.NumEndpoints; endpointIndex++)
                {
                    status = usb->UsbGetEndpointDescriptor(usb, endpointIndex, &endpoint);
                    if ((ulong)status == EFI_SUCCESS &&
                        (endpoint.Attributes & 0x03) == UsbEndpointInterrupt &&
                        (endpoint.EndpointAddress & UsbEndpointDirectionIn) != 0)
                    {
                        endpointAddress = endpoint.EndpointAddress;
                        break;
                    }
                }

                if (endpointAddress == 0 || endpoint.MaxPacketSize < 8)
                    continue;

                if (usb->UsbAsyncInterruptTransfer == null)
                    continue;

                // UsbKbDxe normally owns this interface and has its own asynchronous
                // transfer active. Release that driver before taking the endpoint.
                gBS->DisconnectController(handles[i], null, null);

                EFI_USB_DEVICE_REQUEST request = new EFI_USB_DEVICE_REQUEST
                {
                    RequestType = UsbRequestTypeClassInterfaceOut,
                    Request = UsbSetProtocol,
                    Value = 0,
                    Index = interfaceDescriptor.InterfaceNumber,
                    Length = 0
                };
                uint transferStatus = 0;
                status = usb->UsbControlTransfer(
                    usb,
                    &request,
                    EFI_USB_DATA_DIRECTION.EfiUsbNoData,
                    100,
                    null,
                    0,
                    &transferStatus);
                if ((ulong)status != EFI_SUCCESS)
                    continue;

                s_usbEndpoint = endpointAddress;
                s_usbReportLength = endpoint.MaxPacketSize;
                if (s_usbReportLength > 64)
                    s_usbReportLength = 64;
                for (int keyIndex = 0; keyIndex < s_usbPreviousKeys.Length; keyIndex++)
                    s_usbPreviousKeys[keyIndex] = 0;
                s_usbPreviousModifiers = 0;

                s_usbStarted = true;
                void* callback = (void*)(delegate* unmanaged<void*, ulong, void*, uint, EFI_STATUS>)&KeyboardCallback;
                status = ((delegate* unmanaged<EFI_USB_IO_PROTOCOL*, byte, bool, ulong, ulong, void*, void*, EFI_STATUS>)usb->UsbAsyncInterruptTransfer)(
                    usb,
                    s_usbEndpoint,
                    true,
                    endpoint.Interval,
                    s_usbReportLength,
                    callback,
                    null);
                if ((ulong)status != EFI_SUCCESS)
                {
                    s_usbStarted = false;
                    continue;
                }

                started = true;
            }

            if (handles != null)
                gBS->FreePool(handles);
            return started;
        }

        [UnmanagedCallersOnly]
        private static EFI_STATUS KeyboardCallback(void* data, ulong dataLength, void* context, uint transferStatus)
        {
            if (transferStatus == 0 && data != null)
                ProcessUsbReport((byte*)data, dataLength);
            return (EFI_STATUS)EFI_SUCCESS;
        }

        private static void ProcessUsbReport(byte* report, ulong length)
        {
            if (length < 8)
                return;

            byte modifiers = report[0];
            ProcessModifierTransition(modifiers, s_usbPreviousModifiers, 0x01, 0xE0);
            ProcessModifierTransition(modifiers, s_usbPreviousModifiers, 0x02, 0xE1);
            for (int i = 0; i < s_usbPreviousKeys.Length; i++)
            {
                byte usage = s_usbPreviousKeys[i];
                if (usage != 0 && !ContainsUsage(report + 2, 6, usage))
                {
                    ConsoleKey key = MapUsage(usage);
                    if (key != (ConsoleKey)0)
                        EnqueueUsbKey(new ConsoleKeyEvent(CreateKeyInfo(usage, s_usbPreviousModifiers), false));
                }
            }

            for (int i = 0; i < 6; i++)
            {
                byte usage = report[2 + i];
                if (usage != 0 && !ContainsUsage(s_usbPreviousKeys, s_usbPreviousKeys.Length, usage))
                {
                    ConsoleKey key = MapUsage(usage);
                    if (key != (ConsoleKey)0)
                        EnqueueUsbKey(new ConsoleKeyEvent(CreateKeyInfo(usage, modifiers), true));
                }
            }

            for (int i = 0; i < s_usbPreviousKeys.Length; i++)
                s_usbPreviousKeys[i] = report[2 + i];

            s_usbPreviousModifiers = modifiers;
        }

        private static void ProcessModifierTransition(byte current, byte previous, byte mask, byte usage)
        {
            bool wasPressed = (previous & mask) != 0;
            bool isPressed = (current & mask) != 0;
            if (wasPressed != isPressed)
                EnqueueUsbKey(new ConsoleKeyEvent(CreateKeyInfo(usage, current), isPressed));
        }

        private static bool ContainsUsage(byte* values, int length, byte usage)
        {
            for (int i = 0; i < length; i++)
            {
                if (values[i] == usage)
                    return true;
            }
            return false;
        }

        private static bool ContainsUsage(byte[] values, int length, byte usage)
        {
            for (int i = 0; i < length; i++)
            {
                if (values[i] == usage)
                    return true;
            }
            return false;
        }

        private static void EnqueueUsbKey(ConsoleKeyEvent keyEvent)
        {
            if (s_usbEvents.Count == 64)
                s_usbEvents.Dequeue();
            s_usbEvents.Enqueue(keyEvent);
        }

        private static bool DequeueUsbKey(out ConsoleKeyEvent keyEvent)
        {
            return s_usbEvents.TryDequeue(out keyEvent);
        }

        private static ConsoleKeyInfo CreateKeyInfo(byte usage, byte modifiers)
        {
            ConsoleKey key = MapUsage(usage);
            bool shift = (modifiers & 0x22) != 0;
            bool alt = (modifiers & 0x44) != 0;
            bool control = (modifiers & 0x11) != 0;
            char character = MapCharacter(usage, shift);
            return new ConsoleKeyInfo(character, key, shift, alt, control);
        }

        private static char MapCharacter(byte usage, bool shift)
        {
            if (usage >= 0x04 && usage <= 0x1D)
                return (char)('A' + usage - 0x04);
            if (usage >= 0x1E && usage <= 0x26)
                return (char)('1' + usage - 0x1E);
            if (usage == 0x27)
                return '0';
            if (usage == 0x28)
                return '\r';
            if (usage == 0x29)
                return '\x1B';
            if (usage == 0x2A)
                return '\b';
            if (usage == 0x2B)
                return '\t';
            if (usage == 0x2C)
                return ' ';
            return '\0';
        }

        private static ConsoleKey MapUsage(byte usage)
        {
            if (usage >= 0x04 && usage <= 0x1D)
                return (ConsoleKey)('A' + usage - 0x04);
            if (usage >= 0x1E && usage <= 0x26)
                return (ConsoleKey)(ConsoleKey.D1 + usage - 0x1E);
            if (usage == 0x27)
                return ConsoleKey.D0;

            switch (usage)
            {
                case 0x28: return ConsoleKey.Enter;
                case 0x29: return ConsoleKey.Escape;
                case 0x2A: return ConsoleKey.Backspace;
                case 0x2B: return ConsoleKey.Tab;
                case 0x2C: return ConsoleKey.Spacebar;
                case 0x2D: return ConsoleKey.OemMinus;
                case 0x2E: return ConsoleKey.OemPlus;
                case 0x2F: return ConsoleKey.Oem4;
                case 0x30: return ConsoleKey.Oem6;
                case 0x31: return ConsoleKey.Oem5;
                case 0x33: return ConsoleKey.Oem1;
                case 0x34: return ConsoleKey.Oem7;
                case 0x35: return ConsoleKey.Oem3;
                case 0x36: return ConsoleKey.OemComma;
                case 0x37: return ConsoleKey.OemPeriod;
                case 0x38: return ConsoleKey.Oem2;
                case 0x39: return (ConsoleKey)20;
                case 0x3A: return ConsoleKey.F1;
                case 0x3B: return ConsoleKey.F2;
                case 0x3C: return ConsoleKey.F3;
                case 0x3D: return ConsoleKey.F4;
                case 0x3E: return ConsoleKey.F5;
                case 0x3F: return ConsoleKey.F6;
                case 0x40: return ConsoleKey.F7;
                case 0x41: return ConsoleKey.F8;
                case 0x42: return ConsoleKey.F9;
                case 0x43: return ConsoleKey.F10;
                case 0x44: return ConsoleKey.F11;
                case 0x45: return ConsoleKey.F12;
                case 0x49: return ConsoleKey.Insert;
                case 0x4A: return ConsoleKey.Home;
                case 0x4B: return ConsoleKey.PageUp;
                case 0x4C: return ConsoleKey.Delete;
                case 0x4D: return ConsoleKey.End;
                case 0x4E: return ConsoleKey.PageDown;
                case 0x4F: return ConsoleKey.RightArrow;
                case 0x50: return ConsoleKey.LeftArrow;
                case 0x51: return ConsoleKey.DownArrow;
                case 0x52: return ConsoleKey.UpArrow;
                case 0xE0: return ConsoleKey.LeftControl;
                case 0xE1: return ConsoleKey.LeftShift;
                default: return (ConsoleKey)0;
            }
        }
    }
}
