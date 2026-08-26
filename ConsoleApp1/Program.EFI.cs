using Playground.NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

partial class Program
{
    [RuntimeImport("*", "__managed__Main")]
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern unsafe int ManagedMain(int argc, char** argv);

    static readonly List<string> DxeDrivers = new()
    {
        @"\EFI\Drivers\RngDxe.efi",
        @"\EFI\Drivers\Hash2DxeCrypto.efi",
        @"\EFI\Drivers\DpcDxe.efi",
        @"\EFI\Drivers\DnsDxe.efi",
        @"\EFI\Drivers\HttpUtilitiesDxe.efi",
        @"\EFI\Drivers\TlsDxe.efi",
        @"\EFI\Drivers\HttpDxe.efi",
        @"\EFI\Drivers\AudioDxe.efi",
        @"\EFI\Drivers\XhciDxe.efi",
        @"\EFI\Drivers\UsbMouseDxe.efi"
    };

    [RuntimeExport("EfiMain")]
    unsafe static EFI_STATUS EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
    {
        ulong stackMarker = 0;
        GarbageCollector.InitializeStack(&stackMarker);

        InitializeLib(imageHandle, systemTable);

        //Disable watchdog timer
        gBS->SetWatchdogTimer(0, 0, 0, null);

#if false
        #region Change resolution
        {
            EFI_GRAPHICS_OUTPUT_PROTOCOL* gop;
            gBS->LocateProtocol((EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID, null, (void**)&gop);
            uint numModes = gop->Mode->MaxMode;
            ulong sizeofMode = 0;
            for (uint u = 0; u < numModes; u++)
            {
                EFI_GRAPHICS_OUTPUT_MODE_INFORMATION* modeinfo;
                gop->QueryMode(gop, u, &sizeofMode, &modeinfo);
                printf("GOP Mode %d: %dx%d\r\n"u8, u, modeinfo->HorizontalResolution, modeinfo->VerticalResolution);
            }
            Console.Write("Please select mode: ");
            gop->SetMode(gop, Convert.ToUInt32(Console.ReadLine()));
        }
        #endregion
#endif

        EFI_STATUS certificateStatus = InstallTlsCaCertificates(@"\EFI\Certificates\TlsCaCertificate.esl");
        Console.WriteLine($"TLS CA certificates {(certificateStatus == EFI_SUCCESS ? "are available!" : "are unavailable!")}");

        foreach (var driver in DxeDrivers)
        {
            EFI_STATUS status;
            if ((status = LoadDriver(driver)) != EFI_SUCCESS)
            {
                Console.WriteLine($"Driver {driver} failed to load(0x{(ulong)status:x2})!");
            }
        }

        Console.WriteLine("Connecting PCI controllers...");
        EFI_STATUS connectStatus = ConnectPciControllers();
        Console.WriteLine($"PCI controllers {(connectStatus == EFI_SUCCESS ? "are connected!" : "failed to connect!")}");

        ManagedMain(0, null);

        try
        {
            Console.WriteLine("Try throwing an exception!");
            throw new Exception("This is a test exception.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught! " + ex.Message);
        }
        finally
        {
            Console.WriteLine("Exception thrown!");
        }
        //throw new Exception("Unhandled exception...");

        //Encoding, ToString test
        Console.WriteLine(Encoding.UTF8.GetString("System available memory(MB): "u8) + (GetAvailableMemory() / 1048576f).ToString());

        Console.WriteLine("Welcome to the: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(@" /$$$$$$$                        /$$  /$$$$$$$$           /$$   /$$ /$$$$$$$$ /$$$$$$$$");
        Console.WriteLine(@"| $$__  $$                      | $$ |__  $$__/          | $$$ | $$| $$_____/|__  $$__/");
        Console.WriteLine(@"| $$  \ $$  /$$$$$$   /$$$$$$  /$$$$$$  | $$  /$$$$$$    | $$$$| $$| $$         | $$   ");
        Console.WriteLine(@"| $$$$$$$  /$$__  $$ /$$__  $$|_  $$_/  | $$ /$$__  $$   | $$ $$ $$| $$$$$      | $$   ");
        Console.WriteLine(@"| $$__  $$| $$  \ $$| $$  \ $$  | $$    | $$| $$  \ $$   | $$  $$$$| $$__/      | $$   ");
        Console.WriteLine(@"| $$  \ $$| $$  | $$| $$  | $$  | $$ /$$| $$| $$  | $$   | $$\  $$$| $$         | $$   ");
        Console.WriteLine(@"| $$$$$$$/|  $$$$$$/|  $$$$$$/  |  $$$$/| $$|  $$$$$$//$$| $$ \  $$| $$$$$$$$   | $$   ");
        Console.WriteLine(@"|_______/  \______/  \______/    \___/  |__/ \______/|__/|__/  \__/|________/   |__/   ");
        Console.ForegroundColor = ConsoleColor.Gray;

        printf("GC.Collect freed %d unreferenced objects!\r\n"u8, GC.Collect());

#if false
        Console.WriteLine("Press any key to continue...");

        Console.ReadKey();
        Console.WriteLine("Key pressed!");
#endif

#if false
        Console.WriteLine("Enjoy the song! gonna last about 2 minutes...");
        new System.Media.SoundPlayer(@"\Nokia - Breath.wav").PlaySync();
#endif

#if true
        Console.WriteLine("+++++++++++++++++++++++++++");
        string[] files = Directory.GetFiles(@"\");
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i}){files[i]}");
        }
        Console.Write("Please select NES ROM(number):");
        string file = files[Convert.ToInt32(Console.ReadLine())];
        NesTest test = new NesTest(file);
        _ = test.Run();
#endif

#if false
        #region File Test
        {
            FileTest test = new FileTest();
            _ = test.Run();
        }
        #endregion
#endif

#if false
        #region Socket Test
        SocketTest test = new SocketTest();
        _ = test.Run();
        #endregion
#endif

#if false
        #region Socket Test2
        SocketTest2 test = new SocketTest2();
        _ = test.Run();
        #endregion
#endif

#if false
        #region WebClient Test
        WebClientTest test = new WebClientTest();
        _ = test.Run();
        #endregion
#endif

#if false
        #region Serial Test
        SerialTest test = new SerialTest();
        _ = test.Run();
        #endregion
#endif

        Console.WriteLine("Finished!");
        Thread.Sleep(Timeout.Infinite);

        return EFI_SUCCESS;
    }

