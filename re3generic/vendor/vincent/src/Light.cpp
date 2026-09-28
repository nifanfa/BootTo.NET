// ==========================================================================
//
// light.cpp	Rendering Context Class for 3D Rendering Library
//
//				Lightning Related Operations
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
#include "Light.h"
#include "Material.h"
#include <math.h>


#ifndef M_PI
#	define M_PI       3.14159265358979323846
#endif


using namespace EGL;


// ==========================================================================
// class Light
// ==========================================================================


Light :: Light() 
:	m_AmbientColor(F(0.0f), F(0.0f), F(0.0f), F(1.0f)),
	m_DiffuseColor(F(0.0f), F(0.0f), F(0.0f), F(1.0f)),
	m_SpecularColor(F(0.0f), F(0.0f), F(0.0f), F(1.0f)),
	m_Position(F(0.0f), F(0.0f), F(1.0f), F(0.0f)),
	m_SpotDirection(F(0.0f), F(0.0f), F(-1.0f)),
	m_NormalizedSpotDirection(F(0.0f), F(0.0f), F(-1.0f)),
	m_SpotCutoff(F(180.0f)),
	m_ConstantAttenuation(F(1.0f)),
	m_LinearAttenuation(0),
	m_QuadraticAttenuation(0)
{
}


void Light :: SetAmbientColor(const FractionalColor & color) {
	m_AmbientColor = color;
}


void Light :: SetDiffuseColor(const FractionalColor & color) {
	m_DiffuseColor = color;
}


void Light :: SetSpecularColor(const FractionalColor & color) {
	m_SpecularColor = color;
}


void Light :: SetPosition(const Vec4D & position) {
	m_Position = position;
}


void Light :: SetDirection(const Vec3D & direction) {
	m_SpotDirection = m_NormalizedSpotDirection = direction;
	m_NormalizedSpotDirection.Normalize();
}


void Light :: SetConstantAttenuation(EGL_Fixed attenuation) {
	m_ConstantAttenuation = attenuation;
}


void Light :: SetLinearAttenuation(EGL_Fixed attenuation) {
	m_LinearAttenuation = attenuation;
}


void Light :: SetQuadraticAttenuation(EGL_Fixed attenuation) {
	m_QuadraticAttenuation = attenuation;
}


void Light :: SetSpotExponent(EGL_Fixed exponent) {
	m_SpotExponent = exponent;
}


void Light :: SetSpotCutoff(EGL_Fixed cutoff) {
	m_SpotCutoff = cutoff;
	EGL_Fixed angle = EGL_Mul(cutoff, EGL_FixedFromFloat(static_cast<float>(M_PI) / 180.0f));
	m_CosineSpotCutoff = EGL_Cos(angle);
}


void Light :: InitWithMaterial(const Material& material) {

	m_EffectiveAmbientColor = material.GetAmbientColor() * m_AmbientColor;
	m_EffectiveDiffuseColor = material.GetDiffuseColor() * m_DiffuseColor;
	m_EffectiveSpecularColor = material.GetSpecularColor() * m_SpecularColor;
}

// --------------------------------------------------------------------------
// One-sided lightning calculation
// --------------------------------------------------------------------------


#define EGL_Sqrt EGL_FastSqrt


