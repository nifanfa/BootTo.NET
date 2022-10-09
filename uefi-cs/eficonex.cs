/*++

Copyright (c) 2020 Kagurazaka Kotori <kagurazakakotori@gmail.com>

Module Name:

    eficonex.h

Abstract:

    EFI console extension protocols

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_KEY_TOGGLE_STATE
{
    byte Value;

    public static implicit operator EFI_KEY_TOGGLE_STATE(byte value) => new EFI_KEY_TOGGLE_STATE() { Value = value };
    public static implicit operator byte(EFI_KEY_TOGGLE_STATE value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_KEY_STATE
{
    public uint KeyShiftState;
    public EFI_KEY_TOGGLE_STATE KeyToggleState;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_KEY_DATA
{
    public EFI_INPUT_KEY Key;
    public EFI_KEY_STATE KeyState;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, EFI_KEY_DATA*, EFI_STATUS> ReadKeyStrokeEx;
    public EFI_EVENT WaitForKeyEx;
    public readonly delegate* unmanaged<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, EFI_KEY_TOGGLE_STATE*, EFI_STATUS> SetState;
    public readonly delegate* unmanaged<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, EFI_KEY_DATA*, delegate* unmanaged<EFI_KEY_DATA*, EFI_STATUS>, void**, EFI_STATUS> RegisterKeyNotify;
    public readonly delegate* unmanaged<EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*, void*, EFI_STATUS> UnregisterKeyNotify;
}

