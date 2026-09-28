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
#include "segment.h"


#define BLOCK_SIZE		4096
#define MAX_BLOCKS		512


typedef struct block_t 
{
	U8 memory[BLOCK_SIZE];
} 
block_t;


struct cg_segment_t 
	/************************************************************************/
	/* Code segment data structure											*/
	/************************************************************************/
{
	const char *	name;					/* name of the segment			*/
	block_t *		blocks[MAX_BLOCKS];		/* array of block pointers		*/
	size_t			current_size;			/* current size of the segment	*/
};


cg_segment_t * cg_segment_create(const char * name)
	/************************************************************************/
	/* Create a named segment. The name is really for debugging only.		*/
	/************************************************************************/
{
	cg_segment_t * result = (cg_segment_t *) malloc(sizeof(cg_segment_t));

	memset(result->blocks, 0, sizeof(result->blocks));
	result->current_size = 0;
	result->name = name;

	return result;
}


void cg_segment_destroy(cg_segment_t * segment)
	/************************************************************************/
	/* Destroy the segment and all associated data							*/
	/************************************************************************/
{
	size_t index;

	for (index = 0; index < MAX_BLOCKS; ++index) 
	{
		if (segment->blocks[index]) 
			free(segment->blocks[index]);
	}

	free(segment);
}


size_t cg_segment_size(cg_segment_t * segment) 
	/************************************************************************/
	/* Retrieve the current size of a segement								*/
	/************************************************************************/
{
	return segment->current_size;
}


const char * cg_segment_name(cg_segment_t * segment)
	/************************************************************************/
	/* Return the name of the segment										*/
	/************************************************************************/
{
	return segment->name;
}


size_t cg_segment_align(cg_segment_t * segment, size_t alignment)
	/************************************************************************/
	/* Align the memory segment to the given alignment boundary.			*/
	/* The adjusted size is returned.										*/
	/************************************************************************/
{
	segment->current_size = 
		(segment->current_size + (alignment - 1)) & ~(alignment - 1);
	return segment->current_size;
}


static block_t * block_pointer(cg_segment_t * segment, size_t block_index) 
{
	block_t * block = segment->blocks[block_index];

	if (block == (block_t *) 0) 
	{
		block = (block_t *) malloc(sizeof(block_t));
		memset(block, 0, sizeof(block_t));
		segment->blocks[block_index] = block;
	}

	return block;
}

static void * data_pointer(cg_segment_t * segment, size_t offset) 
{
	size_t block_index = offset / BLOCK_SIZE;
	size_t block_offset = offset % BLOCK_SIZE;

	block_t * block = block_pointer(segment, block_index);

	return block->memory + block_offset;
}

/****************************************************************************/
/* Emit (append) data to the target segment									*/
/****************************************************************************/

void cg_segment_emit_u8(cg_segment_t * segment, U8 byte) 
{
	*((U8 *) data_pointer(segment, segment->current_size)) = byte;
	segment->current_size += sizeof(U8);
}


void cg_segment_emit_u16(cg_segment_t * segment, U16 half_word) 
{
	cg_segment_align(segment, sizeof(U16));
	*((U16 *) data_pointer(segment, segment->current_size)) = half_word;
	segment->current_size += sizeof(U16);
}


void cg_segment_emit_u32(cg_segment_t * segment, U32 word)
{
	cg_segment_align(segment, sizeof(U32));
	*((U32 *) data_pointer(segment, segment->current_size)) = word;
	segment->current_size += sizeof(U32);
}


void cg_segment_emit_i8(cg_segment_t * segment, I8 byte) 
{
	*((I8 *) data_pointer(segment, segment->current_size)) = byte;
	segment->current_size += sizeof(I8);
}


void cg_segment_emit_i16(cg_segment_t * segment, I16 half_word) 
{
	cg_segment_align(segment, sizeof(I16));
	*((I16 *) data_pointer(segment, segment->current_size)) = half_word;
	segment->current_size += sizeof(I16);
}


