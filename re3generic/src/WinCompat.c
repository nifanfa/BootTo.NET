#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <time.h>
#include <string.h>
#include <stdint.h>

extern uint64_t RG_BootTicks(void* userdata);
extern int BTDN_FileRemove(const char* path);
extern int BTDN_FindFirst(const char* pattern, char* name, uint64_t* modified);
extern int BTDN_FindNext(int handle, char* name, uint64_t* modified);
extern void BTDN_FindClose(int handle);
extern void _putchar(char character);

static BOOL WINAPI port_counter(LARGE_INTEGER* counter)
{
    if (!counter) return FALSE;
    counter->QuadPart = (LONGLONG)RG_BootTicks(0) * 1000;
    return TRUE;
}

static BOOL WINAPI port_frequency(LARGE_INTEGER* frequency)
{
    if (!frequency) return FALSE;
    frequency->QuadPart = 1000000;
    return TRUE;
}

static void WINAPI port_debug(const char* message)
{
    if (message)
        while (*message)
            _putchar(*message++);
}

static void fill_find_data(WIN32_FIND_DATAA* data, const char* name, uint64_t modified)
{
    memset(data, 0, sizeof(*data));
    strncpy(data->cFileName, name, MAX_PATH - 1);
    data->ftLastWriteTime.dwLowDateTime = (DWORD)modified;
    data->ftLastWriteTime.dwHighDateTime = (DWORD)(modified >> 32);
}

static HANDLE WINAPI port_find_first(const char* pattern, WIN32_FIND_DATAA* data)
{
    char name[MAX_PATH];
    uint64_t modified;
    int handle;
    if (!data) return INVALID_HANDLE_VALUE;
    handle = BTDN_FindFirst(pattern, name, &modified);
    if (!handle) return INVALID_HANDLE_VALUE;
    fill_find_data(data, name, modified);
    return (HANDLE)(uintptr_t)handle;
}

static BOOL WINAPI port_find_next(HANDLE handle, WIN32_FIND_DATAA* data)
{
    char name[MAX_PATH];
    uint64_t modified;
    if (!data || !BTDN_FindNext((int)(uintptr_t)handle, name, &modified)) return FALSE;
    fill_find_data(data, name, modified);
    return TRUE;
}

static BOOL WINAPI port_find_close(HANDLE handle)
{
    if (handle == INVALID_HANDLE_VALUE) return FALSE;
    BTDN_FindClose((int)(uintptr_t)handle);
    return TRUE;
}

static BOOL WINAPI port_filetime(const FILETIME* filetime, SYSTEMTIME* output)
{
    uint64_t ticks;
    __time64_t epoch;
    struct tm* calendar;
    if (!filetime || !output) return FALSE;
    ticks = ((uint64_t)filetime->dwHighDateTime << 32) | filetime->dwLowDateTime;
    if (ticks < 116444736000000000ULL) return FALSE;
    epoch = (__time64_t)((ticks - 116444736000000000ULL) / 10000000ULL);
    calendar = _localtime64(&epoch);
    if (!calendar) return FALSE;
    memset(output, 0, sizeof(*output));
    output->wYear = (WORD)(calendar->tm_year + 1900);
    output->wMonth = (WORD)(calendar->tm_mon + 1);
    output->wDay = (WORD)calendar->tm_mday;
    output->wDayOfWeek = (WORD)calendar->tm_wday;
    output->wHour = (WORD)calendar->tm_hour;
    output->wMinute = (WORD)calendar->tm_min;
    output->wSecond = (WORD)calendar->tm_sec;
    return TRUE;
}

static int WINAPI port_date(LCID locale, DWORD flags, const SYSTEMTIME* date,
                            const char* format, char* buffer, int capacity)
{
    char text[32];
    int length;
    (void)locale; (void)flags; (void)format;
    if (!date) return 0;
    length = snprintf(text, sizeof(text), "%04u-%02u-%02u", date->wYear, date->wMonth, date->wDay) + 1;
    if (!buffer) return length;
    if (capacity < length) return 0;
    memcpy(buffer, text, length);
    return length;
}

static void WINAPI port_local_time(SYSTEMTIME* output)
{
    __time64_t epoch = _time64(0);
    struct tm* calendar = _localtime64(&epoch);
    if (!output) return;
    memset(output, 0, sizeof(*output));
    if (calendar) {
        output->wYear = (WORD)(calendar->tm_year + 1900);
        output->wMonth = (WORD)(calendar->tm_mon + 1);
        output->wDay = (WORD)calendar->tm_mday;
        output->wDayOfWeek = (WORD)calendar->tm_wday;
        output->wHour = (WORD)calendar->tm_hour;
        output->wMinute = (WORD)calendar->tm_min;
        output->wSecond = (WORD)calendar->tm_sec;
    }
}

static BOOL WINAPI port_delete(const char* path)
{
    return BTDN_FileRemove(path) == 0;
}

void* __imp_QueryPerformanceCounter = port_counter;
void* __imp_QueryPerformanceFrequency = port_frequency;
void* __imp_OutputDebugStringA = port_debug;
void* __imp_FindFirstFileA = port_find_first;
void* __imp_FindNextFileA = port_find_next;
void* __imp_FindClose = port_find_close;
void* __imp_FileTimeToSystemTime = port_filetime;
void* __imp_GetDateFormatA = port_date;
void* __imp_GetLocalTime = port_local_time;
void* __imp_DeleteFileA = port_delete;
