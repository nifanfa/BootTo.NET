#ifndef EGL_TYPES_H
#define EGL_TYPES_H 1

// ==========================================================================
//
// Types.h			Type declarations and helpers 
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


namespace EGL {

	template<class T> T Min(const T& a, const T& b) {
		return a < b ? a : b;
	}

	template<class T> T Max(const T& a, const T& b) {
		return a > b ? a : b;
	}

	struct Rect {

		Rect(GLint _x = 0, GLint _y = 0, GLsizei _width = 0, GLsizei _height = 0): 
			x(_x), y(_y), width(_width), height(_height) 
		{}

		static Rect Intersect(const Rect& first, const Rect& second) {

			GLint x = Max(first.x, second.x);
			GLint y = Max(first.y, second.y);

			GLint xtop = Min(first.x + first.width, second.x + second.width);
			GLint ytop = Min(first.y + first.height, second.y + second.height);

			return Rect(x, y, xtop - x, ytop - y);
		}

		bool Contains(const Rect& other) const {
			return 
				x <= other.x && 
				y <= other.y &&
				x + width >= other.x + other.width && 
				y + height >= other.y + other.height;
		}

		GLint x;
		GLint y;
		GLsizei width;
		GLsizei height;
	};

}


#endif //ndef EGL_TYPES_H