void cg_segment_emit_i32(cg_segment_t * segment, I32 word)
{
	cg_segment_align(segment, sizeof(I32));
	*((I32 *) data_pointer(segment, segment->current_size)) = word;
	segment->current_size += sizeof(I32);
}


void cg_segment_emit_block(cg_segment_t * segment, const void * p, size_t size)
{
	while (size != 0) 
	{
		size_t block_index = segment->current_size / BLOCK_SIZE;
		size_t block_offset = segment->current_size % BLOCK_SIZE;
		
		size_t remaining_size = BLOCK_SIZE - block_offset;
		size_t chunk_size = (size <= remaining_size) ? size : remaining_size;

		block_t * block = block_pointer(segment, block_index);

		memcpy(block->memory, p, chunk_size);

		segment->current_size += chunk_size;
		p = (U8 *) p + chunk_size;
		size -= chunk_size;
	}
}


/****************************************************************************/
/* Set data in the target segment											*/
/****************************************************************************/

void cg_segment_set_u8(cg_segment_t * segment, size_t offset, U8 byte)
{
	*((U8 *) data_pointer(segment, offset)) = byte;
}


void cg_segment_set_u16(cg_segment_t * segment, size_t offset, U16 half_word)
{
	*((U16 *) data_pointer(segment, offset)) = half_word;
}


void cg_segment_set_u32(cg_segment_t * segment, size_t offset, U32 word)
{
	*((U32 *) data_pointer(segment, offset)) = word;
}


void cg_segment_set_i8(cg_segment_t * segment, size_t offset, I8 byte)
{
	*((I8 *) data_pointer(segment, offset)) = byte;
}


void cg_segment_set_i16(cg_segment_t * segment, size_t offset, I16 half_word)
{
	*((I16 *) data_pointer(segment, offset)) = half_word;
}


void cg_segment_set_i32(cg_segment_t * segment, size_t offset, I32 word)
{
	*((I32 *) data_pointer(segment, offset)) = word;
}


void cg_segment_set_block(cg_segment_t * segment, size_t offset, const void * p, 
						  size_t size)
{
	while (size != 0) 
	{
		size_t block_index = offset / BLOCK_SIZE;
		size_t block_offset = offset % BLOCK_SIZE;
		
		size_t remaining_size = BLOCK_SIZE - block_offset;
		size_t chunk_size = (size <= remaining_size) ? size : remaining_size;

		block_t * block = block_pointer(segment, block_index);

		memcpy(block->memory, p, chunk_size);

		offset += chunk_size;
		p = (U8 *) p + chunk_size;
		size -= chunk_size;
	}
}


/****************************************************************************/
/* Retrieve data from the target segment									*/
/****************************************************************************/

U8 cg_segment_get_u8(cg_segment_t * segment, size_t offset)
{
	return *((U8 *) data_pointer(segment, offset));
}


U16 cg_segment_get_u16(cg_segment_t * segment, size_t offset)
{
	return *((U16 *) data_pointer(segment, offset));
}


U32 cg_segment_get_u32(cg_segment_t * segment, size_t offset)
{
	return *((U32 *) data_pointer(segment, offset));
}


I8 cg_segment_get_i8(cg_segment_t * segment, size_t offset)
{
	return *((I8 *) data_pointer(segment, offset));
}


I16 cg_segment_get_i16(cg_segment_t * segment, size_t offset)
{
	return *((I16 *) data_pointer(segment, offset));
}


I32 cg_segment_get_i32(cg_segment_t * segment, size_t offset)
{
	return *((I32 *) data_pointer(segment, offset));
}


void cg_segment_get_block(cg_segment_t * segment, size_t offset, void * p, 
						  size_t size)
{
	while (size != 0) 
	{
		size_t block_index = offset / BLOCK_SIZE;
		size_t block_offset = offset % BLOCK_SIZE;
		
		size_t remaining_size = BLOCK_SIZE - block_offset;
		size_t chunk_size = (size <= remaining_size) ? size : remaining_size;

		block_t * block = block_pointer(segment, block_index);

		memcpy(p, block->memory, chunk_size);

		offset += chunk_size;
		p = (U8 *) p + chunk_size;
		size -= chunk_size;
	}
}

