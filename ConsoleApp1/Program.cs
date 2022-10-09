global using static efi;
using System;
using System.IO;
using System.Net.Sockets;

unsafe class Program
{
    static void Main() { }

    //QEMU corrupts BOOTX64.efi, which is probably why it doesn't work on real hardware, please notice!
    //If you want to running this on real hardware, please press ctrl+b to rebuild this repo and copy
    //files from Disk folder to your usb drive, of course you have to format your usb drive to fat32 and
    //use GUID partition table first
    [System.Runtime.RuntimeExport("EfiMain")]
    static EFI_STATUS EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
    {
        InitializeLib(imageHandle, systemTable);

        //Disable watchdog timer
        gBS->SetWatchdogTimer(0, 0, 0, null);

        Console.Clear();
        Console.WriteLine("Hello world from \"BootTo.NET project\"!");
        Console.WriteLine("Press any key to continue...");

        Console.ReadKey();

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

#if true
        #region GOP Test
        EFI_GRAPHICS_OUTPUT_PROTOCOL* gop;
        gBS->LocateProtocol(EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID, null, (void**)&gop);
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
