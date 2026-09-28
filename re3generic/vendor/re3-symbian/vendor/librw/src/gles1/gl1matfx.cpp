#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include "../rwbase.h"
#include "../rwplg.h"
#include "../rwpipeline.h"
#include "../rwobjects.h"
#include "../rwengine.h"
#include "../rwrender.h"
#include "../rwanim.h"
#include "../rwplugins.h"
#include "rwgles1.h"
#include "rwgles1plg.h"
#include "rwgles1impl.h"

#ifdef RW_GLES1

namespace rw {
namespace gles1 {

void
matfxRenderCB(Atomic *atomic, InstanceDataHeader *header)
{
    uint32 flags = atomic->geometry->flags;
    setWorldMatrix(atomic->getFrame()->getLTM());
    glDisable(GL_LIGHTING);

    setupVertexInput(header);

    InstanceData *inst = header->inst;
    int32 n = header->numMeshes;

    while(n--){
        MatFX *matfx = MatFX::get(inst->material);
        Material *m = inst->material;

        // En GLES1 sin shaders: ignoramos env map, solo dibujamos textura base
        setMaterial(flags, m->color, m->surfaceProps);
        setTexture(0, m->texture);
        rw::SetRenderState(VERTEXALPHA, inst->vertexAlpha || m->color.alpha != 0xFF);
        drawInst(header, inst);
        inst++;
    }

    teardownVertexInput(header);
}

ObjPipeline*
makeMatFXPipeline(void)
{
    ObjPipeline *pipe = ObjPipeline::create();
    pipe->instanceCB   = defaultInstanceCB;
    pipe->uninstanceCB = defaultUninstanceCB;
    pipe->renderCB     = matfxRenderCB;
    pipe->pluginID     = ID_MATFX;
    pipe->pluginData   = 0;
    return pipe;
}

static void*
matfxOpen(void *o, int32, int32)
{
    matFXGlobals.pipelines[PLATFORM_GLES1] = makeMatFXPipeline();
    return o;
}

static void*
matfxClose(void *o, int32, int32)
{
    ((ObjPipeline*)matFXGlobals.pipelines[PLATFORM_GLES1])->destroy();
    matFXGlobals.pipelines[PLATFORM_GLES1] = nil;
    return o;
}

void
initMatFX(void)
{
    Driver::registerPlugin(PLATFORM_GLES1, 0, ID_MATFX, matfxOpen, matfxClose);
}

}
}

#endif
