using System.Runtime.InteropServices;

namespace System.Reflection.PortableExecutable
{
    // Native layouts used while walking the image loaded by the UEFI loader.
    // Public reader types expose managed values instead of pointer-oriented structs.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDataDirectory
    {
        public uint VirtualAddress;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeDosHeader
    {
        public ushort e_magic;
        public ushort e_cblp;
        public ushort e_cp;
        public ushort e_crlc;
        public ushort e_cparhdr;
        public ushort e_minalloc;
        public ushort e_maxalloc;
        public ushort e_ss;
        public ushort e_sp;
        public ushort e_csum;
        public ushort e_ip;
        public ushort e_cs;
        public ushort e_lfarlc;
        public ushort e_ovno;
        public fixed ushort e_res1[4];
        public ushort e_oemid;
        public ushort e_oeminfo;
        public fixed ushort e_res2[10];
        public int e_lfanew;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeFileHeader
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct NativeNtHeaders64
    {
        public uint Signature;
        public NativeFileHeader FileHeader;
        public NativeOptionalHeaders64 OptionalHeader;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct NativeOptionalHeaders64
    {
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public ulong ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public ulong SizeOfStackReserve;
        public ulong SizeOfStackCommit;
        public ulong SizeOfHeapReserve;
        public ulong SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public NativeDataDirectory ExportTable;
        public NativeDataDirectory ImportTable;
        public NativeDataDirectory ResourceTable;
        public NativeDataDirectory ExceptionTable;
        public NativeDataDirectory CertificateTable;
        public NativeDataDirectory BaseRelocationTable;
        public NativeDataDirectory Debug;
        public NativeDataDirectory Architecture;
        public NativeDataDirectory GlobalPtr;
        public NativeDataDirectory TLSTable;
        public NativeDataDirectory LoadConfigTable;
        public NativeDataDirectory BoundImport;
        public NativeDataDirectory IAT;
        public NativeDataDirectory DelayImportDescriptor;
        public NativeDataDirectory CLRRuntimeHeader;
        public NativeDataDirectory Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeSectionHeader
    {
        public fixed byte Name[8];
        public uint PhysicalAddress_VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLineNumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLineNumbers;
        public uint Characteristics;
    }
}
