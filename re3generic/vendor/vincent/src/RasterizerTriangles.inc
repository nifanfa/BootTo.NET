// ==========================================================================
//
// RasterizerTriangles.inc	Rasterizer Class for 3D Rendering Library
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


// The following macros need to be defined for this include file:
//
// InitScanlineStart
// InitScanlineDeltas
// RasterTriangle
//
// HasFog
// HasDepth
// HasColor
// HasTexture
// HasStencil
// HasScissor


#ifndef InitScanlineStart
#error "InitScanlineStart is not defined"
#endif

#ifndef InitScanlineDeltas
#error "InitScanlineDeltas is not defined"
#endif

#ifndef RasterTriangle
#error "RasterTriangle is not defined"
#endif

#ifndef HasFog
#error "HasFog is not defined"
#endif

#ifndef HasDepth
#error "HasDepth is not defined"
#endif

#ifndef HasColor
#error "HasColor is not defined"
#endif

#ifndef HasTexture
#error "HasTexture is not defined"
#endif

#ifndef HasStencil
#error "HasStencil is not defined"
#endif

#ifndef HasScissor
#error "HasScissor is not defined"
#endif


namespace {
	inline void InitScanlineStart(const RasterInfo & rasterInfo, EdgePos & start, const Gradients & grad,
		EGL_Fixed xPreStep, EGL_Fixed yPreStep,
		const FractionalColor& color, EGL_Fixed fog, EGL_Fixed depth, EGL_Fixed invZ, EGL_Fixed tuOverZ[], EGL_Fixed tvOverZ[]) {
#if HasDepth
		start.m_WindowCoords.depth = 
			depth		+ EGL_Mul(grad.dx.m_WindowCoords.depth, xPreStep)	
						+ EGL_Mul(grad.dy.m_WindowCoords.depth, yPreStep); 
#endif

#if HasColor
		start.m_Color.r	= 
			color.r		+ EGL_Mul(grad.dx.m_Color.r, xPreStep)				
						+ EGL_Mul(grad.dy.m_Color.r, yPreStep);

		start.m_Color.g	= 
			color.g		+ EGL_Mul(grad.dx.m_Color.g, xPreStep)				
						+ EGL_Mul(grad.dy.m_Color.g, yPreStep);

		start.m_Color.b	= 
			color.b		+ EGL_Mul(grad.dx.m_Color.b, xPreStep)				
						+ EGL_Mul(grad.dy.m_Color.b, yPreStep);

		start.m_Color.a	= 
			color.a		+ EGL_Mul(grad.dx.m_Color.a, xPreStep)				
						+ EGL_Mul(grad.dy.m_Color.a, yPreStep);
#endif

#if HasTexture		
		start.m_WindowCoords.invZ = 
			invZ		+ EGL_Mul(grad.dx.m_WindowCoords.invZ, xPreStep)	
						+ EGL_Mul(grad.dy.m_WindowCoords.invZ, yPreStep); 

		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {
			if (!rasterInfo.Textures[index])
				continue;

			start.m_TextureCoords[index].tu = 
				tuOverZ[index]		
					+ EGL_Mul(grad.dx.m_TextureCoords[index].tu, xPreStep)		
					+ EGL_Mul(grad.dy.m_TextureCoords[index].tu, yPreStep);

			start.m_TextureCoords[index].tv = 
				tvOverZ[index]		
					+ EGL_Mul(grad.dx.m_TextureCoords[index].tv, xPreStep)		
					+ EGL_Mul(grad.dy.m_TextureCoords[index].tv, yPreStep);

#if EGL_MIPMAP_PER_TEXEL

			// ------------------------------------------------------------------
			// Determine partial derivatives of texture functions, 
			// see eq. (2) and (3) in
			// J. P. Ewins et al (1998), 
			// "MIP-Map Level Selection for Texture Mapping",
			// IEEE Transactions on Visualization and Computer Graphics, 
			// Vol 4, No. 4
			// ------------------------------------------------------------------

			start.m_TextureCoords[index].dtudx = 
				Det2x2(grad.dx.m_TextureCoords[index].tu, tuOverZ[index], grad.dx.m_WindowCoords.invZ, invZ) 
					+ EGL_Mul(grad.dy.m_TextureCoords[index].dtudx, yPreStep);

			// Swap the following two assignments as reported by user
			start.m_TextureCoords[index].dtvdx = 
				Det2x2(grad.dx.m_TextureCoords[index].tv, tvOverZ[index], grad.dx.m_WindowCoords.invZ, invZ)
					+ EGL_Mul(grad.dx.m_TextureCoords[index].dtudy, xPreStep);

			start.m_TextureCoords[index].dtudy = 
				Det2x2(grad.dy.m_TextureCoords[index].tu, tuOverZ[index], grad.dy.m_WindowCoords.invZ, invZ)
					+ EGL_Mul(grad.dy.m_TextureCoords[index].dtvdx, yPreStep);

			start.m_TextureCoords[index].dtvdy = 
				Det2x2(grad.dy.m_TextureCoords[index].tv, tvOverZ[index], grad.dy.m_WindowCoords.invZ, invZ)
					+ EGL_Mul(grad.dx.m_TextureCoords[index].dtvdy, xPreStep);
#endif
		}
#endif

#if HasFog
		start.m_FogDensity = 
			fog			+ EGL_Mul(grad.dx.m_FogDensity, xPreStep)			
						+ EGL_Mul(grad.dy.m_FogDensity, yPreStep);
#endif
	}


