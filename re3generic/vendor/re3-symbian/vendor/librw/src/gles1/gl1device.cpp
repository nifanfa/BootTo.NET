#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

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
#ifdef RE3_GENERIC
#include "re3generic_renderer.h"
#endif

#define PLUGIN_ID 0

namespace rw {
namespace gles1 {

GlGlobals glGlobals;
#ifdef RE3_GENERIC
static RG_SoftwareRenderer *softwareRenderer;
#endif

// terrible hack for GLES
bool32 needToReadBackTextures;

float32 alphaRef;

struct UniformState
{
	float32 fogStart;
	float32 fogEnd;
	RGBAf   fogColor;
};

struct UniformScene
{
	float32 proj[16];
	float32 view[16];
};

#define MAX_LIGHTS 8

struct UniformObject
{
	RawMatrix    world;
	RGBAf        ambLight;
	struct {
		float type;
		float radius;
		float minusCosAngle;
		float hardSpot;
	} lightParams[MAX_LIGHTS];
	V4d lightPosition[MAX_LIGHTS];
	V4d lightDirection[MAX_LIGHTS];
	RGBAf lightColor[MAX_LIGHTS];
};



static GLuint vao;
static GLuint whitetex;
static UniformState uniformState;
static UniformScene uniformScene;
static UniformObject uniformObject;

int32 u_matColor;
int32 u_surfProps;

static bool32 stateDirty = 1;
static bool32 sceneDirty = 1;
static bool32 objectDirty = 1;

struct RwRasterStateCache {
	Raster *raster;
	Texture::Addressing addressingU;
	Texture::Addressing addressingV;
	Texture::FilterMode filter;
};

#define MAXNUMSTAGES 8

// cached RW render states
struct RwStateCache {
	bool32 vertexAlpha;
	uint32 alphaTestEnable;
	uint32 alphaFunc;
	bool32 textureAlpha;
	bool32 blendEnable;
	uint32 srcblend, destblend;
	uint32 zwrite;
	uint32 ztest;
	uint32 cullmode;
	uint32 stencilenable;
	uint32 stencilpass;
	uint32 stencilfail;
	uint32 stencilzfail;
	uint32 stencilfunc;
	uint32 stencilref;
	uint32 stencilmask;
	uint32 stencilwritemask;
	uint32 fogEnable;
	float32 fogStart;
	float32 fogEnd;

	// emulation of PS2 GS
	bool32 gsalpha;
	uint32 gsalpharef;

	RwRasterStateCache texstage[MAXNUMSTAGES];
};
static RwStateCache rwStateCache;

enum
{
	// actual gl states
	RWGL_BLEND,
	RWGL_SRCBLEND,
	RWGL_DESTBLEND,
	RWGL_DEPTHTEST,
	RWGL_DEPTHFUNC,
	RWGL_DEPTHMASK,
	RWGL_CULL,
	RWGL_CULLFACE,
	RWGL_STENCIL,
	RWGL_STENCILFUNC,
	RWGL_STENCILFAIL,
	RWGL_STENCILZFAIL,
	RWGL_STENCILPASS,
	RWGL_STENCILREF,
	RWGL_STENCILMASK,
	RWGL_STENCILWRITEMASK,

	// uniforms
	RWGL_ALPHAFUNC,
	RWGL_ALPHAREF,
	RWGL_FOG,
	RWGL_FOGSTART,
	RWGL_FOGEND,
	RWGL_FOGCOLOR,

	RWGL_NUM_STATES
};
static bool uniformStateDirty[RWGL_NUM_STATES];

struct GlState {
	bool32 blendEnable;
	uint32 srcblend, destblend;

	bool32 depthTest;
	uint32 depthFunc;

	uint32 depthMask;

	bool32 cullEnable;
	uint32 cullFace;

