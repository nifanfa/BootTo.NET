namespace System
{
    public static unsafe class Console
    {
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

        public static ConsoleColor ForegroundColor
        {
            set
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

        public static void Clear()
        {
            gST->ConOut->ClearScreen(gST->ConOut);
        }

        public static void Write(char c)
        {
            char* chr = stackalloc char[2];
            chr[0] = c;
            chr[1] = '\0';
            gST->ConOut->OutputString(gST->ConOut, chr);
        }

        public static void Write(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                Write(s[i]);
            }
        }

        public static void WriteLine(string s)
        {
            Write(s);
            WriteLine();
        }

        public static void WriteLine()
        {
            char* chr = stackalloc char[3];
            chr[0] = '\r';
            chr[1] = '\n';
            chr[2] = '\0';
            gST->ConOut->OutputString(gST->ConOut, chr);
        }

        public static EFI_INPUT_KEY ReadKey()
        {
            EFI_INPUT_KEY key;
            ulong keyEvent = 0;
            gBS->WaitForEvent(1, &gST->ConIn->WaitForKey, &keyEvent);
            gST->ConIn->ReadKeyStroke(gST->ConIn, &key);
            gST->ConIn->Reset(gST->ConIn, false);
            return key;
        }
    }
}
