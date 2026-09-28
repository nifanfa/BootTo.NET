#include "common.h"
#include "CdStream.h"
#include "re3generic_stream.h"

#include <assert.h>
#include <stdint.h>
#include <stdlib.h>

static_assert(MAX_CDIMAGES == RG_MAX_IMAGES, "image limit must match re3");
static_assert(MAX_CDCHANNELS == RG_MAX_CHANNELS, "channel limit must match re3");

static RG_Stream stream;

#ifdef FLUSHABLE_STREAMING
bool flushStream[MAX_CDCHANNELS];
#endif

void CdStreamInitThread(void)
{
}

void CdStreamInit(int32 numChannels)
{
    const RG_Port *port = rg_bound_port();
    assert(port != NULL);
    assert(numChannels > 0 && numChannels <= RG_MAX_CHANNELS);
    if (port == NULL || numChannels <= 0 || numChannels > RG_MAX_CHANNELS ||
        !rg_stream_init(&stream, port, (uint32_t)numChannels))
        abort();
}

void CdStreamShutdown(void)
{
    rg_stream_close(&stream);
}

uint32 GetGTA3ImgSize(void)
{
    uint64_t size = rg_stream_image_size(&stream, 0);
    assert(size <= UINT32_MAX);
    return (uint32)size;
}

int32 CdStreamRead(int32 channel, void *buffer, uint32 offset, uint32 size)
{
    if (channel < 0 ||
        rg_stream_read(&stream, (uint32_t)channel, buffer, offset, size) != RG_STREAM_NONE)
        return STREAM_ERROR;
    return STREAM_SUCCESS;
}

int32 CdStreamGetStatus(int32 channel)
{
    if (channel < 0)
        return STREAM_ERROR;
    return rg_stream_status(&stream, (uint32_t)channel) == RG_STREAM_NONE
        ? STREAM_NONE : STREAM_ERROR;
}

int32 CdStreamSync(int32 channel)
{
    return CdStreamGetStatus(channel);
}

int32 CdStreamGetLastPosn(void)
{
    return (int32)stream.last_position;
}

bool CdStreamAddImage(char const *path)
{
    return rg_stream_add_image(&stream, path) != 0;
}

char *CdStreamGetImageName(int32 cd)
{
    if (cd < 0)
        return nil;
    return const_cast<char *>(rg_stream_image_name(&stream, (uint32_t)cd));
}

void CdStreamRemoveImages(void)
{
    rg_stream_remove_images(&stream);
}

int32 CdStreamGetNumImages(void)
{
    return (int32)stream.image_count;
}
