#ifndef EGL_SURFACE_H
#define EGL_SURFACE_H 1

// ==========================================================================
//
// Surface.h		Drawing Surface Class for 3D Rendering Library
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
#include "GLES/gl.h"
#include "Types.h"
#include "Config.h"
#include "fixed.h"
#include "Color.h"
#include "Color.h"
//#include "linux-config.h"

#ifdef HAVE_X11_EXTENSIONS_XSHM_H
#include <X11/extensions/XShm.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#endif

namespace EGL {

#define NOSHM 1
#define SHMSTD 2
#define SHMPIXMAP 3

	class Surface {
		friend class Context;

	public:

		// Create a PBuffer surface
		Surface(const Config & config, NativeDisplayType hdc = 0);
		~Surface();

		// Is the depth value re-scaled based on near/far settings?.
		void ClearDepthBuffer(U16 depth, bool mask, const Rect& scissor);
		void ClearColorBuffer(const Color & rgba, const Color & mask, const Rect& scissor);
		void ClearStencilBuffer(U32 value, U32 mask, const Rect& scissor);

		void ClearDepthBuffer(U16 depth, bool mask);
		void ClearColorBuffer(const Color & rgba, const Color & mask);
		void ClearStencilBuffer(U32 value, U32 mask);

		U16 GetWidth();
		U16 GetHeight();
		U32 GetPixels();
		const Rect& GetRect() const;

		void SetCurrentContext(Context * context);
		Context * GetCurrentContext();

		U16 * GetColorBuffer();
		U8 *  GetAlphaBuffer();
		U16 * GetDepthBuffer();
		U32 * GetStencilBuffer();

		Config * GetConfig();

		bool Save(const char *filename);
		void Dispose();

		// Windows integration
		NativeDisplayType GetMemoryDC();
		NativePixmapType GetBitmap();
    XImage *GetImage();
    int GetSurfaceType();

	private:
    int m_SurfaceType;
    int m_WindowDepth;
#if defined(HAVE_X11_EXTENSIONS_XSHM_H)
    XShmSegmentInfo *m_ShmInfo;
#endif
    XImage *m_Image;
		NativeDisplayType m_HDC;				// windows device context handle
		Config	m_Config;			// configuration arguments
		U16 *	m_ColorBuffer;		// pointer to frame buffer base address 5-6-5
		U8 *	m_AlphaBuffer;		// pointer to alpha buffer
		U16 *	m_DepthBuffer;		// pointer to Z-buffer base address
		U32 *	m_StencilBuffer;	// stencil buffer
    NativePixmapType m_Bitmap;

		Rect	m_Rect;

		Context *	m_CurrentContext;
		bool	m_Disposed;			// the surface 
	};


	// --------------------------------------------------------------------------
	// Inline accessors
	// --------------------------------------------------------------------------


	inline Config * Surface :: GetConfig() {
		return &m_Config;
	}

	inline U32 Surface :: GetPixels() {
		return GetWidth() * GetHeight();
	}

	inline Context * Surface :: GetCurrentContext() {
		return m_CurrentContext;
	}

	inline NativeDisplayType Surface :: GetMemoryDC() {
		return m_HDC;
	}

	inline NativePixmapType Surface :: GetBitmap() {
		return m_Bitmap;
	}

	inline XImage* Surface :: GetImage() {
		return m_Image;
	}

	inline int Surface :: GetSurfaceType() {
		return m_SurfaceType;
	}
	
	inline U16 * Surface :: GetColorBuffer() {
		return m_ColorBuffer;
	}

	inline U8 * Surface :: GetAlphaBuffer() {
		return m_AlphaBuffer;
	}

	inline U16 * Surface :: GetDepthBuffer() {
		return m_DepthBuffer;
	}

	inline U32 * Surface :: GetStencilBuffer() {
		return m_StencilBuffer;
	}

	inline U16 Surface :: GetWidth() {
		return m_Rect.width;
	}

	inline U16 Surface :: GetHeight() {
		return m_Rect.height;
	}

	inline const Rect& Surface :: GetRect() const {
		return m_Rect;
	}

	inline void Surface :: ClearDepthBuffer(U16 depth, bool mask) {
		ClearDepthBuffer(depth, mask, GetRect());
	}

	inline void Surface :: ClearColorBuffer(const Color & rgba, const Color & mask) {
		ClearColorBuffer(rgba, mask, GetRect());
	}

	inline void Surface :: ClearStencilBuffer(U32 value, U32 mask) {
		ClearStencilBuffer(value, mask, GetRect());
	}

}

#endif // ndef EGL_SURFACE_H
