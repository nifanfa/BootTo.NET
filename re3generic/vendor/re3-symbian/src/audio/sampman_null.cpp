#include "common.h"
#if !defined(AUDIO_OAL) &&  !defined(AUDIO_MSS) && (!defined(__SYMBIAN32__) || defined(NO_AUDIO))
#include "sampman.h"
#include "AudioManager.h"
#ifdef RE3_GENERIC
#include "re3generic.h"
#include <new>
#include <stdio.h>
#define MINIMP3_IMPLEMENTATION
#include "../../../minimp3/minimp3.h"

struct GenericChannel {
	int16_t *samples;
	uint32 length;
	uint32 frequency;
	uint32 volume;
	uint32 pan;
	uint32 loopStart;
	uint32 loopEnd;
	uint32 loopCount;
	uint64 position;
	bool playing;
};

static GenericChannel channels[NUM_CHANNELS];
static void *sampleData;
static bool samplesAvailable;
static uint64 lastAudioTick;
static uint32 mixEffectsVolume = 127;
static uint32 mixFadeVolume = 127;

struct GenericStream {
	void *file;
	uint64 offset;
	uint64 dataStart;
	uint64 dataEnd;
	uint64 position;
	uint32 rate;
	uint32 step;
	uint32 phase;
	uint32 blockBytes;
	uint32 blockFrames;
	uint32 buffered;
	uint32 bufferIndex;
	uint8 channels;
	uint8 track;
	uint8 format;
	uint8 volume;
	uint8 pan;
	bool playing;
	bool paused;
	bool loop;
	int16 current[2];
	int16 decoded[4096];
	mp3dec_t decoder;
};

static GenericStream streams[MAX_STREAMS];

static uint16 stream_u16(const uint8 *bytes)
{
	return bytes[0] | (uint16(bytes[1]) << 8);
}

static uint32 stream_u32(const uint8 *bytes)
{
	return stream_u16(bytes) | (uint32(stream_u16(bytes + 2)) << 16);
}

static void close_stream(GenericStream &stream)
{
	const RG_Port *port = rg_bound_port();
	if (stream.file && port)
		port->file_close(port->userdata, stream.file);
	stream = {};
}

static bool open_stream(GenericStream &stream, uint8 track, uint32 startMs)
{
	close_stream(stream);
	if (track >= TOTAL_STREAMED_SOUNDS)
		return false;
	const RG_Port *port = rg_bound_port();
	stream.file = port->file_open(port->userdata, StreamedNameTable[track]);
	if (!stream.file)
		return false;
	stream.dataEnd = port->file_size(port->userdata, stream.file);
	uint8 header[32] = {};
	if (port->file_read_at(port->userdata, stream.file, 0, header, 12) != 12)
		goto failed;
	if (!memcmp(header, "RIFF", 4) && !memcmp(header + 8, "WAVE", 4)) {
		uint64 cursor = 12;
		while (cursor + 8 <= stream.dataEnd) {
			if (port->file_read_at(port->userdata, stream.file, cursor, header, 8) != 8)
				break;
			uint32 size = stream_u32(header + 4);
			if (size > stream.dataEnd - cursor - 8)
				break;
			if (!memcmp(header, "fmt ", 4) && size >= 16) {
				if (port->file_read_at(port->userdata, stream.file, cursor + 8, header, 20) < 16)
					break;
				stream.format = stream_u16(header);
				stream.channels = stream_u16(header + 2);
				stream.rate = stream_u32(header + 4);
				stream.blockBytes = stream_u16(header + 12);
				stream.blockFrames = stream.format == 17 && size >= 20 ? stream_u16(header + 18) : 0;
			} else if (!memcmp(header, "data", 4)) {
				stream.dataStart = cursor + 8;
				stream.dataEnd = cursor + 8 + size;
				break;
			}
			cursor += 8 + ((uint64(size) + 1) & ~uint64(1));
		}
		if (!stream.dataStart || (stream.format != 17 && stream.format != 1) ||
			(stream.channels != 1 && stream.channels != 2) || !stream.rate ||
			!stream.blockBytes || (stream.format == 17 &&
			(stream.blockBytes > 4096 || stream.blockFrames > 2048 || !stream.blockFrames)))
			goto failed;
	} else {
		stream.format = 3;
		stream.channels = 2;
		stream.rate = 32000;
		mp3dec_init(&stream.decoder);
	}
	stream.step = uint32(uint64(stream.rate) * 65536 / DIGITALRATE);
	stream.track = track;
	stream.offset = stream.dataStart;
	stream.phase = 65536;
	stream.volume = 127;
	stream.pan = 63;
	stream.loop = track < NUM_RADIOS || track == STREAMED_SOUND_CITY_AMBIENT ||
		track == STREAMED_SOUND_WATER_AMBIENT;
	if (startMs && stream.format == 17) {
		uint64 blocks = uint64(startMs) * stream.rate / (1000 * stream.blockFrames);
		stream.offset += blocks * stream.blockBytes;
		stream.position = blocks * stream.blockFrames;
	} else if (startMs && stream.format == 1) {
		stream.position = uint64(startMs) * stream.rate / 1000;
		stream.offset += stream.position * stream.blockBytes;
	}
	if (stream.offset >= stream.dataEnd)
		stream.offset = stream.dataStart;
	return true;
failed:
	close_stream(stream);
	return false;
}

