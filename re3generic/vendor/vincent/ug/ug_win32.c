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

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <GLES/egl.h>
#include <Windows.h>
#include <aygshell.h>
//#include <shellsdk.h>
#include <commdlg.h>


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

typedef struct ugwindow {
    HWND win;
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

static wchar_t WINDOW_CLASS_NAME[] = L"ug window";
static SHACTIVATEINFO s_sai;
static HINSTANCE instance;
static UGCtx_t * context;

static const int TITLEBARHEIGHT = 26;

typedef EGLBoolean (__cdecl *PFNSAVESURFACE)(EGLSurface surface, const TCHAR * filename);
static PFNSAVESURFACE pfnSaveSurface;	 // function pointer used to save images 



UGCtx APIENTRY
ugInit(void) {
    UGCtx_t* ug = malloc(sizeof *ug);
    if (ug) {
		memset(ug, 0, sizeof *ug);
		if (!(ug->dpy = GetDC(0))) {
abort:
			free(ug);
			return 0;
		}
		ug->egldpy = eglGetDisplay(ug->dpy);

		if (!eglInitialize(ug->egldpy, NULL, NULL)) 
			goto abort;
    }
						  // intialize function pointer used to save images
	pfnSaveSurface = (int (__cdecl *)(void *,const unsigned short *))eglGetProcAddress("eglSaveSurfaceHM");

	context = ug;
    return (UGCtx)ug;
}

void APIENTRY
ugFini(UGCtx ug) {
    /*XXXblythe open windows?*/
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


void UpdateWindowPosition(HWND hWnd)
{
    SIPINFO si;
    int cx, cy;
	
   // Query the sip state and size our window appropriately.
    memset (&si, 0, sizeof (si));
    si.cbSize = sizeof (si);
    SHSipInfo(SPI_GETSIPINFO, 0, (PVOID)&si, FALSE); 
    cx = si.rcVisibleDesktop.right - si.rcVisibleDesktop.left;
    cy = si.rcVisibleDesktop.bottom - si.rcVisibleDesktop.top;

    // If the sip is not shown, or showing but not docked, the
    // desktop rect doesn’t include the height of the menu bar.
    if (!(si.fdwFlags & SIPF_ON) ||
        ((si.fdwFlags & SIPF_ON) && !(si.fdwFlags & SIPF_DOCKED))) 
        cy -= TITLEBARHEIGHT;  
    
    SetWindowPos (hWnd, NULL, 0, TITLEBARHEIGHT, cx, cy, 
																				SWP_NOMOVE | SWP_NOZORDER);

}

UGWindow APIENTRY
ugCreateWindow(UGCtx ug,  const char* config,
		const char* title, int width, int height, int x, int y) {
    UGWindow_t *w = malloc(sizeof *w);
    UGCtx_t* _ug = (UGCtx_t*)ug;


    SHMENUBARINFO mbi;		  

    if (w) {
		EGLint n, vid;
		EGLConfig configs[1];
		wchar_t * str;
		int size;
		RECT rect;

		/*XXXblythe horrible hack, need to parse config string*/
		static int attribs[] = { EGL_RED_SIZE, 1, EGL_NONE }; /*XXXblythe*/
		static int attribs2[] = {EGL_RED_SIZE, 1, EGL_DEPTH_SIZE, 1, EGL_NONE};
		int depth = strstr(config, "UG_DEPTH") != 0;

		memset(w, 0, sizeof *w);
		w->ug = _ug;
		if (!eglChooseConfig(_ug->egldpy, depth ? attribs2 : attribs, configs, 1, &n)) {
			free(w);
			return 0;
		}
		w->eglconfig = configs[0];
		eglGetConfigAttrib(_ug->egldpy, configs[0], EGL_NATIVE_VISUAL_ID, &vid);

		size = MultiByteToWideChar( CP_ACP, 0, title, -1,
			0, 0);
		str = malloc(size * sizeof(wchar_t));
		MultiByteToWideChar( CP_ACP, 0, title, -1,
			str, size);

		w->win = CreateWindow(WINDOW_CLASS_NAME, str, WS_VISIBLE,
			//x, y, width, height, 
			CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, 
			NULL, NULL, instance, NULL);

		free(str);

		if (!w->win)
		{	
			free(w);
			return 0;
		}

		SetTimer(w->win, 1, 40, NULL);

        //Create the menubar.
        memset (&mbi, 0, sizeof (SHMENUBARINFO));
        mbi.cbSize     = sizeof (SHMENUBARINFO);
        mbi.hwndParent = w->win;
        mbi.hInstRes   = instance;
        mbi.dwFlags =  SHCMBF_EMPTYBAR;  // no menu, just the sip button
        mbi.nBmpId     = 0;
        mbi.cBmpImages = 0;    

        if (!SHCreateMenuBar(&mbi))
        {
            MessageBox(w->win, L"SHCreateMenuBar Failed", L"Error", MB_OK);
            SendMessage(w->win, WM_QUIT, 0, 0);
			return 0;
        }

		UpdateWindowPosition(w->win);			 // reset position after creating menubar

		w->next = _ug->winlist;
		_ug->winlist = w;
		w->prev = 0;
		if (w->next) w->next->prev = w;

		GetClientRect(w->win, &rect);
		w->width = rect.right - rect.left;
		w->height = rect.bottom - rect.top;

		w->surface = eglCreateWindowSurface(_ug->egldpy, w->eglconfig, (NativeWindowType)(w->win), 0);
		/*XXXblythe share*/
		w->eglctx = eglCreateContext(_ug->egldpy, w->eglconfig, NULL, NULL);
		bind_context(_ug, w);
    }
    return (UGWindow)w;
}

void APIENTRY
ugDestroyWindow(UGWindow uwin) {
    UGWindow_t* w = (UGWindow_t*)uwin;
    UGCtx_t* ug = w->ug;
    eglDestroySurface(ug->egldpy, w->surface);
	DestroyWindow(w->win);
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

static int GetKey(int vk) {
	switch (vk) {
	case VK_F1:			return UG_KEY_F1;
	case VK_F2:			return UG_KEY_F2;
	case VK_F3:			return UG_KEY_F3;
	case VK_F4:			return UG_KEY_F4;
	case VK_F5:			return UG_KEY_F5;
	case VK_F6:			return UG_KEY_F6;
	case VK_F7:			return UG_KEY_F7;
	case VK_F8:			return UG_KEY_F8;
	case VK_F9:			return UG_KEY_F9;
	case VK_F10:		return UG_KEY_F10;
	case VK_F11:		return UG_KEY_F11;
	case VK_F12:		return UG_KEY_F12;
	case VK_LEFT:		return UG_KEY_LEFT;
	case VK_UP:			return UG_KEY_UP;
	case VK_RIGHT:		return UG_KEY_RIGHT;
	case VK_DOWN:		return UG_KEY_DOWN;
	case VK_PRIOR:		return UG_KEY_PAGE_UP;
	case VK_NEXT:		return UG_KEY_PAGE_DOWN;
	case VK_HOME:		return UG_KEY_HOME;
	case VK_END:		return UG_KEY_END;
	case VK_INSERT:		return UG_KEY_INSERT;
	default:			return 0;
	}
}

//
//  FUNCTION: WndProc(HWND, unsigned, WORD, LONG)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	UGWindow_t* w;
	int key;
	POINT point;
	WORD fActive;
	int width, height;

	for(w = context->winlist; w; w = w->next) {
		if (w->win == hWnd) goto found;
	}

	return DefWindowProc(hWnd, message, wParam, lParam);

found:
	bind_context(context, w);
	
	switch (message) 
	{
		case WM_COMMAND:
			break;

		case WM_CREATE:
            // Initialize the shell activate info structure
            memset (&s_sai, 0, sizeof (s_sai));
            s_sai.cbSize = sizeof (s_sai);
			break;

		case WM_CLEAR:
			// eat it
			break;

		case WM_PAINT:
			if (w->draw) {
				PAINTSTRUCT ps;
				RECT rt;

				HDC hdc = BeginPaint(hWnd, &ps);

				if (w->reshape) {
					GetClientRect(hWnd, &rt);
					(w->reshape)((UGWindow)w, rt.right - rt.left, rt.bottom - rt.top);
				}

				(w->draw)((UGWindow) w);
				EndPaint(hWnd, &ps);
			}

			break; 

		case WM_SIZE:
			width = LOWORD(lParam);
			height = HIWORD(lParam);

			if (w->width != width || w->height != height) {
				w->width = width;
				w->height = height;

				if (w->surface) {
				    eglDestroySurface(w->ug->egldpy, w->surface);
					w->surface = eglCreateWindowSurface(w->ug->egldpy, w->eglconfig, (NativeWindowType)(w->win), 0);
				}

				if (w->reshape) {
					(w->reshape)((UGWindow)w, width, height);
				}
			}

			break;

		case WM_LBUTTONDOWN:
		case WM_MBUTTONDOWN:
		case WM_RBUTTONDOWN:
			if (w->pointer) {
				int but, state = UG_BUT_DOWN;

				if (message == WM_LBUTTONDOWN)
					but = UG_BUT_LEFT;
				else if (message == WM_MBUTTONDOWN)
					but = UG_BUT_MIDDLE;
				else if (message == WM_RBUTTONDOWN)
					but = UG_BUT_RIGHT;
				else
					break;

			(*w->pointer)((UGWindow)w, but, state, LOWORD(lParam), HIWORD(lParam));
			}
	    break;

		case WM_TIMER:
			if (context->idle) {
				(context->idle)((UGWindow)w);
			}

			break;

		case WM_DESTROY:
			PostQuitMessage(0);
			break;

		case WM_ACTIVATE:
            // Notify shell of our activate message
			SHHandleWMActivate(hWnd, wParam, lParam, &s_sai, FALSE);

			fActive = LOWORD(wParam);
			if (fActive != WA_INACTIVE)// it isn't inactive
			{
				if ( ((BOOL)HIWORD(wParam)) == FALSE) //it isn't minimized
					UpdateWindowPosition(w->win);
			}

     		break;

		case WM_SETTINGCHANGE:
			SHHandleWMSettingChange(hWnd, wParam, lParam, &s_sai);

			UpdateWindowPosition(w->win);
     		break;

		case WM_CHAR:

			if (pfnSaveSurface && (((TCHAR)wParam == 'c')||((TCHAR)wParam == 'C')))					
			{
																								//show save file dialog
				OPENFILENAME ofn ;		 // common dialog structure
				TCHAR	szFile[MAX_PATH] = TEXT("Screenshot.bmp\0");

				memset( &(ofn), 0, sizeof(ofn));
				ofn.lStructSize	= sizeof(ofn);

				ofn.hwndOwner = w->win;
				ofn.lpstrFile = szFile;
				ofn.nMaxFile = MAX_PATH;	

				ofn.lpstrFilter = TEXT ("Bitmap Files (*.bmp)\0*.bmp\0");
				ofn.Flags = OFN_OVERWRITEPROMPT; 
				ofn.lpstrDefExt = TEXT("bmp");

				if (GetSaveFileName(&ofn))				  //save image
				{
																						// need to re-paint the image		
					InvalidateRect(w->win, NULL, TRUE);		  // after closing the file save dialog,
					UpdateWindow(w->win);							// before attempting to copy the surface contents

					pfnSaveSurface( w->surface, ofn.lpstrFile);
				}
			}
			else
			{
				if (w->kbd) {
					GetCursorPos(&point);
					(w->kbd)((UGWindow)w, wParam, point.x, point.y);
				}
			}

			break;

		case WM_KEYDOWN:
			key = GetKey((int) wParam);

			if (key) {
				if (w->kbd) {
					GetCursorPos(&point);
					(w->kbd)((UGWindow)w, key, point.x, point.y);
				}
			} else {
				return DefWindowProc(hWnd, message, wParam, lParam);
			}

			break;

		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
   }

   return 0;
}

void APIENTRY
ugMainLoop(UGCtx ug) {
	MSG msg;

	context = (UGCtx_t *) ug;

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0)) 
	{
		//if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg)) 
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}
}