	bool32 stencilEnable;
	// glStencilFunc
	uint32 stencilFunc;
	uint32 stencilRef;
	uint32 stencilMask;
	// glStencilOp
	uint32 stencilPass;
	uint32 stencilFail;
	uint32 stencilZFail;
	// glStencilMask
	uint32 stencilWriteMask;
};
static GlState curGlState, oldGlState;

static int32 activeTexture;
static uint32 boundTexture[MAXNUMSTAGES];

static uint32 currentFramebuffer;

static uint32 blendMap[] = {
	GL_ZERO,	// actually invalid
	GL_ZERO,
	GL_ONE,
	GL_SRC_COLOR,
	GL_ONE_MINUS_SRC_COLOR,
	GL_SRC_ALPHA,
	GL_ONE_MINUS_SRC_ALPHA,
	GL_DST_ALPHA,
	GL_ONE_MINUS_DST_ALPHA,
	GL_DST_COLOR,
	GL_ONE_MINUS_DST_COLOR,
	GL_SRC_ALPHA_SATURATE,
};

static uint32 stencilOpMap[] = {
	GL_KEEP,	// actually invalid
	GL_KEEP,
	GL_ZERO,
	GL_REPLACE,
	GL_INCR,
	GL_DECR,
	GL_INVERT,
	GL_INCR,//_WRAP,
	GL_DECR//_WRAP
};

static uint32 stencilFuncMap[] = {
	GL_NEVER,	// actually invalid
	GL_NEVER,
	GL_LESS,
	GL_EQUAL,
	GL_LEQUAL,
	GL_GREATER,
	GL_NOTEQUAL,
	GL_GEQUAL,
	GL_ALWAYS
};

static float maxAnisotropy;

/*
 * GL state cache
 */

void
setGlRenderState(uint32 state, uint32 value)
{
	switch(state){
	case RWGL_BLEND: curGlState.blendEnable = value; break;
	case RWGL_SRCBLEND: curGlState.srcblend = value; break;
	case RWGL_DESTBLEND: curGlState.destblend = value; break;
	case RWGL_DEPTHTEST: curGlState.depthTest = value; break;
	case RWGL_DEPTHFUNC: curGlState.depthFunc = value; break;
	case RWGL_DEPTHMASK: curGlState.depthMask = value; break;
	case RWGL_CULL: curGlState.cullEnable = value; break;
	case RWGL_CULLFACE: curGlState.cullFace = value; break;
	case RWGL_STENCIL: curGlState.stencilEnable = value; break;
	case RWGL_STENCILFUNC: curGlState.stencilFunc = value; break;
	case RWGL_STENCILFAIL: curGlState.stencilFail = value; break;
	case RWGL_STENCILZFAIL: curGlState.stencilZFail = value; break;
	case RWGL_STENCILPASS: curGlState.stencilPass = value; break;
	case RWGL_STENCILREF: curGlState.stencilRef = value; break;
	case RWGL_STENCILMASK: curGlState.stencilMask = value; break;
	case RWGL_STENCILWRITEMASK: curGlState.stencilWriteMask = value; break;
	}
}

void
flushGlRenderState(void)
{
	if(oldGlState.blendEnable != curGlState.blendEnable){
		oldGlState.blendEnable = curGlState.blendEnable;
		(oldGlState.blendEnable ? glEnable : glDisable)(GL_BLEND);
	}

	if(oldGlState.srcblend != curGlState.srcblend ||
	   oldGlState.destblend != curGlState.destblend){
		oldGlState.srcblend = curGlState.srcblend;
		oldGlState.destblend = curGlState.destblend;
		glBlendFunc(oldGlState.srcblend, oldGlState.destblend);
	}

	if(oldGlState.depthTest != curGlState.depthTest){
		oldGlState.depthTest = curGlState.depthTest;
		(oldGlState.depthTest ? glEnable : glDisable)(GL_DEPTH_TEST);
	}
	if(oldGlState.depthFunc != curGlState.depthFunc){
		oldGlState.depthFunc = curGlState.depthFunc;
		glDepthFunc(oldGlState.depthFunc);
	}
	if(oldGlState.depthMask != curGlState.depthMask){
		oldGlState.depthMask = curGlState.depthMask;
		glDepthMask(oldGlState.depthMask);
	}

	if(oldGlState.stencilEnable != curGlState.stencilEnable){
		oldGlState.stencilEnable = curGlState.stencilEnable;
		(oldGlState.stencilEnable ? glEnable : glDisable)(GL_STENCIL_TEST);
	}
	if(oldGlState.stencilFunc != curGlState.stencilFunc ||
	   oldGlState.stencilRef != curGlState.stencilRef ||
	   oldGlState.stencilMask != curGlState.stencilMask){
		oldGlState.stencilFunc = curGlState.stencilFunc;
		oldGlState.stencilRef = curGlState.stencilRef;
		oldGlState.stencilMask = curGlState.stencilMask;
		glStencilFunc(oldGlState.stencilFunc, oldGlState.stencilRef, oldGlState.stencilMask);
	}
	if(oldGlState.stencilPass != curGlState.stencilPass ||
	   oldGlState.stencilFail != curGlState.stencilFail ||
	   oldGlState.stencilZFail != curGlState.stencilZFail){
		oldGlState.stencilPass = curGlState.stencilPass;
		oldGlState.stencilFail = curGlState.stencilFail;
		oldGlState.stencilZFail = curGlState.stencilZFail;
		glStencilOp(oldGlState.stencilFail, oldGlState.stencilZFail, oldGlState.stencilPass);
	}
	if(oldGlState.stencilWriteMask != curGlState.stencilWriteMask){
		oldGlState.stencilWriteMask = curGlState.stencilWriteMask;
		glStencilMask(oldGlState.stencilWriteMask);
	}

	if(oldGlState.cullEnable != curGlState.cullEnable){
		oldGlState.cullEnable = curGlState.cullEnable;
		(oldGlState.cullEnable ? glEnable : glDisable)(GL_CULL_FACE);
	}
	if(oldGlState.cullFace != curGlState.cullFace){
		oldGlState.cullFace = curGlState.cullFace;
		glCullFace(oldGlState.cullFace);
	}
}



void setAlphaBlend(bool32 enable){	
	if(rwStateCache.blendEnable != enable){
		rwStateCache.blendEnable = enable;
		setGlRenderState(RWGL_BLEND, enable);
	}
}

bool32
getAlphaBlend(void)
{
	return rwStateCache.blendEnable;
}

bool32 getAlphaTest(void) { return rwStateCache.alphaTestEnable; }

static void
setDepthTest(bool32 enable)
{
	if(rwStateCache.ztest != enable){
		rwStateCache.ztest = enable;
		if(rwStateCache.zwrite && !enable){
			// If we still want to write, enable but set mode to always
			setGlRenderState(RWGL_DEPTHTEST, true);
			setGlRenderState(RWGL_DEPTHFUNC, GL_ALWAYS);
		}else{
			setGlRenderState(RWGL_DEPTHTEST, rwStateCache.ztest);
			setGlRenderState(RWGL_DEPTHFUNC, GL_LEQUAL);
		}
	}
}

static void
setDepthWrite(bool32 enable)
{
	enable = enable ? GL_TRUE : GL_FALSE;
	if(rwStateCache.zwrite != enable){
		rwStateCache.zwrite = enable;
		if(enable && !rwStateCache.ztest){
			// Have to switch on ztest so writing can work
			setGlRenderState(RWGL_DEPTHTEST, true);
			setGlRenderState(RWGL_DEPTHFUNC, GL_ALWAYS);
		}
		setGlRenderState(RWGL_DEPTHMASK, rwStateCache.zwrite);
	}
}

static void setAlphaTest(bool32 enable)
{
	if(rwStateCache.alphaTestEnable != enable){
		rwStateCache.alphaTestEnable = enable;
		uniformStateDirty[RWGL_ALPHAFUNC] = true;
		stateDirty = 1;
	}
}

static void setAlphaTestFunction(uint32 function)
{
	if(rwStateCache.alphaFunc != function){
		rwStateCache.alphaFunc = function;
		uniformStateDirty[RWGL_ALPHAFUNC] = true;
		stateDirty = 1;
	}
}

void setVertexAlpha(bool32 enable)
{
	if(rwStateCache.vertexAlpha != enable){
		if(!rwStateCache.textureAlpha){
			setAlphaBlend(enable);
			setAlphaTest(enable);
		}
		rwStateCache.vertexAlpha = enable;
	}
}

static void
setActiveTexture(int32 n)
{
	if(activeTexture != n){
		activeTexture = n;
		glActiveTexture(GL_TEXTURE0+n);
	}
}

uint32
bindTexture(uint32 texid)
{
	uint32 prev = boundTexture[activeTexture];
	if(prev != texid){
		boundTexture[activeTexture] = texid;
		glBindTexture(GL_TEXTURE_2D, texid);
	}
	return prev;
}

void
bindFramebuffer(uint32 fbo)
{
	if(currentFramebuffer != fbo){
//		glBindFramebuffer(GL_FRAMEBUFFER, fbo);
		currentFramebuffer = fbo;
	}
}

static GLint filterConvMap_NoMIP[] = {
	0, GL_NEAREST, GL_LINEAR,
	   GL_NEAREST, GL_LINEAR,
	   GL_NEAREST, GL_LINEAR
};
static GLint filterConvMap_MIP[] = {
	0, GL_NEAREST, GL_LINEAR,
	   GL_NEAREST_MIPMAP_NEAREST, GL_LINEAR_MIPMAP_NEAREST,
	   GL_NEAREST_MIPMAP_LINEAR, GL_LINEAR_MIPMAP_LINEAR
};

static GLint addressConvMap[] = {
	0, GL_REPEAT, GL_REPEAT, //GL_MIRRORED_REPEAT,
#if defined(__SYMBIAN32__) || defined(RE3_GENERIC)
	// TODO
	GL_CLAMP_TO_EDGE, GL_CLAMP_TO_EDGE
#else
	GL_CLAMP_TO_EDGE, GL_CLAMP_TO_BORDER
#endif
};

static void
setFilterMode(uint32 stage, int32 filter, int32 maxAniso = 1)
{
	if(rwStateCache.texstage[stage].filter != (Texture::FilterMode)filter){
		rwStateCache.texstage[stage].filter = (Texture::FilterMode)filter;
		Raster *raster = rwStateCache.texstage[stage].raster;
		if(raster){
			Gl1Raster *natras = PLUGINOFFSET(Gl1Raster, rwStateCache.texstage[stage].raster, nativeRasterOffset);
			if(natras->filterMode != filter){
				setActiveTexture(stage);
//				if(natras->autogenMipmap || natras->numLevels > 1){
//					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filterConvMap_MIP[filter]);
//					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filterConvMap_NoMIP[filter]);
//				}else
				{
					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filterConvMap_NoMIP[filter]);
					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filterConvMap_NoMIP[filter]);
				}
				natras->filterMode = filter;
			}
			if(natras->maxAnisotropy != maxAniso){
				setActiveTexture(stage);
//				glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAX_ANISOTROPY_EXT, (float)maxAniso);
				natras->maxAnisotropy = maxAniso;
			}
		}
	}
}

