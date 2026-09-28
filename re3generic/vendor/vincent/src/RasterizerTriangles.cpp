// ==========================================================================
//
// RasterizerTraingles.cpp	Rasterizer Class for 3D Rendering Library
//
// The rasterizer converts transformed and lit primitives and creates a 
// raster image in the current rendering surface.
//
// This files contains the triangle rasterization code, which was
// previously in the Rasterizer.cpp source file.
//
// --------------------------------------------------------------------------
//
// 05-22-2004		Hans-Martin Will	initial version
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
#include "Rasterizer.h"
#include "Surface.h"
#include "Texture.h"

using namespace EGL;


namespace {
	const I8 Permutation[8][3] = {
		{ 0, 1, 2 },
		{ 0, 2, 1 },
		{ 0, 0, 0 },	// impossible
		{ 2, 0, 1 },
		{ 1, 0, 2 },
		{ 0, 0, 0 },	// impossible
		{ 1, 2, 0 },
		{ 2, 1, 0 },
	};

	inline const I8 * SortPermutation(I32 x0, I32 x1, I32 x2) {
		U32 y0 = static_cast<U32>(x0);
		U32 y1 = static_cast<U32>(x1);
		U32 y2 = static_cast<U32>(x2);

		const I8 * result = Permutation[
			(((y1 - y0) >> 29) & 4) |
			(((y2 - y0) >> 30) & 2) |
			(((y2 - y1) >> 31) & 1)];

		assert(result[0] | result[1] | result[2]);

		return result;
	}

	inline int Greater(I32 x0, I32 x1) {
		U32 y0 = static_cast<U32>(x0);
		U32 y1 = static_cast<U32>(x1);

		return (y1 - y0) >> 31;
	}
	
	inline EGL_Fixed TriangleArea(EGL_Fixed x0, EGL_Fixed y0,
								  EGL_Fixed x1, EGL_Fixed y1, 
								  EGL_Fixed x2, EGL_Fixed y2) {
		return 
			EGL_Abs(
				EGL_Mul(x1, y2) + EGL_Mul(x2, y0) + EGL_Mul(x0, y1)
				- EGL_Mul(x2, y1) - EGL_Mul(x0, y2) - EGL_Mul(x1, y0));
	}

	inline int Log2(int value) {
		if (value <= 1) {
			return 0;
		}

		int result = 0;

		while (value > 1) {
			result++;
			value >>= 1;
		}

		return result;
	}

#define DET_SHIFT 4

	inline EGL_Fixed Det2x2(EGL_Fixed a11, EGL_Fixed a12, EGL_Fixed a21, EGL_Fixed a22) {
		return EGL_Mul(a11 >> DET_SHIFT, a22 >> DET_SHIFT) -
			EGL_Mul(a12 >> DET_SHIFT, a21 >> DET_SHIFT);
	}

	inline EGL_Fixed Det2x2Gradient(EGL_Fixed a11, EGL_Fixed a12, EGL_Fixed a21, EGL_Fixed a22) {
		I64 first = static_cast<I64>(a11) * static_cast<I64>(a22);
		I64 second = static_cast<I64>(a12) * static_cast<I64>(a21);

		return static_cast<EGL_Fixed>((first - second) >> (EGL_PRECISION + DET_SHIFT));
	}

}


#define SOLVE_PARAM_XY(val, p1, p2, p3, scale) \
	grad.dx.val = EGL_Mul(																\
			Det2x2Gradient(																		\
				p2 - p1, (pos2.m_WindowCoords.y - pos1.m_WindowCoords.y) >> DET_SHIFT,					\
				p3 - p1, (pos3.m_WindowCoords.y - pos1.m_WindowCoords.y) >> DET_SHIFT),				\
			scale);	\
	grad.dy.val = EGL_Mul(																\
			Det2x2Gradient(																		\
				(pos2.m_WindowCoords.x - pos1.m_WindowCoords.x) >> DET_SHIFT, p2 - p1,					\
				(pos3.m_WindowCoords.x - pos1.m_WindowCoords.x) >> DET_SHIFT, p3 - p1),				\
			scale)


