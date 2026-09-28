#include "ug.h"

void display(UGWindow uwin)
{
/* clear all pixels  */
   glClear (GL_COLOR_BUFFER_BIT);

/* draw white polygon (rectangle) with corners at
 * (0.25, 0.25, 0.0) and (0.75, 0.75, 0.0)  
 */
   glColor4f (1.0, 1.0, 1.0, 1.0);
   glDrawArrays(GL_TRIANGLE_STRIP, 0, 4);

/* don't wait!  
 * start processing buffered OpenGL routines 
 */
   glFlush ();
   ugSwapBuffers(uwin);
}

void init (void) 
{
   const static GLfloat v[] = {
	0.25, 0.25, 0.0,
	0.75, 0.25, 0.0,
	0.25, 0.75, 0.0,
	0.75, 0.75, 0.0
    };
/* select clearing color 	*/
   glClearColor (0.0, 0.0, 0.0, 0.0);

/* initialize viewing values  */
   glMatrixMode(GL_PROJECTION);
   glLoadIdentity();
   glOrthof(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
   glVertexPointer(3, GL_FLOAT, 0, v);
   glEnableClientState(GL_VERTEX_ARRAY);
}

/* 
 * Declare initial window size, position, and display mode
 * (single buffer and RGBA).  Open window with "hello"
 * in its title bar.  Call initialization routines.
 * Register callback function to display graphics.
 * Enter main loop and process events.
 */
int main(int argc, char** argv)
{
   UGCtx ug = ugInit();
   UGWindow uwin = ugCreateWindow (ug, "", "hello", 250, 250, 100, 100);
   init();
   ugDisplayFunc(uwin, display); 
   ugMainLoop(ug);
   return 0;   /* ANSI C requires main to return int. */
}
