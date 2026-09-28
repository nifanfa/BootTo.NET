// ==========================================================================
//
// gl.cpp	EGL Client API entry points
//
// --------------------------------------------------------------------------
//
// 08-04-2003	Hans-Martin Will	initial version
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


#define CONTEXT_EXEC(func) \
	Context * context = Context::DefaultContext();						\
	if (context) {														\
		context->func;													\
	}

#define CONTEXT_EXEC_RETURN(func) \
	Context * context = Context::DefaultContext();						\
	if (context) {														\
		return context->func;											\
	} else {															\
		return 0;														\
	}


GLAPI void APIENTRY glActiveTexture (GLenum texture) { 
	CONTEXT_EXEC(ActiveTexture(texture));
}

GLAPI void APIENTRY glAlphaFunc (GLenum func, GLclampf ref) { 
	CONTEXT_EXEC(AlphaFunc(func, ref));
}

GLAPI void APIENTRY glAlphaFuncx (GLenum func, GLclampx ref) { 
	CONTEXT_EXEC(AlphaFuncx(func, ref));
}

GLAPI void APIENTRY glBindBuffer(GLenum target, GLuint buffer) {
	CONTEXT_EXEC(BindBuffer(target, buffer));
}

GLAPI void APIENTRY glBindTexture (GLenum target, GLuint texture) { 
	CONTEXT_EXEC(BindTexture(target, texture));
}

GLAPI void APIENTRY glBlendFunc (GLenum sfactor, GLenum dfactor) { 
	CONTEXT_EXEC(BlendFunc(sfactor, dfactor));
}

GLAPI void APIENTRY glBufferData(GLenum target, GLsizeiptr size, const GLvoid *data, GLenum usage) {
	CONTEXT_EXEC(BufferData(target, size, data, usage));
}

GLAPI void APIENTRY glBufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid *data) {
	CONTEXT_EXEC(BufferSubData(target, offset, size, data));
}

GLAPI void APIENTRY glClear (GLbitfield mask) { 
	CONTEXT_EXEC(Clear(mask));
}

GLAPI void APIENTRY glClearColor (GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha) { 
	CONTEXT_EXEC(ClearColor(red, green, blue, alpha));
}

GLAPI void APIENTRY glClearColorx (GLclampx red, GLclampx green, GLclampx blue, GLclampx alpha) { 
	CONTEXT_EXEC(ClearColorx(red, green, blue, alpha));
}

GLAPI void APIENTRY glClearDepthf (GLclampf depth) {
	CONTEXT_EXEC(ClearDepthf(depth));
}

GLAPI void APIENTRY glClearDepthx (GLclampx depth) { 
	CONTEXT_EXEC(ClearDepthx(depth));
}

GLAPI void APIENTRY glClearStencil (GLint s) { 
	CONTEXT_EXEC(ClearStencil(s));
}

GLAPI void APIENTRY glClipPlanex(GLenum plane, const GLfixed *equation) {
	CONTEXT_EXEC(ClipPlanex(plane, equation));
}

GLAPI void APIENTRY glClipPlanef(GLenum plane, const GLfloat *equation) {
	CONTEXT_EXEC(ClipPlanef(plane, equation));
}

GLAPI void APIENTRY glClientActiveTexture (GLenum texture) { 
	CONTEXT_EXEC(ClientActiveTexture(texture));
}

GLAPI void APIENTRY glColor4f (GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha) { 
	CONTEXT_EXEC(Color4f(red, green, blue, alpha));
}

GLAPI void APIENTRY glColor4x (GLfixed red, GLfixed green, GLfixed blue, GLfixed alpha) { 
	CONTEXT_EXEC(Color4x(red, green, blue, alpha));
}

GLAPI void APIENTRY glColor4ub (GLubyte red, GLubyte green, GLubyte blue, GLubyte alpha) { 
	CONTEXT_EXEC(Color4x((red << 8) | red, (green << 8) | green, (blue << 8) | blue, (alpha << 8) | alpha));
}

GLAPI void APIENTRY glColorMask (GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha) { 
	CONTEXT_EXEC(ColorMask(red, green , blue, alpha));
}

GLAPI void APIENTRY glColorPointer (GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) { 
	CONTEXT_EXEC(ColorPointer(size, type, stride, pointer));
}

GLAPI void APIENTRY glCompressedTexImage2D (GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const GLvoid *data) { 
	CONTEXT_EXEC(CompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data));
}

