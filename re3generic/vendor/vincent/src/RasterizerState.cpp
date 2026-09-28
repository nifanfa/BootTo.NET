// ==========================================================================
//
// RasterizerState	Rasterizer State Class for 3D Rendering Library
//
//					The rasterizer state maintains all the state information
//					that is necessary to determine the details of the
//					next primitive to be scan-converted.
//
// --------------------------------------------------------------------------
//
// 10-10-2003		Hans-Martin Will	initial version
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
#include "RasterizerState.h"


using namespace EGL;


RasterizerState :: RasterizerState():
	m_ShadingModel(ShadeModelSmooth),
	m_SampleCoverage(EGL_ONE),
	m_InvertSampleCoverage(false)
{
}


bool RasterizerState :: CompareCommon(const RasterizerState& other) const {
	if (!(m_ShadingModel == other.m_ShadingModel)  ||
		!(m_Fog == other.m_Fog) ||
		!(m_DepthTest == other.m_DepthTest)) {
		return false;
	}

	for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		if (!(m_Texture[unit] == other.m_Texture[unit]))
			return false;
	}

	return
		m_Mask == other.m_Mask &&
		m_Alpha == other.m_Alpha &&
		m_Blend == other.m_Blend &&
		m_ScissorTest == other.m_ScissorTest &&
		m_LogicOp == other.m_LogicOp &&
		m_Stencil == other.m_Stencil &&

		m_SampleCoverage == other.m_SampleCoverage &&
		m_InvertSampleCoverage == other.m_InvertSampleCoverage;
}


bool RasterizerState :: ComparePoint(const RasterizerState& other) const {
	return m_Point == other.m_Point &&
		CompareCommon(other);
}


bool RasterizerState :: CompareLine(const RasterizerState& other) const {
	return m_Line == other.m_Line &&
		CompareCommon(other);
}


bool RasterizerState :: ComparePolygon(const RasterizerState& other) const {
	return m_Polygon == other.m_Polygon &&
		CompareCommon(other);
}

