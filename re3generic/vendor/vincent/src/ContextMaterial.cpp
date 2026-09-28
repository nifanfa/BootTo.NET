// ==========================================================================
//
// ContextMaterial.cpp	Rendering Context Class for 3D Rendering Library
//
//						Material related settings
//
// --------------------------------------------------------------------------
//
// 08-02-2003	Hans-Martin Will	initial version
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
#include "RasterizerState.h"
#include "Utils.h"

using namespace EGL;


// ==========================================================================
// Material Functions on Context
// ==========================================================================

void Context :: Materialx(GLenum face, GLenum pname, GLfixed param) { 

	if (face != GL_FRONT_AND_BACK) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	Material * material = &m_FrontMaterial;

	switch (pname) {
	case GL_SHININESS:
		if (param < 0 || param > EGL_FixedFromInt(128)) {
			RecordError(GL_INVALID_VALUE);
		} else {
			material->SetSpecularExponent(param);
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}

void Context :: Materialxv(GLenum face, GLenum pname, const GLfixed *params) { 

	if (face != GL_FRONT_AND_BACK) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	Material * material = &m_FrontMaterial;

	switch (pname) {
	case GL_AMBIENT:
		material->SetAmbientColor(FractionalColor::Clamp(params));
		break;

	case GL_DIFFUSE:
		material->SetDiffuseColor(FractionalColor::Clamp(params));
		break;

	case GL_AMBIENT_AND_DIFFUSE:
		material->SetAmbientColor(FractionalColor::Clamp(params));
		material->SetDiffuseColor(FractionalColor::Clamp(params));
		break;

	case GL_SPECULAR:
		material->SetSpecularColor(FractionalColor::Clamp(params));
		break;

	case GL_EMISSION:
		material->SetEmissiveColor(FractionalColor::Clamp(params));
		break;

	default:
		Materialx(face, pname, *params);
		break;
	}
}

bool Context :: GetMaterialxv(GLenum face, GLenum pname, GLfixed *params) {
	if (face != GL_FRONT && face != GL_BACK) {
		RecordError(GL_INVALID_ENUM);
		return false;
	}

	switch (pname) {
	case GL_AMBIENT:
		CopyColor(m_FrontMaterial.GetAmbientColor(), params);
		break;

	case GL_DIFFUSE:
		CopyColor(m_FrontMaterial.GetDiffuseColor(), params);
		break;

	case GL_SPECULAR:
		CopyColor(m_FrontMaterial.GetSpecularColor(), params);
		break;

	case GL_EMISSION:
		CopyColor(m_FrontMaterial.GetEmissiveColor(), params);
		break;

	case GL_SHININESS:
		params[0] = m_FrontMaterial.GetSpecularExponent();
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return false;
	}

	return true;
}

