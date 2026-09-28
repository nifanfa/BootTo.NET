#ifndef EGL_RASTERIZER_H
#define EGL_RASTERIZER_H 1

// ==========================================================================
//
// Rasterizer.h		Rasterizer Class for 3D Rendering Library
//
//					The rasterizer converts transformed and lit 
//					primitives and creates a raster image in the
//					current rendering surface.
//
// --------------------------------------------------------------------------
//
// 10-06-2003		Hans-Martin Will	initial version
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
#include "fixed.h"
#include "linalg.h"
#include "RasterizerState.h"
#include "FractionalColor.h"
#include "Surface.h"


namespace EGL {

	class Texture;
	class FunctionCache;

	// ----------------------------------------------------------------------
	// u and v coordinates for texture mapping
	// ----------------------------------------------------------------------
	struct TexCoord {
		EGL_Fixed		tu, tv;			// texture coordinates between 0 and 1

#if EGL_MIPMAP_PER_TEXEL
		EGL_Fixed		dtudx, dtvdx;	// partial derivatives
		EGL_Fixed		dtudy, dtvdy;	// partial derivatives
#endif
	};

	struct ScreenCoord {
		EGL_Fixed		x, y;		// x, y window coords
		EGL_Fixed		invZ;		// 1/Z (w) from depth division

		// TO DO: once the rasterizer works properly, change this to binary 0..2^n-1
		EGL_Fixed		depth;		// depth coordinate after transformation
	};

	struct EdgeCoord {
		EGL_Fixed		x, invZ;	// x window coords

		// TO DO: once the rasterizer works properly, change this to binary 0..2^n-1
		EGL_Fixed		depth;		// depth coordinate
	};

	// ----------------------------------------------------------------------
	// Vertex information as input for rasterizer
	// ----------------------------------------------------------------------

	struct RasterPos {
		Vec4D				m_EyeCoords;
		Vec4D				m_ClipCoords;
		ScreenCoord			m_WindowCoords;		
		FractionalColor		m_Color;
		FractionalColor		m_FrontColor;		// color in range 0..255
		FractionalColor		m_BackColor;		
		TexCoord			m_TextureCoords[EGL_NUM_TEXTURE_UNITS];	// texture coords 0..1
		EGL_Fixed			m_FogDensity;		// fog density at this vertex
	};

	// ----------------------------------------------------------------------
	// Coordinate increments used in various parts of the rasterizer
	// ----------------------------------------------------------------------

	struct EdgePos {
		EdgeCoord			m_WindowCoords;								// z over w
		FractionalColor		m_Color;									// color in range 0..255
		TexCoord			m_TextureCoords[EGL_NUM_TEXTURE_UNITS];		// texture coords 0..1 over w
		EGL_Fixed			m_FogDensity;								// fog density at this vertex
	};

	// ----------------------------------------------------------------------
	// Gradient data structure used for triangle rasterization
	// ----------------------------------------------------------------------

	struct Gradients {
		EdgePos				dx;
		EdgePos				dy;
	};

	struct EdgePos;

	struct RasterInfo {
		// surface info
		I32			SurfaceWidth;
		I32			SurfaceHeight;
#ifdef RE3_GENERIC
		U32 *		DepthBuffer;
#else
		U16 *		DepthBuffer;
#endif
		U16 *		ColorBuffer;
		U32 *		StencilBuffer;
		U8 *		AlphaBuffer;
		const I32 *	InversionTablePtr;

		// TODO: will need to add a minimum texture level here
		// TODO: 

		// texture info
		Texture *	Textures[EGL_NUM_TEXTURE_UNITS];
		U32			MipmapLevel[EGL_NUM_TEXTURE_UNITS];
		U32			MaxMipmapLevel[EGL_NUM_TEXTURE_UNITS];

        void Init(Surface * surface, I32 y);
	};

