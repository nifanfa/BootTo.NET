#include "ug.h"



float xrot = 0.0f;

float yrot = 0.0f;



GLfloat triangle[] = {

	-0.25f, -0.25f, 0.0f,

	0.25f, -0.25f, 0.0f,

	-0.25f, 0.25f, 0.0f

};



GLfloat square[] = {

	-0.25f, -0.25f,

	0.25f, -0.25f,

	-0.25f, 0.25f,

	0.25f, 0.25f

};



GLfloat colors[] = {

	1.0f, 0.0f, 0.0f, 1.0f,

	0.0f, 1.0f, 0.0f, 1.0f,

	0.0f, 0.0f, 1.0f, 1.0f

};



void init()

{

	glClearColor (0.0f, 0.0f, 0.0f, 0.0f);

}



void display(UGWindow uwin)

{

	glClear (GL_COLOR_BUFFER_BIT);



	// Triangle

	glShadeModel(GL_SMOOTH);



	glVertexPointer(3, GL_FLOAT, 0, triangle);

	glColorPointer(4, GL_FLOAT, 0, colors);



	glEnableClientState(GL_VERTEX_ARRAY);

	glEnableClientState(GL_COLOR_ARRAY);



	glPushMatrix();

		glTranslatef(0.25f, 0.5f, 0.0f);

		glScalef(0.5f, 0.5f, 0.5f);

		glRotatef(xrot, 1.0f, 0.0f, 0.0f);

		glDrawArrays(GL_TRIANGLES, 0, 3);

	glPopMatrix();



	glDisableClientState(GL_COLOR_ARRAY);



	// Square

	glShadeModel(GL_FLAT);



	glVertexPointer(2, GL_FLOAT, 0, square);

	

	glColor4f(0.25f, 0.25f, 0.75f, 1.0f);



	glPushMatrix();

		glTranslatef(0.75f, 0.5f ,0.0f);

		glScalef(0.5f, 0.5f, 0.5f);

		glRotatef(yrot, 0.0f, 1.0f, 0.0f);

		glDrawArrays(GL_TRIANGLE_STRIP, 0, 4);

	glPopMatrix();



	glFlush ();

	ugSwapBuffers(uwin);

}



void idle(UGWindow uwin)

{

	xrot += 2.0f;

	yrot += 3.0f;



	ugPostRedisplay(uwin);

}



void keyboard(UGWindow uwin, int key, int x, int y)

{

	switch(key)

	{

	case 'q' : exit(0); break;

	case UG_KEY_UP : exit(0); break;

	}

}



void reshape(UGWindow uwin, int width, int height)

{

	glMatrixMode(GL_PROJECTION);

	glLoadIdentity();



	glViewport(0, 0, width, height);

	glOrthof(0.0f, 1.0f, 0.0f, 1.0f, -1.0f, 1.0f);



	glMatrixMode(GL_MODELVIEW);

	glLoadIdentity();

}



void pointer(UGWindow uwin, int button, int state, int x, int y)

{

	ugDestroyWindow(uwin);

	exit(0);

}



int main()

{

	UGCtx ug = ugInit();



	UGWindow uwin = ugCreateWindow(ug, "", "10 - Transformations", 250, 250, 100, 100);



	init();



	ugDisplayFunc(uwin, display);

	ugReshapeFunc(uwin, reshape);



	ugKeyboardFunc(uwin, keyboard);

	ugPointerFunc(uwin, pointer);



	ugIdleFunc(ug, idle);

	ugMainLoop(ug);



	return 0;

}
