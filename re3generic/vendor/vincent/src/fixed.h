#ifndef EGL_FIXED_H
#define EGL_FIXED_H 1

// ==========================================================================
//
// fixed.h	Implementation of Fixed Point Arithmetic
//			If Intel Graphics Performance Primitives (GPP) are available,
//			the functions defined in this header should be mapped to
//			the assembler primitives provided in this library.
//
//			Fixed point numbers are represented in 16.16 format, where
//			the high 16 bits represent the signed integer part, while the
//			lower 16 bits represent the fractional part of a number.
//
// --------------------------------------------------------------------------
//
// 08-07-2003	Hans-Martin Will	initial version
// 10-02-2003	Hans-Martin Will	reworked to make this header file 
//									platform indepdendent
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
#include <math.h>
#include <assert.h>


// --------------------------------------------------------------------------
// Constants
// --------------------------------------------------------------------------


#define EGL_PRECISION 16					// number of fractional bits
#define EGL_ONE		  (1 << EGL_PRECISION)	// representation of 1
#define EGL_ZERO	  0						// representation of 0
#define EGL_HALF	  0x08000				// S15.16 0.5 
#define EGL_PINF	  0x7fffffff			// +inf 
#define EGL_MINF	  0x80000000			// -inf 

#define EGL_2PI			EGL_FixedFromFloat(6.28318530717958647692f)
#define EGL_R2PI		EGL_FixedFromFloat(1.0f/6.28318530717958647692f)

typedef I32 EGL_Fixed;

#define F EGL_FixedFromFloat


inline static I32 EGL_CLAMP(I32 value, I32 min, I32 max) {
	if (value < min) {
		return min;
	} else if (value > max) {
		return max;
	} else {
		return value;
	}
}

inline static U32 EGL_CLAMP(U32 value, U32 min, U32 max) {
	if (value < min) {
		return min;
	} else if (value > max) {
		return max;
	} else {
		return value;
	}
}


static inline I32 EGL_Abs(I32 value) {
	if (value < 0) {
		return -value;
	} else {
		return value;
	}
}


static inline I32 EGL_Min(I32 first, I32 second) {
	return first < second ? first : second;
}


static inline I32 EGL_Max(I32 first, I32 second) {
	return first > second ? first : second;
}


// --------------------------------------------------------------------------
// Convert integer value to fixed point number
//
// Parameters:
//	value		-	Integer value to be converted to fixed point number
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_FixedFromInt(I32 value) {
	return value << EGL_PRECISION;
}


// --------------------------------------------------------------------------
// Convert fixed point number to integer value
//
// Parameters:
//	value		-	Fixed point number to be converted to integer value
// --------------------------------------------------------------------------
inline I32 EGL_IntFromFixed(EGL_Fixed value) {
	return value >> EGL_PRECISION;
}



// --------------------------------------------------------------------------
// Retrieve fractional part from fixed point number 
//
// Parameters:
//	value		-	Fractional part from fixed point number 
// --------------------------------------------------------------------------
inline I32 EGL_FractionFromFixed(EGL_Fixed value) {
	return value & ((1 << EGL_PRECISION) - 1);
}



// --------------------------------------------------------------------------
// Convert fixed point number to integer value by round to nearest
//
// Parameters:
//	value		-	Fixed point number to be rounded to integer value
// --------------------------------------------------------------------------
inline I32 EGL_Round(EGL_Fixed value) {
	return (value + EGL_ONE/2) >> EGL_PRECISION;
}


// --------------------------------------------------------------------------
// Round fixed point number to nearest integer value by round to nearest
//
// Parameters:
//	value		-	Fixed point number to be rounded to integer value
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_NearestInt(EGL_Fixed value) {
	return (value + (EGL_ONE/2 - 1)) & 0xffff0000;
}


// --------------------------------------------------------------------------
// Convert floating point value to fixed point number
//
// Parameters:
//	value		-	Floatin point value to be converted to fixed point number
// --------------------------------------------------------------------------
inline I32 EGL_FixedFromFloat(float value) {
	if (value >= 32767.5f)
		return 0x7fffffff;
	else if (value <= -32768.0f)
		return 0x80000000;
	else
		return static_cast<EGL_Fixed>(value * static_cast<float>(EGL_ONE));
}
//#define EGL_FixedFromFloat(f)   static_cast<EGL_Fixed>((f) >= 32768.0 ? 0x7ffffff : (f) < -32768.0 ? 0x80000000 : (f)*static_cast<float>(EGL_ONE))

// --------------------------------------------------------------------------
// Convert fixed point number to floating point value
//
// Parameters:
//	value		-	Fixed point number to be converted to floating point 
//					value
// --------------------------------------------------------------------------
inline float EGL_FloatFromFixed(EGL_Fixed value) {
	return value * (1.0f/static_cast<float>(EGL_ONE));
}


// --------------------------------------------------------------------------
// Calculate product of two fixed point numbers
//
// Parameters:
//	a			-	first operand
//	b			-	second operand
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Mul(EGL_Fixed a, EGL_Fixed b) {
	return (EGL_Fixed) (((I64) a * (I64) b)  >> EGL_PRECISION);
}


#ifndef EGL_USE_GPP
// ==========================================================================
// Implementation if GPP is not available
// ==========================================================================