void Light :: AccumulateLight(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
							  const Material& currMaterial,
							  FractionalColor& result) {

	Vec3D vp_li = EGL_Direction(vertexCoords, m_Position);
	EGL_Fixed sqLength = vp_li.LengthSq();			// keep squared length around for later
	vp_li.Normalize();								// can optimizer factor this out?

	EGL_Fixed att = EGL_ONE;

	if (m_SpotCutoff != EGL_FixedFromInt(180)) {
		EGL_Fixed cosine = -(vp_li * m_NormalizedSpotDirection);

		if (cosine < m_CosineSpotCutoff) {
			return;
		} else {
			att = EGL_Power(cosine, m_SpotExponent);
		}
	}

	if (m_Position.w() != 0) {
		EGL_Fixed length = EGL_Sqrt(sqLength);

		att = EGL_Mul(att, EGL_Inverse(m_ConstantAttenuation + 
			EGL_Mul(m_LinearAttenuation, length) +
			EGL_Mul(m_QuadraticAttenuation, sqLength)));
	}

	result.Accumulate(currMaterial.GetAmbientColor() * m_AmbientColor, att);

	EGL_Fixed diffuseFactor = vertexNormal * vp_li;

	if (diffuseFactor > 0) {
		result.Accumulate(currMaterial.GetDiffuseColor() * m_DiffuseColor, EGL_Mul(diffuseFactor, att));

			// add specular component
			// calculate h^
		Vec3D h = vp_li + Vec3D(0, 0, EGL_ONE);
		h.Normalize();
		EGL_Fixed specularFactor = vertexNormal * h;

		if (specularFactor > 0)
			result.Accumulate(currMaterial.GetSpecularColor() * m_SpecularColor, 
				EGL_Mul(att, EGL_Power(specularFactor, currMaterial.GetSpecularExponent())));
	}
}


void Light :: AccumulateLight(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
							  const Material& currMaterial,
							  const FractionalColor & currentColor, 
							  FractionalColor& result) {

	Vec3D vp_li = EGL_Direction(vertexCoords, m_Position);
	EGL_Fixed sqLength = vp_li.LengthSq();			// keep squared length around for later
	vp_li.Normalize();								// can optimizer factor this out?

	EGL_Fixed att = EGL_ONE;

	if (m_SpotCutoff != EGL_FixedFromInt(180)) {
		EGL_Fixed cosine = -(vp_li * m_NormalizedSpotDirection);

		if (cosine < m_CosineSpotCutoff) {
			return;
		} else {
			att = EGL_Power(cosine, m_SpotExponent);
		}
	}

	if (m_Position.w() != 0) {
		EGL_Fixed length = EGL_Sqrt(sqLength);

		att = EGL_Mul(att, EGL_Inverse(m_ConstantAttenuation + 
			EGL_Mul(m_LinearAttenuation, length) +
			EGL_Mul(m_QuadraticAttenuation, sqLength)));
	}

	result.Accumulate(currMaterial.GetAmbientColor() * m_AmbientColor, att);

	EGL_Fixed diffuseFactor = vertexNormal * vp_li;

	if (diffuseFactor > 0) {
		result.Accumulate(currentColor * m_DiffuseColor, EGL_Mul(diffuseFactor, att));

			// add specular component
			// calculate h^
		Vec3D h = vp_li + Vec3D(0, 0, EGL_ONE);
		h.Normalize();
		EGL_Fixed specularFactor = vertexNormal * h;

		if (specularFactor > 0)
			result.Accumulate(currMaterial.GetSpecularColor() * m_SpecularColor, 
				EGL_Mul(att, EGL_Power(specularFactor, currMaterial.GetSpecularExponent())));
	}
}


// --------------------------------------------------------------------------
// Two-sided lightning calculation
// --------------------------------------------------------------------------

