#include <e32base.h>
#include <e32keys.h>
#include <coemain.h>
#include <w32std.h>
#include <aknapp.h>
#include <akndoc.h>
#include <aknappui.h>
#include <coecntrl.h>
#include <eikstart.h>
#include <hal.h>
#include <pthread.h>
#ifdef RW_GL3
#include <EGL/egl.h>
#else
#include <GLES/egl.h>
#endif
#include <locale.h>
#include <versioninfo.h>
#include <aknwseventobserver.h>
#include <featdiscovery.h>
#include <aknutils.h>
#ifndef KFeatureIdQwertyInput
#include <featureinfo.h>
#endif

#include "common.h"
#include "rwcore.h"
#include "skel/skeleton.h"
#include "platform.h"
#include "crossplatform.h"

#include "main.h"
#include "FileMgr.h"
#include "Pad.h"
#include "DMAudio.h"
#include "ControllerConfig.h"
#include "Frontend.h"
#include "Game.h"
#include "PCSave.h"
#include "MemoryCard.h"
#include "Sprite2d.h"
#include "AnimViewer.h"
#include "MemoryMgr.h"
#include "Text.h"
#include "Timer.h"

psGlobalType psGlobal;

uint32 gGameState = 0;
long _dwOperatingSystemVersion = 0;
uint32 _dwMemAvailPhys = 0;
size_t streamingMemSize;

static EGLDisplay eglDisplay;
static EGLContext eglContext;
static EGLSurface eglSurface;
static EGLConfig eglConfig;

rw::EngineOpenParams openParams;

static RwBool ForegroundApp = TRUE;
static RwBool RwInitialised = FALSE;

const TUid KUidRE3 = {0xe0d67647};

#define JOY_A 0 // cross
#define JOY_B 1 // circle
#define JOY_X 2 // square
#define JOY_Y 3 // triangle
#define JOY_L1 4
#define JOY_R1 5
#define JOY_L2 6
#define JOY_R2 7
#define JOY_BACK 8
#define JOY_L3 9
#define JOY_R3 10
#define JOY_START 11
#define JOY_DPAD_UP 12
#define JOY_DPAD_RIGHT 13
#define JOY_DPAD_DOWN 14
#define JOY_DPAD_LEFT 15

static uint8 virtualButtons[17] = { 0 };
static float virtualLeftStickX = 0.0f;
static float virtualLeftStickY = 0.0f;
static float virtualRightStickX = 0.0f;
static float virtualRightStickY = 0.0f;

static TInt tickPeriod;
static uint32 cyclesPerMS;

static psGlobalType PsGlobal;

static bool spinning;

bool qwerty;
bool touch;
bool moreVram;
bool powervr;

void _InputTranslateShiftKeyUpDown(RsKeyCodes *rs) {
}

const char* _psGetUserFilesFolder() {
	return "E:\\Data\\gta3\\";
}

void HandleExit() {
	if (ForegroundApp) {
		User::ResetInactivityTime();
	}
	RThread thread;
	TInt error = KErrNone;
	spinning = true;
	while (thread.RequestCount()) {
		if (!CActiveScheduler::RunIfReady(error, CActive::EPriorityIdle))
			continue;
		User::WaitForAnyRequest();
	}
	spinning = false;
}

RwInt32 _psGetNumVideModes() {
	return 1;
}

RwChar** _psGetVideoModeList() {
	static RwChar* modes[] = { (RwChar*)"Native" };
	return modes;
}

void psPostRWinit(void)
{
	CPad::GetPad(0)->Clear(true);
	CPad::GetPad(1)->Clear(true);
}

void _psSelectScreenVM(RwInt32 videoMode) {
	RwTexDictionarySetCurrent( nil );
	FrontEndMenuManager.UnloadTextures();
	
	RwInitialised = FALSE;
	
	RsEventHandler(rsRWTERMINATE, nil);
	
	if ( RsEventHandler(rsRWINITIALIZE, &openParams) == rsEVENTERROR )
		return;

	RwInitialised = TRUE;
	
	RwRect r;
	
	r.x = 0;
	r.y = 0;
	r.w = RsGlobal.maximumWidth;
	r.h = RsGlobal.maximumHeight;

	RsEventHandler(rsCAMERASIZE, &r);
	
	psPostRWinit();
	
	FrontEndMenuManager.LoadAllTextures();
}