static void
setAddressU(uint32 stage, int32 addressing)
{
	if(rwStateCache.texstage[stage].addressingU != (Texture::Addressing)addressing){
		rwStateCache.texstage[stage].addressingU = (Texture::Addressing)addressing;
		Raster *raster = rwStateCache.texstage[stage].raster;
		if(raster){
			Gl1Raster *natras = PLUGINOFFSET(Gl1Raster, raster, nativeRasterOffset);
			if(natras->addressU == addressing){
				setActiveTexture(stage);
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, addressConvMap[addressing]);
				natras->addressU = addressing;
			}
		}
	}
}

static void
setAddressV(uint32 stage, int32 addressing)
{
	if(rwStateCache.texstage[stage].addressingV != (Texture::Addressing)addressing){
		rwStateCache.texstage[stage].addressingV = (Texture::Addressing)addressing;
		Raster *raster = rwStateCache.texstage[stage].raster;
		if(raster){
			Gl1Raster *natras = PLUGINOFFSET(Gl1Raster, rwStateCache.texstage[stage].raster, nativeRasterOffset);
			if(natras->addressV == addressing){
				setActiveTexture(stage);
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, addressConvMap[addressing]);
				natras->addressV = addressing;
			}
		}
	}
}

static void
setRasterStageOnly(uint32 stage, Raster *raster)
{
	bool32 alpha;
	if(raster != rwStateCache.texstage[stage].raster){
		rwStateCache.texstage[stage].raster = raster;
		setActiveTexture(stage);
		if(raster){
			assert(raster->platform == PLATFORM_GLES1);
			Gl1Raster *natras = PLUGINOFFSET(Gl1Raster, raster, nativeRasterOffset);
			glEnable(GL_TEXTURE_2D);
			bindTexture(natras->texid);

			rwStateCache.texstage[stage].filter = (rw::Texture::FilterMode)natras->filterMode;
			rwStateCache.texstage[stage].addressingU = (rw::Texture::Addressing)natras->addressU;
			rwStateCache.texstage[stage].addressingV = (rw::Texture::Addressing)natras->addressV;

			alpha = natras->hasAlpha;
		}else{
			glDisable(GL_TEXTURE_2D);
			bindTexture(whitetex);
			alpha = 0;
		}

		if(stage == 0){
			if(alpha != rwStateCache.textureAlpha){
				rwStateCache.textureAlpha = alpha;
				if(!rwStateCache.vertexAlpha){
					setAlphaBlend(alpha);
					setAlphaTest(alpha);
				}
			}
		}
	}
}

static void
setRasterStage(uint32 stage, Raster *raster)
{
	bool32 alpha;
	if(raster != rwStateCache.texstage[stage].raster){
		rwStateCache.texstage[stage].raster = raster;
		setActiveTexture(stage);
		if(raster){
			assert(raster->platform == PLATFORM_GLES1);
			Gl1Raster *natras = PLUGINOFFSET(Gl1Raster, raster, nativeRasterOffset);
			glEnable(GL_TEXTURE_2D);
			bindTexture(natras->texid);
			uint32 filter = rwStateCache.texstage[stage].filter;
			uint32 addrU = rwStateCache.texstage[stage].addressingU;
			uint32 addrV = rwStateCache.texstage[stage].addressingV;
			if(natras->filterMode != filter){
//				if(natras->autogenMipmap || natras->numLevels > 1){
//					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filterConvMap_MIP[filter]);
//					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filterConvMap_NoMIP[filter]);
//				}else
				{
					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filterConvMap_NoMIP[filter]);
					glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filterConvMap_NoMIP[filter]);
				}
				natras->filterMode = filter;
			}
			if(natras->addressU != addrU){
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, addressConvMap[addrU]);
				natras->addressU = addrU;
			}
			if(natras->addressV != addrV){
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, addressConvMap[addrV]);
				natras->addressV = addrV;
			}
			alpha = natras->hasAlpha;
		}else{
			glDisable(GL_TEXTURE_2D);
			bindTexture(whitetex);
			alpha = 0;
		}

		if(stage == 0){
			if(alpha != rwStateCache.textureAlpha){
				rwStateCache.textureAlpha = alpha;
				if(!rwStateCache.vertexAlpha){
					setAlphaBlend(alpha);
					setAlphaTest(alpha);
				}
			}
		}
	}
}