static const int imaSteps[] = {
	7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31,
	34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143,
	157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658,
	724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499,
	2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630,
	9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623, 27086,
	29794, 32767
};

static int16 ima_sample(int &predictor, int &stepIndex, uint8 nibble)
{
	int step = imaSteps[stepIndex];
	int delta = step / 8;
	if (nibble & 1) delta += step / 4;
	if (nibble & 2) delta += step / 2;
	if (nibble & 4) delta += step;
	predictor += (nibble & 8) ? -delta : delta;
	if (predictor < -32768) predictor = -32768;
	if (predictor > 32767) predictor = 32767;
	static const int adjustments[] = { -1, -1, -1, -1, 2, 4, 6, 8 };
	stepIndex += adjustments[nibble & 7];
	if (stepIndex < 0) stepIndex = 0;
	if (stepIndex > 88) stepIndex = 88;
	return int16(predictor);
}

static bool fill_stream(GenericStream &stream)
{
	const RG_Port *port = rg_bound_port();
	for (int attempts = 0; attempts < 32; ++attempts) {
		if (stream.offset >= stream.dataEnd) {
			if (!stream.loop) {
				stream.playing = false;
				return false;
			}
			stream.offset = stream.dataStart;
			stream.position = 0;
			if (stream.format == 3)
				mp3dec_init(&stream.decoder);
		}
		uint8 bytes[16384];
		uint64 remaining = stream.dataEnd - stream.offset;
		if (stream.format == 3) {
			size_t length = remaining < sizeof(bytes) ? size_t(remaining) : sizeof(bytes);
			length = port->file_read_at(port->userdata, stream.file, stream.offset, bytes, length);
			if (!length) break;
			mp3dec_frame_info_t info = {};
			int frames = mp3dec_decode_frame(&stream.decoder, bytes, int(length), stream.decoded, &info);
			stream.offset += info.frame_bytes ? info.frame_bytes : 1;
			if (frames && (info.channels == 1 || info.channels == 2) && info.hz) {
				stream.channels = uint8(info.channels);
				stream.rate = info.hz;
				stream.step = uint32(uint64(info.hz) * 65536 / DIGITALRATE);
				stream.buffered = frames;
				stream.bufferIndex = 0;
				return true;
			}
		} else if (stream.format == 1) {
			size_t length = remaining < 4096 ? size_t(remaining) : 4096;
			length -= length % stream.blockBytes;
			if (!length || port->file_read_at(port->userdata, stream.file, stream.offset, stream.decoded, length) != length)
				break;
			stream.offset += length;
			stream.buffered = uint32(length / stream.blockBytes);
			stream.bufferIndex = 0;
			return true;
		} else {
			size_t length = remaining < stream.blockBytes ? size_t(remaining) : stream.blockBytes;
			if (length < stream.channels * 4 ||
				port->file_read_at(port->userdata, stream.file, stream.offset, bytes, length) != length)
				break;
			stream.offset += length;
			int predictor[2] = {};
			int stepIndex[2] = {};
			for (uint32 channel = 0; channel < stream.channels; ++channel) {
				predictor[channel] = int16(stream_u16(bytes + channel * 4));
				stepIndex[channel] = bytes[channel * 4 + 2];
				if (stepIndex[channel] > 88) stepIndex[channel] = 88;
				stream.decoded[channel] = int16(predictor[channel]);
			}
			uint32 frames = 1;
			for (size_t offset = stream.channels * 4; offset + stream.channels * 4 <= length &&
				frames < stream.blockFrames; offset += stream.channels * 4) {
				for (uint32 sample = 0; sample < 8 && frames + sample < stream.blockFrames; ++sample) {
					for (uint32 channel = 0; channel < stream.channels; ++channel) {
						uint8 packed = bytes[offset + channel * 4 + sample / 2];
						uint8 nibble = (sample & 1) ? packed >> 4 : packed & 15;
						stream.decoded[(frames + sample) * stream.channels + channel] =
							ima_sample(predictor[channel], stepIndex[channel], nibble);
					}
				}
				frames += 8;
			}
			stream.buffered = frames < stream.blockFrames ? frames : stream.blockFrames;
			stream.bufferIndex = 0;
			return true;
		}
	}
	stream.playing = false;
	return false;
}