void CapturePad(int padID) {
	if (padID != 0) return;

	if (!ControlsManager.m_bFirstCapture) {
		memcpy(&ControlsManager.m_OldState, &ControlsManager.m_NewState, sizeof(ControlsManager.m_NewState));
	} else {
		memset(&ControlsManager.m_OldState, 0, sizeof(ControlsManager.m_OldState));
	}

	ControlsManager.m_NewState.buttons = virtualButtons;
	ControlsManager.m_NewState.numButtons = 17;
	ControlsManager.m_NewState.id = 0;
	ControlsManager.m_NewState.isGamepad = true;
	
	memcpy(ControlsManager.m_NewState.mappedButtons, virtualButtons, 17);

	if (ControlsManager.m_bFirstCapture) {
		memcpy(&ControlsManager.m_OldState, &ControlsManager.m_NewState, sizeof(ControlsManager.m_NewState));
		ControlsManager.m_bFirstCapture = false;
	}
	
	RsPadButtonStatus bs;
	bs.padID = padID;

	RsPadEventHandler(rsPADBUTTONUP, (void *)&bs);
	
	{
		if (CPad::m_bMapPadOneToPadTwo)
			bs.padID = 1;
		
		RsPadEventHandler(rsPADBUTTONUP,   (void *)&bs);
		RsPadEventHandler(rsPADBUTTONDOWN, (void *)&bs);
	}

	CPad *pad = CPad::GetPad(0);
	pad->PCTempJoyState.LeftStickX  = (int32)(virtualLeftStickX * 128.0f);
	pad->PCTempJoyState.LeftStickY  = (int32)(virtualLeftStickY * 128.0f);
	pad->PCTempJoyState.RightStickX = (int32)(virtualRightStickX * 128.0f);
	pad->PCTempJoyState.RightStickY = (int32)(virtualRightStickY * 128.0f);
}

RwBool psCameraBeginUpdate(RwCamera *camera) {
	RwCameraBeginUpdate(camera);
	return TRUE;
}

void psCameraShowRaster(RwCamera *camera) {
	RwCameraShowRaster(camera, NULL, 0);
}

RwMemoryFunctions* psGetMemoryFunctions(void) {
	return nil;
}

RwImage* psGrabScreen(RwCamera *camera) {
	return NULL;
}

RwBool psInstallFileSystem(void) {
	return TRUE;
}

void psMouseSetPos(RwV2d *pos) {
}

RwBool psNativeTextureSupport(void) {
	return TRUE;
}

RwBool psSelectDevice(void) {
	return TRUE;
}

void psTerminate(void) {
}

RwBool psInitialize(void) {
	CFileMgr::Initialise();
	TInt memFree = 0;
	HAL::Get(HALData::EMemoryRAMFree, memFree);
	if (memFree <= 0) {
		_dwMemAvailPhys = 10 * 1024 * 1024; 
	} else if (memFree > 60 * 1024 * 1024) {
		_dwMemAvailPhys = 60 * 1024 * 1024; 
	} else {
		_dwMemAvailPhys = memFree;
	}
	
	C_PcSave::SetSaveDirectory(_psGetUserFilesFolder());
	
	return TRUE;
}

RwUInt32 psTimer(void) {
//	return (RwUInt32)User::NTickCount();
	TTime time;
	time.HomeTime();
	return (RwUInt32)(time.Int64() / 1000);
}

namespace sk {
	void SetMousePosition(int x, int y) {}
}


extern "C" void RDebug_Printf(const char*, ...);
void RDebug_Printf(const char* c, ...)
{
    VA_LIST list;
    VA_START(list, c);
	TBuf8<0x100> buf;
	TPtrC8 des((const TUint8 *)c);
    buf.FormatList(des, list);
	RDebug::RawPrint(buf);
}

uint32 CTimer::GetCyclesPerMillisecond(void) {
//	return cyclesPerMS;
	return 1;
}

RwBool IsForegroundApp() {
	return ForegroundApp;
}