void
evictRaster(Raster *raster)
{
	int i;
	for(i = 0; i < MAXNUMSTAGES; i++){
		//assert(rwStateCache.texstage[i].raster != raster);
		if(rwStateCache.texstage[i].raster != raster)
			continue;
		setRasterStage(i, nil);
	}
}

void
setTexture(int32 stage, Texture *tex)
{
	if(tex == nil || tex->raster == nil){
		setRasterStage(stage, nil);
		return;
	}
	setRasterStageOnly(stage, tex->raster);
	setFilterMode(stage, tex->getFilter(), tex->getMaxAnisotropy());
	setAddressU(stage, tex->getAddressU());
	setAddressV(stage, tex->getAddressV());
}

static void
setRenderState(int32 state, void *pvalue)
{
	uint32 value = (uint32)(uintptr)pvalue;
	switch(state){
	case TEXTURERASTER:
		setRasterStage(0, (Raster*)pvalue);
		break;
	case TEXTUREADDRESS:
		setAddressU(0, value);
		setAddressV(0, value);
		break;
	case TEXTUREADDRESSU:
		setAddressU(0, value);
		break;
	case TEXTUREADDRESSV:
		setAddressV(0, value);
		break;
	case TEXTUREFILTER:
		setFilterMode(0, value);
		break;
	case VERTEXALPHA:
		setVertexAlpha(value);
		break;
	case SRCBLEND:
		if(rwStateCache.srcblend != value){
			rwStateCache.srcblend = value;
			setGlRenderState(RWGL_SRCBLEND, blendMap[rwStateCache.srcblend]);
		}
		break;
	case DESTBLEND:
		if(rwStateCache.destblend != value){
			rwStateCache.destblend = value;
			setGlRenderState(RWGL_DESTBLEND, blendMap[rwStateCache.destblend]);
		}
		break;
	case ZTESTENABLE:
		setDepthTest(value);
		break;
	case ZWRITEENABLE:
		setDepthWrite(value);
		break;
	case FOGENABLE:
		if(rwStateCache.fogEnable != value){
			rwStateCache.fogEnable = value;
			uniformStateDirty[RWGL_FOG] = true;
			stateDirty = 1;
		}
		break;
	case FOGCOLOR:
		// no cache check here...too lazy
		RGBA c;
		c.red = value;
		c.green = value>>8;
		c.blue = value>>16;
		c.alpha = value>>24;
		convColor(&uniformState.fogColor, &c);
		uniformStateDirty[RWGL_FOGCOLOR] = true;
		stateDirty = 1;
		break;
	case CULLMODE:
		if(rwStateCache.cullmode != value){
			rwStateCache.cullmode = value;
			if(rwStateCache.cullmode == CULLNONE)
				setGlRenderState(RWGL_CULL, false);
			else{
				setGlRenderState(RWGL_CULL, true);
				setGlRenderState(RWGL_CULLFACE, rwStateCache.cullmode == CULLBACK ? GL_BACK : GL_FRONT);
			}
		}
		break;

	case STENCILENABLE:
		if(rwStateCache.stencilenable != value){
			rwStateCache.stencilenable = value;
			setGlRenderState(RWGL_STENCIL, value);
		}
		break;
	case STENCILFAIL:
		if(rwStateCache.stencilfail != value){
			rwStateCache.stencilfail = value;
			setGlRenderState(RWGL_STENCILFAIL, stencilOpMap[value]);
		}
		break;
	case STENCILZFAIL:
		if(rwStateCache.stencilzfail != value){
			rwStateCache.stencilzfail = value;
			setGlRenderState(RWGL_STENCILZFAIL, stencilOpMap[value]);
		}
		break;
	case STENCILPASS:
		if(rwStateCache.stencilpass != value){
			rwStateCache.stencilpass = value;
			setGlRenderState(RWGL_STENCILPASS, stencilOpMap[value]);
		}
		break;
	case STENCILFUNCTION:
		if(rwStateCache.stencilfunc != value){
			rwStateCache.stencilfunc = value;
			setGlRenderState(RWGL_STENCILFUNC, stencilFuncMap[value]);
		}
		break;
	case STENCILFUNCTIONREF:
		if(rwStateCache.stencilref != value){
			rwStateCache.stencilref = value;
			setGlRenderState(RWGL_STENCILREF, value);
		}
		break;
	case STENCILFUNCTIONMASK:
		if(rwStateCache.stencilmask != value){
			rwStateCache.stencilmask = value;
			setGlRenderState(RWGL_STENCILMASK, value);
		}
		break;
	case STENCILFUNCTIONWRITEMASK:
		if(rwStateCache.stencilwritemask != value){
			rwStateCache.stencilwritemask = value;
			setGlRenderState(RWGL_STENCILWRITEMASK, value);
		}
		break;

	case ALPHATESTFUNC:
		setAlphaTestFunction(value);
		break;
	case ALPHATESTREF:
		if(alphaRef != value/255.0f){
			alphaRef = value/255.0f;
			uniformStateDirty[RWGL_ALPHAREF] = true;
			stateDirty = 1;
		}
		break;
	case GSALPHATEST:
		rwStateCache.gsalpha = value;
		break;
	case GSALPHATESTREF:
		rwStateCache.gsalpharef = value;
	}
}

