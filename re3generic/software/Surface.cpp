#include "stdafx.h"
#include "Surface.h"

namespace EGL {

Surface::Surface(const Config &config)
    : m_Config(config),
      m_Rect(0, 0, config.GetConfigAttrib(EGL_WIDTH),
             config.GetConfigAttrib(EGL_HEIGHT)),
      m_ColorBuffer(new U16[m_Rect.width * m_Rect.height]()),
      m_AlphaBuffer(new U8[m_Rect.width * m_Rect.height]()),
      m_DepthBuffer(new U32[m_Rect.width * m_Rect.height]()),
      m_StencilBuffer(new U32[m_Rect.width * m_Rect.height]()),
      m_CurrentContext(0)
{
}

Surface::~Surface()
{
    delete[] m_ColorBuffer;
    delete[] m_AlphaBuffer;
    delete[] m_DepthBuffer;
    delete[] m_StencilBuffer;
}

void Surface::Dispose()
{
    if (m_CurrentContext == 0)
        delete this;
}

void Surface::ClearDepthBuffer(U32 depth, bool mask, const Rect &scissor)
{
    if (!mask)
        return;
    Rect clipped = Rect::Intersect(m_Rect, scissor);
    for (int row = clipped.y; row < clipped.y + clipped.height; ++row)
        for (int col = clipped.x; col < clipped.x + clipped.width; ++col)
            m_DepthBuffer[row * m_Rect.width + col] = depth;
}

void Surface::ClearStencilBuffer(U32 value, U32 mask, const Rect &scissor)
{
    Rect clipped = Rect::Intersect(m_Rect, scissor);
    for (int row = clipped.y; row < clipped.y + clipped.height; ++row)
        for (int col = clipped.x; col < clipped.x + clipped.width; ++col) {
            U32 &pixel = m_StencilBuffer[row * m_Rect.width + col];
            pixel = (pixel & ~mask) | (value & mask);
        }
}

void Surface::ClearColorBuffer(const Color &rgba, const Color &mask,
                               const Rect &scissor)
{
    U16 color = rgba.ConvertTo565();
    U16 color_mask = mask.ConvertTo565();
    Rect clipped = Rect::Intersect(m_Rect, scissor);
    for (int row = clipped.y; row < clipped.y + clipped.height; ++row)
        for (int col = clipped.x; col < clipped.x + clipped.width; ++col) {
            int index = row * m_Rect.width + col;
            U16 &pixel = m_ColorBuffer[index];
            pixel = (pixel & ~color_mask) | (color & color_mask);
            if (mask.A())
                m_AlphaBuffer[index] = rgba.A();
        }
}

}
