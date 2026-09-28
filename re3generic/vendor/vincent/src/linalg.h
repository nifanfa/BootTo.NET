#ifndef EGL_LINALG_H
#define EGL_LINALG_H 1

// ==========================================================================
//
// linalg.h	Implementation of Linear Algebra using Fixed Point Arithmetic
//
// --------------------------------------------------------------------------
//
// 10-02-2003	Hans-Martin Will	initial version
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

namespace EGL {

	// --------------------------------------------------------------------------
	// 3-D Vector class
	// --------------------------------------------------------------------------


	class Vec3D {

		// element names compatible to GPP_VEC3D
		EGL_Fixed 	m_x, m_y, m_z;

	public:
		// ----------------------------------------------------------------------
		// Constructor
		// ----------------------------------------------------------------------
		inline Vec3D() {
			m_x = m_y = m_z = 0;
		}

		// ----------------------------------------------------------------------
		// Constructor
		//
		// Parameters:
		//	x, y, z			-	individual coordinates of 3-D vector
		// ----------------------------------------------------------------------
		inline Vec3D(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z) {
			m_x = x;
			m_y = y;
			m_z = z;
		}

		// ----------------------------------------------------------------------
		// Constructor
		//
		// Parameters:
		//	coords			-	individual coordinates of 3-D vector
		// ----------------------------------------------------------------------
		inline Vec3D(const EGL_Fixed * coords) {
			m_x = coords[0];
			m_y = coords[1];
			m_z = coords[2];
		}

		// ----------------------------------------------------------------------
		// Copy constructor
		// ----------------------------------------------------------------------
		inline Vec3D(const Vec3D& other) {
			m_x = other.m_x;
			m_y = other.m_y;
			m_z = other.m_z;
		}

		// ----------------------------------------------------------------------
		// Assignment operator
		// ----------------------------------------------------------------------
		inline Vec3D& operator=(const Vec3D& other) {
			m_x = other.m_x;
			m_y = other.m_y;
			m_z = other.m_z;
			return *this;
		}

		// ----------------------------------------------------------------------
		// Vector addition
		// ----------------------------------------------------------------------
		inline Vec3D operator+(const Vec3D& other) const {
			return Vec3D(m_x + other.m_x, m_y + other.m_y, m_z + other.m_z);
		}

		inline Vec3D& operator +=(const Vec3D& other) {
			m_x += other.m_x;
			m_y += other.m_y;
			m_z += other.m_z;
			return *this;
		}

		inline Vec3D operator-(const Vec3D& other) const {
			return Vec3D(m_x - other.m_x, m_y - other.m_y, m_z - other.m_z);
		}

		inline Vec3D& operator -=(const Vec3D& other) {
			m_x -= other.m_x;
			m_y -= other.m_y;
			m_z -= other.m_z;
			return *this;
		}

		// ----------------------------------------------------------------------
		// Scaling of vector
		//
		// Parameters:
		//	factor		-	scale factor
		// ----------------------------------------------------------------------
		inline Vec3D operator*(EGL_Fixed factor) const {
			return Vec3D(EGL_Mul(m_x, factor), 
							EGL_Mul(m_y, factor),
							EGL_Mul(m_z, factor));
		}

		inline Vec3D& operator *=(EGL_Fixed factor) {
			m_x = EGL_Mul(m_x, factor);
			m_y = EGL_Mul(m_y, factor);
			m_z = EGL_Mul(m_z, factor);
			return *this;
		}

		// ----------------------------------------------------------------------
		// Dot product between two vectors
		//
		// Parameters:
		//	other		-	second operand
		// ----------------------------------------------------------------------
		inline EGL_Fixed operator*(const Vec3D& other) const {
			return EGL_Mul(m_x, other.m_x) +
				EGL_Mul(m_y, other.m_y) +
				EGL_Mul(m_z, other.m_z);
		}

		// ----------------------------------------------------------------------
		// Cross product for two vectors
		//
		// Parameters:
		//	other		-	second operand
		// ----------------------------------------------------------------------
		inline Vec3D Cross(const Vec3D& other) const {
			return Vec3D(EGL_Mul(m_y, other.m_z) - EGL_Mul(m_z, other.m_y),
				-EGL_Mul(m_x, other.m_z) + EGL_Mul(m_z, other.m_x),
				EGL_Mul(m_x, other.m_y) - EGL_Mul(m_y, other.m_x));
		}

