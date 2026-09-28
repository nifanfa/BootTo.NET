// ==========================================================================
//
// points.cpp	Rendering Context Class for 3D Rendering Library
//
//				Rendering Operations for Points
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


void Context :: PointSizex(GLfixed size) { 

	if (size <= 0) {
		RecordError(GL_INVALID_VALUE);
	} else {
		m_PointSize = size;
	}
}


inline EGL_Fixed Context :: SelectPointSizeArrayElement(int index) {

	if (!m_PointSizeArray.effectivePointer) {
		// as long as we do not have anti-aliasing, determining the point size here is fine
		return m_PointSize;
	} else {
		EGL_Fixed coords[1];

		m_PointSizeArray.FetchValues(index, coords);
		return coords[0];
	}
}


void Context :: RenderPoints(GLint first, GLsizei count) {

	m_Rasterizer->PreparePoint();

	while (count >= 1) {
		count -= 1;

		RasterPos pos0;
		EGL_Fixed size = SelectPointSizeArrayElement(first);
		SelectArrayElement(first++);
		CurrentValuesToRasterPos(&pos0);
		RenderPoint(pos0, size);
	}
}


void Context :: RenderPoints(GLsizei count, const GLubyte * indices) {

	m_Rasterizer->PreparePoint();

	while (count >= 1) {
		count -= 1;

		RasterPos pos0;
		EGL_Fixed size = SelectPointSizeArrayElement(*indices);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderPoint(pos0, size);
	}
}


void Context :: RenderPoints(GLsizei count, const GLushort * indices) {

	m_Rasterizer->PreparePoint();

	while (count >= 1) {
		count -= 1;

		RasterPos pos0;
		EGL_Fixed size = SelectPointSizeArrayElement(*indices);
		SelectArrayElement(*indices++);
		CurrentValuesToRasterPos(&pos0);
		RenderPoint(pos0, size);
	}
}


void Context :: RenderPoint(RasterPos& point, EGL_Fixed size) {

	// any user defined clip planes?
	if (m_ClipPlaneEnabled) {
		for (size_t index = 0, mask = 1; index < NUM_CLIP_PLANES; ++index, mask <<= 1) {
			if (m_ClipPlaneEnabled & mask) {
				if (point.m_EyeCoords * m_ClipPlanes[index] < 0) {
					return;
				}
			}
		}
	}
	
	// in principle, the scissor test can be included in here
	if (point.m_ClipCoords.x() < -point.m_ClipCoords.w() ||
		point.m_ClipCoords.x() >  point.m_ClipCoords.w() ||
		point.m_ClipCoords.y() < -point.m_ClipCoords.w() ||
		point.m_ClipCoords.y() >  point.m_ClipCoords.w() ||
		point.m_ClipCoords.z() < -point.m_ClipCoords.w() ||
		point.m_ClipCoords.z() >  point.m_ClipCoords.w()) 
		return;

	ClipCoordsToWindowCoords(point);
	point.m_Color = point.m_FrontColor;

	if (m_PointSizeAttenuate) {
		EGL_Fixed eyeDistance = EGL_Abs(point.m_EyeCoords.z());

		EGL_Fixed factor =
			EGL_InvSqrt(m_PointDistanceAttenuation[0] +
						EGL_Mul(m_PointDistanceAttenuation[1], eyeDistance) +
						EGL_Mul(m_PointDistanceAttenuation[2], EGL_Mul(eyeDistance, eyeDistance)));

		size = EGL_Mul(size, factor);
	}

	// as long as we do not have anti-aliasing, determining the effective point size here is fine
	EGL_Fixed pointSize = EGL_Max(size, EGL_ONE);

	m_Rasterizer->RasterPoint(point, pointSize);
}


void Context :: PointSizePointer(GLenum type, GLsizei stride, const GLvoid *pointer) {

	if (type != GL_FIXED && type != GL_FLOAT) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	const size_t size = 1;

	if (stride < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride == 0) {
		switch (type) {
		case GL_BYTE:
			stride = sizeof (GLbyte) * size;
			break;

		case GL_SHORT:
			stride = sizeof (GLshort) * size;
			break;

		case GL_FIXED:
			stride = sizeof (GLfixed) * size;
			break;

		case GL_FLOAT:
			stride = sizeof (GLfloat) * size;
			break;

		}
	}

	m_PointSizeArray.pointer = pointer;
	m_PointSizeArray.stride = stride;
	m_PointSizeArray.type = type;
	m_PointSizeArray.size = size;
	m_PointSizeArray.boundBuffer = m_CurrentArrayBuffer;
}

void Context :: PointParameterx(GLenum pname, GLfixed param) {
	switch (pname) {
	case GL_POINT_SIZE_MIN:
		m_PointSizeMin = param;
		break;

	case GL_POINT_SIZE_MAX:
		m_PointSizeMax = param;
		break;

	case GL_POINT_FADE_THRESHOLD_SIZE:
		m_PointFadeThresholdSize = param;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		break;
	}
}

void Context :: PointParameterxv(GLenum pname, const GLfixed *params) {
	switch (pname) {
	case GL_POINT_DISTANCE_ATTENUATION:
		m_PointDistanceAttenuation[0] = params[0];
		m_PointDistanceAttenuation[1] = params[1];
		m_PointDistanceAttenuation[2] = params[2];

		m_PointSizeAttenuate =
			(params[0] != EGL_ONE || params[1] || params[2]);

		break;

	default:
		PointParameterx(pname, *params);
		break;
	}
}