static void*
getRenderState(int32 state)
{
	uint32 val;
	RGBA rgba;
	switch(state){
	case TEXTURERASTER:
		return rwStateCache.texstage[0].raster;
	case TEXTUREADDRESS:
		if(rwStateCache.texstage[0].addressingU == rwStateCache.texstage[0].addressingV)
			val = rwStateCache.texstage[0].addressingU;
		else
			val = 0;	// invalid
		break;
	case TEXTUREADDRESSU:
		val = rwStateCache.texstage[0].addressingU;
		break;
	case TEXTUREADDRESSV:
		val = rwStateCache.texstage[0].addressingV;
		break;
	case TEXTUREFILTER:
		val = rwStateCache.texstage[0].filter;
		break;

	case VERTEXALPHA:
		val = rwStateCache.vertexAlpha;
		break;
	case SRCBLEND:
		val = rwStateCache.srcblend;
		break;
	case DESTBLEND:
		val = rwStateCache.destblend;
		break;
	case ZTESTENABLE:
		val = rwStateCache.ztest;
		break;
	case ZWRITEENABLE:
		val = rwStateCache.zwrite;
		break;
	case FOGENABLE:
		val = rwStateCache.fogEnable;
		break;
	case FOGCOLOR:
		convColor(&rgba, &uniformState.fogColor);
		val = RWRGBAINT(rgba.red, rgba.green, rgba.blue, rgba.alpha);
		break;
	case CULLMODE:
		val = rwStateCache.cullmode;
		break;

	case STENCILENABLE:
		val = rwStateCache.stencilenable;
		break;
	case STENCILFAIL:
		val = rwStateCache.stencilfail;
		break;
	case STENCILZFAIL:
		val = rwStateCache.stencilzfail;
		break;
	case STENCILPASS:
		val = rwStateCache.stencilpass;
		break;
	case STENCILFUNCTION:
		val = rwStateCache.stencilfunc;
		break;
	case STENCILFUNCTIONREF:
		val = rwStateCache.stencilref;
		break;
	case STENCILFUNCTIONMASK:
		val = rwStateCache.stencilmask;
		break;
	case STENCILFUNCTIONWRITEMASK:
		val = rwStateCache.stencilwritemask;
		break;

	case ALPHATESTFUNC:
		val = rwStateCache.alphaFunc;
		break;
	case ALPHATESTREF:
		val = (uint32)(alphaRef*255.0f);
		break;
	case GSALPHATEST:
		val = rwStateCache.gsalpha;
		break;
	case GSALPHATESTREF:
		val = rwStateCache.gsalpharef;
		break;
	default:
		val = 0;
	}
	return (void*)(uintptr)val;
}

static void
resetRenderState(void)
{	
	rwStateCache.alphaFunc = ALPHAGREATEREQUAL;
	alphaRef = 10.0f/255.0f;
	uniformState.fogStart = 0.0f;
	uniformState.fogEnd = 0.0f;
	uniformState.fogColor.red = 1.0f;
	uniformState.fogColor.green = 1.0f;
	uniformState.fogColor.blue = 1.0f;
	uniformState.fogColor.alpha = 1.0f;
	rwStateCache.gsalpha = 0;
	rwStateCache.gsalpharef = 128;
	stateDirty = 1;

	rwStateCache.vertexAlpha = 0;
	rwStateCache.textureAlpha = 0;
	rwStateCache.alphaTestEnable = 0;

	memset(&oldGlState, 0xFE, sizeof(oldGlState));

	rwStateCache.blendEnable = 0;
	setGlRenderState(RWGL_BLEND, false);
	rwStateCache.srcblend = BLENDSRCALPHA;
	rwStateCache.destblend = BLENDINVSRCALPHA;
	setGlRenderState(RWGL_SRCBLEND, blendMap[rwStateCache.srcblend]);
	setGlRenderState(RWGL_DESTBLEND, blendMap[rwStateCache.destblend]);

	rwStateCache.zwrite = GL_TRUE;
	setGlRenderState(RWGL_DEPTHMASK, rwStateCache.zwrite);

	rwStateCache.ztest = 1;
	setGlRenderState(RWGL_DEPTHTEST, true);
	setGlRenderState(RWGL_DEPTHFUNC, GL_LEQUAL);

	rwStateCache.cullmode = CULLNONE;
	setGlRenderState(RWGL_CULL, false);
	setGlRenderState(RWGL_CULLFACE, GL_BACK);

	rwStateCache.stencilenable = 0;
	setGlRenderState(RWGL_STENCIL, GL_FALSE);
	rwStateCache.stencilfail = STENCILKEEP;
	setGlRenderState(RWGL_STENCILFAIL, GL_KEEP);
	rwStateCache.stencilzfail = STENCILKEEP;
	setGlRenderState(RWGL_STENCILZFAIL, GL_KEEP);
	rwStateCache.stencilpass = STENCILKEEP;
	setGlRenderState(RWGL_STENCILPASS, GL_KEEP);
	rwStateCache.stencilfunc = STENCILALWAYS;
	setGlRenderState(RWGL_STENCILFUNC, GL_ALWAYS);
	rwStateCache.stencilref = 0;
	setGlRenderState(RWGL_STENCILREF, 0);
	rwStateCache.stencilmask = 0xFFFFFFFF;
	setGlRenderState(RWGL_STENCILMASK, 0xFFFFFFFF);
	rwStateCache.stencilwritemask = 0xFFFFFFFF;
	setGlRenderState(RWGL_STENCILWRITEMASK, 0xFFFFFFFF);

	activeTexture = -1;
	for(int i = 0; i < MAXNUMSTAGES; i++){
		setActiveTexture(i);
		bindTexture(whitetex);
		glDisable(GL_TEXTURE_2D);
	}
	setActiveTexture(0);
	
	glDisable(GL_LIGHTING);
	glDisable(GL_FOG);
	glDisable(GL_ALPHA_TEST);
}

void
setWorldMatrix(Matrix *mat)
{
	convMatrix(&uniformObject.world, mat);
	glMatrixMode(GL_MODELVIEW);
	glLoadMatrixf(uniformScene.view);
	glMultMatrixf((float*)&uniformObject.world);
	objectDirty = 1;
}

