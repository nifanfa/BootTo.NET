#ifndef STANDARDLIB_RUNTIME_H
#define STANDARDLIB_RUNTIME_H

#ifdef __cplusplus
extern "C" {
#endif

void stdlib_initialize(void* boot_services, void* runtime_services);
void stdlib_shutdown(void);

#ifdef __cplusplus
}
#endif

#endif
