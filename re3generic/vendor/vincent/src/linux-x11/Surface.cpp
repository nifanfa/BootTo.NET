// ==========================================================================
//
// Surface.cpp		Drawing Surface Class for 3D Rendering Library
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
#include "Surface.h"
#include "Color.h"
#include <string.h>

using namespace EGL;

Surface :: Surface(const Config & config, NativeDisplayType hdc)
    :	m_Config(config),
      m_Rect (0, 0, config.GetConfigAttrib(EGL_WIDTH), 
              config.GetConfigAttrib(EGL_HEIGHT)),
      m_HDC(NULL)
{
    U32 width = GetWidth();
    U32 height = GetHeight();
    int shm_major, shm_minor, shm_pixmaps;
    int pbuffer = config.GetConfigAttrib(EGL_SURFACE_TYPE) & EGL_PBUFFER_BIT;

    m_WindowDepth = DefaultDepth(hdc, 0);

#ifdef HAVE_X11_EXTENSIONS_XSHM_H
    if(XShmQueryVersion(hdc, &shm_major, &shm_minor, &shm_pixmaps))
    {
        m_SurfaceType = SHMSTD;
    }

    if (m_SurfaceType == SHMSTD)
    {
        m_Bitmap = 0;

        m_ShmInfo = new XShmSegmentInfo;

        memset(m_ShmInfo, 0, sizeof(XShmSegmentInfo));

        if (m_WindowDepth == 16)
        {
            m_ShmInfo->shmid = shmget(IPC_PRIVATE, width*height*sizeof(U16),
                                      IPC_CREAT|0777);
        }
        else
        {
            m_ColorBuffer = new U16[width * height];
            m_ShmInfo->shmid = shmget(IPC_PRIVATE, width*height*sizeof(U32),
                                      IPC_CREAT|0777);
        }
        
        m_ShmInfo->shmaddr = (char *) shmat(m_ShmInfo->shmid, 0, 0);
        m_ShmInfo->readOnly=False;
        XShmAttach(hdc, m_ShmInfo);

        m_Image = XShmCreateImage(hdc, DefaultVisual(hdc, 0), m_WindowDepth, 
                                  ZPixmap, NULL, m_ShmInfo, width, height);

        if (m_WindowDepth == 16)
        {
            m_ColorBuffer = (U16 *) m_ShmInfo->shmaddr;
            m_Image->data = m_ShmInfo->shmaddr;
        }
        else
        {
            m_Image->data = m_ShmInfo->shmaddr;
        }
    }
    else
#endif
    {
        m_SurfaceType = NOSHM;

        m_Bitmap = XCreatePixmap(hdc, XDefaultRootWindow(hdc), 
                                 width, height, 16);

        m_Image = XGetImage(hdc, m_Bitmap, 0, 0, width, height, AllPlanes, 
                            ZPixmap);

        m_ColorBuffer = (U16 *) m_Image->data;
    }

    m_AlphaBuffer = new U8[width * height];
    m_DepthBuffer = new U16[width * height];
    m_StencilBuffer = new U32[width * height];
    
    if (hdc != NULL) 
    {
        m_HDC = hdc;
    }
}

Surface :: ~Surface() 
{
    if (m_Bitmap)
    {
        XFreePixmap(m_HDC, m_Bitmap);
    }

    if (m_Image)
    {
        XDestroyImage(m_Image);
        m_Image = NULL;
#ifdef HAVE_X11_EXTENSIONS_XSHM_H
        if (m_SurfaceType == SHMSTD)
        {
            XShmDetach(m_HDC, m_ShmInfo);

            if(m_ShmInfo->shmaddr)
            {
                shmdt(m_ShmInfo->shmaddr);
            }

            if(m_ShmInfo->shmid >= 0)
            {
                shmctl(m_ShmInfo->shmid, IPC_RMID, NULL);
            }

            delete m_ShmInfo;

            if (m_WindowDepth != 16)
            {
                delete[] m_ColorBuffer;
            }
        }
#endif
        m_ColorBuffer = NULL;
    }

    if (m_HDC != NULL) 
    {
        m_HDC = NULL;
    }

    if (m_AlphaBuffer != 0) 
    {
        delete [] m_AlphaBuffer;
        m_AlphaBuffer = 0;
    }
    
    if (m_DepthBuffer != 0) 
    {
        delete[] m_DepthBuffer;
        m_DepthBuffer = 0;
    }

    if (m_StencilBuffer != 0) 
    {
        delete[] m_StencilBuffer;
        m_StencilBuffer = 0;
    }
}

void Surface :: Dispose() 
{
    if (GetCurrentContext() != 0) 
    {
        m_Disposed = true;
    } 
    else 
    {
        delete this;
    }
}

void Surface :: SetCurrentContext(Context * context) 
{
    m_CurrentContext = context;
    
    if (context == 0 && m_Disposed) 
    {
        delete this;
    }
}

namespace {
    template <class T> void FillRect(T * base, const Rect & bufferRect,
                                     const Rect & fillRect,
                                     const T& value, const T& mask) 
    {
        Rect rect = Rect::Intersect(fillRect, bufferRect);
        
        base += fillRect.x + fillRect.y * bufferRect.width;
        size_t gap = bufferRect.width - fillRect.width;
        
        size_t rows = fillRect.height;
        T inverseMask = ~mask;
        T maskedValue = value & mask;

        while (rows--) 
        {
            for (size_t columns = fillRect.width; columns > 0; columns--) 
            {
                *base = (*base & inverseMask) | maskedValue;
                ++base;
            }
            
            base += gap;
        }
    }

