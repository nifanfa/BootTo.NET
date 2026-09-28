#include "common.h"
#ifdef TIMEBARS
#include "Font.h"
#include "Frontend.h"
#include "Timer.h"
#include "Text.h"

#define MAX_TIMERS (50)
#define MAX_MS_COLLECTED (40)

// enables frame time output
#define FRAMETIME

struct sTimeBar
{
	char name[20];
	float startTime;
	float endTime;
	int32 unk;
};

struct
{
	sTimeBar Timers[MAX_TIMERS];
	uint32 count;
} TimerBar;
float MaxTimes[MAX_TIMERS];
float MaxFrameTime;

uint32 curMS;
uint32 msCollected[MAX_MS_COLLECTED];
#ifdef FRAMETIME
float FrameInitTime;
#endif

extern "C" int vboUploads;
extern "C" int draw;
extern "C" int render;
extern "C" int matfx;
extern "C" int skin;
extern "C" int im2d;
extern "C" int im3d;
int vboUploads;
int draw;
int render;
int matfx;
int skin;
int im2d;
int im3d;
float endOfFrameTime;
uint32 frames;

void tbInit()
{
	TimerBar.count = 0;
	uint32 i = CTimer::GetFrameCounter() & 0x7F;
	if (i == 0) {
		do
			MaxTimes[i++] = 0.0f;
		while (i != MAX_TIMERS);
#ifdef FRAMETIME
		MaxFrameTime = 0.0f;
#endif
	}
#ifdef FRAMETIME
	FrameInitTime = (float)CTimer::GetCurrentTimeInCycles() / (float)CTimer::GetCyclesPerFrame();
#endif
}

void tbStartTimer(int32 unk, Const char *name)
{
	strcpy(TimerBar.Timers[TimerBar.count].name, name);
	TimerBar.Timers[TimerBar.count].unk = unk;
	TimerBar.Timers[TimerBar.count].startTime = (float)CTimer::GetCurrentTimeInCycles() / (float)CTimer::GetCyclesPerFrame();
	TimerBar.count++;
}

void tbEndTimer(Const char* name)
{
	uint32 n = 1500;
	for (uint32 i = 0; i < TimerBar.count; i++) {
		if (strcmp(name, TimerBar.Timers[i].name) == 0)
			n = i;
	}
	assert(n != 1500);
	TimerBar.Timers[n].endTime = (float)CTimer::GetCurrentTimeInCycles() / (float)CTimer::GetCyclesPerFrame();
}

float Diag_GetFPS()
{
	return 39000.0f / (msCollected[(curMS - 1) % MAX_MS_COLLECTED] - msCollected[curMS % MAX_MS_COLLECTED]);
}

#ifdef LOGS_RDEBUG
extern "C" void RDebug_Printf(const char*, ...);
#endif

void tbDisplay()
{
	char temp[200];
	wchar wtemp[200];

#ifdef FRAMETIME
	float FrameEndTime = (float)CTimer::GetCurrentTimeInCycles() / (float)CTimer::GetCyclesPerFrame();
#endif

	msCollected[(curMS++) % MAX_MS_COLLECTED] = RsTimer();
	CFont::SetBackgroundOff();
	CFont::SetBackgroundColor(CRGBA(0, 0, 0, 128));
#ifdef RE3_GENERIC
	CFont::SetScale(0.32f, 0.7f);
#else
	CFont::SetScale(0.48f, 1.12f);
#endif
	CFont::SetCentreOff();
	CFont::SetJustifyOff();
	CFont::SetWrapx(SCREEN_STRETCH_X(DEFAULT_SCREEN_WIDTH));
	CFont::SetRightJustifyOff();
	CFont::SetPropOn();
	CFont::SetFontStyle(FONT_BANK);
	sprintf(temp, "FPS: %.2f", Diag_GetFPS());
	if (frames >= 15) {
//#ifdef LOGS_RDEBUG
//		RDebug_Printf("FPS: %.2f", Diag_GetFPS());
//		RDebug_Printf("vbo: %d, draw: %d, render: %d, matfx: %d, skin: %d, im2d: %d, im3d: %d", vboUploads, draw, render, matfx, skin, im2d, im3d);
//		for (uint32 i = 0; i < TimerBar.count; i++) {
//			MaxTimes[i] = Max(MaxTimes[i], TimerBar.Timers[i].endTime - TimerBar.Timers[i].startTime);
//			RDebug_Printf("%s: %.2f", &TimerBar.Timers[i].name[0], MaxTimes[i]);
//		}
//		MaxFrameTime = Max(MaxFrameTime, FrameEndTime - FrameInitTime);
//		RDebug_Printf("EndOfFrame: %.2f", endOfFrameTime);
//		RDebug_Printf("Frame Time: %.2f", MaxFrameTime);
//		RDebug_Printf(" ");
//#endif
		frames = 0;
	} else frames++;
	
	vboUploads = 0;
	draw = 0;
	render = 0;
	matfx = 0;
	skin = 0;
	im2d = 0;
	im3d = 0;
	
	AsciiToUnicode(temp, wtemp);
	CFont::SetColor(CRGBA(255, 255, 255, 255));
#ifndef MASTER
	if (!CMenuManager::m_PrefsMarketing || !CMenuManager::m_PrefsDisableTutorials)
#endif
	{
#ifdef RE3_GENERIC
		CFont::PrintString(RsGlobal.maximumWidth * 0.42f, RsGlobal.maximumHeight * (4.0f / DEFAULT_SCREEN_HEIGHT), wtemp);
#else
		CFont::PrintString(RsGlobal.maximumWidth * (4.0f / DEFAULT_SCREEN_WIDTH), RsGlobal.maximumHeight * (4.0f / DEFAULT_SCREEN_HEIGHT), wtemp);
#endif

#ifndef FINAL
		// Timers output (my own implementation)
		for (uint32 i = 0; i < TimerBar.count; i++) {
			MaxTimes[i] = Max(MaxTimes[i], TimerBar.Timers[i].endTime - TimerBar.Timers[i].startTime);
			sprintf(temp, "%s: %.2f", &TimerBar.Timers[i].name[0], MaxTimes[i]);
			AsciiToUnicode(temp, wtemp);
			CFont::PrintString(RsGlobal.maximumWidth * (4.0f / DEFAULT_SCREEN_WIDTH), RsGlobal.maximumHeight * ((8.0f * (i + 2)) / 200), wtemp);
		}

#ifdef FRAMETIME
		MaxFrameTime = Max(MaxFrameTime, FrameEndTime - FrameInitTime);
		sprintf(temp, "Frame Time: %.2f", MaxFrameTime);
		AsciiToUnicode(temp, wtemp);

		CFont::PrintString(RsGlobal.maximumWidth * (4.0f / DEFAULT_SCREEN_WIDTH), RsGlobal.maximumHeight * ((8.0f * (TimerBar.count + 4)) / 200), wtemp);
#endif // FRAMETIME
#endif // !FINAL
	}
}
#endif // !MASTER
