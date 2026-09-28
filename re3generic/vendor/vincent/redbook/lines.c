/*
 * License Applicability. Except to the extent portions of this file are
 * made subject to an alternative license as permitted in the SGI Free
 * Software License B, Version 1.1 (the "License"), the contents of this
 * file are subject only to the provisions of the License. You may not use
 * this file except in compliance with the License. You may obtain a copy
 * of the License at Silicon Graphics, Inc., attn: Legal Services, 1600
 * Amphitheatre Parkway, Mountain View, CA 94043-1351, or at:
 * 
 * http://oss.sgi.com/projects/FreeB
 * 
 * Note that, as provided in the License, the Software is distributed on an
 * "AS IS" basis, with ALL EXPRESS AND IMPLIED WARRANTIES AND CONDITIONS
 * DISCLAIMED, INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES AND
 * CONDITIONS OF MERCHANTABILITY, SATISFACTORY QUALITY, FITNESS FOR A
 * PARTICULAR PURPOSE, AND NON-INFRINGEMENT.
 * 
 * Original Code. The Original Code is: OpenGL Sample Implementation,
 * Version 1.2.1, released January 26, 2000, developed by Silicon Graphics,
 * Inc. The Original Code is Copyright (c) 1991-2000 Silicon Graphics, Inc.
 * Copyright in any portions created by third parties is as indicated
 * elsewhere herein. All Rights Reserved.
 * 
 * Additional Notice Provisions: The application programming interfaces
 * established by SGI in conjunction with the Original Code are The
 * OpenGL(R) Graphics System: A Specification (Version 1.2.1), released
 * April 1, 1999; The OpenGL(R) Graphics System Utility Library (Version
 * 1.3), released November 4, 1998; and OpenGL(R) Graphics with the X
 * Window System(R) (Version 1.3), released October 19, 1998. This software
 * was created using the OpenGL(R) version 1.2.1 Sample Implementation
 * published by SGI, but has not been independently verified as being
 * compliant with the OpenGL(R) version 1.2.1 Specification.
 *
 */

/*
 *  lines.c
 *  This program demonstrates geometric primitives and
 *  their attributes.
 */
#include <stdlib.h>
#include "ug.h"

#if 0
#define drawOneLine(x1,y1,x2,y2)  glBegin(GL_LINES);  \
   glVertex2f ((x1),(y1)); glVertex2f ((x2),(y2)); glEnd();
#else
static void drawOneLine(float x1, float y1, float x2, float y2) {
   GLfloat v[4];
   v[0] = x1; v[1] = y1; v[2] = x2; v[3] = y2;
   glVertexPointer(2, GL_FLOAT, 0, v);
   glEnableClientState(GL_VERTEX_ARRAY);
   glDrawArrays(GL_LINES, 0, 2);
}
#endif

void init(void) 
{
   glClearColor (0.0, 0.0, 0.0, 0.0);
   glShadeModel (GL_FLAT);
}

void display(UGWindow uwin)
{
   int i;

   glClear (GL_COLOR_BUFFER_BIT);

/* select white for all lines  */
   glColor4f (1.0, 1.0, 1.0, 1.0);

/* in 1st row, 3 lines, each with a different stipple  */
   //glEnable (GL_LINE_STIPPLE);
   
   //glLineStipple (1, 0x0101);  /*  dotted  */
   glColor4f(1., 0., 0., 1.);
   drawOneLine (50.0, 125.0, 150.0, 125.0);
   //glLineStipple (1, 0x00FF);  /*  dashed  */
   glColor4f(0., 1., 0., 1.);
   drawOneLine (150.0, 125.0, 250.0, 125.0);
   //glLineStipple (1, 0x1C47);  /*  dash/dot/dash  */
   glColor4f(0., 0., 1., 1.);
   drawOneLine (250.0, 125.0, 350.0, 125.0);

   glColor4f(1., 1., 1., 1.);
/* in 2nd row, 3 wide lines, each with different stipple */
   glLineWidth (5.0);
   //glLineStipple (1, 0x0101);  /*  dotted  */
   drawOneLine (50.0, 100.0, 150.0, 100.0);
   //glLineStipple (1, 0x00FF);  /*  dashed  */
   glColor4f(1., 1., 0., 1.);
   drawOneLine (150.0, 100.0, 250.0, 100.0);
   //glLineStipple (1, 0x1C47);  /*  dash/dot/dash  */
   glColor4f(0., 1., 1., 1.);
   drawOneLine (250.0, 100.0, 350.0, 100.0);
   glLineWidth (1.0);
   glColor4f(1., 0., 1., 1.);

/* in 3rd row, 6 lines, with dash/dot/dash stipple  */
/* as part of a single connected line strip         */
   //glLineStipple (1, 0x1C47);  /*  dash/dot/dash  */
#if 0
   glBegin (GL_LINE_STRIP);
   for (i = 0; i < 7; i++)
      glVertex2f (50.0 + ((GLfloat) i * 50.0), 75.0);
   glEnd ();
#else
   {
   GLfloat v[14];
   for (i = 0; i < 7; i++) {
      v[2*i] = 50.0 + ((GLfloat) i * 50.0); v[2*i+1] = 75.0;
   }
   glVertexPointer(2, GL_FLOAT, 0, v);
   glEnableClientState(GL_VERTEX_ARRAY);
   glDrawArrays(GL_LINE_STRIP, 0, 7);
   }
#endif

   glColor4f(1., 1., 1., 1.);
/* in 4th row, 6 independent lines with same stipple  */
   for (i = 0; i < 6; i++) {
      drawOneLine (50.0 + ((GLfloat) i * 50.0), 50.0,
         50.0 + ((GLfloat)(i+1) * 50.0), 50.0);
   }

   glColor4f(.8, .8, .8, 1.);
/* in 5th row, 1 line, with dash/dot/dash stipple    */
/* and a stipple repeat factor of 5                  */
   //glLineStipple (5, 0x1C47);  /*  dash/dot/dash  */
   drawOneLine (50.0, 25.0, 350.0, 25.0);

   //glDisable (GL_LINE_STIPPLE);
   glFlush ();
   ugSwapBuffers(uwin);
}

void reshape (UGWindow uwin, int w, int h)
{
   glViewport (0, 0, (GLsizei) w, (GLsizei) h);
   glMatrixMode (GL_PROJECTION);
   glLoadIdentity ();
   glOrthof (0.0, (GLfloat) w, 0.0, (GLfloat) h, -1.0, 1.0);
}

void keyboard(UGWindow uwin, int key, int x, int y)
{
   switch (key) {
      case 27:
         exit(0);
         break;
   }
}

int main(int argc, char** argv)
{
   UGCtx ug = ugInit();
   UGWindow uwin = ugCreateWindow (ug, "", argv[0], 450, 150, 100, 100);
   init ();
   ugDisplayFunc(uwin, display); 
   ugReshapeFunc(uwin, reshape);
   ugKeyboardFunc(uwin, keyboard);
   ugMainLoop(ug);
   return 0;
}
