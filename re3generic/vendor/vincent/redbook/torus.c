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
 *  torus.c
 *  This program demonstrates the drawing of a torus.
 */

#include <stdio.h>
#include <math.h>
#include <stdlib.h>
#include <math.h>
#include "ug.h"

#define PI_ 3.14159265358979323846


/* Draw a torus */
static void torus(int numc, int numt)
{
   int i, j, k;
   double s, t, x, y, z, twopi;
   static GLfloat* v;
   GLfloat* p;

   if (!v) {
       p = v = malloc(numc*(numt+1)*2*3*sizeof *v);
       twopi = 2 * PI_;
       for (i = 0; i < numc; i++) {
	  for (j = 0; j <= numt; j++) {
	     for (k = 1; k >= 0; k--) {
		s = (i + k) % numc + 0.5;
		t = j % numt;

		x = (1+.1*cos(s*twopi/numc))*cos(t*twopi/numt);
		y = (1+.1*cos(s*twopi/numc))*sin(t*twopi/numt);
		z = .1 * sin(s * twopi / numc);
		*p++ = x;
		*p++ = y;
		*p++ = z;
	     }
	  }
       }
       glVertexPointer(3, GL_FLOAT, 0, v);
       glEnableClientState (GL_VERTEX_ARRAY);
    }
    glDrawArrays(GL_TRIANGLE_STRIP, 0, numc*(numt+1)*2);
}

/* Initialize state */
static void init(void)
{
   glShadeModel(GL_FLAT);
   glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
}

/* Clear window and draw torus */
void display(UGWindow win)
{
   glClear(GL_COLOR_BUFFER_BIT);
   glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
   torus(8, 25);
   glFlush();
   ugSwapBuffers(win);
}

/* Handle window resize */
void reshape(UGWindow uwin, int w, int h)
{
printf("reshape\n");
   glViewport(0, 0, (GLsizei) w, (GLsizei) h);
   glMatrixMode(GL_PROJECTION);
   glLoadIdentity();
   ugluPerspectivef(30, (GLfloat) w/(GLfloat) h, 1.0, 100.0);
   glMatrixMode(GL_MODELVIEW);
   glLoadIdentity();
   ugluLookAtf(0, 0, 10, 0, 0, 0, 0, 1, 0);
}

/* Rotate about x-axis when "x" typed; rotate about y-axis
   when "y" typed; "i" returns torus to original view */
void keyboard(UGWindow uwin, int key, int x, int y)
{
   switch (key) {
   case 'x':
   case 'X':
      glRotatef(30.,1.0,0.0,0.0);
      ugPostRedisplay(uwin);
      break;
   case 'y':
   case 'Y':
      glRotatef(30.,0.0,1.0,0.0);
      ugPostRedisplay(uwin);
      break;
   case 'i':
   case 'I':
      glLoadIdentity();
      ugluLookAtf(0, 0, 10, 0, 0, 0, 0, 1, 0);
      ugPostRedisplay(uwin);
      break;
   case 27:
      exit(0);
      break;
   }
}

int main(int argc, char **argv)
{
   UGCtx ug = ugInit();
   UGWindow win = ugCreateWindow(ug, "", argv[0], 200, 200, 0, 0);
   init();
   ugReshapeFunc(win, reshape);
   ugKeyboardFunc(win, keyboard);
   ugDisplayFunc(win, display);
   ugMainLoop(ug);
   return 0;
}
