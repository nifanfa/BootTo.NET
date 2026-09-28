/*
 * GLESonGL implementation
 * Version:  1.2
 *
 * Copyright (C) 2003  David Blythe   All Rights Reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
 * DAVID BLYTHE BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#include <sys/time.h>
#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <X11/Xlib.h>
#include <GLES/egl.h>

#include "ug.h"

#ifndef DEBUG
#define DEBUG(x)
#endif

typedef struct {
    void (*idle)(UGWindow w);
    NativeDisplayType dpy;
    struct ugwindow* win, * winlist;
    EGLDisplay egldpy;
} UGCtx_t;

static UGCtx_t * context;

typedef struct ugwindow {
    Window win;
    int width, height;
    int redraw;
    void (*draw)(UGWindow w);
    void (*reshape)(UGWindow w, int width,int height);
    void (*kbd)(UGWindow w, int key, int x, int y);
    void (*pointer)(UGWindow w, int button, int state, int x, int y);
    void (*motion)(UGWindow w, int x, int y);
    UGCtx_t* ug;
    struct ugwindow* next, * prev;
    EGLSurface surface;
    EGLConfig eglconfig;
    EGLContext eglctx;
} UGWindow_t;


UGCtx APIENTRY
ugInit(void) {
    UGCtx_t* ug = malloc(sizeof *ug);
    if (ug) {
      memset(ug, 0, sizeof *ug);

      if (!(ug->dpy = XOpenDisplay(NULL)))
        goto end;

      ug->egldpy = eglGetDisplay(ug->dpy);

      if (!eglInitialize(ug->egldpy, NULL, NULL))
        goto end;
    }

  	context = ug;
    return (UGCtx)ug;
 end:
    free(ug);
    return 0;
}

void APIENTRY
ugFini(UGCtx ug) {
    UGCtx_t* _ug = (UGCtx_t*)ug;
    eglTerminate(_ug->egldpy);
    free(_ug);
}

UGCtx APIENTRY ugCtxFromWin(UGWindow uwin) {
    return (UGCtx)((UGWindow_t*)uwin)->ug;
}

static void
bind_context(UGCtx_t* ug, UGWindow_t* w) {
    if (!eglMakeCurrent(ug->egldpy, w->surface, w->surface, w->eglctx)) {
      printf("botch makecurrent\n");
      exit(1);
    }
    ug->win = w;
}

UGWindow APIENTRY
ugCreateWindow(UGCtx ug,  const char* config,
		const char* title, int width, int height, int x, int y) {
    UGWindow_t *w = malloc(sizeof *w);
    UGCtx_t* _ug = (UGCtx_t*)ug;

    if (w) {
      EGLint n, vid;
      EGLConfig configs[1];
      int size,screen_num;

      memset(w, 0, sizeof *w);

      /*XXXblythe horrible hack, need to parse config string*/
      static int attribs[] = { EGL_RED_SIZE, 1, EGL_NONE }; /*XXXblythe*/
      static int attribs2[] = {EGL_RED_SIZE, 1, EGL_DEPTH_SIZE, 1, EGL_NONE};
      int depth = strstr(config, "UG_DEPTH") != 0;

      if (!eglChooseConfig(_ug->egldpy, depth ? attribs2 : attribs, configs, 1, &n)) {
        free(w);
        return 0;
      }
      w->eglconfig = configs[0];
      eglGetConfigAttrib(_ug->egldpy, configs[0], EGL_NATIVE_VISUAL_ID, &vid);

      screen_num = DefaultScreen(_ug->dpy);
      w->win = XCreateSimpleWindow(_ug->dpy, DefaultRootWindow(_ug->dpy), x, y, width, height, 0, BlackPixel(_ug->dpy, screen_num), WhitePixel(_ug->dpy, screen_num));
      if (!w->win) {
        free(w);
        return 0;
      }

      /* subscribe to the given set of event types. */
      XSelectInput(_ug->dpy, w->win, ExposureMask | KeyPressMask | ButtonPressMask | Button1MotionMask | Button2MotionMask | StructureNotifyMask);

      /* make the window actually appear on the screen. */
      XMapWindow(_ug->dpy,w->win);

      /* flush all pending requests to the X server. */
      XFlush(_ug->dpy);

      w->surface = eglCreateWindowSurface(_ug->egldpy, w->eglconfig, (NativeWindowType)(w->win), 0);
      /*XXXblythe share*/
      w->eglctx = eglCreateContext(_ug->egldpy, w->eglconfig, NULL, NULL);

      bind_context(_ug, w);

      w->ug = _ug;
      w->width = width;
      w->height = height;
      w->next = _ug->winlist;
      _ug->winlist = w;
      w->prev = 0;
      if (w->next) 
	w->next->prev = w;
    }
    return (UGWindow)w;
}

void APIENTRY
ugDestroyWindow(UGWindow uwin) {
    UGWindow_t* w = (UGWindow_t*)uwin;
    UGCtx_t* ug = w->ug;
    eglDestroySurface(ug->egldpy, w->surface);

    if (ug->winlist == w) ug->winlist = w->next;
    if (w->next) w->next->prev = w->prev;
    if (w->prev) w->prev->next = w->next;
    free(w);
}

void APIENTRY
ugReshapeFunc(UGWindow uwin, void (*f)(UGWindow uwin, int width, int height)) {
    UGWindow_t *w = (UGWindow_t*)uwin;
    w->reshape = f;
}

void APIENTRY
ugDisplayFunc(UGWindow uwin, void (*f)(UGWindow uwin)) {
    UGWindow_t *w = (UGWindow_t*)uwin;
    w->draw = f;
}

