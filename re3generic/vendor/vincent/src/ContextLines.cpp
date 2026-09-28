// ==========================================================================
//
// ContextLines.cpp	Rendering Context Class for 3D Rendering Library
//
//					Rendering Operations for Lines
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


void Context :: LineWidthx(GLfixed width) { 

	if (width <= 0) {
		RecordError(GL_INVALID_VALUE);
	} else {
		GetRasterizerState()->SetLineWidth(width);
	}
}


// --------------------------------------------------------------------------
// Lines
// --------------------------------------------------------------------------


void Context :: RenderLines(GLint first, GLsizei count) {

	m_Rasterizer->PrepareLine();

	while (count >= 2) {
		count -= 2;

		RasterPos pos0, pos1;
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos1);

		RenderLine(pos0, pos1);
	}
}


void Context :: RenderLines(GLsizei count, const GLubyte * indices) {

	m_Rasterizer->PrepareLine();

	while (count >= 2) {
		count -= 2;

		RasterPos pos0, pos1;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);

		RenderLine(pos0, pos1);
	}
}


void Context :: RenderLines(GLsizei count, const GLushort * indices) {

	m_Rasterizer->PrepareLine();

	while (count >= 2) {
		count -= 2;

		RasterPos pos0, pos1;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos1);

		RenderLine(pos0, pos1);
	}
}


// --------------------------------------------------------------------------
// Line Strips
// --------------------------------------------------------------------------


void Context :: RenderLineStrip(GLint first, GLsizei count) {

	if (count >= 2) {
		m_Rasterizer->PrepareLine();

		RasterPos pos0, pos1;
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		--count;

		while (count >= 2) {
			count -= 2;

			SelectArrayElement(first++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);

			SelectArrayElement(first++);
			CurrentValuesToRasterPos(&pos0);
			RenderLine(pos1, pos0);
		}

		if (count >= 1) {
			SelectArrayElement(first++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);
		}
	}
}


void Context :: RenderLineStrip(GLsizei count, const GLubyte * indices) {

	if (count >= 2) {
		m_Rasterizer->PrepareLine();

		RasterPos pos0, pos1;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		--count;

		while (count >= 2) {
			count -= 2;

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos0);
			RenderLine(pos1, pos0);
		}

		if (count >= 1) {
			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);
		}
	}
}


void Context :: RenderLineStrip(GLsizei count, const GLushort * indices) {

	if (count >= 2) {
		m_Rasterizer->PrepareLine();

		RasterPos pos0, pos1;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		--count;

		while (count >= 2) {
			count -= 2;

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos0);
			RenderLine(pos1, pos0);
		}

		if (count >= 1) {
			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);
		}
	}
}


// --------------------------------------------------------------------------
// Line Loops
// --------------------------------------------------------------------------


void Context :: RenderLineLoop(GLint first, GLsizei count) {

	if (count >= 2) {
		m_Rasterizer->PrepareLine();

		RasterPos pos0, pos1, start;
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&start);
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		RenderLine(start, pos0);

		count -= 2;

		while (count >= 2) {
			count -= 2;

			SelectArrayElement(first++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);

			SelectArrayElement(first++);
			CurrentValuesToRasterPos(&pos0);
			RenderLine(pos1, pos0);
		}

		if (count >= 1) {
			SelectArrayElement(first++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);
			RenderLine(pos1, start);
		} else {
			RenderLine(pos0, start);
		}
	}
}


void Context :: RenderLineLoop(GLsizei count, const GLubyte * indices) {

	if (count >= 2) {
		m_Rasterizer->PrepareLine();

		RasterPos pos0, pos1, start;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&start);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderLine(start, pos0);

		count -= 2;

		while (count >= 2) {
			count -= 2;

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos0);
			RenderLine(pos1, pos0);
		}

		if (count >= 1) {
			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);
			RenderLine(pos1, start);
		} else {
			RenderLine(pos0, start);
		}
	}
}


void Context :: RenderLineLoop(GLsizei count, const GLushort * indices) {

	if (count >= 2) {
		m_Rasterizer->PrepareLine();

		RasterPos pos0, pos1, start;
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&start);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderLine(start, pos0);

		count -= 2;

		while (count >= 2) {
			count -= 2;

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);

			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos0);
			RenderLine(pos1, pos0);
		}

		if (count >= 1) {
			SelectArrayElement(*indices++);
			CurrentValuesToRasterPos(&pos1);
			RenderLine(pos0, pos1);
			RenderLine(pos1, start);
		} else {
			RenderLine(pos0, start);
		}
	}
}


namespace {

	inline EGL_Fixed Interpolate(EGL_Fixed x0, EGL_Fixed x1, EGL_Fixed num, EGL_Fixed denom) {
		return x1 + (EGL_Fixed)((((I64)(x0-x1))*num)/denom);
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

	inline bool ClipX(RasterPos*& from, RasterPos*& to, RasterPos *&tempVertices) {
#		define COORDINATE x()
#		include "LineClipper.inc"
#		undef COORDINATE
	}

	inline bool ClipY(RasterPos*& from, RasterPos*& to, RasterPos *&tempVertices) {
#		define COORDINATE y()
#		include "LineClipper.inc"
#		undef COORDINATE
	}

	inline bool ClipZ(RasterPos*& from, RasterPos*& to, RasterPos *&tempVertices) {
#		define COORDINATE z()
#		include "LineClipper.inc"
#		undef COORDINATE
	}

	inline bool ClipUser(const Vec4D& plane, RasterPos*& from, RasterPos*& to, RasterPos *&tempVertices) {

		EGL_Fixed f = from->m_EyeCoords * plane;
		EGL_Fixed t = to->m_EyeCoords * plane;

		if (f < 0) {
			if (t < 0) {
				return false;
			}

			Interpolate(*tempVertices, *from, *to, t, t - f);
			from = tempVertices++;

			return true;

		} else if (t < 0) {

			Interpolate(*tempVertices, *to, *from, f, f - t);
			to = tempVertices++;

			return true;

		} else {
			// no clipping
			return true;
		}
	}
}


void Context :: RenderLine(RasterPos& from, RasterPos& to) {

	RasterPos * tempVertices = m_Temporary;
	RasterPos * pFrom = &from;
	RasterPos * pTo = &to;

	if (m_ClipPlaneEnabled) {
		for (size_t index = 0, mask = 1; index < NUM_CLIP_PLANES; ++index, mask <<= 1) {
			if (m_ClipPlaneEnabled & mask) {
				if (!ClipUser(m_ClipPlanes[index], pFrom, pTo, tempVertices)) {
					return;
				}
			}
		}
	}

	if (ClipX(pFrom, pTo, tempVertices) &&
		ClipY(pFrom, pTo, tempVertices) &&
		ClipZ(pFrom, pTo, tempVertices)) {

		ClipCoordsToWindowCoords(*pFrom);
		ClipCoordsToWindowCoords(*pTo);

		if (m_RasterizerState.GetShadeModel() == RasterizerState::ShadeModelSmooth) {
			pFrom->m_Color = pFrom->m_FrontColor;
		} else {
			pFrom->m_Color = pTo->m_FrontColor;
		}

		pTo->m_Color = pTo->m_FrontColor;
		m_Rasterizer->RasterLine(*pFrom, *pTo);
	}
}