    template <class T> void FillRect(T * base, const Rect & bufferRect, 
                                     const Rect & fillRect,
                                     const T& value) 
    {
        Rect rect = Rect::Intersect(fillRect, bufferRect);
        
        base += fillRect.x + fillRect.y * bufferRect.width;
        size_t gap = bufferRect.width - fillRect.width;
        
        size_t rows = fillRect.height;
        
        while (rows--) 
        {
            for (size_t columns = fillRect.width; columns > 0; columns--) 
            {
                *base = value;
                ++base;
            }
            
            base += gap;
        }
    }
}

void Surface :: ClearDepthBuffer(U16 depth, bool mask, const Rect& scissor) 
{
    if (!mask || !m_DepthBuffer)
        return;
    
    FillRect(m_DepthBuffer, GetRect(), scissor, depth);
}


void Surface :: ClearStencilBuffer(U32 value, U32 mask, const Rect& scissor) 
{
    if (!mask || !m_StencilBuffer)
        return;
    
    if (mask != ~0) 
    {
        FillRect(m_StencilBuffer, GetRect(), scissor, value, mask);
    } 
    else 
    {
        FillRect(m_StencilBuffer, GetRect(), scissor, value);
    }
}

/*
I32 Surface :: DepthBitsFromDepth(GLclampx depth) {
  I32 result;
	gppMul_16_32s(EGL_CLAMP(depth, EGL_FIXED_0, EGL_FIXED_1), 0xffffff, &result);
	return result;
}
*/

void Surface :: ClearColorBuffer(const Color & rgba, const Color & mask, 
                                 const Rect& scissor) 
{
    U16 color = rgba.ConvertTo565();
    U16 colorMask = mask.ConvertTo565();

    if (colorMask == 0xffff) 
    {
        FillRect(m_ColorBuffer, GetRect(), scissor, color);
    } 
    else 
    {
        FillRect(m_ColorBuffer, GetRect(), scissor, color, colorMask);
    }

    if (mask.A() && m_AlphaBuffer)
        FillRect(m_AlphaBuffer, GetRect(), scissor, rgba.A());
}


/*
 * For debugging
 */

#define BI_BITFIELDS 3

typedef struct tagBITMAPINFOHEADER
{
    int biSize; 
    long biWidth; 
    long biHeight; 
    short biPlanes; 
    short biBitCount; 
    int biCompression; 
    int biSizeImage; 
    long biXPelsPerMeter; 
    long biYPelsPerMeter; 
    int biClrUsed; 
    int biClrImportant; 
} BITMAPINFOHEADER;

struct InfoHeader 
{
		BITMAPINFOHEADER bmiHeader;
		short bmiColors[3];
};

typedef struct tagBITMAPFILEHEADER { 
  short bfType; 
  int bfSize; 
  short bfReserved1; 
  short bfReserved2; 
  int bfOffBits; 
} BITMAPFILEHEADER;

void InitInfoHeader(struct InfoHeader *h, int width, int height) 
{
    h->bmiHeader.biSize = sizeof(h->bmiHeader);
    h->bmiHeader.biWidth = width;
    h->bmiHeader.biHeight = height;
    h->bmiHeader.biPlanes = 1;
    h->bmiHeader.biBitCount = 16;
    h->bmiHeader.biCompression = BI_BITFIELDS;
    h->bmiHeader.biSizeImage = width * height * sizeof(short);
    h->bmiHeader.biXPelsPerMeter = 72 * 25;
    h->bmiHeader.biYPelsPerMeter = 72 * 25;
    h->bmiHeader.biClrUsed = 0;
    h->bmiHeader.biClrImportant = 0;
            
    h->bmiColors[0] = 0xF800;
    h->bmiColors[1] = 0x07E0;
    h->bmiColors[2] = 0x001F;
}

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>

bool Surface::Save(const char *filename)
{
    struct InfoHeader info;
    BITMAPFILEHEADER header;
    int fd= open(filename, O_RDWR|O_CREAT|O_TRUNC);
    U16 *pixels;
    U8 data;
    int h;

    InitInfoHeader(&info, GetWidth(), GetHeight());

    header.bfType      = 0x4d42;
    header.bfSize      = sizeof(BITMAPFILEHEADER) + sizeof(info) + 
        info.bmiHeader.biSizeImage;
    header.bfReserved1 = 0;
    header.bfReserved2 = 0;
    header.bfOffBits   = sizeof(BITMAPFILEHEADER) + sizeof(info);

    printf("Saving image %s,  %ux%u, %u\n", filename, info.bmiHeader.biWidth, 
           info.bmiHeader.biHeight,
           info.bmiHeader.biSizeImage);
    printf("size %u, offset %u\n", header.bfSize, header.bfOffBits);

    if (fd < 3) 
    {
        fprintf(stderr, "Could not open file %s for writing\n", filename);
        return false;
    }

    if (write(fd, &header, sizeof(header)) != sizeof(header))
    {
        fprintf(stderr, "Error writing bitmap %s header\n", filename);
        close(fd);
        return false;
    }

    if (write(fd, &info, sizeof(info)) != sizeof(info))
    {
        fprintf(stderr, "Error writing bitmap %s info\n", filename);
        close(fd);
        return false;
    }

    pixels = m_ColorBuffer;

    for (h = GetHeight(); h; --h) 
    {
        if (write(fd, pixels, sizeof(U16)*GetWidth()) != 
            sizeof(U16)*GetWidth())
        {
            fprintf(stderr, "Error writing bitmap %s data\n", filename);
            close(fd);
            return false;
        }

        pixels += GetWidth();
    }

    close(fd);

    return true;
}

