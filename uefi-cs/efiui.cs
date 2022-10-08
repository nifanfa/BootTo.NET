/*++

Copyright (c) 200  Intel Corporation

Module Name:

    EfiUi.h

Abstract:
    Protocol used to build User Interface (UI) stuff.

    This protocol is just data. It is a multi dimentional array.
    For each string there is an array of UI_STRING_ENTRY. Each string
    is for a different language translation of the same string. The list
    is terminated by a NULL UiString. There can be any number of
    UI_STRING_ENTRY arrays. A NULL array terminates the list. A NULL array
    entry contains all zeros.

    Thus the shortest possible EFI_UI_PROTOCOL has three UI_STRING_ENTRY.
    The String, it's NULL terminator, and the NULL terminator for the entire
    thing.


Revision History

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct UI_STRING_ENTRY
{
    public ISO_639_2* LangCode;
    public char* UiString;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UI_INTERFACE_PROTOCOL
{
    public uint Version;
    public UI_STRING_ENTRY* Entry;
}

