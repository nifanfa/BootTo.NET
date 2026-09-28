#ifndef RE3GENERIC_RENDERER_H
#define RE3GENERIC_RENDERER_H

#include "re3generic.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct RG_SoftwareRenderer RG_SoftwareRenderer;

RG_SoftwareRenderer *rg_renderer_create(uint32_t width, uint32_t height);
void rg_renderer_destroy(RG_SoftwareRenderer *renderer);
void rg_renderer_present(const RG_SoftwareRenderer *renderer);
const void *rg_renderer_pixels(const RG_SoftwareRenderer *renderer);
uint32_t rg_renderer_pitch(const RG_SoftwareRenderer *renderer);

#ifdef __cplusplus
}
#endif

#endif
