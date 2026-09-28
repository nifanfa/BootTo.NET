#ifndef RE3GENERIC_STREAM_H
#define RE3GENERIC_STREAM_H

#include "re3generic.h"

#ifdef __cplusplus
extern "C" {
#endif

#define RG_MAX_IMAGES 8
#define RG_MAX_CHANNELS 5
#define RG_IMAGE_PATH_SIZE 260
#define RG_SECTOR_SIZE 2048

typedef enum RG_StreamStatus {
    RG_STREAM_ERROR = -2,
    RG_STREAM_INVALID = -1,
    RG_STREAM_NONE = 0
} RG_StreamStatus;

typedef struct RG_StreamImage {
    void *file;
    uint64_t size;
    char path[RG_IMAGE_PATH_SIZE];
} RG_StreamImage;

typedef struct RG_Stream {
    RG_Port port;
    RG_StreamImage images[RG_MAX_IMAGES];
    RG_StreamStatus channel_status[RG_MAX_CHANNELS];
    uint32_t last_position;
    uint32_t image_count;
    uint32_t channel_count;
} RG_Stream;

int rg_stream_init(RG_Stream *stream, const RG_Port *port, uint32_t channel_count);
void rg_stream_close(RG_Stream *stream);
void rg_stream_remove_images(RG_Stream *stream);
int rg_stream_add_image(RG_Stream *stream, const char *path);
const char *rg_stream_image_name(const RG_Stream *stream, uint32_t image_index);
uint64_t rg_stream_image_size(const RG_Stream *stream, uint32_t image_index);
RG_StreamStatus rg_stream_read(RG_Stream *stream, uint32_t channel,
                               void *buffer, uint32_t packed_sector,
                               uint32_t sector_count);
RG_StreamStatus rg_stream_status(const RG_Stream *stream, uint32_t channel);

#ifdef __cplusplus
}
#endif

#endif