	inline void InitScanlineDeltas(const RasterInfo & rasterInfo, EdgePos & deltaSmall, EdgePos & deltaBig,
								   const Gradients & gradients, 
								   EGL_Fixed dxdy, I32 dXdYStepInt) {

		deltaSmall.m_WindowCoords.x		= dxdy;	// x offset is stepped for each line (could consider removing fractional part from scanline function)

#if HasDepth
		deltaSmall.m_WindowCoords.depth	= gradients.dx.m_WindowCoords.depth * dXdYStepInt	+ gradients.dy.m_WindowCoords.depth; 
#endif

#if HasColor
		deltaSmall.m_Color.r			= gradients.dx.m_Color.r * dXdYStepInt		+ gradients.dy.m_Color.r;
		deltaSmall.m_Color.g			= gradients.dx.m_Color.g * dXdYStepInt		+ gradients.dy.m_Color.g;
		deltaSmall.m_Color.b			= gradients.dx.m_Color.b * dXdYStepInt		+ gradients.dy.m_Color.b;
		deltaSmall.m_Color.a			= gradients.dx.m_Color.a * dXdYStepInt		+ gradients.dy.m_Color.a;
#endif

#if HasTexture		
		deltaSmall.m_WindowCoords.invZ	= gradients.dx.m_WindowCoords.invZ * dXdYStepInt	+ gradients.dy.m_WindowCoords.invZ; 

		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {
			if (!rasterInfo.Textures[index])
				continue;

			deltaSmall.m_TextureCoords[index].tu	= 
				gradients.dx.m_TextureCoords[index].tu * dXdYStepInt		
					+ gradients.dy.m_TextureCoords[index].tu;
			deltaSmall.m_TextureCoords[index].tv	= 
				gradients.dx.m_TextureCoords[index].tv * dXdYStepInt		
					+ gradients.dy.m_TextureCoords[index].tv;

#if EGL_MIPMAP_PER_TEXEL

			deltaSmall.m_TextureCoords[index].dtudx =													  
				gradients.dy.m_TextureCoords[index].dtudx;
			deltaSmall.m_TextureCoords[index].dtudy = 
				gradients.dx.m_TextureCoords[index].dtudy * dXdYStepInt;
			deltaSmall.m_TextureCoords[index].dtvdx =													  
				gradients.dy.m_TextureCoords[index].dtvdx;
			deltaSmall.m_TextureCoords[index].dtvdy = 
				gradients.dx.m_TextureCoords[index].dtvdy * dXdYStepInt;
#endif
		}
#endif

#if HasFog
		deltaSmall.m_FogDensity			= gradients.dx.m_FogDensity * dXdYStepInt	+ gradients.dy.m_FogDensity;
#endif

		deltaBig.m_WindowCoords.x		= dxdy;

		if (dxdy >= 0) {
#if HasDepth
			deltaBig.m_WindowCoords.depth	= deltaSmall.m_WindowCoords.depth	+ gradients.dx.m_WindowCoords.depth; 
#endif

#if HasColor
			deltaBig.m_Color.r				= deltaSmall.m_Color.r				+ gradients.dx.m_Color.r;
			deltaBig.m_Color.g				= deltaSmall.m_Color.g				+ gradients.dx.m_Color.g;
			deltaBig.m_Color.b				= deltaSmall.m_Color.b				+ gradients.dx.m_Color.b;
			deltaBig.m_Color.a				= deltaSmall.m_Color.a				+ gradients.dx.m_Color.a;
#endif

#if HasTexture			
			deltaBig.m_WindowCoords.invZ	= deltaSmall.m_WindowCoords.invZ	+ gradients.dx.m_WindowCoords.invZ; 

			for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {
				if (!rasterInfo.Textures[index])
					continue;

				deltaBig.m_TextureCoords[index].tu		= deltaSmall.m_TextureCoords[index].tu		+ gradients.dx.m_TextureCoords[index].tu;
				deltaBig.m_TextureCoords[index].tv		= deltaSmall.m_TextureCoords[index].tv		+ gradients.dx.m_TextureCoords[index].tv;

#if EGL_MIPMAP_PER_TEXEL
				deltaBig.m_TextureCoords[index].dtudx	= deltaSmall.m_TextureCoords[index].dtudx;
				deltaBig.m_TextureCoords[index].dtudy	= deltaSmall.m_TextureCoords[index].dtudy	+ gradients.dx.m_TextureCoords[index].dtudy;
				deltaBig.m_TextureCoords[index].dtvdx	= deltaSmall.m_TextureCoords[index].dtvdx;
				deltaBig.m_TextureCoords[index].dtvdy	= deltaSmall.m_TextureCoords[index].dtvdy	+ gradients.dx.m_TextureCoords[index].dtvdy;
#endif
			}
#endif

#if HasFog
			deltaBig.m_FogDensity			= deltaSmall.m_FogDensity			+ gradients.dx.m_FogDensity;
#endif
		} else {
#if HasDepth
			deltaBig.m_WindowCoords.depth	= deltaSmall.m_WindowCoords.depth	- gradients.dx.m_WindowCoords.depth; 
#endif

#if HasColor
			deltaBig.m_Color.r				= deltaSmall.m_Color.r				- gradients.dx.m_Color.r;
			deltaBig.m_Color.g				= deltaSmall.m_Color.g				- gradients.dx.m_Color.g;
			deltaBig.m_Color.b				= deltaSmall.m_Color.b				- gradients.dx.m_Color.b;
			deltaBig.m_Color.a				= deltaSmall.m_Color.a				- gradients.dx.m_Color.a;
#endif

#if HasTexture			
			deltaBig.m_WindowCoords.invZ	= deltaSmall.m_WindowCoords.invZ	- gradients.dx.m_WindowCoords.invZ; 

			for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {
				if (!rasterInfo.Textures[index])
					continue;

				deltaBig.m_TextureCoords[index].tu		= deltaSmall.m_TextureCoords[index].tu		- gradients.dx.m_TextureCoords[index].tu;
				deltaBig.m_TextureCoords[index].tv		= deltaSmall.m_TextureCoords[index].tv		- gradients.dx.m_TextureCoords[index].tv;

#if EGL_MIPMAP_PER_TEXEL
				deltaBig.m_TextureCoords[index].dtudx	= deltaSmall.m_TextureCoords[index].dtudx;
				deltaBig.m_TextureCoords[index].dtudy	= deltaSmall.m_TextureCoords[index].dtudy	- gradients.dx.m_TextureCoords[index].dtudy;
				deltaBig.m_TextureCoords[index].dtvdx	= deltaSmall.m_TextureCoords[index].dtvdx;
				deltaBig.m_TextureCoords[index].dtvdy	= deltaSmall.m_TextureCoords[index].dtvdy	+ gradients.dx.m_TextureCoords[index].dtvdy;
#endif
			}
#endif

#if HasFog
			deltaBig.m_FogDensity			= deltaSmall.m_FogDensity			- gradients.dx.m_FogDensity;
#endif
		}
	}

// --------------------------------------------------------------------------
// Scanline to scanline increment
// --------------------------------------------------------------------------

#if HasFog
#define ScanlineDeltaFogBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_FogDensity			+= deltaBig.m_FogDensity;				

#define ScanlineDeltaFogSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_FogDensity			+= deltaSmall.m_FogDensity;				\

#else
#define ScanlineDeltaFogBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#define ScanlineDeltaFogSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#endif


#if HasDepth
#define ScanlineDeltaDepthBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_WindowCoords.depth	+= deltaBig.m_WindowCoords.depth;		

#define ScanlineDeltaDepthSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_WindowCoords.depth	+= deltaSmall.m_WindowCoords.depth;		

#define ScanlineDeltaDepth(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
		rasterInfo.DepthBuffer			+= rasterInfo.SurfaceWidth;				\

#else
#define ScanlineDeltaDepthBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#define ScanlineDeltaDepthSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#define ScanlineDeltaDepth(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#endif


#if HasTexture
#if EGL_MIPMAP_PER_TEXEL
#define ScanlineDeltaTextureBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
		start.m_WindowCoords.invZ	+= deltaBig.m_WindowCoords.invZ;			\
		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {		\
			if (!m_RasterInfo.Textures[index]) continue;														\
			start.m_TextureCoords[index].tu	+= deltaBig.m_TextureCoords[index].tu;			\
			start.m_TextureCoords[index].tv	+= deltaBig.m_TextureCoords[index].tv;			\
			start.m_TextureCoords[index].dtudx	+= deltaBig.m_TextureCoords[index].dtudx;		\
			start.m_TextureCoords[index].dtudy	+= deltaBig.m_TextureCoords[index].dtudy;		\
			start.m_TextureCoords[index].dtvdx	+= deltaBig.m_TextureCoords[index].dtvdx;		\
			start.m_TextureCoords[index].dtvdy	+= deltaBig.m_TextureCoords[index].dtvdy;		\
		}

#define ScanlineDeltaTextureSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_WindowCoords.invZ	+= deltaSmall.m_WindowCoords.invZ;		\
		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {		\
			if (!m_RasterInfo.Textures[index]) continue;						\
			start.m_TextureCoords[index].tu	+= deltaSmall.m_TextureCoords[index].tu;		\
			start.m_TextureCoords[index].tv	+= deltaSmall.m_TextureCoords[index].tv;		\
			start.m_TextureCoords[index].dtudx	+= deltaSmall.m_TextureCoords[index].dtudx;	\
			start.m_TextureCoords[index].dtudy	+= deltaSmall.m_TextureCoords[index].dtudy;	\
			start.m_TextureCoords[index].dtvdx	+= deltaSmall.m_TextureCoords[index].dtvdx;	\
			start.m_TextureCoords[index].dtvdy	+= deltaSmall.m_TextureCoords[index].dtvdy;	\
		}
#else
#define ScanlineDeltaTextureBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
		start.m_WindowCoords.invZ	+= deltaBig.m_WindowCoords.invZ;			\
		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {		\
			if (!m_RasterInfo.Textures[index]) continue;														\
			start.m_TextureCoords[index].tu	+= deltaBig.m_TextureCoords[index].tu;			\
			start.m_TextureCoords[index].tv	+= deltaBig.m_TextureCoords[index].tv;			\
		}

#define ScanlineDeltaTextureSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_WindowCoords.invZ	+= deltaSmall.m_WindowCoords.invZ;		\
		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {		\
			if (!m_RasterInfo.Textures[index]) continue;						\
			start.m_TextureCoords[index].tu	+= deltaSmall.m_TextureCoords[index].tu;		\
			start.m_TextureCoords[index].tv	+= deltaSmall.m_TextureCoords[index].tv;		\
		}

#endif // EGL_MIPMAP_PER_TEXEL

#else
#define ScanlineDeltaTextureBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#define ScanlineDeltaTextureSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#endif


#if HasColor
#define ScanlineDeltaColorBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_Color				+= deltaBig.m_Color;					

#define ScanlineDeltaColorSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
			start.m_Color				+= deltaSmall.m_Color;					

#else
#define ScanlineDeltaColorBig(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#define ScanlineDeltaColorSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#endif


#if HasStencil
#define ScanlineDeltaStencil(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
		rasterInfo.StencilBuffer		+= rasterInfo.SurfaceWidth;				

#else
#define ScanlineDeltaStencil(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)
#endif

#define ScanlineDelta(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy) \
		xError += xStepError;													\
		if (xError >= EGL_ONE) {												\
			xError -= EGL_ONE;													\
			start.m_WindowCoords.x		+= deltaBig.m_WindowCoords.x;			\
			ScanlineDeltaColorBig	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
			ScanlineDeltaTextureBig	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
			ScanlineDeltaFogBig		(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
			ScanlineDeltaDepthBig	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
		} else {																\
			start.m_WindowCoords.x		+= deltaSmall.m_WindowCoords.x;			\
			ScanlineDeltaColorSmall	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
			ScanlineDeltaTextureSmall(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
			ScanlineDeltaFogSmall	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
			ScanlineDeltaDepthSmall	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
		}																		\
		delta.m_WindowCoords.x			+= dxdy;								\
		ScanlineDeltaDepth	(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
		ScanlineDeltaStencil(rasterInfo, start, delta, deltaSmall, deltaBig, xError, xStepError, dxdy)	\
		rasterInfo.ColorBuffer			+= rasterInfo.SurfaceWidth;				\
		rasterInfo.AlphaBuffer			+= rasterInfo.SurfaceWidth;
}


// ---------------------------------------------------------------------------
// Ultimately, we have the following variantions on rastering a triangle:
//
//	- With or without color
//  - With or without texture
//  - With or without depth
//  - With or without fog
//  - With or without alpha buffer
//  - With or without stencil buffer
//  - With or without scissor test
//
// ---------------------------------------------------------------------------


// ---------------------------------------------------------------------------
// Render the triangle specified by the three transformed and lit vertices
// passed as arguments. 
//
// Parameters:
//		a, b, c		The three vertices of the triangle
//
// Returns:
//		N/A
// --------------------------------------------------------------------------
void Rasterizer :: RasterTriangle(const RasterPos& a, const RasterPos& b,
								  const RasterPos& c) {
	
	// ----------------------------------------------------------------------
	// sort vertices by y
	// ----------------------------------------------------------------------

	const RasterPos * pos[3];
	pos[0] = &a;
	pos[1] = &b;
	pos[2] = &c;

	const I8 * permutation = SortPermutation(a.m_WindowCoords.y, b.m_WindowCoords.y, c.m_WindowCoords.y);
	const RasterPos &pos1 = *pos[permutation[0]];
	const RasterPos &pos2 = *pos[permutation[1]];
	const RasterPos &pos3 = *pos[permutation[2]];

	// ----------------------------------------------------------------------
	// Calculate the screen area of the triangle
	// ----------------------------------------------------------------------

	EGL_Fixed denominator = 
		Det2x2(
			pos2.m_WindowCoords.x - pos1.m_WindowCoords.x, pos2.m_WindowCoords.y - pos1.m_WindowCoords.y,
			pos3.m_WindowCoords.x - pos1.m_WindowCoords.x, pos3.m_WindowCoords.y - pos1.m_WindowCoords.y);

	// ----------------------------------------------------------------------
	// calculate all gradients for interpolation 
	// ----------------------------------------------------------------------

	EGL_Fixed invDenominator = 0;

	Gradients grad;

	EGL_Fixed tuOverZ1[EGL_NUM_TEXTURE_UNITS], 
			  tuOverZ2[EGL_NUM_TEXTURE_UNITS], 
			  tuOverZ3[EGL_NUM_TEXTURE_UNITS], 
			  tvOverZ1[EGL_NUM_TEXTURE_UNITS], 
			  tvOverZ2[EGL_NUM_TEXTURE_UNITS], 
			  tvOverZ3[EGL_NUM_TEXTURE_UNITS];

	if (!denominator) {
		memset(&grad, 0, sizeof grad);
	} else {
		invDenominator = EGL_Inverse(denominator);

#if HasColor
		SOLVE_XY(m_Color.r, invDenominator);
		SOLVE_XY(m_Color.g, invDenominator);
		SOLVE_XY(m_Color.b, invDenominator);
		SOLVE_XY(m_Color.a, invDenominator);
#endif

#if HasFog
		SOLVE_XY(m_FogDensity, invDenominator);
#endif

#if HasDepth
		SOLVE_XY(m_WindowCoords.depth, invDenominator);
#endif


#if HasTexture
		SOLVE_XY(m_WindowCoords.invZ, invDenominator);

		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {

			if (!m_RasterInfo.Textures[index])
				continue;

			tuOverZ1[index] = EGL_Mul(pos1.m_TextureCoords[index].tu, pos1.m_WindowCoords.invZ);
			tuOverZ2[index] = EGL_Mul(pos2.m_TextureCoords[index].tu, pos2.m_WindowCoords.invZ);
			tuOverZ3[index] = EGL_Mul(pos3.m_TextureCoords[index].tu, pos3.m_WindowCoords.invZ);

			tvOverZ1[index] = EGL_Mul(pos1.m_TextureCoords[index].tv, pos1.m_WindowCoords.invZ);
			tvOverZ2[index] = EGL_Mul(pos2.m_TextureCoords[index].tv, pos2.m_WindowCoords.invZ);
			tvOverZ3[index] = EGL_Mul(pos3.m_TextureCoords[index].tv, pos3.m_WindowCoords.invZ);

			SOLVE_PARAM_XY(m_TextureCoords[index].tu, tuOverZ1[index], tuOverZ2[index], tuOverZ3[index], invDenominator);
			SOLVE_PARAM_XY(m_TextureCoords[index].tv, tvOverZ1[index], tvOverZ2[index], tvOverZ3[index], invDenominator);

#if EGL_MIPMAP_PER_TEXEL

			// ------------------------------------------------------------------
			// Determine partial derivatives of texture functions, 
			// see eq. (2) and (3) in
			// J. P. Ewins et al (1998), 
			// "MIP-Map Level Selection for Texture Mapping",
			// IEEE Transactions on Visualization and Computer Graphics, 
			// Vol 4, No. 4
			// ------------------------------------------------------------------

			EGL_Fixed A = grad.dx.m_TextureCoords[index].tu;
			EGL_Fixed B = grad.dy.m_TextureCoords[index].tu;
			EGL_Fixed D = grad.dx.m_WindowCoords.invZ;
			EGL_Fixed E = grad.dy.m_WindowCoords.invZ;
			EGL_Fixed F = pos1.m_WindowCoords.invZ;
			EGL_Fixed G = grad.dx.m_TextureCoords[index].tv;
			EGL_Fixed H = grad.dy.m_TextureCoords[index].tv;

			EGL_Fixed K1 = Det2x2(A, B, D, E);
			EGL_Fixed K2 = Det2x2(G, H, D, E);

			grad.dy.m_TextureCoords[index].dtudx = K1;
			grad.dy.m_TextureCoords[index].dtudy = 0;
			grad.dy.m_TextureCoords[index].dtvdx = K2;
			grad.dy.m_TextureCoords[index].dtvdy = 0;

			grad.dx.m_TextureCoords[index].dtudx = 0;
			grad.dx.m_TextureCoords[index].dtudy = -K1;
			grad.dx.m_TextureCoords[index].dtvdx = 0;
			grad.dx.m_TextureCoords[index].dtvdy = -K2;
#endif
		}
#endif
	}

	// ----------------------------------------------------------------------
	// Constants to determine partial derivatives, see eq. (2) and (3) in
	// J. P. Ewins et al (1998), "MIP-Map Level Selection for Texture Mapping",
	// IEEE Transactions on Visualization and Computer Graphics, Vol 4, No. 4
	// ----------------------------------------------------------------------

	// Share the gradient in x direction for scanline function

	EdgePos& delta = grad.dx;

	// ----------------------------------------------------------------------
	// determine if the depth coordinate needs to be adjusted to
	// support polygon-offset
	// ----------------------------------------------------------------------

//#if HasDepth
	EGL_Fixed depth1 = pos1.m_WindowCoords.depth;
	EGL_Fixed depth2 = pos2.m_WindowCoords.depth;
	EGL_Fixed depth3 = pos3.m_WindowCoords.depth;
//#endif

#if HasDepth

	if (m_State->m_Polygon.OffsetFillEnabled) {

		EGL_Fixed factor = m_State->m_Polygon.OffsetFactor;
		EGL_Fixed units = m_State->m_Polygon.OffsetUnits;

		// calculation here

		EGL_Fixed gradX = EGL_Abs(grad.dx.m_WindowCoords.depth);
		EGL_Fixed gradY = EGL_Abs(grad.dy.m_WindowCoords.depth);

		EGL_Fixed depthSlope = gradX > gradY ? gradX : gradY;

		I32 offset = EGL_Mul(factor, depthSlope) + EGL_IntFromFixed(units * PolygonOffsetUnitSize);

		if (offset > 0) {
			depth1 = depth1 < DepthRangeMax - offset ? depth1 + offset : DepthRangeMax;
			depth2 = depth2 < DepthRangeMax - offset ? depth2 + offset : DepthRangeMax;
			depth3 = depth3 < DepthRangeMax - offset ? depth3 + offset : DepthRangeMax;
		} else {
			depth1 = depth1 > -offset ? depth1 + offset : 0;
			depth2 = depth2 > -offset ? depth2 + offset : 0;
			depth3 = depth3 > -offset ? depth3 + offset : 0;
		}
	}
#endif

	// ----------------------------------------------------------------------
	// determine the appropriate mipmapping level
	// ----------------------------------------------------------------------

//#if HasTexture
	EGL_Fixed invZ1 = pos1.m_WindowCoords.invZ;
	EGL_Fixed invZ2 = pos2.m_WindowCoords.invZ;
	EGL_Fixed invZ3 = pos3.m_WindowCoords.invZ;
//#endif

#if !EGL_MIPMAP_PER_TEXEL && HasTexture

	for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {

		if (!m_RasterInfo.Textures[index])
			continue;
	
		if (m_UseMipmap[index]) {

			int logWidth = Log2(m_Texture[index]->GetTexture(0)->GetWidth());
			int logHeight = Log2(m_Texture[index]->GetTexture(0)->GetHeight());
			int maxLevel = m_RasterInfo.MaxMipmapLevel[index];

			if (invDenominator) {

				EGL_Fixed textureArea = 
					Det2x2(
						(pos2.m_TextureCoords[index].tu - pos1.m_TextureCoords[index].tu) << logWidth, 
						(pos2.m_TextureCoords[index].tv - pos1.m_TextureCoords[index].tv) << logHeight,
						(pos3.m_TextureCoords[index].tu - pos1.m_TextureCoords[index].tu) << logWidth,
						(pos3.m_TextureCoords[index].tv - pos1.m_TextureCoords[index].tv) << logHeight);
				
				EGL_Fixed ratio = EGL_Mul(textureArea, invDenominator) >> EGL_PRECISION;
				
				int logRatio = Log2(ratio);

				if (logRatio <= 0) {
					m_RasterInfo.MipmapLevel[index] = 0;
				} else {
					m_RasterInfo.MipmapLevel[index] = logRatio / 2;

					if (m_RasterInfo.MipmapLevel[index] > maxLevel) {
						m_RasterInfo.MipmapLevel[index] = maxLevel;
					}
				}
			} else {
				m_RasterInfo.MipmapLevel[index] = maxLevel;
			}
		} else {
			m_RasterInfo.MipmapLevel[index] = 0;
		}
	}

#endif	

	// ----------------------------------------------------------------------
	//
	// - Raster top part of triangle
	//		determine integer (i.e. each line) and fractional increments per edgebuffer
	//		determine y-pre step and x-pre step for first pixel of first scanline
	//		determine error for x-stepping.
	//
	// - Raster bottom part of triangle
	// ----------------------------------------------------------------------

	EGL_Fixed yRounded1 = EGL_NearestInt(pos1.m_WindowCoords.y);
	EGL_Fixed yPreStep1 = yRounded1 + (EGL_ONE/2) - pos1.m_WindowCoords.y;
	I32 y = EGL_IntFromFixed(yRounded1);

	EGL_Fixed yRounded2 = EGL_NearestInt(pos2.m_WindowCoords.y);
	EGL_Fixed yPreStep2 = yRounded2 + (EGL_ONE/2) - pos2.m_WindowCoords.y;
	I32 y2 = EGL_IntFromFixed(yRounded2);

	EGL_Fixed yRounded3 = EGL_NearestInt(pos3.m_WindowCoords.y);
	I32 y3 = EGL_IntFromFixed(yRounded3);

	m_RasterInfo.Init(m_Surface, y);

	I32 yScissorStart = m_State->m_ScissorTest.Y;
	I32 yScissorEnd = yScissorStart + m_State->m_ScissorTest.Height;

	// ----------------------------------------------------------------------
	// Determine edge increments and scanline starting point
	// ----------------------------------------------------------------------

	EGL_Fixed deltaY2	= pos2.m_WindowCoords.y - pos1.m_WindowCoords.y;
	EGL_Fixed deltaY3	= pos3.m_WindowCoords.y - pos1.m_WindowCoords.y;
	EGL_Fixed deltaY23	= pos3.m_WindowCoords.y - pos2.m_WindowCoords.y;

	if (y >= y3) {
		// Always, the triangle is empty
		return;
	}

	if (y == y2) {
		// do special case here

		EGL_Fixed invDeltaY3 = EGL_Inverse(deltaY3);	// deltaY3 == 0 should not occur, triangle would already have been skipped
		EGL_Fixed invDeltaY23 = EGL_Inverse(deltaY23);

		EGL_Fixed deltaX3	= pos3.m_WindowCoords.x - pos1.m_WindowCoords.x;
		EGL_Fixed deltaX23	= pos3.m_WindowCoords.x - pos2.m_WindowCoords.x;

		EGL_Fixed dXdY3		= EGL_Mul(deltaX3, invDeltaY3);
		EGL_Fixed dXdY23	= EGL_Mul(deltaX23, invDeltaY23);

		if (dXdY23 < dXdY3) {
			// move beginning of scanline along p1->p3

			// ------------------------------------------------------------------
			// initialize start onto first pixel right off line p1->p3
			// ------------------------------------------------------------------

			EGL_Fixed xStepped1L = pos1.m_WindowCoords.x + EGL_Mul(yPreStep1, dXdY3);
			EGL_Fixed xRounded1 = EGL_NearestInt(xStepped1L);
			EGL_Fixed xPreStep1 = xRounded1 + (EGL_ONE/2) - pos1.m_WindowCoords.x;

			EdgePos start;
			start.m_WindowCoords.x = 
				xStepped1L + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

			InitScanlineStart(m_RasterInfo, start, grad, xPreStep1, yPreStep1,
							  pos1.m_Color, pos1.m_FogDensity, depth1, invZ1, tuOverZ1, tvOverZ1);

			// ------------------------------------------------------------------
			// initialize edge buffer delta2Int & delta2Frac
			// ------------------------------------------------------------------

			// determine integer x step/y
			I32 dXdYStep1Int = dXdY3 >= 0 ? EGL_IntFromFixed(dXdY3) : -EGL_IntFromFixed(-dXdY3);

			EdgePos delta3Small, delta3Big;
			InitScanlineDeltas(m_RasterInfo, delta3Small, delta3Big, grad, dXdY3, dXdYStep1Int);

			// ------------------------------------------------------------------
			// Stepping for right edge
			// ------------------------------------------------------------------

			EGL_Fixed xStepped1R = pos2.m_WindowCoords.x + EGL_Mul(pos1.m_WindowCoords.y - pos2.m_WindowCoords.y + yPreStep1, dXdY23);

			delta.m_WindowCoords.x = 
				xStepped1R + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

			// ------------------------------------------------------------------
			// initialize the x-step error
			// ------------------------------------------------------------------

			EGL_Fixed xStepError, xError;

			if (dXdY3 >= 0) {
				xStepError = dXdY3 - EGL_FixedFromInt(dXdYStep1Int);
				assert(xStepError >= 0 && xStepError < EGL_ONE);
				xError = EGL_FractionFromFixed(xStepped1L + EGL_ONE/2);
			} else {
				xStepError = -dXdY3 + EGL_FixedFromInt(dXdYStep1Int);
				assert(xStepError >= 0 && xStepError < EGL_ONE);
				xError = (EGL_ONE-1) - EGL_FractionFromFixed(xStepped1L + EGL_ONE/2);
			}

			// ------------------------------------------------------------------
			// Raster the top part of the triangle
			// ------------------------------------------------------------------

			for (; y < y3; ++y) {

#if HasScissor
				if (!m_State->m_ScissorTest.Enabled || (y >= yScissorStart && y < yScissorEnd))
#endif
					RasterScanLine(m_RasterInfo, start, delta);												

				ScanlineDelta(m_RasterInfo, start, delta, delta3Small, delta3Big,
							  xError, xStepError, dXdY23);
			}

		} else {
			// move beginning of scanline along p2->p3

			// ------------------------------------------------------------------
			// now do second part of triangle
			//
			// ------------------------------------------------------------------
			//
			// initialize start onto first pixel right off line p2->p3
			// ------------------------------------------------------------------

			EGL_Fixed xStepped2L = pos2.m_WindowCoords.x + EGL_Mul(yPreStep2, dXdY23);
			EGL_Fixed xRounded2 = EGL_NearestInt(xStepped2L);
			EGL_Fixed xPreStep2 = xRounded2 + (EGL_ONE/2) - pos2.m_WindowCoords.x;

			EdgePos start;
			start.m_WindowCoords.x = 
				xStepped2L + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

			InitScanlineStart(m_RasterInfo, start, grad, xPreStep2, yPreStep1,
							  pos2.m_Color, pos2.m_FogDensity, depth2, invZ2, tuOverZ2, tvOverZ2);

			// ------------------------------------------------------------------
			// initialize edge buffer delta2Int & delta2Frac
			// ------------------------------------------------------------------

			// determine integer x step/y
			I32 dXdYStep2Int = dXdY23 >= 0 ? EGL_IntFromFixed(dXdY23) : -EGL_IntFromFixed(-dXdY23);

			EdgePos delta23Small, delta23Big;
			InitScanlineDeltas(m_RasterInfo, delta23Small, delta23Big, grad, dXdY23, dXdYStep2Int);

			// ------------------------------------------------------------------
			// initialize the x coordinate for right edge
			// ------------------------------------------------------------------

			EGL_Fixed xStepped1R = pos1.m_WindowCoords.x + EGL_Mul(pos2.m_WindowCoords.y - pos1.m_WindowCoords.y + yPreStep2, dXdY3);
			delta.m_WindowCoords.x = 
				xStepped1R + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

			// ------------------------------------------------------------------
			// initialize the x-step error
			// ------------------------------------------------------------------

			EGL_Fixed xStepError, xError;

			if (dXdY23 >= 0) {
				xStepError = dXdY23 - EGL_FixedFromInt(dXdYStep2Int);
				assert(xStepError >= 0 && xStepError < EGL_ONE);
				xError = EGL_FractionFromFixed(xStepped2L + EGL_ONE/2);
			} else {
				xStepError = -dXdY23 + EGL_FixedFromInt(dXdYStep2Int);
				assert(xStepError >= 0 && xStepError < EGL_ONE);
				xError = (EGL_ONE-1) - EGL_FractionFromFixed(xStepped2L + EGL_ONE/2);
			}

			// ------------------------------------------------------------------
			// Raster the triangle
			// ------------------------------------------------------------------

			for (; y < y3; ++y) {
#if HasScissor
				if (!m_State->m_ScissorTest.Enabled || (y >= yScissorStart && y < yScissorEnd))
#endif
					RasterScanLine(m_RasterInfo, start, delta);												

				ScanlineDelta(m_RasterInfo, start, delta, delta23Small, delta23Big,
							  xError, xStepError, dXdY3);
			}

		}

		return;
	}

	EGL_Fixed invDeltaY2 = EGL_Inverse(deltaY2);	
	EGL_Fixed invDeltaY3 = EGL_Inverse(deltaY3);	// deltaY3 == 0 should not occur, triangle would already have been skipped

	EGL_Fixed deltaX2	= pos2.m_WindowCoords.x - pos1.m_WindowCoords.x;
	EGL_Fixed deltaX3	= pos3.m_WindowCoords.x - pos1.m_WindowCoords.x;

	EGL_Fixed dXdY2		= EGL_Mul(deltaX2, invDeltaY2);
	EGL_Fixed dXdY3		= EGL_Mul(deltaX3, invDeltaY3);

	if (dXdY2 < dXdY3) {

		// ------------------------------------------------------------------
		// initialize start onto first pixel right off line p1->p2
		// ------------------------------------------------------------------

		EGL_Fixed xStepped1L = pos1.m_WindowCoords.x + EGL_Mul(yPreStep1, dXdY2);
		EGL_Fixed xStepped1R = pos1.m_WindowCoords.x + EGL_Mul(yPreStep1, dXdY3);
		EGL_Fixed xRounded1 = EGL_NearestInt(xStepped1L);
		EGL_Fixed xPreStep1 = xRounded1 + (EGL_ONE/2) - pos1.m_WindowCoords.x;

		EdgePos start;
		start.m_WindowCoords.x = 
			xStepped1L + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

		InitScanlineStart(m_RasterInfo, start, grad, xPreStep1, yPreStep1,
						  pos1.m_Color, pos1.m_FogDensity, depth1, invZ1, tuOverZ1, tvOverZ1);

		// ------------------------------------------------------------------
		// initialize edge buffer delta2Int & delta2Frac
		// ------------------------------------------------------------------

		// determine integer x step/y
		I32 dXdYStep1Int = dXdY2 >= 0 ? EGL_IntFromFixed(dXdY2) : -EGL_IntFromFixed(-dXdY2);

		EdgePos delta2Small, delta2Big;
		InitScanlineDeltas(m_RasterInfo, delta2Small, delta2Big, grad, dXdY2, dXdYStep1Int);

		// ------------------------------------------------------------------
		// initialize the x coordinate for right edge
		// ------------------------------------------------------------------

		delta.m_WindowCoords.x = 
			xStepped1R + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest


		// ------------------------------------------------------------------
		// initialize the x-step error
		// ------------------------------------------------------------------

		EGL_Fixed xStepError, xError;

		if (dXdY2 >= 0) {
			xStepError = dXdY2 - EGL_FixedFromInt(dXdYStep1Int);
			assert(xStepError >= 0 && xStepError < EGL_ONE);
			xError = EGL_FractionFromFixed(xStepped1L + EGL_ONE/2);
			assert(xError >= 0 && xError < EGL_ONE);
		} else {
			xStepError = -dXdY2 + EGL_FixedFromInt(dXdYStep1Int);
			assert(xStepError >= 0 && xStepError < EGL_ONE);
			xError = (EGL_ONE-1) - EGL_FractionFromFixed(xStepped1L + EGL_ONE/2);
			assert(xError >= 0 && xError < EGL_ONE);
		}

		// ------------------------------------------------------------------
		// Raster the top part of the triangle
		// ------------------------------------------------------------------

		for (; y < y2; ++y) {
#if HasScissor
			if (!m_State->m_ScissorTest.Enabled || (y >= yScissorStart && y < yScissorEnd))
#endif
				RasterScanLine(m_RasterInfo, start, delta);												

			ScanlineDelta(m_RasterInfo, start, delta, delta2Small, delta2Big,
						  xError, xStepError, dXdY3);
		}

		if (y >= y3)
			return;

		EGL_Fixed invDeltaY23 = EGL_Inverse(deltaY23);
		EGL_Fixed deltaX23	= pos3.m_WindowCoords.x - pos2.m_WindowCoords.x;
		EGL_Fixed dXdY23	= EGL_Mul(deltaX23, invDeltaY23);

		// ------------------------------------------------------------------
		// now do second part of triangle
		//
		// ------------------------------------------------------------------
		//
		// initialize start onto first pixel right off line p2->p3
		// ------------------------------------------------------------------

		EGL_Fixed xStepped2L = pos2.m_WindowCoords.x + EGL_Mul(yPreStep2, dXdY23);
		EGL_Fixed xRounded2 = EGL_NearestInt(xStepped2L);
		EGL_Fixed xPreStep2 = xRounded2 + (EGL_ONE/2) - pos2.m_WindowCoords.x;

		start.m_WindowCoords.x = 
			xStepped2L + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

		InitScanlineStart(m_RasterInfo, start, grad, xPreStep2, yPreStep2,
						  pos2.m_Color, pos2.m_FogDensity, depth2, invZ2, tuOverZ2, tvOverZ2);

		// ------------------------------------------------------------------
		// initialize edge buffer delta2Int & delta2Frac
		// ------------------------------------------------------------------

		// determine integer x step/y
		I32 dXdYStep2Int = dXdY23 >= 0 ? EGL_IntFromFixed(dXdY23) : -EGL_IntFromFixed(-dXdY23);

		EdgePos delta23Small, delta23Big;
		InitScanlineDeltas(m_RasterInfo, delta23Small, delta23Big, grad, dXdY23, dXdYStep2Int);

		// ------------------------------------------------------------------
		// initialize the x-step error
		// ------------------------------------------------------------------

		if (dXdY23 >= 0) {
			xStepError = dXdY23 - EGL_FixedFromInt(dXdYStep2Int);
			assert(xStepError >= 0 && xStepError < EGL_ONE);
			xError = EGL_FractionFromFixed(xStepped2L + EGL_ONE/2);
		} else {
			xStepError = -dXdY23 + EGL_FixedFromInt(dXdYStep2Int);
			assert(xStepError >= 0 && xStepError < EGL_ONE);
			xError = (EGL_ONE-1) - EGL_FractionFromFixed(xStepped2L + EGL_ONE/2);
		}


		// ------------------------------------------------------------------
		// Raster the bottom part of the triangle
		// ------------------------------------------------------------------

		for (; y < y3; ++y) {
#if HasScissor
			if (!m_State->m_ScissorTest.Enabled || (y >= yScissorStart && y < yScissorEnd))
#endif
				RasterScanLine(m_RasterInfo, start, delta);												

			ScanlineDelta(m_RasterInfo, start, delta, delta23Small, delta23Big,
						  xError, xStepError, dXdY3);
		}

	} else /* dXdY2 >= dXdY3) */ {

		// ------------------------------------------------------------------
		// initialize start onto first pixel right off line p1->p3
		// ------------------------------------------------------------------

		EGL_Fixed xStepped1L = pos1.m_WindowCoords.x + EGL_Mul(yPreStep1, dXdY3);
		EGL_Fixed xRounded1 = EGL_NearestInt(xStepped1L);
		EGL_Fixed xPreStep1 = xRounded1 + (EGL_ONE/2) - pos1.m_WindowCoords.x;

		EdgePos start;

		start.m_WindowCoords.x		= xStepped1L + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

		InitScanlineStart(m_RasterInfo, start, grad, xPreStep1, yPreStep1,
						  pos1.m_Color, pos1.m_FogDensity, depth1, invZ1, tuOverZ1, tvOverZ1);

		// ------------------------------------------------------------------
		// initialize edge buffer delta2Int & delta2Frac
		// ------------------------------------------------------------------

		// determine integer x step/y
		I32 dXdYStep1Int = dXdY3 >= 0 ? EGL_IntFromFixed(dXdY3) : -EGL_IntFromFixed(-dXdY3);

		EdgePos delta3Small, delta3Big;
		InitScanlineDeltas(m_RasterInfo, delta3Small, delta3Big, grad, dXdY3, dXdYStep1Int);

		// ------------------------------------------------------------------
		// initialize the x coordinate for right edge
		// ------------------------------------------------------------------

		EGL_Fixed xStepped1R = pos1.m_WindowCoords.x + EGL_Mul(yPreStep1, dXdY2);
		delta.m_WindowCoords.x = 
			xStepped1R + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

		// ------------------------------------------------------------------
		// initialize the x-step error
		// ------------------------------------------------------------------

		EGL_Fixed xStepError, xError;

		if (dXdY3 >= 0) {
			xStepError = dXdY3 - EGL_FixedFromInt(dXdYStep1Int);
			assert(xStepError >= 0 && xStepError < EGL_ONE);
			xError = EGL_FractionFromFixed(xStepped1L + EGL_ONE/2);
		} else {
			xStepError = -dXdY3 + EGL_FixedFromInt(dXdYStep1Int);
			assert(xStepError >= 0 && xStepError < EGL_ONE);
			xError = (EGL_ONE-1) - EGL_FractionFromFixed(xStepped1L + EGL_ONE/2);
		}

		// ------------------------------------------------------------------
		// Raster the top part of the triangle
		// ------------------------------------------------------------------

		for (; y < y2; ++y) {
#if HasScissor
			if (!m_State->m_ScissorTest.Enabled || (y >= yScissorStart && y < yScissorEnd))
#endif
				RasterScanLine(m_RasterInfo, start, delta);												

			ScanlineDelta(m_RasterInfo, start, delta, delta3Small, delta3Big,
						  xError, xStepError, dXdY2);
		}

		if (y >= y3)
			return;

		EGL_Fixed invDeltaY23 = EGL_Inverse(deltaY23);
		EGL_Fixed deltaX23	= pos3.m_WindowCoords.x - pos2.m_WindowCoords.x;
		EGL_Fixed dXdY23	= EGL_Mul(deltaX23, invDeltaY23);

		// ------------------------------------------------------------------
		// now do second part of triangle
		//
		// ------------------------------------------------------------------
		//
		// initialize end behind first pixel right off line p2->p3
		// ------------------------------------------------------------------

		EGL_Fixed xStepped2R = pos2.m_WindowCoords.x + EGL_Mul(yPreStep2, dXdY23);

		delta.m_WindowCoords.x = 
			xStepped2R + ((EGL_ONE/2) - 1);	// added offset so round down will be round to nearest

		for (; y < y3; ++y) {
#if HasScissor
			if (!m_State->m_ScissorTest.Enabled || (y >= yScissorStart && y < yScissorEnd))
#endif
				RasterScanLine(m_RasterInfo, start, delta);												

			ScanlineDelta(m_RasterInfo, start, delta, delta3Small, delta3Big,
						  xError, xStepError, dXdY23);
		}
	}
}


#undef ScanlineDelta
#undef ScanlineDeltaFogBig
#undef ScanlineDeltaFogSmall
#undef ScanlineDeltaDepthBig
#undef ScanlineDeltaDepthSmall
#undef ScanlineDeltaColorBig
#undef ScanlineDeltaColorSmall
#undef ScanlineDeltaTextureBig
#undef ScanlineDeltaTextureSmall
#undef ScanlineDeltaStencil
#undef ScanlineDeltaDepth


#undef InitScanlineStart
#undef InitScanlineDeltas
#undef RasterTriangle

#undef HasFog
#undef HasDepth
#undef HasColor
#undef HasTexture
#undef HasStencil
#undef HasScissor