static bool next_stream_frame(GenericStream &stream)
{
	if (stream.bufferIndex == stream.buffered && !fill_stream(stream))
		return false;
	uint32 index = stream.bufferIndex++ * stream.channels;
	stream.current[0] = stream.decoded[index];
	stream.current[1] = stream.channels == 2 ? stream.decoded[index + 1] : stream.current[0];
	++stream.position;
	return true;
}
#endif

cSampleManager SampleManager;
bool8 _bSampmanInitialised = FALSE;

uint32 BankStartOffset[MAX_SFX_BANKS];
uint32     nNumMP3s;

cSampleManager::cSampleManager(void)
{
	;
}

cSampleManager::~cSampleManager(void)
{
	
}

#ifdef EXTERNAL_3D_SOUND
void cSampleManager::SetSpeakerConfig(int32 nConfig)
{

}

uint32 cSampleManager::GetMaximumSupportedChannels(void)
{	
	return MAXCHANNELS;
}

uint32 cSampleManager::GetNum3DProvidersAvailable()
{
	return 1;
}

void cSampleManager::SetNum3DProvidersAvailable(uint32 num)
{
	
}

char *cSampleManager::Get3DProviderName(uint8 id)
{
	static char name[64] = "NULL";
	return name;
}

void cSampleManager::Set3DProviderName(uint8 id, char *name)
{
	
}

int8 cSampleManager::GetCurrent3DProviderIndex(void)
{
	return 0;
}

int8 cSampleManager::SetCurrent3DProvider(uint8 nProvider)
{
	return 0;
}
#endif

bool8
cSampleManager::IsMP3RadioChannelAvailable(void)
{
	return nNumMP3s != 0;
}


void cSampleManager::ReleaseDigitalHandle(void)
{
}

void cSampleManager::ReacquireDigitalHandle(void)
{
}

