// ==========================================================================
//
// viewport.cpp	Rendering Context Class for 3D Rendering Library
//
//				Viewport Configuration
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
#include "fixed.h"


using namespace EGL;


// --------------------------------------------------------------------------
// Viewport configuration
// --------------------------------------------------------------------------


void Context :: Viewport(GLint x, GLint y, GLsizei width, GLsizei height) { 

	if (width < 0 || height < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	m_Viewport = Rect(x, y, width, height);

	m_ViewportOrigin = Vec3D(EGL_FixedFromInt(x + (width / 2)), EGL_FixedFromInt(y + (height / 2)), m_ViewportOrigin.z());
	m_ViewportScale = Vec3D(EGL_FixedFromInt(width / 2), EGL_FixedFromInt(height / 2), m_ViewportScale.z());

	UpdateScissorTest();
}


void Context :: Scissor(GLint x, GLint y, GLsizei width, GLsizei height) { 

	if (width < 0 || height < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	m_Scissor = Rect(x, y, width, height);
	UpdateScissorTest();
}

void Context :: DepthRangex(GLclampx zNear, GLclampx zFar) { 

	m_DepthRangeNear = EGL_CLAMP(zNear, 0, EGL_ONE);
	m_DepthRangeFar = EGL_CLAMP(zFar, 0, EGL_ONE);

	m_DepthRangeBase = (m_DepthRangeNear + m_DepthRangeFar) / 2;
	m_DepthRangeFactor = (m_DepthRangeFar - m_DepthRangeNear) / 2 - 1;
}


// --------------------------------------------------------------------------
// Update the stencil test settings based on 
//	- the current viewport size
//	- the current drawing surface dimensions
//	- the actual scissor test settings
// --------------------------------------------------------------------------
void Context :: UpdateScissorTest() {

	// we do not need any scissoring within the rasterizer as long as the viewport
	// rectangle is contained inside the surface area and (if the scissor test
	// is enabled) the scissor rectangle. Otherwise, determine the size
	// of the surface area (& optionally intersected with the scissor rectangle)
	// and enable scissoring in the rasterizer accordingly.

	if (m_DrawSurface == 0)
		return;

	if (m_ScissorTestEnabled) {
		Rect rect = Rect::Intersect(m_DrawSurface->GetRect(), m_Scissor);
		m_RasterizerState.SetScissor(rect.x, rect.y, rect.width, rect.height);

		if (rect.Contains(m_Viewport)) {
			m_RasterizerState.EnableScissorTest(false);
		} else {
			m_RasterizerState.EnableScissorTest(true);
		}
	} else {
		const Rect& rect = m_DrawSurface->GetRect();
		m_RasterizerState.SetScissor(rect.x, rect.y, rect.width, rect.height);

		if (rect.Contains(m_Viewport)) {
			m_RasterizerState.EnableScissorTest(false);
		} else {
			m_RasterizerState.EnableScissorTest(true);
		}
	}
}

