#include <intrin.h>

void* memcpy(void* dest, const void* src, size_t n) {
	__movsb(dest, src, n);
	return dest;
}

void* memset(void* ptr, int value, size_t num) {
	__stosb(ptr, (unsigned char)value, num);
	return ptr;
}