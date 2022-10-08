/*++
Copyright (c) 2000  Intel Corporation

Module name:
    efi_nii.h

Abstract:

Revision history:
    2000-Feb-18 M(f)J   GUID updated.
                Structure order changed for machine word alignment.
                Added StringId[4] to structure.

    2000-Feb-14 M(f)J   Genesis.
--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_NETWORK_INTERFACE_IDENTIFIER_PROTOCOL
{

    public ulong Revision;
    // Revision of the network interface identifier protocol interface.

    public ulong ID;
    // Address of the first byte of the identifying ure for this
    // network interface.  This is set to zero if there is no ure.
    //
    // For PXE/UNDI this is the first byte of the !PXE ure.

    public ulong ImageAddr;
    // Address of the UNrelocated driver/ROM image.  This is set
    // to zero if there is no driver/ROM image.
    //
    // For 16-bit UNDI, this is the first byte of the option ROM in
    // upper memory.
    //
    // For 32/64-bit S/W UNDI, this is the first byte of the EFI ROM
    // image.
    //
    // For H/W UNDI, this is set to zero.

    public uint ImageSize;
    // Size of the UNrelocated driver/ROM image of this network interface.
    // This is set to zero if there is no driver/ROM image.

    public fixed byte StringId[4];
    // 4 char ASCII string to go in class identifier (option 60) in DHCP
    // and Boot Server discover packets.
    // For EfiNetworkInterfaceUndi this field is "UNDI".
    // For EfiNetworkInterfaceSnp this field is "SNPN".

    public byte Type;
    public byte MajorVer;
    public byte MinorVer;
    // Information to be placed into the PXE DHCP and Discover packets.
    // This is the network interface type and version number that will
    // be placed into DHCP option 94 (client network interface identifier).
    public bool Ipv6Supported;
    public byte IfNum;	// interface number to be used with pxeid ure
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_NETWORK_INTERFACE_IDENTIFIER_INTERFACE
{

    public ulong Revision;
    // Revision of the network interface identifier protocol interface.

    public ulong ID;
    // Address of the first byte of the identifying ure for this
    // network interface.  This is set to zero if there is no ure.
    //
    // For PXE/UNDI this is the first byte of the !PXE ure.

    public ulong ImageAddr;
    // Address of the UNrelocated driver/ROM image.  This is set
    // to zero if there is no driver/ROM image.
    //
    // For 16-bit UNDI, this is the first byte of the option ROM in
    // upper memory.
    //
    // For 32/64-bit S/W UNDI, this is the first byte of the EFI ROM
    // image.
    //
    // For H/W UNDI, this is set to zero.

    public uint ImageSize;
    // Size of the UNrelocated driver/ROM image of this network interface.
    // This is set to zero if there is no driver/ROM image.

    public fixed byte StringId[4];
    // 4 char ASCII string to go in class identifier (option 60) in DHCP
    // and Boot Server discover packets.
    // For EfiNetworkInterfaceUndi this field is "UNDI".
    // For EfiNetworkInterfaceSnp this field is "SNPN".

    public byte Type;
    public byte MajorVer;
    public byte MinorVer;
    // Information to be placed into the PXE DHCP and Discover packets.
    // This is the network interface type and version number that will
    // be placed into DHCP option 94 (client network interface identifier).
    public bool Ipv6Supported;
    public byte IfNum;	// interface number to be used with pxeid ure
}

