namespace System.Reflection.PortableExecutable
{
    public enum Machine : ushort
    {
        Unknown = 0x0000,
        I386 = 0x014C,
        WceMipsV2 = 0x0169,
        Alpha = 0x0184,
        SH3 = 0x01A2,
        SH3Dsp = 0x01A3,
        SH3E = 0x01A4,
        SH4 = 0x01A6,
        SH5 = 0x01A8,
        Arm = 0x01C0,
        Thumb = 0x01C2,
        ArmThumb2 = 0x01C4,
        AM33 = 0x01D3,
        PowerPC = 0x01F0,
        PowerPCFP = 0x01F1,
        IA64 = 0x0200,
        MIPS16 = 0x0266,
        Alpha64 = 0x0284,
        MipsFpu = 0x0366,
        MipsFpu16 = 0x0466,
        Tricore = 0x0520,
        Ebc = 0x0EBC,
        Amd64 = 0x8664,
        M32R = 0x9041,
        Arm64 = 0xAA64,
        LoongArch32 = 0x6232,
        LoongArch64 = 0x6264,
    }
}
