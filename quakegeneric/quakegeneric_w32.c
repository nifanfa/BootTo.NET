#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <mmsystem.h>
#include <stdlib.h>
#include <string.h>
#include "quakegeneric.h"

#define KEYBUFFERSIZE 32
int keybuffer[KEYBUFFERSIZE];
int keybuffer_len;
int keybuffer_start;

HBITMAP backBitmap;
HDC hdc_bmp;
HDC globalHdc;
BITMAPINFO* bmi;
unsigned char* frontbuffer;
HWND hwnd;
int WINDOW_WIDTH, WINDOW_HEIGHT;
char* quake_title = "Quake for Windows";
int mouse_x, mouse_y;
int old_mouse_x, old_mouse_y;

#define OLD_WINDOWS

/*
Video backends
- CreateDIBSection: Windows NT 3.50 / Windows 95 and up (currently implemented)
- StretchDIBits: Windows 3.00 and up (tbi)
- WinG: Windows 3.1 w/ Win32s and up (tbi)
- DISPDIB: Windows 3.1 w/ Win32s and up (tbi)

To be added
- Proper support for 256-color displays (CreatePalette/RealizePalette/whatever should do this once I figure out how to do it)
- Mouse support
- Sound (waveOut): waiting on QuakeGeneric
- Networking (WinSock): waiting on QuakeGeneric

The one rule of development is to not use any APIs newer than Windows NT 3.50!!! Everything that Quake does should be possible with
- WINMM.DLL (for sound & timing)
- GDI32.DLL (for drawing to the screen)
- WSOCK32.DLL (for networking)
plus obligatory DLLs like MSVCRT, KERNEL32, USER32, etc.

COM is the devil. DirectX must be avoided at all costs.

Developer hall of shame
- Captain Will Starblazer (BHTY) - 7.7.23 (last updated 7.7.23)
*/

int ConvertToQuakeKey(unsigned int key){
	int qkey;

	switch (key){
		case VK_ESCAPE:
			qkey = K_ESCAPE;
			break;
		case VK_RETURN:
			qkey = K_ENTER;
			break;
		case VK_SPACE:
			qkey = K_SPACE;
			break;
		case VK_UP:
			qkey = K_UPARROW;
			break;
		case VK_DOWN:
			qkey = K_DOWNARROW;
			break;
		case VK_LEFT:
			qkey = K_LEFTARROW;
			break;
		case VK_RIGHT:
			qkey = K_RIGHTARROW;
			break;
		case VK_CONTROL:
			qkey = K_CTRL;
			break;
		case VK_SHIFT:
			qkey = K_SHIFT;
			break;
		default:
			qkey = tolower(key);
			break;
	}

	return qkey;
}

int KeyPop(int *down, int *key)
{
	if (keybuffer_len == 0)
		return 0; // underflow

	*key = keybuffer[keybuffer_start];
	*down = *key < 0;
	if (*key < 0)
		*key = -*key;
	keybuffer_start = (keybuffer_start + 1) % KEYBUFFERSIZE;
	keybuffer_len--;
}

int KeyPush(int down, int key)
{
	if (keybuffer_len == KEYBUFFERSIZE)
		return 0; // overflow
	if (down) {
		key = -key;
	}
	keybuffer[(keybuffer_start + keybuffer_len) % KEYBUFFERSIZE] = key;
	keybuffer_len++;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam){
	HGDIOBJ old_bmp;
	int posx, posy;

	switch (msg){
	case WM_DESTROY:{
						PostQuitMessage(0);
						break;
	}

	case WM_CLOSE:{
					  DestroyWindow(hWnd);
					  ExitProcess(0);
					  break;
	}

	case WM_QUIT:{
					 ExitProcess(0);
					 break;
	}

		case WM_SIZE:{
			WINDOW_WIDTH = lParam & 0xffff;
			WINDOW_HEIGHT = (lParam & 0xffff0000) >> 16;
			break;
		}

		case WM_KEYDOWN:{
			KeyPush(1, ConvertToQuakeKey((unsigned char)wParam));
			break;
		}

		case WM_KEYUP:{
			KeyPush(0, ConvertToQuakeKey((unsigned char)wParam));
			break;
		}

		case WM_LBUTTONDOWN:{
			KeyPush(1, K_MOUSE1);
			break;
		}

		case WM_LBUTTONUP:{
			KeyPush(0, K_MOUSE1);
			break;
		}

		case WM_RBUTTONDOWN:{
								KeyPush(1, K_MOUSE2);
								break;
		}

		case WM_RBUTTONUP:{
							  KeyPush(0, K_MOUSE2);
							  break;
		}

		case WM_MBUTTONDOWN:{
								KeyPush(1, K_MOUSE3);
								break;
		}

		case WM_MBUTTONUP:{
							  KeyPush(0, K_MOUSE3);
							  break;
		}

#ifndef OLD_WINDOWS
		case WM_MOUSEWHEEL:{
			if (GET_WHEEL_DELTA_WPARAM(wParam) > 0){
				KeyPush(1, K_MWHEELUP);
				KeyPush(0, K_MWHEELUP);
			}
			else{
				KeyPush(1, K_MWHEELDOWN);
				KeyPush(0, K_MWHEELDOWN);
			}
			break;
		}
#endif

		case WM_PAINT:{
						  old_bmp = SelectObject(hdc_bmp, backBitmap);
						  StretchBlt(globalHdc, 0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, hdc_bmp, 0, 0, 320, 200, SRCCOPY);
						  DeleteObject(old_bmp);
		}

		default:{
			return DefWindowProc(hWnd, msg, wParam, lParam);
		}
	}
}

