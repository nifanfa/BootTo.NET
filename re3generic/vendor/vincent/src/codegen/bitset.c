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


#include "bitset.h"


cg_bitset_t * cg_bitset_create(cg_heap_t * heap, size_t elements)
{
	size_t words = (elements + CG_BITSET_BITS_PER_WORD - 1) / CG_BITSET_BITS_PER_WORD;
	
	cg_bitset_t * result = (cg_bitset_t *)
		cg_heap_allocate(heap, sizeof(cg_bitset_t) + words * sizeof(U32));
	
	result->elements = elements;
	return result;
}


void cg_bitset_assign(cg_bitset_t * target, cg_bitset_t * source) 
{
	size_t index;
	
	assert(target);
	assert(source);
	assert(target->elements == source->elements);
	
	for (index = 0; index < target->elements / CG_BITSET_BITS_PER_WORD; ++index)
	{
		target->bits[index] = source->bits[index];
	}
}


/* target = target U (source \ minus) */
int cg_bitset_union_minus(cg_bitset_t * target, cg_bitset_t * source,
						   cg_bitset_t * minus)
{
	size_t index;
	int result = 0;
	
	assert(target);
	assert(source);
	assert(minus);
	assert(target->elements == source->elements);
	assert(target->elements == minus->elements);
	
	for (index = 0; index < target->elements / CG_BITSET_BITS_PER_WORD; ++index)
	{
		U32 old = target->bits[index];
		target->bits[index] |= source->bits[index] & ~minus->bits[index];

		result |= (target->bits[index] != old);
	}

	return result;
}


int cg_bitset_union(cg_bitset_t * target, cg_bitset_t * source)
{
	size_t index;
	int result = 0;
	
	assert(target);
	assert(source);
	assert(target->elements == source->elements);
	
	for (index = 0; index < target->elements / CG_BITSET_BITS_PER_WORD; ++index)
	{
		U32 old = target->bits[index];
		target->bits[index] |= source->bits[index];

		result |= (target->bits[index] != old);
	}

	return result;
}


int cg_bitset_intersects(const cg_bitset_t * first, const cg_bitset_t * second) 
{
	size_t index;

	assert(first);
	assert(second);
	assert(first->elements == second->elements);
	
	for (index = 0; index < first->elements / CG_BITSET_BITS_PER_WORD; ++index)
	{
		if (first->bits[index] & second->bits[index])
			return 1;
	}

	return 0;
}
