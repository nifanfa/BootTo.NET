#include "re3generic_stream.h"

#include <string.h>

int rg_stream_init(RG_Stream *stream, const RG_Port *port, uint32_t channel_count)
{
    if (stream == NULL || port == NULL || port->file_open == NULL ||
        port->file_size == NULL || port->file_read_at == NULL ||
        port->file_close == NULL || channel_count == 0 ||
        channel_count > RG_MAX_CHANNELS)
        return 0;

    memset(stream, 0, sizeof(*stream));
    stream->port = *port;
    stream->channel_count = channel_count;
    return 1;
}

void rg_stream_remove_images(RG_Stream *stream)
{
    uint32_t image_index;

    if (stream == NULL || stream->port.file_close == NULL)
        return;

    for (image_index = 0; image_index < stream->image_count; ++image_index) {
        stream->port.file_close(stream->port.userdata, stream->images[image_index].file);
        memset(&stream->images[image_index], 0, sizeof(stream->images[image_index]));
    }
    stream->image_count = 0;
    memset(stream->channel_status, 0, sizeof(stream->channel_status));
}

void rg_stream_close(RG_Stream *stream)
{
    if (stream == NULL)
        return;

    rg_stream_remove_images(stream);
    memset(stream, 0, sizeof(*stream));
}

int rg_stream_add_image(RG_Stream *stream, const char *path)
{
    size_t path_length;
    RG_StreamImage *image;
    void *file;

    if (stream == NULL || stream->port.file_open == NULL || path == NULL ||
        stream->image_count >= RG_MAX_IMAGES)
        return 0;

    path_length = strlen(path);
    if (path_length == 0 || path_length >= RG_IMAGE_PATH_SIZE)
        return 0;

    file = stream->port.file_open(stream->port.userdata, path);
    if (file == NULL)
        return 0;

    image = &stream->images[stream->image_count];
    image->file = file;
    image->size = stream->port.file_size(stream->port.userdata, file);
    memcpy(image->path, path, path_length + 1);
    ++stream->image_count;
    return 1;
}

const char *rg_stream_image_name(const RG_Stream *stream, uint32_t image_index)
{
    if (stream == NULL || image_index >= stream->image_count)
        return NULL;
    return stream->images[image_index].path;
}

uint64_t rg_stream_image_size(const RG_Stream *stream, uint32_t image_index)
{
    if (stream == NULL || image_index >= stream->image_count)
        return 0;
    return stream->images[image_index].size;
}

RG_StreamStatus rg_stream_read(RG_Stream *stream, uint32_t channel,
                               void *buffer, uint32_t packed_sector,
                               uint32_t sector_count)
{
    uint32_t image_index;
    uint64_t offset_bytes;
    uint64_t read_bytes;
    RG_StreamImage *image;

    if (stream == NULL || channel >= stream->channel_count || buffer == NULL)
        return RG_STREAM_INVALID;

    image_index = packed_sector >> 24;
    if (image_index >= stream->image_count || sector_count == 0 ||
        sector_count > SIZE_MAX / RG_SECTOR_SIZE) {
        stream->channel_status[channel] = RG_STREAM_INVALID;
        return RG_STREAM_INVALID;
    }

    stream->last_position = packed_sector + sector_count;
    image = &stream->images[image_index];
    offset_bytes = (uint64_t)(packed_sector & 0xFFFFFFu) * RG_SECTOR_SIZE;
    read_bytes = (uint64_t)sector_count * RG_SECTOR_SIZE;

    if (offset_bytes > image->size || read_bytes > image->size - offset_bytes ||
        stream->port.file_read_at(stream->port.userdata, image->file,
                                  offset_bytes, buffer, (size_t)read_bytes) !=
            (size_t)read_bytes) {
        stream->channel_status[channel] = RG_STREAM_ERROR;
        return RG_STREAM_ERROR;
    }

    stream->channel_status[channel] = RG_STREAM_NONE;
    return RG_STREAM_NONE;
}

RG_StreamStatus rg_stream_status(const RG_Stream *stream, uint32_t channel)
{
    if (stream == NULL || channel >= stream->channel_count)
        return RG_STREAM_INVALID;
    return stream->channel_status[channel];
}