	// signature for generated scanline functions
	typedef void (ScanlineFunction)(const RasterInfo * info, const EdgePos * start, const EdgePos * end);
	typedef void (LineFunction)(const RasterInfo * info, const RasterPos * from, const RasterPos * to);
	typedef void (PointFunction)(const RasterInfo * info, const RasterPos * pos, EGL_Fixed size);

	class Rasterizer {

	public:
		enum {
			PolygonOffsetUnitSize = 1,		// how to determine this?
#ifdef RE3_GENERIC
			DepthRangeMax = 0xffffff
#else
			DepthRangeMax = 0xffff
#endif
		};

		enum PixelFormat {
			PixelFormatRGBA,
			PixelFormatRGB,
			PixelFormatLuminanceAlpha,
			PixelFormatLuminance,
			PixelFormatAlpha
		};

		enum PixelType {
			PixelTypeUnsignedByte,
			PixelTypeUnsignedShort_4_4_4_4,
			PixelTypeUnsignedShort_5_5_5_1,
			PixelTypeUnsignedShort_5_6_5
		};


	public:
		Rasterizer(RasterizerState * state);
		~Rasterizer();

		// ----------------------------------------------------------------------
		// Rendering surface
		// ----------------------------------------------------------------------
		void SetState(RasterizerState * state);
		RasterizerState * GetState() const;

		void SetSurface(Surface * surface);
		Surface * GetSurface() const;

		void SetTexture(size_t unit, MultiTexture * texture);
		MultiTexture * GetTexture(size_t unit)				{ return m_Texture[unit]; }
		const MultiTexture * GetTexture(size_t unit) const	{ return m_Texture[unit]; }
		void PrepareTexture();

		// ----------------------------------------------------------------------
		// Actual rasterization of primitives
		// These functions take care of anything downstream of the
		// scissor test, scan conversion, texturing, compositing, depth &
		// stencil test.
		// ----------------------------------------------------------------------

		typedef void (Rasterizer::*RasterTriangleFunction)(const RasterPos& a, const RasterPos& b,
			const RasterPos& c);

		void RasterPoint(const RasterPos& point, EGL_Fixed size);
		void RasterLine(RasterPos& from, RasterPos& to);
		void RasterTriangle(const RasterPos& a, const RasterPos& b,
			const RasterPos& c);

		// ----------------------------------------------------------------------
		// State management
		// ----------------------------------------------------------------------

		void PreparePoint();
		void PrepareLine();
		void PrepareTriangle();
		void Finish();
		//void UpdateWindowClipping(void);

		// ----------------------------------------------------------------------
		// Include actual rasterization functions here
		// ----------------------------------------------------------------------
		//#include "generated_rasterization_function_declarations.h"

	private:
		// ----------------------------------------------------------------------
		// Rasterization of triangle scan line
		// ----------------------------------------------------------------------
		void RasterScanLine(RasterInfo & info, const EdgePos & start, const EdgePos & end);

		// ----------------------------------------------------------------------
		// Rasterization of triangle
		//
		// Variations are: All, +/- color [Cc], +/- texture [Tt], +/- depth [Dd], +/- fog [Ff], +/- scissor [Ss]
		// combination ct does not exist
		// ----------------------------------------------------------------------
		void RasterTriangleAll(const RasterPos& a, const RasterPos& b, const RasterPos& c);

		enum RasterTriangleBits {
			RasterTriangleColor,
			RasterTriangleTexture,
			RasterTriangleDepth,
			RasterTriangleFog,
			RasterTriangleScissor,
			RasterTriangleStencil,
			RasterTriangleCount,
		};

		void RasterTriangle_cTdfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTdFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTDfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTDFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_Ctdfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtdFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtDfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtDFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTdfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTdFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTDfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTDFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTdfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTdFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTDfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_cTDFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtdfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtdFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtDfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CtDFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTdfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTdFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTDfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
		void RasterTriangle_CTDFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);

