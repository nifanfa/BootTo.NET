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
 *  model.c
 *  This program demonstrates modeling transformations
 */
#include <stdlib.h>
#include "ug.h"

void init(void) 
{
   static const GLfloat v[6] = { 0.0, 25.0, 25.0, -25.0, -25.0, -25.0 };
   glClearColor (0.0, 0.0, 0.0, 0.0);
   glShadeModel (GL_FLAT);
   glVertexPointer(2, GL_FLOAT, 0, v);
   glEnableClientState (GL_VERTEX_ARRAY);
}

void draw_triangle(void)
{
   glDrawArrays(GL_LINE_LOOP, 0, 3);
}

void display(UGWindow uwin)
{
   glClear (GL_COLOR_BUFFER_BIT);
   glColor4f (1.0, 1.0, 1.0, 1.0);

   glLoadIdentity ();
   glColor4f (1.0, 1.0, 1.0, 1.0);
   draw_triangle ();

   /*glEnable (GL_LINE_STIPPLE);*/
   /*glLineStipple (1, 0xF0F0);*/
   glColor4f(1.0f, 0.f, 0.f, 1.f);
   glLoadIdentity ();
   glTranslatef (-20.0, 0.0, 0.0);
   draw_triangle ();

   /*glLineStipple (1, 0xF00F);*/
   glColor4f(0.0f, 1.f, 0.f, 1.f);
   glLoadIdentity ();
   glScalef (1.5, 0.5, 1.0);
   draw_triangle ();

   /*glLineStipple (1, 0x8888);*/
   glColor4f(0.0f, 0.f, 1.f, 1.f);
   glLoadIdentity ();
   glRotatef (90.0, 0.0, 0.0, 1.0);
   draw_triangle ();
   /*glDisable (GL_LINE_STIPPLE);*/

   glFlush ();
   ugSwapBuffers(uwin);
}

void reshape (UGWindow uwin, int w, int h)
{
   glViewport (0, 0, (GLsizei) w, (GLsizei) h);
   glMatrixMode (GL_PROJECTION);
   glLoadIdentity ();
   if (w <= h)
      glOrthof(-50.0, 50.0, -50.0*(GLfloat)h/(GLfloat)w,
         50.0*(GLfloat)h/(GLfloat)w, -1.0, 1.0);
   else
      glOrthof(-50.0*(GLfloat)w/(GLfloat)h,
         50.0*(GLfloat)w/(GLfloat)h, -50.0, 50.0, -1.0, 1.0);
   glMatrixMode(GL_MODELVIEW);
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
   UGWindow uwin = ugCreateWindow (ug, "", argv[0], 500, 500, 100, 100);
   init ();
   ugDisplayFunc(uwin, display); 
   ugReshapeFunc(uwin, reshape);
   ugKeyboardFunc(uwin, keyboard);
   ugMainLoop(ug);
   return 0;
}
