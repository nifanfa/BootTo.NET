#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include "../rwbase.h"
#include "../rwerror.h"
#include "../rwplg.h"
#include "../rwrender.h"
#include "../rwpipeline.h"
#include "../rwobjects.h"
#include "../rwengine.h"
#ifdef RW_OPENGL
#include "rwgl3.h"
#include "rwgl3impl.h"
#include "rwgl3shader.h"

namespace rw {
namespace gl3 {

uint32 im2DVbo, im2DIbo;

Shader *im2dOverrideShader;

static int32 u_xform;

static Shader *im2dShader;

static int primTypeMap[] = {
	GL_POINTS,	// invalid
	GL_LINES,
	GL_LINE_STRIP,
	GL_TRIANGLES,
	GL_TRIANGLE_STRIP,
	GL_TRIANGLE_FAN,
	GL_POINTS
};

void
openIm2D(void)
{
	// must already be registered by device. we just need the value
	u_xform = registerUniform("u_xform", UNIFORM_VEC4);

#include "shaders/im2d_gl.inc"
#include "shaders/simple_fs_gl.inc"
	const char *vs[] = { shaderDecl, header_vert_src, im2d_vert_src, nil };
	const char *fs[] = { shaderDecl, header_frag_src, simple_frag_src, nil };
	im2dShader = Shader::create(vs, fs);
	assert(im2dShader);
}

void
closeIm2D(void)
{
	im2dShader->destroy();
	im2dShader = nil;
}

static Im2DVertex tmpprimbuf[3];

void
im2DRenderLine(void *vertices, int32 numVertices, int32 vert1, int32 vert2)
{
	Im2DVertex *verts = (Im2DVertex*)vertices;
	tmpprimbuf[0] = verts[vert1];
	tmpprimbuf[1] = verts[vert2];
	im2DRenderPrimitive(PRIMTYPELINELIST, tmpprimbuf, 2);
}

void
im2DRenderTriangle(void *vertices, int32 numVertices, int32 vert1, int32 vert2, int32 vert3)
{
	Im2DVertex *verts = (Im2DVertex*)vertices;
	tmpprimbuf[0] = verts[vert1];
	tmpprimbuf[1] = verts[vert2];
	tmpprimbuf[2] = verts[vert3];
	im2DRenderPrimitive(PRIMTYPETRILIST, tmpprimbuf, 3);
}

void
im2DSetXform(void)
{
	GLfloat xform[4];
	Camera *cam;
	cam = (Camera*)engine->currentCamera;
	xform[0] = 2.0f/cam->frameBuffer->width;
	xform[1] = -2.0f/cam->frameBuffer->height;
	xform[2] = -1.0f;
	xform[3] = 1.0f;
	setUniform(u_xform, xform);
//	glUniform4fv(currentShader->uniformLocations[u_xform], 1, xform);
}

extern "C" int im2d;
extern "C" int im3d;

void
im2DRenderPrimitive(PrimitiveType primType, void *vertices, int32 numVertices)
{
	im2d++;
	if(im2dOverrideShader)
		im2dOverrideShader->use();
	else
		im2dShader->use();
	
	im2DSetXform();
	flushCache();
	
	glBindBuffer(GL_ARRAY_BUFFER, 0); 
	
	glEnableVertexAttribArray(ATTRIB_POS);
	glVertexAttribPointer(ATTRIB_POS, 4, GL_FLOAT, GL_FALSE, sizeof(Im2DVertex), (uint8*)vertices + 0);
	
	glEnableVertexAttribArray(ATTRIB_COLOR);
	glVertexAttribPointer(ATTRIB_COLOR, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(Im2DVertex), (uint8*)vertices + OFFSET_OF(Im2DVertex, r));
	
	glEnableVertexAttribArray(ATTRIB_TEXCOORDS0);
	glVertexAttribPointer(ATTRIB_TEXCOORDS0, 2, GL_FLOAT, GL_FALSE, sizeof(Im2DVertex), (uint8*)vertices + OFFSET_OF(Im2DVertex, u));
	
	glDisableVertexAttribArray(ATTRIB_NORMAL);
	glDisableVertexAttribArray(ATTRIB_WEIGHTS);
	glDisableVertexAttribArray(ATTRIB_INDICES);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS1);
	
	glDrawArrays(primTypeMap[primType], 0, numVertices);
	
	glDisableVertexAttribArray(ATTRIB_POS);
	glDisableVertexAttribArray(ATTRIB_COLOR);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS0);
}

