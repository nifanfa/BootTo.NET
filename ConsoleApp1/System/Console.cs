namespace System
{
    public static unsafe class Console
    {
        private sealed class ReadKeyOperation : TaskPoller
        {
            private System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> _completion;
            private bool _useUsbKeyboard;

            internal System.Threading.Tasks.Task<ConsoleKeyInfo> Start()
            {
                System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion =
                    new System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo>();
                _completion = completion;
                _useUsbKeyboard = UsbKeyboard.TryStart();
                TaskScheduler.Register(this);
                Poll();
                return completion.Task;
            }

            internal override void Poll()
            {
                if (_useUsbKeyboard)
                {
                    while (UsbKeyboard.TryDequeue(out ConsoleKeyEvent keyEvent))
                    {
                        if (keyEvent.IsKeyDown)
                        {
                            Complete(keyEvent.KeyInfo);
                            return;
                        }
                    }
                    return;
                }

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
                System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion = _completion;
                _completion = null;
                TaskScheduler.Unregister(this);
                if (completion != null)
                    completion.TrySetResult(key);
            }

            private void CompleteException()
            {
                System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion = _completion;
                _completion = null;
                TaskScheduler.Unregister(this);
                if (completion != null)
                    completion.TrySetException(new Exception("The console input protocol is unavailable."));
            }
        }

        private sealed class ReadKeyEventOperation : TaskPoller
        {
            private System.Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent> _completion;

            internal System.Threading.Tasks.Task<ConsoleKeyEvent> Start()
            {
                System.Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent> completion =
                    new System.Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent>();
                _completion = completion;
                TaskScheduler.Register(this);
                Poll();
                return completion.Task;
            }

            internal override void Poll()
            {
                if (!UsbKeyboard.TryDequeue(out ConsoleKeyEvent keyEvent))
                    return;

                System.Threading.Tasks.TaskCompletionSource<ConsoleKeyEvent> completion = _completion;
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
                throw new System.IO.IOException();
            if (columns > (ulong)int.MaxValue || rows > (ulong)int.MaxValue)
                throw new OverflowException();

            width = (int)columns;
            height = (int)rows;
        }

        private static void SetBufferDimensions(int value, bool height)
        {
            if (value <= 0)
                throw new ArgumentException();

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
                        throw new System.IO.IOException();
                    s_lineWrapped = false;
                    return;
                }
            }

            throw new ArgumentException();
        }

        private static SIMPLE_TEXT_OUTPUT_INTERFACE* GetConsoleOutput()
        {
            if ((void*)gST == null || (void*)gST->ConOut == null ||
                (void*)gST->ConOut->Mode == null)
                throw new InvalidOperationException();

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
            {
                for (int i = 0; i < s.Length; i++)
                    WriteImpl(s[i]);
            }
        }

        public static void WriteLine(string s)
        {
            lock (s_syncRoot)
            {
                for (int i = 0; i < s.Length; i++)
                    WriteImpl(s[i]);
                WriteLineImpl();
            }
        }

        public static void WriteLine()
        {
            lock (s_syncRoot)
                WriteLineImpl();
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

        public static System.Threading.Tasks.Task<ConsoleKeyInfo> ReadKeyAsync()
            => new ReadKeyOperation().Start();

        public static System.Threading.Tasks.Task<ConsoleKeyEvent> ReadKeyEventAsync()
        {
            if (!UsbKeyboard.TryStart())
                throw new Exception("A USB HID boot keyboard is unavailable.");
            return new ReadKeyEventOperation().Start();
        }

        public static bool IsKeyDown(ConsoleKey key)
        {
            return UsbKeyboard.TryStart() && UsbKeyboard.IsKeyDown(key);
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
    }
}
