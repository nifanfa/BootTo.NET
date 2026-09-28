// ==========================================================================
//
// context.cpp	Rendering Context Class for 3D Rendering Library
//
// --------------------------------------------------------------------------
//
// 08-07-2003	Hans-Martin Will	initial version
//
// --------------------------------------------------------------------------
//
// Copyright (c) 2004, Hans-Martin Will. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are 
// met:
// 
//	 *  Redistributions of source code must retain the above copyright
// 		notice, this list of conditions and the following disclaimer. 
//   *	Redistributions in binary form must reproduce the above copyright
// 		notice, this list of conditions and the following disclaimer in the 
// 		documentation and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
//
// ==========================================================================


#include "stdafx.h"
#include "Context.h"
#include "Surface.h"
#include "Rasterizer.h"
#include "Utils.h"


using namespace EGL;


// --------------------------------------------------------------------------
// Constructor and destructor
// --------------------------------------------------------------------------


Context :: Context(const Config & config) 
	:
	m_Config(config),
	m_DrawSurface(0),
	m_ReadSurface(0),
	m_LastError(GL_NO_ERROR),

	// transformation matrices
	m_ModelViewMatrixStack(16),
	m_CurrentMatrixStack(&m_ModelViewMatrixStack),
	m_MatrixMode(GL_MODELVIEW),
	m_Scissor(0, 0, config.GetConfigAttrib(EGL_WIDTH), config.GetConfigAttrib(EGL_HEIGHT)),
	m_Viewport(0, 0, config.GetConfigAttrib(EGL_WIDTH), config.GetConfigAttrib(EGL_HEIGHT)),
	m_CurrentPaletteMatrix(0),

	// server flags
	m_ClipPlaneEnabled(0),
	m_LightingEnabled(false),
	m_TwoSidedLightning(false),
	m_LightEnabled(0),				// no light on
	m_CullFaceEnabled(false),
	m_DitherEnabled(false),
	m_ReverseFaceOrientation(false),
	m_CullMode(CullModeBack),
	m_ColorMaterialEnabled(false),
	m_NormalizeEnabled(false),
	m_RescaleNormalEnabled(false),
	m_PolygonOffsetFillEnabled(false),
	m_MultiSampleEnabled(false),
	m_SampleAlphaToCoverageEnabled(false),
	m_SampleAlphaToOneEnabled(false),
	m_SampleCoverageEnabled(false),
	m_ScissorTestEnabled(false),
	m_MatrixPaletteEnabled(false),
	m_MatrixModePaletteEnabled(false),
	m_ActiveTexture(0),
	m_ClientActiveTexture(0),

	// point parameters
	m_PointSize(EGL_ONE),
	m_PointSizeMin(EGL_ONE),
	m_PointSizeMax(EGL_ONE),		// what is the correct value?
	m_PointFadeThresholdSize(EGL_ONE),
	m_PointSizeAttenuate(false),

	// fog parameters for setup phase
	m_FogMode(FogModeExp),
	m_FogStart(0),
	m_FogEnd(EGL_ONE),
	m_FogGradient(EGL_ONE),
	m_FogGradientShift(0),
	m_FogDensity(EGL_ONE),

	// client flags
	m_VertexArrayEnabled(false),
	m_NormalArrayEnabled(false),
	m_ColorArrayEnabled(false),
	m_PointSizeArrayEnabled(false),

	// buffers
	m_CurrentArrayBuffer(0),
	m_CurrentElementArrayBuffer(0),

	// general context state
	m_Current(false),
	m_Disposed(false),
	m_ViewportInitialized(false),
	m_DefaultNormal(0, 0, EGL_ONE),
	m_DefaultRGBA(EGL_ONE, EGL_ONE, EGL_ONE, EGL_ONE),

	// pixel store state
	m_PixelStorePackAlignment(4),
	m_PixelStoreUnpackAlignment(4),

	// SGIS_generate_mipmap extension
	m_GenerateMipmaps(false),

	// hints
	m_PerspectiveCorrectionHint(GL_DONT_CARE),
	m_PointSmoothHint(GL_DONT_CARE),
	m_LineSmoothHint(GL_DONT_CARE),
	m_FogHint(GL_DONT_CARE),
	m_GenerateMipmapHint(GL_DONT_CARE)
{
	DepthRangex(VIEWPORT_NEAR, VIEWPORT_FAR);
	ClearDepthx(EGL_ONE);
	ClearStencil(0);

	m_Rasterizer = new Rasterizer(GetRasterizerState());	
	m_Buffers.Allocate();			// default buffer

	m_LightModelAmbient.r = m_LightModelAmbient.g = m_LightModelAmbient.b = F(0.2f);
	m_LightModelAmbient.a = F(1.0);

	m_Lights[0].SetDiffuseColor(FractionalColor(F(1.0), F(1.0), F(1.0), F(1.0)));
	m_Lights[0].SetSpecularColor(FractionalColor(F(1.0), F(1.0), F(1.0), F(1.0)));

	m_PointDistanceAttenuation[0] = EGL_ONE;
	m_PointDistanceAttenuation[1] = 0;
	m_PointDistanceAttenuation[2] = 0;

	size_t defaultTexture = m_Textures.Allocate();

	for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		m_TexCoordArrayEnabled[unit] = false;
		m_Rasterizer->SetTexture(unit, m_Textures.GetObject(defaultTexture));
	}

	memset(&m_ClipPlanes, 0, sizeof(m_ClipPlanes));
}


