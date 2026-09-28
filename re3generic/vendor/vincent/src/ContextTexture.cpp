// ==========================================================================
//
// ContextTexture.cpp	Rendering Context Class for 3D Rendering Library
//
//				Texturing Functions
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
#include "Color.h"
#include "Surface.h"
#include "fixed.h"
#include "Utils.h"


using namespace EGL;


// --------------------------------------------------------------------------
// Allocation and selection of texture objects
// --------------------------------------------------------------------------

GLboolean Context :: IsTexture(GLuint texture) {
	return m_Textures.IsObject(texture);
}

void Context :: BindTexture(GLenum target, GLuint texture) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	MultiTexture * multiTexture = m_Textures.GetObject(texture);

	if (multiTexture) {
		GetRasterizer()->SetTexture(m_ActiveTexture, multiTexture);
	}
}

void Context :: DeleteTextures(GLsizei n, const GLuint *textures) { 

	if (n < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	while (n-- != 0) {
		U32 texture = *textures++;

		if (texture != 0) {
			for (size_t unit = 0; unit < EGL_NUM_TEXTURE_UNITS; ++unit) {
				if (m_Textures.GetObject(texture) == GetRasterizer()->GetTexture(unit)) {
					GetRasterizer()->SetTexture(unit, m_Textures.GetObject(0));
				}
			}

			if (texture != 0) {
				m_Textures.Deallocate(texture);
			}
		}
	}
}

void Context :: GenTextures(GLsizei n, GLuint *textures) { 

	if (n < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	while (n != 0) {
		*textures++ = m_Textures.Allocate();
		--n;
	}
}

// --------------------------------------------------------------------------
// Texture specification methods
// --------------------------------------------------------------------------

namespace {
	RasterizerState::TextureFormat TextureFormatFromEnum(GLenum format) {
		switch (format) {
			case GL_ALPHA:
				return RasterizerState::TextureFormatAlpha;

			case 1:
			case GL_LUMINANCE:
				return RasterizerState::TextureFormatLuminance;

			case 2:
			case GL_LUMINANCE_ALPHA:
				return RasterizerState::TextureFormatLuminanceAlpha;

			case 3:
			case GL_RGB:
				return RasterizerState::TextureFormatRGB565;

			case 4:
			case GL_RGBA:
				//return RasterizerState::TextureFormatRGBA5551;
				//return RasterizerState::TextureFormatRGBA4444;
				return RasterizerState::TextureFormatRGBA8;

			default:
				return RasterizerState::TextureFormatInvalid;
		}
	}

	GLenum InternalTypeForInternalFormat(RasterizerState::TextureFormat format) {
		switch (format) {
			case RasterizerState::TextureFormatAlpha:
			case RasterizerState::TextureFormatLuminance:
			case RasterizerState::TextureFormatLuminanceAlpha:
			case RasterizerState::TextureFormatRGB8:
			case RasterizerState::TextureFormatRGBA8:
			default:
				return GL_UNSIGNED_BYTE;

			case RasterizerState::TextureFormatRGB565:
				return GL_UNSIGNED_SHORT_5_6_5;

			case RasterizerState::TextureFormatRGBA5551:
				return GL_UNSIGNED_SHORT_5_5_5_1;

			case RasterizerState::TextureFormatRGBA4444:
				return GL_UNSIGNED_SHORT_4_4_4_4;
		}
	}

	RasterizerState::WrappingMode WrappingModeFromEnum(GLenum mode) {
		switch (mode) {
			case GL_CLAMP_TO_EDGE:	return RasterizerState::WrappingModeClampToEdge;
			case GL_REPEAT:			return RasterizerState::WrappingModeRepeat;
			default:				return RasterizerState::WrappingModeInvalid;
		}
	}

	RasterizerState::FilterMode MinFilterModeFromEnum(GLenum mode) {
		switch (mode) {
			case GL_NEAREST_MIPMAP_LINEAR:	
			case GL_NEAREST_MIPMAP_NEAREST:	
			case GL_NEAREST:				return RasterizerState::FilterModeNearest;

			case GL_LINEAR_MIPMAP_LINEAR:	
			case GL_LINEAR_MIPMAP_NEAREST:	
			case GL_LINEAR:					return RasterizerState::FilterModeLinear;

			default:						return RasterizerState::FilterModeInvalid;
		}
	}

	RasterizerState::FilterMode MipmapFilterModeFromEnum(GLenum mode) {
		switch (mode) {
			case GL_NEAREST_MIPMAP_LINEAR:	
			case GL_LINEAR_MIPMAP_LINEAR:	return RasterizerState::FilterModeLinear;

			case GL_NEAREST_MIPMAP_NEAREST:	
			case GL_LINEAR_MIPMAP_NEAREST:	return RasterizerState::FilterModeNearest;

			case GL_NEAREST:				
			case GL_LINEAR:					return RasterizerState::FilterModeNone;

			default:						return RasterizerState::FilterModeInvalid;
		}
	}

	RasterizerState::FilterMode MagFilterModeFromEnum(GLenum mode) {
		switch (mode) {
			case GL_NEAREST:		return RasterizerState::FilterModeNearest;
			case GL_LINEAR:			return RasterizerState::FilterModeLinear;
			default:				return RasterizerState::FilterModeInvalid;
		}
	}

	inline U32 Align(U32 offset, size_t alignment) {
		return (offset + alignment - 1) & ~(alignment - 1);
	}

	// -------------------------------------------------------------------------
	// Given two bitmaps src and dst, where src has dimensions 
	// (srcWidth * srcHeight) and dst has dimensions (dstWidth * dstHeight),
	// copy the rectangle (srcX, srcY, copyWidth, copyHeight) into
	// dst at location (dstX, dstY).
	//
	// It is assumed that the copy rectangle is non-empty and has been clipped
	// against the src and target rectangles
	// -------------------------------------------------------------------------
	template<class PixelType> 
	void CopyPixels(const PixelType * src, U32 srcWidth, U32 srcHeight, 
					U32 srcX, U32 srcY, U32 copyWidth, U32 copyHeight,
					PixelType * dst, U32 dstWidth, U32 dstHeight, U32 dstX, U32 dstY,
					size_t srcAlignment, size_t dstAlignment) {

		size_t pixelSize = sizeof(PixelType);

		U32 srcBytesWidth = Align(srcWidth * pixelSize, srcAlignment);
		U32 dstBytesWidth = Align(dstWidth * pixelSize, dstAlignment);

		U32 srcGap = srcBytesWidth - copyWidth * pixelSize;	// how many bytes to skip for next line
		U32 dstGap = dstBytesWidth - copyWidth * pixelSize;	// how many bytes to skip for next line

		const U8 * srcPtr = reinterpret_cast<const U8 *>(src) + srcX * pixelSize + srcY * srcBytesWidth;
		U8 * dstPtr = reinterpret_cast<U8 *>(dst) + dstX * pixelSize + dstY * dstBytesWidth;

		do {
			U32 span = copyWidth;

			do {
				*reinterpret_cast<PixelType *>(dstPtr) = *reinterpret_cast<const PixelType *>(srcPtr);
				dstPtr += pixelSize;
				srcPtr += pixelSize;
			} while (--span);

			srcPtr += srcGap;
			dstPtr += dstGap;
		} while (--copyHeight);
	}

	// -------------------------------------------------------------------------
	// Declarations of the different texture conversions that are supported
	// -------------------------------------------------------------------------

	struct RGB2Color {
		enum {
			baseIncr = 3
		};

		Color operator()(const U8 * &ptr) {
			U8 r = *ptr++;
			U8 g = *ptr++;
			U8 b = *ptr++;
			return Color(r, g, b, 0);
		}
	};

	struct Color2RGB {
		enum {
			baseIncr = 3
		};

		void operator()(U8 * &ptr, const Color& value) {
			*ptr++ = value.R();
			*ptr++ = value.G();
			*ptr++ = value.B();
		}
	};

	struct RGBA2Color {
		enum {
			baseIncr = 4
		};

		Color operator()(const U8 * &ptr) {
			U8 r = *ptr++;
			U8 g = *ptr++;
			U8 b = *ptr++;
			U8 a = *ptr++;
			return Color(r, g, b, a);
		}
	};

	struct Color2RGBA {
		enum {
			baseIncr = 4
		};

		void operator()(U8 * &ptr, const Color& value) {
			*ptr++ = value.R();
			*ptr++ = value.G();
			*ptr++ = value.B();
			*ptr++ = value.A();
		}
	};

	struct Color2Luminance {
		enum {
			baseIncr = 1
		};

		void operator()(U8 * &ptr, const Color& value) {
			U8 luminance = value.R();
			
			/*if (value.G() > luminance)
				luminance = value.G();

			if (value.B() > luminance)
				luminance = value.B();*/

			*ptr++ = luminance;
		}
	};

	struct Color2Alpha {
		enum {
			baseIncr = 1
		};

		void operator()(U8 * &ptr, const Color& value) {
			*ptr++ = value.A();
		}
	};

	struct Color2LuminanceAlpha {
		enum {
			baseIncr = 2
		};

		void operator()(U8 * &ptr, const Color& value) {
			U8 luminance = value.R();
			
			/*if (value.G() > luminance)
				luminance = value.G();

			if (value.B() > luminance)
				luminance = value.B();*/

			*ptr++ = luminance;
			*ptr++ = value.A();
		}
	};

	struct RGBA44442Color {
		enum {
			baseIncr = 2
		};

		Color operator()(const U8 * &ptr) {
			const U16 * u16Ptr = reinterpret_cast<const U16 *>(ptr);
			ptr += baseIncr;
			return Color::From4444(*u16Ptr);

		}
	};

	struct Color2RGBA4444 {
		enum {
			baseIncr = 2
		};

		void operator()(U8 * &ptr, const Color& value) {
			U16 * u16Ptr = reinterpret_cast<U16 *>(ptr);
			ptr += baseIncr;
			*u16Ptr = value.ConvertTo4444();
		}
	};

	struct Color2RGB565 {
		enum {
			baseIncr = 2
		};

		void operator()(U8 * &ptr, const Color& value) {
			U16 * u16Ptr = reinterpret_cast<U16 *>(ptr);
			ptr += baseIncr;
			*u16Ptr = value.ConvertTo565();
		}
	};

	struct RGB5652Color {
		enum {
			baseIncr = 2
		};

		Color operator()(const U8 * &ptr) {
			const U16 * u16Ptr = reinterpret_cast<const U16 *>(ptr);
			ptr += baseIncr;
			return Color::From565(*u16Ptr);
		}
	};

	struct RGBA55512Color {
		enum {
			baseIncr = 2
		};

		Color operator()(const U8 * &ptr) {
			const U16 * u16Ptr = reinterpret_cast<const U16 *>(ptr);
			ptr += baseIncr;
			return Color::From5551(*u16Ptr);
		}
	};

	struct Color2RGBA5551 {
		enum {
			baseIncr = 2
		};

		void operator()(U8 * &ptr, const Color& value) {
			U16 * u16Ptr = reinterpret_cast<U16 *>(ptr);
			ptr += baseIncr;
			*u16Ptr = value.ConvertTo5551();
		}
	};

	// -------------------------------------------------------------------------
	// Given two bitmaps src and dst, where src has dimensions 
	// (srcWidth * srcHeight) and dst has dimensions (dstWidth * dstHeight),
	// copy the rectangle (srcX, srcY, copyWidth, copyHeight) into
	// dst at location (dstX, dstY).
	//
	// It is assumed that the copy rectangle is non-empty and has been clipped
	// against the src and target rectangles
	// -------------------------------------------------------------------------
	template<class SrcAccessor, class DstAccessor> 
	void CopyPixelsA(const void * src, U32 srcWidth, U32 srcHeight, 
					U32 srcX, U32 srcY, U32 copyWidth, U32 copyHeight,
					void * dst, U32 dstWidth, U32 dstHeight, U32 dstX, U32 dstY,
					const SrcAccessor&, const DstAccessor&,
					size_t srcAlignment, size_t dstAlignment) {

		SrcAccessor srcAccessor;
		DstAccessor dstAccessor;

		size_t srcPixelSize = SrcAccessor::baseIncr;
		size_t dstPixelSize = DstAccessor::baseIncr;

		U32 srcBytesWidth = Align(srcWidth * srcPixelSize, srcAlignment);
		U32 dstBytesWidth = Align(dstWidth * dstPixelSize, dstAlignment);

		U32 srcGap = srcBytesWidth - copyWidth * srcPixelSize;	// how many bytes to skip for next line
		U32 dstGap = dstBytesWidth - copyWidth * dstPixelSize;	// how many bytes to skip for next line

		const U8 * srcPtr = reinterpret_cast<const U8 *>(src) + srcX * srcPixelSize + srcY * srcBytesWidth;
		U8 * dstPtr = reinterpret_cast<U8 *>(dst) + dstX * dstPixelSize + dstY * dstBytesWidth;

		do {
			U32 span = copyWidth;

			do {
				dstAccessor(dstPtr, srcAccessor(srcPtr));
			} while (--span);

			srcPtr += srcGap;
			dstPtr += dstGap;
		} while (--copyHeight);
	}

	template<class DstAccessor> 
	void CopySurfacePixels(Surface * src, 
						   U32 srcX, U32 srcY, U32 copyWidth, U32 copyHeight,
					       void * dst, U32 dstWidth, U32 dstHeight, U32 dstX, U32 dstY,
					       const DstAccessor&, size_t dstAlignment) {

		DstAccessor dstAccessor;
		size_t dstPixelSize = DstAccessor::baseIncr;
		U32 dstBytesWidth = Align(dstWidth * dstPixelSize, dstAlignment);
		U32 dstGap = dstBytesWidth - copyWidth * dstPixelSize;	// how many bytes to skip for next line

		U32 srcWidth = src->GetWidth();
		U32 srcHeight = src->GetHeight();
		U32 srcGap = srcWidth - copyWidth;

		const U16 * srcPtr = src->GetColorBuffer() + srcX + srcY * srcWidth;
		const U8 * alphaPtr = src->GetAlphaBuffer() + srcX + srcY * srcWidth;
		U8 * dstPtr = reinterpret_cast<U8 *>(dst) + dstX * dstPixelSize + dstY * dstBytesWidth;

		do {
			U32 span = copyWidth;

			do {
				Color color = Color::From565A(*srcPtr++, *alphaPtr++);
				dstAccessor(dstPtr, color);
			} while (--span);

			srcPtr += srcGap;
			alphaPtr += srcGap;
			dstPtr += dstGap;
		} while (--copyHeight);
	}

	// -------------------------------------------------------------------------
	// Given two bitmaps src and dst, where src has dimensions 
	// (srcWidth * srcHeight) and dst has dimensions (dstWidth * dstHeight),
	// copy the rectangle (srcX, srcY, copyWidth, copyHeight) into
	// dst at location (dstX, dstY).
	//
	// The texture format and the type of the source format a given as
	// parameters.
	// -------------------------------------------------------------------------
	void CopyPixels(const void * src, U32 srcWidth, U32 srcHeight, 
					U32 srcX, U32 srcY, U32 copyWidth, U32 copyHeight,
					void * dst, U32 dstWidth, U32 dstHeight, U32 dstX, U32 dstY,
					RasterizerState::TextureFormat format, GLenum srcType, GLenum dstType,
					size_t srcAlignment, size_t dstAlignment) {

		// ---------------------------------------------------------------------
		// clip lower left corner
		// ---------------------------------------------------------------------

		if (srcX >= srcWidth || srcY >= srcHeight ||
			dstX >= dstWidth || dstY >= dstHeight ||
			copyWidth == 0 || copyHeight == 0) {
			return;
		}

		// ---------------------------------------------------------------------
		// clip the copy rectangle to the valid size
		// ---------------------------------------------------------------------

		if (copyWidth > dstWidth) {
			copyWidth = dstWidth;
		}

		if (copyWidth > srcWidth) {
			copyWidth = srcWidth;
		}

		if (copyHeight > dstHeight) {
			copyHeight = dstHeight;
		}

		if (copyHeight > srcHeight) {
			copyHeight = srcHeight;
		}

		// ---------------------------------------------------------------------
		// at this point we know that the copy rectangle is valid and non-empty
		// ---------------------------------------------------------------------

			
		switch (format) {
			case RasterizerState::TextureFormatAlpha:
			case RasterizerState::TextureFormatLuminance:
				CopyPixels(reinterpret_cast<const U8 *>(src), srcWidth, srcHeight, srcX, srcY, copyWidth, copyHeight,
					reinterpret_cast<U8 *>(dst), dstWidth, dstHeight, dstX, dstY, srcAlignment, dstAlignment);
				break;

			case RasterizerState::TextureFormatLuminanceAlpha:
				CopyPixels(reinterpret_cast<const U16 *>(src), srcWidth, srcHeight, srcX, srcY, copyWidth, copyHeight,
					reinterpret_cast<U16 *>(dst), dstWidth, dstHeight, dstX, dstY, srcAlignment, dstAlignment);
				break;

			case RasterizerState::TextureFormatRGB565:
			case RasterizerState::TextureFormatRGB8:
				switch (srcType) {
					case GL_UNSIGNED_BYTE:
						switch (dstType) {
							case GL_UNSIGNED_BYTE:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGB2Color(), Color2RGB(), srcAlignment, dstAlignment);
								break;

							case GL_UNSIGNED_SHORT_5_6_5:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGB2Color(), Color2RGB565(), srcAlignment, dstAlignment);
								break;
						}
						break;

					case GL_UNSIGNED_SHORT_5_6_5:
						switch (dstType) {
							case GL_UNSIGNED_BYTE:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGB5652Color(), Color2RGB(), srcAlignment, dstAlignment);
								break;

							case GL_UNSIGNED_SHORT_5_6_5:
								CopyPixels(reinterpret_cast<const U16 *>(src), srcWidth, srcHeight, srcX, srcY, copyWidth, copyHeight,
									reinterpret_cast<U16 *>(dst), dstWidth, dstHeight, dstX, dstY, srcAlignment, dstAlignment);
								break;
						}
						break;
				}

				break;

			case RasterizerState::TextureFormatRGBA5551:
			case RasterizerState::TextureFormatRGBA4444:
			case RasterizerState::TextureFormatRGBA8:
				switch (srcType) {
					case GL_UNSIGNED_BYTE:
						switch (dstType) {
							case GL_UNSIGNED_BYTE:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA2Color(), Color2RGBA(), srcAlignment, dstAlignment);
								break;
							case GL_UNSIGNED_SHORT_5_5_5_1:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA2Color(), Color2RGBA5551(), srcAlignment, dstAlignment);
								break;
							case GL_UNSIGNED_SHORT_4_4_4_4:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA2Color(), Color2RGBA4444(), srcAlignment, dstAlignment);
								break;
						}
						break;

					case GL_UNSIGNED_SHORT_5_5_5_1:
						switch (dstType) {
							case GL_UNSIGNED_BYTE:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA55512Color(), Color2RGBA(), srcAlignment, dstAlignment);
								break;
							case GL_UNSIGNED_SHORT_5_5_5_1:
								CopyPixels(reinterpret_cast<const U16 *>(src), srcWidth, srcHeight, srcX, srcY, copyWidth, copyHeight,
									reinterpret_cast<U16 *>(dst), dstWidth, dstHeight, dstX, dstY, srcAlignment, dstAlignment);
								break;
							case GL_UNSIGNED_SHORT_4_4_4_4:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA55512Color(), Color2RGBA4444(), srcAlignment, dstAlignment);
								break;
						}
						break;

					case GL_UNSIGNED_SHORT_4_4_4_4:
						switch (dstType) {
							case GL_UNSIGNED_BYTE:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA44442Color(), Color2RGBA(), srcAlignment, dstAlignment);
								break;
							case GL_UNSIGNED_SHORT_5_5_5_1:
								CopyPixelsA(src, srcWidth, srcHeight, srcX, srcY, 
									copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
									RGBA44442Color(), Color2RGBA5551(), srcAlignment, dstAlignment);
								break;
							case GL_UNSIGNED_SHORT_4_4_4_4:
								CopyPixels(reinterpret_cast<const U16 *>(src), srcWidth, srcHeight, srcX, srcY, copyWidth, copyHeight,
									reinterpret_cast<U16 *>(dst), dstWidth, dstHeight, dstX, dstY, srcAlignment, dstAlignment);
								break;
						}
						break;
				}

				break;

		}
	}

	// -------------------------------------------------------------------------
	// Given two bitmaps src and dst, where src is a drawing surface
	// and dst has dimensions (dstWidth * dstHeight),
	// copy the rectangle (srcX, srcY, copyWidth, copyHeight) into
	// dst at location (dstX, dstY).
	//
	// The texture format and the type of the source format a given as
	// parameters.
	// -------------------------------------------------------------------------
	bool CopySurfacePixels(Surface * src, 
					U32 srcX, U32 srcY, U32 copyWidth, U32 copyHeight,
					void * dst, U32 dstWidth, U32 dstHeight, U32 dstX, U32 dstY,
					RasterizerState::TextureFormat format, GLenum dstType,
					size_t dstAlignment) {

		U32 srcWidth = src->GetWidth();
		U32 srcHeight = src->GetHeight();

		// ---------------------------------------------------------------------
		// clip lower left corner
		// ---------------------------------------------------------------------

		if (srcX >= srcWidth || srcY >= srcHeight ||
			dstX >= dstWidth || dstY >= dstHeight ||
			copyWidth == 0 || copyHeight == 0) {
			return true;
		}

		// ---------------------------------------------------------------------
		// clip the copy rectangle to the valid size
		// ---------------------------------------------------------------------

		if (copyWidth > dstWidth) {
			copyWidth = dstWidth;
		}

		if (copyWidth > srcWidth) {
			copyWidth = srcWidth;
		}

		if (copyHeight > dstHeight) {
			copyHeight = dstHeight;
		}

		if (copyHeight > srcHeight) {
			copyHeight = srcHeight;
		}

		// ---------------------------------------------------------------------
		// at this point we know that the copy rectangle is valid and non-empty
		// ---------------------------------------------------------------------

		switch (format) {
			case RasterizerState::TextureFormatRGBA5551:
			case RasterizerState::TextureFormatRGBA4444:
			case RasterizerState::TextureFormatRGBA8:
				switch (dstType) {
				case GL_UNSIGNED_BYTE:
					CopySurfacePixels(src, srcX, srcY,
						copyWidth, copyHeight,
						dst, dstWidth, dstHeight, dstX, dstY,
						Color2RGBA(), dstAlignment);
					return true;

				case GL_UNSIGNED_SHORT_5_5_5_1:
					CopySurfacePixels(src, srcX, srcY,
						copyWidth, copyHeight,
						dst, dstWidth, dstHeight, dstX, dstY,
						Color2RGBA5551(), dstAlignment);
					return true;

				case GL_UNSIGNED_SHORT_4_4_4_4:
					CopySurfacePixels(src, srcX, srcY,
						copyWidth, copyHeight,
						dst, dstWidth, dstHeight, dstX, dstY,
						Color2RGBA4444(), dstAlignment);
					return true;

				}

				return false;

			case RasterizerState::TextureFormatRGB565:
			case RasterizerState::TextureFormatRGB8:
				switch (dstType) {
				case GL_UNSIGNED_BYTE:
					CopyPixelsA(src->GetColorBuffer(), srcWidth, srcHeight, srcX, srcY, 
						copyWidth, copyHeight, dst, dstWidth, dstHeight, dstX, dstY,
						RGB5652Color(), Color2RGB(), 1, dstAlignment);

					return true;

				case GL_UNSIGNED_SHORT_5_6_5:
					CopyPixels(src->GetColorBuffer(), srcWidth, srcHeight, srcX, srcY, copyWidth, copyHeight,
						reinterpret_cast<U16 *>(dst), dstWidth, dstHeight, dstX, dstY, 1, dstAlignment);

					return true;
				}

				return false;

			case RasterizerState::TextureFormatLuminance:
				switch (dstType) {
				case GL_UNSIGNED_BYTE:
					CopySurfacePixels(src, srcX, srcY,
						copyWidth, copyHeight,
						dst, dstWidth, dstHeight, dstX, dstY,
						Color2Luminance(), dstAlignment);
					return true;
				}

				return false;

			case RasterizerState::TextureFormatAlpha:
				switch (dstType) {
				case GL_UNSIGNED_BYTE:
					CopySurfacePixels(src, srcX, srcY,
						copyWidth, copyHeight,
						dst, dstWidth, dstHeight, dstX, dstY,
						Color2Alpha(), dstAlignment);
					return true;
				}

				return false;

			case RasterizerState::TextureFormatLuminanceAlpha:
				switch (dstType) {
				case GL_UNSIGNED_BYTE:
					CopySurfacePixels(src, srcX, srcY,
						copyWidth, copyHeight,
						dst, dstWidth, dstHeight, dstX, dstY,
						Color2LuminanceAlpha(), dstAlignment);
					return true;
				}

				return false;
		}

		return false;
	}

	// -------------------------------------------------------------------------
	// Ensure that the two formats (internalFormat) and
	// (externalFormat, type) are assignment compatible
	//
	// Parameters:
	//	internalFormat	-	the format of the target texture
	//	externalFormat	-	the external bitmap format
	//	type			-	the external bitmap type
	// -------------------------------------------------------------------------
	bool ValidateFormats(RasterizerState::TextureFormat internalFormat,
		RasterizerState::TextureFormat externalFormat, GLenum type) {
		if (internalFormat == RasterizerState::TextureFormatInvalid || 
			externalFormat == RasterizerState::TextureFormatInvalid || 
			internalFormat != externalFormat) {
			return false;
		}

		switch (internalFormat) {
			case RasterizerState::TextureFormatAlpha:
			case RasterizerState::TextureFormatLuminance:
			case RasterizerState::TextureFormatLuminanceAlpha:
				if (type != GL_UNSIGNED_BYTE) {
					return false;
				}

				break;

			case RasterizerState::TextureFormatRGB565:
			case RasterizerState::TextureFormatRGB8:
				if (type != GL_UNSIGNED_BYTE && type != GL_UNSIGNED_SHORT_5_6_5) {
					return false;
				}

				break;

			case RasterizerState::TextureFormatRGBA5551:
			case RasterizerState::TextureFormatRGBA4444:
			case RasterizerState::TextureFormatRGBA8:
				if (type != GL_UNSIGNED_BYTE &&
					type != GL_UNSIGNED_SHORT_4_4_4_4 &&
					type != GL_UNSIGNED_SHORT_5_5_5_1) {
					return false;
				}

				break;

		}

		return true;
	}

	GLenum TypeForInternalFormat(RasterizerState::TextureFormat format) {
		switch (format) {
			default:
			case RasterizerState::TextureFormatAlpha:
			case RasterizerState::TextureFormatLuminance:
			case RasterizerState::TextureFormatLuminanceAlpha:
			case RasterizerState::TextureFormatRGB8:
			case RasterizerState::TextureFormatRGBA8:
				return GL_UNSIGNED_BYTE;

			case RasterizerState::TextureFormatRGB565:
				return GL_UNSIGNED_SHORT_5_6_5;

			case RasterizerState::TextureFormatRGBA5551:
				return GL_UNSIGNED_SHORT_5_5_5_1;

			case RasterizerState::TextureFormatRGBA4444:
				return GL_UNSIGNED_SHORT_4_4_4_4;

		}
	}
}


