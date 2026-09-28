// ==========================================================================
//
// fixed.cpp	Implementation of Fixed Point Arithmetic
//
//		If Intel Graphics Performance Primitives (GPP) are available,
//		the functions defined in this header should be mapped to
//		the assembler primitives provided in this library.
//
//		Fixed point numbers are represented in 16.16 format, where
//		the high 16 bits represent the signed integer part, while the
//		lower 16 bits represent the fractional part of a number.
//
// --------------------------------------------------------------------------
//
// Copyright (C) 2003  David Blythe   All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
// DAVID BLYTHE BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ==========================================================================


#include "stdafx.h"
#include "fixed.h"


#ifndef EGL_USE_GPP


// --------------------------------------------------------------------------
// Calculate inverse of fixed point number
//
// Parameters:
//	a		-	the number whose inverse should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Inverse(EGL_Fixed a) {
    I32 exp;
    EGL_Fixed x;
    /* 1/(4x) */
    static const I32 __gl_rcp_tab[] = { /* domain 0.5 .. 1.0-1/16 */
		0x8000, 0x71c7, 0x6666, 0x5d17, 0x5555, 0x4ec4, 0x4924, 0x4444
    };
    if (a == EGL_ZERO) return 0x7fffffff;
	bool sign = false;

	if (a < 0) {
		sign = true;
		a = -a;
	}

#ifdef EGL_USE_CLZ
	exp = _CountLeadingZeros(a);
#else
    x = a;
    exp = 31;
    if (x & 0xffff0000) { exp -= 16; x >>= 16; }
    if (x & 0xff00) { exp -= 8; x >>= 8; }
    if (x & 0xf0) { exp -= 4; x >>= 4; }
    if (x & 0xc) { exp -= 2; x >>= 2; }
    if (x & 0x2) { exp -= 1; }
#endif
    x = __gl_rcp_tab[(a>>(28-exp))&0x7]<<2;
    exp -= 16;
    if (exp <= 0)
		x >>= -exp;
    else
		x <<= exp;
//printf("est %f\n", __GL_F_2_FLOAT(x));
    /* two iterations of newton-raphson  x = x(2-ax) */
	x = EGL_Mul(x,(EGL_ONE*2 - EGL_Mul(a,x)));
	x = EGL_Mul(x,(EGL_ONE*2 - EGL_Mul(a,x)));
//printf("recip %f %f\n", __GL_F_2_FLOAT(a), __GL_F_2_FLOAT(x));

	if (sign)
		return -x;
	else
		return x;
}


// --------------------------------------------------------------------------
// Calculate the inverse of the square root of fixed point number
//
// Parameters:
//	a		-	the numbers whose inverse of square root should be 
//				calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_InvSqrt(EGL_Fixed a) {
    EGL_Fixed x /*= (a+EGL_ONE)>>1*//*a>>1*/;
    /* 1/(2sqrt(x)) - extra divide by 2 to scale to 16 bits */
    static const U16 __gl_rsq_tab[] = { /* domain 0.5 .. 1.0-1/16 */
		0xb504, 0xaaaa, 0xa1e8, 0x9a5f, 0x93cd, 0x8e00, 0x88d6, 0x8432,
    };
    I32 i, exp;
    if (a == EGL_ZERO) return 0x7fffffff;
    if (a == EGL_ONE) return a;

#ifdef EGL_USE_CLZ
	exp = _CountLeadingZeros(a);
#else
    x = a;
    exp = 31;
    if (x & 0xffff0000) { exp -= 16; x >>= 16; }
    if (x & 0xff00) { exp -= 8; x >>= 8; }
    if (x & 0xf0) { exp -= 4; x >>= 4; }
    if (x & 0xc) { exp -= 2; x >>= 2; }
    if (x & 0x2) { exp -= 1; }
#endif
    x = __gl_rsq_tab[(a>>(28-exp))&0x7]<<1;
//printf("t %f %x %f %d %d\n", __GL_F_2_FLOAT(a), a, __GL_F_2_FLOAT(x), exp, 28-exp);
    exp -= 16;
    if (exp <= 0)
		x >>= -exp>>1;
    else
		x <<= (exp>>1)+(exp&1);
    if (exp&1) x = EGL_Mul(x, __gl_rsq_tab[0]);
//printf("nx %f\n", __GL_F_2_FLOAT(x));

    /* newton-raphson */
    /* x = x/2*(3-(a*x)*x) */
    i = 0;
    do {
//printf("%f\n", __GL_F_2_FLOAT(x));
		x = EGL_Mul((x>>1),(EGL_ONE*3 - EGL_Mul(EGL_Mul(a,x),x)));
    } while(++i < 3);
//printf("rsqrt %f %f\n", __GL_F_2_FLOAT(a), __GL_F_2_FLOAT(x));
    return x;
}

static const U16 __gl_sin_tab[] = {
#include "gl_sin.h"
};