#define SOLVE_XY(param, scale) \
	SOLVE_PARAM_XY(param, pos1.param, pos2.param, pos3.param, scale)


namespace {
	bool hasAlpha(RasterizerState::TextureFormat format) {
		switch (format) {
		case RasterizerState::TextureFormatAlpha:
		case RasterizerState::TextureFormatLuminanceAlpha:
		case RasterizerState::TextureFormatRGBA8:
		case RasterizerState::TextureFormatRGBA4444:
		case RasterizerState::TextureFormatRGBA5551:
			return true;


		default:
			return false;
		}
	}
}


// ---------------------------------------------------------------------------
// Prepare rasterizer object for triangles
// ---------------------------------------------------------------------------

void Rasterizer :: PrepareTriangle() {
	PrepareTexture();

#if EGL_USE_JIT
	m_ScanlineFunction = (ScanlineFunction *)
		m_FunctionCache->GetFunction(FunctionCache::FunctionTypeScanline, 
									 *m_State);
#endif

	// could be optimized
	bool needsColor = !m_State->m_Texture[0].Enabled || 
					  m_State->m_Texture[0].Mode != RasterizerState::TextureModeReplace;

	bool needsTexture = false;
	
	for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		needsTexture |= m_State->m_Texture[unit].Enabled;
	}

	bool needsFog = m_State->m_Fog.Enabled;
	bool needsDepth = m_State->m_DepthTest.Enabled || m_State->m_Mask.Depth || m_State->m_Stencil.Enabled;
	bool needsScissor = m_State->m_ScissorTest.Enabled;
	bool needsStencil = m_State->m_Stencil.Enabled;

	U32 selector =
		((needsColor	? 1 : 0) << RasterTriangleColor		) |
		((needsTexture	? 1 : 0) << RasterTriangleTexture	) |
		((needsFog		? 1 : 0) << RasterTriangleFog		) |
		((needsDepth	? 1 : 0) << RasterTriangleDepth		) |
		((needsScissor	? 1 : 0) << RasterTriangleScissor	) |
		((needsStencil	? 1 : 0) << RasterTriangleStencil	);

	m_RasterTriangleFunction = m_RasterTriangleFunctions[selector];

	if (m_RasterTriangleFunction == 0)
        m_RasterTriangleFunction = &Rasterizer::RasterTriangleAll;

	memset(m_RasterInfo.MipmapLevel, 0, sizeof(m_RasterInfo.MipmapLevel));
}


// --------------------------------------------------------------------------
// number of pixels done with linear interpolation
// --------------------------------------------------------------------------


#define LOG_LINEAR_SPAN 3					// logarithm of value base 2
#define LINEAR_SPAN (1 << LOG_LINEAR_SPAN)	// must be power of 2

#if !EGL_USE_JIT

