#include "ug.h"

GLfloat triangle[] = {
	0.25f, 0.25f, 0.0f,
	0.75f, 0.25f, 0.0f,
	0.25f, 0.75f, 0.0f
};

GLfloat colors[] = {
	1.0f, 0.0f, 0.0f, 1.0f,
	0.0f, 1.0f, 0.0f, 1.0f,
	0.0f, 0.0f, 1.0f, 1.0f
};

bool shaded = false;

void init()
{
	glClearColor (0.0f, 0.0f, 0.0f, 0.0f);

	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	glOrthof(0.0f, 1.0f, 0.0f, 1.0f, -1.0f, 1.0f);

	glVertexPointer(3, GL_FLOAT, 0, triangle);
	glColorPointer(4, GL_FLOAT, 0, colors);
	glEnableClientState(GL_VERTEX_ARRAY);
	glEnableClientState(GL_COLOR_ARRAY);

	glShadeModel(GL_FLAT);
}

void display(UGWindow uwin)
{
   glClear (GL_COLOR_BUFFER_BIT);

   glDrawArrays(GL_TRIANGLES, 0, 3);

   glFlush ();
   ugSwapBuffers(uwin);
}

void keyboard(UGWindow uwin, int key, int x, int y)
{
	switch(key)
	{
	case 'q' : exit(0); break;
	case UG_KEY_UP : exit(0); break;
	case 's' : 
		shaded = !shaded;
		glShadeModel(shaded ? GL_SMOOTH : GL_FLAT);
		ugPostRedisplay(uwin);
		break;
	}
}

void pointer(UGWindow uwin, int button, int state, int x, int y)
{
	exit(0);
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

int main()
{
	UGCtx ug = ugInit();

	UGWindow uwin = ugCreateWindow(ug, "", "08 - Color and Shading", 250, 250, 100, 100);

	init();

	ugDisplayFunc(uwin, display);
	ugKeyboardFunc(uwin, keyboard);
	ugPointerFunc(uwin, pointer);
	ugReshapeFunc(uwin, reshape);

	ugMainLoop(ug);

	return 0;
}