// --------------------------------------------------------------------------
// Calculate cosine of fixed point number
//
// Parameters:
//	a		-	the numbers whose cosine should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Cos(EGL_Fixed a) {
    EGL_Fixed v;
    /* reduce to [0,1) */
    while (a < EGL_ZERO) a += EGL_2PI;
    a *= EGL_R2PI;
    a >>= EGL_PRECISION;
    a += 0x4000;

    /* now in the range [0, 0xffff], reduce to [0, 0xfff] */
    a >>= 4;

    v = (a & 0x400) ? __gl_sin_tab[0x3ff - (a & 0x3ff)] : __gl_sin_tab[a & 0x3ff];
    v = EGL_Mul(v,EGL_ONE);
    return (a & 0x800) ? -v : v;
}

// --------------------------------------------------------------------------
// Calculate sine of fixed point number
//
// Parameters:
//	value		-	the numbers whose sine should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Sin(EGL_Fixed a) {
    EGL_Fixed v;

    /* reduce to [0,1) */
    while (a < EGL_ZERO) a += EGL_2PI;
    a *= EGL_R2PI;
    a >>= EGL_PRECISION;

    /* now in the range [0, 0xffff], reduce to [0, 0xfff] */
    a >>= 4;

    v = (a & 0x400) ? __gl_sin_tab[0x3ff - (a & 0x3ff)] : __gl_sin_tab[a & 0x3ff];
    v = EGL_Mul(v,EGL_ONE);
    return (a & 0x800) ? -v : v;
}

// --------------------------------------------------------------------------
// Calculate square root of fixed point number
//
// Parameters:
//	value		-	the number whose square root should be calculated
// --------------------------------------------------------------------------
EGL_Fixed EGL_Sqrt(EGL_Fixed a) {
    EGL_Fixed s;
    I32 i;
    s = (a + EGL_ONE) >> 1;
    /* 6 iterations to converge */
    for (i = 0; i < 6; i++)
		s = (s + EGL_Div(a, s)) >> 1;
    return s;
}

#endif //ndef EGL_USE_GPP

/* assume 0 <= x <= 1 and y >= 0 */
static __inline EGL_Fixed
xpow(EGL_Fixed x, EGL_Fixed y) {
    I32 exp;
    EGL_Fixed p, f = x;
//#define __POW_LOW_PREC
    /* absolute error < 0.009, more than good enough for 5-bit color */
    static const U16 __gl_log_tab[] = { /* domain .5 - 1.0 */
		0xffff, 0xd47f, 0xad96, 0x8a62, 0x6a3f, 0x4caf, 0x3151, 0x17d6, 0x0000
    };
    static const U16 __gl_alog_tab[] = { /* domain 0 - 1.0 */
		0xffff, 0xeac0, 0xd744, 0xc567, 0xb504, 0xa5fe, 0x9837, 0x8b95, 0x8000
    };

    if (y == EGL_ZERO || x == EGL_ONE)
		return EGL_ONE;
    if (x == EGL_ZERO)
		return EGL_ZERO;

    exp = 15;
    if (f & 0xff00) { exp -= 8; f >>= 8; }
    if (f & 0xf0) { exp -= 4; f >>= 4; }
    if (f & 0xc) { exp -= 2; f >>= 2; }
    if (f & 0x2) { exp -= 1; }
    f = x << exp;
//printf("f/x/exp %f %f %d %x\n", __GL_F_2_FLOAT(f), __GL_F_2_FLOAT(x), exp, x);
    /* compute log base 2 */
    x = (f&0x0fff)<<4;
    f = (f >> 12)&0x7;
    p = EGL_Mul(__gl_log_tab[f+1]-__gl_log_tab[f],x) + __gl_log_tab[f];
//printf("p %f x %f\n", __GL_F_2_FLOAT(p), __GL_F_2_FLOAT(x));
    p = EGL_Mul(p, y) + y*exp;
//printf("np %f \n", __GL_F_2_FLOAT(p));

    /* compute antilog (2^p) */
    exp = EGL_IntFromFixed(p);
    p = EGL_FractionFromFixed(p);
//printf("exp = %d frac %f\n", exp, __GL_F_2_FLOAT(p));
    x = (p&0x1fff)<<3;
    p = p >> 13;
    return (EGL_Mul(__gl_alog_tab[p+1]-__gl_alog_tab[p],x)+
		__gl_alog_tab[p])>>exp;
}

// --------------------------------------------------------------------------
// Exponentiation function
//
// Parameters:
//	value		-	the basis; assume 0 <= value <= 1
//	exponent	-	the exponent, exponent >= 0
// --------------------------------------------------------------------------
OGLES_API EGL_Fixed EGL_Power(EGL_Fixed a, EGL_Fixed n) {
#if (defined(ARM) || defined(_ARM_))
    return xpow(a, n);
#else
	return EGL_FixedFromFloat(pow(EGL_FloatFromFixed(a), EGL_FloatFromFixed(n)));
#endif
}

