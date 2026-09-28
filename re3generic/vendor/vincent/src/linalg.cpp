// ==========================================================================
//
// linalg.cpp	Implementation of Linear Algebra using Fixed Point Arithmetic
//
// --------------------------------------------------------------------------
//
// 10-03-2003	Hans-Martin Will	initial version
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
#include "linalg.h"
#include <math.h>


#ifndef M_PI
#	define M_PI       3.14159265358979323846
#endif

using namespace EGL;


// ==========================================================================
// class Matrix4x4
// ==========================================================================


namespace {
	inline EGL_Fixed Det2X2(EGL_Fixed a, EGL_Fixed b, EGL_Fixed c, EGL_Fixed d) {
		return EGL_Mul(a,d) - EGL_Mul(c,b);
	}
}

Matrix4x4 Matrix4x4 :: InverseUpper3(bool rescale) const {

	Matrix4x4 result;

    int i = 0;
    EGL_Fixed d, r;
    /* compute 3x3 inverse using Cramer's rule: A^-1 = adj(A)/det(A) */
    /* cofactors */
    result.Element(0,0) =  Det2X2(Element(1,1), Element(1,2), Element(2,1), Element(2,2));
    result.Element(0,1) = -Det2X2(Element(1,0), Element(1,2), Element(2,0), Element(2,2));
    result.Element(0,2) =  Det2X2(Element(1,0), Element(1,1), Element(2,0), Element(2,1));

    result.Element(1,0) = -Det2X2(Element(0,1), Element(0,2), Element(2,1), Element(2,2));
    result.Element(1,1) =  Det2X2(Element(0,0), Element(0,2), Element(2,0), Element(2,2));
    result.Element(1,2) = -Det2X2(Element(0,0), Element(0,1), Element(2,0), Element(2,1));

    result.Element(2,0) =  Det2X2(Element(0,1), Element(0,2), Element(1,1), Element(1,2));
    result.Element(2,1) = -Det2X2(Element(0,0), Element(0,2), Element(1,0), Element(1,2));
    result.Element(2,2) =  Det2X2(Element(0,0), Element(0,1), Element(1,0), Element(1,1));

    /* determinant */
    d = 0;

    for(i = 0; i < 3; i++)
		d += EGL_Mul(Element(0,i),result.Element(0,i));

	if (d == 0) {
		// singluar matrix
		return result;
	}

    r = EGL_Inverse(d);

    i = 0;
    do {
		int j = 0;

		do {
			result.Element(i,j) = EGL_Mul(result.Element(i,j), r);
		} while(++j < 3);
    } while(++i < 3);

	result.m_identity = false;

	if (rescale) {
		EGL_Fixed sumOfSquares = 
			EGL_Mul(result.Element(2, 0), result.Element(2, 0)) +
			EGL_Mul(result.Element(2, 1), result.Element(2, 1)) +
			EGL_Mul(result.Element(2, 2), result.Element(2, 2));

		if (sumOfSquares != EGL_ONE) {
			EGL_Fixed factor = EGL_InvSqrt(sumOfSquares);

			for (size_t index = 0; index < 16; ++index) {
				result.Element(index) = EGL_Mul(result.Element(index), factor);
			}
		}
	}

	return result;
}


#define SWAP_ROWS(a, b) { EGL_Fixed *_tmp = a; (a)=(b); (b)=_tmp; }