void Light :: AccumulateLight2(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
							  const Material& currMaterial,
							  FractionalColor& result, 
							  FractionalColor& result2) {

	Vec3D vp_li = EGL_Direction(vertexCoords, m_Position);
	EGL_Fixed sqLength = vp_li.LengthSq();			// keep squared length around for later
	vp_li.Normalize();								// can optimizer factor this out?
//either front or back color have to be enabled...
//	if (diffuseFactor > 0) {
	EGL_Fixed att = EGL_ONE;

	if (m_SpotCutoff != EGL_FixedFromInt(180)) {
		EGL_Fixed cosine = -(vp_li * m_NormalizedSpotDirection);

		if (cosine < m_CosineSpotCutoff) {
			return;
		} else {
			att = EGL_Power(cosine, m_SpotExponent);
		}
	}

	if (m_Position.w() != 0) {
		EGL_Fixed length = EGL_Sqrt(sqLength);

		att = EGL_Mul(att, EGL_Inverse(m_ConstantAttenuation + 
			EGL_Mul(m_LinearAttenuation, length) +
			EGL_Mul(m_QuadraticAttenuation, sqLength)));
	}

	result.Accumulate(currMaterial.GetAmbientColor() * m_AmbientColor, att);
	result2.Accumulate(currMaterial.GetAmbientColor() * m_AmbientColor, att);

	EGL_Fixed diffuseFactor = vertexNormal * vp_li;

	if (diffuseFactor > 0) {
		result.Accumulate(currMaterial.GetDiffuseColor() * m_DiffuseColor, EGL_Mul(diffuseFactor, att));

		// add specular component
		// calculate h^
		Vec3D h = vp_li + Vec3D(0, 0, EGL_ONE);
		h.Normalize();
		EGL_Fixed specularFactor = vertexNormal * h;

		if (specularFactor > 0)
			result.Accumulate(currMaterial.GetSpecularColor() * m_SpecularColor, 
				EGL_Mul(att, EGL_Power(specularFactor, currMaterial.GetSpecularExponent())));
	} else {
		result2.Accumulate(currMaterial.GetDiffuseColor() * m_DiffuseColor, EGL_Mul(-diffuseFactor, att));

		Vec3D h = vp_li + Vec3D(0, 0, EGL_ONE);
		h.Normalize();
		EGL_Fixed specularFactor = vertexNormal * h;

		if (specularFactor < 0)
			result2.Accumulate(currMaterial.GetSpecularColor() * m_SpecularColor, 
				EGL_Mul(att, EGL_Power(-specularFactor, currMaterial.GetSpecularExponent())));
	}

//	}
}


void Light :: AccumulateLight2(const Vec4D& vertexCoords, const Vec3D& vertexNormal, 
							  const Material& currMaterial,
							  const FractionalColor& currentColor,
							  FractionalColor& result, 
							  FractionalColor& result2) {

	Vec3D vp_li = EGL_Direction(vertexCoords, m_Position);
	EGL_Fixed sqLength = vp_li.LengthSq();			// keep squared length around for later
	vp_li.Normalize();								// can optimizer factor this out?

//either front or back color have to be enabled...
//	if (diffuseFactor > 0) {
	EGL_Fixed att = EGL_ONE;

	if (m_SpotCutoff != EGL_FixedFromInt(180)) {
		EGL_Fixed cosine = -(vp_li * m_NormalizedSpotDirection);

		if (cosine < m_CosineSpotCutoff) {
			return;
		} else {
			att = EGL_Power(cosine, m_SpotExponent);
		}
	}

	if (m_Position.w() != 0) {
		EGL_Fixed length = EGL_Sqrt(sqLength);

		att = EGL_Mul(att, EGL_Inverse(m_ConstantAttenuation + 
			EGL_Mul(m_LinearAttenuation, length) +
			EGL_Mul(m_QuadraticAttenuation, sqLength)));
	}

	result.Accumulate(currMaterial.GetAmbientColor() * m_AmbientColor, att);
	result2.Accumulate(currMaterial.GetAmbientColor() * m_AmbientColor, att);

	EGL_Fixed diffuseFactor = vertexNormal * vp_li;

	if (diffuseFactor > 0) {
		result.Accumulate(currentColor * m_DiffuseColor, EGL_Mul(diffuseFactor, att));
			// add specular component
			// calculate h^
		Vec3D h = vp_li + Vec3D(0, 0, EGL_ONE);
		h.Normalize();
		EGL_Fixed specularFactor = vertexNormal * h;

		if (specularFactor > 0)
			result.Accumulate(currMaterial.GetSpecularColor() * m_SpecularColor, 
				EGL_Mul(att, EGL_Power(specularFactor, currMaterial.GetSpecularExponent())));
	} else {
		result2.Accumulate(currentColor * m_DiffuseColor, EGL_Mul(-diffuseFactor, att));
			// add specular component
			// calculate h^
		Vec3D h = vp_li + Vec3D(0, 0, EGL_ONE);
		h.Normalize();
		EGL_Fixed specularFactor = vertexNormal * h;

		if (specularFactor < 0)
			result2.Accumulate(currMaterial.GetSpecularColor() * m_SpecularColor, 
				EGL_Mul(att, EGL_Power(-specularFactor, currMaterial.GetSpecularExponent())));
	}
//	}
}