void APIENTRY
ugPostRedisplay(UGWindow uwin) {
    UGWindow_t* w = (UGWindow_t*)uwin;
    InvalidateRect(w->win, 0, TRUE);
}

void APIENTRY
ugSwapBuffers(UGWindow uwin) {
    UGWindow_t* w = (UGWindow_t*)uwin;
    eglSwapBuffers(GetDC(w->win), w->surface);
}

static ATOM UgRegisterClass(HINSTANCE hInstance, LPTSTR szWindowClass)
{
	WNDCLASS	wc;

    wc.style			= CS_HREDRAW | CS_VREDRAW;
    wc.lpfnWndProc		= (WNDPROC) WndProc;
    wc.cbClsExtra		= 0;
    wc.cbWndExtra		= 0;
    wc.hInstance		= hInstance;
    wc.hIcon			= 0;
    wc.hCursor			= 0;
    wc.hbrBackground	= 0;//(HBRUSH) GetStockObject(WHITE_BRUSH);
    wc.lpszMenuName		= 0;
    wc.lpszClassName	= szWindowClass;

	return RegisterClass(&wc);
}








#define STRIKENUM_MAX 3
typedef struct _FindAppT
{
	TCHAR *pzExeName;
	HWND hwnd;
} 
FindAppT, *pFindAppT;