Matrix4x4 Matrix4x4 :: Inverse() const {
   Matrix4x4 out;
   EGL_Fixed wtmp[4][8];
   EGL_Fixed m0, m1, m2, m3, s;
   EGL_Fixed *r0, *r1, *r2, *r3;

   r0 = wtmp[0], r1 = wtmp[1], r2 = wtmp[2], r3 = wtmp[3];

   r0[0] = Element(0,0), r0[1] = Element(0,1),
   r0[2] = Element(0,2), r0[3] = Element(0,3),
   r0[4] = EGL_ONE, r0[5] = r0[6] = r0[7] = 0,

   r1[0] = Element(1,0), r1[1] = Element(1,1),
   r1[2] = Element(1,2), r1[3] = Element(1,3),
   r1[5] = EGL_ONE, r1[4] = r1[6] = r1[7] = 0,

   r2[0] = Element(2,0), r2[1] = Element(2,1),
   r2[2] = Element(2,2), r2[3] = Element(2,3),
   r2[6] = EGL_ONE, r2[4] = r2[5] = r2[7] = 0,

   r3[0] = Element(3,0), r3[1] = Element(3,1),
   r3[2] = Element(3,2), r3[3] = Element(3,3),
   r3[7] = EGL_ONE, r3[4] = r3[5] = r3[6] = 0;

   /* choose pivot - or die */
   if (EGL_Abs(r3[0])>EGL_Abs(r2[0])) SWAP_ROWS(r3, r2);
   if (EGL_Abs(r2[0])>EGL_Abs(r1[0])) SWAP_ROWS(r2, r1);
   if (EGL_Abs(r1[0])>EGL_Abs(r0[0])) SWAP_ROWS(r1, r0);
   if (0 == r0[0])  return out;

   /* eliminate first variable     */
   m1 = EGL_Div(r1[0], r0[0]); m2 = EGL_Div(r2[0], r0[0]); m3 = EGL_Div(r3[0], r0[0]);
   s = r0[1]; r1[1] -= EGL_Mul(m1, s); r2[1] -= EGL_Mul(m2, s); r3[1] -= EGL_Mul(m3, s);
   s = r0[2]; r1[2] -= EGL_Mul(m1, s); r2[2] -= EGL_Mul(m2, s); r3[2] -= EGL_Mul(m3, s);
   s = r0[3]; r1[3] -= EGL_Mul(m1, s); r2[3] -= EGL_Mul(m2, s); r3[3] -= EGL_Mul(m3, s);
   s = r0[4];
   if (s != 0) { r1[4] -= EGL_Mul(m1, s); r2[4] -= EGL_Mul(m2, s); r3[4] -= EGL_Mul(m3, s); }
   s = r0[5];
   if (s != 0) { r1[5] -= EGL_Mul(m1, s); r2[5] -= EGL_Mul(m2, s); r3[5] -= EGL_Mul(m3, s); }
   s = r0[6];
   if (s != 0) { r1[6] -= EGL_Mul(m1, s); r2[6] -= EGL_Mul(m2, s); r3[6] -= EGL_Mul(m3, s); }
   s = r0[7];
   if (s != 0) { r1[7] -= EGL_Mul(m1, s); r2[7] -= EGL_Mul(m2, s); r3[7] -= EGL_Mul(m3, s); }

   /* choose pivot - or die */
   if (EGL_Abs(r3[1])>EGL_Abs(r2[1])) SWAP_ROWS(r3, r2);
   if (EGL_Abs(r2[1])>EGL_Abs(r1[1])) SWAP_ROWS(r2, r1);
   if (0 == r1[1])  return out;

   /* eliminate second variable */
   m2 = EGL_Div(r2[1], r1[1]); m3 = EGL_Div(r3[1], r1[1]);
   r2[2] -= EGL_Mul(m2, r1[2]); r3[2] -= EGL_Mul(m3, r1[2]);
   r2[3] -= EGL_Mul(m2, r1[3]); r3[3] -= EGL_Mul(m3, r1[3]);
   s = r1[4]; if (0 != s) { r2[4] -= EGL_Mul(m2, s); r3[4] -= EGL_Mul(m3, s); }
   s = r1[5]; if (0 != s) { r2[5] -= EGL_Mul(m2, s); r3[5] -= EGL_Mul(m3, s); }
   s = r1[6]; if (0 != s) { r2[6] -= EGL_Mul(m2, s); r3[6] -= EGL_Mul(m3, s); }
   s = r1[7]; if (0 != s) { r2[7] -= EGL_Mul(m2, s); r3[7] -= EGL_Mul(m3, s); }

   /* choose pivot - or die */
   if (EGL_Abs(r3[2])>EGL_Abs(r2[2])) SWAP_ROWS(r3, r2);
   if (0 == r2[2])  return out;

   /* eliminate third variable */
   m3 = EGL_Div(r3[2], r2[2]);
   r3[3] -= EGL_Mul(m3, r2[3]), r3[4] -= EGL_Mul(m3, r2[4]),
   r3[5] -= EGL_Mul(m3, r2[5]), r3[6] -= EGL_Mul(m3, r2[6]),
   r3[7] -= EGL_Mul(m3, r2[7]);

   /* last check */
   if (0 == r3[3]) return out;

   s = EGL_Inverse(r3[3]);             /* now back substitute row 3 */
   r3[4] = EGL_Mul(r3[4], s); r3[5] = EGL_Mul(r3[5], s); r3[6] = EGL_Mul(r3[6], s); r3[7] = EGL_Mul(r3[7], s);

   m2 = r2[3];                 /* now back substitute row 2 */
   s  = EGL_Inverse(r2[2]);
   r2[4] = EGL_Mul(s, (r2[4] - EGL_Mul(r3[4], m2))), r2[5] = EGL_Mul(s, (r2[5] - EGL_Mul(r3[5], m2))),
   r2[6] = EGL_Mul(s, (r2[6] - EGL_Mul(r3[6], m2))), r2[7] = EGL_Mul(s, (r2[7] - EGL_Mul(r3[7], m2)));
   m1 = r1[3];
   r1[4] -= EGL_Mul(r3[4], m1), r1[5] -= EGL_Mul(r3[5], m1),
   r1[6] -= EGL_Mul(r3[6], m1), r1[7] -= EGL_Mul(r3[7], m1);
   m0 = r0[3];
   r0[4] -= EGL_Mul(r3[4], m0), r0[5] -= EGL_Mul(r3[5], m0),
   r0[6] -= EGL_Mul(r3[6], m0), r0[7] -= EGL_Mul(r3[7], m0);

   m1 = r1[2];                 /* now back substitute row 1 */
   s  = EGL_Inverse(r1[1]);
   r1[4] = EGL_Mul(s, (r1[4] - EGL_Mul(r2[4], m1))), r1[5] = EGL_Mul(s, (r1[5] - EGL_Mul(r2[5], m1))),
   r1[6] = EGL_Mul(s, (r1[6] - EGL_Mul(r2[6], m1))), r1[7] = EGL_Mul(s, (r1[7] - EGL_Mul(r2[7], m1)));
   m0 = r0[2];
   r0[4] -= EGL_Mul(r2[4], m0), r0[5] -= EGL_Mul(r2[5], m0),
   r0[6] -= EGL_Mul(r2[6], m0), r0[7] -= EGL_Mul(r2[7], m0);

   m0 = r0[1];                 /* now back substitute row 0 */
   s  = EGL_Inverse(r0[0]);
   r0[4] = EGL_Mul(s, (r0[4] - EGL_Mul(r1[4], m0))), r0[5] = EGL_Mul(s, (r0[5] - EGL_Mul(r1[5], m0))),
   r0[6] = EGL_Mul(s, (r0[6] - EGL_Mul(r1[6], m0))), r0[7] = EGL_Mul(s, (r0[7] - EGL_Mul(r1[7], m0)));

   out.Element(0,0) = r0[4]; out.Element(0,1) = r0[5],
   out.Element(0,2) = r0[6]; out.Element(0,3) = r0[7],
   out.Element(1,0) = r1[4]; out.Element(1,1) = r1[5],
   out.Element(1,2) = r1[6]; out.Element(1,3) = r1[7],
   out.Element(2,0) = r2[4]; out.Element(2,1) = r2[5],
   out.Element(2,2) = r2[6]; out.Element(2,3) = r2[7],
   out.Element(3,0) = r3[4]; out.Element(3,1) = r3[5],
   out.Element(3,2) = r3[6]; out.Element(3,3) = r3[7];

   return out;
}


