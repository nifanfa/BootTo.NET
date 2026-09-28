// ==========================================================================
//
// triangles.cpp	Rendering Context Class for 3D Rendering Library
//
//					Rendering Operations for Triangles
//
// --------------------------------------------------------------------------
//
// 08-12-2003	Hans-Martin Will	initial version
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
#include "fixed.h"
#include "Rasterizer.h"

using namespace EGL;


// --------------------------------------------------------------------------
// Triangles
// --------------------------------------------------------------------------


void Context :: RenderTriangles(GLint first, GLsizei count) {

	m_Rasterizer->PrepareTriangle();

	while (count >= 3) {
		count -= 3;

		RasterPos pos0, pos1, pos2;
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos1);
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);

		RenderTriangle(pos0, pos1, pos2);
	}
}


void Context :: RenderTriangles(GLsizei count, const GLubyte * indices) {

	m_Rasterizer->PrepareTriangle();

	while (count >= 3) {
		count -= 3;

		RasterPos pos0, pos1, pos2;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);

		RenderTriangle(pos0, pos1, pos2);
	}
}


void Context :: RenderTriangles(GLsizei count, const GLushort * indices) {

	m_Rasterizer->PrepareTriangle();

	while (count >= 3) {
		count -= 3;

		RasterPos pos0, pos1, pos2;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);

		RenderTriangle(pos0, pos1, pos2);
	}
}


// --------------------------------------------------------------------------
// Triangle Strips
// --------------------------------------------------------------------------


void Context :: RenderTriangleStrip(GLint first, GLsizei count) {

	if (count < 3) {
		return;
	}

	m_Rasterizer->PrepareTriangle();

	RasterPos pos0, pos1, pos2;

	SelectArrayElement(first++);
	CurrentValuesToRasterPos(&pos0);
	SelectArrayElement(first++);
	CurrentValuesToRasterPos(&pos1);

	count -= 2;

	while (count >= 6) {
		count -= 6;

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos2, pos1, pos0);

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos2, pos0, pos1);

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos1, pos0, pos2);

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos1, pos2, pos0);

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos0, pos2, pos1);
	}

	if (count >= 1) {
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);
	}

	if (count >= 2) {
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos2, pos1, pos0);
	}

	if (count >= 3) {
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos2, pos0, pos1);
	}

	if (count >= 4) {
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos1, pos0, pos2);
	}

	if (count >= 5) {
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos1, pos2, pos0);
	}

}


void Context :: RenderTriangleStrip(GLsizei count, const GLubyte * indices) {

	if (count < 3) {
		return;
	}

	m_Rasterizer->PrepareTriangle();

	RasterPos pos0, pos1, pos2;

	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos0);
	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos1);

	count -= 2;

	while (count >= 6) {
		count -= 6;

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos2, pos1, pos0);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos2, pos0, pos1);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos1, pos0, pos2);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos1, pos2, pos0);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos0, pos2, pos1);
	}

	if (count >= 1) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);
	}

	if (count >= 2) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos2, pos1, pos0);
	}

	if (count >= 3) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos2, pos0, pos1);
	}

	if (count >= 4) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos1, pos0, pos2);
	}

	if (count >= 5) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos1, pos2, pos0);
	}

}


void Context :: RenderTriangleStrip(GLsizei count, const GLushort * indices) {

	if (count < 3) {
		return;
	}

	m_Rasterizer->PrepareTriangle();

	RasterPos pos0, pos1, pos2;

	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos0);
	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos1);

	count -= 2;

	while (count >= 6) {
		count -= 6;

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos2, pos1, pos0);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos2, pos0, pos1);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos1, pos0, pos2);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos1, pos2, pos0);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos0, pos2, pos1);
	}

	if (count >= 1) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);
	}

	if (count >= 2) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos2, pos1, pos0);
	}

	if (count >= 3) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos2, pos0, pos1);
	}

	if (count >= 4) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos1, pos0, pos2);
	}

	if (count >= 5) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderTriangle(pos1, pos2, pos0);
	}

}


// --------------------------------------------------------------------------
// Triangle Fans
// --------------------------------------------------------------------------