inline void Rasterizer :: RasterScanLine(RasterInfo & rasterInfo, const EdgePos & start, const EdgePos & delta) {

	// In the edge buffer, z, tu and tv are actually divided by w

	FractionalColor baseColor = start.m_Color;

	if (!(delta.m_WindowCoords.x - start.m_WindowCoords.x)) {
		return;
	}

	const FractionalColor& colorIncrement = delta.m_Color;

	EGL_Fixed deltaFog = delta.m_FogDensity;
	EGL_Fixed deltaDepth = delta.m_WindowCoords.depth;

	EGL_Fixed deltaInvZ = delta.m_WindowCoords.invZ;

	size_t unit;
	EGL_Fixed deltaInvU[EGL_NUM_TEXTURE_UNITS],
		deltaInvV[EGL_NUM_TEXTURE_UNITS],
		invTu[EGL_NUM_TEXTURE_UNITS],
		invTv[EGL_NUM_TEXTURE_UNITS];

#if EGL_MIPMAP_PER_TEXEL
	EGL_Fixed deltaInvDu[EGL_NUM_TEXTURE_UNITS],
		deltaInvDv[EGL_NUM_TEXTURE_UNITS],
		dTuDxOverInvZ2[EGL_NUM_TEXTURE_UNITS],
		dTuDyOverInvZ2[EGL_NUM_TEXTURE_UNITS],
		dTvDxOverInvZ2[EGL_NUM_TEXTURE_UNITS],
		dTvDyOverInvZ2[EGL_NUM_TEXTURE_UNITS];
#endif

	for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		deltaInvU[unit] = delta.m_TextureCoords[unit].tu;
		deltaInvV[unit] = delta.m_TextureCoords[unit].tv;

		invTu[unit] = start.m_TextureCoords[unit].tu;
		invTv[unit] = start.m_TextureCoords[unit].tv;

#if EGL_MIPMAP_PER_TEXEL
		deltaInvDu[unit] = delta.m_TextureCoords[unit].dtudy;
		deltaInvDv[unit] = delta.m_TextureCoords[unit].dtvdy;

		dTuDxOverInvZ2[unit] = start.m_TextureCoords[unit].dtudx;
		dTuDyOverInvZ2[unit] = start.m_TextureCoords[unit].dtudy;
		dTvDxOverInvZ2[unit] = start.m_TextureCoords[unit].dtvdx;
		dTvDyOverInvZ2[unit] = start.m_TextureCoords[unit].dtvdy;
#endif
	}

	EGL_Fixed invZ = start.m_WindowCoords.invZ;

	EGL_Fixed fogDensity = start.m_FogDensity;
	I32 x = EGL_IntFromFixed(start.m_WindowCoords.x);
	I32 xEnd = EGL_IntFromFixed(delta.m_WindowCoords.x);
	I32 xLinEnd = x + ((xEnd - x) & ~(LINEAR_SPAN - 1));

	EGL_Fixed z = EGL_Inverse(invZ);
	EGL_Fixed tu[EGL_NUM_TEXTURE_UNITS], tv[EGL_NUM_TEXTURE_UNITS];

	for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		tu[unit] = EGL_Mul(invTu[unit], z);
		tv[unit] = EGL_Mul(invTv[unit], z);
	}

	EGL_Fixed depth = start.m_WindowCoords.depth;

	for (; x < xLinEnd;) {

		// to get started, do mipmap selection at beginning of span

#if EGL_MIPMAP_PER_TEXEL

		for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
			if (m_UseMipmap[unit]) {
				EGL_Fixed z2 = EGL_Mul(z << 4, z << 4);
				EGL_Fixed maxDu = EGL_Max(EGL_Abs(dTuDxOverInvZ2[unit]), EGL_Abs(dTuDyOverInvZ2[unit])) >> (16 - m_Texture[unit]->GetTexture(0)->GetLogWidth());
				EGL_Fixed maxDv = EGL_Max(EGL_Abs(dTvDxOverInvZ2[unit]), EGL_Abs(dTvDyOverInvZ2[unit])) >> (16 - m_Texture[unit]->GetTexture(0)->GetLogHeight());

				//EGL_Fixed maxD = EGL_Max(maxDu, maxDv);
				EGL_Fixed maxD = maxDu + maxDv;
				//I64 rho64 = ((I64) EGL_Mul(z2, EGL_FixedFromFloat(1/sqrt(2.0f)) + 1)) * ((I64) maxD);
				EGL_Fixed rho = EGL_Mul(z2, maxD);

				// we start with nearest/minification only selection; will add LINEAR later

				rasterInfo.MipmapLevel[unit] = EGL_Min(Log2(rho), rasterInfo.MaxMipmapLevel[unit]);

				dTuDyOverInvZ2[unit] += deltaInvDu[unit] << LOG_LINEAR_SPAN;
				dTvDyOverInvZ2[unit] += deltaInvDv[unit] << LOG_LINEAR_SPAN;
			}
		}
