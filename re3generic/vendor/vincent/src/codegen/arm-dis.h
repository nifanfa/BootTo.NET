/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 *
 * Copyright (c) 2002 Wild West Software
 * Copyright (c) 2001, 2002 Sergey Chaban
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

#ifndef ARM_DIS
#define ARM_DIS

#include <stdlib.h>
#include <stdio.h>

#include "segment.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct _ARMDis {
	FILE* dis_out;
	cg_segment_t * segment;
	size_t offset;
} ARMDis;


void armdis_init(ARMDis* dis);
void armdis_set_output(ARMDis* dis, FILE* f);
FILE* armdis_get_output(ARMDis* dis);
void armdis_decode(ARMDis* dis, cg_segment_t * segment);
void armdis_open(ARMDis* dis, const char* dump_name);
void armdis_close(ARMDis* dis);
void armdis_dump(ARMDis* dis, const char* dump_name, cg_segment_t * segment);

#ifdef __cplusplus
}
#endif

#endif /* ARM_DIS */