namespace {

	enum PaletteFormat {
		PaletteInvalid = -1,
		PaletteRGB8,
		PaletteRGBA8,
		PaletteRGB565,
		PaletteRGBA4444,
		PaletteRGBA5551
	};

	template <class SrcAccessor> inline void CreatePalette(const U8 *& data,
		size_t numberOfColors, Color * colors, const SrcAccessor& accessor) {

		for (size_t index = 0; index < numberOfColors; ++index) {
			colors[index] = const_cast<SrcAccessor&>(accessor)(data);
		}
	}

	Color * ExtractColorPalette(const U8 *& data, PaletteFormat format, size_t numberOfColors) {

		Color * colors = new Color[numberOfColors];

		switch (format) {
		case PaletteRGB8:
			CreatePalette(data, numberOfColors, colors, RGB2Color());
			break;

		case PaletteRGBA8:
			CreatePalette(data, numberOfColors, colors, RGBA2Color());
			break;

		case PaletteRGB565:
			CreatePalette(data, numberOfColors, colors, RGB5652Color());
			break;

		case PaletteRGBA4444:
			CreatePalette(data, numberOfColors, colors, RGBA44442Color());
			break;

		case PaletteRGBA5551:
			CreatePalette(data, numberOfColors, colors, RGBA55512Color());
			break;
		}

		return colors;
	}

