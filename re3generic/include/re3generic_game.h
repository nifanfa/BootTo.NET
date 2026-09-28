#ifndef RE3GENERIC_GAME_H
#define RE3GENERIC_GAME_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

int rg_game_init(uint32_t width, uint32_t height);
int rg_game_step(void);
void rg_game_shutdown(void);

#ifdef __cplusplus
}
#endif

#endif