GLAPI void APIENTRY glCompressedTexSubImage2D (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, const GLvoid *data) { 
	CONTEXT_EXEC(CompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, data));
}

GLAPI void APIENTRY glCopyTexImage2D (GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border) { 
	CONTEXT_EXEC(CopyTexImage2D(target, level, internalformat, x, y, width, height, border));
}

GLAPI void APIENTRY glCopyTexSubImage2D (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height) { 
	CONTEXT_EXEC(CopyTexSubImage2D(target, level, xoffset, yoffset, x, y, width, height));
}

GLAPI void APIENTRY glCullFace (GLenum mode) { 
	CONTEXT_EXEC(CullFace(mode));
}

GLAPI void APIENTRY glDeleteBuffers(GLsizei n, const GLuint *buffers) {
	CONTEXT_EXEC(DeleteBuffers(n, buffers));
}

GLAPI void APIENTRY glDeleteTextures (GLsizei n, const GLuint *textures) { 
	CONTEXT_EXEC(DeleteTextures(n, textures));
}

GLAPI void APIENTRY glDepthFunc (GLenum func) { 
	CONTEXT_EXEC(DepthFunc(func));
}

GLAPI void APIENTRY glDepthMask (GLboolean flag) { 
	CONTEXT_EXEC(DepthMask(flag));
}

GLAPI void APIENTRY glDepthRangef (GLclampf zNear, GLclampf zFar) { 
	CONTEXT_EXEC(DepthRangef(zNear, zFar));
}

GLAPI void APIENTRY glDepthRangex (GLclampx zNear, GLclampx zFar) { 
	CONTEXT_EXEC(DepthRangex(zNear, zFar));
}

GLAPI void APIENTRY glDisable (GLenum cap) { 
	CONTEXT_EXEC(Disable(cap));
}

GLAPI void APIENTRY glDisableClientState (GLenum array) { 
	CONTEXT_EXEC(DisableClientState(array));
}

GLAPI void APIENTRY glDrawArrays (GLenum mode, GLint first, GLsizei count) { 
	CONTEXT_EXEC(DrawArrays(mode, first, count));
}

GLAPI void APIENTRY glDrawElements (GLenum mode, GLsizei count, GLenum type, const GLvoid *indices) { 
	CONTEXT_EXEC(DrawElements(mode, count, type, indices));
}

GLAPI void APIENTRY glEnable (GLenum cap) { 
	CONTEXT_EXEC(Enable(cap));
}

GLAPI void APIENTRY glEnableClientState (GLenum array) { 
	CONTEXT_EXEC(EnableClientState(array));
}

GLAPI void APIENTRY glFinish (void) { 
	CONTEXT_EXEC(Finish());
}

GLAPI void APIENTRY glFlush (void) { 
	CONTEXT_EXEC(Flush());
}

GLAPI void APIENTRY glFogf (GLenum pname, GLfloat param) { 
	CONTEXT_EXEC(Fogf(pname, param));
}

GLAPI void APIENTRY glFogfv (GLenum pname, const GLfloat *params) { 
	CONTEXT_EXEC(Fogfv(pname, params));
}

GLAPI void APIENTRY glFogx (GLenum pname, GLfixed param) { 
	CONTEXT_EXEC(Fogx(pname, param));
}

GLAPI void APIENTRY glFogxv (GLenum pname, const GLfixed *params) { 
	CONTEXT_EXEC(Fogxv(pname, params));
}

GLAPI void APIENTRY glFrontFace (GLenum mode) { 
	CONTEXT_EXEC(FrontFace(mode));
}

GLAPI void APIENTRY glFrustumf (GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar) { 
	CONTEXT_EXEC(Frustumf(left, right, bottom, top, zNear, zFar));
}

GLAPI void APIENTRY glFrustumx (GLfixed left, GLfixed right, GLfixed bottom, GLfixed top, GLfixed zNear, GLfixed zFar) { 
	CONTEXT_EXEC(Frustumx(left, right, bottom, top, zNear, zFar));
}

GLAPI void APIENTRY glGetBooleanv(GLenum pname, GLboolean *params) {
	CONTEXT_EXEC(GetBooleanv(pname, params));
}

GLAPI void APIENTRY glGetBufferParameteriv(GLenum target, GLenum pname, GLint *params) {
	CONTEXT_EXEC(GetBufferParameteriv(target, pname, params));
}

