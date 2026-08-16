
using System.Runtime.InteropServices;

public enum SHELL_STATUS
{
    SHELL_SUCCESS = 0,
    SHELL_LOAD_ERROR = 1,
    SHELL_INVALID_PARAMETER = 2,
    SHELL_UNSUPPORTED = 3,
    SHELL_BAD_BUFFER_SIZE = 4,
    SHELL_BUFFER_TOO_SMALL = 5,
    SHELL_NOT_READY = 6,
    SHELL_DEVICE_ERROR = 7,
    SHELL_WRITE_PROTECTED = 8,
    SHELL_OUT_OF_RESOURCES = 9,
    SHELL_VOLUME_CORRUPTED = 10,
    SHELL_VOLUME_FULL = 11,
    SHELL_NO_MEDIA = 12,
    SHELL_MEDIA_CHANGED = 13,
    SHELL_NOT_FOUND = 14,
    SHELL_ACCESS_DENIED = 15,
    SHELL_TIMEOUT = 18,
    SHELL_NOT_STARTED = 19,
    SHELL_ALREADY_STARTED = 20,
    SHELL_ABORTED = 21,
    SHELL_INCOMPATIBLE_VERSION = 25,
    SHELL_SECURITY_VIOLATION = 26,
    SHELL_NOT_EQUAL = 27
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SHELL_FILE_HANDLE
{
    void* Value;

    public static implicit operator SHELL_FILE_HANDLE(void* value) => new SHELL_FILE_HANDLE() { Value = value };
    public static implicit operator void*(SHELL_FILE_HANDLE value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_FILE_INFO
{
    public EFI_LIST_ENTRY Link;
    public EFI_STATUS Status;
    public char* FullName;
    public char* FileName;
    public SHELL_FILE_HANDLE Handle;
    public EFI_FILE_INFO* Info;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_DEVICE_NAME_FLAGS
{
    uint Value;

    public static implicit operator EFI_SHELL_DEVICE_NAME_FLAGS(uint value) => new EFI_SHELL_DEVICE_NAME_FLAGS() { Value = value };
    public static implicit operator uint(EFI_SHELL_DEVICE_NAME_FLAGS value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_HANDLE*, char*, char**, EFI_STATUS*, EFI_STATUS> Execute;
    public readonly delegate* unmanaged<char*, char*> GetEnv;
    public readonly delegate* unmanaged<char*, char*, bool, EFI_STATUS> SetEnv;
    public readonly delegate* unmanaged<char*, bool*, char*> GetAlias;
    public readonly delegate* unmanaged<char*, char*, bool, bool, EFI_STATUS> SetAlias;
    public readonly delegate* unmanaged<char*, char*, char**, EFI_STATUS> GetHelpText;
    public readonly delegate* unmanaged<char*, EFI_DEVICE_PATH_PROTOCOL*> GetDevicePathFromMap;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL**, char*> GetMapFromDevicePath;
    public readonly delegate* unmanaged<char*, EFI_DEVICE_PATH_PROTOCOL*> GetDevicePathFromFilePath;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, char*> GetFilePathFromDevicePath;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, char*, EFI_STATUS> SetMap;
    public readonly delegate* unmanaged<char*, char*> GetCurDir;
    public readonly delegate* unmanaged<char*, char*, EFI_STATUS> SetCurDir;
    public readonly delegate* unmanaged<char*, ulong, EFI_SHELL_FILE_INFO**, EFI_STATUS> OpenFileList;
    public readonly delegate* unmanaged<EFI_SHELL_FILE_INFO**, EFI_STATUS> FreeFileList;
    public readonly delegate* unmanaged<EFI_SHELL_FILE_INFO**, EFI_STATUS> RemoveDupInFileList;
    public readonly delegate* unmanaged<bool> BatchIsActive;
    public readonly delegate* unmanaged<bool> IsRootShell;
    public readonly delegate* unmanaged<void> EnablePageBreak;
    public readonly delegate* unmanaged<void> DisablePageBreak;
    public readonly delegate* unmanaged<bool> GetPageBreak;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_SHELL_DEVICE_NAME_FLAGS, byte*, char**, EFI_STATUS> GetDeviceName;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, EFI_FILE_INFO*> GetFileInfo;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, EFI_FILE_INFO*, EFI_STATUS> SetFileInfo;
    public readonly delegate* unmanaged<char*, SHELL_FILE_HANDLE*, ulong, EFI_STATUS> OpenFileByName;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, EFI_STATUS> CloseFile;
    public readonly delegate* unmanaged<char*, ulong, SHELL_FILE_HANDLE*, EFI_STATUS> CreateFile;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, ulong*, void*, EFI_STATUS> ReadFile;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, ulong*, void*, EFI_STATUS> WriteFile;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, EFI_STATUS> DeleteFile;
    public readonly delegate* unmanaged<char*, EFI_STATUS> DeleteFileByName;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, ulong*, EFI_STATUS> GetFilePosition;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, ulong, EFI_STATUS> SetFilePosition;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, EFI_STATUS> FlushFile;
    public readonly delegate* unmanaged<char*, EFI_SHELL_FILE_INFO**, EFI_STATUS> FindFiles;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, EFI_SHELL_FILE_INFO**, EFI_STATUS> FindFilesInDir;
    public readonly delegate* unmanaged<SHELL_FILE_HANDLE, ulong*, EFI_STATUS> GetFileSize;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, SHELL_FILE_HANDLE*, EFI_STATUS> OpenRoot;
    public readonly delegate* unmanaged<EFI_HANDLE, SHELL_FILE_HANDLE*, EFI_STATUS> OpenRootByHandle;
    public EFI_EVENT ExecutionBreak;
    public uint MajorVersion;
    public uint MinorVersion;
    // Added for Shell 2.1
    public readonly delegate* unmanaged<EFI_GUID*, char*, EFI_STATUS> RegisterGuidName;
    public readonly delegate* unmanaged<EFI_GUID*, char**, EFI_STATUS> GetGuidName;
    public readonly delegate* unmanaged<char*, EFI_GUID*, EFI_STATUS> GetGuidFromName;
    public readonly delegate* unmanaged<char*, uint*, char*> GetEnvEx;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_PARAMETERS_PROTOCOL
{
    public char** Argv;
    public ulong Argc;
    public SHELL_FILE_HANDLE StdIn;
    public SHELL_FILE_HANDLE StdOut;
    public SHELL_FILE_HANDLE StdErr;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SHELL_DYNAMIC_COMMAND_PROTOCOL
{
    public char* CommandName;
    public readonly delegate* unmanaged<EFI_SHELL_DYNAMIC_COMMAND_PROTOCOL*, EFI_SYSTEM_TABLE*, EFI_SHELL_PARAMETERS_PROTOCOL*, EFI_SHELL_PROTOCOL*, SHELL_STATUS> Handler;
    public readonly delegate* unmanaged<EFI_SHELL_DYNAMIC_COMMAND_PROTOCOL*, byte*, char*> GetHelp;
}