		// ----------------------------------------------------------------------
		// Euclidean length of vector
		// ----------------------------------------------------------------------
		inline EGL_Fixed Length() const {
			return EGL_Sqrt(LengthSq());
		}

		// ----------------------------------------------------------------------
		// Square of Euclidean length of vector
		// ----------------------------------------------------------------------
		inline EGL_Fixed LengthSq() const {
			return (*this) * (*this);
		}

		// ----------------------------------------------------------------------
		// Normalize the vector to unit length
		// ----------------------------------------------------------------------
		inline void Normalize() {
			*this *= EGL_InvSqrt(LengthSq());
		}

		// ----------------------------------------------------------------------
		// Element accessors
		// ----------------------------------------------------------------------

		inline EGL_Fixed x() const {
			return m_x;
		}

		inline EGL_Fixed y() const {
			return m_y;
		}

		inline EGL_Fixed z() const {
			return m_z;
		}

		inline void setX(EGL_Fixed value) {
			m_x = value;
		}

		inline void setY(EGL_Fixed value) {
			m_y = value;
		}

		inline void setZ(EGL_Fixed value) {
			m_z = value;
		}
	};


	inline OGLES_API Vec3D operator*(EGL_Fixed factor, const Vec3D& vector) {
		return vector * factor;
	}



	// --------------------------------------------------------------------------
	// 4-D Vector class
	// --------------------------------------------------------------------------


	class Vec4D {

		// element names compatible to GPP_VEC4D
		EGL_Fixed 	m_x, m_y, m_z, m_w;

	public:
		// ----------------------------------------------------------------------
		// Constructor
		// ----------------------------------------------------------------------
		inline Vec4D() {
			m_x = m_y = m_z = 0;
			m_w = 1;
		}

		// ----------------------------------------------------------------------
		// Constructor, canonical embedding of R^3 in SO^2
		//
		// Parameters:
		//	x, y, z			-	individual coordinates of 3-D vector
		// ----------------------------------------------------------------------
		inline Vec4D(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z) {
			m_x = x;
			m_y = y;
			m_z = z;
			m_w = EGL_ONE;
		}

		// ----------------------------------------------------------------------
		// Constructor
		//
		// Parameters:
		//	coords			-	individual coordinates of 3-D vector
		// ----------------------------------------------------------------------
		inline Vec4D(const EGL_Fixed * coords) {
			m_x = coords[0];
			m_y = coords[1];
			m_z = coords[2];
			m_w = coords[3];
		}

		// ----------------------------------------------------------------------
		// Constructor
		//
		// Parameters:
		//	x, y, z, w			-	individual coordinates of 4-D vector
		// ----------------------------------------------------------------------
		inline Vec4D(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z, EGL_Fixed w) {
			m_x = x;
			m_y = y;
			m_z = z;
			m_w = w;
		}

		// ----------------------------------------------------------------------
		// Copy constructor
		// ----------------------------------------------------------------------
		inline Vec4D(const Vec4D& other) {
			m_x = other.m_x;
			m_y = other.m_y;
			m_z = other.m_z;
			m_w = other.m_w;
		}


		// ----------------------------------------------------------------------
		// Assignment operator
		// ----------------------------------------------------------------------
		inline Vec4D& operator=(const Vec4D& other) {
			m_x = other.m_x;
			m_y = other.m_y;
			m_z = other.m_z;
			m_w = other.m_w;
			return *this;
		}

		// ----------------------------------------------------------------------
		// Projection operator
		// ----------------------------------------------------------------------
		inline operator Vec3D() const {
			EGL_Fixed factor = EGL_Inverse(m_w);

			return Vec3D(EGL_Mul(m_x, factor), EGL_Mul(m_y, factor), EGL_Mul(m_z, factor));
		}

		// ----------------------------------------------------------------------
		// Projection operator
		// ----------------------------------------------------------------------
		inline Vec3D Project() const {
			return Vec3D(m_x, m_y, m_z);
		}

		// ----------------------------------------------------------------------
		// Vector addition
		// ----------------------------------------------------------------------
		inline Vec4D operator+(const Vec4D& other) const {
			return Vec4D(m_x + other.m_x, m_y + other.m_y, m_z + other.m_z, m_w + other.m_w);
		}

		inline Vec4D& operator +=(const Vec4D& other) {
			m_x += other.m_x;
			m_y += other.m_y;
			m_z += other.m_z;
			m_w += other.m_w;
			return *this;
		}

