#ifndef CODEGEN_BITSET_H
#define CODEGEN_BITSET_H 1

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
#include "heap.h"


#ifdef __cplusplus
extern "C" {
#endif


#define CG_BITSET_BITS_PER_WORD 32


typedef struct cg_bitset_t
{
	size_t		elements;
	U32			bits[1];
}
cg_bitset_t;


cg_bitset_t * cg_bitset_create(cg_heap_t * heap, size_t elements);


void cg_bitset_assign(cg_bitset_t * target, cg_bitset_t * source);


/* target = target U (source \ minus) */
int cg_bitset_union_minus(cg_bitset_t * target, cg_bitset_t * source,
						   cg_bitset_t * minus);

int cg_bitset_union(cg_bitset_t * target, cg_bitset_t * source);

int cg_bitset_intersects(const cg_bitset_t * first, const cg_bitset_t * second);

#define CG_BITSET_TEST(bitset, element) \
	(((bitset->bits)[(element) / CG_BITSET_BITS_PER_WORD] & \
	  (1 << ((element) % CG_BITSET_BITS_PER_WORD))) != 0)
		
#define CG_BITSET_SET(bitset, element) \
((bitset->bits)[(element) / CG_BITSET_BITS_PER_WORD] |= (1 << ((element) % CG_BITSET_BITS_PER_WORD)))

#define CG_BITSET_CLEAR(bitset, element) \
((bitset->bits)[(element) / CG_BITSET_BITS_PER_WORD] &= ~(1 << ((element) % CG_BITSET_BITS_PER_WORD)))


#ifdef __cplusplus
}
#endif

#endif //ndef CODEGEN_BITSET_H
