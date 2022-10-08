
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_ARG_INFO
{
    public uint Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_INTERFACE
{
    ///
    /// Handle back to original image handle & image information.
    ///
    public EFI_HANDLE ImageHandle;
    public EFI_LOADED_IMAGE_PROTOCOL* Info;

    ///
    /// Parsed arg list converted more C-like format.
    ///
    public char** Argv;
    public ulong Argc;

    ///
    /// Storage for file redirection args after parsing.
    ///
    public char** RedirArgv;
    public ulong RedirArgc;

    ///
    /// A file style handle for console io.
    ///
    public EFI_FILE* StdIn;
    public EFI_FILE* StdOut;
    public EFI_FILE* StdErr;

    ///
    /// List of attributes for each argument.
    ///
    public EFI_SHELL_ARG_INFO* ArgInfo;

    ///
    /// Whether we are echoing.
    ///
    public bool EchoOn;
}