		inline Vec4D operator-(const Vec4D& other) const {
			return Vec4D(m_x - other.m_x, m_y - other.m_y, m_z - other.m_z, m_w - other.m_w);
		}

		inline Vec4D& operator -=(const Vec4D& other) {
			m_x -= other.m_x;
			m_y -= other.m_y;
			m_z -= other.m_z;
			m_w -= other.m_w;
			return *this;
		}

		inline Vec4D operator-() const {
			return Vec4D(-m_x, -m_y, -m_z, -m_w);
		}

		// ----------------------------------------------------------------------
		// Scaling of vector
		//
		// Parameters:
		//	factor		-	scale factor
		// ----------------------------------------------------------------------
		inline Vec4D operator*(EGL_Fixed factor) const {
			return Vec4D(EGL_Mul(m_x, factor), 
							EGL_Mul(m_y, factor),
							EGL_Mul(m_z, factor),
							EGL_Mul(m_w, factor));
		}

		inline Vec4D& operator *=(EGL_Fixed factor) {
			m_x = EGL_Mul(m_x, factor);
			m_y = EGL_Mul(m_y, factor);
			m_z = EGL_Mul(m_z, factor);
			m_w = EGL_Mul(m_w, factor);

			return *this;
		}

		// ----------------------------------------------------------------------
		// Dot product between two vectors
		//
		// Parameters:
		//	other		-	second operand
		// ----------------------------------------------------------------------
		inline EGL_Fixed operator*(const Vec4D& other) const {
			return EGL_Mul(m_x, other.m_x) +
				EGL_Mul(m_y, other.m_y) +
				EGL_Mul(m_z, other.m_z) +
				EGL_Mul(m_w, other.m_w);
		}

		inline I64 longProduct(const Vec4D& other) const {
			return 
				(static_cast<I64>(m_x) * static_cast<I64>(other.m_x) +
				 static_cast<I64>(m_y) * static_cast<I64>(other.m_y) +
				 static_cast<I64>(m_z) * static_cast<I64>(other.m_z) +
				 static_cast<I64>(m_w) * static_cast<I64>(other.m_w)) >> EGL_PRECISION;
		}

		// ----------------------------------------------------------------------
		// Euclidean length of vector
		// ----------------------------------------------------------------------
		inline EGL_Fixed Length() const {
			return EGL_Sqrt(LengthSq());
		}

		// ----------------------------------------------------------------------
		// Square of Euclidean length of vector
		// ----------------------------------------------------------------------
		inline EGL_Fixed LengthSq() const {
			return (*this) * (*this);
		}

		// ----------------------------------------------------------------------
		// Normalize the vector to unit length
		// ----------------------------------------------------------------------
		inline void Normalize() {
			*this *= EGL_InvSqrt(LengthSq());
		}

		// ----------------------------------------------------------------------
		// Element accessors
		// ----------------------------------------------------------------------

		inline EGL_Fixed x() const {
			return m_x;
		}

		inline EGL_Fixed y() const {
			return m_y;
		}

		inline EGL_Fixed z() const {
			return m_z;
		}

		inline EGL_Fixed w() const {
			return m_w;
		}

		inline void setX(EGL_Fixed value) {
			m_x = value;
		}

		inline void setY(EGL_Fixed value) {
			m_y = value;
		}

		inline void setZ(EGL_Fixed value) {
			m_z = value;
		}

		inline void setW(EGL_Fixed value) {
			m_w = value;
		}
	};


	inline OGLES_API Vec3D EGL_Direction(const Vec4D& from, const Vec4D& to) {
		Vec3D result (EGL_Mul(to.x(), from.w()) - EGL_Mul(from.x(), to.w()),
			EGL_Mul(to.y(), from.w()) - EGL_Mul(from.y(), to.w()),
			EGL_Mul(to.z(), from.w()) - EGL_Mul(from.z(), to.w()));

		return result;
	}

	inline OGLES_API Vec4D operator*(EGL_Fixed factor, const Vec4D& vector) {
		return vector * factor;
	}


	// --------------------------------------------------------------------------
	// 4x4 Matrix class
	// --------------------------------------------------------------------------


	class Matrix4x4 {

		enum {
			ROWS = 4,			// number of rows per matrix
			COLUMNS = 4,		// number of columns per matrix
			ELEMENTS = 16,		// total number of elements
		};

		EGL_Fixed m_elements[16];
		bool m_identity;		// flag to mark identity matrix