static int MapScanCode(TInt aScanCode, TInt aModifiers) {
	switch (aScanCode) {

	case EStdKeyLeftArrow:
		if (aModifiers & EModifierRotateBy90) {
			return JOY_DPAD_UP;
		}
		if (aModifiers & EModifierRotateBy180) {
			return JOY_DPAD_RIGHT;
		}
		if (aModifiers & EModifierRotateBy270) {
			return JOY_DPAD_DOWN;
		}
		return JOY_DPAD_LEFT;
	case EStdKeyRightArrow:
		if (aModifiers & EModifierRotateBy90) {
			return JOY_DPAD_DOWN;
		}
		if (aModifiers & EModifierRotateBy180) {
			return JOY_DPAD_LEFT;
		}
		if (aModifiers & EModifierRotateBy270) {
			return JOY_DPAD_UP;
		}
		return JOY_DPAD_RIGHT;
	case EStdKeyUpArrow:
		if (aModifiers & EModifierRotateBy90) {
			return JOY_DPAD_RIGHT;
		}
		if (aModifiers & EModifierRotateBy180) {
			return JOY_DPAD_DOWN;
		}
		if (aModifiers & EModifierRotateBy270) {
			return JOY_DPAD_LEFT;
		}
		return JOY_DPAD_UP;
	case EStdKeyDownArrow:
		if (aModifiers & EModifierRotateBy90) {
			return JOY_DPAD_LEFT;
		}
		if (aModifiers & EModifierRotateBy180) {
			return JOY_DPAD_UP;
		}
		if (aModifiers & EModifierRotateBy270) {
			return JOY_DPAD_RIGHT;
		}
		return JOY_DPAD_DOWN;
		
	case 'z':
	case 'Z':
	case '1':
	case EStdKeyDevice3:
	case EStdKeyEnter:
	case EStdKeyNkpEnter:
		return JOY_B;
	case 'x':
	case 'X':
	case '2':
		return JOY_X;
	case 'a':
	case 'A':
	case '4':
		return JOY_A;
	case 's':
	case 'S':
	case '3':
	case EStdKeyBackspace:
		return JOY_Y;
		
	case '5':
		return JOY_L1;
	case '6':
		return JOY_R1;
	case '7':
		return JOY_L2;
	case '8':
		return JOY_R2;
	case '9':
		return JOY_L3;
	case '0':
		return JOY_R3;
		
	case EStdKeyDevice0:
		return JOY_BACK;
	case EStdKeyDevice1:
	case EStdKeySpace:
	case ' ':
		return JOY_START;
		
	}
	return -1;
}