    class NesTest
    {
        Emulator nes = new Emulator();

        public unsafe NesTest(string rom)
        {
            Console.Clear();
            int height = Console.BufferHeight - 2;
            int width = Console.BufferWidth - 1;
            int h = 0;
            for (; h <= height; h++)
            {
                for (int i = 0; i <= width; i++)
                {
                    if (h % height == 0 || i % width == 0)
                        Console.Write('#');
                    else Console.Write(' ');
                }
                Console.WriteLine();
            }
            Console.Write(Convert.ToBoolean(IsTcg()) ? "Slow QEMU TCG detected. Enable Windows Hypervisor Platform." : string.Empty);

            nes.openROM(rom);
        }

        async Task RunInput()
        {
            for (; ; )
            {
                ConsoleKeyEvent keyEvent = await Console.ReadKeyEventAsync();
                nes.SendKey(keyEvent.Key, keyEvent.IsKeyDown);
            }
        }

        public async Task Run()
        {
            _ = RunInput();

            for (; ; )
            {
                nes.runGame();
            }
        }
    }

    class SerialTest
    {
        public async Task Run()
        {
            SerialPort port = new SerialPort("COM1", 9600);
            port.Open();
            await port.WriteAsync("Hello world from BootTo.NET project!"u8);
            port.Close();
        }
    }

    class FileTest
    {
        public async Task Run()
        {
            FileStream fs = new FileStream("Test.txt", FileMode.Open);
            byte[] buffer = new byte[fs.Length];
            await fs.ReadAsync(buffer);
            fs.Close();
            Console.Write("Content of Test.txt is: ");
            unsafe
            {
                printf("%s\r\n"u8, buffer);
            }
        }
    }

    class SocketTest
    {
        public async Task Run()
        {
            IPAddress address = IPAddress.Parse("192.168.0.102");

            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            await socket.ConnectAsync(address, 54188);
            await socket.SendAsync(Encoding.UTF8.GetBytes("Hello world from BootTo.NET project!"));
            Console.WriteLine("Try receive 64bytes from server");
            byte[] buffer = new byte[64];
            await socket.ReceiveAsync(buffer);
            unsafe
            {
                printf("Buffer received: %s\r\n"u8, buffer);
            }
            socket.Close();
        }
    }

    class SocketTest2
    {
        public async Task Run()
        {
            IPAddress address = IPAddress.Parse("192.168.0.102");

            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            await socket.ConnectAsync(address, 54188);
            await socket.SendAsync(Encoding.UTF8.GetBytes("Hello world from BootTo.NET project!"));
            Console.WriteLine("Try receive 64bytes from server");
            byte[] buffer = new byte[64];
            var result = await socket.ReceiveFromAsync(buffer);
            await socket.ReceiveAsync(buffer);
            unsafe
            {
                printf("Buffer received: %s\r\n"u8, buffer);
            }
            socket.Close();
        }
    }