int32
setLights(WorldLights *lightData)
{
	//sin luces. menos codigo
    glDisable(GL_LIGHTING);
    return 0;

	/*int i, n;
	Light *l;
	int32 bits = 0;

	glLightModelfv(GL_LIGHT_MODEL_AMBIENT, (GLfloat*)&lightData->ambient);

	if(lightData->numAmbients)
		bits |= VSLIGHT_AMBIENT;

	n = 0;
	for(i = 0; i < MAX_LIGHTS; i++)
		glDisable(GL_LIGHT0 + i);

	for(i = 0; i < lightData->numDirectionals && i < 8; i++){
		l = lightData->directionals[i];
		glEnable(GL_LIGHT0 + n);
		GLfloat dir[4] = { -l->getFrame()->getLTM()->at.x, -l->getFrame()->getLTM()->at.y, -l->getFrame()->getLTM()->at.z, 0.0f };
		glLightfv(GL_LIGHT0 + n, GL_POSITION, dir);
		glLightfv(GL_LIGHT0 + n, GL_DIFFUSE, (GLfloat*)&l->color);
		glLightfv(GL_LIGHT0 + n, GL_SPECULAR, (GLfloat*)&l->color);
		bits |= VSLIGHT_DIRECT;
		n++;
		if(n >= MAX_LIGHTS) goto out;
	}

	for(i = 0; i < lightData->numLocals; i++){
		Light *l = lightData->locals[i];

		switch(l->getType()){
		case Light::POINT:
			glEnable(GL_LIGHT0 + n);
			{
				GLfloat pos[4] = { l->getFrame()->getLTM()->pos.x, l->getFrame()->getLTM()->pos.y, l->getFrame()->getLTM()->pos.z, 1.0f };
				glLightfv(GL_LIGHT0 + n, GL_POSITION, pos);
				glLightfv(GL_LIGHT0 + n, GL_DIFFUSE, (GLfloat*)&l->color);
				glLightf(GL_LIGHT0 + n, GL_CONSTANT_ATTENUATION, 1.0f);
				glLightf(GL_LIGHT0 + n, GL_LINEAR_ATTENUATION, l->radius > 0.0f ? 1.0f / l->radius : 0.0f);
			}
			bits |= VSLIGHT_POINT;
			n++;
			if(n >= MAX_LIGHTS) goto out;
			break;
		case Light::SPOT:
		case Light::SOFTSPOT:
			glEnable(GL_LIGHT0 + n);
			{
				GLfloat pos[4] = { l->getFrame()->getLTM()->pos.x, l->getFrame()->getLTM()->pos.y, l->getFrame()->getLTM()->pos.z, 1.0f };
				GLfloat dir[4] = { l->getFrame()->getLTM()->at.x, l->getFrame()->getLTM()->at.y, l->getFrame()->getLTM()->at.z, 0.0f };
				glLightfv(GL_LIGHT0 + n, GL_POSITION, pos);
				glLightfv(GL_LIGHT0 + n, GL_SPOT_DIRECTION, dir);
				glLightfv(GL_LIGHT0 + n, GL_DIFFUSE, (GLfloat*)&l->color);
				glLightf(GL_LIGHT0 + n, GL_SPOT_CUTOFF, 45.0f);
			}
			bits |= VSLIGHT_SPOT;
			n++;
			if(n >= MAX_LIGHTS) goto out;
			break;
		}
	}

out:
	if(n > 0 || lightData->numAmbients > 0)
		glEnable(GL_LIGHTING);
	else
		glDisable(GL_LIGHTING);

	objectDirty = 1;
	return bits;*/
}

void
setProjectionMatrix(float32 *mat)
{
	memcpy(&uniformScene.proj, mat, 64);
	glMatrixMode(GL_PROJECTION);
	glLoadMatrixf(uniformScene.proj);
	sceneDirty = 1;
}

void
setViewMatrix(float32 *mat)
{
	memcpy(&uniformScene.view, mat, 64);
	glMatrixMode(GL_MODELVIEW);
	glLoadMatrixf(uniformScene.view);
	glMultMatrixf((float*)&uniformObject.world);
	sceneDirty = 1;
}

void
setMaterial(const RGBA &color, const SurfaceProperties &surfaceprops, float extraSurfProp)
{
	rw::RGBAf col;
	convColor(&col, &color);
	glColor4f(col.red, col.green, col.blue, col.alpha);
	
	GLfloat ambient[4] = { col.red * surfaceprops.ambient, col.green * surfaceprops.ambient, col.blue * surfaceprops.ambient, col.alpha };
	GLfloat diffuse[4] = { col.red * surfaceprops.diffuse, col.green * surfaceprops.diffuse, col.blue * surfaceprops.diffuse, col.alpha };
	GLfloat specular[4] = { surfaceprops.specular, surfaceprops.specular, surfaceprops.specular, 1.0f };
	
	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, ambient);
	glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, diffuse);
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, specular);
}

void flushCache(void){
	flushGlRenderState();

	if(uniformStateDirty[RWGL_ALPHAFUNC] || uniformStateDirty[RWGL_ALPHAREF]){
		if (rwStateCache.alphaTestEnable) {
			glEnable(GL_ALPHA_TEST);
			int func = GL_ALWAYS;
			switch(rwStateCache.alphaFunc){
			case ALPHAALWAYS:
				func = GL_ALWAYS;
				break;
			case ALPHAGREATEREQUAL:
				func = GL_GEQUAL;
				break;
			case ALPHALESS:
				func = GL_LESS;
				break;
			}
			glAlphaFunc(func, alphaRef);
		} else {
			glDisable(GL_ALPHA_TEST);
		}
		uniformStateDirty[RWGL_ALPHAFUNC] = false;
		uniformStateDirty[RWGL_ALPHAREF] = false;
	}

	if(uniformStateDirty[RWGL_FOG] ||
	   uniformStateDirty[RWGL_FOGSTART] ||
	   uniformStateDirty[RWGL_FOGEND] ||
	   uniformStateDirty[RWGL_FOGCOLOR]){
		glDisable(GL_FOG);
		if (rwStateCache.fogEnable) {
			glEnable(GL_FOG);
			glFogf(GL_FOG_MODE, GL_LINEAR);
			glFogf(GL_FOG_START, rwStateCache.fogStart);
			glFogf(GL_FOG_END, rwStateCache.fogEnd);
			GLfloat fcolor[4] = { uniformState.fogColor.red, uniformState.fogColor.green, uniformState.fogColor.blue, uniformState.fogColor.alpha };
			glFogfv(GL_FOG_COLOR, fcolor);
		} else {
			glDisable(GL_FOG);
		}
		uniformStateDirty[RWGL_FOG] = false;
		uniformStateDirty[RWGL_FOGSTART] = false;
		uniformStateDirty[RWGL_FOGEND] = false;
		uniformStateDirty[RWGL_FOGCOLOR] = false;
	}
}

