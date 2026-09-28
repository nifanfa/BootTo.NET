// ==========================================================================
//
// Config.cpp		Configuration Management Class
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


#include "stdafx.h"
#include "Config.h"


using namespace EGL;

// --------------------------------------------------------------------------
// Table of Supported Configurations
// --------------------------------------------------------------------------
namespace {
    const EGL::Config s_AllConfigurations[] = {
   	    // ----------------------------------------------------------------------
	    // Initial default configuration 
	    // RGB 565, as PBuffer or Windows surface
	    // ----------------------------------------------------------------------
	    Config(
		    32,				//	EGLint	bufferSize,
		    5,				//	EGLint	redSize,
		    6,				//	EGLint	greenSize,
		    5,				//	EGLint	blueSize,
		    8,				//	EGLint	alphaSize,
		    EGL_SLOW_CONFIG,//	EGLint	configCaveat,
		    1,				//	EGLint	configID,
		    16,				//	EGLint	depthSize,
		    0,				//	EGLint	level,
		    1024,			//	EGLint	maxPBufferWidth,
		    1024,			//	EGLint	maxPBufferHeight,
		    1024 * 1024,	//	EGLint	mxPBufferPixels,
		    EGL_FALSE,		//	EGLint	nativeRenderable,
		    0,				//	EGLint	nativeVisualID,
		    EGL_NONE,		//	EGLint	nativeVisualType,
		    0,				//	EGLint	sampleBuffers,
		    0,				//	EGLint	samples,
		    32,				//	EGLint	stencilSize,
		    EGL_PBUFFER_BIT | EGL_WINDOW_BIT,//	EGLint	surfaceType,
		    EGL_NONE,		//	EGLint	transparentType,
		    0,				//	EGLint	transparentRedValue,
		    0,				//	EGLint	transparentGreenValue,
		    0,				//	EGLint	transparentBlueValue,
		    240,			//  EGLint	width,
		    320				//  EGLint	height
	    )
    };

    // total number of supported configurations
    const int s_NumConfigurations = 1;
};

Config :: Config(
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
	EGLint	width,
	EGLint	height):
	m_BufferSize(bufferSize),
	m_RedSize(redSize),
	m_GreenSize(greenSize),
	m_BlueSize(blueSize),
	m_AlphaSize(alphaSize),
	m_ConfigCaveat(configCaveat),
	m_ConfigID(configID),
	m_DepthSize(depthSize),
	m_Level(level),
	m_MaxPBufferWidth(maxPBufferWidth),
	m_MaxPBufferHeight(maxPBufferHeight),
	m_MaxPBufferPixels(mxPBufferPixels),
	m_NativeRenderable(nativeRenderable),
	m_NativeVisualID(nativeVisualID),
	m_NativeVisualType(nativeVisualType),
	m_SampleBuffers(sampleBuffers),
	m_Samples(samples),
	m_StencilSize(stencilSize),
	m_SurfaceType(surfaceType),
	m_TransparentType(transparentType),
	m_TransparentRedValue(transparentRedValue),
	m_TransparentGreenValue(transparentGreenValue),
	m_TransparentBlueValue(transparentBlueValue),
	m_Width(width),
	m_Height(height)
{
}


