// ==========================================================================
//
// LineClipper.inc	Rendering Context Class for 3D Rendering Library
//
//					Clipping Code for Clipping of Lines
//					This code is included by "Lines.cpp" with different
//					settings for the COORDINATE
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
// Line clipping at a specific coordinate against the frustrum boundary
// Return false if the whole segment is clipped away, otherwise adjust
// line end points to lie withinn valid range.
// --------------------------------------------------------------------------

	if (from->m_ClipCoords.COORDINATE < -from->m_ClipCoords.w()) {
		if (to->m_ClipCoords.COORDINATE < -to->m_ClipCoords.w()) {
			return false;
		}

		EGL_Fixed c_x = from->m_ClipCoords.COORDINATE;
		EGL_Fixed c_w = -from->m_ClipCoords.w();
		EGL_Fixed p_x = to->m_ClipCoords.COORDINATE;
		EGL_Fixed p_w = -to->m_ClipCoords.w();
		
		Interpolate(*tempVertices, *from, *to, p_w - p_x, (p_w - p_x) - (c_w - c_x));
		from = tempVertices++;

		return true;

	} else if (from->m_ClipCoords.COORDINATE > from->m_ClipCoords.w()) {
		if (to->m_ClipCoords.COORDINATE > to->m_ClipCoords.w()) {
			return false;
		}

		EGL_Fixed c_x = from->m_ClipCoords.COORDINATE;
		EGL_Fixed c_w = from->m_ClipCoords.w();
		EGL_Fixed p_x = to->m_ClipCoords.COORDINATE;
		EGL_Fixed p_w = to->m_ClipCoords.w();
		
		Interpolate(*tempVertices, *from, *to, p_w - p_x, (p_w - p_x) - (c_w - c_x));
		from = tempVertices++;

		return true;

	} else if (to->m_ClipCoords.COORDINATE < -to->m_ClipCoords.w()) {

		EGL_Fixed c_x = to->m_ClipCoords.COORDINATE;
		EGL_Fixed c_w = -to->m_ClipCoords.w();
		EGL_Fixed p_x = from->m_ClipCoords.COORDINATE;
		EGL_Fixed p_w = -from->m_ClipCoords.w();
		
		Interpolate(*tempVertices, *to, *from, p_w - p_x, (p_w - p_x) - (c_w - c_x));
		to = tempVertices++;

		return true;

	} else if (to->m_ClipCoords.COORDINATE > to->m_ClipCoords.w()) {

		EGL_Fixed c_x = to->m_ClipCoords.COORDINATE;
		EGL_Fixed c_w = to->m_ClipCoords.w();
		EGL_Fixed p_x = from->m_ClipCoords.COORDINATE;
		EGL_Fixed p_w = from->m_ClipCoords.w();
		
		Interpolate(*tempVertices, *to, *from, p_w - p_x, (p_w - p_x) - (c_w - c_x));
		to = tempVertices++;

		return true;

	} else {
		// no clipping
		return true;
	}