static void
setFrameBuffer(Camera *cam)
{
	Raster *fbuf = cam->frameBuffer->parent;
	Raster *zbuf = cam->zBuffer->parent;
	assert(fbuf);

	Gl1Raster *natfb = PLUGINOFFSET(Gl1Raster, fbuf, nativeRasterOffset);
	Gl1Raster *natzb = PLUGINOFFSET(Gl1Raster, zbuf, nativeRasterOffset);
	assert(fbuf->type == Raster::CAMERA || fbuf->type == Raster::CAMERATEXTURE);

	// Have to make sure depth buffer is attached to FB's fbo
	bindFramebuffer(natfb->fbo);
	if (zbuf) {
		if(natfb->fboMate != zbuf){
//			glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, GETGL3RASTEREXT(zbuf)->texid);
			natfb->fboMate = zbuf;
		}
	} else if(natfb->fboMate) {
//		glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, 0);
		natfb->fboMate = nil;
	}
}

static Rect
getFramebufferRect(Raster *frameBuffer)
{
	Rect r;
	Raster *fb = frameBuffer->parent;
	if(fb->type == Raster::CAMERA){
		r.w = glGlobals.modes[0].width;
		r.h = glGlobals.modes[0].height;
	}else{
		r.w = fb->width;
		r.h = fb->height;
	}
	r.x = 0;
	r.y = 0;

	// Got a subraster
	if(frameBuffer != fb){
		r.x = frameBuffer->offsetX;
		// GL y offset is from bottom
		r.y = r.h - frameBuffer->height - frameBuffer->offsetY;
		r.w = frameBuffer->width;
		r.h = frameBuffer->height;
	}

	return r;
}

static void
setViewport(Raster *frameBuffer)
{
	Rect r = getFramebufferRect(frameBuffer);
	if(r.w != glGlobals.presentWidth || r.h != glGlobals.presentHeight ||
	   r.x != glGlobals.presentOffX || r.y != glGlobals.presentOffY){
		glViewport(r.x, r.y, r.w, r.h);
		glGlobals.presentWidth = r.w;
		glGlobals.presentHeight = r.h;
		glGlobals.presentOffX = r.x;
		glGlobals.presentOffY = r.y;
	}
}

static void
beginUpdate(Camera *cam)
{
	float view[16], proj[16];
	// View Matrix
	Matrix inv;
	Matrix::invert(&inv, cam->getFrame()->getLTM());
	// Since we're looking into positive Z,
	// flip X to ge a left handed view space.
	view[0]  = -inv.right.x;
	view[1]  =  inv.right.y;
	view[2]  =  inv.right.z;
	view[3]  =  0.0f;
	view[4]  = -inv.up.x;
	view[5]  =  inv.up.y;
	view[6]  =  inv.up.z;
	view[7]  =  0.0f;
	view[8]  =  -inv.at.x;
	view[9]  =   inv.at.y;
	view[10] =  inv.at.z;
	view[11] =  0.0f;
	view[12] = -inv.pos.x;
	view[13] =  inv.pos.y;
	view[14] =  inv.pos.z;
	view[15] =  1.0f;
	memcpy(&cam->devView, &view, sizeof(RawMatrix));
	setViewMatrix(view);

	// Projection Matrix
	float32 invwx = 1.0f/cam->viewWindow.x;
	float32 invwy = 1.0f/cam->viewWindow.y;
	float32 invz = 1.0f/(cam->farPlane-cam->nearPlane);

	proj[0] = invwx;
	proj[1] = 0.0f;
	proj[2] = 0.0f;
	proj[3] = 0.0f;

	proj[4] = 0.0f;
	proj[5] = invwy;
	proj[6] = 0.0f;
	proj[7] = 0.0f;

	proj[8] = cam->viewOffset.x*invwx;
	proj[9] = cam->viewOffset.y*invwy;
	proj[12] = -proj[8];
	proj[13] = -proj[9];
	if(cam->projection == Camera::PERSPECTIVE){
		proj[10] = (cam->farPlane+cam->nearPlane)*invz;
		proj[11] = 1.0f;

		proj[14] = -2.0f*cam->nearPlane*cam->farPlane*invz;
		proj[15] = 0.0f;
	}else{
		proj[10] = 2.0f*invz;
		proj[11] = 0.0f;

		proj[14] = -(cam->farPlane+cam->nearPlane)*invz;
		proj[15] = 1.0f;
	}
	memcpy(&cam->devProj, &proj, sizeof(RawMatrix));
	setProjectionMatrix(proj);

	if(rwStateCache.fogStart != cam->fogPlane){
		rwStateCache.fogStart = cam->fogPlane;
		uniformStateDirty[RWGL_FOGSTART] = true;
		stateDirty = 1;
	}
	if(rwStateCache.fogEnd != cam->farPlane){
		rwStateCache.fogEnd = cam->farPlane;
		uniformStateDirty[RWGL_FOGEND] = true;
		stateDirty = 1;
	}

	setFrameBuffer(cam);

	setViewport(cam->frameBuffer);
}

static void
endUpdate(Camera *cam)
{
}

static void
clearCamera(Camera *cam, RGBA *col, uint32 mode)
{
	RGBAf colf;
	GLbitfield mask;

	setFrameBuffer(cam);

	// make sure we're only clearing the part of the framebuffer
	// that is subrastered
	bool setScissor = cam->frameBuffer != cam->frameBuffer->parent;
	if(setScissor){
		Rect r = getFramebufferRect(cam->frameBuffer);
		glScissor(r.x, r.y, r.w, r.h);
		glEnable(GL_SCISSOR_TEST);
	}

	convColor(&colf, col);
	glClearColor(colf.red, colf.green, colf.blue, colf.alpha);
	mask = 0;
	if(mode & Camera::CLEARIMAGE)
		mask |= GL_COLOR_BUFFER_BIT;
	if(mode & Camera::CLEARZ)
		mask |= GL_DEPTH_BUFFER_BIT;
	if(mode & Camera::CLEARSTENCIL)
		mask |= GL_STENCIL_BUFFER_BIT;
	glDepthMask(GL_TRUE);
	glClear(mask);
	glDepthMask(rwStateCache.zwrite);

	if(setScissor)
		glDisable(GL_SCISSOR_TEST);
}

static void
showRaster(Raster *raster, uint32 flags)
{
//	glViewport(raster->offsetX, raster->offsetY,
//		raster->width, raster->height);
	// TODO check if context is lost
#ifdef RE3_GENERIC
	rg_renderer_present(softwareRenderer);
#else
	eglSwapBuffers(eglGetCurrentDisplay(), eglGetCurrentSurface(EGL_DRAW));
#endif
}