void Context :: RenderTriangleFan(GLint first, GLsizei count) {

	if (count < 3) {
		return;
	}

	m_Rasterizer->PrepareTriangle();

	RasterPos pos0, pos1, pos2;

	SelectArrayElement(first++);
	CurrentValuesToRasterPos(&pos0);
	SelectArrayElement(first++);
	CurrentValuesToRasterPos(&pos1);

	count -= 2;

	while (count >= 2) {
		count -= 2;

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);

		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos0, pos2, pos1);
	}

	if (count >= 1) {
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);
	}
}


void Context :: RenderTriangleFan(GLsizei count, const GLubyte * indices) {

	if (count < 3) {
		return;
	}

	m_Rasterizer->PrepareTriangle();

	RasterPos pos0, pos1, pos2;

	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos0);
	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos1);

	count -= 2;

	while (count >= 2) {
		count -= 2;

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos0, pos2, pos1);
	}

	if (count >= 1) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);
	}
}


void Context :: RenderTriangleFan(GLsizei count, const GLushort * indices) {

	if (count < 3) {
		return;
	}

	m_Rasterizer->PrepareTriangle();

	RasterPos pos0, pos1, pos2;

	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos0);
	SelectArrayElement(*indices++);
	CurrentValuesToRasterPos(&pos1);

	count -= 2;

	while (count >= 2) {
		count -= 2;

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);

		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);
		RenderTriangle(pos0, pos2, pos1);
	}

	if (count >= 1) {
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos2);
		RenderTriangle(pos0, pos1, pos2);
	}
}


namespace {

	inline EGL_Fixed Interpolate(EGL_Fixed x0, EGL_Fixed x1, EGL_Fixed num, EGL_Fixed denom) {
		return static_cast<EGL_Fixed>(x1 + (((I64)(x0-x1))*num)/denom);
	}

	inline void Interpolate(RasterPos& result, const RasterPos& dst, const RasterPos& src, EGL_Fixed num, EGL_Fixed denom) {
		result.m_ClipCoords.setX(Interpolate(dst.m_ClipCoords.x(), src.m_ClipCoords.x(), num, denom));
		result.m_ClipCoords.setY(Interpolate(dst.m_ClipCoords.y(), src.m_ClipCoords.y(), num, denom));
		result.m_ClipCoords.setZ(Interpolate(dst.m_ClipCoords.z(), src.m_ClipCoords.z(), num, denom));
		result.m_ClipCoords.setW(Interpolate(dst.m_ClipCoords.w(), src.m_ClipCoords.w(), num, denom));
		result.m_Color.r = Interpolate(dst.m_Color.r, src.m_Color.r, num, denom);
		result.m_Color.g = Interpolate(dst.m_Color.g, src.m_Color.g, num, denom);
		result.m_Color.b = Interpolate(dst.m_Color.b, src.m_Color.b, num, denom);
		result.m_Color.a = Interpolate(dst.m_Color.a, src.m_Color.a, num, denom);

		for (size_t index = 0; index < EGL_NUM_TEXTURE_UNITS; ++index) {
			result.m_TextureCoords[index].tu = Interpolate(dst.m_TextureCoords[index].tu, src.m_TextureCoords[index].tu, num, denom);
			result.m_TextureCoords[index].tv = Interpolate(dst.m_TextureCoords[index].tv, src.m_TextureCoords[index].tv, num, denom);
		}

		result.m_FogDensity = Interpolate(dst.m_FogDensity, src.m_FogDensity, num, denom);
	}

	inline void InterpolateWithEye(RasterPos& result, const RasterPos& dst, const RasterPos& src, EGL_Fixed num, EGL_Fixed denom) {
		result.m_EyeCoords.setX(Interpolate(dst.m_EyeCoords.x(), src.m_EyeCoords.x(), num, denom));
		result.m_EyeCoords.setY(Interpolate(dst.m_EyeCoords.y(), src.m_EyeCoords.y(), num, denom));
		result.m_EyeCoords.setZ(Interpolate(dst.m_EyeCoords.z(), src.m_EyeCoords.z(), num, denom));
		result.m_EyeCoords.setW(Interpolate(dst.m_EyeCoords.w(), src.m_EyeCoords.w(), num, denom));
		Interpolate(result, dst, src, num, denom);
	}

