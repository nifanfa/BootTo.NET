#ifndef EGL_MATRIX_STACK_H
#define EGL_MATRIX_STACK_H 1

// ==========================================================================
//
// MatrixStack.h	Matrix Stack Class for 3D Rendering Library
//
// --------------------------------------------------------------------------
//
// 08-08-2003	Hans-Martin Will	initial version
// 10-04-2003	Hans-Martin Will	rework against new matrix class
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
#include "fixed.h"
#include "linalg.h"


namespace EGL {

	class MatrixStack {

	public:

		MatrixStack(I32 maxStackElements = 2);
		~MatrixStack();

		bool PopMatrix(void);
		bool PushMatrix(void);

		void MultMatrix(const Matrix4x4 &matrix);

		void LoadIdentity(void);
		void LoadMatrix(const Matrix4x4 &matrix);

		inline Matrix4x4 & CurrentMatrix() {
			return m_Stack[m_StackPointer];
		}

		inline I32 GetStackSize() const {
			return m_StackSize;
		}

		inline I32 GetCurrentStackSize() const {
			return m_StackPointer + 1;
		}

	private:

		Matrix4x4	*m_Stack;
		I32			m_StackPointer;
		I32			m_StackSize;
	};
}

#endif //ndef EGL_MATRIX_STACK_H
