using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal enum EFI_USB_DATA_DIRECTION : uint
{
    EfiUsbDataIn,
    EfiUsbDataOut,
    EfiUsbNoData
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct EFI_USB_DEVICE_REQUEST
{
    public byte RequestType;
    public byte Request;
    public ushort Value;
    public ushort Index;
    public ushort Length;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct EFI_USB_INTERFACE_DESCRIPTOR
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
internal struct EFI_USB_ENDPOINT_DESCRIPTOR
{
    public byte Length;
    public byte DescriptorType;
    public byte EndpointAddress;
    public byte Attributes;
    public ushort MaxPacketSize;
    public byte Interval;
}

internal unsafe struct EFI_USB_IO_PROTOCOL
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

internal unsafe sealed class UsbKeyboard
{
    private const byte UsbClassHid = 0x03;
    private const byte UsbSubclassBoot = 0x01;
    private const byte UsbProtocolKeyboard = 0x01;
    private const byte UsbEndpointInterrupt = 0x03;
    private const byte UsbEndpointDirectionIn = 0x80;
    private const byte UsbRequestTypeClassInterfaceOut = 0x21;
    private const byte UsbSetProtocol = 0x0B;

    private static EFI_GUID EFI_USB_IO_PROTOCOL_GUID => new EFI_GUID(0x2b2f68d6, 0x0cd2, 0x44cf, 0x8e, 0x8b, 0xbb, 0xa2, 0x0b, 0x1b, 0x5b, 0x75);

    private static readonly UsbKeyboard s_instance = new UsbKeyboard();

    private byte _endpoint;
    private ulong _reportLength;
    private byte[] _previousKeys;
    private byte _previousModifiers;
    private bool[] _pressed;
    private Queue<ConsoleKeyEvent> _events;
    private bool _startAttempted;
    private bool _started;

    private UsbKeyboard()
    {
        _previousKeys = new byte[6];
        _pressed = new bool[256];
        _events = new Queue<ConsoleKeyEvent>(64);
    }

    internal static bool TryStart()
    {
        return s_instance.Start();
    }

    internal static bool TryDequeue(out ConsoleKeyEvent keyEvent)
    {
        return s_instance.Dequeue(out keyEvent);
    }

    internal static bool IsKeyDown(ConsoleKey key)
    {
        return s_instance.IsPressed(key);
    }

    private bool Start()
    {
        if (_started)
            return true;
        if (_startAttempted)
            return false;

        _startAttempted = true;

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
            DisconnectKeyboardDriver(handles[i]);

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

            _endpoint = endpointAddress;
            _reportLength = endpoint.MaxPacketSize;
            if (_reportLength > 64)
                _reportLength = 64;
            for (int keyIndex = 0; keyIndex < _previousKeys.Length; keyIndex++)
                _previousKeys[keyIndex] = 0;
            _previousModifiers = 0;

            _started = true;
            void* callback = (void*)(delegate* unmanaged<void*, ulong, void*, uint, EFI_STATUS>)&KeyboardCallback;
            status = ((delegate* unmanaged<EFI_USB_IO_PROTOCOL*, byte, bool, ulong, ulong, void*, void*, EFI_STATUS>)usb->UsbAsyncInterruptTransfer)(
                usb,
                _endpoint,
                true,
                endpoint.Interval,
                _reportLength,
                callback,
                null);
            if ((ulong)status != EFI_SUCCESS)
            {
                _started = false;
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
            s_instance.ProcessReport((byte*)data, dataLength);
        return (EFI_STATUS)EFI_SUCCESS;
    }

    private static void DisconnectKeyboardDriver(EFI_HANDLE controller)
    {
        // The USB interface handle is owned by UsbKbDxe. Passing null as the
        // driver image asks Boot Services to stop every driver on this child
        // handle while preserving the parent USB host controller.
        gBS->DisconnectController(controller, default, null);
    }

    private void ProcessReport(byte* report, ulong length)
    {
        if (length < 8)
            return;

        byte modifiers = report[0];
        for (int i = 0; i < _previousKeys.Length; i++)
        {
            byte usage = _previousKeys[i];
            if (usage != 0 && !ContainsUsage(report + 2, 6, usage))
            {
                _pressed[usage] = false;
                ConsoleKey key = MapUsage(usage);
                if (key != (ConsoleKey)0)
                    Enqueue(new ConsoleKeyEvent(CreateKeyInfo(usage, _previousModifiers), false));
            }
        }

        for (int i = 0; i < 6; i++)
        {
            byte usage = report[2 + i];
            if (usage != 0 && !ContainsUsage(_previousKeys, _previousKeys.Length, usage))
            {
                _pressed[usage] = true;
                ConsoleKey key = MapUsage(usage);
                if (key != (ConsoleKey)0)
                    Enqueue(new ConsoleKeyEvent(CreateKeyInfo(usage, modifiers), true));
            }
        }

        for (int i = 0; i < _previousKeys.Length; i++)
            _previousKeys[i] = report[2 + i];

        UpdateModifier(0xE0, (modifiers & 0x01) != 0, _previousModifiers, 0x01);
        UpdateModifier(0xE1, (modifiers & 0x02) != 0, _previousModifiers, 0x02);
        UpdateModifier(0xE2, (modifiers & 0x04) != 0, _previousModifiers, 0x04);
        UpdateModifier(0xE4, (modifiers & 0x10) != 0, _previousModifiers, 0x10);
        UpdateModifier(0xE5, (modifiers & 0x20) != 0, _previousModifiers, 0x20);
        UpdateModifier(0xE6, (modifiers & 0x40) != 0, _previousModifiers, 0x40);
        _previousModifiers = modifiers;
    }

    private void UpdateModifier(byte usage, bool isDown, byte previousModifiers, byte mask)
    {
        bool wasDown = (previousModifiers & mask) != 0;
        if (wasDown != isDown)
            _pressed[usage] = isDown;
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

    private void Enqueue(ConsoleKeyEvent keyEvent)
    {
        if (_events.Count == 64)
            _events.Dequeue();
        _events.Enqueue(keyEvent);
    }

    private bool Dequeue(out ConsoleKeyEvent keyEvent)
    {
        return _events.TryDequeue(out keyEvent);
    }

    private bool IsPressed(ConsoleKey key)
    {
        for (int usage = 0; usage < _pressed.Length; usage++)
        {
            if (_pressed[usage] && MapUsage((byte)usage) == key)
                return true;
        }
        return false;
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
            default: return (ConsoleKey)0;
        }
    }
}
