#ifndef EGL_CONTEXT_H
#define EGL_CONTEXT_H 1

// ==========================================================================
//
// context.h	Rendering Context Class for 3D Rendering Library
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


#include "OGLES.h"
#include "GLES/gl.h"
#include "GLES/glext.h"
#include "Types.h"
#include "Arrays.h"
#include "linalg.h"
#include "FractionalColor.h"
#include "Config.h"
#include "Material.h"
#include "Light.h"
#include "RasterizerState.h"
#include "Rasterizer.h"
#include "MatrixStack.h"
#include "Texture.h"


namespace EGL {

	class Surface;
	class MultiTexture;

	#define EGL_NUMBER_LIGHTS 8

	class Context {

		// ----------------------------------------------------------------------
		// Default viewport configuration
		// ----------------------------------------------------------------------
		enum {
			NUM_CLIP_PLANES = 6,
			MATRIX_PALETTE_SIZE = 9,

			VIEWPORT_X = 0,
			VIEWPORT_Y = 0,
			VIEWPORT_WIDTH = 240,
			VIEWPORT_HEIGHT = 320,
			VIEWPORT_NEAR = 0,
			VIEWPORT_FAR = EGL_ONE
		};

		enum CullMode {
			CullModeBack,
			CullModeFront,
			CullModeBackAndFront
		};

		enum FogMode {
			FogLinear,
			FogModeExp,
			FogModeExp2
		};

	public:
		Context(const Config & config);

		~Context();

		// ----------------------------------------------------------------------
		// Fixed point API
		// ----------------------------------------------------------------------