GLAPI void APIENTRY glGetClipPlanef(GLenum pname, GLfloat eqn[4]) {
	CONTEXT_EXEC(GetClipPlanef(pname, eqn));
}

GLAPI void APIENTRY glGetClipPlanex(GLenum pname, GLfixed eqn[4]) {
	CONTEXT_EXEC(GetClipPlanex(pname, eqn));
}

GLAPI void APIENTRY glGenBuffers(GLsizei n, GLuint *buffers) {
	CONTEXT_EXEC(GenBuffers(n, buffers));
}

GLAPI void APIENTRY glGenTextures (GLsizei n, GLuint *textures) { 
	CONTEXT_EXEC(GenTextures(n, textures));
}

GLAPI GLenum APIENTRY glGetError (void) { 
	CONTEXT_EXEC_RETURN(GetError());
}

GLAPI void APIENTRY glGetFixedv(GLenum pname, GLfixed *params) {
	CONTEXT_EXEC(GetFixedv(pname, params));
}

GLAPI void APIENTRY glGetFloatv(GLenum pname, GLfloat *params) {
	CONTEXT_EXEC(GetFloatv(pname, params));
}

GLAPI void APIENTRY glGetIntegerv (GLenum pname, GLint *params) {
	CONTEXT_EXEC(GetIntegerv(pname, params));
}

GLAPI void APIENTRY glGetLightfv(GLenum light, GLenum pname, GLfloat *params) {
	CONTEXT_EXEC(GetLightfv(light, pname, params));
}

GLAPI void APIENTRY glGetLightxv(GLenum light, GLenum pname, GLfixed *params) {
	CONTEXT_EXEC(GetLightxv(light, pname, params));
}

GLAPI void APIENTRY glGetMaterialfv(GLenum face, GLenum pname, GLfloat *params) {
	CONTEXT_EXEC(GetMaterialfv(face, pname, params));
}

GLAPI void APIENTRY glGetMaterialxv(GLenum face, GLenum pname, GLfixed *params) {
	CONTEXT_EXEC(GetMaterialxv(face, pname, params));
}

GLAPI void APIENTRY glGetPointerv(GLenum pname, void **params) {
	CONTEXT_EXEC(GetPointerv(pname, params));
}

GLAPI const GLubyte * APIENTRY glGetString (GLenum name) { 
	CONTEXT_EXEC_RETURN(GetString(name));
}

GLAPI void APIENTRY glGetTexEnviv(GLenum env, GLenum pname, GLint *params) {
	CONTEXT_EXEC(GetTexEnviv(env, pname, params));
}

GLAPI void APIENTRY glGetTexEnvfv(GLenum env, GLenum pname, GLfloat *params) {
	CONTEXT_EXEC(GetTexEnvfv(env, pname, params));
}

GLAPI void APIENTRY glGetTexEnvxv(GLenum env, GLenum pname, GLfixed *params) {
	CONTEXT_EXEC(GetTexEnvxv(env, pname, params));
}

GLAPI void APIENTRY glGetTexParameteriv(GLenum target, GLenum pname, GLint *params) {
	CONTEXT_EXEC(GetTexParameteriv(target, pname, params));
}

GLAPI void APIENTRY glGetTexParameterfv(GLenum target, GLenum pname, GLfloat *params) {
	CONTEXT_EXEC(GetTexParameterfv(target, pname, params));
}

GLAPI void APIENTRY glGetTexParameterxv(GLenum target, GLenum pname, GLfixed *params) {
	CONTEXT_EXEC(GetTexParameterxv(target, pname, params));
}

GLAPI void APIENTRY glHint (GLenum target, GLenum mode) { 
	CONTEXT_EXEC(Hint(target, mode));
}

GLAPI GLboolean APIENTRY glIsBuffer(GLuint buffer) {
	CONTEXT_EXEC_RETURN(IsBuffer(buffer));
}

GLAPI GLboolean APIENTRY glIsEnabled(GLenum cap) {
	CONTEXT_EXEC_RETURN(IsEnabled(cap));
}

GLAPI GLboolean APIENTRY glIsTexture(GLuint texture) {
	CONTEXT_EXEC_RETURN(IsTexture(texture));
}

GLAPI void APIENTRY glLightModelf (GLenum pname, GLfloat param) { 
	CONTEXT_EXEC(LightModelf(pname, param));
}

GLAPI void APIENTRY glLightModelfv (GLenum pname, const GLfloat *params) { 
	CONTEXT_EXEC(LightModelfv(pname, params));
}

