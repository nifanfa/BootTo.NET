#include <stddef.h>
#include <stdint.h>
#include <stdlib.h>
#include "runtime.h"

typedef uint64_t(__cdecl* allocate_pool_fn)(int memory_type, size_t size, void** buffer);
typedef uint64_t(__cdecl* free_pool_fn)(void* buffer);

typedef struct boot_services_prefix {
    uint64_t header[3];
    void* raise_tpl;
    void* restore_tpl;
    void* allocate_pages;
    void* free_pages;
    void* get_memory_map;
    allocate_pool_fn allocate_pool;
    free_pool_fn free_pool;
} boot_services_prefix;

typedef struct allocation_header {
    size_t size;
    size_t reserved;
} allocation_header;

static allocate_pool_fn allocate_pool;
static free_pool_fn free_pool;
static void (__cdecl* exit_callbacks[512])(void);
static size_t exit_callback_count;
extern void stdlib_time_initialize(void* runtime_services);

void stdlib_initialize(void* boot_services, void* runtime_services)
{
    boot_services_prefix* services = (boot_services_prefix*)boot_services;
    allocate_pool = services->allocate_pool;
    free_pool = services->free_pool;
    stdlib_time_initialize(runtime_services);
}

int __cdecl atexit(void (__cdecl* callback)(void))
{
    if (callback == 0 || exit_callback_count == sizeof(exit_callbacks) / sizeof(exit_callbacks[0]))
        return -1;
    exit_callbacks[exit_callback_count++] = callback;
    return 0;
}

void stdlib_shutdown(void)
{
    while (exit_callback_count != 0)
        exit_callbacks[--exit_callback_count]();
}

void* __cdecl malloc(size_t size)
{
    allocation_header* header = 0;
    if (size > SIZE_MAX - sizeof(*header) || allocate_pool == 0 ||
        allocate_pool(2, sizeof(*header) + (size ? size : 1), (void**)&header) != 0)
        return 0;
    header->size = size;
    return header + 1;
}

void __cdecl free(void* pointer)
{
    if (pointer != 0 && free_pool != 0)
        free_pool((allocation_header*)pointer - 1);
}

void* __cdecl calloc(size_t count, size_t size)
{
    size_t index;
    size_t total;
    unsigned char* result;
    if (size != 0 && count > SIZE_MAX / size)
        return 0;
    total = count * size;
    result = (unsigned char*)malloc(total);
    if (result != 0)
        for (index = 0; index < total; ++index)
            result[index] = 0;
    return result;
}

void* __cdecl realloc(void* pointer, size_t size)
{
    size_t index;
    size_t old_size;
    unsigned char* result;
    unsigned char* old = (unsigned char*)pointer;
    if (pointer == 0)
        return malloc(size);
    if (size == 0) {
        free(pointer);
        return 0;
    }
    old_size = ((allocation_header*)pointer - 1)->size;
    result = (unsigned char*)malloc(size);
    if (result == 0)
        return 0;
    for (index = 0; index < (size < old_size ? size : old_size); ++index)
        result[index] = old[index];
    free(pointer);
    return result;
}

__declspec(noreturn) void __cdecl abort(void)
{
    for (;;)
        ;
}

__declspec(noreturn) void __cdecl exit(int status)
{
    (void)status;
    stdlib_shutdown();
    abort();
}

double __cdecl strtod(const char* text, char** end)
{
    const char* cursor = text;
    double result = 0.0;
    int sign = 1;
    int digits = 0;
    int exponent = 0;
    int exponent_sign = 1;
    while (*cursor == ' ' || (*cursor >= '\t' && *cursor <= '\r'))
        ++cursor;
    if (*cursor == '-' || *cursor == '+') {
        if (*cursor == '-')
            sign = -1;
        ++cursor;
    }
    while (*cursor >= '0' && *cursor <= '9') {
        result = result * 10.0 + (*cursor++ - '0');
        ++digits;
    }
    if (*cursor == '.') {
        double fraction = 0.1;
        ++cursor;
        while (*cursor >= '0' && *cursor <= '9') {
            result += (*cursor++ - '0') * fraction;
            fraction *= 0.1;
            ++digits;
        }
    }
    if (!digits) {
        if (end)
            *end = (char*)text;
        return 0.0;
    }
    if (*cursor == 'e' || *cursor == 'E') {
        const char* exponent_start = cursor++;
        if (*cursor == '-' || *cursor == '+') {
            if (*cursor == '-')
                exponent_sign = -1;
            ++cursor;
        }
        if (*cursor < '0' || *cursor > '9')
            cursor = exponent_start;
        else {
            while (*cursor >= '0' && *cursor <= '9') {
                if (exponent < 400)
                    exponent = exponent * 10 + (*cursor - '0');
                ++cursor;
            }
        }
    }
    if (exponent > 308)
        exponent = 308;
    while (exponent-- > 0)
        result = exponent_sign < 0 ? result / 10.0 : result * 10.0;
    if (end)
        *end = (char*)cursor;
    return sign * result;
}

double __cdecl atof(const char* text)
{
    return strtod(text, 0);
}

int __cdecl atoi(const char* text)
{
    return (int)strtod(text, 0);
}
