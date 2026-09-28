// ==========================================================================
//
// ContextFloat.cpp	Rendering Context Class for 3D Rendering Library
//
//					Emulation of EGL Floating Point Primitives
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


using namespace EGL;


void Context :: AlphaFunc (GLenum func, GLclampf ref) {
	AlphaFuncx(func, EGL_FixedFromFloat(ref));
}

void Context :: ClearColor (GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha) {
	ClearColorx(EGL_FixedFromFloat(red), EGL_FixedFromFloat(green), 
		EGL_FixedFromFloat(blue), EGL_FixedFromFloat(alpha));
}

void Context :: ClearDepthf (GLclampf depth) {
	ClearDepthx(EGL_FixedFromFloat(depth));
}

void Context :: Color4f (GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha) {
	Color4x(EGL_FixedFromFloat(red), EGL_FixedFromFloat(green), 
		EGL_FixedFromFloat(blue), EGL_FixedFromFloat(alpha));
}

void Context :: DepthRangef (GLclampf zNear, GLclampf zFar) {
	DepthRangex(EGL_FixedFromFloat(zNear), EGL_FixedFromFloat(zFar));
}

void Context :: Fogf (GLenum pname, GLfloat param) {
	switch (pname) {
		case GL_FOG_MODE:
			Fogx(pname, static_cast<GLenum>(param));
			break;

		case GL_FOG_START:
		case GL_FOG_END:
		case GL_FOG_DENSITY:
			Fogx(pname, EGL_FixedFromFloat(param));
			break;

		default:
			RecordError(GL_INVALID_ENUM);
	}
}

