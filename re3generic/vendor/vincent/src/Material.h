#ifndef EGL_MATERIAL_H
#define EGL_MATERIAL_H 1

// ==========================================================================
//
// Material.h		Material Class for 3D Rendering Library
//
// --------------------------------------------------------------------------
//
// 09-14-2003		Hans-Martin Will	initial version
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
#include "FractionalColor.h"


namespace EGL {

	class Material {

	public:
		Material();

		void SetAmbientColor(const FractionalColor & color);
		inline const FractionalColor& GetAmbientColor() const;

		void SetDiffuseColor(const FractionalColor & color);
		inline const FractionalColor& GetDiffuseColor() const;

		void SetSpecularColor(const FractionalColor & color);
		inline const FractionalColor& GetSpecularColor() const;

		void SetEmissiveColor(const FractionalColor & color);
		inline const FractionalColor& GetEmissiveColor() const;

		void SetSpecularExponent(EGL_Fixed exponent);
		inline EGL_Fixed GetSpecularExponent() const;

	private:
		FractionalColor			m_AmbientColor;
		FractionalColor			m_DiffuseColor;
		FractionalColor			m_SpecularColor;
		FractionalColor			m_EmissiveColor;
		EGL_Fixed				m_SpecularExponent;
	};


	// --------------------------------------------------------------------------
	// Inline Functions
	// --------------------------------------------------------------------------


	inline const FractionalColor& Material :: GetAmbientColor() const {
		return m_AmbientColor;
	}


	inline const FractionalColor& Material :: GetDiffuseColor() const {
		return m_DiffuseColor;
	}


	inline const FractionalColor& Material :: GetSpecularColor() const {
		return m_SpecularColor;
	}


	inline const FractionalColor& Material :: GetEmissiveColor() const {
		return m_EmissiveColor;
	}


	inline EGL_Fixed Material :: GetSpecularExponent() const {
		return m_SpecularExponent;
	}
}

#endif //ndef EGL_MATERIAL_H