bool8
cSampleManager::Initialise(void)
{
#ifdef RE3_GENERIC
	const RG_Port *port = rg_bound_port();
	if (!port || !port->submit_pcm)
		return TRUE;
	void *description = port->file_open(port->userdata, "AUDIO\\SFX.SDT");
	sampleData = port->file_open(port->userdata, "AUDIO\\SFX.RAW");
	if (!description || !sampleData ||
		port->file_size(port->userdata, description) < sizeof(m_aSamples) ||
		port->file_read_at(port->userdata, description, 0, m_aSamples,
			sizeof(m_aSamples)) != sizeof(m_aSamples)) {
		if (description)
			port->file_close(port->userdata, description);
		if (sampleData)
			port->file_close(port->userdata, sampleData);
		sampleData = 0;
		return TRUE;
	}
	port->file_close(port->userdata, description);
	samplesAvailable = true;
	m_nEffectsVolume = m_nEffectsFadeVolume = 127;
	mixEffectsVolume = mixFadeVolume = 127;
	lastAudioTick = 0;
#endif
	return TRUE;
}

void
cSampleManager::Terminate(void)
{
#ifdef RE3_GENERIC
	for (uint32 index = 0; index < MAX_STREAMS; ++index)
		close_stream(streams[index]);
	for (uint32 index = 0; index < NUM_CHANNELS; ++index) {
		delete[] channels[index].samples;
		channels[index] = {};
	}
	const RG_Port *port = rg_bound_port();
	if (sampleData && port)
		port->file_close(port->userdata, sampleData);
	sampleData = 0;
	samplesAvailable = false;
	lastAudioTick = 0;
#endif
}

bool8 cSampleManager::CheckForAnAudioFileOnCD(void)
{
	return TRUE;
}

char cSampleManager::GetCDAudioDriveLetter(void)
{
	return '\0';
}

void
cSampleManager::UpdateEffectsVolume(void)
{
	
}

void
cSampleManager::SetEffectsMasterVolume(uint8 nVolume)
{
#ifdef RE3_GENERIC
	m_nEffectsVolume = nVolume;
	mixEffectsVolume = nVolume;
#endif
}

void
cSampleManager::SetMusicMasterVolume(uint8 nVolume)
{
}

void
cSampleManager::SetEffectsFadeVolume(uint8 nVolume)
{
#ifdef RE3_GENERIC
	m_nEffectsFadeVolume = nVolume;
	mixFadeVolume = nVolume;
#endif
}

void
cSampleManager::SetMusicFadeVolume(uint8 nVolume)
{
}

void
cSampleManager::SetMonoMode(uint8 nMode)
{
}

bool8
cSampleManager::LoadSampleBank(uint8 nBank)
{
	ASSERT( nBank < MAX_SFX_BANKS );
#ifdef RE3_GENERIC
	return samplesAvailable;
#else
	return FALSE;
#endif
}

void
cSampleManager::UnloadSampleBank(uint8 nBank)
{
	ASSERT( nBank < MAX_SFX_BANKS );
}

int8
cSampleManager::IsSampleBankLoaded(uint8 nBank)
{
	ASSERT( nBank < MAX_SFX_BANKS );
#ifdef RE3_GENERIC
	return samplesAvailable ? LOADING_STATUS_LOADED : LOADING_STATUS_NOT_LOADED;
#else
	return LOADING_STATUS_NOT_LOADED;
#endif
}

uint8
cSampleManager::IsPedCommentLoaded(uint32 nComment)
{
	ASSERT( nComment < TOTAL_AUDIO_SAMPLES );
#ifdef RE3_GENERIC
	return samplesAvailable && m_aSamples[nComment].nSize ? LOADING_STATUS_LOADED : LOADING_STATUS_NOT_LOADED;
#else
	return LOADING_STATUS_NOT_LOADED;
#endif
}


int32
cSampleManager::_GetPedCommentSlot(uint32 nComment)
{
	return -1;
}

bool8
cSampleManager::LoadPedComment(uint32 nComment)
{
	ASSERT( nComment < TOTAL_AUDIO_SAMPLES );
#ifdef RE3_GENERIC
	return samplesAvailable && m_aSamples[nComment].nSize;
#else
	return FALSE;
#endif
}