	inline size_t ClipXLow(RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {

#		define SET_COORDINATE setX
#		define COORDINATE x()
#		include "TriangleClipperLow.inc"
#		undef COORDINATE
#		undef SET_COORDINATE

	}

	inline size_t ClipXHigh(RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {

#		define SET_COORDINATE setX
#		define COORDINATE x()
#		include "TriangleClipperHigh.inc"
#		undef COORDINATE
#		undef SET_COORDINATE

	}

	inline size_t ClipYLow(RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {

#		define SET_COORDINATE setY
#		define COORDINATE y()
#		include "TriangleClipperLow.inc"
#		undef COORDINATE
#		undef SET_COORDINATE

	}

	inline size_t ClipYHigh(RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {

#		define SET_COORDINATE setY
#		define COORDINATE y()
#		include "TriangleClipperHigh.inc"
#		undef COORDINATE
#		undef SET_COORDINATE

	}

	inline size_t ClipZLow(RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {

#		define SET_COORDINATE setZ
#		define COORDINATE z()
#		include "TriangleClipperLow.inc"
#		undef COORDINATE
#		undef SET_COORDINATE

	}

	inline size_t ClipZHigh(RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {

#		define SET_COORDINATE setZ
#		define COORDINATE z()
#		include "TriangleClipperHigh.inc"
#		undef COORDINATE
#		undef SET_COORDINATE

	}


	size_t ClipUser(const Vec4D& plane, RasterPos * input[], size_t inputCount, RasterPos * output[], RasterPos *& nextTemporary) {
		if (inputCount < 3) {
			return 0;
		}

		RasterPos * previous = input[inputCount - 1];
		RasterPos * current;
		int resultCount = 0;

		for (size_t index = 0; index < inputCount; ++index) {

			current = input[index];

			EGL_Fixed c = current->m_EyeCoords * plane;
			EGL_Fixed p = previous->m_EyeCoords * plane;

			if (c >= 0) {
				if (p >= 0) {
					// line segment between previous and current is fully contained in cube
					output[resultCount++] = current;
				} else {
					// line segment between previous and current is intersected;
					// create vertex at intersection, then add current
					RasterPos & newVertex = *nextTemporary++;
					output[resultCount++] = &newVertex;
										
					InterpolateWithEye(newVertex, *current, *previous, p, p - c); 
					output[resultCount++] = current;
				}
			} else {
				if (p >= 0) {
					// line segment between previous and current is intersected;
					// create vertex at intersection and add it
					RasterPos & newVertex = *nextTemporary++;
					output[resultCount++] = &newVertex;
					
					InterpolateWithEye(newVertex, *current, *previous, p, p - c); 
				}
			}

			previous = current;
		}

		return resultCount;
	}


	inline I64 MulLong(EGL_Fixed a, EGL_Fixed b) {
		return (((I64) a * (I64) b)  >> EGL_PRECISION);
	}

	inline EGL_Fixed Round(EGL_Fixed value) {
		return (value + 8) >> 4;
	}

}


void Context :: FrontFace(GLenum mode) { 

	switch (mode) {
		case GL_CW:
			m_ReverseFaceOrientation = true;
			break;

		case GL_CCW:
			m_ReverseFaceOrientation = false;
			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
	}
}

void Context :: CullFace(GLenum mode) { 

	switch (mode) { 
		case GL_FRONT:
			m_CullMode = CullModeFront;
			break;

		case GL_BACK:
			m_CullMode = CullModeBack;
			break;

		case GL_FRONT_AND_BACK:
			m_CullMode = CullModeBackAndFront;
			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
	}
}


inline bool Context :: IsCulled(RasterPos& a, RasterPos& b, RasterPos& c) {

	EGL_Fixed x0 = a.m_ClipCoords.w();
	EGL_Fixed x1 = a.m_ClipCoords.x();
	EGL_Fixed x2 = a.m_ClipCoords.y();
								
	EGL_Fixed y0 = b.m_ClipCoords.w();
	EGL_Fixed y1 = b.m_ClipCoords.x();
	EGL_Fixed y2 = b.m_ClipCoords.y();
								
	EGL_Fixed z0 = c.m_ClipCoords.w();
	EGL_Fixed z1 = c.m_ClipCoords.x();
	EGL_Fixed z2 = c.m_ClipCoords.y();

	I64 sign,t;
	
	if (((x0 & 0xff000000) == 0 || (x0 & 0xff000000) == 0xff000000) &&
		((y0 & 0xff000000) == 0 || (y0 & 0xff000000) == 0xff000000) &&
		((z0 & 0xff000000) == 0 || (z0 & 0xff000000) == 0xff000000)) {
         sign=Round(x0);
         sign*=MulLong(Round(y1), Round(z2)) - MulLong(Round(z1), Round(y2));
         t=Round(y0);
         t*=MulLong(Round(x1), Round(z2)) - MulLong(Round(z1), Round(x2));
         sign-=t;
         t=Round(z0);
         t*=MulLong(Round(x1), Round(y2)) - MulLong(Round(y1), Round(x2));
         sign+=t;
	} else {
         sign=Round(x0>>6);
         sign*=MulLong(Round(y1), Round(z2)) - MulLong(Round(z1), Round(y2));
         t=Round(y0>>6);
         t*=MulLong(Round(x1), Round(z2)) - MulLong(Round(z1), Round(x2));
         sign-=t;
         t=Round(z0>>6);
         t*=MulLong(Round(x1), Round(y2)) - MulLong(Round(y1), Round(x2));
         sign+=t;
	}

	switch (m_CullMode) {
		case CullModeBack:
			return (sign < 0) ^ m_ReverseFaceOrientation;

		case CullModeFront:
			return (sign > 0) ^ m_ReverseFaceOrientation;

		default:
		case CullModeBackAndFront:
			return true;
	}
}


void Context :: RenderTriangle(RasterPos& a, RasterPos& b, RasterPos& c) {

	bool culled = IsCulled(a, b, c);

	if (m_CullFaceEnabled) {
		if (culled) {
			return;
		}
	} 
	
	if (m_RasterizerState.GetShadeModel() == RasterizerState::ShadeModelSmooth) {
		if (m_TwoSidedLightning && culled) {
			a.m_Color = a.m_BackColor;
			b.m_Color = b.m_BackColor;
			c.m_Color = c.m_BackColor;
		} else {
			a.m_Color = a.m_FrontColor;
			b.m_Color = b.m_FrontColor;
			c.m_Color = c.m_FrontColor;
		}
	} else {
		if (m_TwoSidedLightning && culled) {
			a.m_Color =  b.m_Color = c.m_Color = c.m_BackColor;
		} else {
			a.m_Color = b.m_Color = c.m_Color = c.m_FrontColor;
		}
	}

	RasterPos * array1[16];
	array1[0] = &a;
	array1[1] = &b;
	array1[2] = &c;
	RasterPos * array2[16];
	RasterPos * tempVertices = m_Temporary;

	size_t numVertices = 3;

	if (m_ClipPlaneEnabled) {
		for (size_t index = 0, mask = 1; index < NUM_CLIP_PLANES; ++index, mask <<= 1) {
			if (m_ClipPlaneEnabled & mask) {
				numVertices = ClipUser(m_ClipPlanes[index], array1, numVertices, array2, tempVertices);

				if (!numVertices) {
					return;
				}

				for (size_t idx = 0; idx < numVertices; ++idx) {
					array1[idx] = array2[idx];
				}
			}
		}
	}

	numVertices = ClipXLow(array1, numVertices, array2, tempVertices);
	numVertices = ClipXHigh(array2, numVertices, array1, tempVertices);
	numVertices = ClipYLow(array1, numVertices, array2, tempVertices);
	numVertices = ClipYHigh(array2, numVertices, array1, tempVertices);
	numVertices = ClipZLow(array1, numVertices, array2, tempVertices);
	numVertices = ClipZHigh(array2, numVertices, array1, tempVertices);

	if (numVertices >= 3) {
		ClipCoordsToWindowCoords(*array1[0]);
		ClipCoordsToWindowCoords(*array1[1]);

		for (size_t index = 2; index < numVertices; ++index) {
			ClipCoordsToWindowCoords(*array1[index]);
			m_Rasterizer->RasterTriangle(*array1[0], *array1[index - 1], *array1[index]);
		}
	}
}



