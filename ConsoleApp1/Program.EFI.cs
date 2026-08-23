using Playground.NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
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
        @"\EFI\Drivers\AudioDxe.efi",
        @"\EFI\Drivers\XhciDxe.efi",
        @"\EFI\Drivers\UsbMouseDxe.efi"
    };

    [System.Runtime.RuntimeExport("EfiMain")]
    unsafe static EFI_STATUS EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
    {
        ulong stackMarker = 0;
        EFI.GC.InitializeStack(&stackMarker);

        InitializeLib(imageHandle, systemTable);

        //Disable watchdog timer
        gBS->SetWatchdogTimer(0, 0, 0, null);

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

        foreach (var driver in DxeDrivers)
        {
            WriteLoadDriver(driver);
        }

        void WriteLoadDriver(string path)
        {
            EFI_STATUS driverStatus = LoadDriver(path);
            Console.WriteLine("Driver " + path + " " + (driverStatus == EFI_SUCCESS ? "is loaded!" : "failed to load!"));
        }

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

#if true
        Console.WriteLine("Press any key to continue...");

        Console.ReadKey();
        Console.WriteLine("Key pressed!");
#endif

#if false
        Console.WriteLine("Enjoy the song! gonna last about 2 minutes...");
        WavPlayer.Play(@"\Nokia - Breath.wav");
#endif

#if true
        EFI_GRAPHICS_OUTPUT_PROTOCOL* Graphics = null;
        EFI_STATUS GraphicsStatus = gBS->LocateProtocol(
            (EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID,
            null,
            (void**)&Graphics);
        if ((ulong)GraphicsStatus != EFI_SUCCESS ||
            Graphics == null ||
            Graphics->Mode == null ||
            Graphics->Mode->Info == null)
        {
            return (ulong)GraphicsStatus == EFI_SUCCESS ? EFI_DEVICE_ERROR : GraphicsStatus;
        }

        Console.WriteLine("+++++++++++++++++++++++++++");
        string[] files = Directory.GetFiles(@"\");
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i}){files[i]}");
        }
        Console.Write("Please select NES ROM(number):");
        string file = files[Convert.ToInt32(Console.ReadLine())];
        NesTest test = new NesTest(Graphics, file);
        _ = test.Run();
#endif

