namespace System
{
    public static unsafe class Console
    {
        private sealed class ReadKeyOperation : TaskPoller
        {
            private System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> _completion;

            internal System.Threading.Tasks.Task<ConsoleKeyInfo> Start()
            {
                System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo> completion =
                    new System.Threading.Tasks.TaskCompletionSource<ConsoleKeyInfo>();
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
        private static readonly object s_syncRoot = new object();
        static Console()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.Gray;
            gST->ConOut->EnableCursor(gST->ConOut, true);
            Clear();
        }

        static ulong EfiBackgroundColor, EfiForegroundColor;

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
                gST->ConOut->ClearScreen(gST->ConOut);
        }

        public static void Write(char c)
        {
            lock (s_syncRoot)
                WriteImpl(c);
        }

        private static void WriteImpl(char c)
        {
            char* chr = stackalloc char[2];
            chr[0] = c;
            chr[1] = '\0';
            gST->ConOut->OutputString(gST->ConOut, chr);
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
            char* chr = stackalloc char[3];
            chr[0] = '\r';
            chr[1] = '\n';
            chr[2] = '\0';
            gST->ConOut->OutputString(gST->ConOut, chr);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return ReadKeyAsync().GetAwaiter().GetResult();
        }

        public static System.Threading.Tasks.Task<ConsoleKeyInfo> ReadKeyAsync()
            => new ReadKeyOperation().Start();

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
