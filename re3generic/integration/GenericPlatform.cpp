#include "common.h"
#include "platform.h"
#include "crossplatform.h"
#include "FileMgr.h"
#include "PCSave.h"
#include "re3generic.h"

psGlobalType psGlobal;
RwUInt32 gGameState;
DWORD _dwOperatingSystemVersion;
size_t _dwMemAvailPhys = 256u * 1024u * 1024u;
size_t streamingMemSize;
rw::EngineOpenParams openParams;

const char *_psGetUserFilesFolder()
{
    return ".";
}

extern "C" {

RwUInt32 psTimer(void)
{
    const RG_Port *port = rg_bound_port();
    return port ? (RwUInt32)port->ticks_ms(port->userdata) : 0;
}

RwBool psInitialize(void)
{
    CFileMgr::Initialise();
    C_PcSave::SetSaveDirectory(_psGetUserFilesFolder());
    return TRUE;
}

void psTerminate(void) {}
void psCameraShowRaster(RwCamera *camera) { RwCameraShowRaster(camera, nil, 0); }
RwBool psCameraBeginUpdate(RwCamera *camera) { return RwCameraBeginUpdate(camera) != nil; }
RwImage *psGrabScreen(RwCamera *) { return nil; }
void psMouseSetPos(RwV2d *) {}
RwBool psSelectDevice(void) { return TRUE; }
RwMemoryFunctions *psGetMemoryFunctions(void) { return nil; }
RwBool psInstallFileSystem(void) { return TRUE; }
RwBool psNativeTextureSupport(void) { return TRUE; }
void _InputTranslateShiftKeyUpDown(RsKeyCodes *) {}
long _InputInitialiseMouse(void) { return 0; }
void _InputInitialiseJoys(void) {}
void HandleExit(void) {}
RwBool _psSetVideoMode(RwInt32, RwInt32) { return TRUE; }
RwInt32 _psGetNumVideModes(void) { return 1; }
RwChar **_psGetVideoModeList(void)
{
    static RwChar *modes[] = { (RwChar *)"Software" };
    return modes;
}
void _psSelectScreenVM(RwInt32) {}
void InitialiseLanguage(void) {}

}

int strcasecmp(const char *left, const char *right)
{
    return _stricmp(left, right);
}

RwBool IsForegroundApp(void) { return TRUE; }
void CapturePad(RwInt32) {}
void joysChangeCB(int, int) {}
