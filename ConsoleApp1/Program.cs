global using static efi;
using System;
using System.IO;
using System.Net.Sockets;

unsafe class Program
{
    static void Main() { }

    [System.Runtime.RuntimeExport("EfiMain")]
    static EFI_STATUS EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
    {
        InitializeLib(imageHandle, systemTable);

        //Disable watchdog timer
        gBS->SetWatchdogTimer(0, 0, 0, null);

        Console.Clear();
        Console.WriteLine("Welcome to the: ");
        gST->ConOut->SetAttribute(gST->ConOut, EFI_BACKGROUND_BLACK | EFI_LIGHTGREEN);
        Console.WriteLine("  ____              _ _______     _   _ ______ _______ ");
        Console.WriteLine(" |  _ \\            | |__   __|   | \\ | |  ____|__   __| ");
        Console.WriteLine(" | |_) | ___   ___ | |_ | | ___  |  \\| | |__     | | ");
        Console.WriteLine(" |  _ < / _ \\ / _ \\| __|| |/ _ \\ | . ` |  __|    | | ");
        Console.WriteLine(" | |_) | (_) | (_) | |_ | | (_) || |\\  | |____   | | ");
        Console.WriteLine(" |____/ \\___/ \\___/ \\__||_|\\___(_)_| \\_|______|  |_| ");
        gST->ConOut->SetAttribute(gST->ConOut,EFI_BACKGROUND_BLACK | EFI_LIGHTGRAY);
        Console.WriteLine("Press any key to continue...");

        Console.ReadKey();

#if true
        #region Cursor
        int[] cursor = new int[]
            {
                1,0,0,0,0,0,0,0,0,0,0,0,
                1,1,0,0,0,0,0,0,0,0,0,0,
                1,2,1,0,0,0,0,0,0,0,0,0,
                1,2,2,1,0,0,0,0,0,0,0,0,
                1,2,2,2,1,0,0,0,0,0,0,0,
                1,2,2,2,2,1,0,0,0,0,0,0,
                1,2,2,2,2,2,1,0,0,0,0,0,
                1,2,2,2,2,2,2,1,0,0,0,0,
                1,2,2,2,2,2,2,2,1,0,0,0,
                1,2,2,2,2,2,2,2,2,1,0,0,
                1,2,2,2,2,2,2,2,2,2,1,0,
                1,2,2,2,2,2,2,2,2,2,2,1,
                1,2,2,2,2,2,2,1,1,1,1,1,
                1,2,2,2,1,2,2,1,0,0,0,0,
                1,2,2,1,0,1,2,2,1,0,0,0,
                1,2,1,0,0,1,2,2,1,0,0,0,
                1,1,0,0,0,0,1,2,2,1,0,0,
                0,0,0,0,0,0,1,2,2,1,0,0,
                0,0,0,0,0,0,0,1,1,0,0,0
            };

        EFI_SIMPLE_POINTER_PROTOCOL* pointer;
        gBS->LocateProtocol((EFI_GUID*)EFI_SIMPLE_POINTER_PROTOCOL_GUID, null, (void**)&pointer);
        GetFB(out var fb, out var width, out var height);
        EFI_SIMPLE_POINTER_STATE sts;
        pointer->Mode->ResolutionX = width;
        pointer->Mode->ResolutionY = height;
        float MouseSpeed = 200;

        int CursorX = 0;
        int CursorY = 0;

        for (; ; )
        {
            pointer->GetState(pointer, &sts);
            CursorX = Clamp(CursorX + (int)((sts.RelativeMovementX / 65536f) * MouseSpeed), 0, (int)width);
            CursorY = Clamp(CursorY + (int)((sts.RelativeMovementY / 65536f) * MouseSpeed), 0, (int)height);
            DrawCursor(fb,CursorX, CursorY);
        }

        void DrawCursor(uint* fb, int x, int y)
        {
            for (int h = 0; h < 19; h++)
            {
                for (int w = 0; w < 12; w++)
                {
                    if (cursor[h * 12 + w] == 1)
                    {
                        SetPixel(w + x, h + y, 0);
                    }
                    if (cursor[h * 12 + w] == 2)
                    {
                        SetPixel(w + x, h + y, 0xFFFFFFFF);
                    }
                }
            }
        }

        void SetPixel(int x,int y,uint color)
        {
            Clamp(x, 0, (int)width);
            Clamp(y, 0, (int)height);
            fb[width * y + x] = color;
        }

        int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        void GetFB(out uint* fb,out uint width,out uint height)
        {
            EFI_GRAPHICS_OUTPUT_PROTOCOL* gop;
            gBS->LocateProtocol((EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID, null, (void**)&gop);
            fb = (uint*)gop->Mode->FrameBufferBase;
            width = gop->Mode->Info->HorizontalResolution;
            height = gop->Mode->Info->VerticalResolution;
        }
        #endregion
#endif

#if false
        #region File Test
        byte[] buffer = File.ReadAllBytes("Test.txt");
        Console.Write("Content of Test.txt is: ");
        for(int i = 0; i < buffer.Length; i++)
        {
            Console.Write((char)buffer[i]);
        }
        #endregion
#endif

#if false
        #region GOP Test
        EFI_GRAPHICS_OUTPUT_PROTOCOL* gop;
        gBS->LocateProtocol((EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID, null, (void**)&gop);
        uint numModes = gop->Mode->MaxMode;
        ulong sizeofMode = 0;
        for (uint u = 0; u < numModes; u++)
        {
            EFI_GRAPHICS_OUTPUT_MODE_INFORMATION* modeinfo;
            gop->QueryMode(gop, u, &sizeofMode, &modeinfo);
            Console.Write("GOP Mode ");
            Console.Write(Convert.ToString(u, 10));
            Console.Write(":");
            Console.Write(Convert.ToString(modeinfo->HorizontalResolution, 10));
            Console.Write("x");
            Console.WriteLine(Convert.ToString(modeinfo->VerticalResolution, 10));
        }
        //gop->SetMode(gop,7);
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
        uint* fb = (uint*)gop->Mode->FrameBufferBase;
        for (uint w = 0; w < gop->Mode->Info->HorizontalResolution; w++)
        {
            for (uint h = 0; h < gop->Mode->Info->VerticalResolution; h++)
            {
                fb[h * gop->Mode->Info->HorizontalResolution + w] = 0xFFFF0000;
            }
        }
        Console.WriteLine("The background should be red when you see this message");
        #endregion
#endif

#if false
        #region Socket Test
        EFI_IPv4_ADDRESS address = new EFI_IPv4_ADDRESS();
        address.Addr[0] = 192;
        address.Addr[1] = 168;
        address.Addr[2] = 137;
        address.Addr[3] = 1;

        Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(address, 54188);
        socket.Send(GetBytes("Hello world from BootTo.NET project!"));
        Console.WriteLine("Try receive 64bytes from server");
        byte[] buffer = new byte[64];
        socket.Receive(buffer);
        Console.Write("Buffer received: ");
        for(int i = 0; i < buffer.Length; i++)
        {
            Console.Write((char)buffer[i]);
        }
        socket.Close();
        #endregion
#endif

        for (; ; );
    }

    public static byte[] GetBytes(string s)
    {
        byte[] buffer = new byte[s.Length];
        for (int i = 0; i < s.Length; i++) buffer[i] = (byte)s[i];
        return buffer;
    }
}