static bool32
rasterRenderFast(Raster *raster, int32 x, int32 y)
{
	Raster *src = raster;
	Raster *dst = Raster::getCurrentContext();
	Gl1Raster *natdst = PLUGINOFFSET(Gl1Raster, dst, nativeRasterOffset);
	Gl1Raster *natsrc = PLUGINOFFSET(Gl1Raster, src, nativeRasterOffset);

	switch(dst->type){
	case Raster::NORMAL:
	case Raster::TEXTURE:
	case Raster::CAMERATEXTURE:
		switch(src->type){
		case Raster::CAMERA:
			setActiveTexture(0);
			glBindTexture(GL_TEXTURE_2D, natdst->texid);
			glCopyTexSubImage2D(GL_TEXTURE_2D, 0, x, (dst->height-src->height)-y,
				0, 0, src->width, src->height);
			glBindTexture(GL_TEXTURE_2D, boundTexture[0]);
			return 1;
		}
		break;
	}
	return 0;
}

static int
openSymbian(EngineOpenParams *openparams)
{
	glGlobals.winWidth = openparams->width;
	glGlobals.winHeight = openparams->height;
	
	glGlobals.modes = (DisplayMode*)rwMalloc(sizeof(DisplayMode), ID_DRIVER | MEMDUR_EVENT);
	glGlobals.modes[0].width = openparams->width;
	glGlobals.modes[0].height = openparams->height;
	glGlobals.modes[0].depth = 32;
	glGlobals.modes[0].flags = VIDEOMODEEXCLUSIVE;
	
	glGlobals.numModes = 1;
	glGlobals.currentMode = 0;
	
	glGlobals.numMonitors = 1;
	glGlobals.currentMonitor = 0;

	return 1;
}

static int
closeSymbian(void)
{
	if(glGlobals.modes){
		rwFree(glGlobals.modes);
		glGlobals.modes = nil;
	}
	return 1;
}

static int
startSymbian(void)
{
#ifdef RE3_GENERIC
	softwareRenderer = rg_renderer_create(glGlobals.winWidth, glGlobals.winHeight);
	if(softwareRenderer == nil)
		return 0;
#endif
	glGlobals.presentWidth = glGlobals.winWidth;
	glGlobals.presentHeight = glGlobals.winHeight;
	glGlobals.presentOffX = 0;
	glGlobals.presentOffY = 0;
	
	return 1;
}

static int
stopSymbian(void)
{
#ifdef RE3_GENERIC
	rg_renderer_destroy(softwareRenderer);
	softwareRenderer = nil;
#endif
	return 1;
}

static int
initOpenGL(void)
{
	glClearColor(0.25, 0.25, 0.25, 1.0);

	byte whitepixel[4] = {0xFF, 0xFF, 0xFF, 0xFF};
	glGenTextures(1, &whitetex);
	glBindTexture(GL_TEXTURE_2D, whitetex);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 1, 1,
	             0, GL_RGBA, GL_UNSIGNED_BYTE, &whitepixel);

	resetRenderState();

	glEnable(GL_NORMALIZE);

	openIm2D();
	openIm3D();

	return 1;
}

static int
termOpenGL(void)
{
	closeIm3D();
	closeIm2D();

	glDeleteTextures(1, &whitetex);
	whitetex = 0;

	return 1;
}

static int
finalizeOpenGL(void)
{
	return 1;
}

static int
deviceSystemSymbian(DeviceReq req, void *arg, int32 n)
{
	VideoMode *rwmode;

	switch(req){
	case DEVICEOPEN:
		return openSymbian((EngineOpenParams*)arg);

	case DEVICECLOSE:
		return closeSymbian();

	case DEVICEINIT:
		return startSymbian() && initOpenGL();

	case DEVICETERM:
		return termOpenGL() && stopSymbian();

	case DEVICEFINALIZE:
		return finalizeOpenGL();

	case DEVICEGETNUMSUBSYSTEMS:
		return glGlobals.numMonitors;

	case DEVICEGETCURRENTSUBSYSTEM:
		return glGlobals.currentMonitor;

	case DEVICESETSUBSYSTEM:
		if(n >= glGlobals.numMonitors)
			return 0;
		glGlobals.currentMonitor = n;
		return 1;

	case DEVICEGETSUBSSYSTEMINFO:
		if(n >= glGlobals.numMonitors)
			return 0;
		strncpy(((SubSystemInfo*)arg)->name, "Symbian Display", 80 /*sizeof(SubSystemInfo::name)*/);
		return 1;

	case DEVICEGETNUMVIDEOMODES:
		return glGlobals.numModes;

	case DEVICEGETCURRENTVIDEOMODE:
		return glGlobals.currentMode;

	case DEVICESETVIDEOMODE:
		if(n >= glGlobals.numModes)
			return 0;
		glGlobals.currentMode = n;
		return 1;

	case DEVICEGETVIDEOMODEINFO:
		rwmode = (VideoMode*)arg;
		rwmode->width = glGlobals.modes[n].width;
		rwmode->height = glGlobals.modes[n].height;
		rwmode->depth = glGlobals.modes[n].depth;
		rwmode->flags = glGlobals.modes[n].flags;
		return 1;

	case DEVICEGETMAXMULTISAMPLINGLEVELS:
		return 1;

	case DEVICEGETMULTISAMPLINGLEVELS:
		return 1;

	case DEVICESETMULTISAMPLINGLEVELS:
		glGlobals.numSamples = 1;
		return 1;

	default:
		assert(0 && "not implemented");
		return 0;
	}
	return 1;
}

Device renderdevice = {
	-1.0f, 1.0f,
	gles1::beginUpdate,
	gles1::endUpdate,
	gles1::clearCamera,
	gles1::showRaster,
	gles1::rasterRenderFast,
	gles1::setRenderState,
	gles1::getRenderState,
	gles1::im2DRenderLine,
	gles1::im2DRenderTriangle,
	gles1::im2DRenderPrimitive,
	gles1::im2DRenderIndexedPrimitive,
	gles1::im3DTransform,
	gles1::im3DRenderPrimitive,
	gles1::im3DRenderIndexedPrimitive,
	gles1::im3DEnd,
	gles1::deviceSystemSymbian
};

}
}

#else
// urgh, probably should get rid of that eventually
#include "rwgles1.h"
namespace rw {
namespace gles1 { 
bool32 needToReadBackTextures;
}
}
#endif
