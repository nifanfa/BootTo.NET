// ==========================================================================
//
// render.cpp	Rendering Context Class for 3D Rendering Library
//
//				Rendering Operations
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
#include <string.h>
#include "fixed.h"
#include "Surface.h"


using namespace EGL;


// --------------------------------------------------------------------------
// Setup mesh arrays
// --------------------------------------------------------------------------

void Context :: ToggleClientState(GLenum array, bool value) { 
	switch (array) {
	case GL_TEXTURE_COORD_ARRAY:
		m_TexCoordArrayEnabled[m_ClientActiveTexture] = value;
		break;

	case GL_COLOR_ARRAY:
		m_ColorArrayEnabled = value;
		break;

	case GL_NORMAL_ARRAY:
		m_NormalArrayEnabled = value;
		break;

	case GL_VERTEX_ARRAY:
		m_VertexArrayEnabled = value;
		break;

	case GL_POINT_SIZE_ARRAY_OES:
		m_PointSizeArrayEnabled = value;
		break;

	case GL_WEIGHT_ARRAY_OES:
		m_WeightArrayEnabled = value;
		break;

	case GL_MATRIX_INDEX_ARRAY_OES:
		m_MatrixIndexArrayEnabled = value;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
	}
}

void Context :: DisableClientState(GLenum array) { 
	ToggleClientState(array, false);
}

void Context :: EnableClientState(GLenum array) { 
	ToggleClientState(array, true);
}

void Context :: ColorPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) { 

	if (type != GL_UNSIGNED_BYTE && type != GL_FIXED && type != GL_FLOAT) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (size != 4) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride == 0) {
		switch (type) {
		case GL_UNSIGNED_BYTE:
			stride = sizeof (GLubyte) * size;
			break;

		case GL_FIXED:
			stride = sizeof (GLfixed) * size;
			break;

		case GL_FLOAT:
			stride = sizeof (GLfloat) * size;
			break;
		}
	}

	m_ColorArray.pointer = pointer;
	m_ColorArray.stride = stride;
	m_ColorArray.type = type;
	m_ColorArray.size = size;
	m_ColorArray.boundBuffer = m_CurrentArrayBuffer;
}

void Context :: NormalPointer(GLenum type, GLsizei stride, const GLvoid *pointer) { 

	if (type != GL_BYTE && type != GL_SHORT && type != GL_FIXED && type != GL_FLOAT) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	GLsizei size = 3;

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

	m_NormalArray.pointer = pointer;
	m_NormalArray.stride = stride;
	m_NormalArray.type = type;
	m_NormalArray.size = size;
	m_NormalArray.boundBuffer = m_CurrentArrayBuffer;
}

void Context :: VertexPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) { 

	if (type != GL_BYTE && type != GL_SHORT && type != GL_FIXED && type != GL_FLOAT) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (size < 2 || size > 4) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

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

	m_VertexArray.pointer = pointer;
	m_VertexArray.stride = stride;
	m_VertexArray.type = type;
	m_VertexArray.size = size;
	m_VertexArray.boundBuffer = m_CurrentArrayBuffer;
}

void Context :: TexCoordPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) { 

	if (type != GL_BYTE && type != GL_SHORT && type != GL_FIXED && type != GL_FLOAT) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (size < 2 || size > 4) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

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

	m_TexCoordArray[m_ClientActiveTexture].pointer = pointer;
	m_TexCoordArray[m_ClientActiveTexture].stride = stride;
	m_TexCoordArray[m_ClientActiveTexture].type = type;
	m_TexCoordArray[m_ClientActiveTexture].size = size;
	m_TexCoordArray[m_ClientActiveTexture].boundBuffer = m_CurrentArrayBuffer;
}


// --------------------------------------------------------------------------
// Default values of array is disabled
// --------------------------------------------------------------------------


void Context :: Color4x(GLfixed red, GLfixed green, GLfixed blue, GLfixed alpha) { 
	m_DefaultRGBA.r = red;
	m_DefaultRGBA.g = green;
	m_DefaultRGBA.b = blue;
	m_DefaultRGBA.a = alpha;
}


