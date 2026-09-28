#include "../include/re3generic.h"

extern uint64_t RG_BootTicks(void *userdata);
extern void RG_BootPresent(void *userdata, const void *pixels, uint32_t width,
                           uint32_t height, uint32_t pitch, RG_PixelFormat format);
extern int RG_BootPollInput(void *userdata, RG_InputEvent *event);
extern void *RG_BootFileOpen(void *userdata, const char *path);
extern uint64_t RG_BootFileSize(void *userdata, void *file);
extern size_t RG_BootFileReadAt(void *userdata, void *file, uint64_t offset,
                                void *buffer, size_t bytes);
extern void RG_BootFileClose(void *userdata, void *file);
extern void RG_BootAudioSubmit(void *userdata, const int16_t *samples,
                               uint32_t frames, uint32_t sample_rate);

int RG_BootBind(void)
{
    RG_Port port = { 0 };
    port.ticks_ms = RG_BootTicks;
    port.present = RG_BootPresent;
    port.poll_input = RG_BootPollInput;
    port.file_open = RG_BootFileOpen;
    port.file_size = RG_BootFileSize;
    port.file_read_at = RG_BootFileReadAt;
    port.file_close = RG_BootFileClose;
    port.submit_pcm = RG_BootAudioSubmit;
    return rg_bind_port(&port);
}
