/*
Copyright (C) 1996-1997 Id Software, Inc.

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  

See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

*/

#include <string.h>
#include <math.h>
#include <time.h>
#include <dos.h>

#include "quakegeneric.h"

int old_mode;
union REGS regs;

unsigned char *VGA = 0xA0000L;

void QG_Init(void)
{
	// get old video mode
	regs.h.ah = 0x0f;
	int386(0x10, &regs, &regs);
	old_mode = regs.h.al;

	// set video mode to 320x200x8
	regs.w.ax = 0x13;
	int386(0x10, &regs, &regs);
}

int QG_GetKey(int *down, int *key)
{
	return 0;
}

void QG_GetMouseMove(int *x, int *y)
{

}

void QG_Quit(void)
{
	// set old video mode
	regs.w.ax = old_mode;
	int386(0x10, &regs, &regs);
}

void QG_DrawFrame(void *pixels)
{
	memcpy(VGA, pixels, QUAKEGENERIC_RES_X * QUAKEGENERIC_RES_Y);
}

void QG_SetPalette(unsigned char palette[768])
{
	int i;
	for (i = 0; i < 256; i++)
	{
		outp(0x3c8, i);
		outp(0x3c9, (palette[i * 3] * 63) / 255);
		outp(0x3c9, (palette[(i * 3) + 1] * 63) / 255);
		outp(0x3c9, (palette[(i * 3) + 2] * 63) / 255);
	}
}

int main(int argc, char *argv[])
{
	int running;
	double oldtime, newtime;

	QG_Create(argc, argv);

	running = 1;
	oldtime = (((double)clock()) / CLOCKS_PER_SEC) - 0.1;
	while (running)
	{
		newtime = ((double)clock()) / CLOCKS_PER_SEC;
		QG_Tick(newtime - oldtime);
		oldtime = newtime;
	}

	return 0;
}
