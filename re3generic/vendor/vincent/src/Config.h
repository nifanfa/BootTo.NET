#ifndef EGL_CONFIG_H
#define EGL_CONFIG_H 1

// ==========================================================================
//
// Config.h			Configuration Management Class
//					for 3D Rendering Library
//
// --------------------------------------------------------------------------
//
// 08-14-2003		Hans-Martin Will	initial version
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
#include "GLES/egl.h"


namespace EGL {

	class Context;

	class Config {

#if defined(EGL_ON_LINUX)
		friend class Context;
#else
		friend Context;
#endif

	public:
		Config(
			EGLint	bufferSize,
			EGLint	redSize,
			EGLint	greenSize,
			EGLint	blueSize,
			EGLint	alphaSize,
			EGLint	configCaveat,
			EGLint	configID,
			EGLint	depthSize,
			EGLint	level,
			EGLint	maxPBufferWidth,
			EGLint	maxPBufferHeight,
			EGLint	mxPBufferPixels,
			EGLint	nativeRenderable,
			EGLint	nativeVisualID,
			EGLint	nativeVisualType,
			EGLint	sampleBuffers,
			EGLint	samples,
			EGLint	stencilSize,
			EGLint	surfaceType,
			EGLint	transparentType,
			EGLint	transparentRedValue,
			EGLint	transparentGreenValue,
			EGLint	transparentBlueValue,
			EGLint  width,
			EGLint	height
			);

		OGLES_API Config() { }
		Config (const Config &other, const EGLint * attribList = 0, const EGLint * validAttributes = 0);
		Config & operator=(const Config & other);

		EGLint GetConfigAttrib(EGLint attribute) const;
		void SetConfigAttrib(EGLint attribute, EGLint value);

		static EGLBoolean GetConfigs(EGLConfig * result, EGLint configSize, EGLint * numConfig);
		static EGLBoolean ChooseConfig(const EGLint * attribList, EGLConfig * result, EGLint configSize, EGLint * numConfig);

	private:
		bool Matches(EGLint attribList);

		static bool IsValidAttribute(const EGLint * validAttributes, EGLint attribute);

	private:
		EGLint	m_BufferSize;
		EGLint	m_RedSize;
		EGLint	m_GreenSize;
		EGLint	m_BlueSize;
		EGLint	m_AlphaSize;
		EGLint	m_ConfigCaveat;
		EGLint	m_ConfigID;
		EGLint	m_DepthSize;
		EGLint	m_Level;
		EGLint	m_MaxPBufferWidth;
		EGLint	m_MaxPBufferHeight;
		EGLint	m_MaxPBufferPixels;
		EGLint	m_NativeRenderable;
		EGLint	m_NativeVisualID;
		EGLint	m_NativeVisualType;
		EGLint	m_SampleBuffers;
		EGLint	m_Samples;
		EGLint	m_StencilSize;
		EGLint	m_SurfaceType;
		EGLint	m_TransparentType;
		EGLint	m_TransparentRedValue;
		EGLint	m_TransparentGreenValue;
		EGLint	m_TransparentBlueValue;
		EGLint  m_Width;
		EGLint	m_Height;
	};
}

#endif // ndef EGL_CONFIG_H
