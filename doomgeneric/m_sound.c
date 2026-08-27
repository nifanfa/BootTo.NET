#include "config.h"

#include "libc.h"

#include "deh_str.h"
#include "i_sound.h"
#include "w_wad.h"
#include "z_zone.h"

#define MIX_RATE 11025
#define MIX_FRAMES 315
#define NUM_CHANNELS 8

typedef struct
{
    const uint8_t *samples;
    uint32_t sample_count;
    uint32_t position;
    uint32_t step;
    int volume;
    int separation;
    boolean active;
} sound_channel_t;

static snddevice_t sound_devices[] =
{
    SNDDEVICE_SB,
    SNDDEVICE_PAS,
    SNDDEVICE_GUS,
    SNDDEVICE_WAVEBLASTER,
    SNDDEVICE_SOUNDCANVAS,
    SNDDEVICE_AWE32,
};

static sound_channel_t channels[NUM_CHANNELS];
static int16_t mix_buffer[MIX_FRAMES * 2];

extern int DG_AudioWrite(const int16_t *samples, int frame_count);

static void GetSfxLumpName(sfxinfo_t *sfx, char *buffer, size_t buffer_length)
{
    if (sfx->link != NULL)
    {
        sfx = sfx->link;
    }

    DEH_snprintf(buffer, buffer_length, "ds%s", DEH_String(sfx->name));
}

static int I_GetSfxLumpNum(sfxinfo_t *sfx)
{
    char name[9];

    GetSfxLumpName(sfx, name, sizeof(name));
    return W_GetNumForName(name);
}

static void I_UpdateSoundParams(int handle, int volume, int separation)
{
    int channel = handle - 1;

    if ((unsigned int) channel < NUM_CHANNELS && channels[channel].active)
    {
        channels[channel].volume = volume;
        channels[channel].separation = separation;
    }
}

static int I_StartSound(sfxinfo_t *sfx, int channel, int volume, int separation)
{
    const uint8_t *data;
    uint32_t sample_count;
    uint16_t sample_rate;
    int lump_length;
    int slot;

    (void) channel;

    data = W_CacheLumpNum(sfx->lumpnum, PU_STATIC);
    lump_length = W_LumpLength(sfx->lumpnum);

    if (data == NULL || lump_length < 8 || *(const uint16_t *) data != 3)
    {
        return 0;
    }

    sample_rate = *(const uint16_t *) (data + 2);
    sample_count = *(const uint32_t *) (data + 4);
    if (sample_rate == 0 || sample_count == 0 || sample_count > (uint32_t) (lump_length - 8))
    {
        return 0;
    }

    for (slot = 0; slot < NUM_CHANNELS; ++slot)
    {
        if (!channels[slot].active)
        {
            break;
        }
    }

    if (slot == NUM_CHANNELS)
    {
        return 0;
    }

    channels[slot].samples = data + 8;
    channels[slot].sample_count = sample_count;
    channels[slot].position = 0;
    channels[slot].step = ((uint32_t) sample_rate << 16) / MIX_RATE;
    if (channels[slot].step == 0)
    {
        channels[slot].step = 1;
    }
    channels[slot].volume = volume;
    channels[slot].separation = separation;
    channels[slot].active = true;

    return slot + 1;
}

static void I_StopSound(int handle)
{
    int channel = handle - 1;

    if ((unsigned int) channel < NUM_CHANNELS)
    {
        channels[channel].active = false;
    }
}

static boolean I_SoundIsPlaying(int handle)
{
    int channel = handle - 1;

    return (unsigned int) channel < NUM_CHANNELS && channels[channel].active;
}

static int ClampSample(int value)
{
    if (value > 32767)
    {
        return 32767;
    }
    if (value < -32768)
    {
        return -32768;
    }
    return value;
}

static void I_UpdateSound(void)
{
    int frame;
    boolean have_sound = false;

    for (frame = 0; frame < MIX_FRAMES; ++frame)
    {
        int left = 0;
        int right = 0;
        int channel;

        for (channel = 0; channel < NUM_CHANNELS; ++channel)
        {
            sound_channel_t *source = &channels[channel];
            uint32_t sample_index;
            int sample;

            if (!source->active)
            {
                continue;
            }

            sample_index = source->position >> 16;
            if (sample_index >= source->sample_count)
            {
                source->active = false;
                continue;
            }

            sample = ((int) source->samples[sample_index] - 128) << 8;
            left += sample * (254 - source->separation) * source->volume / (254 * 127);
            right += sample * source->separation * source->volume / (254 * 127);
            source->position += source->step;
            have_sound = true;
        }

        mix_buffer[frame * 2] = (int16_t) ClampSample(left);
        mix_buffer[frame * 2 + 1] = (int16_t) ClampSample(right);
    }

    if (have_sound)
    {
        DG_AudioWrite(mix_buffer, MIX_FRAMES);
    }
}

static boolean SoundInit(boolean use_sfx_prefix)
{
    (void) use_sfx_prefix;
    return true;
}

static void SoundShutdown(void)
{
}

static void SoundPrecache(sfxinfo_t *sounds, int num_sounds)
{
    (void) sounds;
    (void) num_sounds;
}

sound_module_t DG_sound_module =
{
    sound_devices,
    sizeof(sound_devices) / sizeof(sound_devices[0]),
    SoundInit,
    SoundShutdown,
    I_GetSfxLumpNum,
    I_UpdateSound,
    I_UpdateSoundParams,
    I_StartSound,
    I_StopSound,
    I_SoundIsPlaying,
    SoundPrecache,
};