	public:
		// ----------------------------------------------------------------------
		// This function effectively defines the order in which individual
		// matrix elements are stored in the underlying 1-D vector.
		//
		// Parameters:
		//	row			- row index into the matrix
		//	column		- column index into the matrix
		//
		// Returns:
		//	Reference to the indexed matrix element
		// ----------------------------------------------------------------------
		inline EGL_Fixed& Element(int row, int column) {
			return m_elements[row + column * ROWS]; 
		}

		inline const EGL_Fixed& Element(int row, int column) const {
			return m_elements[row + column * ROWS]; 
		}

		inline EGL_Fixed& Element(int index) {
			return m_elements[index]; 
		}

		inline const EGL_Fixed& Element(int index) const {
			return m_elements[index]; 
		}

		inline bool IsIdentity() const {
			return m_identity;
		}

		// ----------------------------------------------------------------------
		// Default constructor: Initialize the matrix as identity matrix
		// ----------------------------------------------------------------------
		inline Matrix4x4() {
			MakeIdentity();
		}


		// ----------------------------------------------------------------------
		// Construct matrix from vector of elements
		//
		// Parameters:
		//	elements		-	Pointer to array of elements, which are stored
		//						column by column
		// ----------------------------------------------------------------------
		inline Matrix4x4(const EGL_Fixed * elements) {
			for (int index = 0; index < ELEMENTS; ++index) {
				m_elements[index] = elements[index];
			}

			m_identity = false;
		}


		// ----------------------------------------------------------------------
		// Construct matrix from individual elements
		//
		// Parameters:
		//	m00,...,m33		-	mij specifies the value of Element(i, j)
		// ----------------------------------------------------------------------
		inline Matrix4x4(EGL_Fixed m00, EGL_Fixed m01, EGL_Fixed m02, EGL_Fixed m03,
							EGL_Fixed m10, EGL_Fixed m11, EGL_Fixed m12, EGL_Fixed m13,
							EGL_Fixed m20, EGL_Fixed m21, EGL_Fixed m22, EGL_Fixed m23,
							EGL_Fixed m30, EGL_Fixed m31, EGL_Fixed m32, EGL_Fixed m33) {

			Element(0, 0) = m00;
			Element(1, 0) = m10;
			Element(2, 0) = m20;
			Element(3, 0) = m30;

			Element(0, 1) = m01;
			Element(1, 1) = m11;
			Element(2, 1) = m21;
			Element(3, 1) = m31;

			Element(0, 2) = m02;
			Element(1, 2) = m12;
			Element(2, 2) = m22;
			Element(3, 2) = m32;

			Element(0, 3) = m03;
			Element(1, 3) = m13;
			Element(2, 3) = m23;
			Element(3, 3) = m33;

			m_identity = false;
		}


		// ----------------------------------------------------------------------
		// Copy constructor
		// ----------------------------------------------------------------------
		inline Matrix4x4(const Matrix4x4& other) {
			for (int index = 0; index < ELEMENTS; ++index) {
				m_elements[index] = other.m_elements[index];
			}

			m_identity = other.m_identity;
		}


		// ----------------------------------------------------------------------
		// Assignment operator
		// ----------------------------------------------------------------------
		inline Matrix4x4& operator=(const Matrix4x4& other) {
			for (int index = 0; index < ELEMENTS; ++index) {
				m_elements[index] = other.m_elements[index];
			}

			m_identity = other.m_identity;

			return *this;
		}


		// ----------------------------------------------------------------------
		// Reset matrix to be an identity matrix
		// ----------------------------------------------------------------------
		inline void MakeIdentity() {
			Element(0, 0) = Element(1, 1) = Element(2, 2) = Element(3, 3) = EGL_ONE;
			Element(0, 1) = Element(0, 2) = Element(0, 3) = 0;
			Element(1, 0) = Element(1, 2) = Element(1, 3) = 0;
			Element(2, 0) = Element(2, 1) = Element(2, 3) = 0;
			Element(3, 0) = Element(3, 1) = Element(3, 2) = 0;
			m_identity = true;
		}