Matrix4x4 Matrix4x4 :: CreateScale(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z) {
	Matrix4x4 result;

	result.Element(0, 0) = x;
	result.Element(1, 1) = y;
	result.Element(2, 2) = z;

	result.m_identity = false;
	return result;
}


Matrix4x4 Matrix4x4 :: CreateRotate(EGL_Fixed angle, EGL_Fixed x, 
									EGL_Fixed y, EGL_Fixed z) {

	Matrix4x4 matrix;
	Vec3D axis(x, y, z);
	axis.Normalize();
	EGL_Fixed r_x = axis.x();
	EGL_Fixed r_y = axis.y();
	EGL_Fixed r_z = axis.z();

	angle = EGL_Mul(angle, EGL_FixedFromFloat(static_cast<float>(M_PI) / 180.0f));

	EGL_Fixed sine = EGL_Sin(angle);
	EGL_Fixed cosine = EGL_Cos(angle);
	
	EGL_Fixed one_minus_cosine = EGL_ONE - cosine;

	matrix.Element(0, 0) = cosine + EGL_Mul(one_minus_cosine, EGL_Mul(r_x, r_x));
	matrix.Element(0, 1) = EGL_Mul(one_minus_cosine, EGL_Mul(r_x, r_y)) - EGL_Mul(r_z, sine);
	matrix.Element(0, 2) = EGL_Mul(EGL_Mul(one_minus_cosine, r_x), r_z) + EGL_Mul(r_y, sine);

	matrix.Element(1, 0) = EGL_Mul(EGL_Mul(one_minus_cosine, r_x),  r_y) + EGL_Mul(r_z, sine);
	matrix.Element(1, 1) = cosine + EGL_Mul(EGL_Mul(one_minus_cosine, r_y), r_y);
	matrix.Element(1, 2) = EGL_Mul(EGL_Mul(one_minus_cosine, r_y), r_z) - EGL_Mul(r_x, sine);

	matrix.Element(2, 0) = EGL_Mul(EGL_Mul(one_minus_cosine, r_x), r_z) - EGL_Mul(r_y, sine);
	matrix.Element(2, 1) = EGL_Mul(EGL_Mul(one_minus_cosine, r_y), r_z) + EGL_Mul(r_x, sine);
	matrix.Element(2, 2) = cosine + EGL_Mul(EGL_Mul(one_minus_cosine, r_z), r_z);

	matrix.m_identity = false;
	return matrix;
}