int32
cSampleManager::GetBankContainingSound(uint32 offset)
{
#ifdef RE3_GENERIC
	return samplesAvailable ? SFX_BANK_0 : INVALID_SFX_BANK;
#else
	return INVALID_SFX_BANK;
#endif
}

uint32
cSampleManager::GetSampleBaseFrequency(uint32 nSample)
{
	ASSERT( nSample < TOTAL_AUDIO_SAMPLES );
#ifdef RE3_GENERIC
	return samplesAvailable ? m_aSamples[nSample].nFrequency : 0;
#else
	return 0;
#endif
}

uint32
cSampleManager::GetSampleLoopStartOffset(uint32 nSample)
{
	ASSERT( nSample < TOTAL_AUDIO_SAMPLES );
#ifdef RE3_GENERIC
	return samplesAvailable ? m_aSamples[nSample].nLoopStart : 0;
#else
	return 0;
#endif
}

int32
cSampleManager::GetSampleLoopEndOffset(uint32 nSample)
{
	ASSERT( nSample < TOTAL_AUDIO_SAMPLES );
#ifdef RE3_GENERIC
	return samplesAvailable ? m_aSamples[nSample].nLoopEnd : 0;
#else
	return 0;
#endif
}

uint32
cSampleManager::GetSampleLength(uint32 nSample)
{
	ASSERT( nSample < TOTAL_AUDIO_SAMPLES );
#ifdef RE3_GENERIC
	return samplesAvailable ? m_aSamples[nSample].nSize / sizeof(int16_t) : 0;
#else
	return 0;
#endif
}

bool8 cSampleManager::UpdateReverb(void)
{
	return FALSE;
}

void
cSampleManager::SetChannelReverbFlag(uint32 nChannel, bool8 nReverbFlag)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
}

bool8
cSampleManager::InitialiseChannel(uint32 nChannel, uint32 nSfx, uint8 nBank)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	if (!samplesAvailable || nChannel >= NUM_CHANNELS || nSfx >= TOTAL_AUDIO_SAMPLES)
		return FALSE;
	GenericChannel &channel = channels[nChannel];
	delete[] channel.samples;
	channel = {};
	const RG_Port *port = rg_bound_port();
	const tSample &sample = m_aSamples[nSfx];
	if (sample.nSize < sizeof(int16_t) || sample.nSize > 256 * 1024 ||
		static_cast<uint64>(sample.nOffset) + sample.nSize > port->file_size(port->userdata, sampleData))
		return FALSE;
	channel.samples = new (std::nothrow) int16_t[sample.nSize / sizeof(int16_t)];
	if (!channel.samples)
		return FALSE;
	if (port->file_read_at(port->userdata, sampleData, sample.nOffset,
		channel.samples, sample.nSize) != sample.nSize) {
		delete[] channel.samples;
		channel = {};
		return FALSE;
	}
	channel.length = sample.nSize / sizeof(int16_t);
	channel.frequency = sample.nFrequency;
	channel.volume = 127;
	channel.pan = 63;
	channel.loopEnd = channel.length;
	channel.loopCount = 1;
	return TRUE;
#else
	return FALSE;
#endif
}

#ifdef EXTERNAL_3D_SOUND
void
cSampleManager::SetChannelEmittingVolume(uint32 nChannel, uint32 nVolume)
{
	ASSERT( nChannel < MAXCHANNELS );
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
}

void
cSampleManager::SetChannel3DPosition(uint32 nChannel, float fX, float fY, float fZ)
{
	ASSERT( nChannel < MAXCHANNELS );
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
}

void
cSampleManager::SetChannel3DDistances(uint32 nChannel, float fMax, float fMin)
{
	ASSERT( nChannel < MAXCHANNELS );
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
}
#endif

void
cSampleManager::SetChannelVolume(uint32 nChannel, uint32 nVolume)
{
	ASSERT( nChannel >= MAXCHANNELS );
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	channels[nChannel].volume = nVolume > 127 ? 127 : nVolume;
#endif
}