#if false
        #region NyanCat
        {
            EFI_GRAPHICS_OUTPUT_PROTOCOL* Graphics = null;
            EFI_STATUS GraphicsStatus = gBS->LocateProtocol(
                (EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID,
                null,
                (void**)&Graphics);
            if ((ulong)GraphicsStatus != EFI_SUCCESS ||
                Graphics == null ||
                Graphics->Mode == null ||
                Graphics->Mode->Info == null)
            {
                return (ulong)GraphicsStatus == EFI_SUCCESS ? EFI_DEVICE_ERROR : GraphicsStatus;
            }

            int ScreenWidth = (int)Graphics->Mode->Info->HorizontalResolution;
            int ScreenHeight = (int)Graphics->Mode->Info->VerticalResolution;
            uint[] Frame = new uint[NyanCat.PixelCount];
            uint[] Screen = new uint[ScreenWidth * ScreenHeight];

            for (; ; )
            {
                for (int frameIndex = 0; frameIndex < NyanCat.FrameCount; frameIndex++)
                {
                    NyanCat.DecodeFrame(frameIndex, Frame);
                    Resize(Screen, ScreenWidth, Frame, NyanCat.Width);

                    fixed (uint* pixels = Screen)
                    {
                        Graphics->Blt(
                            Graphics,
                            (EFI_GRAPHICS_OUTPUT_BLT_PIXEL*)pixels,
                            EfiBltBufferToVideo,
                            0,
                            0,
                            0,
                            0,
                            (ulong)ScreenWidth,
                            (ulong)ScreenHeight,
                            0);
                    }

                    Thread.Sleep(NyanCat.FrameDelayMilliseconds);
                }
            }
        }
        #endregion
#endif

#if false
        #region Cursor
        {
            int[] cursor = new int[]
        {
            1,0,0,0,0,0,0,0,0,0,0,0,
            1,1,0,0,0,0,0,0,0,0,0,0,
            1,0,1,0,0,0,0,0,0,0,0,0,
            1,0,0,1,0,0,0,0,0,0,0,0,
            1,0,0,0,1,0,0,0,0,0,0,0,
            1,0,0,0,0,1,0,0,0,0,0,0,
            1,0,0,0,0,0,1,0,0,0,0,0,
            1,0,0,0,0,0,0,1,0,0,0,0,
            1,0,0,0,0,0,0,0,1,0,0,0,
            1,0,0,0,0,0,0,0,0,1,0,0,
            1,0,0,0,0,0,0,0,0,0,1,0,
            1,0,0,0,0,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,1,1,1,1,1,
            1,0,0,0,1,0,0,1,0,0,0,0,
            1,0,0,1,0,1,0,0,1,0,0,0,
            1,0,1,0,0,1,0,0,1,0,0,0,
            1,1,0,0,0,0,1,0,0,1,0,0,
            0,0,0,0,0,0,1,0,0,1,0,0,
            0,0,0,0,0,0,0,1,1,0,0,0
        };

            EFI_SIMPLE_POINTER_PROTOCOL* simplePointer = null;
            EFI_STATUS simplePointerStatus = gBS->LocateProtocol((EFI_GUID*)EFI_SIMPLE_POINTER_PROTOCOL_GUID, null, (void**)&simplePointer);
            if ((ulong)simplePointerStatus != EFI_SUCCESS || simplePointer == null || simplePointer->Mode == null)
            {
                Console.WriteLine("EFI Simple Pointer Protocol is unavailable.");
                return (ulong)simplePointerStatus == EFI_SUCCESS ? EFI_DEVICE_ERROR : simplePointerStatus;
            }

            GetFB(out var fb, out var width, out var height);
            EFI_SIMPLE_POINTER_STATE simpleState;
            const double PointerPixelsPerMillimeter = 4.0;
            double remainingMovementX = 0;
            double remainingMovementY = 0;

            int CursorX = 640;
            int CursorY = 400;
            DrawCursor(fb, CursorX, CursorY);

            for (; ; )
            {
                EFI_STATUS stateStatus = simplePointer->GetState(simplePointer, &simpleState);
                if ((ulong)stateStatus != EFI_SUCCESS)
                    continue;

                remainingMovementX += ScalePointerMovement(simpleState.RelativeMovementX, simplePointer->Mode->ResolutionX);
                remainingMovementY += ScalePointerMovement(simpleState.RelativeMovementY, simplePointer->Mode->ResolutionY);

                int movementX = (int)remainingMovementX;
                int movementY = (int)remainingMovementY;
                remainingMovementX -= movementX;
                remainingMovementY -= movementY;

                int x = Clamp(CursorX + movementX, 0, (int)width - 1);
                int y = Clamp(CursorY + movementY, 0, (int)height - 1);

                if (CursorX != x || CursorY != y)
                {
                    DrawCursor(fb, CursorX, CursorY, true);
                    CursorX = x;
                    CursorY = y;
                    DrawCursor(fb, CursorX, CursorY);
                }
            }

            void DrawCursor(uint* fb, int x, int y, bool clear = false)
            {
                for (int h = 0; h < 19; h++)
                {
                    for (int w = 0; w < 12; w++)
                    {
                        if (cursor[h * 12 + w] == 0 || clear)
                        {
                            SetPixel(w + x, h + y, 0);
                        }
                        else if (cursor[h * 12 + w] == 1)
                        {
                            SetPixel(w + x, h + y, 0xFFFFFFFF);
                        }
                    }
                }
            }

            void SetPixel(int x, int y, uint color)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                    return;

                fb[width * y + x] = color;
            }

            double ScalePointerMovement(int movement, ulong resolution)
            {
                return resolution == 0
                    ? movement
                    : movement * PointerPixelsPerMillimeter / resolution;
            }

            int Clamp(int value, int min, int max)
            {
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }

            void GetFB(out uint* fb, out uint width, out uint height)
            {
                EFI_GRAPHICS_OUTPUT_PROTOCOL* gop;
                gBS->LocateProtocol((EFI_GUID*)EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID, null, (void**)&gop);
                fb = (uint*)gop->Mode->FrameBufferBase;
                width = gop->Mode->Info->HorizontalResolution;
                height = gop->Mode->Info->VerticalResolution;
            }
        }
        #endregion
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
        #region GOP Test
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
            //gop->SetMode(gop,7);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
            var fb = (uint*)gop->Mode->FrameBufferBase;
            for (uint w = 0; w < gop->Mode->Info->HorizontalResolution; w++)
            {
                for (uint h = 0; h < gop->Mode->Info->VerticalResolution; h++)
                {
                    fb[h * gop->Mode->Info->HorizontalResolution + w] = 0xFFFF0000;
                }
            }
            Console.WriteLine("The background should be red when you see this message");
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

        unsafe EFI_GRAPHICS_OUTPUT_PROTOCOL* Graphics;
        int ScreenWidth, ScreenHeight;

        uint[] cachedDisplayBuffer;

        public unsafe NesTest(EFI_GRAPHICS_OUTPUT_PROTOCOL* graphics, string rom)
        {
            Graphics = graphics;

            ScreenWidth = (int)Graphics->Mode->Info->HorizontalResolution;
            ScreenHeight = (int)Graphics->Mode->Info->VerticalResolution;

            cachedDisplayBuffer = new uint[nes.gameRender.screenWidth * nes.gameRender.screenHeight];

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

                if (nes.gameRender.screenUpdated)
                {
                    int baseX = (ScreenWidth / 2) - (nes.gameRender.screenWidth / 2);
                    int baseY = (ScreenHeight / 2) - (nes.gameRender.screenHeight / 2);

                    for (int y = 0; y < nes.gameRender.screenHeight; y++)
                    {
                        for (int x = 0; x < nes.gameRender.screenWidth; x++)
                        {
                            if (cachedDisplayBuffer[y * nes.gameRender.screenWidth + x] != nes.gameRender.displayBuffer[y * nes.gameRender.screenWidth + x])
                            {
                                uint color = nes.gameRender.displayBuffer[y * nes.gameRender.screenWidth + x];
                                cachedDisplayBuffer[y * nes.gameRender.screenWidth + x] = color;
                                unsafe
                                {
                                    Graphics->Blt(Graphics, (EFI_GRAPHICS_OUTPUT_BLT_PIXEL*)&color, EfiBltBufferToVideo, 0,0, (ulong)(baseX + x), (ulong)(baseY + y), 1, 1, 0);
                                }
                            }
                        }
                    }

                    nes.gameRender.screenUpdated = false;
                }
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
            System.Net.IPAddress address = System.Net.IPAddress.Parse("192.168.0.102");

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
        if (!System.IO.File.Exists(path))
            return EFI_NOT_FOUND;

        byte[] image = System.IO.File.ReadAllBytes(path);
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

        EFI_HANDLE* handles = null;
        ulong handleCount = 0;
        status = gBS->LocateHandleBuffer(AllHandles, null, null, &handleCount, &handles);
        if ((ulong)status != EFI_SUCCESS)
            return status;

        for (ulong i = 0; i < handleCount; i++)
            gBS->ConnectController(handles[i], null, null, true);

        if (handles != null)
            gBS->FreePool(handles);

        return EFI_SUCCESS;
    }
}
