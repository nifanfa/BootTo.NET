using System;
using System.Text;

unsafe class Program
{
    static void Main() { }

    [System.Runtime.RuntimeExport("EfiMain")]
    static EFI_STATUS EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
    {
        ulong stackMarker = 0;
        GarbageCollector.InitializeStack(&stackMarker);
        InitializeLib(imageHandle, systemTable);

        //Disable watchdog timer
        gBS->SetWatchdogTimer(0, 0, 0, null);

        Console.Clear();

        double pi = 3.1415926;
        int one = 1;
        printf("hello world from printf! one: %d, pi: %f\r\n"u8, one, pi);

        //Encoding, ToString test
        Console.Write(Encoding.UTF8.GetString("System available memory(MB): "u8));
        Console.WriteLine((GetAvailableMemory() / 1048576f).ToString());

        WriteLoadDriver(@"\EFI\Drivers\UsbKbDxe.efi");
        WriteLoadDriver(@"\EFI\Drivers\UsbMouseDxe.efi");

        void WriteLoadDriver(string path)
        {
            Console.Write("Driver ");
            EFI_STATUS usbMouseDriverStatus = LoadDriver(path);
            Console.Write(path);
            Console.Write(' ');
            Console.WriteLine(usbMouseDriverStatus == EFI_SUCCESS ? "is loaded!" : "failed to load!");
        }

        Console.WriteLine("Welcome to the: ");
        gST->ConOut->SetAttribute(gST->ConOut, EFI_BACKGROUND_BLACK | EFI_LIGHTGREEN);
        Console.WriteLine("  ____              _ _______     _   _ ______ _______ ");
        Console.WriteLine(" |  _ \\            | |__   __|   | \\ | |  ____|__   __| ");
        Console.WriteLine(" | |_) | ___   ___ | |_ | | ___  |  \\| | |__     | | ");
        Console.WriteLine(" |  _ < / _ \\ / _ \\| __|| |/ _ \\ | . ` |  __|    | | ");
        Console.WriteLine(" | |_) | (_) | (_) | |_ | | (_) || |\\  | |____   | | ");
        Console.WriteLine(" |____/ \\___/ \\___/ \\__||_|\\___(_)_| \\_|______|  |_| ");
        gST->ConOut->SetAttribute(gST->ConOut, EFI_BACKGROUND_BLACK | EFI_LIGHTGRAY);

        Console.WriteLine("Press any key to continue...");

        Console.ReadKey();
        Console.WriteLine("Key pressed!");

#if true
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
                    for (int y = 0; y < ScreenHeight; y++)
                    {
                        int sourceRow = y * NyanCat.Height / ScreenHeight * NyanCat.Width;
                        int destinationRow = y * ScreenWidth;
                        for (int x = 0; x < ScreenWidth; x++)
                        {
                            Screen[destinationRow + x] =
                                Frame[sourceRow + x * NyanCat.Width / ScreenWidth];
                        }
                    }

                    fixed (uint* pixels = Screen)
                    {
                        Graphics->Blt(
                            Graphics,
                            (EFI_GRAPHICS_OUTPUT_BLT_PIXEL*)pixels,
                            EFI_GRAPHICS_OUTPUT_BLT_OPERATION.EfiBltBufferToVideo,
                            0,
                            0,
                            0,
                            0,
                            (ulong)ScreenWidth,
                            (ulong)ScreenHeight,
                            0);
                    }

                    gBS->Stall((ulong)NyanCat.FrameDelayMilliseconds * 1000);
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
            byte[] buffer = File.ReadAllBytes("Test.txt");
            Console.Write("Content of Test.txt is: ");
            for (int i = 0; i < buffer.Length; i++)
            {
                Console.Write((char)buffer[i]);
            }
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
        {
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
            for (int i = 0; i < buffer.Length; i++)
            {
                Console.Write((char)buffer[i]);
            }
            socket.Close();
        }
        #endregion
#endif

        for (; ; );
    }

    public static ulong GetAvailableMemory()
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
                EFI_MEMORY_TYPE.EfiLoaderData,
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
                if (descriptor->Type == (uint)EFI_MEMORY_TYPE.EfiConventionalMemory)
                    availablePages += descriptor->NumberOfPages;
            }

            gBS->FreePool(memoryMap);
            return availablePages * EFI_PAGE_SIZE;
        }

        return 0;
    }

    public static byte[] GetBytes(string s)
    {
        byte[] buffer = new byte[s.Length];
        for (int i = 0; i < s.Length; i++) buffer[i] = (byte)s[i];
        return buffer;
    }

    static EFI_STATUS LoadDriver(string path)
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
        status = gBS->LocateHandleBuffer(EFI_LOCATE_SEARCH_TYPE.AllHandles, null, null, &handleCount, &handles);
        if ((ulong)status != EFI_SUCCESS)
            return status;

        for (ulong i = 0; i < handleCount; i++)
            gBS->ConnectController(handles[i], null, null, true);

        if (handles != null)
            gBS->FreePool(handles);

        return EFI_SUCCESS;
    }
}
