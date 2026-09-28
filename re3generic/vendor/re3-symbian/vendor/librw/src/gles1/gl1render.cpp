#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "../rwbase.h"
#include "../rwerror.h"
#include "../rwplg.h"
#include "../rwrender.h"
#include "../rwengine.h"
#include "../rwpipeline.h"
#include "../rwobjects.h"
#ifdef RW_GLES1
#include "rwgles1.h"
#include "rwgles1impl.h"

namespace rw {
namespace gles1 {

#define MAX_LIGHTS 

void
drawInst_simple(InstanceDataHeader *header, InstanceData *inst)
{
	flushCache();
	glDrawElements(header->primType, inst->numIndex,
	               GL_UNSIGNED_SHORT, (void*)(uintptr)inst->offset);
}

void
drawInst(InstanceDataHeader *header, InstanceData *inst)
{
	drawInst_simple(header, inst);
}


void
setupVertexInput(InstanceDataHeader *header)
{
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, header->ibo);
	glBindBuffer(GL_ARRAY_BUFFER, header->vbo);
	bool hasNormal = false;
	bool hasColor = false;
	bool hasTex = false;

	for (int32 i = 0; i < header->numAttribs; i++) {
		AttribDesc *a = &header->attribDesc[i];
		if (a->index == ATTRIB_POS) {
			glEnableClientState(GL_VERTEX_ARRAY);
			glVertexPointer(a->size, a->type, a->stride, (void*)(uintptr)a->offset);
		} else if (a->index == ATTRIB_NORMAL) {
			glEnableClientState(GL_NORMAL_ARRAY);
			glNormalPointer(a->type, a->stride, (void*)(uintptr)a->offset);
			hasNormal = true;
		} else if (a->index == ATTRIB_COLOR){
			glEnableClientState(GL_COLOR_ARRAY);
			glColorPointer(a->size, a->type, a->stride, (void*)(uintptr)a->offset);
			hasColor = true;
		} else if(a->index == ATTRIB_TEXCOORDS0){
			glEnableClientState(GL_TEXTURE_COORD_ARRAY);
			glTexCoordPointer(a->size, a->type, a->stride, (void*)(uintptr)a->offset);
			hasTex = true;
		}
	}
	if (!hasNormal) glDisableClientState(GL_NORMAL_ARRAY);
	if (!hasColor) {
		glDisableClientState(GL_COLOR_ARRAY);
		glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
	}
	if(!hasTex) glDisableClientState(GL_TEXTURE_COORD_ARRAY);
}

void
teardownVertexInput(InstanceDataHeader *header)
{
}

int32
lightingCB(Atomic *atomic)
{
	WorldLights lightData;
	Light *directionals[8];
	Light *locals[8];
	lightData.directionals = directionals;
	lightData.numDirectionals = 0; // 8;
	lightData.locals = locals;
	lightData.numLocals = 0; // 8;

	if(atomic->geometry->flags & rw::Geometry::LIGHT){
		((World*)engine->currentWorld)->enumerateLights(atomic, &lightData);
		if((atomic->geometry->flags & rw::Geometry::NORMALS) == 0){
			// Get rid of lights that need normals when we don't have any
			lightData.numDirectionals = 0;
			lightData.numLocals = 0;
		}
		return setLights(&lightData);
	}else{
		memset(&lightData, 0, sizeof(lightData));
		return setLights(&lightData);
	}
}

void
defaultRenderCB(Atomic *atomic, InstanceDataHeader *header)
{
	Material *m;

	uint32 flags = atomic->geometry->flags;
	setWorldMatrix(atomic->getFrame()->getLTM());
	//int32 vsBits = lightingCB(atomic);
	
    glDisable(GL_LIGHTING);

	setupVertexInput(header);

	InstanceData *inst = header->inst;
	int32 n = header->numMeshes;

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


}
}

#endif