	struct ConstU4Ptr {
		ConstU4Ptr(const void * _ptr) {
			ptr = reinterpret_cast<const U8 *>(_ptr);
			nibble = false;
		}

		U8 operator*() const {
			U8 value = *ptr;

			if (nibble) 
				return value & 0xf;
			else 
				return value >> 4;
		}

		const ConstU4Ptr& operator++() {
			if (nibble) {
				++ptr;
				nibble = false;
			} else {
				nibble = true;
			}

			return *this;
		}

		const U8 * ptr;
		bool nibble;
	};
}


namespace {
	bool IsPowerOf2(GLsizei value) {
		if (!value)
			return true;

		while (!(value & 1))
			value >>= 1;

		value >>= 1;

		return value == 0;
	}
}


void Context :: CompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, 
									 GLint border, GLsizei imageSize, const GLvoid *data) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (level > 0 || -level > RasterizerState::LogMaxTextureSize) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (border != 0 || width < 0 || height < 0 || !IsPowerOf2(width) || !IsPowerOf2(height)) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	I32 paletteBits, colorSize;
	PaletteFormat paletteFormat;
	bool hasAlpha;

	switch (internalformat) {
	case GL_PALETTE4_RGB8_OES:
		paletteBits = 4;
		colorSize = 3;
		paletteFormat = PaletteRGB8;
		hasAlpha = false;
		break;

	case GL_PALETTE4_RGBA8_OES:
		paletteBits = 4;
		colorSize = 4;
		paletteFormat = PaletteRGBA8;
		hasAlpha = true;
		break;

	case GL_PALETTE4_R5_G6_B5_OES:
		paletteBits = 4;
		colorSize = 2;
		paletteFormat = PaletteRGB565;
		hasAlpha = false;
		break;

	case GL_PALETTE4_RGBA4_OES:
		paletteBits = 4;
		colorSize = 2;
		paletteFormat = PaletteRGBA4444;
		hasAlpha = true;
		break;

	case GL_PALETTE4_RGB5_A1_OES:
		paletteBits = 4;
		colorSize = 2;
		paletteFormat = PaletteRGBA5551;
		hasAlpha = true;
		break;

	case GL_PALETTE8_RGB8_OES:
		paletteBits = 8;
		colorSize = 3;
		paletteFormat = PaletteRGB8;
		hasAlpha = false;
		break;

	case GL_PALETTE8_RGBA8_OES:
		paletteBits = 8;
		colorSize = 4;
		paletteFormat = PaletteRGBA8;
		hasAlpha = true;
		break;

	case GL_PALETTE8_R5_G6_B5_OES:
		paletteBits = 8;
		colorSize = 2;
		paletteFormat = PaletteRGB565;
		hasAlpha = false;
		break;

	case GL_PALETTE8_RGBA4_OES:
		paletteBits = 8;
		colorSize = 2;
		paletteFormat = PaletteRGBA4444;
		hasAlpha = true;
		break;

	case GL_PALETTE8_RGB5_A1_OES:
		paletteBits = 8;
		colorSize = 2;
		paletteFormat = PaletteRGBA5551;
		hasAlpha = true;
		break;

	default:
		RecordError(GL_INVALID_VALUE);
		return;
	}

	I32 paletteSize = 1 << paletteBits;

    if (((width * paletteBits) + 7) / 8 * height + paletteSize * colorSize != imageSize) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	// extract the palette

	if (!data)
		return;

	const U8 * dataPtr = reinterpret_cast<const U8 *>(data);
	Color * colors = ExtractColorPalette(dataPtr, paletteFormat, paletteSize);
	U16 * pixels = new U16[width * height];

	GLint minLevel, maxLevel;

	if (level >= 0) {
		minLevel = maxLevel = level;
	} else {
		minLevel = 0;
		maxLevel = -level - 1;
	}

	if (paletteSize == 16) {

		ConstU4Ptr nibblePtr(dataPtr);

		for (GLint level = minLevel; level <= maxLevel; ++level) {

			size_t pixelCount = width * height;

			if (hasAlpha) {
				U16 * expandedPixels = pixels;

				while (pixelCount--) {
					*expandedPixels++ = colors[*nibblePtr].ConvertTo4444();
					++nibblePtr;
				}

				TexImage2D(target, level, GL_RGBA, width, height, 0, 
						   GL_RGBA, GL_UNSIGNED_SHORT_4_4_4_4, pixels);
			} else {
				U16 * expandedPixels = pixels;

				while (pixelCount--) {
					*expandedPixels++ = colors[*nibblePtr].ConvertTo565();
					++nibblePtr;
				}

				TexImage2D(target, level, GL_RGB, width, height, 0, 
						   GL_RGB, GL_UNSIGNED_SHORT_5_6_5, pixels);
			}

			if (width > 1)
				width >>= 1;

			if (height > 1) 
				height >>= 1;
		}
	} else {
		for (GLint level = minLevel; level <= maxLevel; ++level) {

			size_t pixelCount = width * height;

			if (hasAlpha) {
				U16 * expandedPixels = pixels;

				while (pixelCount--) {
					*expandedPixels++ = colors[*dataPtr++].ConvertTo4444();
				}

				TexImage2D(target, level, GL_RGBA, width, height, 0, 
						   GL_RGBA, GL_UNSIGNED_SHORT_4_4_4_4, pixels);
			} else {
				U16 * expandedPixels = pixels;

				while (pixelCount--) {
					*expandedPixels++ = colors[*dataPtr++].ConvertTo565();
				}

				TexImage2D(target, level, GL_RGB, width, height, 0, 
						   GL_RGB, GL_UNSIGNED_SHORT_5_6_5, pixels);
			}

			if (width > 1)
				width >>= 1;

			if (height > 1) 
				height >>= 1;
		}
	}

	delete[] colors;
	delete[] pixels;
}

