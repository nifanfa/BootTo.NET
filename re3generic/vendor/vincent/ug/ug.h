/*
 * GLESonGL implementation
 * Version:  1.0
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

#ifndef __ug_h__
#define __ug_h__

#include "GLES/gl.h"
#include "GLES/egl.h"

#ifdef _WIN32
#ifndef APIENTRY
#define APIENTRY __stdcall
#endif
#else
#define APIENTRY
#endif

#ifdef __cplusplus
extern "C" {
#endif

#define UG_KEY_F1                       201
#define UG_KEY_F2                       202
#define UG_KEY_F3                       203
#define UG_KEY_F4                       204
#define UG_KEY_F5                       205
#define UG_KEY_F6                       206
#define UG_KEY_F7                       207
#define UG_KEY_F8                       208
#define UG_KEY_F9                       209
#define UG_KEY_F10                      210
#define UG_KEY_F11                      211
#define UG_KEY_F12                      211

    /* directional keys */
#define UG_KEY_LEFT                     300
#define UG_KEY_UP                       301
#define UG_KEY_RIGHT                    302
#define UG_KEY_DOWN                     303
#define UG_KEY_PAGE_UP                  304
#define UG_KEY_PAGE_DOWN                305
#define UG_KEY_HOME                     306
#define UG_KEY_END                      307
#define UG_KEY_INSERT                   308

    /* pointer buttons */
#define UG_BUT_LEFT                     0
#define UG_BUT_MIDDLE                   1
#define UG_BUT_RIGHT                    2

    /* pointer button state */
#define UG_BUT_DOWN                     0
#define UG_BUT_UP                       1


typedef long UGCtx;
typedef long UGWindow;

extern UGCtx APIENTRY ugInit(void);
extern void APIENTRY ugFini(UGCtx ug);

extern UGCtx APIENTRY ugCtxFromWin(UGWindow uwin);

extern UGWindow APIENTRY ugCreateWindow(UGCtx ug,  const char* config,
		const char* title, int width, int height, int x, int y);
extern void APIENTRY ugDestroyWindow(UGWindow uwin);

extern void APIENTRY ugDisplayFunc(UGWindow uwin, void (*f)(UGWindow uwin));
extern void APIENTRY ugReshapeFunc(UGWindow uwin, void (*f)(UGWindow uwin, int width, int height));
extern void APIENTRY ugKeyboardFunc(UGWindow uwin, void (*f)(UGWindow uwin, int key, int x, int y));
extern void APIENTRY ugPointerFunc(UGWindow uwin, void (*f)(UGWindow uwin, int button, int state, int x, int y));
extern void APIENTRY ugMotionFunc(UGWindow uwin, void (*f)(UGWindow uwin, int x, int y));
extern void APIENTRY ugIdleFunc(UGCtx ug, void (*f)(UGWindow uwin));

extern void APIENTRY ugMainLoop(UGCtx ug);

extern void APIENTRY ugPostRedisplay(UGWindow uwin);
extern void APIENTRY ugSwapBuffers(UGWindow uwin);


extern void APIENTRY ugSolidBox(GLfloat Width, GLfloat Depth, GLfloat Height);
extern void APIENTRY ugSolidConef(GLfloat base, GLfloat height, GLint slices, GLint stacks);
extern void APIENTRY ugSolidCubef(GLfloat size);
extern void APIENTRY ugSolidDisk(GLfloat inner_radius, GLfloat outer_radius, GLshort rings, GLshort slices);
extern void APIENTRY ugSolidSpheref(GLfloat radius, GLint slices, GLint stacks);
extern void APIENTRY ugSolidTorusf(GLfloat ir, GLfloat orr, GLint sides, GLint rings);
extern void APIENTRY ugSolidTube(GLfloat radius, GLfloat height, GLshort stacks, GLshort slices);

extern void APIENTRY ugWireBox(GLfloat Width, GLfloat Depth, GLfloat Height);
extern void APIENTRY ugWireConef(GLfloat base, GLfloat height, GLint slices, GLint stacks);
extern void APIENTRY ugWireCubef(GLfloat size);
extern void APIENTRY ugWireDisk(GLfloat inner_radius, GLfloat outer_radius, GLshort rings, GLshort slices);
extern void APIENTRY ugWireSpheref(GLfloat radius, GLint slices, GLint stacks);
extern void APIENTRY ugWireTorusf(GLfloat ir, GLfloat orr, GLint sides, GLint rings);
extern void APIENTRY ugWireTube(GLfloat radius, GLfloat height, GLshort stacks, GLshort slices);


extern void APIENTRY ugluPerspectivef(GLfloat fovy, GLfloat aspect, GLfloat n, GLfloat f);
extern void APIENTRY ugluPerspectivex(GLfixed fovy, GLfixed aspect, GLfixed n, GLfixed f);
extern void APIENTRY ugluLookAtf(GLfloat eyex, GLfloat eyey, GLfloat eyez, GLfloat centerx,
    GLfloat centery, GLfloat centerz, GLfloat upx, GLfloat upy, GLfloat upz);
extern void APIENTRY ugluLookAtx(GLfixed eyex, GLfixed eyey, GLfixed eyez, GLfixed centerx,
    GLfixed centery, GLfixed centerz, GLfixed upx, GLfixed upy, GLfixed upz);

#ifdef __cplusplus
};
#endif

#endif /*__ug_h__*/

