#ifndef CODEGEN_SGEMENT_H
#define CODEGEN_SGEMENT_H 1

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

#if defined(__GCC32__) && defined(__cplusplus)
struct cg_segment_t;
#else
typedef struct cg_segment_t cg_segment_t;
#endif
	/************************************************************************/
	/* Code segment data structure											*/
	/************************************************************************/


cg_segment_t * cg_segment_create(const char * name);
	/************************************************************************/
	/* Create a named segment. The name is really for debugging only.		*/
	/************************************************************************/


void cg_segment_destroy(cg_segment_t * segment);
	/************************************************************************/
	/* Destroy the segment and all associated data							*/
	/************************************************************************/


const char * cg_segment_name(cg_segment_t * segment);
	/************************************************************************/
	/* Return the name of the segment										*/
	/************************************************************************/


size_t cg_segment_size(cg_segment_t * segment);
	/************************************************************************/
	/* Retrieve the current size of a segement								*/
	/************************************************************************/


size_t cg_segment_align(cg_segment_t * segment, size_t alignment);
	/************************************************************************/
	/* Align the memory segment to the given alignment boundary.			*/
	/* The adjusted size is returned.										*/
	/************************************************************************/


/****************************************************************************/
/* Emit (append) data to the target segment									*/
/****************************************************************************/

void cg_segment_emit_u8(cg_segment_t * segment, U8 byte);
void cg_segment_emit_u16(cg_segment_t * segment, U16 half_word);
void cg_segment_emit_u32(cg_segment_t * segment, U32 word);
void cg_segment_emit_i8(cg_segment_t * segment, I8 byte);
void cg_segment_emit_i16(cg_segment_t * segment, I16 half_word);
void cg_segment_emit_i32(cg_segment_t * segment, I32 word);
void cg_segment_emit_block(cg_segment_t * segment, const void * p, size_t size);


/****************************************************************************/
/* Set data in the target segment											*/
/****************************************************************************/

void cg_segment_set_u8(cg_segment_t * segment, size_t offset, U8 byte);
void cg_segment_set_u16(cg_segment_t * segment, size_t offset, U16 half_word);
void cg_segment_set_u32(cg_segment_t * segment, size_t offset, U32 word);
void cg_segment_set_i8(cg_segment_t * segment, size_t offset, I8 byte);
void cg_segment_set_i16(cg_segment_t * segment, size_t offset, I16 half_word);
void cg_segment_set_i32(cg_segment_t * segment, size_t offset, I32 word);
void cg_segment_set_block(cg_segment_t * segment, size_t offset, const void * p, 
						  size_t size);


/****************************************************************************/
/* Retrieve data from the target segment									*/
/****************************************************************************/

U8 cg_segment_get_u8(cg_segment_t * segment, size_t offset);
U16 cg_segment_get_u16(cg_segment_t * segment, size_t offset);
U32 cg_segment_get_u32(cg_segment_t * segment, size_t offset);
I8 cg_segment_get_i8(cg_segment_t * segment, size_t offset);
I16 cg_segment_get_i16(cg_segment_t * segment, size_t offset);
I32 cg_segment_get_i32(cg_segment_t * segment, size_t offset);
void cg_segment_get_block(cg_segment_t * segment, size_t offset, void * p, 
						  size_t size);


#ifdef __cplusplus
}
#endif

#endif //ndef CODEGEN_SGEMENT_H
