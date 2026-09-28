// ==========================================================================
//
// gl.cpp	EGL Client API entry points
//
// --------------------------------------------------------------------------
//
// 08-04-2003	Hans-Martin Will	initial version
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
#include "GLES/egl.h"
#include "Context.h"
#include "Rasterizer.h"
#include "Config.h"
#include "Surface.h"
#include <pthread.h>

using namespace EGL;

pthread_key_t s_TlsIndexContext;
pthread_key_t s_TlsIndexError;

NativeWindowType native_window;
EGLDisplay display;

static EGLint  RefCount = 0;

// version numbers
#define EGL_VERSION_MAJOR 1
#define EGL_VERSION_MINOR 0

static void eglRecordError(EGLint error) 
{
    pthread_setspecific(s_TlsIndexError, reinterpret_cast<void *>(error));
}

static NativeDisplayType GetNativeDisplay (EGLDisplay display) 
{
    return (NativeDisplayType) display;
}

EGLAPI EGLBoolean EGLAPIENTRY eglBindAPI(EGLenum api){
    return EGL_TRUE;
}

EGLAPI EGLint EGLAPIENTRY eglGetError (void) 
{
    return  reinterpret_cast<EGLint>(pthread_getspecific(s_TlsIndexError));
}

EGLAPI EGLDisplay EGLAPIENTRY eglGetDisplay (NativeDisplayType display) 
{
    return (EGLDisplay) display;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglInitialize (EGLDisplay dpy, EGLint *major, EGLint *minor) 
{
    display = dpy;

    if (major != 0) 
    {
        *major = EGL_VERSION_MAJOR;
    }

    if (minor != 0) 
    {
        *minor = EGL_VERSION_MINOR;
    }

    if (RefCount == 0) 
    {
        if ((pthread_key_create(&s_TlsIndexContext, NULL) != 0)||
            (pthread_key_create(&s_TlsIndexError, NULL) != 0))
        {
            eglRecordError(EGL_NOT_INITIALIZED);
            return false;
        }			
    }
    
    RefCount++;
    eglRecordError(EGL_SUCCESS);

    return true;
}

EGLAPI EGLBoolean EGLAPIENTRY eglTerminate (EGLDisplay dpy) 
{
    if (RefCount > 0) 
    {
        if (RefCount == 1) 
        {
            pthread_key_delete(s_TlsIndexContext);
            pthread_key_delete(s_TlsIndexError);
        }

        RefCount--;
    }

    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI const char * EGLAPIENTRY eglQueryString (EGLDisplay dpy, EGLint name) 
{
    eglRecordError(EGL_SUCCESS);

    switch (name) 
    {
    case EGL_VENDOR:
        return EGL_CONFIG_VENDOR;

    case EGL_VERSION:
        return EGL_VERSION_NUMBER;

    case EGL_EXTENSIONS:
        return "";

    default:
        eglRecordError(EGL_BAD_PARAMETER);
        return 0;
    }
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglSaveSurfaceHM(EGLSurface surface, const char *filename) 
{
    return surface->Save(filename);
}

#define FunctionEntry(s) { #s, (void (EGLAPIENTRY *)(void)) s }

static const struct 
{
    const char * name;
    void (EGLAPIENTRY * ptr)(void);
    
} FunctionTable[] = {
    /* OES_query_matrix */
    FunctionEntry(glQueryMatrixxOES),
    
    /* OES_point_size_array */
    FunctionEntry(glPointSizePointerOES),
    
    /* OES_matrix_palette */
    FunctionEntry(glCurrentPaletteMatrixOES),
    FunctionEntry(glLoadPaletteFromModelViewMatrixOES),
    FunctionEntry(glMatrixIndexPointerOES),
    FunctionEntry(glWeightPointerOES),
    
    FunctionEntry(eglSaveSurfaceHM)
};


EGLAPI void (EGLAPIENTRY * eglGetProcAddress(const char *procname))(void) 
{
    eglRecordError(EGL_SUCCESS);

    if (!procname) 
    {
        return 0;
    }

    size_t functions = sizeof(FunctionTable) / sizeof(FunctionTable[0]);

    for (size_t index = 0; index < functions; ++index) 
    {
        if (!strcmp(FunctionTable[index].name, procname))
            return (void (EGLAPIENTRY *)(void)) FunctionTable[index].ptr;
    }

    return 0;
}


EGLAPI EGLBoolean EGLAPIENTRY 
eglGetConfigs (EGLDisplay dpy, EGLConfig *configs, EGLint config_size, 
               EGLint *num_config) 
{
    eglRecordError(EGL_SUCCESS);
    return Config::GetConfigs(configs, config_size, num_config);
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglChooseConfig (EGLDisplay dpy, const EGLint *attrib_list, EGLConfig *configs,
                 EGLint config_size, EGLint *num_config) 
{
    eglRecordError(EGL_SUCCESS);
    return Config::ChooseConfig(attrib_list, configs, config_size, num_config);
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglGetConfigAttrib (EGLDisplay dpy, EGLConfig config, EGLint attribute, 
                    EGLint *value) 
{
    *value = config->GetConfigAttrib(attribute);
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}


EGLAPI EGLSurface EGLAPIENTRY 
eglCreateWindowSurface (EGLDisplay dpy, EGLConfig config, 
                        NativeWindowType window, const EGLint *attrib_list) 
{
    XWindowAttributes attr;

    native_window = window;

    XGetWindowAttributes((NativeDisplayType)dpy, native_window, 
                         &attr);
   
    Config surfaceConfig(*config);
    surfaceConfig.SetConfigAttrib(EGL_SURFACE_TYPE, EGL_WINDOW_BIT);
    surfaceConfig.SetConfigAttrib(EGL_WIDTH, attr.width);
    surfaceConfig.SetConfigAttrib(EGL_HEIGHT, attr.height);

    EGLSurface new_surface = new EGL::Surface(surfaceConfig, 
                                              (NativeDisplayType) dpy);

    eglRecordError(EGL_SUCCESS);
    return (new_surface); 
}

EGLAPI EGLSurface EGLAPIENTRY 
eglCreatePixmapSurface (EGLDisplay dpy, EGLConfig config, 
                        NativePixmapType pixmap, const EGLint *attrib_list) 
{
    // Cannot support rendering to arbitrary native surfaces; use pbuffer 
    // surface and eglCopySurfaces instead
    eglRecordError(EGL_BAD_MATCH);
    return EGL_NO_SURFACE;
}

EGLAPI EGLSurface EGLAPIENTRY 
eglCreatePbufferSurface (EGLDisplay dpy, EGLConfig config, 
                         const EGLint *attrib_list) 
{
    static const EGLint validAttributes[] = {
        EGL_WIDTH, 0,
        EGL_HEIGHT, 0,
        EGL_NONE
    };

    Config surfaceConfig(*config, attrib_list, validAttributes);
    surfaceConfig.SetConfigAttrib(EGL_SURFACE_TYPE, EGL_PBUFFER_BIT);
    eglRecordError(EGL_SUCCESS);
    return new EGL::Surface(surfaceConfig, GetNativeDisplay(dpy));
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglDestroySurface (EGLDisplay dpy, EGLSurface surface) 
{
    surface->Dispose();
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglQuerySurface (EGLDisplay dpy, EGLSurface surface, EGLint attribute, 
                 EGLint *value) 
{
    *value = surface->GetConfig()->GetConfigAttrib(attribute);
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLContext EGLAPIENTRY 
eglCreateContext (EGLDisplay dpy, EGLConfig config, EGLContext share_list, 
                  const EGLint *attrib_list) 
{
    eglRecordError(EGL_SUCCESS);
    return new Context(*config);
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglDestroyContext (EGLDisplay dpy, EGLContext ctx) 
{
    ctx->Dispose();
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglMakeCurrent (EGLDisplay dpy, EGLSurface draw, EGLSurface read, 
                EGLContext ctx) 
{
    Context::SetCurrentContext(ctx);
    
    if (ctx) 
    {
        ctx->SetDrawSurface(draw);
        ctx->SetReadSurface(read);
    }
    
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLContext EGLAPIENTRY eglGetCurrentContext (void) 
{
    eglRecordError(EGL_SUCCESS);
    return Context::GetCurrentContext();
}

EGLAPI EGLSurface EGLAPIENTRY eglGetCurrentSurface (EGLint readdraw) 
{
    EGLContext currentContext = eglGetCurrentContext();
    eglRecordError(EGL_SUCCESS);
    
    if (currentContext != 0) 
    {
        switch (readdraw) 
        {
        case EGL_DRAW:
            return currentContext->GetDrawSurface();
            
        case EGL_READ:
            return currentContext->GetReadSurface();
            
        default: 
            return 0;
        }
    } 
    else 
    {
        return 0;
    }
}

EGLAPI EGLDisplay EGLAPIENTRY eglGetCurrentDisplay (void) 
{
    eglRecordError(EGL_SUCCESS);
    return display;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglQueryContext (EGLDisplay dpy, EGLContext ctx, EGLint attribute, 
                 EGLint *value) 
{
    *value = ctx->GetConfig()->GetConfigAttrib(attribute);
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY eglWaitGL (void) 
{
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY eglWaitNative (EGLint engine) 
{
    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY eglSwapBuffers (EGLDisplay dpy, EGLSurface draw) 
{
    NativeDisplayType disp = (NativeDisplayType) dpy;
    Context::GetCurrentContext()->Flush();
    int w = draw->GetWidth();
    int h = draw->GetHeight();
    XImage *img;
    GC gc;

    XWindowAttributes attr;

    XGetWindowAttributes(disp, native_window, &attr);

#ifdef HAVE_X11_EXTENSIONS_XSHM_H
    if (draw->GetSurfaceType() == SHMSTD)
    {
        img = draw->GetImage();

        if (attr.depth != 16)
        {
            U16 *cb = draw->GetColorBuffer();
            U32 *ib = (U32 *) img->data;
            U32 rgb;
            int x, y;

            if (attr.depth != 24)
            {
                return EGL_FALSE;
            }

            for (y = 0; y < h; y++)
                for (x = 0; x < w; x++)
                {
                    int rv, gv, bv;
                    rv = (cb[x+y*w] & 0xF800u) >> 8;
                    gv = (cb[x+y*w] & 0x07E0u) >> 3;
                    bv = (cb[x+y*w] & 0x001Fu) << 3;
                    
                    rv |= rv >> 5;
                    gv |= gv >> 6;
                    bv |= bv >> 5;
                    
                    rgb = (rv << 16)|(gv << 8)|bv; 
                    
                    ib[x+(h-y-1)*w] = rgb;
                }
        }

        gc = XCreateGC(disp, native_window, 0, NULL);
        XShmPutImage(disp, native_window, gc, img, 0, 0, 0, 0, 
                     draw->GetWidth(), draw->GetHeight(), false);
        XFreeGC(disp, gc);
    }
    else
#endif
    {
        gc = XCreateGC(disp, native_window, 0, NULL);

        if (attr.depth != 16)
        {
            U16 *cb = draw->GetColorBuffer();
            U32 rgb;
            int x, y;
            
            if (attr.depth != 24)
            {
                return EGL_FALSE;
            }

            /*
             * Evil slow hack starts, this way we can avoid BadMatch errors
             */

            img = XGetImage(disp, native_window, 0, 0, w, h, AllPlanes, 
                            ZPixmap);
            
            for (y = 0; y < h; y++)
                for (x = 0; x < w; x++)
                {
                    int rv, gv, bv;
                    rv = (cb[x+y*w] & 0xF800u) >> 8;
                    gv = (cb[x+y*w] & 0x07E0u) >> 3;
                    bv = (cb[x+y*w] & 0x001Fu) << 3;
                    
                    rv |= rv >> 5;
                    gv |= gv >> 6;
                    bv |= bv >> 5;
                    
                    rgb = (rv << 16)|(gv << 8)|bv; 
                    
                    XPutPixel(img, x, h - y - 1, rgb);
                }

            XPutImage(disp, native_window, gc, img, 0, 0, 0, 0, 
                      draw->GetWidth(), draw->GetHeight());
            XDestroyImage(img);
            XFreeGC(disp, gc);
        }
        else // fine, we have 16-bit depth
        {
            gc = XCreateGC((Display *) dpy, native_window, 0, NULL);
            img = draw->GetImage();
            XPutImage(disp, native_window, gc, img, 0, 0, 0, 0, 
                      draw->GetWidth(), draw->GetHeight());
            XFreeGC(disp, gc);
        }
    }

    XFlush(disp);

    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglCopyBuffers (EGLDisplay dpy, EGLSurface surface, NativePixmapType target) 
{
    NativeDisplayType disp = (NativeDisplayType) dpy;
    Context::GetCurrentContext()->Flush();
    int w = surface->GetWidth();
    int h = surface->GetHeight();
    XImage *img;
    XWindowAttributes attr;
    GC gc;

    XGetWindowAttributes(disp, target, &attr);

#ifdef HAVE_X11_EXTENSIONS_XSHM_H
    if (surface->GetSurfaceType() == SHMSTD)
    {
        img = surface->GetImage();

        if (attr.depth != 16)
        {
            U16 *cb = surface->GetColorBuffer();
            U32 *ib = (U32 *) img->data;
            U32 rgb;
            int x, y;

            if (attr.depth != 24)
            {
                return EGL_FALSE;
            }

            for (y = 0; y < h; y++)
                for (x = 0; x < w; x++)
                {
                    int rv, gv, bv;
                    rv = (cb[x+y*w] & 0xF800u) >> 8;
                    gv = (cb[x+y*w] & 0x07E0u) >> 3;
                    bv = (cb[x+y*w] & 0x001Fu) << 3;
                    
                    rv |= rv >> 5;
                    gv |= gv >> 6;
                    bv |= bv >> 5;
                    
                    rgb = (rv << 16)|(gv << 8)|bv; 
                    
                    ib[x+y*w] = rgb;
                }
        }

        gc = XCreateGC(disp, target, 0, NULL);
        XShmPutImage(disp, target, gc, img, 0, 0, 0, 0, 
                     surface->GetWidth(), surface->GetHeight(), false);
        XFreeGC(disp, gc);
    }
    else
#endif
    {
        gc = XCreateGC(disp, target, 0, NULL);

        if (attr.depth != 16)
        {
            U16 *cb = surface->GetColorBuffer();
            U32 rgb;
            int x, y;
            
            if (attr.depth != 24)
            {
                return EGL_FALSE;
            }

            /*
             * Evil slow hack starts, this way we can avoid BadMatch errors
             */

            img = XGetImage(disp, target, 0, 0, w, h, AllPlanes, ZPixmap);
            
            for (y = 0; y < h; y++)
                for (x = 0; x < w; x++)
                {
                    int rv, gv, bv;
                    rv = (cb[x+y*w] & 0xF800u) >> 8;
                    gv = (cb[x+y*w] & 0x07E0u) >> 3;
                    bv = (cb[x+y*w] & 0x001Fu) << 3;
                    
                    rv |= rv >> 5;
                    gv |= gv >> 6;
                    bv |= bv >> 5;
                    
                    rgb = (rv << 16)|(gv << 8)|bv; 
                    
                    XPutPixel(img, x, y, rgb);
                }

            XPutImage(disp, target, gc, img, 0, 0, 0, 0, 
                      surface->GetWidth(), surface->GetHeight());
            XDestroyImage(img);
            XFreeGC(disp, gc);
        }
        else // fine, we have 16-bit depth
        {
            gc = XCreateGC((Display *) dpy, target, 0, NULL);
            img = surface->GetImage();
            XPutImage(disp, target, gc, img, 0, 0, 0, 0, 
                      surface->GetWidth(), surface->GetHeight());
            XFreeGC(disp, gc);
        }
    }

    XFlush(disp);

    eglRecordError(EGL_SUCCESS);
    return EGL_TRUE;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglSurfaceAttrib (EGLDisplay dpy, EGLSurface surface, EGLint attribute, 
                  EGLint value) 
{
    return EGL_FALSE;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglBindTexImage(EGLDisplay dpy, EGLSurface surface, EGLint buffer) 
{
    return EGL_FALSE;
}

EGLAPI EGLBoolean EGLAPIENTRY 
eglReleaseTexImage(EGLDisplay dpy, EGLSurface surface, EGLint buffer) 
{
    return EGL_FALSE;
}

EGLAPI EGLBoolean EGLAPIENTRY eglSwapInterval(EGLDisplay dpy, EGLint interval) 
{
    return EGL_FALSE;
}