    class WebClientTest
    {
        public async Task Run()
        {
            try
            {
                WebClient wc = new WebClient();
                Console.WriteLine(wc.DownloadString("https://example.com"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to run {nameof(WebClientTest)}: {e.Message}");
            }
        }
    }

    static void Resize(uint[] dst, long dstWidth, uint[] src, long srcWidth)
    {
        int srcHeight = (int)(src.Length / srcWidth);
        int dstHeight = (int)(dst.Length / dstWidth);

        for (int y = 0; y < dstHeight; y++)
        {
            int srcY = y * srcHeight / dstHeight;
            int sourceRow = srcY * (int)srcWidth;
            int destinationRow = y * (int)dstWidth;

            for (int x = 0; x < dstWidth; x++)
            {
                int srcX = x * (int)srcWidth / (int)dstWidth;

                dst[destinationRow + x] = src[sourceRow + srcX];
            }
        }
    }

    public unsafe static ulong GetAvailableMemory()
    {
        ulong memoryMapSize = 0;
        ulong mapKey = 0;
        ulong descriptorSize = 0;
        uint descriptorVersion = 0;

        EFI_STATUS status = gBS->GetMemoryMap(
            &memoryMapSize,
            null,
            &mapKey,
            &descriptorSize,
            &descriptorVersion);
        if ((ulong)status != EFI_BUFFER_TOO_SMALL ||
            descriptorSize < (ulong)sizeof(EFI_MEMORY_DESCRIPTOR))
        {
            return 0;
        }

        for (int attempt = 0; attempt < 3; attempt++)
        {
            memoryMapSize += descriptorSize * 2;
            EFI_MEMORY_DESCRIPTOR* memoryMap = null;
            status = gBS->AllocatePool(
                EfiLoaderData,
                memoryMapSize,
                (void**)&memoryMap);
            if ((ulong)status != EFI_SUCCESS || memoryMap == null)
                return 0;

            ulong actualMemoryMapSize = memoryMapSize;
            status = gBS->GetMemoryMap(
                &actualMemoryMapSize,
                memoryMap,
                &mapKey,
                &descriptorSize,
                &descriptorVersion);

            if ((ulong)status == EFI_BUFFER_TOO_SMALL)
            {
                gBS->FreePool(memoryMap);
                memoryMapSize = actualMemoryMapSize;
                continue;
            }

            if ((ulong)status != EFI_SUCCESS)
            {
                gBS->FreePool(memoryMap);
                return 0;
            }

            ulong availablePages = 0;
            for (ulong offset = 0;
                offset + descriptorSize <= actualMemoryMapSize;
                offset += descriptorSize)
            {
                EFI_MEMORY_DESCRIPTOR* descriptor =
                    (EFI_MEMORY_DESCRIPTOR*)((byte*)memoryMap + offset);
                if (descriptor->Type == (uint)EfiConventionalMemory)
                    availablePages += descriptor->NumberOfPages;
            }

            gBS->FreePool(memoryMap);
            return availablePages * EFI_PAGE_SIZE;
        }

        return 0;
    }

    unsafe static EFI_STATUS LoadDriver(string path)
    {
        if (!File.Exists(path))
            return EFI_NOT_FOUND;

        byte[] image = File.ReadAllBytes(path);
        EFI_HANDLE driverHandle = (void*)null;
        EFI_STATUS status;
        fixed (byte* imageBuffer = image)
            status = gBS->LoadImage(false, gImageHandle, null, imageBuffer, (ulong)image.Length, &driverHandle);

        if ((ulong)status != EFI_SUCCESS)
            return status;

        status = gBS->StartImage(driverHandle, null, null);
        if ((ulong)status != EFI_SUCCESS)
        {
            gBS->UnloadImage(driverHandle);
            return status;
        }

        return EFI_SUCCESS;
    }

    unsafe static EFI_STATUS InstallTlsCaCertificates(string path)
    {
        EFI_GUID TlsCaCertificateGuid = new EFI_GUID(
            0xfd2340d0, 0x3dab, 0x4349, 0xa6, 0xc7, 0x3b, 0x4f, 0x12, 0xb4, 0x8e, 0xae);

        fixed (char* variableName = "TlsCaCertificate")
        {
            ulong existingSize = 0;
            EFI_STATUS status = gRT->GetVariable(variableName, &TlsCaCertificateGuid, null, &existingSize, null);
            if ((ulong)status == EFI_SUCCESS || (ulong)status == EFI_BUFFER_TOO_SMALL)
                return EFI_SUCCESS;
            if ((ulong)status != EFI_NOT_FOUND)
                return status;

            if (!File.Exists(path))
                return EFI_NOT_FOUND;

            byte[] certificates = File.ReadAllBytes(path);
            fixed (byte* certificateData = certificates)
            {
                return gRT->SetVariable(
                    variableName,
                    &TlsCaCertificateGuid,
                    (uint)EFI_VARIABLE_BOOTSERVICE_ACCESS,
                    (ulong)certificates.Length,
                    certificateData);
            }
        }
    }

    unsafe static EFI_STATUS ConnectPciControllers()
    {
        EFI_HANDLE* handles = null;
        ulong handleCount = 0;
        EFI_GUID pciIoProtocol = EFI_PCI_IO_PROTOCOL_GUID;
        EFI_STATUS status = gBS->LocateHandleBuffer(
            ByProtocol,
            &pciIoProtocol,
            null,
            &handleCount,
            &handles);
        if ((ulong)status != EFI_SUCCESS)
            return status;

        for (ulong i = 0; i < handleCount; i++)
            gBS->ConnectController(handles[i], null, null, true);

        if (handles != null)
            gBS->FreePool(handles);

        return EFI_SUCCESS;
    }
}
