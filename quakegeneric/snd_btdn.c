/* BootTo.NET PCM output backend for Quake's portable sound mixer. */

#include "quakedef.h"

#define BTDN_AUDIO_RATE 11025
#define BTDN_AUDIO_CHANNELS 2
#define BTDN_AUDIO_FRAMES 16384
#define BTDN_AUDIO_SAMPLES (BTDN_AUDIO_FRAMES * BTDN_AUDIO_CHANNELS)
#define BTDN_AUDIO_BYTES (BTDN_AUDIO_SAMPLES * sizeof(short))
#define BTDN_AUDIO_SUBMIT_FRAMES 2048

static unsigned char audio_buffer[BTDN_AUDIO_BYTES];
static uint64_t submitted_frames;
static uint64_t clock_frame;
static uint32_t clock_milliseconds;
static int clock_started;

extern int QG_AudioWrite(const short *samples, int frame_count);
extern uint32_t BTDN_GetMilliseconds(void);

qboolean SNDDMA_Init(void)
{
    memset((void *)&sn, 0, sizeof(sn));
    memset(audio_buffer, 0, sizeof(audio_buffer));

    shm = &sn;
    shm->channels = BTDN_AUDIO_CHANNELS;
    shm->samplebits = 16;
    shm->speed = BTDN_AUDIO_RATE;
    shm->samples = BTDN_AUDIO_SAMPLES;
    shm->samplepos = 0;
    shm->submission_chunk = 1;
    shm->soundalive = true;
    shm->gamealive = true;
    shm->buffer = audio_buffer;

    submitted_frames = 0;
    clock_frame = 0;
    clock_milliseconds = BTDN_GetMilliseconds();
    clock_started = 0;
    return true;
}

int SNDDMA_GetDMAPos(void)
{
    uint32_t now;
    uint32_t elapsed;
    uint64_t played_frames;

    if (shm == NULL || !clock_started)
        return 0;

    now = BTDN_GetMilliseconds();
    elapsed = now - clock_milliseconds;
    played_frames = clock_frame +
        ((uint64_t)elapsed * BTDN_AUDIO_RATE) / 1000;

    if (played_frames >= submitted_frames)
    {
        played_frames = submitted_frames;
        clock_frame = played_frames;
        clock_milliseconds = now;
    }

    shm->samplepos = (int)((played_frames & (BTDN_AUDIO_FRAMES - 1)) *
                           BTDN_AUDIO_CHANNELS);
    return shm->samplepos;
}

void SNDDMA_Submit(void)
{
    uint64_t target_frames;

    if (shm == NULL || paintedtime < 0)
        return;

    target_frames = (uint64_t)paintedtime;
    if (target_frames < submitted_frames)
        submitted_frames = target_frames;

    while (submitted_frames < target_frames)
    {
        int frame_offset = (int)(submitted_frames & (BTDN_AUDIO_FRAMES - 1));
        int frame_count = (int)(target_frames - submitted_frames);
        int contiguous_frames = BTDN_AUDIO_FRAMES - frame_offset;
        int written;

        if (frame_count > contiguous_frames)
            frame_count = contiguous_frames;
        if (frame_count > BTDN_AUDIO_SUBMIT_FRAMES)
            frame_count = BTDN_AUDIO_SUBMIT_FRAMES;

        written = QG_AudioWrite(
            (const short *)shm->buffer + frame_offset * BTDN_AUDIO_CHANNELS,
            frame_count);
        if (written <= 0)
            break;
        if (written > frame_count)
            written = frame_count;

        if (!clock_started)
        {
            clock_frame = submitted_frames;
            clock_milliseconds = BTDN_GetMilliseconds();
            clock_started = 1;
        }
        submitted_frames += (uint64_t)written;
        if (written != frame_count)
            break;
    }
}

void SNDDMA_Shutdown(void)
{
    sn.soundalive = false;
    sn.gamealive = false;
    submitted_frames = 0;
    clock_started = 0;
}
