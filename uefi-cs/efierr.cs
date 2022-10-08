/*++

Copyright (c) 1998  Intel Corporation

Module Name:

    efierr.h

Abstract:

    EFI error codes




Revision History

--*/

public partial class efi
{
    public const ulong EFI_SUCCESS = 0;
    public const ulong EFI_LOAD_ERROR = 0x8000000000000000 | 1;
    public const ulong EFI_INVALID_PARAMETER = 0x8000000000000000 | 2;
    public const ulong EFI_UNSUPPORTED = 0x8000000000000000 | 3;
    public const ulong EFI_BAD_BUFFER_SIZE = 0x8000000000000000 | 4;
    public const ulong EFI_BUFFER_TOO_SMALL = 0x8000000000000000 | 5;
    public const ulong EFI_NOT_READY = 0x8000000000000000 | 6;
    public const ulong EFI_DEVICE_ERROR = 0x8000000000000000 | 7;
    public const ulong EFI_WRITE_PROTECTED = 0x8000000000000000 | 8;
    public const ulong EFI_OUT_OF_RESOURCES = 0x8000000000000000 | 9;
    public const ulong EFI_VOLUME_CORRUPTED = 0x8000000000000000 | 10;
    public const ulong EFI_VOLUME_FULL = 0x8000000000000000 | 11;
    public const ulong EFI_NO_MEDIA = 0x8000000000000000 | 12;
    public const ulong EFI_MEDIA_CHANGED = 0x8000000000000000 | 13;
    public const ulong EFI_NOT_FOUND = 0x8000000000000000 | 14;
    public const ulong EFI_ACCESS_DENIED = 0x8000000000000000 | 15;
    public const ulong EFI_NO_RESPONSE = 0x8000000000000000 | 16;
    public const ulong EFI_NO_MAPPING = 0x8000000000000000 | 17;
    public const ulong EFI_TIMEOUT = 0x8000000000000000 | 18;
    public const ulong EFI_NOT_STARTED = 0x8000000000000000 | 19;
    public const ulong EFI_ALREADY_STARTED = 0x8000000000000000 | 20;
    public const ulong EFI_ABORTED = 0x8000000000000000 | 21;
    public const ulong EFI_ICMP_ERROR = 0x8000000000000000 | 22;
    public const ulong EFI_TFTP_ERROR = 0x8000000000000000 | 23;
    public const ulong EFI_PROTOCOL_ERROR = 0x8000000000000000 | 24;
    public const ulong EFI_INCOMPATIBLE_VERSION = 0x8000000000000000 | 25;
    public const ulong EFI_SECURITY_VIOLATION = 0x8000000000000000 | 26;
    public const ulong EFI_CRC_ERROR = 0x8000000000000000 | 27;
    public const ulong EFI_END_OF_MEDIA = 0x8000000000000000 | 28;
    public const ulong EFI_END_OF_FILE = 0x8000000000000000 | 31;
    public const ulong EFI_INVALID_LANGUAGE = 0x8000000000000000 | 32;
    public const ulong EFI_COMPROMISED_DATA = 0x8000000000000000 | 33;
}