		// ----------------------------------------------------------------------
		// Matrix multiplication as (*this) * other
		//
		// Parameters:
		//	other		-	RHS in matrix multiplication
		// ----------------------------------------------------------------------
		inline Matrix4x4 operator*(const Matrix4x4& other) const {
			Matrix4x4 result;

			for (int i = 0; i < ROWS; ++i) {
				for (int j = 0; j < COLUMNS; ++j) {
					EGL_Fixed sum = 0;

					for (int k = 0; k < COLUMNS; ++k) {
						sum += EGL_Mul(Element(i, k), other.Element(k, j));
					}

					result.Element(i, j) = sum;
				}
			}

			result.m_identity = m_identity && other.m_identity;
			return result;
		}


		// ----------------------------------------------------------------------
		// Transform a 3-D vector using this matrix. The vector is extended
		// with a homogenuous coordinate of 1 before being multiplied.
		//
		// Parameters:
		//	vector		-	The vector to be transformed
		// ----------------------------------------------------------------------
		inline Vec4D operator*(const Vec3D& vector) const {
			return Vec4D(
				EGL_Mul(vector.x(), Element(0, 0)) +
				EGL_Mul(vector.y(), Element(0, 1)) +
				EGL_Mul(vector.z(), Element(0, 2)) +
				Element(0, 3),

				EGL_Mul(vector.x(), Element(1, 0)) +
				EGL_Mul(vector.y(), Element(1, 1)) +
				EGL_Mul(vector.z(), Element(1, 2)) +
				Element(1, 3),

				EGL_Mul(vector.x(), Element(2, 0)) +
				EGL_Mul(vector.y(), Element(2, 1)) +
				EGL_Mul(vector.z(), Element(2, 2)) +
				Element(2, 3),

				EGL_Mul(vector.x(), Element(3, 0)) +
				EGL_Mul(vector.y(), Element(3, 1)) +
				EGL_Mul(vector.z(), Element(3, 2)) +
				Element(3, 3));
		}


		// ----------------------------------------------------------------------
		// Transform a 3-D vector using this matrix using only the upper right
		// 3x3 sub-matrix. The vector is extended
		// with a homogenuous coordinate of 1 before being multiplied.
		//
		// Parameters:
		//	vector		-	The vector to be transformed
		// ----------------------------------------------------------------------
		inline Vec3D Multiply3x3(const Vec3D& vector) const {
			return Vec3D(
				EGL_Mul(vector.x(), Element(0, 0)) +
				EGL_Mul(vector.y(), Element(0, 1)) +
				EGL_Mul(vector.z(), Element(0, 2)),

				EGL_Mul(vector.x(), Element(1, 0)) +
				EGL_Mul(vector.y(), Element(1, 1)) +
				EGL_Mul(vector.z(), Element(1, 2)),

				EGL_Mul(vector.x(), Element(2, 0)) +
				EGL_Mul(vector.y(), Element(2, 1)) +
				EGL_Mul(vector.z(), Element(2, 2)));
		}



		// ----------------------------------------------------------------------
		// Return the Z coordinate after transformation using this matrix
		// Used for fog calculation, which requires eye distance
		// ----------------------------------------------------------------------
		inline EGL_Fixed GetTransformedZ(const Vec3D& vector) const {
			return
				EGL_Mul(vector.x(), Element(2, 0)) +
				EGL_Mul(vector.y(), Element(2, 1)) +
				EGL_Mul(vector.z(), Element(2, 2)) +
				Element(2, 3);
		}


		// ----------------------------------------------------------------------
		// Transform a 4-D vector using this matrix. 
		//
		// Parameters:
		//	vector		-	The vector to be transformed
		// ----------------------------------------------------------------------
		inline Vec4D operator*(const Vec4D& vector) const {
			return Vec4D(
				EGL_Mul(vector.x(), Element(0, 0)) +
				EGL_Mul(vector.y(), Element(0, 1)) +
				EGL_Mul(vector.z(), Element(0, 2)) +
				EGL_Mul(vector.w(), Element(0, 3)),

				EGL_Mul(vector.x(), Element(1, 0)) +
				EGL_Mul(vector.y(), Element(1, 1)) +
				EGL_Mul(vector.z(), Element(1, 2)) +
				EGL_Mul(vector.w(), Element(1, 3)),

				EGL_Mul(vector.x(), Element(2, 0)) +
				EGL_Mul(vector.y(), Element(2, 1)) +
				EGL_Mul(vector.z(), Element(2, 2)) +
				EGL_Mul(vector.w(), Element(2, 3)),

				EGL_Mul(vector.x(), Element(3, 0)) +
				EGL_Mul(vector.y(), Element(3, 1)) +
				EGL_Mul(vector.z(), Element(3, 2)) +
				EGL_Mul(vector.w(), Element(3, 3)));
		}