GLAPI void APIENTRY glLightModelx (GLenum pname, GLfixed param) { 
	CONTEXT_EXEC(LightModelx(pname, param));
}

GLAPI void APIENTRY glLightModelxv (GLenum pname, const GLfixed *params) { 
	CONTEXT_EXEC(LightModelxv(pname, params));
}

GLAPI void APIENTRY glLightf (GLenum light, GLenum pname, GLfloat param) { 
	CONTEXT_EXEC(Lightf(light, pname, param));
}

GLAPI void APIENTRY glLightfv (GLenum light, GLenum pname, const GLfloat *params) { 
	CONTEXT_EXEC(Lightfv(light, pname, params));
}

GLAPI void APIENTRY glLightx (GLenum light, GLenum pname, GLfixed param) { 
	CONTEXT_EXEC(Lightx(light, pname, param));
}

GLAPI void APIENTRY glLightxv (GLenum light, GLenum pname, const GLfixed *params) { 
	CONTEXT_EXEC(Lightxv(light, pname, params));
}

GLAPI void APIENTRY glLineWidth (GLfloat width) { 
	CONTEXT_EXEC(LineWidth(width));
}

GLAPI void APIENTRY glLineWidthx (GLfixed width) { 
	CONTEXT_EXEC(LineWidthx(width));
}

GLAPI void APIENTRY glLoadIdentity (void) { 
	CONTEXT_EXEC(LoadIdentity());
}

GLAPI void APIENTRY glLoadMatrixf (const GLfloat *m) { 
	CONTEXT_EXEC(LoadMatrixf(m));
}

GLAPI void APIENTRY glLoadMatrixx (const GLfixed *m) { 
	CONTEXT_EXEC(LoadMatrixx(m));
}

GLAPI void APIENTRY glLogicOp (GLenum opcode) { 
	CONTEXT_EXEC(LogicOp(opcode));
}

GLAPI void APIENTRY glMaterialf (GLenum face, GLenum pname, GLfloat param) { 
	CONTEXT_EXEC(Materialf(face, pname, param));
}

GLAPI void APIENTRY glMaterialfv (GLenum face, GLenum pname, const GLfloat *params) { 
	CONTEXT_EXEC(Materialfv(face, pname, params));
}

GLAPI void APIENTRY glMaterialx (GLenum face, GLenum pname, GLfixed param) { 
	CONTEXT_EXEC(Materialx(face, pname, param));
}

GLAPI void APIENTRY glMaterialxv (GLenum face, GLenum pname, const GLfixed *params) { 
	CONTEXT_EXEC(Materialxv(face, pname, params));
}

GLAPI void APIENTRY glMatrixMode (GLenum mode) { 
	CONTEXT_EXEC(MatrixMode(mode));
}

GLAPI void APIENTRY glMultMatrixf (const GLfloat *m) { 
	CONTEXT_EXEC(MultMatrixf(m));
}

GLAPI void APIENTRY glMultMatrixx (const GLfixed *m) { 
	CONTEXT_EXEC(MultMatrixx(m));
}

GLAPI void APIENTRY glMultiTexCoord4f (GLenum target, GLfloat s, GLfloat t, GLfloat r, GLfloat q) { 
	CONTEXT_EXEC(MultiTexCoord4f(target, s, t, r, q));
}

GLAPI void APIENTRY glMultiTexCoord4x (GLenum target, GLfixed s, GLfixed t, GLfixed r, GLfixed q) { 
	CONTEXT_EXEC(MultiTexCoord4x(target, s, t, r, q));
}

GLAPI void APIENTRY glNormal3f (GLfloat nx, GLfloat ny, GLfloat nz) { 
	CONTEXT_EXEC(Normal3f(nx, ny, nz));
}

GLAPI void APIENTRY glNormal3x (GLfixed nx, GLfixed ny, GLfixed nz) { 
	CONTEXT_EXEC(Normal3x(nx, ny, nz));
}

GLAPI void APIENTRY glNormalPointer (GLenum type, GLsizei stride, const GLvoid *pointer) { 
	CONTEXT_EXEC(NormalPointer(type, stride, pointer));
}

GLAPI void APIENTRY glOrthof (GLfloat left, GLfloat right, GLfloat bottom, GLfloat top, GLfloat zNear, GLfloat zFar) { 
	CONTEXT_EXEC(Orthof(left, right, bottom, top, zNear, zFar));
}

