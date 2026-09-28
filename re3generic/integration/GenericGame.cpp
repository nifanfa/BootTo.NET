#include "common.h"
#include "main.h"
#include "platform.h"
#include "crossplatform.h"
#include "Game.h"
#include "Frontend.h"
#include "Text.h"
#include "Pad.h"
#include "ControllerConfig.h"
#include "re3generic_game.h"
#include "re3generic.h"

extern rw::EngineOpenParams openParams;
extern psGlobalType psGlobal;
extern "C" void rg_audio_pump(void);

static bool initialised;
static bool playing;
static int32_t mouse_delta_x;
static int32_t mouse_delta_y;
static int32_t mouse_wheel;
static bool mouse_buttons[5];

extern "C" void rg_generic_mouse_update(CMouseControllerState *state)
{
    state->Clear();
    int sign_x = !FrontEndMenuManager.m_bMenuActive && MousePointerStateHelper.bInvertHorizontally ? -1 : 1;
    int sign_y = !FrontEndMenuManager.m_bMenuActive && MousePointerStateHelper.bInvertVertically ? -1 : 1;
    state->x = (float)(sign_x * (int64_t)mouse_delta_x);
    state->y = (float)(sign_y * (int64_t)mouse_delta_y);
    state->LMB = mouse_buttons[RG_MOUSE_LEFT];
    state->RMB = mouse_buttons[RG_MOUSE_RIGHT];
    state->MMB = mouse_buttons[RG_MOUSE_MIDDLE];
    state->MXB1 = mouse_buttons[RG_MOUSE_X1];
    state->MXB2 = mouse_buttons[RG_MOUSE_X2];
    state->WHEELUP = mouse_wheel > 0;
    state->WHEELDN = mouse_wheel < 0;
    mouse_delta_x = mouse_delta_y = mouse_wheel = 0;
}

int rg_game_init(uint32_t width, uint32_t height)
{
    if (initialised || !rg_bound_port() || width == 0 || height == 0)
        return 0;

    RsGlobal.ps = &psGlobal;
    if (RsEventHandler(rsINITIALIZE, nil) == rsEVENTERROR)
        return 0;

    CMenuManager::m_PrefsLanguage = CMenuManager::LANGUAGE_AMERICAN;
    RsGlobal.width = RsGlobal.maximumWidth = width;
    RsGlobal.height = RsGlobal.maximumHeight = height;
    openParams.width = width;
    openParams.height = height;
    openParams.windowtitle = "re3generic";
    ControlsManager.MakeControllerActionsBlank();
    ControlsManager.InitDefaultControlConfiguration();
    if (RsEventHandler(rsRWINITIALIZE, &openParams) == rsEVENTERROR)
        return 0;

    if (!CGame::InitialiseOnceAfterRW())
        return 0;

    RwRect screen = { 0, 0, (RwInt32)width, (RwInt32)height };
    RsEventHandler(rsCAMERASIZE, &screen);
    FrontEndMenuManager.m_bGameNotLoaded = true;
    CMenuManager::m_bStartUpFrontEndRequested = true;
    initialised = true;
    return 1;
}

int rg_game_step(void)
{
    if (!initialised || RsGlobal.quit)
        return 0;

    const RG_Port *port = rg_bound_port();
    RG_InputEvent event;
    while (port->poll_input(port->userdata, &event)) {
        if (event.type == RG_INPUT_KEY_DOWN || event.type == RG_INPUT_KEY_UP) {
            RsKeyStatus status = {};
            status.keyCharCode = (RsKeyCodes)event.code;
            RsKeyboardEventHandler(event.type == RG_INPUT_KEY_DOWN ? rsKEYDOWN : rsKEYUP, &status);
        } else if (event.type == RG_INPUT_MOUSE_MOVE) {
            FrontEndMenuManager.m_nMouseTempPosX = event.value;
            FrontEndMenuManager.m_nMouseTempPosY = event.value_y;
        } else if (event.type == RG_INPUT_MOUSE_DELTA) {
            mouse_delta_x += event.value;
            mouse_delta_y += event.value_y;
        } else if (event.type == RG_INPUT_MOUSE_WHEEL) {
            mouse_wheel += event.value;
        } else if (event.type == RG_INPUT_MOUSE_BUTTON_DOWN || event.type == RG_INPUT_MOUSE_BUTTON_UP) {
            if (event.code >= RG_MOUSE_LEFT && event.code <= RG_MOUSE_X2)
                mouse_buttons[event.code] = event.type == RG_INPUT_MOUSE_BUTTON_DOWN;
        } else if (event.type == RG_INPUT_MOUSE_RESET) {
            mouse_delta_x = mouse_delta_y = mouse_wheel = 0;
            for (unsigned button = 0; button < 5; ++button)
                mouse_buttons[button] = false;
        }
    }

    if (!playing) {
        RsEventHandler(rsFRONTENDIDLE, nil);
        if (!FrontEndMenuManager.m_bMenuActive || FrontEndMenuManager.m_bWantToLoad) {
            InitialiseGame();
            FrontEndMenuManager.m_bGameNotLoaded = false;
            playing = true;
        }
    } else {
        RsEventHandler(rsIDLE, (void *)TRUE);
    }
    rg_audio_pump();
    return !RsGlobal.quit;
}

void rg_game_shutdown(void)
{
    if (!initialised)
        return;
    if (playing)
        CGame::ShutDown();
    RsEventHandler(rsRWTERMINATE, nil);
    RsEventHandler(rsTERMINATE, nil);
    initialised = false;
    playing = false;
}