void Context :: CompressedTexSubImage2D(GLenum target, GLint level, 
										GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, 
										GLsizei imageSize, const GLvoid *data) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	// "Specifies the format of the pixel data. Currently, there is no supported format"
	RecordError(GL_INVALID_OPERATION);
}


void Context :: TexImage2D(GLenum target, GLint level, GLint internalformat, 
						   GLsizei width, GLsizei height, GLint border, 
						   GLenum format, GLenum type, const GLvoid *pixels) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (level < 0 || level >= MultiTexture::MAX_LEVELS) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	RasterizerState::TextureFormat internalFormat = TextureFormatFromEnum(internalformat);
	RasterizerState::TextureFormat externalFormat = TextureFormatFromEnum(format);

	if (internalFormat == RasterizerState::TextureFormatInvalid ||
		externalFormat == RasterizerState::TextureFormatInvalid) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (border != 0 || width < 0 || height < 0 || !IsPowerOf2(width) || !IsPowerOf2(height)) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (!ValidateFormats(internalFormat, externalFormat, type)) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	MultiTexture * multiTexture = GetCurrentTexture();
	Texture * texture = multiTexture->GetTexture(level);
	
	if (!texture->Initialize(width, height, internalFormat)) {
		RecordError(GL_OUT_OF_MEMORY);
		return;
	}

	if (!level) {
		GetRasterizerState()->SetInternalFormat(m_ActiveTexture, internalFormat);
	}

	if (pixels != 0) {
		CopyPixels(const_cast<const void *>(pixels), width, height, 0, 0, width, height,
			texture->GetData(), width, height, 0, 0, internalFormat, type,
			InternalTypeForInternalFormat(internalFormat), m_PixelStoreUnpackAlignment, 1);
	}

	if (level == 0 && m_GenerateMipmaps) {
		UpdateMipmaps();
	}
}

void Context :: TexSubImage2D(GLenum target, GLint level, 
							  GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, 
							  GLenum format, GLenum type, const GLvoid *pixels) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (level < 0 || level >= MultiTexture::MAX_LEVELS) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	RasterizerState::TextureFormat externalFormat = TextureFormatFromEnum(format);

	if (externalFormat == RasterizerState::TextureFormatInvalid) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	MultiTexture * multiTexture = GetCurrentTexture();
	Texture * texture = multiTexture->GetTexture(level);

	if (!texture) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	if (xoffset < 0 || yoffset < 0 || width < 0 || height < 0 ||
		static_cast<U32>(xoffset + width) > texture->GetWidth() || 
		static_cast<U32>(yoffset + height) > texture->GetHeight()) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	RasterizerState::TextureFormat internalFormat = texture->GetInternalFormat();

	if (!ValidateFormats(internalFormat, externalFormat, type)) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	if (pixels != 0) {
		CopyPixels(const_cast<const void *>(pixels), width, height, 0, 0, width, height,
			texture->GetData(), texture->GetWidth(), texture->GetHeight(),
			xoffset, yoffset, internalFormat, type, InternalTypeForInternalFormat(internalFormat),
			m_PixelStoreUnpackAlignment, 1);
	}

	if (level == 0 && m_GenerateMipmaps) {
		UpdateMipmaps();
	}
}

