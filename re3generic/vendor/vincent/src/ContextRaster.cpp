// ==========================================================================
//
// ContextRaster.cpp	Rendering Context Class for 3D Rendering Library
//
//						RasterizerState related settings
//
// --------------------------------------------------------------------------
//
// 08-02-2003	Hans-Martin Will	initial version
//
// --------------------------------------------------------------------------
//
// Copyright (c) 2004, Hans-Martin Will. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are 
// met:
// 
//	 *  Redistributions of source code must retain the above copyright
// 		notice, this list of conditions and the following disclaimer. 
//   *	Redistributions in binary form must reproduce the above copyright
// 		notice, this list of conditions and the following disclaimer in the 
// 		documentation and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
//
// ==========================================================================


#include "stdafx.h"
#include "Context.h"
#include "RasterizerState.h"

using namespace EGL;


// ==========================================================================
// Rasterizer State Functions on Context
// ==========================================================================

namespace {
	RasterizerState::ComparisonFunc ComparisonFuncFromEnum(GLenum func) {
		switch (func) {
			case GL_NEVER:		return RasterizerState::CompFuncNever;
			case GL_LESS:		return RasterizerState::CompFuncLess;
			case GL_LEQUAL:		return RasterizerState::CompFuncLEqual;
			case GL_GREATER:	return RasterizerState::CompFuncGreater;
			case GL_GEQUAL:		return RasterizerState::CompFuncGEqual;
			case GL_EQUAL:		return RasterizerState::CompFuncEqual;
			case GL_NOTEQUAL:	return RasterizerState::CompFuncNotEqual;
			case GL_ALWAYS:		return RasterizerState::CompFuncAlways;
			default:			return RasterizerState::CompFuncInvalid;
		}
	}

	RasterizerState::StencilOp StencilOpFromEnum(GLenum op) {
		switch (op) {
		default:						return RasterizerState::StencilOpInvalid;
		case GL_KEEP:					return RasterizerState::StencilOpKeep;
		case GL_ZERO:					return RasterizerState::StencilOpZero;
		case GL_REPLACE:				return RasterizerState::StencilOpReplace;
		case GL_INCR:					return RasterizerState::StencilOpIncr;
		case GL_DECR:					return RasterizerState::StencilOpDecr;
		case GL_INVERT:					return RasterizerState::StencilOpInvert;
		}
	}

	RasterizerState::LogicOp LogicOpFromEnum(GLenum op) {
		switch (op) {
		default:						return RasterizerState::LogicOpInvalid;
		case GL_CLEAR:					return RasterizerState::LogicOpClear;
		case GL_AND:					return RasterizerState::LogicOpAnd;
		case GL_AND_REVERSE:			return RasterizerState::LogicOpAndReverse;
		case GL_COPY:					return RasterizerState::LogicOpCopy;
		case GL_AND_INVERTED:			return RasterizerState::LogicOpAndInverted;
		case GL_NOOP:					return RasterizerState::LogicOpNoop;
		case GL_XOR:					return RasterizerState::LogicOpXor;
		case GL_OR:						return RasterizerState::LogicOpOr;
		case GL_NOR:					return RasterizerState::LogicOpNor;
		case GL_EQUIV:					return RasterizerState::LogicOpEquiv;
		case GL_INVERT:					return RasterizerState::LogicOpInvert;
		case GL_OR_REVERSE:				return RasterizerState::LogicOpOrReverse;
		case GL_COPY_INVERTED:			return RasterizerState::LogicOpCopyInverted;
		case GL_OR_INVERTED:			return RasterizerState::LogicOpOrInverted;
		case GL_NAND:					return RasterizerState::LogicOpNand;
		case GL_SET:					return RasterizerState::LogicOpSet;
		}
	}
}

void Context :: AlphaFuncx(GLenum func, GLclampx ref) { 
	RasterizerState::ComparisonFunc compFunc = ComparisonFuncFromEnum(func);

	if (compFunc != RasterizerState::CompFuncInvalid) {
		GetRasterizerState()->SetAlphaFunc(compFunc, ref);
	} else {
		RecordError(GL_INVALID_ENUM);
	}
}


void Context :: StencilFunc(GLenum func, GLint ref, GLuint mask) {
	RasterizerState::ComparisonFunc compFunc = ComparisonFuncFromEnum(func);

	if (compFunc != RasterizerState::CompFuncInvalid) {
		GetRasterizerState()->SetStencilFunc(compFunc, ref, mask);
	} else {
		RecordError(GL_INVALID_ENUM);
	}
}


void Context :: DepthFunc(GLenum func) { 
	RasterizerState::ComparisonFunc compFunc = ComparisonFuncFromEnum(func);

	if (compFunc != RasterizerState::CompFuncInvalid) {
		GetRasterizerState()->SetDepthFunc(compFunc);
	} else {
		RecordError(GL_INVALID_ENUM);
	}
}


