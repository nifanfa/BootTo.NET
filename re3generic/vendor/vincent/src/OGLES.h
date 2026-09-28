#ifndef EGL_OGLES_H
#define EGL_OGLES_H 1

// ==========================================================================
//
// OGLES.h		Base include file for 3D Rendering Library
//
// --------------------------------------------------------------------------
//
// 10-02-2003	Hans-Martin Will	initial version
// 12-17-2003	Hans-Martin Will	added configuration information
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


#ifdef EGL_ON_WINCE
	#ifndef STATIC_GLES
		#	ifdef OGLES_EXPORTS
		#		define OGLES_API __declspec(dllexport)
		#	else
		#		define OGLES_API __declspec(dllimport)
		#	endif
	#else
		#define OGLES_API 
	#endif
#endif

#ifdef EGL_ON_SYMBIAN
#   define EGL_USE_TOP_DOWN_SURFACE
#	ifdef OGLES_EXPORTS
#		define OGLES_API EXPORT_C
#	else
#		define OGLES_API IMPORT_C
#	endif
#endif

#ifdef EGL_ON_LINUX
#  define OGLES_API
#if !defined(APIENTRY) && !defined(_WIN32)
#  define APIENTRY
#endif
#endif

#if (defined(ARM) || defined(_ARM_) || defined(__MARM__)) && !defined(EGL_NO_COMPILE)
#	define EGL_USE_JIT	1
#else
#	define EGL_USE_JIT  0
#endif


#ifndef EGL_RELEASE
#	define EGL_RELEASE				"x.yy"
#endif

#define EGL_CONFIG_VENDOR			"Hans-Martin Will"
#define EGL_CONFIG_VERSION			"OpenGL ES-CM 1.1"
#define EGL_CONFIG_EXTENSIONS		"GL_OES_fixed_point "\
									"GL_OES_single_precision "\
									"GL_OES_read_format "\
									"GL_OES_query_matrix "\
									"GL_OES_matrix_palette "\
									"GL_OES_point_size_array "\
									"GL_OES_point_sprite "\
									"GL_OES_compressed_paletted_texture"

#ifdef EGL_USE_GPP
#	define EGL_CONFIG_RENDERER		"Software GPP"
#else
#	define EGL_CONFIG_RENDERER		"Software"
#endif

#if EGL_USE_JIT
#	define	EGL_JIT_VERSION			" JIT"
#else
#	define	EGL_JIT_VERSION			" No JIT"
#endif

#define EGL_VERSION_NUMBER			EGL_RELEASE " " EGL_CONFIG_RENDERER EGL_JIT_VERSION

#define EGL_NUM_TEXTURE_UNITS		2

#define EGL_MIPMAP_PER_TEXEL		1

// --------------------------------------------------------------------------
// type definitions
// --------------------------------------------------------------------------

#ifndef EGL_USE_GPP		// define only if not including Intel's GPP library

#	ifdef EGL_ON_SYMBIAN
	    typedef TInt8	 I8;
        typedef TUint8  U8;
        typedef TInt16  I16;
        typedef TUint16 U16;
        typedef TInt    I32;
        typedef TUint   U32;
#	else
	    typedef signed char 		I8;
	    typedef unsigned char 		U8;
	    typedef short 				I16;
	    typedef unsigned short 		U16;
	    typedef int 				I32;
	    typedef unsigned int 		U32;
#	endif
#endif

#ifdef EGL_XSCALE
#define EGL_USE_CLZ
#endif

#ifdef EGL_ON_WINCE
typedef unsigned __int64	U64;
typedef __int64				I64;
#endif

#ifdef EGL_ON_LINUX
typedef unsigned long long U64;
typedef long long          I64;
#if !defined(INVALID_HANDLE_VALUE) && !defined(_WIN32)
#define INVALID_HANDLE_VALUE NULL
#endif
#endif

#ifdef EGL_ON_SYMBIAN

typedef TInt64X	U64;
typedef TInt64X	I64;


namespace EGL {
	class Context;
    class Config;

	struct TlsInfo {
        TlsInfo();
        ~TlsInfo();
		Context * m_Context;
        Config* m_AllConfigurations;
		I32 m_LastError;
	};
}


#endif

// --------------------------------------------------------------------------
// Memory helper macros
// --------------------------------------------------------------------------

// number of elements of an array
#define elementsof(a) (sizeof(a) / sizeof(*(a)))

// offset in bytes of a structure member from the base address
#ifndef offsetof
#define offsetof(s,m)   (size_t)&(((s *)0)->m)
#endif


#endif // ndef EGL_OGLES_H