#endif

		invZ += deltaInvZ << LOG_LINEAR_SPAN;
		EGL_Fixed endZ = EGL_Inverse(invZ);
		EGL_Fixed deltaZ = (endZ - z) >> LOG_LINEAR_SPAN;

		EGL_Fixed endTu[EGL_NUM_TEXTURE_UNITS];
		EGL_Fixed endTv[EGL_NUM_TEXTURE_UNITS];
		EGL_Fixed deltaTu[EGL_NUM_TEXTURE_UNITS];
		EGL_Fixed deltaTv[EGL_NUM_TEXTURE_UNITS];

		for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
			invTu[unit] += deltaInvU[unit] << LOG_LINEAR_SPAN;
			invTv[unit] += deltaInvV[unit] << LOG_LINEAR_SPAN;

			endTu[unit] = EGL_Mul(invTu[unit], endZ);
			endTv[unit] = EGL_Mul(invTv[unit], endZ);
			deltaTu[unit] = (endTu[unit] - tu[unit]) >> LOG_LINEAR_SPAN; 
			deltaTv[unit] = (endTv[unit] - tv[unit]) >> LOG_LINEAR_SPAN;
		}

		int count = LINEAR_SPAN; 

		do {
			Fragment(&rasterInfo, x, depth, tu, tv, baseColor, fogDensity);

			baseColor += colorIncrement;
			depth += deltaDepth;
			fogDensity += deltaFog;
			z += deltaZ;

			for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
				tu[unit] += deltaTu[unit];
				tv[unit] += deltaTv[unit];
			}

			++x;
		} while (--count);
	}

	if (x != xEnd) {

		I32 deltaX = xEnd - x;

#if EGL_MIPMAP_PER_TEXEL
		for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
			if (m_UseMipmap[unit]) {
				EGL_Fixed z2 = EGL_Mul(z << 4, z << 4);
				EGL_Fixed maxDu = EGL_Max(EGL_Abs(dTuDxOverInvZ2[unit]), EGL_Abs(dTuDyOverInvZ2[unit])) >> (16 - m_Texture[unit]->GetTexture(0)->GetLogWidth());
				EGL_Fixed maxDv = EGL_Max(EGL_Abs(dTvDxOverInvZ2[unit]), EGL_Abs(dTvDyOverInvZ2[unit])) >> (16 - m_Texture[unit]->GetTexture(0)->GetLogHeight());

				//EGL_Fixed maxD = EGL_Max(maxDu, maxDv);
				EGL_Fixed maxD = maxDu + maxDv;
				//I64 rho64 = ((I64) EGL_Mul(z2, EGL_FixedFromFloat(1/sqrt(2.0f)) + 1)) * ((I64) maxD);
				EGL_Fixed rho = EGL_Mul(z2, maxD);

				// we start with nearest/minification only selection; will add LINEAR later

				rasterInfo.MipmapLevel[unit] = EGL_Min(Log2(rho), rasterInfo.MaxMipmapLevel[unit]);

				dTuDyOverInvZ2[unit] += deltaInvDu[unit] << LOG_LINEAR_SPAN;
				dTvDyOverInvZ2[unit] += deltaInvDv[unit] << LOG_LINEAR_SPAN;
			}
		}