void Context :: Fogfv (GLenum pname, const GLfloat *params) {

	GLfixed param;

	switch (pname) {
	case GL_FOG_MODE:
		param = static_cast<GLenum>(*params);
		Fogxv(pname, &param);
		break;

	case GL_FOG_DENSITY:
	case GL_FOG_START:
	case GL_FOG_END:
	//case GL_FOG_INDEX:
		param = EGL_FixedFromFloat(*params);
		Fogxv(pname, &param);
		break;

	case GL_FOG_COLOR:
		GLfixed fixed_params[4];

		for (int index = 0; index < 4; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		Fogxv(pname, fixed_params);
		break;
	}
}

void Context :: Frustumf (GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar) {
	Frustumx(EGL_FixedFromFloat(left), EGL_FixedFromFloat(right),
		EGL_FixedFromFloat(bottom), EGL_FixedFromFloat(top), 
		EGL_FixedFromFloat(zNear), EGL_FixedFromFloat(zFar));
}

void Context :: LightModelf (GLenum pname, GLfloat param) {
	LightModelx(pname, EGL_FixedFromFloat(param));
}

void Context :: LightModelfv (GLenum pname, const GLfloat *params) {

	int index;

	switch (pname) {
	case GL_LIGHT_MODEL_AMBIENT:
		GLfixed fixed_params[4];

		for (index = 0; index < 4; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		LightModelxv(pname, fixed_params);
		break;

	//case GL_LIGHT_MODEL_LOCAL_VIEWER:
	case GL_LIGHT_MODEL_TWO_SIDE:
	//case GL_LIGHT_MODEL_COLOR_CONTROL:
		GLfixed param;
		param = EGL_FixedFromFloat(*params);
		LightModelxv(pname, &param);
		break;
	}
}

void Context :: Lightf (GLenum light, GLenum pname, GLfloat param) {
	Lightx(light, pname, EGL_FixedFromFloat(param));
}

void Context :: Lightfv (GLenum light, GLenum pname, const GLfloat *params) {
	//void Context :: Lightxv (GLenum light, GLenum pname, const GLfixed *params);
	GLfixed fixed_params[4];
	int index;

	switch (pname) {
	case GL_AMBIENT:
	case GL_DIFFUSE:
	case GL_SPECULAR:
//	case GL_EMISSION:
	case GL_POSITION:
		for (index = 0; index < 4; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		Lightxv(light, pname, fixed_params);
		break;

	case GL_SPOT_DIRECTION:
		for (index = 0; index < 3; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		Lightxv(light, pname, fixed_params);
		break;

//	case GL_SHININESS:
	case GL_SPOT_EXPONENT:
	case GL_SPOT_CUTOFF:
	case GL_CONSTANT_ATTENUATION:
	case GL_LINEAR_ATTENUATION:
	case GL_QUADRATIC_ATTENUATION:
		fixed_params[0] = EGL_FixedFromFloat(params[0]);
		Lightxv(light, pname, fixed_params);
		break;

	/*
	case GL_COLOR_INDEXES:
		for (index = 0; index < 3; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		glLightxv(light, pname, fixed_params);
		break;
	*/
	}
}

void Context :: LineWidth (GLfloat width) {
	LineWidthx(EGL_FixedFromFloat(width));
}

void Context :: LoadMatrixf (const GLfloat *m) {
	GLfixed mx[16];

	for (int index = 0; index < 16; ++index) {
		mx[index] = EGL_FixedFromFloat(m[index]);
	}

	LoadMatrixx(mx);
}

void Context :: Materialf (GLenum face, GLenum pname, GLfloat param) {
	Materialx(face, pname, EGL_FixedFromFloat(param));
}

void Context :: Materialfv (GLenum face, GLenum pname, const GLfloat *params) {
	GLfixed fixed_params[4];
	int index;

	switch (pname) {
	case GL_AMBIENT:
	case GL_DIFFUSE:
	case GL_AMBIENT_AND_DIFFUSE:
	case GL_SPECULAR:
	case GL_EMISSION:
		for (index = 0; index < 4; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		Materialxv(face, pname, fixed_params);
		break;

	case GL_SHININESS:
		fixed_params[0] = EGL_FixedFromFloat(params[0]);
		Materialxv(face, pname, fixed_params);
		break;

	/*
	case GL_COLOR_INDEXES:
		for (index = 0; index < 3; ++index) {
			fixed_params[index] = EGL_FixedFromFloat(params[index]);
		}

		glMaterialxv(light, pname, fixed_params);
		break;
	*/
	}
}

void Context :: MultMatrixf (const GLfloat *m) {
	GLfixed mx[16];

	for (int index = 0; index < 16; ++index) {
		mx[index] = EGL_FixedFromFloat(m[index]);
	}

	MultMatrixx(mx);
}

void Context :: MultiTexCoord4f (GLenum target, GLfloat s, GLfloat t, GLfloat r, GLfloat q) {
	MultiTexCoord4x(target, 
		EGL_FixedFromFloat(s), EGL_FixedFromFloat(t),
		EGL_FixedFromFloat(r), EGL_FixedFromFloat(q));
}

void Context :: Normal3f (GLfloat nx, GLfloat ny, GLfloat nz) {
	Normal3x(EGL_FixedFromFloat(nx), EGL_FixedFromFloat(ny), EGL_FixedFromFloat(nz));
}

void Context :: Orthof (GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar) {
	Orthox(EGL_FixedFromFloat(left), EGL_FixedFromFloat(right),
		EGL_FixedFromFloat(bottom), EGL_FixedFromFloat(top),
		EGL_FixedFromFloat(zNear), EGL_FixedFromFloat(zFar));
}

void Context :: PointSize (GLfloat size) {
	PointSizex(EGL_FixedFromFloat(size));
}

void Context :: PolygonOffset (GLfloat factor, GLfloat units) {
	PolygonOffsetx(EGL_FixedFromFloat(factor), EGL_FixedFromFloat(units));
}

void Context :: Rotatef (GLfloat angle, GLfloat x, GLfloat y, GLfloat z) {
	Rotatex(EGL_FixedFromFloat(angle), EGL_FixedFromFloat(x),
		EGL_FixedFromFloat(y), EGL_FixedFromFloat(z));
}

void Context :: SampleCoverage (GLclampf value, GLboolean invert) {
	SampleCoveragex(EGL_FixedFromFloat(value), invert);
}

void Context :: Scalef (GLfloat x, GLfloat y, GLfloat z) {
	Scalex(EGL_FixedFromFloat(x), EGL_FixedFromFloat(y), EGL_FixedFromFloat(z));
}

void Context :: TexEnvf (GLenum target, GLenum pname, GLfloat param) {

	switch (pname) {
	case GL_RGB_SCALE:
	case GL_ALPHA_SCALE:
		TexEnvx(target, pname, EGL_FixedFromFloat(param));
		break;

	default:
		TexEnvx(target, pname, (GLfixed) param);
		break;
	}
}

void Context :: TexEnvfv (GLenum target, GLenum pname, const GLfloat *params) {
	GLfixed fixed_params[4];
	int index;

	switch (pname) {
		case GL_TEXTURE_ENV_COLOR:
			for (index = 0; index < 4; ++index) {
				fixed_params[index] = EGL_FixedFromFloat(params[index]);
			}

			TexEnvxv(target, pname, fixed_params);
			break;

		default:
			TexEnvf(target, pname, *params);
			break;
	}
}


void Context :: TexParameterf (GLenum target, GLenum pname, GLfloat param) {
	switch (pname) {
		case GL_TEXTURE_MIN_FILTER:
		case GL_TEXTURE_MAG_FILTER:
		case GL_TEXTURE_WRAP_S:
		case GL_TEXTURE_WRAP_T:
		case GL_GENERATE_MIPMAP:
			TexParameterx(target, pname, (GLfixed) param);
			break;

		default:
			RecordError(GL_INVALID_ENUM);
	}
}

void Context :: Translatef (GLfloat x, GLfloat y, GLfloat z) {
	Translatex(EGL_FixedFromFloat(x), EGL_FixedFromFloat(y), EGL_FixedFromFloat(z));
}

void Context :: ClipPlanef(GLenum plane, const GLfloat *equation) {
	EGL_Fixed fixedEqn[4];

	for (size_t index = 0; index < 4; ++index) {
		fixedEqn[index] = EGL_FixedFromFloat(equation[index]);
	}

	ClipPlanex(plane, fixedEqn);
}

void Context :: DrawTexf(GLfloat x, GLfloat y, GLfloat z, GLfloat width, GLfloat height) {
	assert(0);
}

void Context :: DrawTexfv(GLfloat *coords) {
	assert(0);
}

void Context :: GetClipPlanef(GLenum plane, GLfloat eqn[4]) {
	if (plane < GL_CLIP_PLANE0 || plane >= GL_CLIP_PLANE0 + NUM_CLIP_PLANES) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	size_t index = plane - GL_CLIP_PLANE0;
	eqn[0] = EGL_FloatFromFixed(m_ClipPlanes[index].x());
	eqn[1] = EGL_FloatFromFixed(m_ClipPlanes[index].y());
	eqn[2] = EGL_FloatFromFixed(m_ClipPlanes[index].z());
	eqn[3] = EGL_FloatFromFixed(m_ClipPlanes[index].w());
}


namespace {
	void CopyFixed(const GLfixed * in, GLfloat * out, size_t size) {
		while (size--) {
			*out++ = EGL_FloatFromFixed(*in++);
		}
	}
}


void Context :: GetFloatv(GLenum pname, GLfloat *params) {

	GLfixed buffer[16];

	switch (pname) {
	case GL_FOG_DENSITY:
	case GL_FOG_START:
	case GL_FOG_END:
	case GL_POLYGON_OFFSET_UNITS:
	case GL_POLYGON_OFFSET_FACTOR:
	case GL_SAMPLE_COVERAGE_VALUE:
	case GL_POINT_SIZE:
	case GL_LINE_WIDTH:
	case GL_POINT_SIZE_MIN:
	case GL_POINT_SIZE_MAX:
	case GL_POINT_FADE_THRESHOLD_SIZE:
		if (GetFixedv(pname, buffer)) {
			CopyFixed(buffer, params, 1);
		}

		break;

	case GL_ALIASED_LINE_WIDTH_RANGE:
	case GL_ALIASED_POINT_SIZE_RANGE:
	case GL_SMOOTH_LINE_WIDTH_RANGE:
	case GL_SMOOTH_POINT_SIZE_RANGE:
	case GL_DEPTH_RANGE:
		if (GetFixedv(pname, buffer)) {
			CopyFixed(buffer, params, 2);
		}

		break;

	case GL_CURRENT_NORMAL:
	case GL_POINT_DISTANCE_ATTENUATION:
		if (GetFixedv(pname, buffer)) {
			CopyFixed(buffer, params, 3);
		}

		break;

	case GL_CURRENT_COLOR:
	case GL_CURRENT_TEXTURE_COORDS:
	case GL_FOG_COLOR:
	case GL_LIGHT_MODEL_AMBIENT:
	case GL_COLOR_CLEAR_VALUE:
		if (GetFixedv(pname, buffer)) {
			CopyFixed(buffer, params, 4);
		}

		break;

	case GL_MODELVIEW_MATRIX:
	case GL_PROJECTION_MATRIX:
	case GL_TEXTURE_MATRIX:
		if (GetFixedv(pname, buffer)) {
			CopyFixed(buffer, params, 16);
		}

		break;
	}

}

void Context :: GetLightfv(GLenum light, GLenum pname, GLfloat *params) {

	GLfixed buffer[16];

	switch (pname) {
	case GL_AMBIENT:
	case GL_DIFFUSE:
	case GL_SPECULAR:
	case GL_POSITION:
		if (GetLightxv(light, pname, buffer)) {
			CopyFixed(buffer, params, 4);
		}

		break;

	case GL_SPOT_DIRECTION:
		if (GetLightxv(light, pname, buffer)) {
			CopyFixed(buffer, params, 3);
		}

		break;

	case GL_SPOT_EXPONENT:
	case GL_SPOT_CUTOFF:
	case GL_CONSTANT_ATTENUATION:
	case GL_LINEAR_ATTENUATION:
	case GL_QUADRATIC_ATTENUATION:
		if (GetLightxv(light, pname, buffer)) {
			CopyFixed(buffer, params, 1);
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}

void Context :: GetMaterialfv(GLenum face, GLenum pname, GLfloat *params) {

	GLfixed buffer[16];

	switch (pname) {
	case GL_AMBIENT:
	case GL_DIFFUSE:
	case GL_SPECULAR:
	case GL_EMISSION:
		if (GetMaterialxv(face, pname, buffer)) {
			CopyFixed(buffer, params, 4);
		}

		break;

	case GL_SHININESS:
		if (GetMaterialxv(face, pname, buffer)) {
			CopyFixed(buffer, params, 1);
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}

void Context :: GetTexEnvfv(GLenum env, GLenum pname, GLfloat *params) {

	GLfixed buffer[16];

	switch (env) {
	case GL_TEXTURE_ENV:
		switch (pname) {
		case GL_TEXTURE_ENV_COLOR:
			if (GetTexEnvxv(env, pname, buffer)) {
				CopyFixed(buffer, params, 4);
			}

			break;

		case GL_RGB_SCALE:
		case GL_ALPHA_SCALE:
			if (GetTexEnvxv(env, pname, buffer)) {
				params[0] = EGL_FloatFromFixed(buffer[0]);
			}

			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		break;
	}

}

void Context :: GetTexParameterfv(GLenum target, GLenum pname, GLfloat *params) {

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

#if 0
	switch (pname) {

	default:
#endif
		RecordError(GL_INVALID_ENUM);
#if 0
		break;
	}
#endif
}

void Context :: PointParameterf(GLenum pname, GLfloat param) {
	PointParameterx(pname, EGL_FixedFromFloat(param));
}

void Context :: PointParameterfv(GLenum pname, const GLfloat *params) {
	switch (pname) {
	case GL_POINT_DISTANCE_ATTENUATION:
		{
			GLfixed fixedParams[3];

			for (size_t index = 0; index < 3; ++index) {
				fixedParams[index] = EGL_FixedFromFloat(params[index]);
			}

			PointParameterxv(pname, fixedParams);
		}

		break;

	default:
		PointParameterx(pname, EGL_FixedFromFloat(*params));
	}
}
