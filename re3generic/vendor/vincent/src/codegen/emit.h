#ifndef CODEGEN_EMIT_H
#define CODEGEN_EMIT_H 1

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
#include "instruction.h"
#include "heap.h"
#include "segment.h"


#ifdef __cplusplus
extern "C" {
#endif


/****************************************************************************/
/* Symbolic labels															*/
/****************************************************************************/

#if defined(__GCC32__) && defined(__cplusplus)
struct cg_label_t;
struct cg_codegen_t;
#else
typedef struct cg_label_t cg_label_t;
typedef struct cg_codegen_t cg_codegen_t;
#endif

typedef enum 
{
	cg_reference_offset12,					/* LDR relative from code		*/
	cg_reference_branch24,					/* branch relative from code	*/
	cg_reference_absolute32,				/* absolute 32-bit address		*/
} 
cg_reference_type_t;


/****************************************************************************/
/* Runtime function pointers												*/
/* Accidentially, these methods signatures co-incide with the mathematical  */
/* functions defined in the Intel GPP...									*/
/****************************************************************************/


typedef struct cg_runtime_info_t
{
	div_t (*div)				(I32 numer, I32 denom);	/* Std. C runtime	*/

	I32 (*div_HP_16_32s)		(I32 num, I32 denom);
	I32 (*inv_HP_16_32s)		(I32 src);
	I32 (*sqrt_HP_16_32s)		(I32 src);
	I32 (*inv_sqrt_HP_16_32s)	(I32 src);

	I32 (*div_LP_16_32s)		(I32 num, I32 denom);
	I32 (*inv_LP_16_32s)		(I32 src);
	I32 (*sqrt_LP_16_32s)		(I32 src);
	I32 (*inv_sqrt_LP_16_32s)	(I32 src);

	/*
	void (*sin_LP_16_32s)		(I32 theta, I32* sin_theta);
	void (*cos_LP_16_32s)		(I32 theta, I32* cos_theta);
	void (*sin_cos_LP_16_32s)	(I32 theta, I32* sin_theta, I32* cos_theta);
	void (*sin_HP_16_32s)		(I32 theta, I32* sin_theta);
	void (*cos_HP_16_32s)		(I32 theta, I32* cos_theta);
	void (*sin_cos_HP_16_32s)	(I32 theta, I32* sin_theta, I32* cos_theta);
	*/
}
cg_runtime_info_t;


typedef struct cg_processor_info_t 
{
	int		useV5;				/* can use instructions from V5 arch.		*/
}
cg_processor_info_t;

/****************************************************************************/
/* Code generator															*/
/****************************************************************************/

cg_codegen_t * cg_codegen_create(cg_heap_t * heap, cg_runtime_info_t * runtime,
								 cg_processor_info_t * processor);

void cg_codegen_destroy(cg_codegen_t * gen);

void cg_codegen_emit_module(cg_codegen_t * gen, cg_module_t * module);
void cg_codegen_emit_proc(cg_codegen_t * gen, cg_proc_t * proc);
void cg_codegen_emit_block(cg_codegen_t * gen, cg_block_t * block, int reinit);
void cg_codegen_emit_inst(cg_codegen_t * gen, cg_inst_t * inst);

size_t cg_codegen_emit_literal(cg_codegen_t * gen, U32 literal, int distinct);

void cg_codegen_fix_refs(cg_codegen_t * gen);

cg_label_t * cg_codegen_create_label(cg_codegen_t * gen);

void cg_codegen_define(cg_codegen_t * gen, cg_label_t * label);
void cg_codegen_reference(cg_codegen_t * gen, cg_label_t * label, 
						  cg_reference_type_t ref_type);

cg_segment_t * cg_codegen_segment(cg_codegen_t * gen);

#ifdef __cplusplus
}
#endif

#endif //ndef CODEGEN_EMIT_H
