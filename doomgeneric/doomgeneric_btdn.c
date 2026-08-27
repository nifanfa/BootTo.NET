#include "doomkeys.h"

#include "doomgeneric.h"

#include "../NativeLib/printf.h"

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
	case 8:
		key = KEY_BACKSPACE;
		break;
	case 9:
		key = KEY_TAB;
		break;
	case 37:
		key = KEY_LEFTARROW;
		break;
	case 38:
		key = KEY_UPARROW;
		break;
	case 39:
		key = KEY_RIGHTARROW;
		break;
	case 40:
		key = KEY_DOWNARROW;
		break;
	case 112:
		key = KEY_F1;
		break;
	case 113:
		key = KEY_F2;
		break;
	case 114:
		key = KEY_F3;
		break;
	case 115:
		key = KEY_F4;
		break;
	case 116:
		key = KEY_F5;
		break;
	case 117:
		key = KEY_F6;
		break;
	case 118:
		key = KEY_F7;
		break;
	case 119:
		key = KEY_F8;
		break;
	case 120:
		key = KEY_F9;
		break;
	case 121:
		key = KEY_F10;
		break;
	case 122:
		key = KEY_F11;
		break;
	case 123:
		key = KEY_F12;
		break;
	case 160:
		key = KEY_RSHIFT;
		break;
	default:
		if (key >= 'A' && key <= 'Z')
		{
			key = key - 'A' + 'a';
		}
		else if (key < ' ' || key > '~')
		{
			key = 0;
		}
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