Config :: Config (const Config &other, const EGLint * attribList, const EGLint * validAttributes) {
	m_BufferSize			= other.m_BufferSize;
	m_RedSize				= other.m_RedSize;
	m_GreenSize				= other.m_GreenSize;
	m_BlueSize				= other.m_BlueSize;
	m_AlphaSize				= other.m_AlphaSize;
	m_ConfigCaveat			= other.m_ConfigCaveat;
	m_ConfigID				= other.m_ConfigID;
	m_DepthSize				= other.m_DepthSize;
	m_Level					= other.m_Level;
	m_MaxPBufferWidth		= other.m_MaxPBufferWidth;
	m_MaxPBufferHeight		= other.m_MaxPBufferHeight;
	m_MaxPBufferPixels		= other.m_MaxPBufferPixels;
	m_NativeRenderable		= other.m_NativeRenderable;
	m_NativeVisualID		= other.m_NativeVisualID;
	m_NativeVisualType		= other.m_NativeVisualType;
	m_SampleBuffers			= other.m_SampleBuffers;
	m_Samples				= other.m_Samples;
	m_StencilSize			= other.m_StencilSize;
	m_SurfaceType			= other.m_SurfaceType;
	m_TransparentType		= other.m_TransparentType;
	m_TransparentRedValue	= other.m_TransparentRedValue;
	m_TransparentGreenValue = other.m_TransparentGreenValue;
	m_TransparentBlueValue	= other.m_TransparentBlueValue;
	m_Width					= other.m_Width;
	m_Height				= other.m_Height;

	if (attribList != 0 && validAttributes != 0) {
		while (*attribList != EGL_NONE) {
			if (IsValidAttribute(validAttributes, *attribList)) {
				SetConfigAttrib(attribList[0], attribList[1]);
			}

			attribList += 2;
		}
	}
}


Config & Config :: operator=(const Config & other) {
	m_BufferSize			= other.m_BufferSize;
	m_RedSize				= other.m_RedSize;
	m_GreenSize				= other.m_GreenSize;
	m_BlueSize				= other.m_BlueSize;
	m_AlphaSize				= other.m_AlphaSize;
	m_ConfigCaveat			= other.m_ConfigCaveat;
	m_ConfigID				= other.m_ConfigID;
	m_DepthSize				= other.m_DepthSize;
	m_Level					= other.m_Level;
	m_MaxPBufferWidth		= other.m_MaxPBufferWidth;
	m_MaxPBufferHeight		= other.m_MaxPBufferHeight;
	m_MaxPBufferPixels		= other.m_MaxPBufferPixels;
	m_NativeRenderable		= other.m_NativeRenderable;
	m_NativeVisualID		= other.m_NativeVisualID;
	m_NativeVisualType		= other.m_NativeVisualType;
	m_SampleBuffers			= other.m_SampleBuffers;
	m_Samples				= other.m_Samples;
	m_StencilSize			= other.m_StencilSize;
	m_SurfaceType			= other.m_SurfaceType;
	m_TransparentType		= other.m_TransparentType;
	m_TransparentRedValue	= other.m_TransparentRedValue;
	m_TransparentGreenValue = other.m_TransparentGreenValue;
	m_TransparentBlueValue	= other.m_TransparentBlueValue;
	m_Width					= other.m_Width;
	m_Height				= other.m_Height;

	return *this;
}


EGLint Config :: GetConfigAttrib(EGLint attribute) const {
	switch (attribute) {
	case EGL_BUFFER_SIZE:
		return m_BufferSize;

	case EGL_ALPHA_SIZE:
		return m_AlphaSize;

	case EGL_BLUE_SIZE:
		return m_BlueSize;

	case EGL_GREEN_SIZE:
		return m_GreenSize;

	case EGL_RED_SIZE:
		return m_RedSize;

	case EGL_DEPTH_SIZE:
		return m_DepthSize;

	case EGL_STENCIL_SIZE:
		return m_StencilSize;

	case EGL_CONFIG_CAVEAT:
		return m_ConfigCaveat;

	case EGL_CONFIG_ID:
		return m_ConfigID;

	case EGL_LEVEL:
		return m_Level;

	case EGL_MAX_PBUFFER_HEIGHT:
		return m_MaxPBufferHeight;

	case EGL_MAX_PBUFFER_PIXELS:
		return m_MaxPBufferPixels;

	case EGL_MAX_PBUFFER_WIDTH:
		return m_MaxPBufferWidth;

	case EGL_NATIVE_RENDERABLE:
		return m_NativeRenderable;

	case EGL_NATIVE_VISUAL_ID:
		return m_NativeVisualID;

	case EGL_NATIVE_VISUAL_TYPE:
		return m_NativeVisualType;

	case EGL_SAMPLES:
		return m_Samples;

	case EGL_SAMPLE_BUFFERS:
		return m_SampleBuffers;

	case EGL_SURFACE_TYPE:
		return m_SurfaceType;

	case EGL_TRANSPARENT_TYPE:
		return m_TransparentType;

	case EGL_TRANSPARENT_BLUE_VALUE:
		return m_TransparentBlueValue;

	case EGL_TRANSPARENT_GREEN_VALUE:
		return m_TransparentGreenValue;

	case EGL_TRANSPARENT_RED_VALUE:
		return m_TransparentRedValue;

	case EGL_WIDTH:
		return m_Width;

	case EGL_HEIGHT:
		return m_Height;

	default:
		return EGL_NONE;
	}
}


