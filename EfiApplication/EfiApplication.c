typedef unsigned __int64 runtime_size_t;
typedef unsigned __int64 efi_status;
typedef unsigned int uint32_t;
typedef efi_status(__cdecl* allocate_pool_fn)(int memory_type, runtime_size_t size, void** buffer);
typedef efi_status(__cdecl* free_pool_fn)(void* buffer);

typedef struct efi_table_header
{
    runtime_size_t signature;
    uint32_t revision;
    uint32_t header_size;
    uint32_t crc32;
    uint32_t reserved;
} efi_table_header;

typedef struct efi_boot_services
{
    efi_table_header header;
    void* raise_tpl;
    void* restore_tpl;
    void* allocate_pages;
    void* free_pages;
    void* get_memory_map;
    allocate_pool_fn allocate_pool;
    free_pool_fn free_pool;
} efi_boot_services;

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
    efi_boot_services* boot_services;
} efi_system_table;

extern efi_status ManagedEfiMain(void* image_handle, efi_system_table* system_table);

static allocate_pool_fn allocate_pool;
static free_pool_fn free_pool;

efi_status EfiMain(void* image_handle, efi_system_table* system_table)
{
    if (system_table == 0 || system_table->boot_services == 0)
        return ~(efi_status)0;
    allocate_pool = system_table->boot_services->allocate_pool;
    free_pool = system_table->boot_services->free_pool;
    return ManagedEfiMain(image_handle, system_table);
}

void* malloc(runtime_size_t size)
{
    void* allocation = 0;
    if (size == 0)
        size = 1;
    if (allocate_pool == 0 || allocate_pool(2, size, &allocation) != 0)
        return 0;
    return allocation;
}

void* calloc(runtime_size_t count, runtime_size_t size)
{
    runtime_size_t total;
    unsigned char* allocation;
    runtime_size_t index;

    if (size != 0 && count > (~(runtime_size_t)0) / size)
        return 0;
    total = count * size;
    allocation = (unsigned char*)malloc(total);
    if (allocation == 0)
        return 0;
    for (index = 0; index < total; index++)
        allocation[index] = 0;
    return allocation;
}

void free(void* allocation)
{
    if (allocation != 0 && free_pool != 0)
        free_pool(allocation);
}

__declspec(noreturn) void abort(void)
{
    for (;;)
    {
    }
}
