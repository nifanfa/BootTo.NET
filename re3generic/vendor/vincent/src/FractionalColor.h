#ifndef EGL_FRACTIONAL_COLOR_H
#define EGL_FRACTIONAL_COLOR_H 1

// ==========================================================================
//
// FractionalColor	Fixed point representation of an RGBA color
//
// --------------------------------------------------------------------------
//
// 10-05-2003		Hans-Martin Will	initial version
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
#include "Color.h"


namespace EGL {
	class FractionalColor {

	public:
		EGL_Fixed	r, g, b, a;			// rgba components

	public:
		FractionalColor() {
			r = g = b = a = 0;
		}

		FractionalColor(const Color & color) {
			r = (color.R() << 8 | color.R()) + (color.R() >> 7);
			g = (color.G() << 8 | color.G()) + (color.G() >> 7);
			b = (color.B() << 8 | color.B()) + (color.B() >> 7);
			a = (color.A() << 8 | color.A()) + (color.A() >> 7);
		}

		FractionalColor(EGL_Fixed R, EGL_Fixed G, EGL_Fixed B, EGL_Fixed A) {
			r = R;
			g = G;
			b = B;
			a = A;
		}

		FractionalColor(const EGL_Fixed * rgba) {
			r = rgba[0];
			g = rgba[1];
			b = rgba[2];
			a = rgba[3];
		}

		static FractionalColor Clamp(const EGL_Fixed * rgba) {
			return FractionalColor(
				EGL_CLAMP(rgba[0], 0, EGL_ONE),
				EGL_CLAMP(rgba[1], 0, EGL_ONE),
				EGL_CLAMP(rgba[2], 0, EGL_ONE),
				EGL_CLAMP(rgba[3], 0, EGL_ONE));
		}

		FractionalColor(const FractionalColor& other) {
			r = other.r;
			b = other.b;
			g = other.g;
			a = other.a;
		}

		FractionalColor& operator=(const FractionalColor& other) {
			r = other.r;
			b = other.b;
			g = other.g;
			a = other.a;
			return *this;
		}

		// TODO; Decide: Is Fatcional color 8.16 or (1).16 format
		inline U16 ConvertTo565() const {
			return 
				 EGL_IntFromFixed(b * 0xFF) >> 3 | 
				(EGL_IntFromFixed(g * 0xFF) & 0xFC) << 3 | 
				(EGL_IntFromFixed(r * 0xFF) & 0xF8) << 8;
		}

		inline U16 ConvertTo5551() const {
			return 
				 EGL_IntFromFixed(b * 0xFF) >> 3 | 
				(EGL_IntFromFixed(g * 0xFF) & 0xF8) << 2 | 
				(EGL_IntFromFixed(r * 0xFF) & 0xF8) << 7 | 
				(EGL_IntFromFixed(a * 0xFF) & 0x80) << 8;
		}

		// convert fixed point to byte format
		inline operator Color() const {
			return Color(
				EGL_IntFromFixed((0x1FF * EGL_CLAMP(r, 0, EGL_ONE)) >> 1),
				EGL_IntFromFixed((0x1FF * EGL_CLAMP(g, 0, EGL_ONE)) >> 1),
				EGL_IntFromFixed((0x1FF * EGL_CLAMP(b, 0, EGL_ONE)) >> 1),
				EGL_IntFromFixed((0x1FF * EGL_CLAMP(a, 0, EGL_ONE)) >> 1));
		}

		inline void Clamp() {
			r = EGL_CLAMP(r, 0, EGL_ONE);
			g = EGL_CLAMP(g, 0, EGL_ONE);
			b = EGL_CLAMP(b, 0, EGL_ONE);
			a = EGL_CLAMP(a, 0, EGL_ONE);
		}

		inline FractionalColor operator*(const FractionalColor& other) const {
			return FractionalColor(EGL_Mul(r, other.r),
				EGL_Mul(g, other.g), EGL_Mul(b, other.b), a);
		}


		inline FractionalColor& operator*=(const FractionalColor& other) {
			r = EGL_Mul(r, other.r);
			g = EGL_Mul(g, other.g);
			b = EGL_Mul(b, other.b);
			return *this;
		}

		inline FractionalColor operator*(EGL_Fixed scale) const {
			return FractionalColor(EGL_Mul(r, scale),
				EGL_Mul(g, scale), EGL_Mul(b, scale), EGL_Mul(a, scale));
		}


		inline FractionalColor& operator*=(EGL_Fixed scale) {
			r = EGL_Mul(r, scale);
			g = EGL_Mul(g, scale);
			b = EGL_Mul(b, scale);
			a = EGL_Mul(a, scale);
			return *this;
		}

		inline FractionalColor operator-(const FractionalColor& other) const {
			return FractionalColor(r - other.r, g - other.g, b - other.b, a - other.a);
		}

		inline FractionalColor operator+(const FractionalColor& other) const {
			return FractionalColor(r + other.r, g + other.g, b + other.b, a + other.a);
		}

		inline void Accumulate(const FractionalColor& color,
							EGL_Fixed scale) {
			r += EGL_Mul(color.r, scale);
			g += EGL_Mul(color.g, scale);
			b += EGL_Mul(color.b, scale);
		}


		inline FractionalColor& operator+=(const FractionalColor& color) {
			r += color.r;
			g += color.g;
			b += color.b;
			//a += color.a;

			return *this;
		}
	};
}


#endif //ndef EGL_FRACTIONAL_COLOR_H