void Context :: StencilMask(GLuint mask) { 
	GetRasterizerState()->SetStencilMask(mask);
}


void Context :: StencilOp(GLenum fail, GLenum zfail, GLenum zpass) {
	
	RasterizerState::StencilOp opFail = StencilOpFromEnum(fail);
	RasterizerState::StencilOp opZFail = StencilOpFromEnum(zfail);
	RasterizerState::StencilOp opZPass = StencilOpFromEnum(zpass);

	if (opFail != RasterizerState::StencilOpInvalid &&
		opZFail != RasterizerState::StencilOpInvalid &&
		opZPass != RasterizerState::StencilOpInvalid) {
		GetRasterizerState()->SetStencilOp(opFail, opZFail, opZPass);
	} else {
		RecordError(GL_INVALID_ENUM);
	}
}


void Context :: DepthMask(GLboolean flag) { 
	GetRasterizerState()->SetDepthMask(flag != 0);
}


void Context :: BlendFunc(GLenum sfactor, GLenum dfactor) { 

	RasterizerState::BlendFuncSrc source;
	RasterizerState::BlendFuncDst dest;

	switch (sfactor) {
		case GL_ZERO:					source = RasterizerState::BlendFuncSrcZero; break;
		case GL_ONE:					source = RasterizerState::BlendFuncSrcOne; break;
		case GL_DST_COLOR:				source = RasterizerState::BlendFuncSrcDstColor; break;
		case GL_ONE_MINUS_DST_COLOR:	source = RasterizerState::BlendFuncSrcOneMinusDstColor; break;
		case GL_SRC_ALPHA:				source = RasterizerState::BlendFuncSrcSrcAlpha; break;
		case GL_ONE_MINUS_SRC_ALPHA:	source = RasterizerState::BlendFuncSrcOneMinusSrcAlpha; break;
		case GL_DST_ALPHA:				source = RasterizerState::BlendFuncSrcDstAlpha; break;
		case GL_ONE_MINUS_DST_ALPHA:	source = RasterizerState::BlendFuncSrcOneMinusDstAlpha; break;
		case GL_SRC_ALPHA_SATURATE:		source = RasterizerState::BlendFuncSrcSrcAlphaSaturate; break;

		default:						
			RecordError(GL_INVALID_ENUM);
			return;
	}

	switch (dfactor) {
		case GL_ZERO:					dest = RasterizerState::BlendFuncDstZero; break;
		case GL_SRC_COLOR:				dest = RasterizerState::BlendFuncDstSrcColor; break;
		case GL_ONE_MINUS_SRC_COLOR:	dest = RasterizerState::BlendFuncDstOneMinusSrcColor; break;
		case GL_SRC_ALPHA:				dest = RasterizerState::BlendFuncDstSrcAlpha; break;
		case GL_ONE_MINUS_SRC_ALPHA:	dest = RasterizerState::BlendFuncDstOneMinusSrcAlpha; break;
		case GL_DST_ALPHA:				dest = RasterizerState::BlendFuncDstDstAlpha; break;
		case GL_ONE_MINUS_DST_ALPHA:	dest = RasterizerState::BlendFuncDstOneMinusDstAlpha; break;
		case GL_ONE:					dest = RasterizerState::BlendFuncDstOne; break;

		default:						
			RecordError(GL_INVALID_ENUM);
			return;
	}

	GetRasterizerState()->SetBlendFunc(source, dest);
}


void Context :: ColorMask(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha) { 
	GetRasterizerState()->SetColorMask(red != 0, green != 0, blue != 0, alpha != 0);
}


void Context :: LogicOp(GLenum opcode) { 
	RasterizerState::LogicOp op = LogicOpFromEnum(opcode);

	if (op != RasterizerState::LogicOpInvalid) {
		GetRasterizerState()->SetLogicOp(op);
	} else {
		RecordError(GL_INVALID_ENUM);
	}
}


void Context :: PolygonOffsetx(GLfixed factor, GLfixed units) { 
	GetRasterizerState()->SetPolygonOffset(factor, units);
}


void Context :: SampleCoveragex(GLclampx value, GLboolean invert) { 
	GetRasterizerState()->SetSampleCoverage(value, invert != 0);
}


void Context :: ShadeModel(GLenum mode) { 

	switch (mode) {
	case GL_FLAT:		GetRasterizerState()->SetShadeModel(RasterizerState::ShadeModelFlat);	return;
	case GL_SMOOTH:		GetRasterizerState()->SetShadeModel(RasterizerState::ShadeModelSmooth); return;
	default:			RecordError(GL_INVALID_ENUM);											return;
	}
}