		inline void Multiply(const Vec4D& vector, Vec4D& result) const {
			result = Vec4D(
				EGL_Mul(vector.x(), Element(0, 0)) +
				EGL_Mul(vector.y(), Element(0, 1)) +
				EGL_Mul(vector.z(), Element(0, 2)) +
				EGL_Mul(vector.w(), Element(0, 3)),

				EGL_Mul(vector.x(), Element(1, 0)) +
				EGL_Mul(vector.y(), Element(1, 1)) +
				EGL_Mul(vector.z(), Element(1, 2)) +
				EGL_Mul(vector.w(), Element(1, 3)),

				EGL_Mul(vector.x(), Element(2, 0)) +
				EGL_Mul(vector.y(), Element(2, 1)) +
				EGL_Mul(vector.z(), Element(2, 2)) +
				EGL_Mul(vector.w(), Element(2, 3)),

				EGL_Mul(vector.x(), Element(3, 0)) +
				EGL_Mul(vector.y(), Element(3, 1)) +
				EGL_Mul(vector.z(), Element(3, 2)) +
				EGL_Mul(vector.w(), Element(3, 3)));
		}


		// ----------------------------------------------------------------------
		// Calculate the matrix for which the upper left 3x3 matrix is the 
		// inverse of the upper left 3x3 matrix of the receiver canconically
		// embedded into 4-dimensional space
		// ----------------------------------------------------------------------
		Matrix4x4 InverseUpper3(bool rescale) const;

		// ----------------------------------------------------------------------
		// Compute general inverse of a 4 by 4 matrix
		// ----------------------------------------------------------------------
		Matrix4x4 Inverse() const;

		Matrix4x4 Transpose() const {
			Matrix4x4 result;

			for (int i = 0; i < ROWS; ++i) {
				for (int j = 0; j < COLUMNS; ++j) {
					result.Element(i, j) = Element(j, i);
				}
			}

			result.m_identity = m_identity;
			return result;
		}

		// ----------------------------------------------------------------------
		// Create a transformation matrix that scales in x, y and z direction
		// according to the given scale factors.
		//
		// Parameters:
		//	x			-	scale factor in x direction
		//	y			-	scale factor in y direction
		//	z			-	scale factor in z direction
		// ----------------------------------------------------------------------
		static Matrix4x4 CreateScale(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z);


		// ----------------------------------------------------------------------
		// Create a transformation matrix that rotates around the axis specified
		// by x, and and z by a the given angle.
		//
		// Parameters:
		//	angle		-	the angle in degrees
		//  x, y, z		-	the direction of the axis as vector from the origin
		// ----------------------------------------------------------------------
		static Matrix4x4 CreateRotate(EGL_Fixed angle, EGL_Fixed x, 
			EGL_Fixed y, EGL_Fixed z);


		// ----------------------------------------------------------------------
		// Create a transformation matrix that translates in x, y and z direction
		// according to the given translation values.
		//
		// Parameters:
		//	x			-	translation in x direction
		//	y			-	translation in y direction
		//	z			-	translation in z direction
		// ----------------------------------------------------------------------
		static Matrix4x4 CreateTranslate(EGL_Fixed x, EGL_Fixed y, EGL_Fixed z);


		// ----------------------------------------------------------------------
		// Create a transformation matrix for a perspective projection of the
		// cube described by its corners into the unit volume.
		//
		// Parameters:
		//	l, b, -n	- lower left point to be mapped into lower left front
		//  r, t, -f	- upper right point and maximum depth coordinate
		// ----------------------------------------------------------------------
		static Matrix4x4 CreateFrustrum(EGL_Fixed left, EGL_Fixed right, 
			EGL_Fixed bottom, EGL_Fixed top, EGL_Fixed zNear, EGL_Fixed zFar);


		// ----------------------------------------------------------------------
		// Create a transformation matrix for an orthographic projection of the
		// cube described by its corners into the unit volume.
		//
		// Parameters:
		//	l, b, -n	- lower left point to be mapped into lower left front
		//  r, t, -f	- upper right point and maximum depth coordinate
		// ----------------------------------------------------------------------
		static Matrix4x4 CreateOrtho(EGL_Fixed left, EGL_Fixed right, 
			EGL_Fixed bottom, EGL_Fixed top, EGL_Fixed zNear, EGL_Fixed zFar);
	};
}

#endif // ndef EGL_LINALG_H
