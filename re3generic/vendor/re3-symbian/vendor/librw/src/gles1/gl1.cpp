#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include "../rwbase.h"
#include "../rwerror.h"
#include "../rwplg.h"
#include "../rwpipeline.h"
#include "../rwobjects.h"
#include "../rwengine.h"

#include "rwgles1.h"
#include "rwgles1impl.h"

#ifdef RW_GLES1

namespace rw {
namespace gles1 {

static void*
driverOpen(void* object, int32 offset, int32 size)
{
	engine->driver[PLATFORM_GLES1]->defaultPipeline = makeDefaultPipeline();
	engine->driver[PLATFORM_GLES1]->rasterNativeOffset = nativeRasterOffset;
	engine->driver[PLATFORM_GLES1]->rasterCreate       = rasterCreate;
	engine->driver[PLATFORM_GLES1]->rasterLock         = rasterLock;
	engine->driver[PLATFORM_GLES1]->rasterUnlock       = rasterUnlock;
	engine->driver[PLATFORM_GLES1]->rasterNumLevels    = rasterNumLevels;
	engine->driver[PLATFORM_GLES1]->imageFindRasterFormat = imageFindRasterFormat;
	engine->driver[PLATFORM_GLES1]->rasterFromImage    = rasterFromImage;
	engine->driver[PLATFORM_GLES1]->rasterToImage      = rasterToImage;
    return object;
}

static void*
driverClose(void* object, int32 offset, int32 size)
{
    // stub: no hace nada
    return object;
}

void
registerPlatformPlugins(void)
{
	Driver::registerPlugin(PLATFORM_GLES1, 0, PLATFORM_GLES1,
	                       driverOpen, driverClose);
	registerNativeRaster();
}

}
}

#endif
