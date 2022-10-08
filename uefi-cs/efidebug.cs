/*++

Copyright (c) 1998  Intel Corporation

Module Name:

    efidebug.h

Abstract:

    EFI library debug functions



Revision History

--*/

using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_EXCEPTION_TYPE
{
    public long Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FX_SAVE_STATE_IA32
{
    public ushort Fcw;
    public ushort Fsw;
    public ushort Ftw;
    public ushort Opcode;
    public uint Eip;
    public ushort Cs;
    public ushort Reserved1;
    public uint DataOffset;
    public ushort Ds;
    public fixed byte Reserved2[10];
    public fixed byte St0Mm0[10], Reserved3[6];
    public fixed byte St1Mm1[10], Reserved4[6];
    public fixed byte St2Mm2[10], Reserved5[6];
    public fixed byte St3Mm3[10], Reserved6[6];
    public fixed byte St4Mm4[10], Reserved7[6];
    public fixed byte St5Mm5[10], Reserved8[6];
    public fixed byte St6Mm6[10], Reserved9[6];
    public fixed byte St7Mm7[10], Reserved10[6];
    public fixed byte Xmm0[16];
    public fixed byte Xmm1[16];
    public fixed byte Xmm2[16];
    public fixed byte Xmm3[16];
    public fixed byte Xmm4[16];
    public fixed byte Xmm5[16];
    public fixed byte Xmm6[16];
    public fixed byte Xmm7[16];
    public fixed byte Reserved11[14 * 16];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_CONTEXT_IA32
{
    public uint ExceptionData;
    public EFI_FX_SAVE_STATE_IA32 FxSaveState;
    public uint Dr0;
    public uint Dr1;
    public uint Dr2;
    public uint Dr3;
    public uint Dr6;
    public uint Dr7;
    public uint Cr0;
    public uint Cr1;
    public uint Cr2;
    public uint Cr3;
    public uint Cr4;
    public uint Eflags;
    public uint Ldtr;
    public uint Tr;
    public fixed uint Gdtr[2];
    public fixed uint Idtr[2];
    public uint Eip;
    public uint Gs;
    public uint Fs;
    public uint Es;
    public uint Ds;
    public uint Cs;
    public uint Ss;
    public uint Edi;
    public uint Esi;
    public uint Ebp;
    public uint Esp;
    public uint Ebx;
    public uint Edx;
    public uint Ecx;
    public uint Eax;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FX_SAVE_STATE_X64
{
    public ushort Fcw;
    public ushort Fsw;
    public ushort Ftw;
    public ushort Opcode;
    public ulong Rip;
    public ulong DataOffset;
    public fixed byte Reserved1[8];
    public fixed byte St0Mm0[10], Reserved2[6];
    public fixed byte St1Mm1[10], Reserved3[6];
    public fixed byte St2Mm2[10], Reserved4[6];
    public fixed byte St3Mm3[10], Reserved5[6];
    public fixed byte St4Mm4[10], Reserved6[6];
    public fixed byte St5Mm5[10], Reserved7[6];
    public fixed byte St6Mm6[10], Reserved8[6];
    public fixed byte St7Mm7[10], Reserved9[6];
    public fixed byte Xmm0[16];
    public fixed byte Xmm1[16];
    public fixed byte Xmm2[16];
    public fixed byte Xmm3[16];
    public fixed byte Xmm4[16];
    public fixed byte Xmm5[16];
    public fixed byte Xmm6[16];
    public fixed byte Xmm7[16];
    public fixed byte Reserved11[14 * 16];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_CONTEXT_X64
{
    public ulong ExceptionData;
    public EFI_FX_SAVE_STATE_X64 FxSaveState;
    public ulong Dr0;
    public ulong Dr1;
    public ulong Dr2;
    public ulong Dr3;
    public ulong Dr6;
    public ulong Dr7;
    public ulong Cr0;
    public ulong Cr1;
    public ulong Cr2;
    public ulong Cr3;
    public ulong Cr4;
    public ulong Cr8;
    public ulong Rflags;
    public ulong Ldtr;
    public ulong Tr;
    public fixed ulong Gdtr[2];
    public fixed ulong Idtr[2];
    public ulong Rip;
    public ulong Gs;
    public ulong Fs;
    public ulong Es;
    public ulong Ds;
    public ulong Cs;
    public ulong Ss;
    public ulong Rdi;
    public ulong Rsi;
    public ulong Rbp;
    public ulong Rsp;
    public ulong Rbx;
    public ulong Rdx;
    public ulong Rcx;
    public ulong Rax;
    public ulong R8;
    public ulong R9;
    public ulong R10;
    public ulong R11;
    public ulong R12;
    public ulong R13;
    public ulong R14;
    public ulong R15;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_CONTEXT_IPF
{
    public ulong Reserved;
    public ulong R1;
    public ulong R2;
    public ulong R3;
    public ulong R4;
    public ulong R5;
    public ulong R6;
    public ulong R7;
    public ulong R8;
    public ulong R9;
    public ulong R10;
    public ulong R11;
    public ulong R12;
    public ulong R13;
    public ulong R14;
    public ulong R15;
    public ulong R16;
    public ulong R17;
    public ulong R18;
    public ulong R19;
    public ulong R20;
    public ulong R21;
    public ulong R22;
    public ulong R23;
    public ulong R24;
    public ulong R25;
    public ulong R26;
    public ulong R27;
    public ulong R28;
    public ulong R29;
    public ulong R30;
    public ulong R31;
    public fixed ulong F2[2];
    public fixed ulong F3[2];
    public fixed ulong F4[2];
    public fixed ulong F5[2];
    public fixed ulong F6[2];
    public fixed ulong F7[2];
    public fixed ulong F8[2];
    public fixed ulong F9[2];
    public fixed ulong F10[2];
    public fixed ulong F11[2];
    public fixed ulong F12[2];
    public fixed ulong F13[2];
    public fixed ulong F14[2];
    public fixed ulong F15[2];
    public fixed ulong F16[2];
    public fixed ulong F17[2];
    public fixed ulong F18[2];
    public fixed ulong F19[2];
    public fixed ulong F20[2];
    public fixed ulong F21[2];
    public fixed ulong F22[2];
    public fixed ulong F23[2];
    public fixed ulong F24[2];
    public fixed ulong F25[2];
    public fixed ulong F26[2];
    public fixed ulong F27[2];
    public fixed ulong F28[2];
    public fixed ulong F29[2];
    public fixed ulong F30[2];
    public fixed ulong F31[2];
    public ulong Pr;
    public ulong B0;
    public ulong B1;
    public ulong B2;
    public ulong B3;
    public ulong B4;
    public ulong B5;
    public ulong B6;
    public ulong B7;
    public ulong ArRsc;
    public ulong ArBsp;
    public ulong ArBspstore;
    public ulong ArRnat;
    public ulong ArFcr;
    public ulong ArEflag;
    public ulong ArCsd;
    public ulong ArSsd;
    public ulong ArCflg;
    public ulong ArFsr;
    public ulong ArFir;
    public ulong ArFdr;
    public ulong ArCcv;
    public ulong ArUnat;
    public ulong ArFpsr;
    public ulong ArPfs;
    public ulong ArLc;
    public ulong ArEc;
    public ulong CrDcr;
    public ulong CrItm;
    public ulong CrIva;
    public ulong CrPta;
    public ulong CrIpsr;
    public ulong CrIsr;
    public ulong CrIip;
    public ulong CrIfa;
    public ulong CrItir;
    public ulong CrIipa;
    public ulong CrIfs;
    public ulong CrIim;
    public ulong CrIha;
    public ulong Dbr0;
    public ulong Dbr1;
    public ulong Dbr2;
    public ulong Dbr3;
    public ulong Dbr4;
    public ulong Dbr5;
    public ulong Dbr6;
    public ulong Dbr7;
    public ulong Ibr0;
    public ulong Ibr1;
    public ulong Ibr2;
    public ulong Ibr3;
    public ulong Ibr4;
    public ulong Ibr5;
    public ulong Ibr6;
    public ulong Ibr7;
    public ulong IntNat;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_CONTEXT_EBC
{
    public ulong R0;
    public ulong R1;
    public ulong R2;
    public ulong R3;
    public ulong R4;
    public ulong R5;
    public ulong R6;
    public ulong R7;
    public ulong Flags;
    public ulong ControlFlags;
    public ulong Ip;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_CONTEXT_ARM
{
    public uint R0;
    public uint R1;
    public uint R2;
    public uint R3;
    public uint R4;
    public uint R5;
    public uint R6;
    public uint R7;
    public uint R8;
    public uint R9;
    public uint R10;
    public uint R11;
    public uint R12;
    public uint SP;
    public uint LR;
    public uint PC;
    public uint CPSR;
    public uint DFSR;
    public uint DFAR;
    public uint IFSR;
    public uint IFAR;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EFI_SYSTEM_CONTEXT
{
    [FieldOffset(0)]
    public EFI_SYSTEM_CONTEXT_EBC  *SystemContextEbc;
    [FieldOffset(0)]
    public EFI_SYSTEM_CONTEXT_IA32 *SystemContextIa32;
    [FieldOffset(0)]
    public EFI_SYSTEM_CONTEXT_X64  *SystemContextX64;
    [FieldOffset(0)]
    public EFI_SYSTEM_CONTEXT_IPF  *SystemContextIpf;
    [FieldOffset(0)]
    public EFI_SYSTEM_CONTEXT_ARM  *SystemContextArm;
}

public enum EFI_INSTRUCTION_SET_ARCHITECTURE
{
    IsaIa32 = 0x014c,
    IsaX64 = 0x8664,
    IsaIpf = 0x0200,
    IsaEbc = 0x0EBC,
    IsaArm = 0x01C2,
    //	IsaArm64 = 0xAA64
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_TABLE_POINTER
{
    public ulong Signature;
    public EFI_PHYSICAL_ADDRESS EfiSystemTableBase;
    public uint Crc32;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEBUG_IMAGE_INFO_NORMAL
{
    public uint ImageInfoType;
    public EFI_LOADED_IMAGE_PROTOCOL* LoadedImageProtocolInstance;
    public EFI_HANDLE* ImageHandle;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EFI_DEBUG_IMAGE_INFO
{
    [FieldOffset(0)]
    public uint* ImageInfoType;
    [FieldOffset(0)]
    public EFI_DEBUG_IMAGE_INFO_NORMAL* NormalImage;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEBUG_IMAGE_INFO_TABLE_HEADER
{
    public volatile uint UpdateStatus;
    public uint TableSize;
    public EFI_DEBUG_IMAGE_INFO* EfiDebugImageInfoTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEBUG_SUPPORT_PROTOCOL
{
    public EFI_INSTRUCTION_SET_ARCHITECTURE Isa;
    public readonly delegate* unmanaged<EFI_DEBUG_SUPPORT_PROTOCOL*, ulong, EFI_STATUS> GetMaximumProcessorIndex;
    public readonly delegate* unmanaged<EFI_DEBUG_SUPPORT_PROTOCOL*, ulong, delegate* unmanaged<EFI_SYSTEM_CONTEXT, void>, EFI_STATUS> RegisterPeriodicCallback;
    public readonly delegate* unmanaged<EFI_DEBUG_SUPPORT_PROTOCOL*, ulong, delegate* unmanaged<EFI_EXCEPTION_TYPE, EFI_SYSTEM_CONTEXT, void>, EFI_STATUS> RegisterExceptionCallback;
    public readonly delegate* unmanaged<EFI_DEBUG_SUPPORT_PROTOCOL*, ulong, void*, ulong, EFI_STATUS> InvalidateInionCache;
}

