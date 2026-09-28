#ifndef EGL_TEXTURE_H
#define EGL_TEXTURE_H 1

// ==========================================================================
//
// Texture.h		Texture Class for 3D Rendering Library
//
// --------------------------------------------------------------------------
//
// 09-14-2003		Hans-Martin Will	initial version
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
#include "Config.h"
#include "Color.h"
#include "RasterizerState.h"


namespace EGL {

	class Rasterizer;
	class CodeGenerator;

	class Texture { 

#ifdef EGL_ON_LINUX
		friend class Rasterizer;
		friend class CodeGenerator;
#else
		friend Rasterizer;
		friend CodeGenerator;
#endif

	public:

		void Init();
		void Dispose();

		bool Initialize(U32 width, U32 height, RasterizerState::TextureFormat format);

		U32 GetWidth() const				{ return 1 << m_LogWidth; }
		U32 GetHeight() const				{ return 1 << m_LogHeight; }
		U32 GetLogWidth() const				{ return m_LogWidth; }
		U32 GetLogHeight() const			{ return m_LogHeight; }
		U32 GetLogBytesPerPixel() const;

		RasterizerState::TextureFormat 
			GetInternalFormat() const		{ return m_InternalFormat; }

		U8 GetBytesPerPixel() const			{ return s_BytesPerPixel[m_InternalFormat]; }

		void * GetData() const				{ return m_Data; }

	private:
		void *							m_Data;
		U32								m_LogWidth;
		U32								m_LogHeight;
		RasterizerState::TextureFormat	m_InternalFormat;

		const static U8 s_BytesPerPixel[];
	};


	class MultiTexture {

#ifdef EGL_ON_LINUX
		friend class Rasterizer;
#else
		friend Rasterizer;
#endif

	public:
		enum {
			MAX_LEVELS = RasterizerState::LogMaxTextureSize
		};

		MultiTexture();
		~MultiTexture();

		Texture * GetTexture(int level)				{ return m_TextureLevels + level; }
		const Texture * GetTexture(int level) const	{ return m_TextureLevels + level; }

		void SetMinFilterMode(RasterizerState::FilterMode mode)		{ m_MinFilterMode = mode; }
		void SetMagFilterMode(RasterizerState::FilterMode mode)		{ m_MagFilterMode = mode; }
		void SetMipmapFilterMode(RasterizerState::FilterMode mode)	{ m_MipmapFilterMode = mode; }
		void SetWrappingModeS(RasterizerState::WrappingMode mode)	{ m_WrappingModeS = mode; }
		void SetWrappingModeT(RasterizerState::WrappingMode mode)	{ m_WrappingModeT = mode; }

		RasterizerState::FilterMode GetMinFilterMode() const		{ return m_MinFilterMode; }
		RasterizerState::FilterMode GetMagFilterMode() const		{ return m_MagFilterMode; }
		RasterizerState::FilterMode GetMipmapFilterMode() const		{ return m_MipmapFilterMode; }
		RasterizerState::WrappingMode GetWrappingModeS() const		{ return m_WrappingModeS; }
		RasterizerState::WrappingMode GetWrappingModeT() const		{ return m_WrappingModeT; }

		RasterizerState::TextureFormat 
			GetInternalFormat() const				{ return m_TextureLevels[0].GetInternalFormat(); }

		bool IsComplete() const;

		bool IsMipMap() const;

	private:
		Texture							m_TextureLevels[MAX_LEVELS + 1];
		RasterizerState::FilterMode		m_MinFilterMode;
		RasterizerState::FilterMode		m_MagFilterMode;
		RasterizerState::FilterMode		m_MipmapFilterMode;
		RasterizerState::WrappingMode	m_WrappingModeS;
		RasterizerState::WrappingMode	m_WrappingModeT;
		U32								m_Levels;
	};

	inline bool MultiTexture :: IsMipMap() const {
		return 
			m_MipmapFilterMode == RasterizerState::FilterModeNearest ||
			m_MipmapFilterMode == RasterizerState::FilterModeLinear;
	}

}

#endif //ndef EGL_TEXTURE_H
