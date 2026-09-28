#ifndef EGL_ARRAYS_H
#define EGL_ARRAYS_H 1

// ==========================================================================
//
// Arrays.h			Array type declarations and helpers 
//
// --------------------------------------------------------------------------
//
// 05-26-2004		Hans-Martin Will	initial version
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
#include "GLES/gl.h"
#include "Types.h"
#include "linalg.h"
#include "Buffer.h"
#include "Texture.h"


namespace EGL {

	struct VertexArray {
		VertexArray() {
			pointer = 0;
			stride = 0;
			size = 4;
			type = GL_FIXED;
			effectivePointer = 0;
			boundBuffer = 0;
			fetchFunction = 0;
		};

		typedef void (VertexArray::*FetchValueFunction)(int row, GLfixed * buffer);

		void FetchByteValues(int row, GLfixed * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLbyte * byteBase = reinterpret_cast<const GLbyte *>(base);

			do {
				*buffer++ = EGL_FixedFromInt(*byteBase++);
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void FetchUnsignedByteValues(int row, GLubyte * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLubyte * byteBase = reinterpret_cast<const GLubyte *>(base);

			do {
				*buffer++ = *byteBase++;
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void FetchUnsignedByteValues(int row, GLfixed * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLubyte * byteBase = reinterpret_cast<const GLubyte *>(base);

			do {
				*buffer++ = EGL_FixedFromInt(*byteBase++);
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void FetchByteColorValues(int row, GLfixed * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLubyte * byteBase = reinterpret_cast<const GLubyte *>(base);

			do {
				U8 byteValue = *byteBase++;
				*buffer++ = static_cast<GLfixed>(((byteValue << 8) | byteValue) + (byteValue >> 7));
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void FetchShortValues(int row, GLfixed * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLshort * shortBase = reinterpret_cast<const GLshort *>(base);

			do {
				*buffer++ = EGL_FixedFromInt(*shortBase++);
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void FetchFixedValues(int row, GLfixed * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLfixed * fixedBase = reinterpret_cast<const GLfixed *>(base);

			do {
				*buffer++ = *fixedBase++;
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void FetchFloatValues(int row, GLfixed * buffer) {
			GLsizei rowOffset = row * stride;
			const unsigned char * base = reinterpret_cast<const unsigned char *>(effectivePointer) + rowOffset;
			size_t count = size;

			const GLfloat * floatBase = reinterpret_cast<const GLfloat *>(base);

			do {
				*buffer++ = EGL_FixedFromFloat(*floatBase++);
			} while (--count);

#ifdef EGL_XSCALE
			_PreLoad(const_cast<unsigned long *>(reinterpret_cast<const unsigned long *>(base + stride)));
#endif

		}

		void PrepareFetchValues(bool colorMode) {
			switch (type) {
			case GL_BYTE:
				fetchFunction = &VertexArray::FetchByteValues;
				
				break;

			case GL_UNSIGNED_BYTE:
				if (colorMode) {
                    fetchFunction = &VertexArray::FetchByteColorValues;
				} else {
                    fetchFunction = &VertexArray::FetchUnsignedByteValues;
				}

				break;

			case GL_SHORT:
				fetchFunction = &VertexArray::FetchShortValues;
				break;

			case GL_FIXED:
				fetchFunction = &VertexArray::FetchFixedValues;
				break;

			case GL_FLOAT:
				fetchFunction = &VertexArray::FetchFloatValues;
				break;

			default:
				fetchFunction = 0;
				break;
			}
		}

		inline void FetchValues(int row, GLfixed * buffer) {
			(this->*fetchFunction)(row, buffer);
		}

		size_t				size;
		GLenum				type;
		GLsizei				stride;
		const GLvoid *		pointer;
		size_t				boundBuffer;
		const void *		effectivePointer;
		FetchValueFunction	fetchFunction;
	};

	template <class ELEMENT>
	struct ObjectArray {
		enum {
			INITIAL_SIZE = 64,
			FACTOR = 2
		};

		struct ObjectRecord {
			size_t value;

			void SetPointer(ELEMENT * texture) {
				value = reinterpret_cast<size_t>(texture);
			}

			ELEMENT * GetPointer() {
				assert(IsPointer());
				return reinterpret_cast<ELEMENT *>(value);
			}

			bool IsPointer() {
				return (value & 1) == 0;
			}

			bool IsNil() {
				return value == ~(size_t)0;
			}

			void SetNil() {
				value = ~(size_t)0;
			}

			void SetIndex(size_t index) {
				value = (index << 1) | 1;
			}

			size_t GetIndex() {
				assert(!IsPointer() && !IsNil());
				return (value >> 1);
			}

		};

		ObjectArray() {
			m_Objects = new ObjectRecord[INITIAL_SIZE];

			for (size_t index = 0; index < INITIAL_SIZE; ++index) {
				m_Objects[index].SetIndex(index + 1);
			}

			m_Objects[INITIAL_SIZE - 1].SetNil();

			m_FreeObjects = m_AllocatedObjects = INITIAL_SIZE;
			m_FreeListHead = 0;
		}

		~ObjectArray() {

			if (m_Objects != 0) {
				for (size_t index = 0; index < m_AllocatedObjects; ++index) {
					if (m_Objects[index].IsPointer() && m_Objects[index].GetPointer())
						delete m_Objects[index].GetPointer();
				}

				delete [] m_Objects;
			}
		}

		void Increase(size_t minSize = 0) {

			assert(m_FreeListHead == 0xffffffffu);

			size_t newAllocatedObjects = m_AllocatedObjects * FACTOR;

			if (newAllocatedObjects < minSize)
				newAllocatedObjects = minSize;

			ObjectRecord * newObjects = new ObjectRecord[newAllocatedObjects];
			size_t index;

			for (index = 0; index < m_AllocatedObjects; ++index) {
				newObjects[index] = m_Objects[index];
			}

			for (index = m_AllocatedObjects; index < newAllocatedObjects - 1; ++index) {
				newObjects[index].SetIndex(index + 1);
			}

			newObjects[newAllocatedObjects - 1].SetNil();

			delete [] m_Objects;
			m_Objects = newObjects;
			m_FreeObjects = newAllocatedObjects - m_AllocatedObjects;
			m_FreeListHead = m_AllocatedObjects;
			m_AllocatedObjects = newAllocatedObjects;
		}

		size_t Allocate() {

			if (m_FreeObjects == 0) {
				Increase();
				assert(m_FreeObjects != 0);
			}

			size_t result = m_FreeListHead;
			m_FreeListHead = m_Objects[result].IsNil() ? 0xffffffffu : m_Objects[result].GetIndex();
			m_FreeObjects--;
			m_Objects[result].SetPointer(0);

			return result;
		}

		void Deallocate(size_t index) {

			if (m_Objects[index].IsPointer()) {
				if (m_Objects[index].GetPointer())
					delete m_Objects[index].GetPointer();

				m_Objects[index].SetIndex(m_FreeListHead);
				m_FreeListHead = index;
				m_FreeObjects++;
			}
		}

		ELEMENT * GetObject(size_t index) {

			if (index >= m_AllocatedObjects)
				Increase(index + 1);
				
			if (!m_Objects[index].IsPointer()) {
				// allocate the requested object
				if (m_FreeListHead == index) {
					m_FreeListHead = m_Objects[index].IsNil() ? 0xffffffffu : m_Objects[index].GetIndex();
				} else {
					assert(m_FreeListHead != 0xffffffffu);
					size_t idx = m_FreeListHead;

					while (!m_Objects[idx].IsNil() && m_Objects[idx].GetIndex() != index)
						idx = m_Objects[idx].GetIndex();

					if (m_Objects[index].IsNil())
						m_Objects[idx].SetNil();
					else
						m_Objects[idx].SetIndex(m_Objects[index].GetIndex());
				}
				
				m_Objects[index].SetPointer(0);
			}

			if (!m_Objects[index].GetPointer()) {
				m_Objects[index].SetPointer(new ELEMENT());
			}

			return m_Objects[index].GetPointer();
		}

		size_t GetIndex(const ELEMENT * element) const {
			for (size_t index = 0; index < m_AllocatedObjects; ++index) {
				if (m_Objects[index].IsPointer() &&
					m_Objects[index].GetPointer() == element) {
					return index;
				}
			}

			return ~0;
		}

		bool IsObject(size_t index) {

			return index < m_AllocatedObjects && m_Objects[index].IsPointer();
		}

		ObjectRecord *		m_Objects;
		size_t				m_FreeObjects;
		size_t				m_AllocatedObjects;
		size_t				m_FreeListHead;
	};

	typedef ObjectArray<MultiTexture>	TextureArray;
	typedef ObjectArray<Buffer>			BufferArray;
}



#endif //ndef EGL_ARRAYS_H