Context :: ~Context() {

	if (m_DrawSurface != 0) {
		m_DrawSurface->SetCurrentContext(0);
	}

	if (m_ReadSurface != 0 && m_ReadSurface != m_DrawSurface) {
		m_ReadSurface->SetCurrentContext(0);
	}

	m_ReadSurface = m_DrawSurface = 0;

	if (m_Rasterizer != 0) {

		delete m_Rasterizer;
		m_Rasterizer = 0;
	}
}


void Context :: SetReadSurface(EGL::Surface * surface) {
	if (m_ReadSurface != 0 && m_ReadSurface != m_DrawSurface && m_ReadSurface != surface) {
		m_ReadSurface->SetCurrentContext(0);
	}

	m_ReadSurface = surface;
	m_ReadSurface->SetCurrentContext(this);
}


void Context :: SetDrawSurface(EGL::Surface * surface) {
	if (m_DrawSurface != 0 && m_ReadSurface != m_DrawSurface && m_DrawSurface != surface) {
		m_DrawSurface->SetCurrentContext(0);
	}

	if (surface != 0 && !m_ViewportInitialized) {
		U16 width = surface->GetWidth();
		U16 height = surface->GetHeight();

		Viewport(0, 0, width, height);
		Scissor(0, 0, width, height);
		m_ViewportInitialized = true;
	}

	m_DrawSurface = surface;
	m_DrawSurface->SetCurrentContext(this);
	m_Rasterizer->SetSurface(surface);

	UpdateScissorTest();
}


void Context :: Dispose() {
	if (m_Current) {
		m_Disposed = true;
	} else {
		delete this;
	}
}

void Context :: SetCurrent(bool current) {
	m_Current = current;

	if (!m_Current && m_Disposed) {
		delete this;
	}
}

// --------------------------------------------------------------------------
// Error handling
// --------------------------------------------------------------------------


GLenum Context :: GetError(void) { 
	GLenum result = m_LastError;
	m_LastError = GL_NO_ERROR;
	return result;
}


void Context :: RecordError(GLenum error) {
	if (error != GL_NO_ERROR && m_LastError == GL_NO_ERROR) {
		m_LastError = error;
	}
}


// --------------------------------------------------------------------------
// Clearing
// --------------------------------------------------------------------------


void Context :: Clear(GLbitfield mask) { 

	if (m_DrawSurface == 0) {
		return;
	}

	if ((mask & (GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT)) != mask) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (m_ScissorTestEnabled) {
		if (mask & GL_COLOR_BUFFER_BIT) {
			// is clamping [min, max] or [min, max)
			m_DrawSurface->ClearColorBuffer(m_ColorClearValue, 
				m_RasterizerState.GetColorMask(), m_Scissor);
		}

		if (mask & GL_DEPTH_BUFFER_BIT) {
			// actually need to transform depth to correct value
#ifdef RE3_GENERIC
			U32 clearValue = static_cast<U32>((static_cast<I64>(m_DepthClearValue) * 0xffffff) / EGL_ONE);
#else
			EGL_Fixed clearValue = EGL_MAP_0_1(m_DepthClearValue);
#endif
			m_DrawSurface->ClearDepthBuffer(clearValue, m_RasterizerState.GetDepthMask(), m_Scissor);
		}

		if (mask & GL_STENCIL_BUFFER_BIT) {
			m_DrawSurface->ClearStencilBuffer(m_StencilClearValue, m_RasterizerState.GetStencilMask(), m_Scissor);
		}
	} else {
		if (mask & GL_COLOR_BUFFER_BIT) {
			// is clamping [min, max] or [min, max)
			m_DrawSurface->ClearColorBuffer(m_ColorClearValue, m_RasterizerState.GetColorMask());
		}

		if (mask & GL_DEPTH_BUFFER_BIT) {
			// actually need to transform depth to correct value
#ifdef RE3_GENERIC
			U32 clearValue = static_cast<U32>((static_cast<I64>(m_DepthClearValue) * 0xffffff) / EGL_ONE);
#else
			EGL_Fixed clearValue = EGL_MAP_0_1(m_DepthClearValue);
#endif
			m_DrawSurface->ClearDepthBuffer(clearValue, m_RasterizerState.GetDepthMask());
		}

		if (mask & GL_STENCIL_BUFFER_BIT) {
			m_DrawSurface->ClearStencilBuffer(m_StencilClearValue, m_RasterizerState.GetStencilMask());
		}
	}
}


void Context :: ClearColorx(GLclampx red, GLclampx green, GLclampx blue, GLclampx alpha) { 
	m_ColorClearValue = FractionalColor(red, green, blue, alpha);
}


void Context :: ClearDepthx(GLclampx depth) { 
	m_DepthClearValue = EGL_CLAMP(depth, 0, EGL_ONE);
}


void Context :: ClearStencil(GLint s) { 
	m_StencilClearValue = s;
}


// --------------------------------------------------------------------------
//
// --------------------------------------------------------------------------