// --------------------------------------------------------------------------
// Calculate inverse of fixed point number
//
// Parameters:
//	value		-	the number whose inverse should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Inverse(EGL_Fixed value);


// --------------------------------------------------------------------------
// Perform division of two fixed point numbers
//
// Parameters:
//	a			-	dividend
//	b			-	divisor
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Div(EGL_Fixed a, EGL_Fixed b) {

	if ((b >> 24) && (b >> 24) + 1) {
		return EGL_Mul(a >> 8, EGL_Inverse(b >> 8));
	} else {
		return EGL_Mul(a, EGL_Inverse(b));
	}
}


// --------------------------------------------------------------------------
// Calculate square root of fixed point number
//
// Parameters:
//	value		-	the number whose square root should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Sqrt(EGL_Fixed value);


// --------------------------------------------------------------------------
// Calculate the inverse of the square root of fixed point number
//
// Parameters:
//	value		-	the numbers whose inverse of square root should be 
//					calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_InvSqrt(EGL_Fixed value);


// --------------------------------------------------------------------------
// Calculate square root of fixed point number; fast calculation using
// inverse and inverse square root
//
// Parameters:
//	value		-	the number whose square root should be calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_FastSqrt(EGL_Fixed value) {
	return value <= 0 ? 0 : EGL_Inverse(EGL_InvSqrt(value));
}


// --------------------------------------------------------------------------
// Calculate sine of fixed point number
//
// Parameters:
//	value		-	the numbers whose sine should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Sin(EGL_Fixed value);


// --------------------------------------------------------------------------
// Calculate cosine of fixed point number
//
// Parameters:
//	value		-	the numbers whose cosine should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Cos(EGL_Fixed value);


#else 
// ==========================================================================
// Implementation if GPP is available
// ==========================================================================


// --------------------------------------------------------------------------
// Perform division of two fixed point numbers
//
// Parameters:
//	a			-	dividend
//	b			-	divisor
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Div(EGL_Fixed a, EGL_Fixed b) {
	assert(b);
	assert(-b);
	I32 result;
	gppDiv_16_32s(a, b, &result);
	return result;
}


// --------------------------------------------------------------------------
// Calculate inverse of fixed point number
//
// Parameters:
//	value		-	the number whose inverse should be calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Inverse(EGL_Fixed value) {
	I32 result;
	assert(value);
	assert(-value);
	gppInv_16_32s(value, &result);
	return result;
}


// --------------------------------------------------------------------------
// Calculate square root of fixed point number
//
// Parameters:
//	value		-	the number whose square root should be calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_FastSqrt(EGL_Fixed value) {
	U32 result;
	gppSqrtLP_16_32s(static_cast<U32>(value), &result);
	return static_cast<I32>(result);
}


// --------------------------------------------------------------------------
// Calculate square root of fixed point number
//
// Parameters:
//	value		-	the number whose square root should be calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Sqrt(EGL_Fixed value) {
	U32 result;
	gppSqrtHP_16_32s(static_cast<U32>(value), &result);
	return static_cast<I32>(result);
}


// --------------------------------------------------------------------------
// Calculate the inverse of the square root of fixed point number
//
// Parameters:
//	value		-	the numbers whose inverse of square root should be 
//					calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_InvSqrt(EGL_Fixed value) {
	U32 result;
	gppInvSqrtLP_16_32s(static_cast<U32>(value), &result);
	return static_cast<I32>(result);
}


// --------------------------------------------------------------------------
// Calculate sine of fixed point number
//
// Parameters:
//	value		-	the numbers whose sine should be calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Sin(EGL_Fixed value) {
	I32 result;
	gppSinHP_16_32s(value, &result);
	return result;
}


// --------------------------------------------------------------------------
// Calculate cosine of fixed point number
//
// Parameters:
//	value		-	the numbers whose cosine should be calculated
// --------------------------------------------------------------------------
inline EGL_Fixed EGL_Cos(EGL_Fixed value) {
	I32 result;
	gppCosHP_16_32s(value, &result);
	return result;
}


#endif // ndef EGL_USE_GPP


// --------------------------------------------------------------------------
// Poor man's exponentiation, only exponents of 0, 1 and 2 are supported
//
// Parameters:
//	value		-	the basis; assume 0 <= value <= 1
//	exponent	-	the exponent, exponent >= 0
// --------------------------------------------------------------------------
OGLES_API EGL_Fixed EGL_Power(EGL_Fixed value, EGL_Fixed exponent);


// --------------------------------------------------------------------------
// map [0..1] to [0..1)
// --------------------------------------------------------------------------
inline static EGL_Fixed EGL_MAP_0_1(EGL_Fixed value) {
	return EGL_Mul(value, 0xffff);
}


// --------------------------------------------------------------------------
// clamp to [0..1], then map into range 0 .. 2^k-1
// --------------------------------------------------------------------------
inline static U32 EGL_MAP_BITFIELD(I32 value, unsigned bits) {

	I32 mask = (1 << bits) - 1;
	return (U32) EGL_Mul(EGL_CLAMP(value, 0, EGL_ONE), mask) & mask;
}




#endif //ndef EGL_FIXED_H
