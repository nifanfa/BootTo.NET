// ==========================================================================
//
// ContextMesg		Rendering Context Class for 3D Rendering Library
//
//					Mesh rendering
//
// --------------------------------------------------------------------------
//
// 03-25-2004		Hans-Martin Will	adaptation from prototype
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


using namespace EGL;

/* 
   Added by Valtteri Rahkonen:
   This is propably legacy (not declared on Context.h) and it wont compile
   with gcc, so I just if it out.
*/
#if !defined(EGL_ON_LINUX)

void Context :: DrawMesh(GLsizei count, GLenum type, GLsizei stride,
						 GLsizei offsetVertex, GLsizei strideVertex,
						 GLsizei offsetNormal, GLsizei strideNormal,
						 GLsizei offsetTexture, GLsizei strideTexture,
						 GLsizei offsetColor, GLsizei strideColor, 
						 const GLvoid *pointer) {

	m_Rasterizer->PrepareTriangle();
	RasterPos pos[3];

	if (type == GL_UNSIGNED_BYTE) {

		while (count-- > 0) {
			const U8 * pVertexIndex = reinterpret_cast<const U8 *>(pointer) + offsetVertex; 
			const U8 * pNormalIndex = reinterpret_cast<const U8 *>(pointer) + offsetNormal; 
			const U8 * pTextureIndex = reinterpret_cast<const U8 *>(pointer) + offsetTexture; 
			const U8 * pColorIndex = reinterpret_cast<const U8 *>(pointer) + offsetColor; 

			for (size_t index = 0; index < 3; ++index) {
				
				SelectArrayElement(*reinterpret_cast<const GLubyte *>(pVertexIndex),
								   *reinterpret_cast<const GLubyte *>(pNormalIndex),
								   *reinterpret_cast<const GLubyte *>(pTextureIndex),
								   *reinterpret_cast<const GLubyte *>(pColorIndex));

				CurrentValuesToRasterPos(pos + index);

				pVertexIndex += strideVertex;
				pNormalIndex += strideNormal;
				pTextureIndex += strideTexture;
				pColorIndex += strideColor;
			}

			RenderTriangle(pos[0], pos[1], pos[2]);
	
			pointer = reinterpret_cast<const U8 *>(pointer) + stride;
		}
	} else if (type == GL_UNSIGNED_SHORT) {
		while (count-- > 0) {
			const U8 * pVertexIndex = reinterpret_cast<const U8 *>(pointer) + offsetVertex; 
			const U8 * pNormalIndex = reinterpret_cast<const U8 *>(pointer) + offsetNormal; 
			const U8 * pTextureIndex = reinterpret_cast<const U8 *>(pointer) + offsetTexture; 
			const U8 * pColorIndex = reinterpret_cast<const U8 *>(pointer) + offsetColor; 

			for (size_t index = 0; index < 3; ++index) {
				
				SelectArrayElement(*reinterpret_cast<const GLushort *>(pVertexIndex),
								   *reinterpret_cast<const GLushort *>(pNormalIndex),
								   *reinterpret_cast<const GLushort *>(pTextureIndex),
								   *reinterpret_cast<const GLushort *>(pColorIndex));

				CurrentValuesToRasterPos(pos + index);

				pVertexIndex += strideVertex;
				pNormalIndex += strideNormal;
				pTextureIndex += strideTexture;
				pColorIndex += strideColor;
			}

			RenderTriangle(pos[0], pos[1], pos[2]);
	
			pointer = reinterpret_cast<const U8 *>(pointer) + stride;
		}
	}
}

#endif
