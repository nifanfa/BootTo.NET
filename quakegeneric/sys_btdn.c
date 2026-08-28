/* BootTo.NET system services for Quake Generic. */

#include "quakedef.h"
#include "quakegeneric.h"

qboolean isDedicated;

extern unsigned int BTDN_GetMilliseconds(void);

int Sys_FileOpenRead(char *path, int *handle)
{
    long length;
    int file = BTDN_FileOpen(path, "rb");
    if (file <= 0)
    {
        *handle = -1;
        return -1;
    }

    if (BTDN_FileSeek(file, 0, SEEK_END) < 0)
    {
        BTDN_FileClose(file);
        *handle = -1;
        return -1;
    }
    length = BTDN_FileTell(file);
    BTDN_FileSeek(file, 0, SEEK_SET);
    *handle = file;
    return length > INT_MAX ? INT_MAX : (int)length;
}

int Sys_FileOpenWrite(char *path)
{
    int handle = BTDN_FileOpen(path, "wb");
    if (handle <= 0)
        Sys_Error("Error opening %s", path);
    return handle <= 0 ? -1 : handle;
}

void Sys_FileClose(int handle)
{
    if (handle > 0)
        BTDN_FileClose(handle);
}

void Sys_FileSeek(int handle, int position)
{
    BTDN_FileSeek(handle, position, SEEK_SET);
}

int Sys_FileRead(int handle, void *destination, int count)
{
    return BTDN_FileRead(handle, destination, count);
}

int Sys_FileWrite(int handle, void *data, int count)
{
    return BTDN_FileWrite(handle, data, count);
}

int Sys_FileTime(char *path)
{
    return BTDN_FileExists(path) ? 1 : -1;
}

void Sys_mkdir(char *path)
{
    BTDN_FileCreateDirectory(path);
}

void Sys_MakeCodeWriteable(unsigned long start_address, unsigned long length)
{
    (void)start_address;
    (void)length;
}

void Sys_DebugLog(char *file, char *format, ...)
{
    char buffer[4096];
    int handle;
    int length;
    va_list arguments;

    va_start(arguments, format);
    length = vsnprintf_(buffer, sizeof(buffer), format, arguments);
    va_end(arguments);
    if (length < 0)
        return;
    if (length >= (int)sizeof(buffer))
        length = (int)sizeof(buffer) - 1;

    handle = BTDN_FileOpen(file, "ab");
    if (handle > 0)
    {
        BTDN_FileWrite(handle, buffer, length);
        BTDN_FileClose(handle);
    }
}

__declspec(noreturn) void Sys_Error(char *error, ...)
{
    va_list arguments;
    printf("Sys_Error: ");
    va_start(arguments, error);
    vprintf_(error, arguments);
    va_end(arguments);
    printf("\n");
    BTDN_ExitQuake(1);
}

void Sys_Printf(char *format, ...)
{
    va_list arguments;

    if (host_initialized)
        return;

    va_start(arguments, format);
    vprintf_(format, arguments);
    va_end(arguments);
}

void Sys_Quit(void)
{
    Host_Shutdown();
    BTDN_ExitQuake(0);
}

double Sys_FloatTime(void)
{
    return (double)BTDN_GetMilliseconds() / 1000.0;
}

char *Sys_ConsoleInput(void)
{
    return NULL;
}

void Sys_Sleep(void)
{
}

void Sys_SendKeyEvents(void)
{
}

void Sys_HighFPPrecision(void)
{
}

void Sys_LowFPPrecision(void)
{
}

void Sys_SetFPCW(void)
{
}
