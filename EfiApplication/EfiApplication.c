#include "../StdLib/runtime.h"

typedef unsigned __int64 runtime_size_t;
typedef unsigned __int64 efi_status;
typedef unsigned int uint32_t;

typedef struct efi_table_header
{
    runtime_size_t signature;
    uint32_t revision;
    uint32_t header_size;
    uint32_t crc32;
    uint32_t reserved;
} efi_table_header;

typedef struct efi_system_table
{
    efi_table_header header;
    void* firmware_vendor;
    uint32_t firmware_revision;
    uint32_t padding;
    void* console_in_handle;
    void* console_in;
    void* console_out_handle;
    void* console_out;
    void* standard_error_handle;
    void* standard_error;
    void* runtime_services;
    void* boot_services;
} efi_system_table;

extern efi_status managed_EfiMain(void* image_handle, efi_system_table* system_table);

typedef void (__cdecl* initializer)(void);
#pragma section(".CRT$XCA", read)
#pragma section(".CRT$XCZ", read)
__declspec(allocate(".CRT$XCA")) initializer constructor_start[] = { 0 };
__declspec(allocate(".CRT$XCZ")) initializer constructor_end[] = { 0 };

efi_status EfiMain(void* image_handle, efi_system_table* system_table)
{
    efi_status result;
    if (system_table == 0 || system_table->boot_services == 0)
        return ~(efi_status)0;
    stdlib_initialize(system_table->boot_services, system_table->runtime_services);
    for (initializer* current = constructor_start + 1; current < constructor_end; ++current)
        if (*current)
            (*current)();
    result = managed_EfiMain(image_handle, system_table);
    stdlib_shutdown();
    return result;
}
