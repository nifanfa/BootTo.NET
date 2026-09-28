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
 *  aargb.c
 *  This program draws shows how to draw anti-aliased lines. It draws
 *  two diagonal lines to form an X; when 'r' is typed in the window, 
 *  the lines are rotated in opposite directions.
 */
#include <stdlib.h>
#include <stdio.h>
#include "ug.h"

static float rotAngle = 0.;

/*  Initialize antialiasing for RGBA mode, including alpha
 *  blending, hint, and line width.  Print out implementation
 *  specific info on line width granularity and width.
 */
   static const GLfloat v1[] = {
      -0.5, 0.5,
      0.5, -0.5
	};

   static const GLfloat v2[] = {
	  0.5, 0.5,
	  -0.5,-0.5
   };

void init(void)
{

   GLint values[2];
#if 0
   glGetIntegerv (GL_SMOOTH_LINE_WIDTH_GRANULARITY, values);
   printf ("GL_LINE_WIDTH_GRANULARITY value is %d\n", values[0]);
#endif

   glGetIntegerv (GL_SMOOTH_LINE_WIDTH_RANGE, values);
   printf ("GL_LINE_WIDTH_RANGE values are %d %d\n",
      values[0], values[1]);

   glEnable(GL_LINE_SMOOTH);
   glEnable(GL_BLEND);
   glBlendFunc (GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
   glHint(GL_LINE_SMOOTH_HINT, GL_DONT_CARE);
   glLineWidth (1.5);

   glClearColor(0.0, 0.0, 0.0, 0.0);
}

/* Draw 2 diagonal lines to form an X
 */
void display(UGWindow uwin)
{

   glClear(GL_COLOR_BUFFER_BIT);

   glVertexPointer(2, GL_FLOAT, 0, v1);
   glEnableClientState(GL_VERTEX_ARRAY);

   glColor4f(0.0, 1.0, 0.0, 1.0);
   glPushMatrix();
   glRotatef(-rotAngle, 0.0, 0.0, 0.1);
   glDrawArrays(GL_LINES, 0, 2);
   glPopMatrix();

   glDisableClientState(GL_VERTEX_ARRAY);


   glVertexPointer(2, GL_FLOAT, 0, v2);
   glEnableClientState(GL_VERTEX_ARRAY);

   glColor4f (0.0, 0.0, 1.0, 10);
   glPushMatrix();
   glRotatef(rotAngle, 0.0, 0.0, 0.1);
   glDrawArrays(GL_LINES, 0, 2);
   glPopMatrix();

   glDisableClientState(GL_VERTEX_ARRAY);


   glFlush();
   ugSwapBuffers(uwin);
}

void reshape(UGWindow uwin, int w, int h)
{
   glViewport(0, 0, w, h);
   glMatrixMode(GL_PROJECTION);
   glLoadIdentity();
   if (w <= h) 
   {
      glOrthof (-1.0, 1.0, 
         -1.0*(GLfloat)h/(GLfloat)w, 1.0*(GLfloat)h/(GLfloat)w, -1.0, 1.0);

//	 glClearColor(0.0, 1.0, 0.0, 0.0);

	}
	else 
	{
		glOrthof (-1.0*(GLfloat)w/(GLfloat)h,  
							1.0*(GLfloat)w/(GLfloat)h, -1.0, 1.0, -1.0, 1.0);

//		glClearColor(0.0, 0.0, 1.0, 0.0);
	}
   glMatrixMode(GL_MODELVIEW);
   glLoadIdentity();
}

void keyboard(UGWindow uwin, int key, int x, int y)
{
   switch (key) {
      case 'r':
      case 'R':
         rotAngle += 20.;
         if (rotAngle >= 360.) rotAngle = 0.;
         ugPostRedisplay(uwin);	
         break;
      case 27:  /*  Escape Key  */
         exit(0);
         break;
      default:
         break;
    }
}

/*  Main Loop
 *  Open window with initial window size, title bar, 
 *  RGBA display mode, and handle input events.
 */
int main(int argc, char** argv)
{
   UGCtx ug = ugInit();
   UGWindow uwin = ugCreateWindow (ug, "", argv[0], 200, 200, 100, 100);
   init();
   ugDisplayFunc(uwin, display); 
   ugReshapeFunc(uwin, reshape);
   ugKeyboardFunc(uwin, keyboard);
   ugMainLoop(ug);
   return 0;
}