		void ActiveTexture(GLenum texture);
		void AlphaFuncx(GLenum func, GLclampx ref);
		void BindBuffer(GLenum target, GLuint buffer);
		void BindTexture(GLenum target, GLuint texture);
		void BlendFunc(GLenum sfactor, GLenum dfactor);
		void BufferData(GLenum target, GLsizeiptr size, const GLvoid *data, GLenum usage);
		void BufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid *data);
		void Clear(GLbitfield mask);
		void ClearColorx(GLclampx red, GLclampx green, GLclampx blue, GLclampx alpha);
		void ClearDepthx(GLclampx depth);
		void ClearStencil(GLint s);
		void ClientActiveTexture(GLenum texture);
		void ClipPlanex(GLenum plane, const GLfixed *equation);
		void Color4x(GLfixed red, GLfixed green, GLfixed blue, GLfixed alpha);
		void ColorMask(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha);
		void ColorPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer);
		void CompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const GLvoid *data);
		void CompressedTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, const GLvoid *data);
		void CopyTexImage2D(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border);
		void CopyTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height);
		void CullFace(GLenum mode);
		void DeleteBuffers(GLsizei n, const GLuint *buffers);
		void DeleteTextures(GLsizei n, const GLuint *textures);
		void DepthFunc(GLenum func);
		void DepthMask(GLboolean flag);
		void DepthRangex(GLclampx zNear, GLclampx zFar);
		void Disable(GLenum cap);
		void DisableClientState(GLenum array);
		void DrawArrays(GLenum mode, GLint first, GLsizei count);
		void DrawElements(GLenum mode, GLsizei count, GLenum type, const GLvoid *indices);
		void Enable(GLenum cap);
		void EnableClientState(GLenum array);
		void Finish(void);
		void Flush(void);
		void Fogx(GLenum pname, GLfixed param);
		void Fogxv(GLenum pname, const GLfixed *params);
		void FrontFace(GLenum mode);
		void Frustumx(GLfixed left, GLfixed right, GLfixed bottom, GLfixed top, GLfixed zNear, GLfixed zFar);
		void GetBooleanv(GLenum pname, GLboolean *params);
		void GetBufferParameteriv(GLenum target, GLenum pname, GLint *params);
		void GetClipPlanex(GLenum pname, GLfixed eqn[4]);
		void GenBuffers(GLsizei n, GLuint *buffers);
		void GenTextures(GLsizei n, GLuint *textures);
		GLenum GetError(void);
		bool GetFixedv(GLenum pname, GLfixed *params);
		void GetIntegerv(GLenum pname, GLint *params);
		bool GetLightxv(GLenum light, GLenum pname, GLfixed *params);
		bool GetMaterialxv(GLenum face, GLenum pname, GLfixed *params);
		void GetPointerv(GLenum pname, void **params);
		void GetTexEnviv(GLenum env, GLenum pname, GLint *params);
		bool GetTexEnvxv(GLenum env, GLenum pname, GLfixed *params);
		void GetTexParameteriv(GLenum target, GLenum pname, GLint *params);
		void GetTexParameterxv(GLenum target, GLenum pname, GLfixed *params);
		const GLubyte * GetString(GLenum name);
		void Hint(GLenum target, GLenum mode);
		GLboolean IsBuffer(GLuint buffer);
		GLboolean IsEnabled(GLenum cap);
		GLboolean IsTexture(GLuint texture);
		void LightModelx(GLenum pname, GLfixed param);
		void LightModelxv(GLenum pname, const GLfixed *params);
		void Lightx(GLenum light, GLenum pname, GLfixed param);
		void Lightxv(GLenum light, GLenum pname, const GLfixed *params);
		void LineWidthx(GLfixed width);
		void LoadIdentity(void);
		void LoadMatrixx(const GLfixed *m);
		void LogicOp(GLenum opcode);
		void Materialx(GLenum face, GLenum pname, GLfixed param);
		void Materialxv(GLenum face, GLenum pname, const GLfixed *params);
		void MatrixMode(GLenum mode);
		void MultMatrixx(const GLfixed *m);
		void MultiTexCoord4x(GLenum target, GLfixed s, GLfixed t, GLfixed r, GLfixed q);
		void Normal3x(GLfixed nx, GLfixed ny, GLfixed nz);
		void NormalPointer(GLenum type, GLsizei stride, const GLvoid *pointer);
		void Orthox(GLfixed left, GLfixed right, GLfixed bottom, GLfixed top, GLfixed zNear, GLfixed zFar);
		void PixelStorei(GLenum pname, GLint param);
		void PointParameterx(GLenum pname, GLfixed param);
		void PointParameterxv(GLenum pname, const GLfixed *params);
		void PointSizex(GLfixed size);
		void PolygonOffsetx(GLfixed factor, GLfixed units);
		void PopMatrix(void);
		void PushMatrix(void);
		void ReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLvoid *pixels);
		void Rotatex(GLfixed angle, GLfixed x, GLfixed y, GLfixed z);
		void SampleCoveragex(GLclampx value, GLboolean invert);
		void Scalex(GLfixed x, GLfixed y, GLfixed z);
		void Scissor(GLint x, GLint y, GLsizei width, GLsizei height);
		void ShadeModel(GLenum mode);
		void StencilFunc(GLenum func, GLint ref, GLuint mask);
		void StencilMask(GLuint mask);
		void StencilOp(GLenum fail, GLenum zfail, GLenum zpass);
		void TexCoordPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer);
		void TexEnvx(GLenum target, GLenum pname, GLfixed param);
		void TexEnvxv(GLenum target, GLenum pname, const GLfixed *params);
		void TexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const GLvoid *pixels);
		void TexParameterx(GLenum target, GLenum pname, GLfixed param);
		void TexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const GLvoid *pixels);
		void Translatex(GLfixed x, GLfixed y, GLfixed z);
		void VertexPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer);
		void Viewport(GLint x, GLint y, GLsizei width, GLsizei height);

		// ----------------------------------------------------------------------
		// Floating Point API
		// ----------------------------------------------------------------------

		void AlphaFunc(GLenum func, GLclampf ref);
		void ClearColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
		void ClearDepthf(GLclampf depth);
		void ClipPlanef(GLenum plane, const GLfloat *equation);
		void Color4f(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);
		void DepthRangef(GLclampf zNear, GLclampf zFar);
		void DrawTexf(GLfloat x, GLfloat y, GLfloat z, GLfloat width, GLfloat height);
		void DrawTexfv(GLfloat *coords);
		void Fogf(GLenum pname, GLfloat param);
		void Fogfv(GLenum pname, const GLfloat *params);
		void Frustumf(GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar);
		void GetClipPlanef(GLenum pname, GLfloat eqn[4]);
		void GetFloatv(GLenum pname, GLfloat *params);
		void GetLightfv(GLenum light, GLenum pname, GLfloat *params);
		void GetMaterialfv(GLenum face, GLenum pname, GLfloat *params);
		void GetTexEnvfv(GLenum env, GLenum pname, GLfloat *params);
		void GetTexParameterfv(GLenum target, GLenum pname, GLfloat *params);
		void LightModelf(GLenum pname, GLfloat param);
		void LightModelfv(GLenum pname, const GLfloat *params);
		void Lightf(GLenum light, GLenum pname, GLfloat param);
		void Lightfv(GLenum light, GLenum pname, const GLfloat *params);
		void LineWidth(GLfloat width);
		void LoadMatrixf(const GLfloat *m);
		void Materialf(GLenum face, GLenum pname, GLfloat param);
		void Materialfv(GLenum face, GLenum pname, const GLfloat *params);
		void MultMatrixf(const GLfloat *m);
		GLbitfield QueryMatrixx(GLfixed mantissa[16], GLint exponent[16]);
		void MultiTexCoord4f(GLenum target, GLfloat s, GLfloat t, GLfloat r, GLfloat q);
		void Normal3f(GLfloat nx, GLfloat ny, GLfloat nz);
		void Orthof(GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar);
		void PointParameterf(GLenum pname, GLfloat param);
		void PointParameterfv(GLenum pname, const GLfloat *params);
		void PointSize(GLfloat size);
		void PolygonOffset(GLfloat factor, GLfloat units);
		void Rotatef(GLfloat angle, GLfloat x, GLfloat y, GLfloat z);
		void SampleCoverage(GLclampf value, GLboolean invert);
		void Scalef(GLfloat x, GLfloat y, GLfloat z);
		void TexEnvf(GLenum target, GLenum pname, GLfloat param);
		void TexEnvfv(GLenum target, GLenum pname, const GLfloat *params);
		void TexEnvi(GLenum target, GLenum pname, GLint param);
		void TexEnviv(GLenum target, GLenum pname, const GLint* params);
		void TexParameteri(GLenum target, GLenum pname, GLint param);
		void TexParameterf(GLenum target, GLenum pname, GLfloat param);
		void TexParameterfv(GLenum target, GLenum pname, const GLfloat *params);
		void TexParameteriv(GLenum target, GLenum pname, const GLint *params);
		void TexParameterxv(GLenum target, GLenum pname, const GLfixed *params);
		void Translatef(GLfloat x, GLfloat y, GLfloat z);

		// ----------------------------------------------------------------------
		// Extensions
		// ----------------------------------------------------------------------

		/* OES_point_size_array */
		void PointSizePointer(GLenum type, GLsizei stride, const GLvoid *pointer);

		/* OES_matrix_palette */
		void CurrentPaletteMatrix(GLuint index);
		void LoadPaletteFromModelViewMatrix(void);
		void MatrixIndexPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer);
		void WeightPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer);

		// ----------------------------------------------------------------------
		// Context Management Functions
		// ----------------------------------------------------------------------

		void Dispose();
		void SetCurrent(bool current);
		Config * GetConfig();

		RasterizerState * GetRasterizerState() {
			return &m_RasterizerState;
		}

		const RasterizerState * GetRasterizerState() const {
			return &m_RasterizerState;
		}

		Rasterizer * GetRasterizer() {
			return m_Rasterizer;
		}

		MultiTexture * GetCurrentTexture() {
			return m_Rasterizer->GetTexture(m_ActiveTexture);
		}

		const MultiTexture * GetCurrentTexture() const {
			return m_Rasterizer->GetTexture(m_ActiveTexture);
		}

		static void SetCurrentContext(Context * context);
		static Context * GetCurrentContext();
		static Context * DefaultContext();

		void SetReadSurface(Surface * surface);
		void SetDrawSurface(Surface * surface);
		Surface * GetReadSurface() const;
		Surface * GetDrawSurface() const;

	private:
		// ----------------------------------------------------------------------
		// Private Functions
		// ----------------------------------------------------------------------

		inline MatrixStack * CurrentMatrixStack() {
			return m_CurrentMatrixStack;
		}

		inline const Matrix4x4& CurrentMatrix() {
			return CurrentMatrixStack()->CurrentMatrix();
		}

		void UpdateInverseModelViewMatrix(void);
		void RebuildMatrices(void);
		void MultMatrix(const Matrix4x4 & m);

		// SGIS_generate_mipmap extension
		void UpdateMipmaps(void);

		// clipping rectangle recalculation
		void UpdateScissorTest(void);

		void RecordError(GLenum error);
		void Toggle(GLenum cap, bool value);
		void ToggleClientState(GLenum array, bool value);

		// ----------------------------------------------------------------------
		// Private Functions - Rendering of collections
		// ----------------------------------------------------------------------

		void RenderPoints(GLint first, GLsizei count);
		void RenderPoints(GLsizei count, const GLubyte * indices);
		void RenderPoints(GLsizei count, const GLushort * indices);

		void RenderLines(GLint first, GLsizei count);
		void RenderLines(GLsizei count, const GLubyte * indices);
		void RenderLines(GLsizei count, const GLushort * indices);

		void RenderLineStrip(GLint first, GLsizei count);
		void RenderLineStrip(GLsizei count, const GLubyte * indices);
		void RenderLineStrip(GLsizei count, const GLushort * indices);

		void RenderLineLoop(GLint first, GLsizei count);
		void RenderLineLoop(GLsizei count, const GLubyte * indices);
		void RenderLineLoop(GLsizei count, const GLushort * indices);

		void RenderTriangles(GLint first, GLsizei count);
		void RenderTriangles(GLsizei count, const GLubyte * indices);
		void RenderTriangles(GLsizei count, const GLushort * indices);

		void RenderTriangleStrip(GLint first, GLsizei count);
		void RenderTriangleStrip(GLsizei count, const GLubyte * indices);
		void RenderTriangleStrip(GLsizei count, const GLushort * indices);

		void RenderTriangleFan(GLint first, GLsizei count);
		void RenderTriangleFan(GLsizei count, const GLubyte * indices);
		void RenderTriangleFan(GLsizei count, const GLushort * indices);

		// ----------------------------------------------------------------------
		// Private Functions - Rendering of individual elements
		// ----------------------------------------------------------------------

