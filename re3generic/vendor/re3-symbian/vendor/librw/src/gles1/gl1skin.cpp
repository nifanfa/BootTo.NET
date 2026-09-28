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
skinRenderCB(Atomic *atomic, InstanceDataHeader *header)
{
    Material *m;
    uint32 flags = atomic->geometry->flags;
    setWorldMatrix(atomic->getFrame()->getLTM());
    glDisable(GL_LIGHTING);

    setupVertexInput(header);

    InstanceData *inst = header->inst;
    int32 n = header->numMeshes;

    //test rapido a ver si es la textura
    glDisableClientState(GL_COLOR_ARRAY);
    glColor4f(1,1,1,1);

    while(n--){
        m = inst->material;
        setMaterial(flags, m->color, m->surfaceProps);
        setTexture(0, m->texture);
        rw::SetRenderState(VERTEXALPHA, inst->vertexAlpha || m->color.alpha != 0xFF);
        drawInst(header, inst);
        inst++;
    }

    teardownVertexInput(header);
}

// skinInstanceCB: igual que defaultInstanceCB, sin pesos/indices
// porque GLES1 no puede hacer skinning en GPU
// aunque... capaz podemos hacerloc on el PowerVR y las extensiones
void
skinInstanceCB(Geometry *geo, InstanceDataHeader *header, bool32 reinstance)
{
    defaultInstanceCB(geo, header, reinstance);
}

void
skinUninstanceCB(Geometry *geo, InstanceDataHeader *header)
{
    assert(0 && "can't uninstance");
}

ObjPipeline*
makeSkinPipeline(void)
{
    ObjPipeline *pipe = ObjPipeline::create();
    pipe->instanceCB   = skinInstanceCB;
    pipe->uninstanceCB = skinUninstanceCB;
    pipe->renderCB     = skinRenderCB;
    pipe->pluginID     = ID_SKIN;
    pipe->pluginData   = 1;
    return pipe;
}

static void*
skinOpen(void *o, int32, int32)
{
    skinGlobals.pipelines[PLATFORM_GLES1] = makeSkinPipeline();
    return o;
}

static void*
skinClose(void *o, int32, int32)
{
    ((ObjPipeline*)skinGlobals.pipelines[PLATFORM_GLES1])->destroy();
    skinGlobals.pipelines[PLATFORM_GLES1] = nil;
    return o;
}

void
initSkin(void)
{
    Driver::registerPlugin(PLATFORM_GLES1, 0, ID_SKIN, skinOpen, skinClose);
}

}
}

#endif