		// ----------------------------------------------------------------------
		// Rasterization of fragment
		// ----------------------------------------------------------------------

		void Fragment(I32 x, I32 y, EGL_Fixed depth, EGL_Fixed tu[], EGL_Fixed tv[], 
			EGL_Fixed fogDensity, const Color& baseColor);
			// will have special cases based on settings
			// the coordinates are integer coordinates

		void Fragment(const RasterInfo * rasterInfo, I32 x, EGL_Fixed depth, 
					  EGL_Fixed tu[], EGL_Fixed tv[],
					  const Color& baseColor, EGL_Fixed fog);
			// fragment rendering with signature corresponding to function fragment
			// generated by code generator

		Color GetTexColor(const RasterizerState::TextureState * state, const Texture * texture, EGL_Fixed tu, EGL_Fixed tv, 
						  RasterizerState::FilterMode filterMode);
			// retrieve the texture color from a texture plane 
						  
		Color GetRawTexColor(const RasterizerState::TextureState * state, const Texture * texture, EGL_Fixed tu, EGL_Fixed tv);
			// retrieve the texture color from a texture plane
						  
	private:
		// ----------------------------------------------------------------------
		// other settings
		// ----------------------------------------------------------------------

		RasterInfo				m_RasterInfo;
		Surface *				m_Surface;			// rendering surface
		MultiTexture *			m_Texture[EGL_NUM_TEXTURE_UNITS];	// current texture 
		RasterizerState *		m_State;			// current rasterization settings
		FunctionCache *			m_FunctionCache;

		ScanlineFunction *		m_ScanlineFunction;
		LineFunction *			m_LineFunction;		// raster lines function
		PointFunction *			m_PointFunction;

		RasterTriangleFunction	m_RasterTriangleFunction;

		RasterTriangleFunction	m_RasterTriangleFunctions[1 << RasterTriangleCount];

		// ----------------------------------------------------------------------
		// internal state
		// ----------------------------------------------------------------------
		EGL_Fixed				m_MinX;
		EGL_Fixed				m_MaxX;
		EGL_Fixed				m_MinY;
		EGL_Fixed				m_MaxY;

		bool					m_UseMipmap[EGL_NUM_TEXTURE_UNITS];
	};


	// --------------------------------------------------------------------------
	// Inline member definitions
	// --------------------------------------------------------------------------


	inline void Rasterizer :: SetSurface(Surface * surface) {
		m_Surface = surface;
	}


	inline Surface * Rasterizer :: GetSurface() const {
		return m_Surface;
	}


	inline void Rasterizer :: RasterTriangle(const RasterPos& a, const RasterPos& b, const RasterPos& c) {
		(this->*m_RasterTriangleFunction)(a, b, c);
	}


#	if EGL_USE_JIT

	inline void Rasterizer :: RasterPoint(const RasterPos& point, EGL_Fixed size) {
		m_PointFunction(&m_RasterInfo, &point, size);
	}

	inline void Rasterizer :: RasterLine(RasterPos& p_from, RasterPos& p_to) {
		p_from.m_WindowCoords.x = ((p_from.m_WindowCoords.x + 0x800) & ~0xfff);
		p_from.m_WindowCoords.y = ((p_from.m_WindowCoords.y + 0x800) & ~0xfff);
		p_to.m_WindowCoords.x = ((p_to.m_WindowCoords.x + 0x800) & ~0xfff);
		p_to.m_WindowCoords.y = ((p_to.m_WindowCoords.y + 0x800) & ~0xfff);

		m_LineFunction(&m_RasterInfo, &p_from, &p_to);
	}

	inline void Rasterizer :: RasterScanLine(RasterInfo & rasterInfo, const EdgePos & start, const EdgePos & end) {
		m_ScanlineFunction(&rasterInfo, &start, &end);
	}

#	endif // EGL_USE_JIT

}



#endif //ndef EGL_RASTERIZER_H