#endif

		EGL_Fixed endZ = EGL_Inverse(invZ + deltaX * deltaInvZ);
		EGL_Fixed invSpan = EGL_Inverse(EGL_FixedFromInt(xEnd - x));
		EGL_Fixed deltaZ = EGL_Mul(endZ - z, invSpan);

		EGL_Fixed endTu[EGL_NUM_TEXTURE_UNITS];
		EGL_Fixed endTv[EGL_NUM_TEXTURE_UNITS];
		EGL_Fixed deltaTu[EGL_NUM_TEXTURE_UNITS];
		EGL_Fixed deltaTv[EGL_NUM_TEXTURE_UNITS];

		for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
			endTu[unit] = EGL_Mul(invTu[unit] + deltaX * deltaInvU[unit], endZ);
			endTv[unit] = EGL_Mul(invTv[unit] + deltaX * deltaInvV[unit], endZ);
			deltaTu[unit] = EGL_Mul(endTu[unit] - tu[unit], invSpan);
			deltaTv[unit] = EGL_Mul(endTv[unit] - tv[unit], invSpan);
		}

		for (; x < xEnd; ++x) {

			Fragment(&rasterInfo, x, depth, tu, tv, baseColor, fogDensity);

			baseColor += colorIncrement;
			depth += deltaDepth;

			for (unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
				tu[unit] += deltaTu[unit];
				tv[unit] += deltaTv[unit];
			}

			fogDensity += deltaFog;
		}
	}
}
#endif // !EGL_USE_JIT


// --------------------------------------------------------------------------
// Specialized triangle rasterizers go here
// --------------------------------------------------------------------------


//
// RasterTriangleAll
//

#define InitScanlineStart		InitScanlineStartAll
#define InitScanlineDeltas		InitScanlineDeltasAll
#define RasterTriangle			RasterTriangleAll

#define HasFog		1
#define HasDepth	1
#define HasColor	1
#define HasTexture	1
#define HasStencil	1
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTdfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTdfs
#define InitScanlineDeltas		InitScanlineDeltas_cTdfs
#define RasterTriangle			RasterTriangle_cTdfs

#define HasFog		0
#define HasDepth	0
#define HasColor	0
#define HasTexture	1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTdFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTdFs
#define InitScanlineDeltas		InitScanlineDeltas_cTdFs
#define RasterTriangle			RasterTriangle_cTdFs

#define HasFog		1
#define HasDepth	0
#define HasColor	0
#define HasTexture	1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTDfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTDfs
#define InitScanlineDeltas		InitScanlineDeltas_cTDfs
#define RasterTriangle			RasterTriangle_cTDfs

#define HasColor	0
#define HasTexture	1
#define HasDepth	1
#define HasFog		0
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTDFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTDFs
#define InitScanlineDeltas		InitScanlineDeltas_cTDFs
#define RasterTriangle			RasterTriangle_cTDFs

#define HasColor	0
#define HasTexture	1
#define HasDepth	1
#define HasFog		1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_Ctdfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_Ctdfs
#define InitScanlineDeltas		InitScanlineDeltas_Ctdfs
#define RasterTriangle			RasterTriangle_Ctdfs

#define HasColor	1
#define HasTexture	0
#define HasDepth	0
#define HasFog		0
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtdFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtdFs
#define InitScanlineDeltas		InitScanlineDeltas_CtdFs
#define RasterTriangle			RasterTriangle_CtdFs

#define HasColor	1
#define HasTexture	0
#define HasDepth	0
#define HasFog		1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtDfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtDfs
#define InitScanlineDeltas		InitScanlineDeltas_CtDfs
#define RasterTriangle			RasterTriangle_CtDfs

#define HasColor	1
#define HasTexture	0
#define HasDepth	1
#define HasFog		0
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtDFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtDFs
#define InitScanlineDeltas		InitScanlineDeltas_CtDFs
#define RasterTriangle			RasterTriangle_CtDFs

#define HasColor	1
#define HasTexture	0
#define HasDepth	1
#define HasFog		1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTdfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTdfs
#define InitScanlineDeltas		InitScanlineDeltas_CTdfs
#define RasterTriangle			RasterTriangle_CTdfs

#define HasColor	1
#define HasTexture	1
#define HasDepth	0
#define HasFog		0
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTdFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTdFs
#define InitScanlineDeltas		InitScanlineDeltas_CTdFs
#define RasterTriangle			RasterTriangle_CTdFs

#define HasColor	1
#define HasTexture	1
#define HasDepth	0
#define HasFog		1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTDfs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTDfs
#define InitScanlineDeltas		InitScanlineDeltas_CTDfs
#define RasterTriangle			RasterTriangle_CTDfs

