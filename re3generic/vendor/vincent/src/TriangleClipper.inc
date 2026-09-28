// ==========================================================================
//
// TriangleClipper.inc	Rendering Context Class for 3D Rendering Library
//
//				Clipping Code for Clipping of Triangles
//				This code is included by "ContextTriangles.cpp" with different
//				settings for the COORDINATE, COMPARISON, CLIP_VALUE
//
// --------------------------------------------------------------------------
//
// 20-12-2003		Hans-Martin Will	initial version
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

// --------------------------------------------------------------------------
// Triangle clipping at a specific coordinate against the frustrum boundary
// Return number of vertices retained.
// --------------------------------------------------------------------------

	if (inputCount < 3) {
		return 0;
	}

	RasterPos * previous = input[inputCount - 1];
	RasterPos * current;
	int resultCount = 0;

	for (int index = 0; index < inputCount; ++index) {

		current = input[index];

		if (current->m_ClipCoords.COORDINATE COMPARISON CLIP_VALUE(current)) {

			if (previous->m_ClipCoords.COORDINATE COMPARISON CLIP_VALUE(previous)) {
				// line segment between previous and current is fully contained in cube
				output[resultCount++] = current;
				//previous = current;
			} else {
				// line segment between previous and current is intersected;
				// create vertex at intersection, then add current
				RasterPos & newVertex = *nextTemporary++;
				output[resultCount++] = &newVertex;
				// initialize the new vertex
				//EGL_Fixed factor = 
				//	InterpolationCoefficient(previous->m_ClipCoords.COORDINATE, CLIP_VALUE(previous), current->m_ClipCoords.COORDINATE);
				
				EGL_Fixed c_x = current->m_ClipCoords.COORDINATE;
				EGL_Fixed c_w = CLIP_VALUE(current);
				EGL_Fixed p_x = previous->m_ClipCoords.COORDINATE;
				EGL_Fixed p_w = CLIP_VALUE(previous);
				
				EGL_Fixed factor = EGL_Div(p_w - p_x, (p_w - p_x) - (c_w - c_x)); 
				Interpolate(newVertex, *current, *previous, factor);
				newVertex.m_ClipCoords.SET_COORDINATE(CLIP_VALUE((&newVertex)));

				output[resultCount++] = current;
				//previous = current;
			}
		} else {
			if (previous->m_ClipCoords.COORDINATE COMPARISON CLIP_VALUE(previous)) {
				// line segment between previous and current is intersected;
				// create vertex at intersection and add it
				RasterPos & newVertex = *nextTemporary++;
				output[resultCount++] = &newVertex;
				// initialize the new vertex
				//EGL_Fixed factor = 
				//	InterpolationCoefficient(current->m_ClipCoords.COORDINATE, CLIP_VALUE(previous), previous->m_ClipCoords.COORDINATE);
				
				EGL_Fixed c_x = current->m_ClipCoords.COORDINATE;
				EGL_Fixed c_w = CLIP_VALUE(current);
				EGL_Fixed p_x = previous->m_ClipCoords.COORDINATE;
				EGL_Fixed p_w = CLIP_VALUE(previous);
				
				EGL_Fixed factor = EGL_Div(p_w - p_x, (p_w - p_x) - (c_w - c_x)); 
				Interpolate(newVertex, *current, *previous, factor);
				newVertex.m_ClipCoords.SET_COORDINATE(CLIP_VALUE((&newVertex)));

				//previous = current;
			}
		}

		previous = current;
	}

	return resultCount;
