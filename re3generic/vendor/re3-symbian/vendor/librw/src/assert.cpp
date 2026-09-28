#ifdef _DEBUG
#include <assert.h>
#include <stdlib.h>
#include <e32std.h>

void __assert(const char *func, const char *file, int line, const char *message) {
	User::Panic(_L("assert"), 0);
}
#endif