#define HasColor	1
#define HasTexture	1
#define HasDepth	1
#define HasFog		0
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTDFs(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTDFs
#define InitScanlineDeltas		InitScanlineDeltas_CTDFs
#define RasterTriangle			RasterTriangle_CTDFs

#define HasColor	1
#define HasTexture	1
#define HasDepth	1
#define HasFog		1
#define HasStencil	0
#define HasScissor	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTdfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTdfS
#define InitScanlineDeltas		InitScanlineDeltas_cTdfS
#define RasterTriangle			RasterTriangle_cTdfS

#define HasFog		0
#define HasDepth	0
#define HasColor	0
#define HasTexture	1
#define HasScissor	1
#define HasStencil	0

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTdFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTdFS
#define InitScanlineDeltas		InitScanlineDeltas_cTdFS
#define RasterTriangle			RasterTriangle_cTdFS

#define HasFog		1
#define HasDepth	0
#define HasColor	0
#define HasTexture	1
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTDfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTDfS
#define InitScanlineDeltas		InitScanlineDeltas_cTDfS
#define RasterTriangle			RasterTriangle_cTDfS

#define HasColor	0
#define HasTexture	1
#define HasDepth	1
#define HasFog		0
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_cTDFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_cTDFS
#define InitScanlineDeltas		InitScanlineDeltas_cTDFS
#define RasterTriangle			RasterTriangle_cTDFS

#define HasColor	0
#define HasTexture	1
#define HasDepth	1
#define HasFog		1
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtdfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtdfS
#define InitScanlineDeltas		InitScanlineDeltas_CtdfS
#define RasterTriangle			RasterTriangle_CtdfS

#define HasColor	1
#define HasTexture	0
#define HasDepth	0
#define HasFog		0
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtdFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtdFS
#define InitScanlineDeltas		InitScanlineDeltas_CtdFS
#define RasterTriangle			RasterTriangle_CtdFS

#define HasColor	1
#define HasTexture	0
#define HasDepth	0
#define HasFog		1
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtDfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtDfS
#define InitScanlineDeltas		InitScanlineDeltas_CtDfS
#define RasterTriangle			RasterTriangle_CtDfS

#define HasColor	1
#define HasTexture	0
#define HasDepth	1
#define HasFog		0
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CtDFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CtDFS
#define InitScanlineDeltas		InitScanlineDeltas_CtDFS
#define RasterTriangle			RasterTriangle_CtDFS

#define HasColor	1
#define HasTexture	0
#define HasDepth	1
#define HasFog		1
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTdfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTdfS
#define InitScanlineDeltas		InitScanlineDeltas_CTdfS
#define RasterTriangle			RasterTriangle_CTdfS

#define HasColor	1
#define HasTexture	1
#define HasDepth	0
#define HasFog		0
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTdFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTdFS
#define InitScanlineDeltas		InitScanlineDeltas_CTdFS
#define RasterTriangle			RasterTriangle_CTdFS

#define HasColor	1
#define HasTexture	1
#define HasDepth	0
#define HasFog		1
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTDfS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTDfS
#define InitScanlineDeltas		InitScanlineDeltas_CTDfS
#define RasterTriangle			RasterTriangle_CTDfS

#define HasColor	1
#define HasTexture	1
#define HasDepth	1
#define HasFog		0
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"


//
// void RasterTriangle_CTDFS(const RasterPos& a, const RasterPos& b, const RasterPos& c);
//

#define InitScanlineStart		InitScanlineStart_CTDFS
#define InitScanlineDeltas		InitScanlineDeltas_CTDFS
#define RasterTriangle			RasterTriangle_CTDFS

#define HasColor	1
#define HasTexture	1
#define HasDepth	1
#define HasFog		1
#define HasStencil	0
#define HasScissor	1

#include "RasterizerTriangles.inc"