class CCContainer : public CCoeControl, MAknWsEventObserver {
public:
	CAknAppUi* iAppUi;
	CPeriodic* iPeriodic;
	RwUInt32 gGameState;
	static TInt LoopCallBack(TAny* p) {
		CCContainer* container = (CCContainer*) p;
		if(RsGlobal.quit) {
			RsEventHandler(rsRWTERMINATE, nil);
			container->iAppUi->Exit();
			return EFalse;
		}
		
		if (spinning) return ETrue;
		RwInitialised = TRUE;
		
		switch (container->gGameState) {
		case GS_START_UP:
			if (HAL::Get(HAL::ENanoTickPeriod, tickPeriod) != KErrNone) {
				User::Panic(_L("Could not init timer"), 0);
			}
			cyclesPerMS = 1000 / tickPeriod;
			if (cyclesPerMS < 1) cyclesPerMS = 1;
			container->gGameState = GS_INIT_ONCE;
			break;
		case GS_INIT_ONCE:
			LoadingScreen(nil, nil, "loadsc0");
			if ( !CGame::InitialiseOnceAfterRW() ) {
				RsGlobal.quit = TRUE;
				return ETrue;
			}
			container->gGameState = GS_INIT_FRONTEND;
//			container->gGameState = GS_INIT_PLAYING_GAME;
			break;
		case GS_INIT_FRONTEND:
			LoadingScreen(nil, nil, "loadsc0");
			
			FrontEndMenuManager.m_bGameNotLoaded = true;
			
			CMenuManager::m_bStartUpFrontEndRequested = true;
			
			container->gGameState = GS_FRONTEND;
			break;
		case GS_FRONTEND:
			if (!ForegroundApp) break;
			RsEventHandler(rsFRONTENDIDLE, nil);
			if ( !FrontEndMenuManager.m_bMenuActive || FrontEndMenuManager.m_bWantToLoad )
			{
				container->gGameState = GS_INIT_PLAYING_GAME;
				TRACE("gGameState = GS_INIT_PLAYING_GAME;");
			}
			if ( FrontEndMenuManager.m_bWantToLoad )
			{
				InitialiseGame();
				FrontEndMenuManager.m_bGameNotLoaded = false;
				container->gGameState = GS_PLAYING_GAME;
				TRACE("gGameState = GS_PLAYING_GAME;");
			}
			break;
		case GS_INIT_PLAYING_GAME:
			InitialiseGame();

			FrontEndMenuManager.m_bGameNotLoaded = false;
			container->gGameState = GS_PLAYING_GAME;
			TRACE("gGameState = GS_PLAYING_GAME;");
			break;
		case GS_PLAYING_GAME:
//			float ms = (float)CTimer::GetCurrentTimeInCycles() / (float)CTimer::GetCyclesPerMillisecond();
			if (RwInitialised && ForegroundApp) {
//				if (!CMenuManager::m_PrefsFrameLimiter || (1000.0f / (float)RsGlobal.maxFPS) < ms)
					RsEventHandler(rsIDLE, (void *)TRUE);
			}
			break;
		}
		
		if (FrontEndMenuManager.m_bWantToRestart) {
			RwInitialised = FALSE;
			FrontEndMenuManager.UnloadTextures();
			CPad::ResetCheats();
			CPad::StopPadsShaking();
			DMAudio.ChangeMusicMode(MUSICMODE_DISABLE);
			CTimer::Stop();
			if ( FrontEndMenuManager.m_bWantToLoad )
			{
				CGame::ShutDownForRestart();
				CGame::InitialiseWhenRestarting();
				DMAudio.ChangeMusicMode(MUSICMODE_GAME);
				LoadSplash(GetLevelSplashScreen(CGame::currLevel));
				FrontEndMenuManager.m_bWantToLoad = false;
			} else {
				if ( container->gGameState == GS_PLAYING_GAME )
					CGame::ShutDown();
				
				CTimer::Stop();
				
				if ( FrontEndMenuManager.m_bFirstTime == true )
				{
					container->gGameState = GS_INIT_FRONTEND;
					TRACE("gGameState = GS_INIT_FRONTEND;");
				}
				else
				{
					container->gGameState = GS_INIT_PLAYING_GAME;
					TRACE("gGameState = GS_INIT_PLAYING_GAME;");
				}
			}
			
			FrontEndMenuManager.m_bFirstTime = false;
			FrontEndMenuManager.m_bWantToRestart = false;
		}
		
		if (ForegroundApp) {
			User::ResetInactivityTime();
		}

		return ETrue;
	}

	void RestartTimerL(TInt aInterval) {
		if (iPeriodic) iPeriodic->Cancel();
		else iPeriodic = CPeriodic::NewL(CActive::EPriorityLow);
		iPeriodic->Start(aInterval, aInterval, TCallBack(CCContainer::LoopCallBack, this));
	}
	
	void ConstructL(const TRect& aRect, CAknAppUi* aAppUi) {
		iAppUi = aAppUi;
		
		CAknWsEventMonitor* monitor = iAppUi->EventMonitor();
		monitor->AddObserverL(this);
		monitor->Enable();
		
		CreateWindowL();
		iAppUi->SetOrientationL(CAknAppUiBase::EAppUiOrientationLandscape);
		SetExtentToWholeScreen();

		SetFocus(ETrue);

#ifndef S60V3
		Window().EnableAdvancedPointers();
#endif
		EnableDragEvents();
		ActivateL();

#ifndef S60V3
		// VC4 check
		VersionInfo::TPlatformVersion platformVersion;
		VersionInfo::GetVersion(platformVersion);
		moreVram = platformVersion.iMajorVersion == 5 && platformVersion.iMinorVersion >= 4;
		if (moreVram) {
			streamingMemSize = 12 * 1024 * 1024;
		} else {
			streamingMemSize = 10 * 1024 * 1024;
		}
#else
		streamingMemSize = 8 * 1024 * 1024;
#endif

		TSize size = Size();
		RsGlobal.width = size.iWidth;
		RsGlobal.height = size.iHeight;

		EGLint attribs[] = {
			EGL_BUFFER_SIZE,       16,
			EGL_DEPTH_SIZE,        16,
			EGL_STENCIL_SIZE,      0,
			EGL_SURFACE_TYPE,      EGL_WINDOW_BIT,
#ifdef RW_GL3
			EGL_SAMPLES, 0,
			EGL_COLOR_BUFFER_TYPE, EGL_RGB_BUFFER,
			EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
#endif
			EGL_NONE
		};

		eglDisplay = eglGetDisplay(EGL_DEFAULT_DISPLAY);
		eglInitialize(eglDisplay, NULL, NULL);
#ifdef RW_GL3
		eglBindAPI(EGL_OPENGL_ES_API);
#endif

		EGLint numConfigs;
		eglChooseConfig(eglDisplay, attribs, &eglConfig, 1, &numConfigs);

#ifdef RW_GL3
		EGLint contextAttribs[ 3 ] = { EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE };
		eglContext = eglCreateContext(eglDisplay, eglConfig, EGL_NO_CONTEXT, contextAttribs);
#else
		eglContext = eglCreateContext(eglDisplay, eglConfig, EGL_NO_CONTEXT, NULL);
#endif

		RWindow& window = Window();
		eglSurface = eglCreateWindowSurface(eglDisplay, eglConfig, &window, NULL);
		
		eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext);
		