void Context :: CopyTexImage2D(GLenum target, GLint level, GLenum internalformat, 
							   GLint x, GLint y, GLsizei width, GLsizei height, GLint border) { 
	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	Surface * readSurface = GetReadSurface();

	if (level < 0 || level >= MultiTexture::MAX_LEVELS) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	RasterizerState::TextureFormat internalFormat = TextureFormatFromEnum(internalformat);

	if (internalFormat == RasterizerState::TextureFormatInvalid) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (border != 0 || width < 0 || height < 0 || !IsPowerOf2(width) || !IsPowerOf2(height)) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	MultiTexture * multiTexture = GetCurrentTexture();
	Texture * texture = multiTexture->GetTexture(level);

	if (!texture->Initialize(width, height, internalFormat)) {
		RecordError(GL_OUT_OF_MEMORY);
		return;
	}

	bool result = CopySurfacePixels(readSurface, x, y, width, height,
				texture->GetData(), width, height, 0, 0, internalFormat, 
				InternalTypeForInternalFormat(internalFormat), 1);

	if (!result) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	if (level == 0 && m_GenerateMipmaps) {
		UpdateMipmaps();
	}
}

void Context :: CopyTexSubImage2D(GLenum target, GLint level, 
								  GLint xoffset, GLint yoffset, 
								  GLint x, GLint y, GLsizei width, GLsizei height) { 
	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	Surface * readSurface = GetReadSurface();

	if (level < 0 || level >= MultiTexture::MAX_LEVELS) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	MultiTexture * multiTexture = GetCurrentTexture();
	Texture * texture = multiTexture->GetTexture(level);

	if (!texture) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	if (xoffset < 0 || yoffset < 0 || width < 0 || height < 0 ||
		static_cast<U32>(xoffset + width) > texture->GetWidth() || 
		static_cast<U32>(yoffset + height) > texture->GetHeight()) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	RasterizerState::TextureFormat internalFormat = texture->GetInternalFormat();

	if (!ValidateFormats(internalFormat, RasterizerState::TextureFormatRGB565, GL_UNSIGNED_SHORT_5_6_5) &&
		!ValidateFormats(internalFormat, RasterizerState::TextureFormatRGBA8, GL_UNSIGNED_BYTE)) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	bool result = CopySurfacePixels(readSurface, x, y, width, height,
				texture->GetData(), texture->GetWidth(), texture->GetHeight(), xoffset, yoffset, 
				internalFormat, InternalTypeForInternalFormat(internalFormat), 1);

	if (!result) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (level == 0 && m_GenerateMipmaps) {
		UpdateMipmaps();
	}
}

// --------------------------------------------------------------------------
// Texture parameters
// --------------------------------------------------------------------------

void Context :: TexParameteri(GLenum target, GLenum pname, GLint param) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	MultiTexture * multiTexture = GetCurrentTexture();

	switch (pname) {
		case GL_TEXTURE_MIN_FILTER:
			{
				RasterizerState::FilterMode mode = MinFilterModeFromEnum(param);

				if (mode != RasterizerState::FilterModeInvalid) {
					multiTexture->SetMinFilterMode(mode);
					RasterizerState::FilterMode mipmapMode = MipmapFilterModeFromEnum(param);
					multiTexture->SetMipmapFilterMode(mipmapMode);
				} else {
					RecordError(GL_INVALID_ENUM);
				}
			}
			break;

		case GL_TEXTURE_MAG_FILTER:
			{
				RasterizerState::FilterMode mode = MagFilterModeFromEnum(param);

				if (mode != RasterizerState::FilterModeInvalid) {
					multiTexture->SetMagFilterMode(mode);
				} else {
					RecordError(GL_INVALID_ENUM);
				}
			}
			break;

		case GL_TEXTURE_WRAP_S:
			{
				RasterizerState::WrappingMode mode = WrappingModeFromEnum(param);

				if (mode != RasterizerState::WrappingModeInvalid) {
					multiTexture->SetWrappingModeS(mode);
				} else {
					RecordError(GL_INVALID_ENUM);
				}
			}
			break;

		case GL_TEXTURE_WRAP_T:
			{
				RasterizerState::WrappingMode mode = WrappingModeFromEnum(param);

				if (mode != RasterizerState::WrappingModeInvalid) {
					multiTexture->SetWrappingModeT(mode);
				} else {
					RecordError(GL_INVALID_ENUM);
				}
			}
			break;

		case GL_GENERATE_MIPMAP:
			{
				m_GenerateMipmaps = (param != 0);
			}

			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
	}
}

void Context :: TexParameterx(GLenum target, GLenum pname, GLfixed param) { 

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	TexParameteri(target, pname, param);
}

void Context :: TexEnvx(GLenum target, GLenum pname, GLfixed param) { 

	switch (target) {
	case GL_TEXTURE_ENV:
		switch (pname) {
			case GL_TEXTURE_ENV_MODE:
				switch (param) {
					case GL_MODULATE:
						GetRasterizerState()->SetTextureMode(m_ActiveTexture, RasterizerState::TextureModeModulate);
						break;

					case GL_REPLACE:
						GetRasterizerState()->SetTextureMode(m_ActiveTexture, RasterizerState::TextureModeReplace);
						break;

					case GL_DECAL:
						GetRasterizerState()->SetTextureMode(m_ActiveTexture, RasterizerState::TextureModeDecal);
						break;

					case GL_BLEND:
						GetRasterizerState()->SetTextureMode(m_ActiveTexture, RasterizerState::TextureModeBlend);
						break;

					case GL_ADD:
						GetRasterizerState()->SetTextureMode(m_ActiveTexture, RasterizerState::TextureModeAdd);
						break;

					case GL_COMBINE:
						GetRasterizerState()->SetTextureMode(m_ActiveTexture, RasterizerState::TextureModeCombine);
						break;

					default:
						RecordError(GL_INVALID_ENUM);
						break;
				}

				break;

			case GL_COMBINE_RGB:
				switch (param) {
				case GL_REPLACE:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineReplace);
					break;

				case GL_MODULATE:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineModulate);
					break;

				case GL_ADD:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineAdd);
					break;

				case GL_ADD_SIGNED:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineAddSigned);
					break;

				case GL_INTERPOLATE:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineInterpolate);
					break;

				case GL_SUBTRACT:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineSubtract);
					break;

				case GL_DOT3_RGB:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineDot3RGB);
					break;

				case GL_DOT3_RGBA:
					GetRasterizerState()->SetTextureCombineFuncRGB(m_ActiveTexture, RasterizerState::TextureModeCombineDot3RGBA);
					break;

				default:
					RecordError(GL_INVALID_ENUM);
				}

				break;

			case GL_COMBINE_ALPHA:
				switch (param) {
				case GL_REPLACE:
					GetRasterizerState()->SetTextureCombineFuncAlpha(m_ActiveTexture, RasterizerState::TextureModeCombineReplace);
					break;

				case GL_MODULATE:
					GetRasterizerState()->SetTextureCombineFuncAlpha(m_ActiveTexture, RasterizerState::TextureModeCombineModulate);
					break;

				case GL_ADD:
					GetRasterizerState()->SetTextureCombineFuncAlpha(m_ActiveTexture, RasterizerState::TextureModeCombineAdd);
					break;

				case GL_ADD_SIGNED:
					GetRasterizerState()->SetTextureCombineFuncAlpha(m_ActiveTexture, RasterizerState::TextureModeCombineAddSigned);
					break;

				case GL_INTERPOLATE:
					GetRasterizerState()->SetTextureCombineFuncAlpha(m_ActiveTexture, RasterizerState::TextureModeCombineInterpolate);
					break;

				case GL_SUBTRACT:
					GetRasterizerState()->SetTextureCombineFuncAlpha(m_ActiveTexture, RasterizerState::TextureModeCombineSubtract);
					break;

				default:
					RecordError(GL_INVALID_ENUM);
				}

				break;

			case GL_SRC0_RGB:
			case GL_SRC1_RGB:
			case GL_SRC2_RGB:
				{
					size_t index = pname - GL_SRC0_RGB;

					switch (param) {
					case GL_TEXTURE:
						GetRasterizerState()->SetTextureCombineSrcRGB(m_ActiveTexture, index, RasterizerState::TextureCombineSrcTexture);
						break;

					case GL_CONSTANT:
						GetRasterizerState()->SetTextureCombineSrcRGB(m_ActiveTexture, index, RasterizerState::TextureCombineSrcConstant);
						break;

					case GL_PRIMARY_COLOR:
						GetRasterizerState()->SetTextureCombineSrcRGB(m_ActiveTexture, index, RasterizerState::TextureCombineSrcPrimaryColor);
						break;

					case GL_PREVIOUS:
						GetRasterizerState()->SetTextureCombineSrcRGB(m_ActiveTexture, index, RasterizerState::TextureCombineSrcPrevious);
						break;

					default:
						RecordError(GL_INVALID_ENUM);
						break;
					}
				}

				break;

			case GL_SRC0_ALPHA:
			case GL_SRC1_ALPHA:
			case GL_SRC2_ALPHA:
				{
					size_t index = pname - GL_SRC0_ALPHA;

					switch (param) {
					case GL_TEXTURE:
						GetRasterizerState()->SetTextureCombineSrcAlpha(m_ActiveTexture, index, RasterizerState::TextureCombineSrcTexture);
						break;

					case GL_CONSTANT:
						GetRasterizerState()->SetTextureCombineSrcAlpha(m_ActiveTexture, index, RasterizerState::TextureCombineSrcConstant);
						break;

					case GL_PRIMARY_COLOR:
						GetRasterizerState()->SetTextureCombineSrcAlpha(m_ActiveTexture, index, RasterizerState::TextureCombineSrcPrimaryColor);
						break;

					case GL_PREVIOUS:
						GetRasterizerState()->SetTextureCombineSrcAlpha(m_ActiveTexture, index, RasterizerState::TextureCombineSrcPrevious);
						break;

					default:
						RecordError(GL_INVALID_ENUM);
						break;
					}
				}

				break;

			case GL_OPERAND0_RGB:
			case GL_OPERAND1_RGB:
			case GL_OPERAND2_RGB:
				{
					size_t index = pname - GL_OPERAND0_RGB;

					switch (param) {
					case GL_SRC_COLOR:
						GetRasterizerState()->SetTextureCombineOpRGB(m_ActiveTexture, index, RasterizerState::TextureCombineOpSrcColor);
						break;

					case GL_ONE_MINUS_SRC_COLOR:
						GetRasterizerState()->SetTextureCombineOpRGB(m_ActiveTexture, index, RasterizerState::TextureCombineOpOneMinusSrcColor);
						break;

					case GL_SRC_ALPHA:
						GetRasterizerState()->SetTextureCombineOpRGB(m_ActiveTexture, index, RasterizerState::TextureCombineOpSrcAlpha);
						break;

					case GL_ONE_MINUS_SRC_ALPHA:
						GetRasterizerState()->SetTextureCombineOpRGB(m_ActiveTexture, index, RasterizerState::TextureCombineOpOneMinusSrcAlpha);
						break;

					default:
						RecordError(GL_INVALID_ENUM);
					}
				}

				break;

			case GL_OPERAND0_ALPHA:
			case GL_OPERAND1_ALPHA:
			case GL_OPERAND2_ALPHA:
				{
					size_t index = pname - GL_OPERAND0_ALPHA;

					switch (param) {
					case GL_SRC_ALPHA:
						GetRasterizerState()->SetTextureCombineOpAlpha(m_ActiveTexture, index, RasterizerState::TextureCombineOpSrcAlpha);
						break;

					case GL_ONE_MINUS_SRC_ALPHA:
						GetRasterizerState()->SetTextureCombineOpAlpha(m_ActiveTexture, index, RasterizerState::TextureCombineOpOneMinusSrcAlpha);
						break;

					default:
						RecordError(GL_INVALID_ENUM);
					}
				}

				break;

			case GL_RGB_SCALE:
				GetRasterizerState()->SetTextureScaleRGB(m_ActiveTexture, param);
				break;

			case GL_ALPHA_SCALE:
				GetRasterizerState()->SetTextureScaleAlpha(m_ActiveTexture, param);
				break;

			default:
				RecordError(GL_INVALID_ENUM);
				break;
		}

		break;

	case GL_POINT_SPRITE_OES:
		switch (pname) {
		case GL_COORD_REPLACE_OES:
			GetRasterizerState()->SetPointCoordReplaceEnabled(m_ActiveTexture, param != 0);
			break;

		default:
			RecordError(GL_INVALID_VALUE);
		}
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		break;
	}
}