void
cSampleManager::SetChannelPan(uint32 nChannel, uint32 nPan)
{
	ASSERT( nChannel >= MAXCHANNELS );
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	channels[nChannel].pan = nPan > 127 ? 127 : nPan;
#endif
}

void
cSampleManager::SetChannelFrequency(uint32 nChannel, uint32 nFreq)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	channels[nChannel].frequency = nFreq;
#endif
}

void
cSampleManager::SetChannelLoopPoints(uint32 nChannel, uint32 nLoopStart, int32 nLoopEnd)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	GenericChannel &channel = channels[nChannel];
	channel.loopStart = nLoopStart / sizeof(int16_t);
	channel.loopEnd = nLoopEnd < 0 ? channel.length : static_cast<uint32>(nLoopEnd) / sizeof(int16_t);
	if (channel.loopEnd > channel.length)
		channel.loopEnd = channel.length;
#endif
}

void
cSampleManager::SetChannelLoopCount(uint32 nChannel, uint32 nLoopCount)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	channels[nChannel].loopCount = nLoopCount;
#endif
}

bool8
cSampleManager::GetChannelUsedFlag(uint32 nChannel)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	return channels[nChannel].playing;
#else
	return FALSE;
#endif
}

void
cSampleManager::StartChannel(uint32 nChannel)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	channels[nChannel].position = 0;
	channels[nChannel].playing = channels[nChannel].samples != 0;
#endif
}

void
cSampleManager::StopChannel(uint32 nChannel)
{
	ASSERT( nChannel < MAXCHANNELS+MAX2DCHANNELS );
#ifdef RE3_GENERIC
	delete[] channels[nChannel].samples;
	channels[nChannel] = {};
#endif
}

void
cSampleManager::PreloadStreamedFile(uint8 nFile, uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	open_stream(streams[nStream], nFile, 0);
#endif
}

void
cSampleManager::PauseStream(bool8 nPauseFlag, uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	streams[nStream].paused = nPauseFlag != FALSE;
#endif
}

void
cSampleManager::StartPreloadedStreamedFile(uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	streams[nStream].playing = streams[nStream].file != 0;
	streams[nStream].paused = false;
#endif
}

bool8
cSampleManager::StartStreamedFile(uint8 nFile, uint32 nPos, uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	if (open_stream(streams[nStream], nFile, nPos)) {
		streams[nStream].playing = true;
		return TRUE;
	}
#endif
	return FALSE;
}

void
cSampleManager::StopStreamedFile(uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	close_stream(streams[nStream]);
#endif
}

int32
cSampleManager::GetStreamedFilePosition(uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	GenericStream &stream = streams[nStream];
	return stream.rate ? int32(stream.position * 1000 / stream.rate) : 0;
#endif
	return 0;
}

void
cSampleManager::SetStreamedVolumeAndPan(uint8 nVolume, uint8 nPan, uint8 nEffectFlag, uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	streams[nStream].volume = nVolume > 127 ? 127 : nVolume;
	streams[nStream].pan = nPan > 127 ? 127 : nPan;
#endif
}

int32
cSampleManager::GetStreamedFileLength(uint8 nStream)
{
	ASSERT( nStream < TOTAL_STREAMED_SOUNDS );
#ifdef RE3_GENERIC
	GenericStream probe = {};
	if (!open_stream(probe, nStream, 0))
		return 1;
	uint64 length = 60000;
	if (probe.format == 17)
		length = (probe.dataEnd - probe.dataStart) / probe.blockBytes * probe.blockFrames * 1000 / probe.rate;
	else if (probe.format == 1)
		length = (probe.dataEnd - probe.dataStart) / probe.blockBytes * 1000 / probe.rate;
	else {
		uint8 bytes[16384];
		size_t count = probe.dataEnd < sizeof(bytes) ? size_t(probe.dataEnd) : sizeof(bytes);
		if (rg_bound_port()->file_read_at(rg_bound_port()->userdata, probe.file, 0, bytes, count) == count) {
			mp3dec_frame_info_t info = {};
			mp3dec_decode_frame(&probe.decoder, bytes, int(count), probe.decoded, &info);
			if (info.bitrate_kbps)
				length = probe.dataEnd * 8 / info.bitrate_kbps;
		}
	}
	close_stream(probe);
	return int32(length ? length : 1);
#endif
	return 1;
}

