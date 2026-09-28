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


#include "heap.h"


#define HEAP_ALIGNMENT	8


typedef struct block_t 
{
	struct block_t * next;
	char * base;
	size_t total;
	size_t current;
} block_t;


struct cg_heap_t 
{
	block_t *	blocks;
	size_t		default_block_size;
};


static block_t * create_block(size_t block_size) 
{
	block_t * result = (block_t *) malloc(sizeof(block_t));

	result->base = (char *) malloc(block_size);
	result->total = block_size;
	result->current = 0;
	result->next = (block_t *) 0;

	return result;
}


cg_heap_t * cg_heap_create(size_t default_block_size)
	/************************************************************************/
	/* Create a new heap object.											*/
	/************************************************************************/
{
	cg_heap_t * heap = (cg_heap_t *) malloc(sizeof(cg_heap_t));

	heap->blocks = create_block(default_block_size);
	heap->default_block_size = default_block_size;

	return heap;
}


void cg_heap_destroy(cg_heap_t * heap)
	/************************************************************************/
	/* Destroy a heap and deallocate any memory belonging to it				*/
	/************************************************************************/
{
	block_t * current, *next;

	for (current = heap->blocks; current != (block_t *) 0; current = next) 
	{
		next = current->next;
		free(current->base);
		free(current);
	}

	free(heap);
}




void * cg_heap_allocate(cg_heap_t * heap, size_t amount)
	/************************************************************************/
	/* Allocate a block of memory of the given size on the heap.			*/
	/************************************************************************/
{
	void * result;

	amount = (amount + HEAP_ALIGNMENT - 1) & ~(HEAP_ALIGNMENT - 1);

	if (heap->blocks->total - heap->blocks->current < amount) 
	{
		block_t * new_block;

		if (amount > heap->default_block_size) 
		{ 
			new_block = create_block(amount);
		} 
		else 
		{
			new_block = create_block(heap->default_block_size);
		}

		new_block->next = heap->blocks;
		heap->blocks = new_block;
	}

	result = heap->blocks->base + heap->blocks->current;
	heap->blocks->current += amount;

	memset(result, 0, amount);
	
	return result;
}