void Context :: Toggle(GLenum cap, bool value) { 
	switch (cap) {
	case GL_LIGHTING:
		m_LightingEnabled = value;
		break;

	case GL_FOG:
		GetRasterizerState()->EnableFog(value);
		break;

	case GL_TEXTURE_2D:
		GetRasterizerState()->EnableTexture(m_ActiveTexture, value);
		break;

	case GL_CULL_FACE:
		m_CullFaceEnabled = value;
		break;

	case GL_ALPHA_TEST:
		GetRasterizerState()->EnableAlphaTest(value);
		break;

	case GL_BLEND:
		GetRasterizerState()->EnableBlending(value);
		break;

	case GL_COLOR_LOGIC_OP:
		GetRasterizerState()->EnableLogicOp(value);
		break;

	case GL_DITHER:
		m_DitherEnabled = value;
		break;

	case GL_STENCIL_TEST:
		GetRasterizerState()->EnableStencilTest(value);
		break;

	case GL_DEPTH_TEST:
		GetRasterizerState()->EnableDepthTest(value);
		break;

	case GL_LIGHT0:
	case GL_LIGHT1:
	case GL_LIGHT2:
	case GL_LIGHT3:
	case GL_LIGHT4:
	case GL_LIGHT5:
	case GL_LIGHT6:
	case GL_LIGHT7:
		{
			int mask = 1 << (cap - GL_LIGHT0);

			if (value) {
				m_LightEnabled |= mask;
			} else {
				m_LightEnabled &= ~mask;
			}
		}
		break;

	case GL_POINT_SMOOTH:
		GetRasterizerState()->SetPointSmoothEnabled(value);
		break;

	case GL_POINT_SPRITE_OES:
		GetRasterizerState()->SetPointSpriteEnabled(value);
		break;

	case GL_LINE_SMOOTH:
		GetRasterizerState()->SetLineSmoothEnabled(value);
		break;

	case GL_SCISSOR_TEST:
		m_ScissorTestEnabled = value;
		UpdateScissorTest();
		break;

	case GL_COLOR_MATERIAL:
		m_ColorMaterialEnabled = value;
		break;

	case GL_NORMALIZE:
		m_NormalizeEnabled = value;
		break;

	case GL_CLIP_PLANE0:
	case GL_CLIP_PLANE1:
	case GL_CLIP_PLANE2:
	case GL_CLIP_PLANE3:
	case GL_CLIP_PLANE4:
	case GL_CLIP_PLANE5:
		{
			size_t plane = cap - GL_CLIP_PLANE0;
			U32 mask = ~(1u << plane);
			U32 bit = value ? (1u << plane) : 0;

			m_ClipPlaneEnabled = (m_ClipPlaneEnabled & mask) | bit;
		}

		break;

	case GL_RESCALE_NORMAL:
		m_RescaleNormalEnabled = value;
		UpdateInverseModelViewMatrix();
		break;

	case GL_POLYGON_OFFSET_FILL:
		GetRasterizerState()->EnablePolygonOffsetFill(value);
		break;

	case GL_MULTISAMPLE:
		m_MultiSampleEnabled = value;
		break;

	case GL_SAMPLE_ALPHA_TO_COVERAGE:
		m_SampleAlphaToCoverageEnabled = value;
		break;

	case GL_SAMPLE_ALPHA_TO_ONE:
		m_SampleAlphaToOneEnabled = value;
		break;

	case GL_SAMPLE_COVERAGE:
		m_SampleCoverageEnabled = value;
		break;

	case GL_MATRIX_PALETTE_OES:
		m_MatrixPaletteEnabled = value;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}

void Context :: Disable(GLenum cap) { 
	Toggle(cap, false);
}

void Context :: Enable(GLenum cap) { 
	Toggle(cap, true);
}

void Context :: Hint(GLenum target, GLenum mode) { 

	if (mode != GL_DONT_CARE && mode != GL_FASTEST && mode != GL_NICEST) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	switch (target) {
	case GL_FOG_HINT:
		m_FogHint = mode;
		break;

	case GL_LINE_SMOOTH_HINT:
		m_LineSmoothHint = mode;
		break;

	case GL_PERSPECTIVE_CORRECTION_HINT:
		m_PerspectiveCorrectionHint = mode;
		break;

	case GL_POINT_SMOOTH_HINT:
		m_PointSmoothHint = mode;
		break;

	case GL_GENERATE_MIPMAP_HINT:
		m_GenerateMipmapHint = mode;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}

namespace {
    static const GLenum formats[] = {
		GL_PALETTE4_RGB8_OES,
		GL_PALETTE4_RGBA8_OES,
		GL_PALETTE4_R5_G6_B5_OES,
		GL_PALETTE4_RGBA4_OES,
		GL_PALETTE4_RGB5_A1_OES,
		GL_PALETTE8_RGB8_OES,
		GL_PALETTE8_RGBA8_OES,
		GL_PALETTE8_R5_G6_B5_OES,
		GL_PALETTE8_RGBA4_OES,
		GL_PALETTE8_RGB5_A1_OES
    };

	void CopyRect(const Rect & rect, GLint * params) {
		params[0] = rect.x;
		params[1] = rect.y;
		params[2] = rect.width;
		params[3] = rect.height;
	}

	GLenum EnumFromComparisonFunc(RasterizerState::ComparisonFunc func) {
		switch (func) {
			case RasterizerState::CompFuncNever:	return GL_NEVER;
			case RasterizerState::CompFuncLess:		return GL_LESS;
			case RasterizerState::CompFuncLEqual:	return GL_LEQUAL;
			case RasterizerState::CompFuncGreater:	return GL_GREATER;
			case RasterizerState::CompFuncGEqual:	return GL_GEQUAL;
			case RasterizerState::CompFuncEqual:	return GL_EQUAL;
			case RasterizerState::CompFuncNotEqual:	return GL_NOTEQUAL;
			case RasterizerState::CompFuncAlways:	return GL_ALWAYS;
			default:								return 0;
		}
	}

	GLenum EnumFromStencilOp(RasterizerState::StencilOp op) {
		switch (op) {
		case RasterizerState::StencilOpKeep:	return GL_KEEP;
		case RasterizerState::StencilOpZero:	return GL_ZERO;
		case RasterizerState::StencilOpReplace:	return GL_REPLACE;
		case RasterizerState::StencilOpIncr:	return GL_INCR;
		case RasterizerState::StencilOpDecr:	return GL_DECR;
		case RasterizerState::StencilOpInvert:	return GL_INVERT;
		default:								return 0;
		}
	}

	GLenum EnumFromLogicOp(RasterizerState::LogicOp op) {
		switch (op) {
		case RasterizerState::LogicOpClear:			return GL_CLEAR;
		case RasterizerState::LogicOpAnd:			return GL_AND;
		case RasterizerState::LogicOpAndReverse:	return GL_AND_REVERSE;
		case RasterizerState::LogicOpCopy:			return GL_COPY;
		case RasterizerState::LogicOpAndInverted:	return GL_AND_INVERTED;
		case RasterizerState::LogicOpNoop:			return GL_NOOP;
		case RasterizerState::LogicOpXor:			return GL_XOR;
		case RasterizerState::LogicOpOr:			return GL_OR;
		case RasterizerState::LogicOpNor:			return GL_NOR;
		case RasterizerState::LogicOpEquiv:			return GL_EQUIV;
		case RasterizerState::LogicOpInvert:		return GL_INVERT;
		case RasterizerState::LogicOpOrReverse:		return GL_OR_REVERSE;
		case RasterizerState::LogicOpCopyInverted:	return GL_COPY_INVERTED;
		case RasterizerState::LogicOpOrInverted:	return GL_OR_INVERTED;
		case RasterizerState::LogicOpNand:			return GL_NAND;
		case RasterizerState::LogicOpSet:			return GL_SET;
		default:									return 0;
		}
	}

	GLenum EnumFromBlendFuncSrc(RasterizerState::BlendFuncSrc sfactor) {
		switch (sfactor) {
		case RasterizerState::BlendFuncSrcZero:				return GL_ZERO;
		case RasterizerState::BlendFuncSrcOne:				return GL_ONE;
		case RasterizerState::BlendFuncSrcDstColor:			return GL_DST_COLOR;
		case RasterizerState::BlendFuncSrcOneMinusDstColor:	return GL_ONE_MINUS_DST_COLOR;
		case RasterizerState::BlendFuncSrcSrcAlpha:			return GL_SRC_ALPHA;
		case RasterizerState::BlendFuncSrcOneMinusSrcAlpha:	return GL_ONE_MINUS_SRC_ALPHA;
		case RasterizerState::BlendFuncSrcDstAlpha:			return GL_DST_ALPHA;
		case RasterizerState::BlendFuncSrcOneMinusDstAlpha:	return GL_ONE_MINUS_DST_ALPHA;
		case RasterizerState::BlendFuncSrcSrcAlphaSaturate:	return GL_SRC_ALPHA_SATURATE;
		default:											return 0;
		}
	}

	GLenum EnumFromBlendFuncDst(RasterizerState::BlendFuncDst dfactor) {
		switch (dfactor) {
		case RasterizerState::BlendFuncDstZero:				return GL_ZERO;
		case RasterizerState::BlendFuncDstSrcColor:			return GL_SRC_COLOR;
		case RasterizerState::BlendFuncDstOneMinusSrcColor:	return GL_ONE_MINUS_SRC_COLOR;
		case RasterizerState::BlendFuncDstSrcAlpha:			return GL_SRC_ALPHA;
		case RasterizerState::BlendFuncDstOneMinusSrcAlpha:	return GL_ONE_MINUS_SRC_ALPHA;
		case RasterizerState::BlendFuncDstDstAlpha:			return GL_DST_ALPHA;
		case RasterizerState::BlendFuncDstOneMinusDstAlpha:	return GL_ONE_MINUS_DST_ALPHA;
		case RasterizerState::BlendFuncDstOne:				return GL_ONE;
		default:											return 0;
		}
	}
}


void Context :: GetIntegerv(GLenum pname, GLint *params) { 
	switch (pname) {
	/* 1.0 stuff */
	case GL_SMOOTH_LINE_WIDTH_RANGE:
	case GL_SMOOTH_POINT_SIZE_RANGE:
	case GL_ALIASED_POINT_SIZE_RANGE:
	case GL_ALIASED_LINE_WIDTH_RANGE:
		params[0] = params[1] = 1;
		return;

	case GL_ALPHA_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_ALPHA_SIZE);
		break;

	case GL_BLUE_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_BLUE_SIZE);
		break;

	case GL_DEPTH_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_DEPTH_SIZE);
		break;

	case GL_GREEN_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_GREEN_SIZE);
		break;

	case GL_RED_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_RED_SIZE);
		break;

	case GL_STENCIL_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_STENCIL_SIZE);
		break;

	case GL_SUBPIXEL_BITS:
		params[0] = m_Config.GetConfigAttrib(EGL_SAMPLES);
		break;

	case GL_COMPRESSED_TEXTURE_FORMATS:
		{
			size_t numFormats = sizeof(formats) / sizeof(formats[0]);

			for (size_t index = 0; index < numFormats; ++index) 
				params[index] = formats[index];
		}

		break;

	case GL_NUM_COMPRESSED_TEXTURE_FORMATS:
		params[0] = sizeof(formats) / sizeof(formats[0]);
		break;

	case GL_MAX_ELEMENTS_INDICES:
	case GL_MAX_ELEMENTS_VERTICES:
		params[0] = INT_MAX;
		break;

	case GL_MAX_LIGHTS:
		params[0] = EGL_NUMBER_LIGHTS;
		break;

	case GL_MAX_CLIP_PLANES:
		params[0] = NUM_CLIP_PLANES;
		break;

	case GL_MAX_MODELVIEW_STACK_DEPTH:
		params[0] = m_ModelViewMatrixStack.GetStackSize();
		break;

	case GL_MODELVIEW_STACK_DEPTH:
		params[0] = m_ModelViewMatrixStack.GetCurrentStackSize();
		break;

	case GL_MAX_PROJECTION_STACK_DEPTH:
		params[0] = m_ProjectionMatrixStack.GetStackSize();
		break;

	case GL_PROJECTION_STACK_DEPTH:
		params[0] = m_ProjectionMatrixStack.GetCurrentStackSize();
		break;

	case GL_MAX_TEXTURE_STACK_DEPTH:
		params[0] = m_TextureMatrixStack[m_ActiveTexture].GetStackSize();
		break;

	case GL_TEXTURE_STACK_DEPTH:
		params[0] = m_TextureMatrixStack[m_ActiveTexture].GetCurrentStackSize();
		break;

	case GL_MAX_TEXTURE_SIZE:
		params[0] = RasterizerState::MaxTextureSize;
		break;

	case GL_MAX_TEXTURE_UNITS:
		params[0] = EGL_NUM_TEXTURE_UNITS;
		break;

	case GL_IMPLEMENTATION_COLOR_READ_TYPE_OES:
		params[0] = GL_UNSIGNED_SHORT_5_6_5;
		break;

	case GL_IMPLEMENTATION_COLOR_READ_FORMAT_OES:
		params[0] = GL_RGB;
		break;

	case GL_MAX_VIEWPORT_DIMS:
		params[0] = m_Config.m_Width;
		params[1] = m_Config.m_Height;
		break;

	case GL_MAX_PALETTE_MATRICES_OES:
	case GL_MAX_VERTEX_UNITS_OES:
		params[0] = MATRIX_PALETTE_SIZE;
		break;