Matrix4x4 Matrix4x4 :: CreateTranslate(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z) {
	Matrix4x4 result;

	result.Element(0, 3) = x;
	result.Element(1, 3) = y;
	result.Element(2, 3) = z;

	result.m_identity = false;
	return result;
}


Matrix4x4 Matrix4x4 :: CreateFrustrum(EGL_Fixed l, EGL_Fixed r, 
									  EGL_Fixed b, EGL_Fixed t, EGL_Fixed n, EGL_Fixed f) {
	Matrix4x4 matrix;

	EGL_Fixed inv_width = (r - l) ? EGL_Inverse(r - l) : 0;
	EGL_Fixed inv_height = (t - b) ? EGL_Inverse(t - b) : 0;
	EGL_Fixed inv_depth = (f - n) ? EGL_Inverse(f - n) : 0;

	EGL_Fixed two_n = n * 2;

	matrix.Element(0, 0) = EGL_Mul(two_n, inv_width);
	matrix.Element(0, 2) = EGL_Mul(r + l, inv_width);

	matrix.Element(1, 1) = EGL_Mul(two_n, inv_height);
	matrix.Element(1, 2) = EGL_Mul(t + b, inv_height);

	matrix.Element(2, 2) = EGL_Mul(-f - n, inv_depth);
	matrix.Element(2, 3) = EGL_Mul(EGL_Mul(-two_n, inv_depth), f);

	matrix.Element(3, 2) = -EGL_ONE;
	matrix.Element(3, 3) = 0;	

	matrix.m_identity = false;
#ifdef EGL_USE_TOP_DOWN_SURFACE
    matrix = CreateScale(EGL_ONE,-EGL_ONE,EGL_ONE)*matrix;
#endif
	return matrix;
}

Matrix4x4 Matrix4x4 :: CreateOrtho(EGL_Fixed l, EGL_Fixed r, 
								   EGL_Fixed b, EGL_Fixed t, EGL_Fixed n, EGL_Fixed f) {
	Matrix4x4 matrix;

	EGL_Fixed inv_width = (r - l) ? EGL_Inverse(r - l) : 0;
	EGL_Fixed inv_height = (t - b) ? EGL_Inverse(t - b) : 0;
	EGL_Fixed inv_depth = (f - n) ? EGL_Inverse(f - n) : 0;

	EGL_Fixed two_n = n * 2;

	matrix.Element(0, 0) = 2 * inv_width;
	matrix.Element(0, 3) = -EGL_Mul(r + l, inv_width);

	matrix.Element(1, 1) = 2 * inv_height;
	matrix.Element(1, 3) = -EGL_Mul(t + b, inv_height);

	matrix.Element(2, 2) = -2 * inv_depth;
	matrix.Element(2, 3) = EGL_Mul(-f - n, inv_depth);

	matrix.m_identity = false;
	return matrix;
}
