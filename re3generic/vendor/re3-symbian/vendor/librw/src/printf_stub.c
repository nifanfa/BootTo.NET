#ifndef _DEBUG
#include <stdio.h>

int printf(const char * __restrict s, ...) {
	return 0;
}

int puts(const char *s) {
	return 0;
}
#endif
