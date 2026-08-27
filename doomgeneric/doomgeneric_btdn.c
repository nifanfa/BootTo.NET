#include "doomkeys.h"

#include "doomgeneric.h"

#include "../NativeLib/printf.h"

char* gameBinary;

#define KEYQUEUE_SIZE 48

static unsigned short s_KeyQueue[KEYQUEUE_SIZE];
static unsigned int s_KeyQueueWriteIndex = 0;
static unsigned int s_KeyQueueReadIndex = 0;
static int locked;

void DG_Init()
{
	locked = 0;
	memset(s_KeyQueue, 0, KEYQUEUE_SIZE * sizeof(unsigned short));
}

void DG_SetWindowTitle(const char* title)
{}

extern void DG_PresentFrame(const unsigned int *pixels, int width, int height);
extern void DG_Sleep(unsigned int milliseconds);
extern unsigned int DG_GetTicks(void);
extern int DG_PollKey(int *pressed, unsigned char *key);

void DG_DrawFrame()
{
	DG_PresentFrame(DG_ScreenBuffer, DOOMGENERIC_RESX, DOOMGENERIC_RESY);
}

void DG_SleepMs(uint32_t ms)
{
	DG_Sleep(ms);
}

uint32_t DG_GetTicksMs()
{
	return DG_GetTicks();
}

static unsigned char convertToDoomKey(unsigned char key)
{
	switch (key)
	{
	case 13:
		key = KEY_ENTER;
		break;
	case 27:
		key = KEY_ESCAPE;
		break;
	case 87:
		key = KEY_UPARROW;
		break;
	case 83:
		key = KEY_DOWNARROW;
		break;
	case 65:
		key = KEY_STRAFE_L;
		break;
	case 68:
		key = KEY_STRAFE_R;
		break;
	case 69:
		key = KEY_USE;
		break;
	case 160:
		key = KEY_RSHIFT;
		break;
	default:
		key = 0;
		break;
	}

	return key;
}


void addKeyToQueue(int pressed, unsigned char keyCode)
{
	unsigned char key = convertToDoomKey(keyCode);

	unsigned short keyData = (pressed << 8) | key;

	s_KeyQueue[s_KeyQueueWriteIndex] = keyData;
	s_KeyQueueWriteIndex++;
	s_KeyQueueWriteIndex %= KEYQUEUE_SIZE;
}

int DG_GetKey(int* pressed, unsigned char* doomKey)
{
	int nativePressed;
	unsigned char nativeKey;

	if (DG_PollKey(&nativePressed, &nativeKey))
	{
		addKeyToQueue(nativePressed, nativeKey);
	}

	if (s_KeyQueueReadIndex == s_KeyQueueWriteIndex)
	{
		//key queue is empty

		return 0;
	}
	else
	{
		unsigned short keyData = s_KeyQueue[s_KeyQueueReadIndex];
		s_KeyQueueReadIndex++;
		s_KeyQueueReadIndex %= KEYQUEUE_SIZE;

		*pressed = keyData >> 8;
		*doomKey = keyData & 0xFF;

		return 1;
	}
}