GLAPI void APIENTRY glOrthox (GLfixed left, GLfixed right, GLfixed bottom, GLfixed top, GLfixed zNear, GLfixed zFar) { 
	CONTEXT_EXEC(Orthox(left, right, bottom, top, zNear, zFar));
}

GLAPI void APIENTRY glPixelStorei (GLenum pname, GLint param) { 
	CONTEXT_EXEC(PixelStorei(pname, param));
}

GLAPI void APIENTRY glPointParameterf(GLenum pname, GLfloat param) {
	CONTEXT_EXEC(PointParameterf(pname, param));
}

GLAPI void APIENTRY glPointParameterfv(GLenum pname, const GLfloat *params) {
	CONTEXT_EXEC(PointParameterfv(pname, params));
}

GLAPI void APIENTRY glPointParameterx(GLenum pname, GLfixed param) {
	CONTEXT_EXEC(PointParameterx(pname, param));
}

GLAPI void APIENTRY glPointParameterxv(GLenum pname, const GLfixed *params) {
	CONTEXT_EXEC(PointParameterxv(pname, params));
}

GLAPI void APIENTRY glPointSize (GLfloat size) { 
	CONTEXT_EXEC(PointSize(size));
}

GLAPI void APIENTRY glPointSizex (GLfixed size) { 
	CONTEXT_EXEC(PointSizex(size));
}

GLAPI void APIENTRY glPolygonOffset (GLfloat factor, GLfloat units) { 
	CONTEXT_EXEC(PolygonOffset(factor, units));
}

GLAPI void APIENTRY glPolygonOffsetx (GLfixed factor, GLfixed units) { 
	CONTEXT_EXEC(PolygonOffsetx(factor, units));
}

GLAPI void APIENTRY glPopMatrix (void) { 
	CONTEXT_EXEC(PopMatrix());
}

GLAPI void APIENTRY glPushMatrix (void) { 
	CONTEXT_EXEC(PushMatrix());
}

GLAPI GLbitfield APIENTRY glQueryMatrixxOES(GLfixed mantissa[16], GLint exponent[16]) {
	CONTEXT_EXEC_RETURN(QueryMatrixx(mantissa, exponent));
}

GLAPI void APIENTRY glReadPixels (GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLvoid *pixels) { 
	CONTEXT_EXEC(ReadPixels(x, y, width, height, format, type, pixels));
}

GLAPI void APIENTRY glRotatef (GLfloat angle, GLfloat x, GLfloat y, GLfloat z) { 
	CONTEXT_EXEC(Rotatef(angle, x, y, z));
}

GLAPI void APIENTRY glRotatex (GLfixed angle, GLfixed x, GLfixed y, GLfixed z) { 
	CONTEXT_EXEC(Rotatex(angle, x, y, z));
}

GLAPI void APIENTRY glSampleCoverage (GLclampf value, GLboolean invert) { 
	CONTEXT_EXEC(SampleCoverage(value, invert));
}

GLAPI void APIENTRY glSampleCoveragex (GLclampx value, GLboolean invert) { 
	CONTEXT_EXEC(SampleCoveragex(value, invert));
}

GLAPI void APIENTRY glScalef (GLfloat x, GLfloat y, GLfloat z) { 
	CONTEXT_EXEC(Scalef(x, y, z));
}

GLAPI void APIENTRY glScalex (GLfixed x, GLfixed y, GLfixed z) { 
	CONTEXT_EXEC(Scalex(x, y, z));
}

GLAPI void APIENTRY glScissor (GLint x, GLint y, GLsizei width, GLsizei height) { 
	CONTEXT_EXEC(Scissor(x, y, width, height));
}

GLAPI void APIENTRY glShadeModel (GLenum mode) { 
	CONTEXT_EXEC(ShadeModel(mode));
}

GLAPI void APIENTRY glStencilFunc (GLenum func, GLint ref, GLuint mask) { 
	CONTEXT_EXEC(StencilFunc(func, ref, mask));
}

GLAPI void APIENTRY glStencilMask (GLuint mask) { 
	CONTEXT_EXEC(StencilMask(mask));
}

GLAPI void APIENTRY glStencilOp (GLenum fail, GLenum zfail, GLenum zpass) { 
	CONTEXT_EXEC(StencilOp(fail, zfail, zpass));
}

GLAPI void APIENTRY glTexCoordPointer (GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) { 
	CONTEXT_EXEC(TexCoordPointer(size, type, stride, pointer));
}

