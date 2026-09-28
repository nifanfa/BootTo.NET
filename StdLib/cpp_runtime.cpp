#include <new>
#include <cstdlib>

namespace std {
const nothrow_t nothrow{};
}

void* __cdecl operator new(size_t size)
{
    void* allocation = malloc(size ? size : 1);
    if (!allocation)
        abort();
    return allocation;
}

void* __cdecl operator new[](size_t size)
{
    return operator new(size);
}

void* __cdecl operator new(size_t size, const std::nothrow_t&) noexcept
{
    return malloc(size ? size : 1);
}

void* __cdecl operator new[](size_t size, const std::nothrow_t&) noexcept
{
    return malloc(size ? size : 1);
}

void __cdecl operator delete(void* pointer) noexcept
{
    free(pointer);
}

void __cdecl operator delete[](void* pointer) noexcept
{
    free(pointer);
}

void __cdecl operator delete(void* pointer, size_t) noexcept
{
    free(pointer);
}

void __cdecl operator delete[](void* pointer, size_t) noexcept
{
    free(pointer);
}

void __cdecl operator delete(void* pointer, const std::nothrow_t&) noexcept
{
    free(pointer);
}

void __cdecl operator delete[](void* pointer, const std::nothrow_t&) noexcept
{
    free(pointer);
}

extern "C" int __cdecl _purecall(void)
{
    abort();
}

extern "C" void __cdecl _wassert(const wchar_t*, const wchar_t*, unsigned)
{
    abort();
}