		const char* renderer = (const char *)glGetString(GL_RENDERER);
		if (strstr(renderer, "MBX") || strstr(renderer, "SGX")) {
			// use pvrtc
			powervr = true;
		} else if (strstr(renderer, "VideoCore")) {
			// use etc1
		} else {
			User::Leave(KErrNotSupported);
			return;
		}
		
		setlocale(LC_ALL, "");	
		setlocale(LC_CTYPE, "C");
		setlocale(LC_COLLATE, "C");
		setlocale(LC_NUMERIC, "C");
		
		touch = AknLayoutUtils::PenEnabled();
		TRAP_IGNORE(qwerty = CFeatureDiscovery::IsFeatureSupportedL(KFeatureIdQwertyInput));
		
		chdir("E:\\Data\\gta3\\");
		gGameState = GS_START_UP;
		FrontEndMenuManager.LoadSettings();
		CMenuManager::OS_Language = LANG_ENGLISH;
		CMenuManager::m_PrefsLanguage = CMenuManager::LANGUAGE_AMERICAN;
		TheText.Unload();
		TheText.Load();

		chdir("E:\\Data\\gta3\\");
		if(RsEventHandler(rsINITIALIZE, nil) == rsEVENTERROR) return;

		openParams.width = RsGlobal.width;
		openParams.height = RsGlobal.height;
		
		ControlsManager.MakeControllerActionsBlank();
		ControlsManager.InitDefaultControlConfiguration();

		RwInitialised = FALSE;
		if(RsEventHandler(rsRWINITIALIZE, &openParams) == rsEVENTERROR) return;
		
		RwInitialised = TRUE;
		psPostRWinit();
		
		PsGlobal.lastMousePos.x = PsGlobal.lastMousePos.y = 0.0f;
		RsGlobal.ps = &PsGlobal;

		ControlsManager.InitDefaultControlConfigJoyPad(16);
		ControlsManager.InitDefaultControlConfigMouse(MousePointerStateHelper.GetMouseSetUp());
		
		{
			RwRect r;

			r.x = 0;
			r.y = 0;
			r.w = RsGlobal.maximumWidth;
			r.h = RsGlobal.maximumHeight;

			RsEventHandler(rsCAMERASIZE, &r);
		}