GLAPI void APIENTRY glTexEnvf (GLenum target, GLenum pname, GLfloat param) { 
	CONTEXT_EXEC(TexEnvf(target, pname, param));
}

GLAPI void APIENTRY glTexEnvi (GLenum target, GLenum pname, GLint param) { 
	CONTEXT_EXEC(TexEnvi(target, pname, param));
}

GLAPI void APIENTRY glTexEnvfv (GLenum target, GLenum pname, const GLfloat *params) { 
	CONTEXT_EXEC(TexEnvfv(target, pname, params));
}

GLAPI void APIENTRY glTexEnviv (GLenum target, GLenum pname, const GLint *params) { 
	CONTEXT_EXEC(TexEnviv(target, pname, params));
}

GLAPI void APIENTRY glTexEnvx (GLenum target, GLenum pname, GLfixed param) { 
	CONTEXT_EXEC(TexEnvx(target, pname, param));
}

GLAPI void APIENTRY glTexEnvxv (GLenum target, GLenum pname, const GLfixed *params) { 
	CONTEXT_EXEC(TexEnvxv(target, pname, params));
}

GLAPI void APIENTRY glTexImage2D (GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const GLvoid *pixels) { 
	CONTEXT_EXEC(TexImage2D(target, level, internalformat, width, height, border, format, type, pixels));
}

GLAPI void APIENTRY glTexParameteri (GLenum target, GLenum pname, GLint param) { 
	CONTEXT_EXEC(TexParameteri(target, pname, param));
}

GLAPI void APIENTRY glTexParameterf (GLenum target, GLenum pname, GLfloat param) { 
	CONTEXT_EXEC(TexParameterf(target, pname, param));
}

GLAPI void APIENTRY glTexParameterx (GLenum target, GLenum pname, GLfixed param) { 
	CONTEXT_EXEC(TexParameterx(target, pname, param));
}

GLAPI void APIENTRY glTexParameterfv (GLenum target, GLenum pname, const GLfloat* params) { 
	CONTEXT_EXEC(TexParameterfv(target, pname, params));
}

GLAPI void APIENTRY glTexParameteriv (GLenum target, GLenum pname, const GLint* params) { 
	CONTEXT_EXEC(TexParameteriv(target, pname, params));
}

GLAPI void APIENTRY glTexParameterxv (GLenum target, GLenum pname, const GLfixed* params) { 
	CONTEXT_EXEC(TexParameterxv(target, pname, params));
}

GLAPI void APIENTRY glTexSubImage2D (GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const GLvoid *pixels) { 
	CONTEXT_EXEC(TexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels));
}

GLAPI void APIENTRY glTranslatef (GLfloat x, GLfloat y, GLfloat z) { 
	CONTEXT_EXEC(Translatef(x, y, z));
}

GLAPI void APIENTRY glTranslatex (GLfixed x, GLfixed y, GLfixed z) { 
	CONTEXT_EXEC(Translatex(x, y, z));
}

GLAPI void APIENTRY glVertexPointer (GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) { 
	CONTEXT_EXEC(VertexPointer(size, type, stride, pointer));
}

GLAPI void APIENTRY glViewport (GLint x, GLint y, GLsizei width, GLsizei height) { 
	CONTEXT_EXEC(Viewport(x, y, width, height));
}

/*****************************************************************************************/
/*                                 OES extension functions                               */
/*****************************************************************************************/

/* OES_point_size_array */
GLAPI void APIENTRY glPointSizePointerOES(GLenum type, GLsizei stride, const GLvoid *pointer) {
	CONTEXT_EXEC(PointSizePointer(type, stride, pointer));
}

/* OES_matrix_palette */
GLAPI void APIENTRY glLoadPaletteFromModelViewMatrixOES(void) {
	CONTEXT_EXEC(LoadPaletteFromModelViewMatrix());
}

GLAPI void APIENTRY glCurrentPaletteMatrixOES(GLuint index) {
	CONTEXT_EXEC(CurrentPaletteMatrix(index));
}

GLAPI void APIENTRY glMatrixIndexPointerOES(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) {
	CONTEXT_EXEC(MatrixIndexPointer(size, type, stride, pointer));
}

GLAPI void APIENTRY glWeightPointerOES(GLint size, GLenum type, GLsizei stride, const GLvoid *pointer) {
	CONTEXT_EXEC(WeightPointer(size, type, stride, pointer));
}