//		params[0] = EGL_NUM_TEXTURE_UNITS;
//		break;

	case GL_VERTEX_ARRAY_SIZE:
		params[0] = m_VertexArray.size;
		break;

	case GL_VERTEX_ARRAY_STRIDE:
		params[0] = m_VertexArray.stride;
		break;

	case GL_VERTEX_ARRAY_TYPE:
		params[0] = m_VertexArray.type;
		break;

	case GL_NORMAL_ARRAY_STRIDE:
		params[0] = m_NormalArray.stride;
		break;

	case GL_NORMAL_ARRAY_TYPE:
		params[0] = m_NormalArray.type;
		break;

	case GL_COLOR_ARRAY_SIZE:
		params[0] = m_ColorArray.size;
		break;

	case GL_COLOR_ARRAY_STRIDE:
		params[0] = m_ColorArray.stride;
		break;

	case GL_COLOR_ARRAY_TYPE:
		params[0] = m_ColorArray.type;
		break;

	case GL_TEXTURE_COORD_ARRAY_SIZE:
		params[0] = m_TexCoordArray[m_ClientActiveTexture].size;
		break;

	case GL_TEXTURE_COORD_ARRAY_STRIDE:
		params[0] = m_TexCoordArray[m_ClientActiveTexture].stride;
		break;

	case GL_TEXTURE_COORD_ARRAY_TYPE:
		params[0] = m_TexCoordArray[m_ClientActiveTexture].type;
		break;

	case GL_MATRIX_INDEX_ARRAY_SIZE_OES:
		params[0] = m_MatrixIndexArray.size;
		break;

	case GL_MATRIX_INDEX_ARRAY_TYPE_OES:
		params[0] = m_MatrixIndexArray.type;
		break;

	case GL_MATRIX_INDEX_ARRAY_STRIDE_OES:
		params[0] = m_MatrixIndexArray.stride;
		break;

	case GL_WEIGHT_ARRAY_SIZE_OES:
		params[0] = m_WeightArray.size;
		break;

	case GL_WEIGHT_ARRAY_TYPE_OES:
		params[0] = m_WeightArray.type;
		break;

	case GL_WEIGHT_ARRAY_STRIDE_OES:
		params[0] = m_WeightArray.stride;
		break;

	case GL_POINT_SIZE_ARRAY_TYPE_OES:
		params[0] = m_PointSizeArray.type;
		break;

	case GL_POINT_SIZE_ARRAY_STRIDE_OES:
		params[0] = m_PointSizeArray.stride;
		break;

	case GL_VERTEX_ARRAY_BUFFER_BINDING:
		params[0] = m_VertexArray.boundBuffer;
		break;

	case GL_NORMAL_ARRAY_BUFFER_BINDING:
		params[0] = m_NormalArray.boundBuffer;
		break;

	case GL_COLOR_ARRAY_BUFFER_BINDING:
		params[0] = m_ColorArray.boundBuffer;
		break;

	case GL_TEXTURE_COORD_ARRAY_BUFFER_BINDING:
		params[0] = m_TexCoordArray[m_ClientActiveTexture].boundBuffer;
		break;

	case GL_ELEMENT_ARRAY_BUFFER_BINDING:
		params[0] = m_CurrentElementArrayBuffer;
		break;

	case GL_POINT_SIZE_ARRAY_BUFFER_BINDING_OES:
		params[0] = m_PointSizeArray.boundBuffer;
		break;

	case GL_MATRIX_INDEX_ARRAY_BUFFER_BINDING_OES:
		params[0] = m_MatrixIndexArray.boundBuffer;
		break;

	case GL_WEIGHT_ARRAY_BUFFER_BINDING_OES:
		params[0] = m_WeightArray.boundBuffer;
		break;

	case GL_ARRAY_BUFFER_BINDING:
		params[0] = m_CurrentArrayBuffer;
		break;

	case GL_UNPACK_ALIGNMENT:
		params[0] = m_PixelStoreUnpackAlignment;
		break;

	case GL_PACK_ALIGNMENT:
		params[0] = m_PixelStorePackAlignment;
		break;

	case GL_STENCIL_CLEAR_VALUE:
		params[0] = m_StencilClearValue;
		break;

	case GL_DEPTH_CLEAR_VALUE:
		params[0] = EGL_Mul(0x7fffffff, m_DepthClearValue);
		break;

	case GL_SCISSOR_BOX:
		CopyRect(m_Scissor, params);
		break;

	case GL_SAMPLE_BUFFERS:
		params[0] = m_Config.m_SampleBuffers;
		break;

	case GL_SAMPLES:
		params[0] = m_Config.m_Samples;
		break;

	case GL_VIEWPORT:
		CopyRect(m_Viewport, params);
		break;

	case GL_STENCIL_WRITEMASK:
		params[0] = m_RasterizerState.GetStencilMask();
		break;

	case GL_STENCIL_VALUE_MASK:
		params[0] = m_RasterizerState.GetStencilComparisonMask();
		break;

	case GL_STENCIL_REF:
		params[0] = m_RasterizerState.GetStencilRef();
		break;

	case GL_ALPHA_TEST_REF:
		params[0] = EGL_Mul(0x7fffffff, m_RasterizerState.GetAlphaRef());
		break;

	case GL_ALPHA_TEST_FUNC:
		params[0] = EnumFromComparisonFunc(m_RasterizerState.GetAlphaFunc());
		break;

	case GL_STENCIL_FUNC:
		params[0] = EnumFromComparisonFunc(m_RasterizerState.GetStencilFunc());
		break;

	case GL_STENCIL_FAIL:
		params[0] = EnumFromStencilOp(m_RasterizerState.GetStencilOpFail());
		break;

	case GL_STENCIL_PASS_DEPTH_FAIL:
		params[0] = EnumFromStencilOp(m_RasterizerState.GetStencilOpFailZFail());
		break;

	case GL_STENCIL_PASS_DEPTH_PASS:
		params[0] = EnumFromStencilOp(m_RasterizerState.GetStencilOpFailZPass());
		break;

	case GL_DEPTH_FUNC:
		params[0] = EnumFromComparisonFunc(m_RasterizerState.GetDepthFunc());
		break;

	case GL_LOGIC_OP_MODE:
		params[0] = EnumFromLogicOp(m_RasterizerState.GetLogicOp());
		break;

	case GL_BLEND_SRC:
		params[0] = EnumFromBlendFuncSrc(m_RasterizerState.GetBlendFuncSrc());
		break;

	case GL_BLEND_DST:
		params[0] = EnumFromBlendFuncDst(m_RasterizerState.GetBlendFuncDst());
		break;

	case GL_FOG_MODE:
		{
			switch (m_FogMode) {
			case FogLinear:		params[0] = GL_LINEAR;	break;
			case FogModeExp:	params[0] = GL_EXP;		break;
			case FogModeExp2:	params[0] = GL_EXP2;	break;
			}
		}

		break;

	case GL_SHADE_MODEL:
		{
			switch (m_RasterizerState.GetShadeModel()) {
			case RasterizerState::ShadeModelFlat:	params[0] = GL_FLAT;
			case RasterizerState::ShadeModelSmooth:	params[0] = GL_SMOOTH;
			}
		}

		break;

	case GL_CULL_FACE_MODE:
		{
			switch (m_CullMode) {
			case CullModeFront:			params[0] = GL_FRONT;			break;
			case CullModeBack:			params[0] = GL_BACK;			break;
			case CullModeBackAndFront:	params[0] = GL_FRONT_AND_BACK;	break;
			}
		}

		break;

	case GL_FRONT_FACE:
		params[0] = m_ReverseFaceOrientation ? GL_CW : GL_CCW;
		break;

	case GL_MATRIX_MODE:
		params[0] = m_MatrixMode;
		break;

	case GL_ACTIVE_TEXTURE:
		params[0] = GL_TEXTURE0 + m_ActiveTexture;
		break;

	case GL_CLIENT_ACTIVE_TEXTURE:
		params[0] = GL_TEXTURE0 + m_ClientActiveTexture;
		break;

	case GL_TEXTURE_BINDING_2D:
		{
			size_t index = m_Textures.GetIndex(m_Rasterizer->GetTexture(m_ActiveTexture));

			if (index == ~0) {
				params[0] = 0;
			} else {
				params[0] = index;
			}
		}

		break;

	case GL_COORD_REPLACE_OES:
		params[0] = m_RasterizerState.IsPointCoordReplaceEnabled(m_ActiveTexture);
		break;

	case GL_CURRENT_COLOR:
		{
			params[0] = EGL_Mul(0x7fffffff, m_DefaultRGBA.r);
			params[1] = EGL_Mul(0x7fffffff, m_DefaultRGBA.g);
			params[2] = EGL_Mul(0x7fffffff, m_DefaultRGBA.b);
			params[3] = EGL_Mul(0x7fffffff, m_DefaultRGBA.a);
		}

		break;

	case GL_PERSPECTIVE_CORRECTION_HINT:
		params[0] = m_PerspectiveCorrectionHint;
		break;

	case GL_POINT_SMOOTH_HINT:
		params[0] = m_PointSmoothHint;
		break;

	case GL_LINE_SMOOTH_HINT:
		params[0] = m_LineSmoothHint;
		break;

	case GL_FOG_HINT:
		params[0] = m_FogHint;
		break;

	case GL_GENERATE_MIPMAP_HINT:
		params[0] = m_GenerateMipmapHint;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
	}
}