void
im2DRenderIndexedPrimitive(PrimitiveType primType,
	void *vertices, int32 numVertices,
	void *indices, int32 numIndices)
{
	im2d++;
	if(im2dOverrideShader)
		im2dOverrideShader->use();
	else
		im2dShader->use();

	im2DSetXform();
	flushCache();

	glBindBuffer(GL_ARRAY_BUFFER, 0);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

	glEnableVertexAttribArray(ATTRIB_POS);
	glVertexAttribPointer(ATTRIB_POS, 4, GL_FLOAT, GL_FALSE, sizeof(Im2DVertex), (uint8*)vertices + 0);
	
	glEnableVertexAttribArray(ATTRIB_COLOR);
	glVertexAttribPointer(ATTRIB_COLOR, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(Im2DVertex), (uint8*)vertices + OFFSET_OF(Im2DVertex, r));
	
	glEnableVertexAttribArray(ATTRIB_TEXCOORDS0);
	glVertexAttribPointer(ATTRIB_TEXCOORDS0, 2, GL_FLOAT, GL_FALSE, sizeof(Im2DVertex), (uint8*)vertices + OFFSET_OF(Im2DVertex, u));

	glDisableVertexAttribArray(ATTRIB_NORMAL);
	glDisableVertexAttribArray(ATTRIB_WEIGHTS);
	glDisableVertexAttribArray(ATTRIB_INDICES);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS1);

	glDrawElements(primTypeMap[primType], numIndices, GL_UNSIGNED_SHORT, indices);

	glDisableVertexAttribArray(ATTRIB_POS);
	glDisableVertexAttribArray(ATTRIB_COLOR);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS0);
}


// Im3D


static Shader *im3dShader;
static int32 num3DVertices;
static void* currentIm3dVertices;

void
openIm3D(void)
{
#include "shaders/im3d_gl.inc"
#include "shaders/simple_fs_gl.inc"
	const char *vs[] = { shaderDecl, header_vert_src, im3d_vert_src, nil };
	const char *fs[] = { shaderDecl, header_frag_src, simple_frag_src, nil };
	im3dShader = Shader::create(vs, fs);
	assert(im3dShader);
	currentIm3dVertices = nil;
}

void
closeIm3D(void)
{
	im3dShader->destroy();
	im3dShader = nil;
}

void
im3DTransform(void *vertices, int32 numVertices, Matrix *world, uint32 flags)
{
	if(world == nil){
		static Matrix ident;
		ident.setIdentity();
		world = &ident;
	}
	setWorldMatrix(world);
	im3dShader->use();

	if((flags & im3d::VERTEXUV) == 0)
		SetRenderStatePtr(TEXTURERASTER, nil);

	glBindBuffer(GL_ARRAY_BUFFER, 0);
	currentIm3dVertices = vertices;
	num3DVertices = numVertices;
}

void
im3DRenderPrimitive(PrimitiveType primType)
{
	im3d++;
	flushCache();

	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

	glEnableVertexAttribArray(ATTRIB_POS);
	glVertexAttribPointer(ATTRIB_POS, 3, GL_FLOAT, GL_FALSE, sizeof(Im3DVertex), (uint8*)currentIm3dVertices + 0);
	
	glEnableVertexAttribArray(ATTRIB_COLOR);
	glVertexAttribPointer(ATTRIB_COLOR, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(Im3DVertex), (uint8*)currentIm3dVertices + OFFSET_OF(Im3DVertex, r));
	
	glEnableVertexAttribArray(ATTRIB_TEXCOORDS0);
	glVertexAttribPointer(ATTRIB_TEXCOORDS0, 2, GL_FLOAT, GL_FALSE, sizeof(Im3DVertex), (uint8*)currentIm3dVertices + OFFSET_OF(Im3DVertex, u));

	glDisableVertexAttribArray(ATTRIB_NORMAL);
	glDisableVertexAttribArray(ATTRIB_WEIGHTS);
	glDisableVertexAttribArray(ATTRIB_INDICES);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS1);

	glDrawArrays(primTypeMap[primType], 0, num3DVertices);

	glDisableVertexAttribArray(ATTRIB_POS);
	glDisableVertexAttribArray(ATTRIB_COLOR);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS0);
}

void
im3DRenderIndexedPrimitive(PrimitiveType primType, void *indices, int32 numIndices)
{
	im3d++;
	flushCache();

	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

	glEnableVertexAttribArray(ATTRIB_POS);
	glVertexAttribPointer(ATTRIB_POS, 3, GL_FLOAT, GL_FALSE, sizeof(Im3DVertex), (uint8*)currentIm3dVertices + 0);
	
	glEnableVertexAttribArray(ATTRIB_COLOR);
	glVertexAttribPointer(ATTRIB_COLOR, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(Im3DVertex), (uint8*)currentIm3dVertices + OFFSET_OF(Im3DVertex, r));
	
	glEnableVertexAttribArray(ATTRIB_TEXCOORDS0);
	glVertexAttribPointer(ATTRIB_TEXCOORDS0, 2, GL_FLOAT, GL_FALSE, sizeof(Im3DVertex), (uint8*)currentIm3dVertices + OFFSET_OF(Im3DVertex, u));

	glDisableVertexAttribArray(ATTRIB_NORMAL);
	glDisableVertexAttribArray(ATTRIB_WEIGHTS);
	glDisableVertexAttribArray(ATTRIB_INDICES);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS1);

	glDrawElements(primTypeMap[primType], numIndices, GL_UNSIGNED_SHORT, indices);

	glDisableVertexAttribArray(ATTRIB_POS);
	glDisableVertexAttribArray(ATTRIB_COLOR);
	glDisableVertexAttribArray(ATTRIB_TEXCOORDS0);
}

void
im3DEnd(void)
{
}

}
}

#endif