		RestartTimerL(10000);
	}
	
	static bool c(int px, int py, int x, int y, int w, int h) {
		return px >= x && py >= y && px <= x + w && py <= y + h;
	}
	
	virtual void HandlePointerEventL(const TPointerEvent& aPointerEvent) {
		touch = true;
		int x = aPointerEvent.iPosition.iX;
		int y = aPointerEvent.iPosition.iY;
		int w = Size().iWidth;
		int h = Size().iHeight;
		
		if (h > 360) y = (y * 360) / 480;

//		if (FrontEndMenuManager.m_bMenuActive) {
//			PSGLOBAL(lastMousePos).x = x;
//			PSGLOBAL(lastMousePos).y = y;
//			FrontEndMenuManager.m_nMouseTempPosX = x;
//			FrontEndMenuManager.m_nMouseTempPosY = y;
//			
//			if (aPointerEvent.iType == TPointerEvent::EButton1Down) {
//				CPad::GetPad(0)->PCTempMouseControllerState.LMB = true;
//			} else if (aPointerEvent.iType == TPointerEvent::EButton1Up) {
//				CPad::GetPad(0)->PCTempMouseControllerState.RMB = false;
//			}
//			CCoeControl::HandlePointerEventL(aPointerEvent);
//			return;
//		}
#ifndef S60V3
		const TAdvancedPointerEvent* advpointer = aPointerEvent.AdvancedPointerEvent();
		int i = advpointer != NULL ? advpointer->PointerNumber() : 0;
#else
		int i = 0;
#endif

		static int activeZone[10];
		static int stickCenterX[10];
		static int stickCenterY[10];
		
		if (i >= 10) {
			CCoeControl::HandlePointerEventL(aPointerEvent);
			return;
		}

		if (aPointerEvent.iType == TPointerEvent::EButton1Down) {
			if (y >= 0 && y <= 32) {
				if (x > 40 && x < 40 + 90) {
					// l1
					activeZone[i] = JOY_L1 + 1;
					virtualButtons[JOY_L1] = 1;
				} else if (x > 180 && x < 180 + 90) {
					// l2
					activeZone[i] = JOY_L2 + 1;
					virtualButtons[JOY_L2] = 1;
				} else if (x > w - 40 - 90 && x < w - 40) {
					// r1
					activeZone[i] = JOY_R1 + 1;
					virtualButtons[JOY_R1] = 1;
				} else if (x > w - 180 - 90 && x < w - 180) {
					// r2
					activeZone[i] = JOY_R2 + 1;
					virtualButtons[JOY_R2] = 1;
				}
			} else if (y > 220 && y <= 360 && (x < 200 || x > w - 200)) {
				if (x < 200) {
					activeZone[i] = -1;
					stickCenterX[i] = x;
					stickCenterY[i] = y;
				} else {
					activeZone[i] = -2;
					stickCenterX[i] = x;
					stickCenterY[i] = y;
				}
			} else if (y > 280 && y < 280 + 32) {
				if (x > 320 - 80 && x < 320 - 20) {
					// select
					activeZone[i] = JOY_BACK + 1;
					virtualButtons[JOY_BACK] = 1;
				} else if (x > 320 + 20 && x < 320 + 80) {
					// start
					activeZone[i] = JOY_START + 1;
					virtualButtons[JOY_START] = 1;
				}
			} else if (y > 280 - 32 && y < 280) {
				if (x > 320 - 80 && x < 320 - 20) {
					activeZone[i] = JOY_L3 + 1;
					virtualButtons[JOY_L3] = 1;
				} else if (x > 320 + 20 && x < 320 + 80) {
					activeZone[i] = JOY_R3 + 1;
					virtualButtons[JOY_R3] = 1;
				}
			} else if (y >= 80 && y <= 80 + 120) {
				do {
					int cy = (h * 2) / 5;
					if (x < w / 4) {
						// dpad
						int b = 0;
						int cx = w / 8;
						if (c(x, y, cx - 20, cy - 60, 40, 30)) {
							b = JOY_DPAD_UP;
						} else if (c(x, y, cx - 20, cy + 30, 40, 40)) {
							b = JOY_DPAD_DOWN;
						} else if (c(x, y, cx - 75, cy - 20, 40, 40)) {
							b = JOY_DPAD_LEFT;
						} else if (c(x, y, cx + 35, cy - 20, 40, 40)) {
							b = JOY_DPAD_RIGHT;
						} else {
							break;
						}
						activeZone[i] = b + 1;
						virtualButtons[b] = 1;
					} else if (x > w - w / 4) {
						// abxy
						int cx = w - w / 8;
						int b = 0;
						if (c(x, y, cx - 20, cy - 60, 40, 40)) {
							b = JOY_Y;
						} else if (c(x, y, cx - 20, cy + 30, 40, 40)) {
							b = JOY_B;
						} else if (c(x, y, cx - 75, cy - 20, 40, 40)) {
							b = JOY_X;
						} else if (c(x, y, cx + 35, cy - 20, 40, 40)) {
							b = JOY_A;
						} else {
							break;
						}
						activeZone[i] = b + 1;
						virtualButtons[b] = 1;
					}
				} while (0);
			}
		} else if (aPointerEvent.iType == TPointerEvent::EDrag) {
			if (activeZone[i] == -1 || activeZone[i] == -2) {
				float dx = (float)(x - stickCenterX[i]) / (w / 8.0f);
				float dy = (float)(y - stickCenterY[i]) / (h / 6.0f);
				
				if (dx < -1.0f) dx = -1.0f;
				if (dx > 1.0f) dx = 1.0f;
				if (dy < -1.0f) dy = -1.0f;
				if (dy > 1.0f) dy = 1.0f;

				if (activeZone[i] == -1) {
					virtualLeftStickX = dx;
					virtualLeftStickY = dy;
				} else {
					virtualRightStickX = dx;
					virtualRightStickY = dy;
				}
			}
		} else if (aPointerEvent.iType == TPointerEvent::EButton1Up) {
			switch (activeZone[i]) {
			case -1:
				virtualLeftStickX = 0.0f;
				virtualLeftStickY = 0.0f;
				break;
			case -2:
				virtualRightStickX = 0.0f;
				virtualRightStickY = 0.0f;
				break;
			case 0:
				break;
			default:
				virtualButtons[activeZone[i] - 1] = 0;
			}
			activeZone[i] = 0;
		}

		CCoeControl::HandlePointerEventL(aPointerEvent);
	}

	void HandleWsEventL(const TWsEvent &aEvent, CCoeControl *aDestination) {
		if (!ForegroundApp || iAppUi->IsDisplayingDialog()) return;
		switch (aEvent.Type()) {
		case EEventKeyDown:
		case EEventKeyUp: {
			TInt k = MapScanCode(aEvent.Key()->iScanCode, aEvent.Key()->iModifiers);
			if (k == -1) break;
			touch = false;
			virtualButtons[k] = aEvent.Type() == EEventKeyDown;
			break;
		}
		default:
			break;
		}
	}
	
	void HandleResourceChange(TInt aType) {
		switch (aType) {
		case KEikDynamicLayoutVariantSwitch:
			SetExtentToWholeScreen();
			break;
		}
	}
};

