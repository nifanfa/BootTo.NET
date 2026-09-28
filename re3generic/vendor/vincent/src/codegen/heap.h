#ifndef CODEGEN_HEAP_H
#define CODEGEN_HEAP_H 1

/****************************************************************************/
/*																			*/
/* Copyright (c) 2004, Hans-Martin Will. All rights reserved.				*/
/*																			*/
/* Redistribution and use in source and binary forms, with or without		*/
/* modification, are permitted provided that the following conditions are   */
/* met:																		*/
/*																			*/
/* *  Redistributions of source code must retain the above copyright		*/
/*    notice, this list of conditions and the following disclaimer.			*/
/*																			*/
/* *  Redistributions in binary form must reproduce the above copyright		*/
/*    notice, this list of conditions and the following disclaimer in the   */
/*    documentation and/or other materials provided with the distribution.  */
/*																			*/
/* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS		*/
/* "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT		*/
/* LIMITED TO, THEIMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A   */
/* PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER */
/* OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, */
/* EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,		*/
/* PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR		*/
/* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF   */
/* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING		*/
/* NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS		*/
/* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.				*/
/*																			*/
/****************************************************************************/


#include "codegen.h"


#ifdef __cplusplus
extern "C" {
#endif


/****************************************************************************/
/* Heap data type															*/
/****************************************************************************/
#if defined(__GCC32__) && defined(__cplusplus)
struct cg_heap_t;
#else
typedef struct cg_heap_t cg_heap_t;
#endif

cg_heap_t * cg_heap_create(size_t default_block_size);
	/************************************************************************/
	/* Create a new heap object.											*/
	/************************************************************************/


void cg_heap_destroy(cg_heap_t * heap);
	/************************************************************************/
	/* Destroy a heap and deallocate any memory belonging to it				*/
	/************************************************************************/


void * cg_heap_allocate(cg_heap_t * heap, size_t amount);
	/************************************************************************/
	/* Allocate a block of memory of the given size on the heap.			*/
	/************************************************************************/


#define NEW(Heap,Type) ((Type *) CG_Allocate(Heap, sizeof(Type)))


#ifdef __cplusplus
}
#endif

#endif //ndef CODEGEN_HEAP_H