void Config :: SetConfigAttrib(EGLint attribute, EGLint value) {
	switch (attribute) {
	case EGL_BUFFER_SIZE:
		m_BufferSize = value;
		break;

	case EGL_ALPHA_SIZE:
		m_AlphaSize = value;
		break;

	case EGL_BLUE_SIZE:
		m_BlueSize = value;
		break;

	case EGL_GREEN_SIZE:
		m_GreenSize = value;
		break;

	case EGL_RED_SIZE:
		m_RedSize = value;
		break;

	case EGL_DEPTH_SIZE:
		m_DepthSize = value;
		break;

	case EGL_STENCIL_SIZE:
		m_StencilSize = value;
		break;

	case EGL_CONFIG_CAVEAT:
		m_ConfigCaveat = value;
		break;

	case EGL_CONFIG_ID:
		m_ConfigID = value;
		break;

	case EGL_LEVEL:
		m_Level = value;
		break;

	case EGL_MAX_PBUFFER_HEIGHT:
		m_MaxPBufferHeight = value;
		break;

	case EGL_MAX_PBUFFER_PIXELS:
		m_MaxPBufferPixels = value;
		break;

	case EGL_MAX_PBUFFER_WIDTH:
		m_MaxPBufferWidth = value;
		break;

	case EGL_NATIVE_RENDERABLE:
		m_NativeRenderable = value;
		break;

	case EGL_NATIVE_VISUAL_ID:
		m_NativeVisualID = value;
		break;

	case EGL_NATIVE_VISUAL_TYPE:
		m_NativeVisualType = value;
		break;

	case EGL_SAMPLES:
		m_Samples = value;
		break;

	case EGL_SAMPLE_BUFFERS:
		m_SampleBuffers = value;
		break;

	case EGL_SURFACE_TYPE:
		m_SurfaceType = value;
		break;

	case EGL_TRANSPARENT_TYPE:
		m_TransparentType = value;
		break;

	case EGL_TRANSPARENT_BLUE_VALUE:
		m_TransparentBlueValue = value;
		break;

	case EGL_TRANSPARENT_GREEN_VALUE:
		m_TransparentGreenValue = value;
		break;

	case EGL_TRANSPARENT_RED_VALUE:
		m_TransparentRedValue = value;
		break;

	case EGL_WIDTH:
		m_Width = value;
		break;

	case EGL_HEIGHT:
		m_Height = value;
		break;

	}
}


bool Config :: IsValidAttribute(const EGLint * validAttributes, EGLint attribute) {
	if (validAttributes == 0) {
		return false;
	}

	while (*validAttributes != EGL_NONE) {
		if (*validAttributes == attribute) {
			return true;
		}

		validAttributes += 2;
	}

	return false;
}


EGLBoolean Config :: GetConfigs(EGLConfig * result, EGLint configSize, EGLint * numConfig) {

	if (result == 0) {
		// special case: inquire number of configurations available
		*numConfig = s_NumConfigurations;
		return EGL_TRUE;
	}

	if (configSize > s_NumConfigurations) {
		configSize = s_NumConfigurations;
	}

	for (int index = 0; index < configSize; ++index) {
		result[index] = &s_AllConfigurations[index];
	}

	*numConfig = configSize;
	return EGL_TRUE;
}


EGLBoolean Config :: ChooseConfig(const EGLint * attribList, EGLConfig * result, EGLint configSize, EGLint * numConfig) {
	// for now, as we only have one configuration available, just return that.
	// to be a correct implementation, we will have at least to verify that
	// the attributes specified match that.
	return GetConfigs(result, configSize, numConfig);
}