class RE3AppUi : public CAknAppUi {
	CCContainer* iContainer;
public:
	void ConstructL() {
		BaseConstructL(CAknAppUi::EAknEnableSkin);
		iContainer = new (ELeave) CCContainer;
		iContainer->SetMopParent(this);
		iContainer->ConstructL(ClientRect(), this);
		AddToStackL(iContainer);
	}
	void HandleForegroundEventL(TBool aForeground) {
		ForegroundApp = aForeground;
	}

	~RE3AppUi() {
		if (iContainer) {
			RemoveFromStack(iContainer);
			delete iContainer;
		}
	}
	void HandleCommandL(TInt aCommand) {
		if (aCommand == EAknSoftkeyBack || aCommand == EEikCmdExit) {
			RsGlobal.quit = 1;
//			Exit();
		}
	}
};

class RE3Document: public CAknDocument {
public:
	static RE3Document* NewL(CEikApplication& aApp);
	virtual ~RE3Document();
protected:
	void ConstructL();
public:
	RE3Document(CEikApplication& aApp);
private:
	CEikAppUi* CreateAppUiL();
};

RE3Document::RE3Document(CEikApplication& aApp) : CAknDocument(aApp) {}

RE3Document::~RE3Document() {}

void RE3Document::ConstructL() {}

RE3Document* RE3Document::NewL(CEikApplication& aApp) {
	RE3Document* self = new (ELeave) RE3Document(aApp);
	CleanupStack::PushL(self);
	self->ConstructL();
	CleanupStack::Pop();
	return self;
}

CEikAppUi* RE3Document::CreateAppUiL() {
	return new (ELeave) RE3AppUi;
}

class RE3App: public CAknApplication {
private:
	CApaDocument* CreateDocumentL();
	TUid AppDllUid() const;
};

LOCAL_C CApaApplication* NewApplication();

GLDEF_C TInt E32Main();

TUid RE3App::AppDllUid() const {
	return KUidRE3;
}

CApaDocument* RE3App::CreateDocumentL() {
	return RE3Document::NewL(*this);
}

CApaApplication* NewApplication() {
	return new RE3App;
}

TInt E32Main() {
	User::SetFloatingPointMode(EFpModeRunFast);
	return EikStart::RunApplication(NewApplication);
}