void Context :: TexEnvi(GLenum target, GLenum pname, GLint param) { 

	TexEnvx(target, pname, param);

}

void Context :: TexEnviv(GLenum target, GLenum pname, const GLint* params) { 

	TexEnvxv(target, pname, (const GLfixed*)params);

}

void Context :: TexEnvxv(GLenum target, GLenum pname, const GLfixed *params) { 

	switch (target) {
	case GL_TEXTURE_ENV:
		switch (pname) {
			case GL_TEXTURE_ENV_COLOR:
				GetRasterizerState()->SetTexEnvColor(m_ActiveTexture, FractionalColor(params));
				break;

			default:
				TexEnvx(target, pname, *params);
				break;
		}

		break;

	case GL_POINT_SPRITE_OES:
		TexEnvx(target, pname, *params);
		break;

	default:
		RecordError(GL_INVALID_ENUM);
		break;
	}

}

// --------------------------------------------------------------------------
// The following methods are not really implemented; we only have a single 
// texture unit
// --------------------------------------------------------------------------

void Context :: ActiveTexture(GLenum texture) { 
	if (texture < GL_TEXTURE0 || texture >= GL_TEXTURE0 + EGL_NUM_TEXTURE_UNITS) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	m_ActiveTexture = texture - GL_TEXTURE0;

	if (m_MatrixMode == GL_TEXTURE)
	{
		m_CurrentMatrixStack = &m_TextureMatrixStack[m_ActiveTexture];
	}
}


void Context :: ClientActiveTexture(GLenum texture) { 
	if (texture < GL_TEXTURE0 || texture >= GL_TEXTURE0 + EGL_NUM_TEXTURE_UNITS) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	m_ClientActiveTexture = texture - GL_TEXTURE0;
}

// --------------------------------------------------------------------------
// Pixel transfer to bitmap - basically the inverse of the texture copy
// functions
// --------------------------------------------------------------------------

void Context :: ReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, 
						   GLenum format, GLenum type, GLvoid *pixels) { 

   // right now, use a hardcoded image format
	if (format != GL_RGBA && format != GL_RGB) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (type != GL_UNSIGNED_BYTE && type != GL_UNSIGNED_SHORT_5_6_5) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	if (width < 0 || height < 0) {
		RecordError(GL_INVALID_VALUE);
		return;
	}

	if (format == GL_RGBA && type != GL_UNSIGNED_BYTE ||
		format == GL_RGB && type != GL_UNSIGNED_SHORT_5_6_5) {
		RecordError(GL_INVALID_OPERATION);
		return;
	}

	RasterizerState::TextureFormat externalFormat = TextureFormatFromEnum(format);

	Surface * readSurface = GetReadSurface();

	bool result = CopySurfacePixels(readSurface, 
				x, y, width, height,
				pixels, width, height, 0, 0, 
				externalFormat, type, m_PixelStorePackAlignment);
	
	if (!result) {
		RecordError(GL_INVALID_VALUE);
	}

}


void Context :: PixelStorei(GLenum pname, GLint param) { 
	switch (pname) {
		case GL_UNPACK_ALIGNMENT:
			switch (param) {
			case 1:
			case 2:
			case 4:
			case 8:
				m_PixelStoreUnpackAlignment = param;
				break;

			default:
				RecordError(GL_INVALID_VALUE);
				break;
			}

			break;

		case GL_PACK_ALIGNMENT:
			switch (param) {
			case 1:
			case 2:
			case 4:
			case 8:
				m_PixelStorePackAlignment = param;
				break;

			default:
				RecordError(GL_INVALID_VALUE);
				break;
			}

			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
	}
}


namespace {
	U32 Log(U32 value) {
		U32 result = 0;
		U32 mask = 1;

		while ((value & mask) != value) {
			++result;
			mask = (mask << 1) | 1;
		}

		return result;
	}
}