const GLubyte * Context :: GetString(GLenum name) { 

	switch (name) {
	case GL_VENDOR:
		return (GLubyte *) EGL_CONFIG_VENDOR;

	case GL_VERSION:
		return (GLubyte *) EGL_CONFIG_VERSION;

	case GL_RENDERER:
		return (GLubyte *) EGL_CONFIG_RENDERER;

	case GL_EXTENSIONS:
		return (GLubyte *) EGL_CONFIG_EXTENSIONS;

	default:
		RecordError(GL_INVALID_ENUM);
		return 0;
	}
}

void Context :: Finish(void) { }
void Context :: Flush(void) { }


void Context :: GetBooleanv(GLenum pname, GLboolean *params) {
	switch (pname) {
	case GL_LIGHT_MODEL_TWO_SIDE:
		params[0] = m_TwoSidedLightning;
		break;

	case GL_COLOR_WRITEMASK:
		{
			Color mask = m_RasterizerState.GetColorMask();
			params[0] = mask.R() != 0;
			params[1] = mask.G() != 0;
			params[2] = mask.B() != 0;
			params[3] = mask.A() != 0;
		}
		break;

	case GL_DEPTH_WRITEMASK:
		params[0] = m_RasterizerState.GetDepthMask();
		break;

	case GL_SAMPLE_COVERAGE_INVERT:
		params[0] = m_RasterizerState.GetSampleCoverageInvert();
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}


bool Context :: GetFixedv(GLenum pname, GLfixed *params) {
	switch (pname) {
	case GL_CURRENT_COLOR:
		CopyColor(m_DefaultRGBA, params);
		break;

	case GL_CURRENT_TEXTURE_COORDS:
		CopyVector(m_DefaultTextureCoords[m_ActiveTexture], params);
		break;

	case GL_CURRENT_NORMAL:
		params[0] = m_DefaultNormal.x();
		params[1] = m_DefaultNormal.y();
		params[2] = m_DefaultNormal.z();
		break;

	case GL_MODELVIEW_MATRIX:
		CopyMatrix(m_ModelViewMatrixStack.CurrentMatrix(), params);
		break;

	case GL_PROJECTION_MATRIX:
		CopyMatrix(m_ProjectionMatrixStack.CurrentMatrix(), params);
		break;

	case GL_TEXTURE_MATRIX:
		CopyMatrix(m_TextureMatrixStack[m_ActiveTexture].CurrentMatrix(), params);
		break;

	case GL_FOG_COLOR:
		CopyColor(m_RasterizerState.GetFogColor(), params);
		break;

	case GL_FOG_DENSITY:
		params[0] = m_FogDensity;
		break;

	case GL_FOG_START:
		params[0] = m_FogStart;
		break;

	case GL_FOG_END:
		params[0] = m_FogEnd;
		break;

	case GL_LIGHT_MODEL_AMBIENT:
		CopyColor(m_LightModelAmbient, params);
		break;

	case GL_COLOR_CLEAR_VALUE:
		CopyColor(m_ColorClearValue, params);
		break;

	case GL_ALIASED_LINE_WIDTH_RANGE:
	case GL_ALIASED_POINT_SIZE_RANGE:
	case GL_SMOOTH_LINE_WIDTH_RANGE:
	case GL_SMOOTH_POINT_SIZE_RANGE:
		params[0] = params[1] = EGL_ONE;
		break;

	case GL_POLYGON_OFFSET_UNITS:
		params[0] = m_RasterizerState.GetPolygonOffsetUnits();
		break;

	case GL_POLYGON_OFFSET_FACTOR:
		params[0] = m_RasterizerState.GetPolygonOffsetFactor();
		break;

	case GL_SAMPLE_COVERAGE_VALUE:
		params[0] = m_RasterizerState.GetSampleCoverage();
		break;

	case GL_POINT_SIZE_MIN:
		params[0] = m_PointSizeMin;
		break;

	case GL_POINT_SIZE_MAX:
		params[0] = m_PointSizeMax;
		break;

	case GL_POINT_FADE_THRESHOLD_SIZE:
		params[0] = m_PointFadeThresholdSize;
		break;

	case GL_POINT_DISTANCE_ATTENUATION:
		params[0] = m_PointDistanceAttenuation[0];
		params[1] = m_PointDistanceAttenuation[1];
		params[2] = m_PointDistanceAttenuation[2];
		break;

	case GL_DEPTH_RANGE:
		params[0] = m_DepthRangeNear;
		params[1] = m_DepthRangeFar;
		break;

	case GL_POINT_SIZE:
	case GL_LINE_WIDTH:
		params[0] = EGL_ONE;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return false;
	}

	return true;
}

void Context :: GetPointerv(GLenum pname, void **params) {
	switch (pname) {
	case GL_VERTEX_ARRAY_POINTER:
		params[0] = const_cast<void *>(m_VertexArray.pointer);
		break;

	case GL_NORMAL_ARRAY_POINTER:
		params[0] = const_cast<void *>(m_NormalArray.pointer);
		break;

	case GL_COLOR_ARRAY_POINTER:
		params[0] = const_cast<void *>(m_ColorArray.pointer);
		break;

	case GL_TEXTURE_COORD_ARRAY_POINTER:
		params[0] = const_cast<void *>(m_TexCoordArray[m_ClientActiveTexture].pointer);
		break;

	case GL_MATRIX_INDEX_ARRAY_POINTER_OES:
		params[0] = const_cast<void *>(m_MatrixIndexArray.pointer);
		break;

	case GL_WEIGHT_ARRAY_POINTER_OES:
		params[0] = const_cast<void *>(m_WeightArray.pointer);
		break;

	case GL_POINT_SIZE_ARRAY_POINTER_OES:
		params[0] = const_cast<void *>(m_PointSizeArray.pointer);
		break;


	default:
		RecordError(GL_INVALID_ENUM);
		break;
	}
}

GLboolean Context :: IsEnabled(GLenum cap) {
	switch (cap) {
	case GL_VERTEX_ARRAY:
		return m_VertexArrayEnabled;

	case GL_NORMAL_ARRAY:
		return m_NormalArrayEnabled;

	case GL_COLOR_ARRAY:
		return m_ColorArrayEnabled;

	case GL_TEXTURE_COORD_ARRAY:
		return m_TexCoordArrayEnabled[m_ClientActiveTexture];

	case GL_MATRIX_INDEX_ARRAY_OES:
		return m_MatrixIndexArrayEnabled;

	case GL_WEIGHT_ARRAY_OES:
		return m_WeightArrayEnabled;

	case GL_POINT_SIZE_ARRAY_OES:
		return m_PointSizeArrayEnabled;

	case GL_NORMALIZE:
		return m_NormalizeEnabled;

	case GL_RESCALE_NORMAL:
		return m_RescaleNormalEnabled;

	case GL_LIGHT0:
	case GL_LIGHT1:
	case GL_LIGHT2:
	case GL_LIGHT3:
	case GL_LIGHT4:
	case GL_LIGHT5:
	case GL_LIGHT6:
	case GL_LIGHT7:
		{
			int mask = 1 << (cap - GL_LIGHT0);
			return (m_LightEnabled & mask) != 0;
		}

	case GL_CLIP_PLANE0:
	case GL_CLIP_PLANE1:
	case GL_CLIP_PLANE2:
	case GL_CLIP_PLANE3:
	case GL_CLIP_PLANE4:
	case GL_CLIP_PLANE5:
		{
			U32 bit = 1u << (cap - GL_CLIP_PLANE0);
			return (m_ClipPlaneEnabled & bit) != 0;
		}

		break;

	case GL_FOG:
		return m_RasterizerState.IsEnabledFog();

	case GL_LIGHTING:
		return m_LightingEnabled;

	case GL_COLOR_MATERIAL:
		return m_ColorMaterialEnabled;

	case GL_POINT_SMOOTH:
		return m_RasterizerState.IsPointSmoothEnabled();

	case GL_LINE_SMOOTH:
		return m_RasterizerState.IsLineSmoothEnabled();

	case GL_CULL_FACE:
		return m_CullFaceEnabled;

	case GL_POLYGON_OFFSET_FILL:
		return m_RasterizerState.IsEnabledPolygonOffsetFill();

	case GL_SCISSOR_TEST:
		return m_ScissorTestEnabled;

	case GL_ALPHA_TEST:
		return m_RasterizerState.IsEnabledAlphaTest();

	case GL_STENCIL_TEST:
		return m_RasterizerState.IsEnabledStencilTest();

	case GL_DEPTH_TEST:
		return m_RasterizerState.IsEnabledDepthTest();

	case GL_BLEND:
		return m_RasterizerState.IsEnabledBlending();

	case GL_COLOR_LOGIC_OP:
		return m_RasterizerState.IsEnabledLogicOp();

	case GL_TEXTURE_2D:
		return m_RasterizerState.IsEnabledTexture(m_ActiveTexture);

	case GL_MATRIX_PALETTE_OES:
		return m_MatrixPaletteEnabled;

	case GL_POINT_SPRITE_OES:
		return m_RasterizerState.IsPointSpriteEnabled();

	case GL_MULTISAMPLE:
		return m_MultiSampleEnabled;

	case GL_SAMPLE_ALPHA_TO_COVERAGE:
		return m_SampleAlphaToCoverageEnabled;

	case GL_SAMPLE_ALPHA_TO_ONE:
		return m_SampleAlphaToOneEnabled;

	case GL_SAMPLE_COVERAGE:
		return m_SampleCoverageEnabled;

	case GL_DITHER:
		return m_DitherEnabled;

	default:
		RecordError(GL_INVALID_ENUM);
		return false;
	}
}
