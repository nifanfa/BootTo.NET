// ==========================================================================
//
// ContextBuffer.cpp	Rendering Context Class for 3D Rendering Library
//
//				Buffer Functions
//
// --------------------------------------------------------------------------
//
// 08-30-2004	Hans-Martin Will	initial version
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
#include "Color.h"
#include "Surface.h"
#include "fixed.h"


using namespace EGL;


// --------------------------------------------------------------------------
// Allocation and selection of buffer objects
// --------------------------------------------------------------------------


size_t * Context :: CurrentBufferForTarget(GLenum target) {
	switch (target) {
	case GL_ARRAY_BUFFER:
		return &m_CurrentArrayBuffer;

	case GL_ELEMENT_ARRAY_BUFFER:
		return &m_CurrentElementArrayBuffer;

	default:
		RecordError(GL_INVALID_ENUM);
		return 0;
	}
}


GLboolean Context :: IsBuffer(GLuint buffer) {
	return m_Buffers.IsObject(buffer);
}

void Context :: BindBuffer(GLenum target, GLuint index) {

	size_t * currentBuffer = CurrentBufferForTarget(target);

	if (!currentBuffer) {
		return;
	}

	if (!index) {
		*currentBuffer = 0;
	} else {
		if (m_Buffers.GetObject(index)) {
			*currentBuffer = index;
		} else {
			RecordError(GL_OUT_OF_MEMORY);
		}
	}
}

void Context :: DeleteBuffers(GLsizei n, const GLuint *buffers) {

	if (n < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	while (n-- != 0) {
		U32 buffer = *buffers++;

		if (buffer != 0) {
			
			if (buffer == m_CurrentArrayBuffer) {
				m_CurrentArrayBuffer = 0;
			}

			if (buffer == m_CurrentElementArrayBuffer) {
				m_CurrentElementArrayBuffer = 0;
			}

			if (buffer != 0) {
				m_Buffers.Deallocate(buffer);
			}
		}
	}
}

void Context :: GenBuffers(GLsizei n, GLuint *buffers) {

	if (n < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	while (n != 0) {
		*buffers++ = m_Buffers.Allocate();
		--n;
	}
}

void Context :: BufferData(GLenum target, GLsizeiptr size, const GLvoid *data, GLenum usage) {

	size_t * currentBuffer = CurrentBufferForTarget(target);

	if (!currentBuffer) {
		return;
	}

	BufferUsage bufferUsage;

	switch (usage) {
	case GL_STATIC_DRAW:
		bufferUsage = BufferUsageStaticDraw;
		break;

	case GL_DYNAMIC_DRAW:
		bufferUsage = BufferUsageDynamicDraw;
		break;

	default:
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (size < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (*currentBuffer == 0 || !m_Buffers.IsObject(*currentBuffer)) {
		RecordError(GL_INVALID_OPERATION);
	}

	Buffer * buffer = m_Buffers.GetObject(*currentBuffer);

	if (buffer->Allocate(size, bufferUsage)) {
		memcpy(buffer->GetData(), data, size);
	} else {
		RecordError(GL_OUT_OF_MEMORY);
	}
}

void Context :: BufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid *data) {

	size_t * currentBuffer = CurrentBufferForTarget(target);

	if (!currentBuffer) {
		return;
	}

	if (*currentBuffer == 0 || !m_Buffers.IsObject(*currentBuffer)) {
		RecordError(GL_INVALID_OPERATION);
	}

	Buffer * buffer = m_Buffers.GetObject(*currentBuffer);

	if (size < 0 || offset < 0 || static_cast<size_t>(size + offset) > buffer->GetSize()) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	U8 * bufferData = static_cast<U8 *>(buffer->GetData()) + offset;

	memcpy(bufferData, data, size);
}

void Context :: GetBufferParameteriv(GLenum target, GLenum pname, GLint *params) {

	size_t * currentBuffer = CurrentBufferForTarget(target);

	if (!currentBuffer) {
		return;
	}

	Buffer * buffer = m_Buffers.GetObject(*currentBuffer);

	switch (pname) {
	case GL_BUFFER_SIZE:
		params[0] = buffer->GetSize();
		break;

	case GL_BUFFER_USAGE:
		params[0] = (buffer->GetUsage() == BufferUsageStaticDraw) ? GL_STATIC_DRAW : GL_DYNAMIC_DRAW;
		break;

	case GL_BUFFER_ACCESS:
		params[0] = GL_WRITE_ONLY;
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}
}