void Context :: MultiTexCoord4x(GLenum target, GLfixed s, GLfixed t, GLfixed r, GLfixed q) { 
	
	if (target < GL_TEXTURE0 || target >= GL_TEXTURE0 + EGL_NUM_TEXTURE_UNITS) {
		// only have one texture unit
		RecordError(GL_INVALID_VALUE);
		return;
	}

	size_t unit = target - GL_TEXTURE0;

	m_DefaultTextureCoords[unit] = Vec4D(s, t, r, q);

	I32 inverse = EGL_Inverse(q);
	m_DefaultScaledTextureCoords[unit].tu = EGL_Mul(s, inverse);
	m_DefaultScaledTextureCoords[unit].tv = EGL_Mul(t, inverse);
}


void Context :: Normal3x(GLfixed nx, GLfixed ny, GLfixed nz) { 
	m_DefaultNormal = Vec3D(nx, ny, nz);
}


// --------------------------------------------------------------------------
// Actual array rendering
// --------------------------------------------------------------------------


void Context :: DrawArrays(GLenum mode, GLint first, GLsizei count) { 

	if (count < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (!m_VertexArrayEnabled ||
		(m_MatrixPaletteEnabled && (!m_MatrixIndexArrayEnabled || !m_WeightArrayEnabled))) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	PrepareRendering();

	switch (mode) {
	case GL_POINTS:
		RenderPoints(first, count);
		break;

	case GL_LINES:
		RenderLines(first, count);
		break;

	case GL_LINE_STRIP:
		RenderLineStrip(first, count);
		break;

	case GL_LINE_LOOP:
		RenderLineLoop(first, count);
		break;

	case GL_TRIANGLES:
		RenderTriangles(first, count);
		break;

	case GL_TRIANGLE_STRIP:
		RenderTriangleStrip(first, count);
		break;

	case GL_TRIANGLE_FAN:
		RenderTriangleFan(first, count);
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}


void Context :: DrawElements(GLenum mode, GLsizei count, GLenum type, const GLvoid *indices) { 
	
	if (count < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (!m_VertexArrayEnabled ||
		(m_MatrixPaletteEnabled && (!m_MatrixIndexArrayEnabled || !m_WeightArrayEnabled))) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	if (m_CurrentElementArrayBuffer) {
		U8 * bufferBase =
			static_cast<U8 *>(m_Buffers.GetObject(m_CurrentElementArrayBuffer)->GetData());

		if (!bufferBase) {
			RecordError(GL_INVALID_OPERATION);
			return;
		}

		size_t offset = static_cast<const U8 *>(indices) - static_cast<const U8 *>(0);
		indices = bufferBase + offset;
	}

	if (!indices) {
		return;
	}

	PrepareRendering();

	switch (mode) {
	case GL_POINTS:
		if (type == GL_UNSIGNED_BYTE) {
			RenderPoints(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderPoints(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	case GL_LINES:
		if (type == GL_UNSIGNED_BYTE) {
			RenderLines(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderLines(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	case GL_LINE_STRIP:
		if (type == GL_UNSIGNED_BYTE) {
			RenderLineStrip(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderLineStrip(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	case GL_LINE_LOOP:
		if (type == GL_UNSIGNED_BYTE) {
			RenderLineLoop(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderLineLoop(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	case GL_TRIANGLES:
		if (type == GL_UNSIGNED_BYTE) {
			RenderTriangles(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderTriangles(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	case GL_TRIANGLE_STRIP:
		if (type == GL_UNSIGNED_BYTE) {
			RenderTriangleStrip(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderTriangleStrip(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	case GL_TRIANGLE_FAN:
		if (type == GL_UNSIGNED_BYTE) {
			RenderTriangleFan(count, reinterpret_cast<const GLubyte *>(indices));
		} else if (type == GL_UNSIGNED_SHORT) {
			RenderTriangleFan(count, reinterpret_cast<const GLushort *>(indices));
		} else {
			RecordError(GL_INVALID_ENUM);
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}


// --------------------------------------------------------------------------
// Load all the current coordinates from either a specific array or from
// the common settings.
//
// Parameters:
//	index		-	The array index from which any array coordinates should
//					be retrieved.
// --------------------------------------------------------------------------
void Context :: SelectArrayElement(int index) {

	if (m_MatrixPaletteEnabled) {
		if (m_WeightArray.effectivePointer) {
			m_WeightArray.FetchValues(index, m_CurrentWeights);
		}

		if (m_MatrixIndexArray.effectivePointer) {
			m_MatrixIndexArray.FetchUnsignedByteValues(index, m_PaletteMatrixIndex);
		}
	}

	assert(m_VertexArray.effectivePointer);

	{
		EGL_Fixed coords[4];

		m_VertexArray.FetchValues(index, coords);

		if (m_VertexArray.size == 3) {
			m_CurrentVertex = Vec4D(coords[0], coords[1], coords[2]);
		} else if (m_VertexArray.size == 2) {
			m_CurrentVertex = Vec4D(coords[0], coords[1], 0);
		} else {
			m_CurrentVertex = Vec4D(coords);
		}
	}

	if (m_NormalArray.effectivePointer) {
		EGL_Fixed coords[3];

		m_NormalArray.FetchValues(index, coords);
		m_CurrentNormal = Vec3D(coords);
	}

	if (m_ColorArray.effectivePointer) {
		EGL_Fixed coords[4];

		m_ColorArray.FetchValues(index, coords);
		m_CurrentRGBA = FractionalColor(coords);
	}

	for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		if (m_TexCoordArray[unit].effectivePointer) {
			EGL_Fixed coords[4];

			m_TexCoordArray[unit].FetchValues(index, coords);

			if (m_TexCoordArray[unit].size < 4) {
				m_CurrentTextureCoords[unit].tu = coords[0];
				m_CurrentTextureCoords[unit].tv = coords[1];
			} else {
				I32 factor = EGL_Inverse(coords[3]);
				m_CurrentTextureCoords[unit].tu = EGL_Mul(coords[0], factor);
				m_CurrentTextureCoords[unit].tv = EGL_Mul(coords[1], factor);
			}
		}
	}
}


namespace {

	inline EGL_Fixed Exp(EGL_Fixed value) {
		return EGL_FixedFromFloat(exp(EGL_FloatFromFixed(-value)));
	}

	inline EGL_Fixed Exp2(EGL_Fixed value) {
		return Exp(EGL_Mul(value, value));
	}
}


// --------------------------------------------------------------------------
// Calculate the fog density for a vertex at the given distance
// --------------------------------------------------------------------------
EGL_Fixed Context :: FogDensity(EGL_Fixed eyeDistance) const {

	switch (m_FogMode) {
		default:
		case FogLinear:
			return EGL_CLAMP(EGL_Mul((m_FogEnd - eyeDistance) >> m_FogGradientShift, m_FogGradient) + 128, 0, EGL_ONE);

		case FogModeExp:
			return EGL_CLAMP(Exp(EGL_Mul(m_FogDensity, eyeDistance)) + 128, 0, EGL_ONE);

		case FogModeExp2:
			return EGL_CLAMP(Exp2(EGL_Mul(m_FogDensity, eyeDistance)) + 128, 0, EGL_ONE);
	}

}


void Context :: PrepareArray(VertexArray & array, bool enabled, bool isColor) {

	array.effectivePointer = 0;

	if (enabled) {
		if (array.boundBuffer) {
			if (m_Buffers.IsObject(array.boundBuffer)) {
				U8 * bufferBase =
					static_cast<U8 *>(m_Buffers.GetObject(array.boundBuffer)->GetData());

				if (!bufferBase) {
					return;
				}

				size_t offset = static_cast<const U8 *>(array.pointer) - static_cast<const U8 *>(0);
				array.effectivePointer = bufferBase + offset;
			} 
		} else {
			array.effectivePointer = array.pointer;
		}
	}

	array.PrepareFetchValues(isColor);
}


void Context :: PrepareRendering() {

	if (m_LightingEnabled) {
		if (m_ColorMaterialEnabled) {
			if (m_TwoSidedLightning) {
                m_GeometryFunction = &Context::CurrentValuesToRasterPosTwoSidedTrack;
			} else {
				m_GeometryFunction = &Context::CurrentValuesToRasterPosOneSidedTrack;
			}
		} else {
			if (m_TwoSidedLightning) {
				m_GeometryFunction = &Context::CurrentValuesToRasterPosTwoSidedNoTrack;
			} else {
				m_GeometryFunction = &Context::CurrentValuesToRasterPosOneSidedNoTrack;
			}
		}
	} else {
		m_GeometryFunction = &Context::CurrentValuesToRasterPosNoLight;
	}

	PrepareArray(m_VertexArray,   m_VertexArrayEnabled);

	PrepareArray(m_NormalArray,	  m_NormalArrayEnabled);

	if (!m_NormalArray.effectivePointer) {
		m_CurrentNormal = m_DefaultNormal;
	}

	PrepareArray(m_ColorArray,    m_ColorArrayEnabled, true);

	if (!m_ColorArray.effectivePointer) {
		m_CurrentRGBA = m_DefaultRGBA;
	}

	for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		PrepareArray(m_TexCoordArray[unit], m_TexCoordArrayEnabled[unit]);

		if (!m_TexCoordArray[unit].effectivePointer) {
			m_CurrentTextureCoords[unit].tu = m_DefaultScaledTextureCoords[unit].tu;
			m_CurrentTextureCoords[unit].tv = m_DefaultScaledTextureCoords[unit].tv;
		}
	}

	PrepareArray(m_PointSizeArray,m_PointSizeArrayEnabled);

	if (m_MatrixPaletteEnabled) {
		PrepareArray(m_WeightArray,	  m_WeightArrayEnabled);
		PrepareArray(m_MatrixIndexArray,m_MatrixIndexArrayEnabled);
		memset(m_CurrentWeights, 0, sizeof m_CurrentWeights);
		memset(m_PaletteMatrixIndex, 0, sizeof m_PaletteMatrixIndex);
	}

}


inline void Context :: CurrentTextureValuesToRasterPos(RasterPos * rasterPos) {
	for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
		if (m_TextureMatrixStack[unit].CurrentMatrix().IsIdentity()) {
			rasterPos->m_TextureCoords[unit].tu = m_CurrentTextureCoords[unit].tu;
			rasterPos->m_TextureCoords[unit].tv = m_CurrentTextureCoords[unit].tv;
		} else {
			Vec3D inCoords(m_CurrentTextureCoords[unit].tu, m_CurrentTextureCoords[unit].tv, 0);
			Vec4D outCoords = m_TextureMatrixStack[unit].CurrentMatrix() * inCoords;
			rasterPos->m_TextureCoords[unit].tu = outCoords.x();
			rasterPos->m_TextureCoords[unit].tv = outCoords.y();
		}
	}
}


// --------------------------------------------------------------------------
// Perform lightning and geometry transformation on the current vertex
// and store the results in buffer for the rasterization stage of the
// pipeline.
//
// Parameters:
//	rasterPos	-	A pointer to a vertex parameter buffer for the
//					rasterization stage
// --------------------------------------------------------------------------


void Context :: CurrentValuesToRasterPosNoLight(RasterPos * rasterPos) {
	FractionalColor color;
	FractionalColor backColor;

	// apply projection matrix to eye coordinates 
	if (!m_MatrixPaletteEnabled) {
		m_ModelViewMatrixStack.CurrentMatrix().Multiply(m_CurrentVertex, rasterPos->m_EyeCoords);
	} else {
		rasterPos->m_EyeCoords = m_MatrixPalette[m_PaletteMatrixIndex[0]] * (m_CurrentVertex * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			rasterPos->m_EyeCoords += m_MatrixPalette[m_PaletteMatrixIndex[index]] * (m_CurrentVertex * m_CurrentWeights[index]);
		}
	}

	rasterPos->m_ClipCoords = m_ProjectionMatrixStack.CurrentMatrix() * rasterPos->m_EyeCoords;

	//	copy current colors to raster pos
	rasterPos->m_FrontColor = rasterPos->m_BackColor = m_CurrentRGBA;

	if (m_RasterizerState.IsEnabledFog()) {
		// populate fog density here...
		rasterPos->m_FogDensity = FogDensity(EGL_Abs(rasterPos->m_EyeCoords.z()));
	} else {
		rasterPos->m_FogDensity = 0;
	}

	CurrentTextureValuesToRasterPos(rasterPos);
}


void Context :: CurrentValuesToRasterPosOneSidedNoTrack(RasterPos * rasterPos) {

	FractionalColor color;
	FractionalColor backColor;

	// apply projection matrix to eye coordinates 
	if (!m_MatrixPaletteEnabled) {
		m_ModelViewMatrixStack.CurrentMatrix().Multiply(m_CurrentVertex, rasterPos->m_EyeCoords);
	} else {
		rasterPos->m_EyeCoords = m_MatrixPalette[m_PaletteMatrixIndex[0]] * (m_CurrentVertex * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			rasterPos->m_EyeCoords += m_MatrixPalette[m_PaletteMatrixIndex[index]] * (m_CurrentVertex * m_CurrentWeights[index]);
		}
	}

	rasterPos->m_ClipCoords = m_ProjectionMatrixStack.CurrentMatrix() * rasterPos->m_EyeCoords;

	// populate fog density here...
	rasterPos->m_FogDensity = FogDensity(EGL_Abs(rasterPos->m_EyeCoords.z()));

	// apply inverse of model view matrix to normals -> eye coordinates normals
	Vec3D eyeNormal;
	
	if (!m_MatrixPaletteEnabled) {
		eyeNormal = m_InverseModelViewMatrix.Multiply3x3(m_CurrentNormal);
	} else {
		eyeNormal = m_MatrixPaletteInverse[m_PaletteMatrixIndex[0]] * (m_CurrentNormal * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			eyeNormal += m_MatrixPaletteInverse[m_PaletteMatrixIndex[index]] * (m_CurrentNormal * m_CurrentWeights[index]);
		}
	}

	if (m_NormalizeEnabled) {
		eyeNormal.Normalize();
	}

	// for each light that is turned on, call into calculation
	int mask = 1;

	color = m_FrontMaterial.GetAmbientColor() * m_LightModelAmbient;
	color.a = m_FrontMaterial.GetDiffuseColor().a;
	color += m_FrontMaterial.GetEmissiveColor();

	for (int index = 0; index < EGL_NUMBER_LIGHTS; ++index, mask <<= 1) {
		if (m_LightEnabled & mask) {
			m_Lights[index].AccumulateLight(rasterPos->m_EyeCoords, eyeNormal, 
				m_FrontMaterial, color);
		}
	}

	color.Clamp();
	rasterPos->m_FrontColor = color;

	CurrentTextureValuesToRasterPos(rasterPos);
}


void Context :: CurrentValuesToRasterPosOneSidedTrack(RasterPos * rasterPos) {
	FractionalColor color;
	FractionalColor backColor;

	// apply projection matrix to eye coordinates 
	if (!m_MatrixPaletteEnabled) {
		m_ModelViewMatrixStack.CurrentMatrix().Multiply(m_CurrentVertex, rasterPos->m_EyeCoords);
	} else {
		rasterPos->m_EyeCoords = m_MatrixPalette[m_PaletteMatrixIndex[0]] * (m_CurrentVertex * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			rasterPos->m_EyeCoords += m_MatrixPalette[m_PaletteMatrixIndex[index]] * (m_CurrentVertex * m_CurrentWeights[index]);
		}
	}

	rasterPos->m_ClipCoords = m_ProjectionMatrixStack.CurrentMatrix() * rasterPos->m_EyeCoords;

	// populate fog density here...
	rasterPos->m_FogDensity = FogDensity(EGL_Abs(rasterPos->m_EyeCoords.z()));

	// apply inverse of model view matrix to normals -> eye coordinates normals
	Vec3D eyeNormal;
	
	if (!m_MatrixPaletteEnabled) {
		eyeNormal = m_InverseModelViewMatrix.Multiply3x3(m_CurrentNormal);
	} else {
		eyeNormal = m_MatrixPaletteInverse[m_PaletteMatrixIndex[0]] * (m_CurrentNormal * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			eyeNormal += m_MatrixPaletteInverse[m_PaletteMatrixIndex[index]] * (m_CurrentNormal * m_CurrentWeights[index]);
		}
	}

	if (m_NormalizeEnabled) {
		eyeNormal.Normalize();
	}

	// for each light that is turned on, call into calculation
	int mask = 1;

	color = m_CurrentRGBA * m_LightModelAmbient;
	color += m_FrontMaterial.GetEmissiveColor();

	for (int index = 0; index < EGL_NUMBER_LIGHTS; ++index, mask <<= 1) {
		if (m_LightEnabled & mask) {
			m_Lights[index].AccumulateLight(rasterPos->m_EyeCoords, eyeNormal, 
				m_FrontMaterial, m_CurrentRGBA, color);
		}
	}

	color.Clamp();
	rasterPos->m_FrontColor = color;

	CurrentTextureValuesToRasterPos(rasterPos);
}


void Context :: CurrentValuesToRasterPosTwoSidedNoTrack(RasterPos * rasterPos) {
	FractionalColor color;
	FractionalColor backColor;

	// apply projection matrix to eye coordinates 
	if (!m_MatrixPaletteEnabled) {
		m_ModelViewMatrixStack.CurrentMatrix().Multiply(m_CurrentVertex, rasterPos->m_EyeCoords);
	} else {
		rasterPos->m_EyeCoords = m_MatrixPalette[m_PaletteMatrixIndex[0]] * (m_CurrentVertex * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			rasterPos->m_EyeCoords += m_MatrixPalette[m_PaletteMatrixIndex[index]] * (m_CurrentVertex * m_CurrentWeights[index]);
		}
	}

	rasterPos->m_ClipCoords = m_ProjectionMatrixStack.CurrentMatrix() * rasterPos->m_EyeCoords;

	// populate fog density here...
	rasterPos->m_FogDensity = FogDensity(EGL_Abs(rasterPos->m_EyeCoords.z()));

	// apply inverse of model view matrix to normals -> eye coordinates normals
	Vec3D eyeNormal;
	
	if (!m_MatrixPaletteEnabled) {
		eyeNormal = m_InverseModelViewMatrix.Multiply3x3(m_CurrentNormal);
	} else {
		eyeNormal = m_MatrixPaletteInverse[m_PaletteMatrixIndex[0]] * (m_CurrentNormal * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			eyeNormal += m_MatrixPaletteInverse[m_PaletteMatrixIndex[index]] * (m_CurrentNormal * m_CurrentWeights[index]);
		}
	}

	if (m_NormalizeEnabled) {
		eyeNormal.Normalize();
	}

	// for each light that is turned on, call into calculation
	int mask = 1;

	color = m_FrontMaterial.GetAmbientColor() * m_LightModelAmbient;
	color.a = m_FrontMaterial.GetDiffuseColor().a;
	color += m_FrontMaterial.GetEmissiveColor();

	backColor = color;		

	for (int index = 0; index < EGL_NUMBER_LIGHTS; ++index, mask <<= 1) {
		if (m_LightEnabled & mask) {
			m_Lights[index].AccumulateLight2(rasterPos->m_EyeCoords, eyeNormal, 
				m_FrontMaterial, color, backColor);
		}
	}

	color.Clamp();
	backColor.Clamp();
	
	rasterPos->m_FrontColor = color;
	rasterPos->m_BackColor = backColor;

	CurrentTextureValuesToRasterPos(rasterPos);
}


void Context :: CurrentValuesToRasterPosTwoSidedTrack(RasterPos * rasterPos) {
	FractionalColor color;
	FractionalColor backColor;

	// apply projection matrix to eye coordinates 
	if (!m_MatrixPaletteEnabled) {
		m_ModelViewMatrixStack.CurrentMatrix().Multiply(m_CurrentVertex, rasterPos->m_EyeCoords);
	} else {
		rasterPos->m_EyeCoords = m_MatrixPalette[m_PaletteMatrixIndex[0]] * (m_CurrentVertex * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			rasterPos->m_EyeCoords += m_MatrixPalette[m_PaletteMatrixIndex[index]] * (m_CurrentVertex * m_CurrentWeights[index]);
		}
	}

	rasterPos->m_ClipCoords = m_ProjectionMatrixStack.CurrentMatrix() * rasterPos->m_EyeCoords;

	// populate fog density here...
	rasterPos->m_FogDensity = FogDensity(EGL_Abs(rasterPos->m_EyeCoords.z()));

	// apply inverse of model view matrix to normals -> eye coordinates normals
	Vec3D eyeNormal;
	
	if (!m_MatrixPaletteEnabled) {
		eyeNormal = m_InverseModelViewMatrix.Multiply3x3(m_CurrentNormal);
	} else {
		eyeNormal = m_MatrixPaletteInverse[m_PaletteMatrixIndex[0]] * (m_CurrentNormal * m_CurrentWeights[0]);

		for (size_t index = 1; index < m_MatrixIndexArray.size; ++index) {
			eyeNormal += m_MatrixPaletteInverse[m_PaletteMatrixIndex[index]] * (m_CurrentNormal * m_CurrentWeights[index]);
		}
	}

	if (m_NormalizeEnabled) {
		eyeNormal.Normalize();
	}

	// for each light that is turned on, call into calculation
	int mask = 1;

	color = m_CurrentRGBA * m_LightModelAmbient;
	color += m_FrontMaterial.GetEmissiveColor();

	backColor = color;		

	for (int index = 0; index < EGL_NUMBER_LIGHTS; ++index, mask <<= 1) {
		if (m_LightEnabled & mask) {
			m_Lights[index].AccumulateLight2(rasterPos->m_EyeCoords, eyeNormal, 
				m_FrontMaterial, m_CurrentRGBA, color, backColor);
		}
	}

	color.Clamp();
	backColor.Clamp();
	
	rasterPos->m_FrontColor = color;
	rasterPos->m_BackColor = backColor;

	CurrentTextureValuesToRasterPos(rasterPos);
}


void Context :: ClipCoordsToWindowCoords(RasterPos & pos) {

	// perform depth division
	EGL_Fixed x = pos.m_ClipCoords.x();
	EGL_Fixed y = pos.m_ClipCoords.y();
	EGL_Fixed z = pos.m_ClipCoords.z();
	EGL_Fixed w = pos.m_ClipCoords.w();

	// fix possible rounding problems
	if (x < -w)	x = -w;
	if (x >= w)	x = w - 1;
	if (y < -w)	y = -w;
	if (y >= w)	y = w - 1;
	if (z < -w)	z = -w;
	if (z >= w)	z = w - 1;

	if ((w >> 24) && (w >> 24) + 1) {
		// keep this value around for perspective-correct texturing
		EGL_Fixed invDenominator = EGL_Inverse(w >> 8);

		// Scale 1/Z by 2^2 to avoid rounding problems during prespective correct
		// interpolation
		// See book by LaMothe for more detailed discussion on this
		pos.m_WindowCoords.invZ = invDenominator << 2;

		pos.m_WindowCoords.x = 
			EGL_Mul(EGL_Mul(x >> 8, invDenominator), m_ViewportScale.x()) + m_ViewportOrigin.x();
		pos.m_WindowCoords.y = 
			EGL_Mul(EGL_Mul(y >> 8, invDenominator), m_ViewportScale.y()) + m_ViewportOrigin.y();
#ifndef RE3_GENERIC
		pos.m_WindowCoords.depth = 
			EGL_CLAMP(EGL_Mul(z >> 8, EGL_Mul(m_DepthRangeFactor, invDenominator))  + m_DepthRangeBase, 0, 0xffff);
#endif

	} else {
		// keep this value around for perspective-correct texturing
		EGL_Fixed invDenominator = w ? EGL_Inverse(w) : 0;

		// Scale 1/Z by 2^10 to avoid rounding problems during prespective correct
		// interpolation
		// See book by LaMothe for more detailed discussion on this
		pos.m_WindowCoords.invZ = invDenominator << 10;

		pos.m_WindowCoords.x = 
			EGL_Mul(EGL_Mul(x, invDenominator), m_ViewportScale.x()) + m_ViewportOrigin.x();
		pos.m_WindowCoords.y = 
			EGL_Mul(EGL_Mul(y, invDenominator), m_ViewportScale.y()) + m_ViewportOrigin.y();
#ifndef RE3_GENERIC
		pos.m_WindowCoords.depth = 
			EGL_CLAMP(EGL_Mul(EGL_Mul(z, invDenominator), m_DepthRangeFactor)  + m_DepthRangeBase, 0, 0xffff);
#endif

	}

#ifdef RE3_GENERIC
	I64 depth = static_cast<I64>(m_DepthRangeBase) * 256;
	if (w)
		depth += (static_cast<I64>(z) * m_DepthRangeFactor * 256) / w;
	pos.m_WindowCoords.depth = static_cast<EGL_Fixed>(EGL_CLAMP(depth, 0, 0xffffff));
#endif
	pos.m_WindowCoords.x = ((pos.m_WindowCoords.x + 0x80) & ~0xff);
	pos.m_WindowCoords.y = ((pos.m_WindowCoords.y + 0x80) & ~0xff);
}

void Context :: GetClipPlanex(GLenum plane, GLfixed eqn[4]) {
	if (plane < GL_CLIP_PLANE0 || plane >= GL_CLIP_PLANE0 + NUM_CLIP_PLANES) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	size_t index = plane - GL_CLIP_PLANE0;
	eqn[0] = m_ClipPlanes[index].x();
	eqn[1] = m_ClipPlanes[index].y();
	eqn[2] = m_ClipPlanes[index].z();
	eqn[3] = m_ClipPlanes[index].w();
}

void Context :: ClipPlanex(GLenum plane, const GLfixed *equation) {
	
	if (plane < GL_CLIP_PLANE0 || plane >= GL_CLIP_PLANE0 + NUM_CLIP_PLANES) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	size_t index = plane - GL_CLIP_PLANE0;
	m_ClipPlanes[index] = m_FullInverseModelViewMatrix.Transpose() * Vec4D(equation);
}

// --------------------------------------------------------------------------
// Additional array pointers for matrix palette support
// --------------------------------------------------------------------------

void Context :: MatrixIndexPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) {
	if (type != GL_UNSIGNED_BYTE) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (size < 1 || size > MATRIX_PALETTE_SIZE) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride == 0) {
		stride = sizeof (GLubyte) * size;
	}

	m_MatrixIndexArray.pointer = pointer;
	m_MatrixIndexArray.stride = stride;
	m_MatrixIndexArray.type = type;
	m_MatrixIndexArray.size = size;
	m_MatrixIndexArray.boundBuffer = m_CurrentArrayBuffer;
}


void Context :: WeightPointer(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) {
	if (type != GL_FIXED && type != GL_FLOAT) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (size < 1 || size > MATRIX_PALETTE_SIZE) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (stride == 0) {
		switch (type) {
		case GL_FIXED:
			stride = sizeof (GLfixed) * size;
			break;

		case GL_FLOAT:
			stride = sizeof (GLfloat) * size;
			break;

		}
	}

	m_WeightArray.pointer = pointer;
	m_WeightArray.stride = stride;
	m_WeightArray.type = type;
	m_WeightArray.size = size;
	m_WeightArray.boundBuffer = m_CurrentArrayBuffer;
}
