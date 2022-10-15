namespace System
{
    public static unsafe class Console
    {
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
            fixed(char* ptr = s)
            {
                gST->ConOut->OutputString(gST->ConOut, ptr);
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
            chr[0] = '\n';
            chr[1] = '\r';
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
