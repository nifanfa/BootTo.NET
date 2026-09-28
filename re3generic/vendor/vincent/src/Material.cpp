// ==========================================================================
//
// material.cpp	Rendering Context Class for 3D Rendering Library
//
//				Coloring and Material Related Operations
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
#include "Material.h"


using namespace EGL;


// ==========================================================================
// Material Class Members
// ==========================================================================


Material :: Material():
	m_AmbientColor(F(0.2f), F(0.2f), F(0.2f), F(1.0f)),
	m_DiffuseColor(F(0.8f), F(0.8f), F(0.8f), F(1.0f)),
	m_SpecularExponent(0)
{
}

void Material :: SetAmbientColor(const FractionalColor & color) {
	m_AmbientColor = color;
}


void Material :: SetDiffuseColor(const FractionalColor & color) {
	m_DiffuseColor = color;
}


void Material :: SetSpecularColor(const FractionalColor & color) {
	m_SpecularColor = color;
}


void Material :: SetEmissiveColor(const FractionalColor & color) {
	m_EmissiveColor = color;
}


void Material :: SetSpecularExponent(EGL_Fixed exponent) {
	m_SpecularExponent = EGL_CLAMP(exponent, 0, EGL_FixedFromInt(128));
}