BOOL CALLBACK FindApplicationWindowProc(HWND hwnd, LPARAM lParam)
{
	DWORD		dwProcessID;
	INT		    iLen;
	TCHAR	    szTempName[MAX_PATH]=TEXT("\0");
	pFindAppT	pFindApp=(pFindAppT)lParam;

	GetWindowThreadProcessId(hwnd,&dwProcessID);
	if (!dwProcessID) 
		return TRUE;

	iLen=GetModuleFileName((HMODULE)dwProcessID,szTempName,MAX_PATH);
	if (!iLen) 
		return TRUE;

	if (!_tcsicmp(szTempName, pFindApp->pzExeName) )
	{	

		// Check window title to make sure the main window is found.
		GetWindowText(hwnd, szTempName, MAX_PATH);
		if (!_tcsicmp(szTempName, pFindApp->pzExeName) ) 
		{	
			pFindApp->hwnd=hwnd;		
			return FALSE;
		}
	}

	return TRUE;
}





extern int main(int argc, const char * argv[]);

#define MAX_ARGS 64

int WINAPI WinMain(	HINSTANCE hInstance,
					HINSTANCE hPrevInstance,
					LPTSTR    lpCmdLine,
					int       nCmdShow)
{
	int argc = 1;
	const char * argv[MAX_ARGS];
	int size;
	char * str = 0;
	char * pch;
	char* cname = 0;
	int result;




	// WinCE: Only one application instance can be run
	HANDLE hMuTex;
	TCHAR szTempName[MAX_PATH], szExeName[MAX_PATH];

	INT	i, iValue;
	FindAppT findApp;
	BOOL bGoAway;
	
	iValue=GetModuleFileName(NULL, szExeName,MAX_PATH);			
	_tcscpy(szTempName,szExeName);
	for(i = 0; i < iValue; i++) 
	{
		if (szTempName[i]=='\\') 
			szTempName[i]='/';
	}

	hMuTex = CreateMutex(NULL, FALSE, szTempName);	 // create a mutex w/ the exe name
	if (hMuTex!=NULL)
	{
		if (GetLastError() == ERROR_ALREADY_EXISTS) 
		{
			bGoAway = FALSE;
			memset(&findApp,0,sizeof(FindAppT));

			findApp.pzExeName=(TCHAR *)szExeName;

			for (i=0; i< STRIKENUM_MAX; i++) 
			{
				EnumWindows(FindApplicationWindowProc,(LPARAM)&findApp);

				if (findApp.hwnd) 
				{			  // if a previous instance is found, set it to the foreground and quit.

					HWND hWnd = (HWND)findApp.hwnd;
					SetForegroundWindow(hWnd);

					bGoAway=TRUE;
					break;

				}
			}
			if (bGoAway) 
				return 0;
		}
	}


	instance = hInstance;
	UgRegisterClass(hInstance, WINDOW_CLASS_NAME);


	/* convert unicode string to ansi */
	size = WideCharToMultiByte( CP_ACP, 0, szExeName, -1,
        cname, 0, NULL, NULL );
	cname = malloc(size);
	WideCharToMultiByte( CP_ACP, 0, szExeName, -1,
        cname, size, NULL, NULL );

	argv[0] = cname;				 // set first argument to this exe's name;
											  // this results in the window title being the exe name,
											 //	  which is how 	previous instances are identified


	/* convert unicode string to ansi */
	size = WideCharToMultiByte( CP_ACP, 0, lpCmdLine, -1,
        str, 0, NULL, NULL );
	str = malloc(size);
	WideCharToMultiByte( CP_ACP, 0, lpCmdLine, -1,
        str, size, NULL, NULL );

	/* break ansi string into components */

	pch = strtok (str," ");

	while (pch != NULL) 
	{
		argv[argc++] = pch;
	    pch = strtok (NULL, " ");
	}

	result = main(argc, argv);

	return result;
}



