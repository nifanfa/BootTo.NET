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
 *  polyoff.c
 *  This program demonstrates polygon offset to draw a shaded
 *  polygon and its wireframe counterpart without ugly visual
 *  artifacts ("stitching").
 */
#include <stdio.h>
#include <stdlib.h>
#include "ug.h"

#if defined(GL_VERSION_1_1) || defined(GL_OES_VERSION_1_0)
GLuint list;
GLint spinx = 0;
GLint spiny = 0;
GLfloat tdist = 0.0;
GLfloat polyfactor = 1.0;
GLfloat polyunits = 1.0;

/*  display() draws two spheres, one with a gray, diffuse material,
 *  the other sphere with a magenta material with a specular highlight.
 */
void display (UGWindow uwin)
{
    GLfloat mat_ambient[] = { 0.8, 0.8, 0.8, 1.0 };
    GLfloat mat_diffuse[] = { 1.0, 0.0, 0.5, 1.0 };
    GLfloat mat_specular[] = { 1.0, 1.0, 1.0, 1.0 };
    GLfloat gray[] = { 0.8, 0.8, 0.8, 1.0 };
    GLfloat black[] = { 0.0, 0.0, 0.0, 1.0 };

    glClear (GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glShadeModel(GL_SMOOTH);
    glPushMatrix ();
    glTranslatef (0.0, 0.0, tdist);
    glRotatef ((GLfloat) spinx, 1.0, 0.0, 0.0);
    glRotatef ((GLfloat) spiny, 0.0, 1.0, 0.0);

    glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE, gray);
    glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, black);
    glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, 0.0);
    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);
    glEnable(GL_POLYGON_OFFSET_FILL);
    glPolygonOffset(polyfactor, polyunits);
    ugSolidSpheref(1.0, 12, 20);
    glDisable(GL_POLYGON_OFFSET_FILL);

    glDisable(GL_LIGHTING);
    glDisable(GL_LIGHT0);
    glColor4f (1.0, 1.0, 1.0, 1.0);
    //glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
    ugWireSpheref(1.0, 12, 20);
    //glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

    glPopMatrix ();
    glFlush ();
    ugSwapBuffers(uwin);
}

/*  specify initial properties
 *  create display list with sphere  
 *  initialize lighting and depth buffer
 */
void gfxinit (void)
{
    GLfloat light_ambient[] = { 0.0, 0.0, 0.0, 1.0 };
    GLfloat light_diffuse[] = { 1.0, 1.0, 1.0, 1.0 };
    GLfloat light_specular[] = { 1.0, 1.0, 1.0, 1.0 };
    GLfloat light_position[] = { 1.0, 1.0, 1.0, 0.0 };

    GLfloat global_ambient[] = { 0.2, 0.2, 0.2, 1.0 };

    glClearColor (0.0, 0.0, 0.0, 1.0);

    glEnable(GL_DEPTH_TEST);

    glLightfv (GL_LIGHT0, GL_AMBIENT, light_ambient);
    glLightfv (GL_LIGHT0, GL_DIFFUSE, light_diffuse);
    glLightfv (GL_LIGHT0, GL_SPECULAR, light_specular);
    glLightfv (GL_LIGHT0, GL_POSITION, light_position);
    glLightModelfv (GL_LIGHT_MODEL_AMBIENT, global_ambient);
}

/*  call when window is resized  */
void reshape(UGWindow uwin, int width, int height)
{
    glViewport (0, 0, width, height);
    glMatrixMode (GL_PROJECTION);
    glLoadIdentity ();
    ugluPerspectivef(45.0, (GLfloat)width/(GLfloat)height,
	    1.0, 10.0);
    glMatrixMode (GL_MODELVIEW);
    glLoadIdentity ();
    ugluLookAtf(0.0, 0.0, 5.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
}

/*  call when mouse button is pressed  */
void mouse(UGWindow uwin, int button, int state, int x, int y) {
    switch (button) {
	case UG_BUT_LEFT:
	    switch (state) {
		case UG_BUT_DOWN:
		    spinx = (spinx + 5) % 360; 
                    ugPostRedisplay(uwin);
		    break;
		default:
		    break;
            }
            break;
	case UG_BUT_MIDDLE:
	    switch (state) {
		case UG_BUT_DOWN:
		    spiny = (spiny + 5) % 360; 
                    ugPostRedisplay(uwin);
		    break;
		default:
		    break;
            }
            break;
	case UG_BUT_RIGHT:
	    switch (state) {
		case UG_BUT_UP:
		    exit(0);
		    break;
		default:
		    break;
            }
            break;
        default:
            break;
    }
}

void keyboard (UGWindow uwin, int key, int x, int y)
{
   switch (key) {
      case 't':
         if (tdist < 4.0) {
            tdist = (tdist + 0.5);
            ugPostRedisplay(uwin);
         }
         break;
      case 'T':
         if (tdist > -5.0) {
            tdist = (tdist - 0.5);
            ugPostRedisplay(uwin);
         }
         break;
      case 'F':
         polyfactor = polyfactor + 0.1;
	 printf ("polyfactor is %f\n", polyfactor);
         ugPostRedisplay(uwin);
         break;
      case 'f':
         polyfactor = polyfactor - 0.1;
	 printf ("polyfactor is %f\n", polyfactor);
         ugPostRedisplay(uwin);
         break;
      case 'U':
         polyunits = polyunits + 1.0;
	 printf ("polyunits is %f\n", polyunits);
         ugPostRedisplay(uwin);
         break;
      case 'u':
         polyunits = polyunits - 1.0;
	 printf ("polyunits is %f\n", polyunits);
         ugPostRedisplay(uwin);
         break;
      case '\033':
	 exit(0);
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
    UGWindow uwin = ugCreateWindow (ug, "UG_DEPTH", argv[0], 500, 500, 100, 100);
    gfxinit();
    ugReshapeFunc(uwin, reshape);
    ugDisplayFunc(uwin, display); 
    ugPointerFunc(uwin, mouse);
    ugKeyboardFunc(uwin, keyboard);
    ugMainLoop(ug);
    return 0;
}
#else
int main(int argc, char** argv)
{
    fprintf (stderr, "This program demonstrates a feature which is not in OpenGL Version 1.0.\n");
    fprintf (stderr, "If your implementation of OpenGL Version 1.0 has the right extensions,\n");
    fprintf (stderr, "you may be able to modify this program to make it run.\n");
    return 0;
}
#endif