private:
		void SelectArrayElement(int index);
		EGL_Fixed SelectPointSizeArrayElement(int index);

		void CurrentValuesToRasterPos(RasterPos * rasterPos);
		void CurrentTextureValuesToRasterPos(RasterPos * rasterPos);

		typedef void (Context::*GeometryFunction)(RasterPos * rasterPos);

		void PrepareRendering();
		void PrepareArray(VertexArray & array, bool enabled, bool isColor = false);

		void CurrentValuesToRasterPosNoLight(RasterPos * rasterPos);
		void CurrentValuesToRasterPosOneSidedNoTrack(RasterPos * rasterPos);
		void CurrentValuesToRasterPosOneSidedTrack(RasterPos * rasterPos);
		void CurrentValuesToRasterPosTwoSidedNoTrack(RasterPos * rasterPos);
		void CurrentValuesToRasterPosTwoSidedTrack(RasterPos * rasterPos);

		void InterpolateRasterPos(RasterPos * a, RasterPos * b, GLfixed x, RasterPos * result);

private:
		// ----------------------------------------------------------------------
		// Perform clipping, depth division & actual call into rasterizer
		// ----------------------------------------------------------------------
		void RenderPoint(RasterPos& point, EGL_Fixed size);
		void RenderLine(RasterPos& from, RasterPos& to);
		void RenderTriangle(RasterPos& a, RasterPos& b, RasterPos& c);
		bool IsCulled(RasterPos& a, RasterPos& b, RasterPos& c);

		void ClipCoordsToWindowCoords(RasterPos & pos);
		EGL_Fixed FogDensity(EGL_Fixed eyeDistance) const;

		void InitFogTable();

		size_t * CurrentBufferForTarget(GLenum target);

	private:
		GLenum				m_LastError;

		// ----------------------------------------------------------------------
		// Matrix stacks
		// ----------------------------------------------------------------------

		MatrixStack			m_ModelViewMatrixStack;
		MatrixStack			m_ProjectionMatrixStack;
		MatrixStack			m_TextureMatrixStack[EGL_NUM_TEXTURE_UNITS];
		Matrix4x4			m_MatrixPalette[MATRIX_PALETTE_SIZE];
		Matrix4x4			m_MatrixPaletteInverse[MATRIX_PALETTE_SIZE];

		MatrixStack *		m_CurrentMatrixStack;
		Matrix4x4			m_InverseModelViewMatrix;
		Matrix4x4			m_FullInverseModelViewMatrix;
		I32					m_CurrentPaletteMatrix;
		EGL_Fixed			m_CurrentWeights[MATRIX_PALETTE_SIZE];
		U8					m_PaletteMatrixIndex[MATRIX_PALETTE_SIZE];
		GLenum				m_MatrixMode;

		// ----------------------------------------------------------------------
		// Viewport configuration
		// ----------------------------------------------------------------------

		Rect				m_Viewport;
		Rect				m_Scissor;
		GLclampx			m_ViewportNear;
		GLclampx			m_ViewportFar;
		bool				m_ScissorTestEnabled;

		// origin and scale for actual transformation
		Vec3D				m_ViewportOrigin;
		Vec3D				m_ViewportScale;

		// ----------------------------------------------------------------------
		// Client state variables
		// ----------------------------------------------------------------------

		bool				m_VertexArrayEnabled;
		bool				m_NormalArrayEnabled;
		bool				m_ColorArrayEnabled;
		bool				m_TexCoordArrayEnabled[EGL_NUM_TEXTURE_UNITS];
		bool				m_PointSizeArrayEnabled;
		bool				m_WeightArrayEnabled;
		bool				m_MatrixIndexArrayEnabled;

		VertexArray			m_VertexArray;
		VertexArray			m_NormalArray;
		VertexArray			m_ColorArray;
		VertexArray			m_TexCoordArray[EGL_NUM_TEXTURE_UNITS];
		VertexArray			m_PointSizeArray;
		VertexArray			m_WeightArray;
		VertexArray			m_MatrixIndexArray;

		// ----------------------------------------------------------------------
		// Default values if arrays are disabled
		// ----------------------------------------------------------------------

		Vec3D				m_DefaultNormal;
		FractionalColor		m_DefaultRGBA;
		Vec4D				m_DefaultTextureCoords[EGL_NUM_TEXTURE_UNITS];
		TexCoord			m_DefaultScaledTextureCoords[EGL_NUM_TEXTURE_UNITS];
		size_t				m_ActiveTexture;
		size_t				m_ClientActiveTexture;

		// ----------------------------------------------------------------------
		// Current values for setup
		// ----------------------------------------------------------------------

		Vec4D				m_CurrentVertex;	
		Vec3D				m_CurrentNormal;
		FractionalColor		m_CurrentRGBA;
		TexCoord			m_CurrentTextureCoords[EGL_NUM_TEXTURE_UNITS];

		// ----------------------------------------------------------------------
		// Hints
		// ----------------------------------------------------------------------

		GLenum				m_PerspectiveCorrectionHint;
		GLenum				m_PointSmoothHint;
		GLenum				m_LineSmoothHint;
		GLenum				m_FogHint;
		GLenum				m_GenerateMipmapHint;

		// ----------------------------------------------------------------------
		// Rendering State
		// ----------------------------------------------------------------------

		Vec4D				m_ClipPlanes[NUM_CLIP_PLANES];
		U32					m_ClipPlaneEnabled;
		Light				m_Lights[EGL_NUMBER_LIGHTS];
		Material			m_FrontMaterial;
		FractionalColor		m_LightModelAmbient;
		GLint				m_LightEnabled;

		EGL_Fixed			m_DepthClearValue;
		FractionalColor		m_ColorClearValue;
		U32					m_StencilClearValue;

		FogMode				m_FogMode;			// the fog color is still in the
		EGL_Fixed			m_FogStart, m_FogDensity, m_FogEnd;	// rasterizer state
		EGL_Fixed			m_FogGradient;
		U8					m_FogGradientShift;

		EGL_Fixed			m_DepthRangeNear, m_DepthRangeFar;
		EGL_Fixed			m_DepthRangeBase, m_DepthRangeFactor;

		EGL_Fixed			m_PointSize;
		EGL_Fixed			m_PointSizeMin, m_PointSizeMax;
		EGL_Fixed			m_PointFadeThresholdSize;
		EGL_Fixed			m_PointDistanceAttenuation[3];
		bool				m_PointSizeAttenuate;

		bool				m_LightingEnabled;		// is lightning enabled?
		bool				m_TwoSidedLightning;	// do we have two-sided lightning
		bool				m_CullFaceEnabled;
		bool				m_DitherEnabled;
		bool				m_ReverseFaceOrientation;
		bool				m_ColorMaterialEnabled;
		bool				m_NormalizeEnabled;
		bool				m_RescaleNormalEnabled;
		bool				m_PolygonOffsetFillEnabled;
		bool				m_MultiSampleEnabled;
		bool				m_SampleAlphaToCoverageEnabled;
		bool				m_SampleAlphaToOneEnabled;
		bool				m_SampleCoverageEnabled;
		bool				m_GenerateMipmaps;
		bool				m_MatrixPaletteEnabled;
		bool				m_MatrixModePaletteEnabled;

		I32					m_PixelStorePackAlignment;
		I32					m_PixelStoreUnpackAlignment;

		CullMode			m_CullMode;

		RasterizerState		m_RasterizerState;
		Rasterizer *		m_Rasterizer;
		GeometryFunction	m_GeometryFunction;

		// ----------------------------------------------------------------------
		// texturing related state
		// ----------------------------------------------------------------------

		TextureArray		m_Textures;

		// ----------------------------------------------------------------------
		// vertex buffer related state
		// ----------------------------------------------------------------------

		BufferArray			m_Buffers;
		size_t				m_CurrentArrayBuffer;
		size_t				m_CurrentElementArrayBuffer;

		// ----------------------------------------------------------------------
		// Object-Life Cycle State
		// ----------------------------------------------------------------------

		Surface *			m_DrawSurface;		// current surface for drawing
		Surface *			m_ReadSurface;		// current surface for reading

		Config				m_Config;			// copy of configuration args
		bool				m_Current;			// this context has been selected
												// as a current context
		bool				m_Disposed;			// this context has been deleted,
												// but is stil selected into a 
												// thread
		bool				m_ViewportInitialized;	// if true, the viewport has been
													// initialized

		RasterPos			m_Temporary[16];	// temporary coordinates
	};


	inline Config * Context :: GetConfig() {
		return &m_Config;
	}


	inline Context * Context :: DefaultContext() {
		return Context::GetCurrentContext();
	}


	inline Surface * Context :: GetDrawSurface() const {
		return m_DrawSurface;
	}


	inline Surface * Context :: GetReadSurface() const {
		return m_ReadSurface;
	}


	inline void Context :: CurrentValuesToRasterPos(RasterPos * rasterPos) {
		(this->*m_GeometryFunction)(rasterPos);
	}


}

#endif //ndef EGL_CONTEXT_H