void QG_Init(){
	HDC hdcScreen;
	WNDCLASS wc;
	RECT winRect;

	winRect.left = 0;
	winRect.top = 0;
	winRect.bottom = 200;
	winRect.right = 320;

	WINDOW_WIDTH = 320;
	WINDOW_HEIGHT = 200;

	AdjustWindowRect(&winRect, WS_OVERLAPPEDWINDOW, FALSE);

	bmi = malloc(sizeof(BITMAPINFO)+256 * sizeof(RGBQUAD));

	bmi->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bmi->bmiHeader.biWidth = 320;
	bmi->bmiHeader.biHeight = -200;
	bmi->bmiHeader.biPlanes = 1;
	bmi->bmiHeader.biBitCount = 8;
	bmi->bmiHeader.biClrUsed = 256;
	bmi->bmiHeader.biCompression = BI_RGB;

	hdcScreen = GetDC(NULL);
	backBitmap = CreateDIBSection(hdcScreen, bmi, DIB_RGB_COLORS, (void**)(&frontbuffer), NULL, NULL);
	ReleaseDC(NULL, hdcScreen);

	wc.style = 0;
	wc.lpfnWndProc = WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = GetModuleHandle(NULL);
	wc.hIcon = LoadIcon(GetModuleHandle(NULL), MAKEINTRESOURCE(101));
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wc.lpszMenuName = NULL;
	wc.lpszClassName = quake_title;
	RegisterClass(&wc);

	hwnd = CreateWindow(quake_title, quake_title, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, winRect.right - winRect.left, winRect.bottom - winRect.top, NULL, NULL, GetModuleHandle(NULL), NULL);
	ShowWindow(hwnd, SW_SHOW);
	UpdateWindow(hwnd);

	globalHdc = GetDC(hwnd);
	hdc_bmp = CreateCompatibleDC(globalHdc);
}

int QG_GetKey(int* down, int *key){
	return KeyPop(down, key);
}

void QG_GetMouseMove(int *x, int *y){
	*x = mouse_x;
	*y = mouse_y;

	mouse_x = mouse_y = 0;
}

void QG_Quit(){

}

void QG_SetPalette(unsigned char palette[768]){ //do the whole SetPalette/SelectPalette/RealizePalette thing for 8bpp displays
	int i;
	RGBQUAD* willPalette = &(bmi->bmiColors[0]);
	HDC tempHDC = CreateCompatibleDC(NULL);

	for (i = 0; i < 256; i++){
		willPalette[i].rgbRed = *(palette);
		willPalette[i].rgbGreen = *(palette+1);
		willPalette[i].rgbBlue = *(palette+2);

		palette += 3;
	}

	SelectObject(tempHDC, backBitmap);
	SetDIBColorTable(tempHDC, 0, 256, willPalette);
	DeleteDC(tempHDC);
}

void QG_DrawFrame(void* pixels, void* palette){
	memcpy(frontbuffer, pixels, 64000);
	InvalidateRect(hwnd, NULL, 0);
}

int main(int argc, char** argv){
	MSG Msg;
	double oldtime, newtime;
	int running = 1;

	timeBeginPeriod(1);

	QG_Create(argc, argv);
	oldtime = (double)timeGetTime() / 1000 - 0.1;

	while (running){
		while (PeekMessage(&Msg, hwnd, 0, 0, PM_REMOVE)) {
			TranslateMessage(&Msg);
			DispatchMessage(&Msg);
		}

		newtime = (double)timeGetTime() / 1000;
		QG_Tick(newtime - oldtime);
		oldtime = newtime;
	}

	return 0;
}