void APIENTRY
ugKeyboardFunc(UGWindow uwin, void (*f)(UGWindow uwin, int key, int x, int y)) {
    UGWindow_t *w = (UGWindow_t*)uwin;
    w->kbd = f;
}

void APIENTRY
ugPointerFunc(UGWindow uwin, void (*f)(UGWindow uwin, int button, int state, int x, int y)) {
    UGWindow_t *w = (UGWindow_t*)uwin;
    w->pointer = f;
}

void APIENTRY
ugMotionFunc(UGWindow uwin, void (*f)(UGWindow uwin, int x, int y)) {
    UGWindow_t *w = (UGWindow_t*)uwin;
    w->motion = f;
}

void APIENTRY
ugIdleFunc(UGCtx ug, void (*f)(UGWindow w)) {
    UGCtx_t *_ug = (UGCtx_t*)ug;
    _ug->idle = f;
}

static int GetKey(KeySym vk) {
	switch (vk) {
	case XK_F1:			return UG_KEY_F1;
	case XK_F2:			return UG_KEY_F2;
	case XK_F3:			return UG_KEY_F3;
	case XK_F4:			return UG_KEY_F4;
	case XK_F5:			return UG_KEY_F5;
	case XK_F6:			return UG_KEY_F6;
	case XK_F7:			return UG_KEY_F7;
	case XK_F8:			return UG_KEY_F8;
	case XK_F9:			return UG_KEY_F9;
	case XK_F10:		return UG_KEY_F10;
	case XK_F11:		return UG_KEY_F11;
	case XK_F12:		return UG_KEY_F12;
	case XK_Left:		return UG_KEY_LEFT;
	case XK_Up:			return UG_KEY_UP;
	case XK_Right:		return UG_KEY_RIGHT;
	case XK_Down:		return UG_KEY_DOWN;
	case XK_Prior:		return UG_KEY_PAGE_UP;
	case XK_Next:		return UG_KEY_PAGE_DOWN;
	case XK_Home:		return UG_KEY_HOME;
	case XK_End:		return UG_KEY_END;
	case XK_Insert:		return UG_KEY_INSERT;
	default:			return vk;
	}
}

static int next_event_timeout(Display* dsp, XEvent* event_return, struct timeval* tv)
{
  // optimization
  if (tv == NULL) {
    XNextEvent(dsp, event_return);
    return 1;
  }

  // the real deal
  if (XPending(dsp) == 0) {
    int fd = ConnectionNumber(dsp);
    fd_set readset;
    FD_ZERO(&readset);
    FD_SET(fd, &readset);
    if (select(fd+1, &readset, NULL, NULL, tv) == 0) {
      return 0;
    } else {
      XNextEvent(dsp, event_return);
      return 1;
    }
  } else {
    XNextEvent(dsp, event_return);
    return 1;
  }
}

void APIENTRY
ugMainLoop(UGCtx ug) {
	  context = (UGCtx_t *) ug;
    UGWindow_t *w = context->win;
    int width,height;
    XEvent an_event;

    while (1) {
      struct timeval tv = { 0, 40000}; 
      if (next_event_timeout(context->dpy, &an_event, &tv)) {
        switch (an_event.type) {
        case Expose:
          if(w->draw){
            printf("call draw fun\n");
            (w->draw)((UGWindow) w);
          }
          break;
        case ConfigureNotify:
          width = an_event.xconfigure.width;
          height = an_event.xconfigure.height;
          if (w->width != width || w->height != height){
            printf("%d,%d,%d,%d\n",w->width,w->height,width,height);
            w->width = width;
            w->height = height;

            if (w->surface) {
              eglDestroySurface(w->ug->egldpy, w->surface);
              w->surface = eglCreateWindowSurface(w->ug->egldpy, w->eglconfig, (NativeWindowType)(w->win), 0);
              bind_context(ug,w);
            }

            if(w->reshape){
              printf("call reshape fun,%d,%d\n",width,height);
              (w->reshape)((UGWindow) w,width,height);
            }
          }
          break;
        case ButtonPress:
          if(w->pointer){
            XButtonEvent *xbtn = &an_event.xbutton;
            printf("call pointer fun\n");
            if(xbtn->button == Button1)
              w->pointer((UGWindow)w,UG_BUT_LEFT, UG_BUT_DOWN,xbtn->x,xbtn->y);
            else if(xbtn->button == Button2)
              w->pointer((UGWindow)w,UG_BUT_RIGHT,UG_BUT_DOWN,xbtn->x,xbtn->y);
            else if(xbtn->button == Button3)
              w->pointer((UGWindow)w,UG_BUT_MIDDLE,UG_BUT_DOWN,xbtn->x,xbtn->y);
          }
          break;
        case MotionNotify:
          break;
        case KeyPress:
          {
            KeySym keysym;
            char buf[128] = {0};
            XLookupString(&an_event.xkey, buf, sizeof buf, &keysym, NULL);
            if(w->kbd){
              int k = GetKey(keysym);
              printf("call kbd fun %d\n",k);
              w->kbd((UGWindow)w,k,0,0);
            }
            break;
          }
        default: /* ignore any other event types. */
          break;
        } /* end switch */
      } else {
        if(context->idle) {
          printf("call idle fun\n");
          context->idle((UGWindow)w);
        }
      }
    } /* end while events handling */
}

void APIENTRY
ugPostRedisplay(UGWindow uwin) {
    printf("post redisplay\n");
    UGWindow_t* w = (UGWindow_t*)uwin;
    XClearArea(w->ug->dpy, w->win, 0, 0, 1, 1, true);
    XFlush(w->ug->dpy);
}

void APIENTRY
ugSwapBuffers(UGWindow uwin) {
    UGWindow_t* w = (UGWindow_t*)uwin;
    eglSwapBuffers(w->ug->egldpy, w->surface);
}



