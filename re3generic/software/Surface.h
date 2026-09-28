#ifndef RE3_GENERIC_VINCENT_SURFACE_H
#define RE3_GENERIC_VINCENT_SURFACE_H

#include "OGLES.h"
#include "GLES/egl.h"
#include "Types.h"
#include "Config.h"
#include "Color.h"

namespace EGL {

class Context;

class Surface {
public:
    explicit Surface(const Config &config);
    ~Surface();

    void ClearDepthBuffer(U32 depth, bool mask, const Rect &scissor);
    void ClearColorBuffer(const Color &rgba, const Color &mask, const Rect &scissor);
    void ClearStencilBuffer(U32 value, U32 mask, const Rect &scissor);
    void ClearDepthBuffer(U32 depth, bool mask) { ClearDepthBuffer(depth, mask, m_Rect); }
    void ClearColorBuffer(const Color &rgba, const Color &mask) { ClearColorBuffer(rgba, mask, m_Rect); }
    void ClearStencilBuffer(U32 value, U32 mask) { ClearStencilBuffer(value, mask, m_Rect); }

    U16 GetWidth() const { return m_Rect.width; }
    U16 GetHeight() const { return m_Rect.height; }
    const Rect &GetRect() const { return m_Rect; }
    U16 *GetColorBuffer() { return m_ColorBuffer; }
    U8 *GetAlphaBuffer() { return m_AlphaBuffer; }
    U32 *GetDepthBuffer() { return m_DepthBuffer; }
    U32 *GetStencilBuffer() { return m_StencilBuffer; }
    Config *GetConfig() { return &m_Config; }
    void SetCurrentContext(Context *context) { m_CurrentContext = context; }
    Context *GetCurrentContext() const { return m_CurrentContext; }
    void Dispose();

private:
    Config m_Config;
    Rect m_Rect;
    U16 *m_ColorBuffer;
    U8 *m_AlphaBuffer;
    U32 *m_DepthBuffer;
    U32 *m_StencilBuffer;
    Context *m_CurrentContext;
};

}

#endif
