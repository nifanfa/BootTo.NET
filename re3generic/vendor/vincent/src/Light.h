#ifndef EGL_LIGHT_H
#define EGL_LIGHT_H 1

// ==========================================================================
//
// Light.h			Light Class for 3D Rendering Library
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
#include "linalg.h"
#include "FractionalColor.h"


namespace EGL {

	class Material;


	class Light {

	public:
		Light();

		void SetAmbientColor(const FractionalColor & color);
		FractionalColor GetAmbientColor() const;

		void SetDiffuseColor(const FractionalColor & color);
		FractionalColor GetDiffuseColor() const;

		void SetSpecularColor(const FractionalColor & color);
		FractionalColor GetSpecularColor() const;

		void SetPosition(const Vec4D & position);
		Vec4D GetPosition() const;

		void SetDirection(const Vec3D & direction);
		Vec3D GetNormalizedDirection() const;
		Vec3D GetDirection() const;

		void SetConstantAttenuation(EGL_Fixed attenuation);
		EGL_Fixed GetConstantAttenuation() const;

		void SetLinearAttenuation(EGL_Fixed attenuation);
		EGL_Fixed GetLinearAttenuation() const;

		void SetQuadraticAttenuation(EGL_Fixed attenuation);
		EGL_Fixed GetQuadraticAttenuation() const;

		void SetSpotExponent(EGL_Fixed exponent);
		EGL_Fixed GetSpotExponent() const;

		void SetSpotCutoff(EGL_Fixed cutoff);
		EGL_Fixed GetSpotCutoff() const;

		void InitWithMaterial(const Material& material);

		// one-sided lightning
		// TO DO:
		// create version for: light at infinity or light at location
		// spot light vs. point light
		// color material vs. material color
		void AccumulateLight(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
			const Material& currMaterial, FractionalColor& result);

		void AccumulateLight(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
			const Material& currMaterial, const FractionalColor& currentColor, FractionalColor& result);


		// two-sided lightning
		// TO DO:
		// create version for: light at infinity or light at location
		// spot light vs. point light
		// color material vs. material color
		void AccumulateLight2(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
			const Material& currMaterial, FractionalColor& result, 
			FractionalColor& result2);

		void AccumulateLight2(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
			const Material& currMaterial, const FractionalColor& currentColor, FractionalColor& result, 
			FractionalColor& result2);

	private:
		FractionalColor			m_AmbientColor;
		FractionalColor			m_DiffuseColor;
		FractionalColor			m_SpecularColor;
		Vec4D					m_Position;
		Vec3D					m_NormalizedSpotDirection;
		Vec3D					m_SpotDirection;
		EGL_Fixed				m_ConstantAttenuation;
		EGL_Fixed				m_LinearAttenuation;
		EGL_Fixed				m_QuadraticAttenuation;
		EGL_Fixed				m_SpotExponent;
		EGL_Fixed				m_SpotCutoff;
		EGL_Fixed				m_CosineSpotCutoff;

		// effective color contributions for current material settings
		FractionalColor			m_EffectiveAmbientColor;
		FractionalColor			m_EffectiveDiffuseColor;
		FractionalColor			m_EffectiveSpecularColor;
	};


	inline FractionalColor Light :: GetAmbientColor() const {
		return m_AmbientColor;
	}

	inline FractionalColor Light :: GetDiffuseColor() const {
		return m_DiffuseColor;
	}

	inline FractionalColor Light :: GetSpecularColor() const {
		return m_SpecularColor;
	}

	inline Vec4D Light :: GetPosition() const {
		return m_Position;
	}

	inline Vec3D Light :: GetNormalizedDirection() const {
		return m_NormalizedSpotDirection;
	}

	inline Vec3D Light :: GetDirection() const {
		return m_SpotDirection;
	}

	inline EGL_Fixed Light :: GetConstantAttenuation() const {
		return m_ConstantAttenuation;
	}

	inline EGL_Fixed Light :: GetLinearAttenuation() const {
		return m_LinearAttenuation;
	}

	inline EGL_Fixed Light :: GetQuadraticAttenuation() const {
		return m_QuadraticAttenuation;
	}

	inline EGL_Fixed Light :: GetSpotExponent() const {
		return m_SpotExponent;
	}

	inline EGL_Fixed Light :: GetSpotCutoff() const {
		return m_SpotCutoff;
	}

}

#endif //ndef EGL_LIGHT_H
