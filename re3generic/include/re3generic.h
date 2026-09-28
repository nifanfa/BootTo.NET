#ifndef RE3GENERIC_H
#define RE3GENERIC_H

#include <stddef.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum RG_PixelFormat {
    RG_PIXEL_RGB565 = 1,
    RG_PIXEL_XRGB8888 = 2
} RG_PixelFormat;

typedef enum RG_InputType {
    RG_INPUT_KEY_DOWN = 1,
    RG_INPUT_KEY_UP,
    RG_INPUT_MOUSE_MOVE,
    RG_INPUT_MOUSE_BUTTON_DOWN,
    RG_INPUT_MOUSE_BUTTON_UP,
    RG_INPUT_PAD_BUTTON_DOWN,
    RG_INPUT_PAD_BUTTON_UP,
    RG_INPUT_PAD_AXIS,
    RG_INPUT_MOUSE_DELTA,
    RG_INPUT_MOUSE_WHEEL,
    RG_INPUT_MOUSE_RESET
} RG_InputType;

typedef enum RG_MouseButton {
    RG_MOUSE_LEFT = 0,
    RG_MOUSE_RIGHT = 1,
    RG_MOUSE_MIDDLE = 2,
    RG_MOUSE_X1 = 3,
    RG_MOUSE_X2 = 4
} RG_MouseButton;

typedef enum RG_KeyCode {
    RG_KEY_ESCAPE = 1000,
    RG_KEY_F1 = 1001,
    RG_KEY_F12 = 1012,
    RG_KEY_UP = 1019,
    RG_KEY_DOWN = 1020,
    RG_KEY_LEFT = 1021,
    RG_KEY_RIGHT = 1022,
    RG_KEY_BACKSPACE = 1042,
    RG_KEY_TAB = 1043,
    RG_KEY_ENTER = 1045,
    RG_KEY_LEFT_SHIFT = 1046,
    RG_KEY_RIGHT_SHIFT = 1047,
    RG_KEY_LEFT_CTRL = 1049,
    RG_KEY_RIGHT_CTRL = 1050,
    RG_KEY_LEFT_ALT = 1051,
    RG_KEY_RIGHT_ALT = 1052
} RG_KeyCode;

typedef struct RG_InputEvent {
    RG_InputType type;
    int32_t code;
    int32_t value;
    int32_t value_y;
} RG_InputEvent;

typedef struct RG_Port {
    void *userdata;
    uint64_t (*ticks_ms)(void *userdata);
    void (*present)(void *userdata, const void *pixels, uint32_t width,
                    uint32_t height, uint32_t pitch_bytes, RG_PixelFormat format);
    int (*poll_input)(void *userdata, RG_InputEvent *event);
    void *(*file_open)(void *userdata, const char *path);
    uint64_t (*file_size)(void *userdata, void *file);
    size_t (*file_read_at)(void *userdata, void *file, uint64_t offset,
                           void *buffer, size_t bytes);
    void (*file_close)(void *userdata, void *file);
    void (*submit_pcm)(void *userdata, const int16_t *interleaved_stereo,
                       uint32_t frames, uint32_t sample_rate);
} RG_Port;

int rg_bind_port(const RG_Port *port);
const RG_Port *rg_bound_port(void);

#ifdef __cplusplus
}
#endif

#endif