bool8
cSampleManager::IsStreamPlaying(uint8 nStream)
{
	ASSERT( nStream < MAX_STREAMS );
#ifdef RE3_GENERIC
	return streams[nStream].playing;
#endif
	return FALSE;
}

bool8
cSampleManager::InitialiseSampleBanks(void)
{
	return TRUE;
}

#ifdef RE3_GENERIC
extern "C" void rg_audio_pump(void)
{
	const RG_Port *port = rg_bound_port();
	if (!port || !port->submit_pcm)
		return;
	uint64 now = port->ticks_ms(port->userdata);
	if (!lastAudioTick) {
		lastAudioTick = now;
		return;
	}
	uint64 elapsed = now - lastAudioTick;
	lastAudioTick = now;
	if (elapsed > 128)
		elapsed = 128;
	uint32 frames = static_cast<uint32>(elapsed * DIGITALRATE / 1000);
	while (frames) {
		uint32 batch = frames < 512 ? frames : 512;
		int16_t output[512 * DIGITALCHANNELS];
		for (uint32 frame = 0; frame < batch; ++frame) {
			int32 left = 0;
			int32 right = 0;
			for (uint32 index = 0; samplesAvailable && index < NUM_CHANNELS; ++index) {
				GenericChannel &channel = channels[index];
				if (!channel.playing || !channel.samples)
					continue;
				uint32 end = channel.loopEnd;
				if (end > channel.length)
					end = channel.length;
				if (channel.position >> 16 >= end) {
					if (channel.loopCount == 1 || channel.loopStart >= end) {
						channel.playing = false;
						continue;
					}
					if (channel.loopCount > 1)
						--channel.loopCount;
					channel.position = static_cast<uint64>(channel.loopStart) << 16;
				}
				int32 sample = channel.samples[channel.position >> 16];
				uint32 gain = channel.volume * mixEffectsVolume * mixFadeVolume / (127 * 127);
				uint32 panLeft = channel.pan >= 64 ? (127 - channel.pan) * 2 : 127;
				uint32 panRight = channel.pan <= 63 ? channel.pan * 2 : 127;
				left += sample * static_cast<int32>(gain * panLeft) / (127 * 127);
				right += sample * static_cast<int32>(gain * panRight) / (127 * 127);
				channel.position += static_cast<uint64>(channel.frequency) * 65536 / DIGITALRATE;
			}
			for (uint32 index = 0; index < MAX_STREAMS; ++index) {
				GenericStream &stream = streams[index];
				if (!stream.playing || stream.paused || !stream.volume)
					continue;
				while (stream.phase >= 65536) {
					if (!next_stream_frame(stream))
						break;
					stream.phase -= 65536;
				}
				if (!stream.playing)
					continue;
				uint32 gainLeft = stream.pan >= 64 ? (127 - stream.pan) * 2 : 127;
				uint32 gainRight = stream.pan <= 63 ? stream.pan * 2 : 127;
				left += int32(stream.current[0]) * stream.volume * int32(gainLeft) / (127 * 127);
				right += int32(stream.current[1]) * stream.volume * int32(gainRight) / (127 * 127);
				stream.phase += stream.step;
			}
			if (left < -32768) left = -32768;
			if (left > 32767) left = 32767;
			if (right < -32768) right = -32768;
			if (right > 32767) right = 32767;
			output[frame * 2] = static_cast<int16_t>(left);
			output[frame * 2 + 1] = static_cast<int16_t>(right);
		}
		port->submit_pcm(port->userdata, output, batch, DIGITALRATE);
		frames -= batch;
	}
}
#endif

#endif