void Context :: UpdateMipmaps(void) {

	MultiTexture * multiTexture = GetCurrentTexture();
	Texture * texture = multiTexture->GetTexture(0);

	U32 logWidth = Log(texture->GetWidth());
	U32 logHeight = Log(texture->GetHeight());

	U32 logSquareBound = logWidth < logHeight ? logWidth : logHeight;
	U32 level;

	for (level = 1; level <= logSquareBound; ++level) {
		Texture * outer = multiTexture->GetTexture(level - 1);
		Texture * inner = multiTexture->GetTexture(level);
			
		inner->Initialize(outer->GetWidth() / 2, 
			outer->GetHeight() / 2, outer->GetInternalFormat());

		size_t x, y;
		size_t width = inner->GetWidth();
		size_t height = inner->GetHeight();

		switch (outer->GetInternalFormat()) {
		case RasterizerState::TextureFormatAlpha:
			{
				U8 * outerBase = reinterpret_cast<U8 *>(outer->GetData());
				U8 * innerBase = reinterpret_cast<U8 *>(inner->GetData());

				for (y = 0; y < height; ++y) {
					for (x = 0; x < width; ++x) {
						Color a = Color(0xff, 0xff, 0xff, outerBase[2 * x + (2 * width) * 2 * y]);
						Color b = Color(0xff, 0xff, 0xff, outerBase[2 * x + 1 + (2 * width) * 2 * y]);
						Color c = Color(0xff, 0xff, 0xff, outerBase[2 * x + (2 * width) * (2 * y + 1)]);
						Color d = Color(0xff, 0xff, 0xff, outerBase[2 * x + 1+ (2 * width) * (2 * y + 1)]);

						Color target = Color::Average(a, b, c, d);
						innerBase[x + width * y] = target.A();
					}
				}
			}
			break;

		case RasterizerState::TextureFormatLuminance:
			{
				U8 * outerBase = reinterpret_cast<U8 *>(outer->GetData());
				U8 * innerBase = reinterpret_cast<U8 *>(inner->GetData());

				for (y = 0; y < height; ++y) {
					for (x = 0; x < width; ++x) {
						Color a = Color(outerBase[2 * x + (2 * width) * 2 * y], 0xff, 0xff, 0xff);
						Color b = Color(outerBase[2 * x + 1 + (2 * width) * 2 * y], 0xff, 0xff, 0xff);
						Color c = Color(outerBase[2 * x + (2 * width) * (2 * y + 1)], 0xff, 0xff, 0xff);
						Color d = Color(outerBase[2 * x + 1+ (2 * width) * (2 * y + 1)], 0xff, 0xff, 0xff);

						Color target = Color::Average(a, b, c, d);
						innerBase[x + width * y] = target.R();
					}
				}
			}
			break;

		case RasterizerState::TextureFormatLuminanceAlpha:
			{
				U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
				U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

				for (y = 0; y < height; ++y) {
					for (x = 0; x < width; ++x) {
						Color a = Color::FromLuminanceAlpha(outerBase[2 * x + (2 * width) * 2 * y]);
						Color b = Color::FromLuminanceAlpha(outerBase[2 * x + 1 + (2 * width) * 2 * y]);
						Color c = Color::FromLuminanceAlpha(outerBase[2 * x + (2 * width) * (2 * y + 1)]);
						Color d = Color::FromLuminanceAlpha(outerBase[2 * x + 1+ (2 * width) * (2 * y + 1)]);

						Color target = Color::Average(a, b, c, d);
						innerBase[x + width * y] = target.R() | target.A() << 8;
					}
				}
			}
			break;

		case RasterizerState::TextureFormatRGB565:
			{
				U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
				U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

				for (y = 0; y < height; ++y) {
					for (x = 0; x < width; ++x) {
						Color a = Color::From565(outerBase[2 * x + (2 * width) * 2 * y]);
						Color b = Color::From565(outerBase[2 * x + 1 + (2 * width) * 2 * y]);
						Color c = Color::From565(outerBase[2 * x + (2 * width) * (2 * y + 1)]);
						Color d = Color::From565(outerBase[2 * x + 1+ (2 * width) * (2 * y + 1)]);

						Color target = Color::Average(a, b, c, d);
						innerBase[x + width * y] = target.ConvertTo565();
					}
				}
			}
			break;

		case RasterizerState::TextureFormatRGBA5551:
			{
				U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
				U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

				for (y = 0; y < height; ++y) {
					for (x = 0; x < width; ++x) {
						Color a = Color::From5551(outerBase[2 * x + (2 * width) * 2 * y]);
						Color b = Color::From5551(outerBase[2 * x + 1 + (2 * width) * 2 * y]);
						Color c = Color::From5551(outerBase[2 * x + (2 * width) * (2 * y + 1)]);
						Color d = Color::From5551(outerBase[2 * x + 1+ (2 * width) * (2 * y + 1)]);

						Color target = Color::Average(a, b, c, d);
						innerBase[x + width * y] = target.ConvertTo5551();
					}
				}
			}
			break;

		case RasterizerState::TextureFormatRGBA4444:
			{
				U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
				U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

				for (y = 0; y < height; ++y) {
					for (x = 0; x < width; ++x) {
						Color a = Color::From4444(outerBase[2 * x + (2 * width) * 2 * y]);
						Color b = Color::From4444(outerBase[2 * x + 1 + (2 * width) * 2 * y]);
						Color c = Color::From4444(outerBase[2 * x + (2 * width) * (2 * y + 1)]);
						Color d = Color::From4444(outerBase[2 * x + 1+ (2 * width) * (2 * y + 1)]);

						Color target = Color::Average(a, b, c, d);
						innerBase[x + width * y] = target.ConvertTo4444();
					}
				}
			}
			break;

		default:
			assert(0);
		}
	}

	if (logWidth < logHeight) {
		assert(multiTexture->GetTexture(level - 1)->GetWidth() == 1);

		for (; level < logHeight; ++level) {
			Texture * outer = multiTexture->GetTexture(level - 1);
			Texture * inner = multiTexture->GetTexture(level);
				
			inner->Initialize(outer->GetWidth(), 
				outer->GetHeight() / 2, outer->GetInternalFormat());

			size_t x, y;
			size_t width = inner->GetWidth();
			size_t height = inner->GetHeight();

			switch (outer->GetInternalFormat()) {
			case RasterizerState::TextureFormatAlpha:
				{
					U8 * outerBase = reinterpret_cast<U8 *>(outer->GetData());
					U8 * innerBase = reinterpret_cast<U8 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color(0xff, 0xff, 0xff, outerBase[x + width * 2 * y]);
							Color b = Color(0xff, 0xff, 0xff, outerBase[x + width * (2 * y + 1)]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.A();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatLuminance:
				{
					U8 * outerBase = reinterpret_cast<U8 *>(outer->GetData());
					U8 * innerBase = reinterpret_cast<U8 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color(outerBase[x + width * 2 * y], 0xff, 0xff, 0xff);
							Color b = Color(outerBase[x + width * (2 * y + 1)], 0xff, 0xff, 0xff);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.R();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatLuminanceAlpha:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::FromLuminanceAlpha(outerBase[x + width * 2 * y]);
							Color b = Color::FromLuminanceAlpha(outerBase[x + width * (2 * y + 1)]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.R() | target.A() << 8;
						}
					}
				}
				break;

			case RasterizerState::TextureFormatRGB565:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::From565(outerBase[x + width * 2 * y]);
							Color b = Color::From565(outerBase[x + width * (2 * y + 1)]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.ConvertTo565();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatRGBA5551:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::From5551(outerBase[x + width * 2 * y]);
							Color b = Color::From5551(outerBase[x + width * (2 * y + 1)]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.ConvertTo5551();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatRGBA4444:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::From4444(outerBase[x + width * 2 * y]);
							Color b = Color::From4444(outerBase[x + width * (2 * y + 1)]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.ConvertTo4444();
						}
					}
				}
				break;

			default:
				assert(0);
			}
		}
	} else if (logWidth > logHeight) {
		assert(multiTexture->GetTexture(level - 1)->GetHeight() == 1);

		for (; level < logWidth; ++level) {
			Texture * outer = multiTexture->GetTexture(level - 1);
			Texture * inner = multiTexture->GetTexture(level);
				
			inner->Initialize(outer->GetWidth() / 2, 
				outer->GetHeight(), outer->GetInternalFormat());

			size_t x, y;
			size_t width = inner->GetWidth();
			size_t height = inner->GetHeight();

			switch (outer->GetInternalFormat()) {
			case RasterizerState::TextureFormatAlpha:
				{
					U8 * outerBase = reinterpret_cast<U8 *>(outer->GetData());
					U8 * innerBase = reinterpret_cast<U8 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color(0xff, 0xff, 0xff, outerBase[2 * x + (2 * width) * y]);
							Color b = Color(0xff, 0xff, 0xff, outerBase[2 * x + 1 + (2 * width) * y]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.A();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatLuminance:
				{
					U8 * outerBase = reinterpret_cast<U8 *>(outer->GetData());
					U8 * innerBase = reinterpret_cast<U8 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color(outerBase[2 * x + (2 * width) * y], 0xff, 0xff, 0xff);
							Color b = Color(outerBase[2 * x + 1 + (2 * width) * y], 0xff, 0xff, 0xff);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.R();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatLuminanceAlpha:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::FromLuminanceAlpha(outerBase[2 * x + (2 * width) * y]);
							Color b = Color::FromLuminanceAlpha(outerBase[2 * x + 1 + (2 * width) * y]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.R() | target.A() << 8;
						}
					}
				}
				break;

			case RasterizerState::TextureFormatRGB565:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::From565(outerBase[2 * x + (2 * width) * y]);
							Color b = Color::From565(outerBase[2 * x + 1 + (2 * width) * y]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.ConvertTo565();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatRGBA5551:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::From5551(outerBase[2 * x + (2 * width) * y]);
							Color b = Color::From5551(outerBase[2 * x + 1 + (2 * width) * y]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.ConvertTo5551();
						}
					}
				}
				break;

			case RasterizerState::TextureFormatRGBA4444:
				{
					U16 * outerBase = reinterpret_cast<U16 *>(outer->GetData());
					U16 * innerBase = reinterpret_cast<U16 *>(inner->GetData());

					for (y = 0; y < height; ++y) {
						for (x = 0; x < width; ++x) {
							Color a = Color::From4444(outerBase[2 * x + (2 * width) * y]);
							Color b = Color::From4444(outerBase[2 * x + 1 + (2 * width) * y]);

							Color target = Color::Average(a, b);
							innerBase[x + width * y] = target.ConvertTo4444();
						}
					}
				}
				break;

			default:
				assert(0);
			}
		}
	}
}


void Context :: GetTexEnviv(GLenum env, GLenum pname, GLint *params) {

	switch (env) {
	case GL_TEXTURE_ENV:
		switch (pname) {
		case GL_TEXTURE_ENV_MODE:
			{
				switch (m_RasterizerState.GetTextureMode(m_ActiveTexture)) {
				case RasterizerState::TextureModeModulate:	params[0] = GL_MODULATE;	break;
				case RasterizerState::TextureModeReplace:	params[0] = GL_REPLACE;		break;
				case RasterizerState::TextureModeDecal:		params[0] = GL_DECAL;		break;
				case RasterizerState::TextureModeBlend:		params[0] = GL_BLEND;		break;
				case RasterizerState::TextureModeAdd:		params[0] = GL_ADD;			break;
				case RasterizerState::TextureModeCombine:	params[0] = GL_COMBINE;		break;
				}
			}

			break;

		case GL_COMBINE_RGB:
			{
				switch (m_RasterizerState.GetTextureCombineFuncRGB(m_ActiveTexture)) {
				case RasterizerState::TextureModeCombineReplace:	params[0] = GL_REPLACE;		break;
				case RasterizerState::TextureModeCombineModulate:	params[0] = GL_MODULATE;	break;
				case RasterizerState::TextureModeCombineAdd:		params[0] = GL_ADD;			break;
				case RasterizerState::TextureModeCombineAddSigned:	params[0] = GL_ADD_SIGNED;	break;
				case RasterizerState::TextureModeCombineInterpolate:params[0] = GL_INTERPOLATE;	break;
				case RasterizerState::TextureModeCombineSubtract:	params[0] = GL_SUBTRACT;	break;
				case RasterizerState::TextureModeCombineDot3RGB:	params[0] = GL_DOT3_RGB;	break;
				case RasterizerState::TextureModeCombineDot3RGBA:	params[0] = GL_DOT3_RGBA;	break;
				}
			}

			break;

		case GL_COMBINE_ALPHA:
			{
				switch (m_RasterizerState.GetTextureCombineFuncAlpha(m_ActiveTexture)) {
				case RasterizerState::TextureModeCombineReplace:	params[0] = GL_REPLACE;		break;
				case RasterizerState::TextureModeCombineModulate:	params[0] = GL_MODULATE;	break;
				case RasterizerState::TextureModeCombineAdd:		params[0] = GL_ADD;			break;
				case RasterizerState::TextureModeCombineAddSigned:	params[0] = GL_ADD_SIGNED;	break;
				case RasterizerState::TextureModeCombineInterpolate:params[0] = GL_INTERPOLATE;	break;
				case RasterizerState::TextureModeCombineSubtract:	params[0] = GL_SUBTRACT;	break;
				}
			}

			break;

		case GL_SRC0_RGB:
		case GL_SRC1_RGB:
		case GL_SRC2_RGB:
			{
				size_t index = pname - GL_SRC0_RGB;
				RasterizerState::TextureCombineSrc src = m_RasterizerState.GetTextureCombineSrcRGB(m_ActiveTexture, index);

				switch (src) {
				case RasterizerState::TextureCombineSrcTexture:		params[0] = GL_TEXTURE;			break;
				case RasterizerState::TextureCombineSrcConstant:	params[0] = GL_CONSTANT;		break;
				case RasterizerState::TextureCombineSrcPrimaryColor:params[0] = GL_PRIMARY_COLOR;	break;
				case RasterizerState::TextureCombineSrcPrevious:	params[0] = GL_PREVIOUS;		break;
				}
			}

			break;

		case GL_SRC0_ALPHA:
		case GL_SRC1_ALPHA:
		case GL_SRC2_ALPHA:
			{
				size_t index = pname - GL_SRC0_ALPHA;
				RasterizerState::TextureCombineSrc src = m_RasterizerState.GetTextureCombineSrcAlpha(m_ActiveTexture, index);

				switch (src) {
				case RasterizerState::TextureCombineSrcTexture:		params[0] = GL_TEXTURE;			break;
				case RasterizerState::TextureCombineSrcConstant:	params[0] = GL_CONSTANT;		break;
				case RasterizerState::TextureCombineSrcPrimaryColor:params[0] = GL_PRIMARY_COLOR;	break;
				case RasterizerState::TextureCombineSrcPrevious:	params[0] = GL_PREVIOUS;		break;
				}
			}

			break;

		case GL_OPERAND0_RGB:
		case GL_OPERAND1_RGB:
		case GL_OPERAND2_RGB:
			{
				size_t index = pname - GL_OPERAND0_RGB;
				RasterizerState::TextureCombineOp op = m_RasterizerState.GetTextureCombineOpRGB(m_ActiveTexture, index);

				switch (op) {
				case RasterizerState::TextureCombineOpSrcColor:			params[0] = GL_SRC_COLOR;			break;
				case RasterizerState::TextureCombineOpOneMinusSrcColor:	params[0] = GL_ONE_MINUS_SRC_COLOR;	break;
				case RasterizerState::TextureCombineOpSrcAlpha:			params[0] = GL_SRC_ALPHA;			break;
				case RasterizerState::TextureCombineOpOneMinusSrcAlpha:	params[0] = GL_ONE_MINUS_SRC_ALPHA;	break;
				}
			}

			break;

		case GL_OPERAND0_ALPHA:
		case GL_OPERAND1_ALPHA:
		case GL_OPERAND2_ALPHA:
			{
				size_t index = pname - GL_OPERAND0_ALPHA;
				RasterizerState::TextureCombineOp op = m_RasterizerState.GetTextureCombineOpAlpha(m_ActiveTexture, index);

				switch (op) {
				case RasterizerState::TextureCombineOpSrcAlpha:			params[0] = GL_SRC_ALPHA;			break;
				case RasterizerState::TextureCombineOpOneMinusSrcAlpha:	params[0] = GL_ONE_MINUS_SRC_ALPHA;	break;
				}
			}

			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
		}

		break;

	case GL_POINT_SPRITE_OES:
		switch (pname) {
		case GL_COORD_REPLACE_OES:
			params[0] = GetRasterizerState()->IsPointCoordReplaceEnabled(m_ActiveTexture);
			break;

		default:
			RecordError(GL_INVALID_ENUM);
			break;
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		break;
	}
}

bool Context :: GetTexEnvxv(GLenum env, GLenum pname, GLfixed *params) {

	switch (env) {
	case GL_TEXTURE_ENV:
		switch (pname) {
		case GL_TEXTURE_ENV_COLOR:
			CopyColor(m_RasterizerState.GetTexEnvColor(m_ActiveTexture), params);
			break;

		case GL_RGB_SCALE:
			params[0] = m_RasterizerState.GetTextureScaleRGB(m_ActiveTexture);
			break;

		case GL_ALPHA_SCALE:
			params[0] = m_RasterizerState.GetTextureScaleAlpha(m_ActiveTexture);
			break;

		default:
			RecordError(GL_INVALID_ENUM);
			return false;
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return false;
	}

	return true;
}

void Context :: GetTexParameteriv(GLenum target, GLenum pname, GLint *params) {

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

	MultiTexture * multiTexture = GetCurrentTexture();

	switch (pname) {
	case GL_GENERATE_MIPMAP:
		params[0] = m_GenerateMipmaps != 0;
		break;

	case GL_TEXTURE_MAG_FILTER:
		if (multiTexture->GetMagFilterMode() == RasterizerState::FilterModeNearest) {
			params[0] = GL_NEAREST;
		} else {
			params[0] = GL_LINEAR;
		}

		break;

	case GL_TEXTURE_MIN_FILTER:
		if (multiTexture->GetMinFilterMode() == RasterizerState::FilterModeNearest) {
			if (multiTexture->GetMipmapFilterMode() == RasterizerState::FilterModeNone) {
				params[0] = GL_NEAREST;
			} else if (multiTexture->GetMipmapFilterMode() == RasterizerState::FilterModeNearest) {
				params[0] = GL_NEAREST_MIPMAP_NEAREST;
			} else {
				params[0] = GL_NEAREST_MIPMAP_LINEAR;
			}
		} else {
			if (multiTexture->GetMipmapFilterMode() == RasterizerState::FilterModeNone) {
				params[0] = GL_LINEAR;
			} else if (multiTexture->GetMipmapFilterMode() == RasterizerState::FilterModeNearest) {
				params[0] = GL_LINEAR_MIPMAP_NEAREST;
			} else {
				params[0] = GL_LINEAR_MIPMAP_LINEAR;
			}
		}

		break;

	case GL_TEXTURE_WRAP_S:
		if (multiTexture->GetWrappingModeS() == RasterizerState::WrappingModeClampToEdge) {
			params[0] = GL_CLAMP_TO_EDGE;
		} else {
			params[0] = GL_REPEAT;
		}

		break;

	case GL_TEXTURE_WRAP_T:
		if (multiTexture->GetWrappingModeT() == RasterizerState::WrappingModeClampToEdge) {
			params[0] = GL_CLAMP_TO_EDGE;
		} else {
			params[0] = GL_REPEAT;
		}

		break;

	default:
		RecordError(GL_INVALID_ENUM);
		return;
	}

}


void Context :: GetTexParameterxv(GLenum target, GLenum pname, GLfixed *params) {

	if (target != GL_TEXTURE_2D) {
		RecordError(GL_INVALID_ENUM);
		return;
	}

#if 0
	switch (pname) {

	default:
#endif
		RecordError(GL_INVALID_ENUM);
#if 0
		break;
	}
#endif
}

void Context :: TexParameterfv(GLenum target, GLenum pname, const GLfloat *params) {

	TexParameteri(target, pname, (GLint)params[0]);

}

void Context :: TexParameteriv(GLenum target, GLenum pname, const GLint *params) {

	TexParameteri(target, pname, params[0]);

}

void Context :: TexParameterxv(GLenum target, GLenum pname, const GLfixed *params) {

	TexParameteri(target, pname, params[0]);

}

