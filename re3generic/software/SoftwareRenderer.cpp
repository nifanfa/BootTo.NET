#include "stdafx.h"
#include "re3generic_renderer.h"
#include "Config.h"
#include "Context.h"
#include "Surface.h"

#include <new>

struct RG_SoftwareRenderer {
    EGL::Context *context;
    EGL::Surface *surface;
};

RG_SoftwareRenderer *rg_renderer_create(uint32_t width, uint32_t height)
{
    EGLConfig candidate = 0;
    EGLint count = 0;
    if (width == 0 || height == 0 || width > 1024 || height > 1024 ||
        EGL::Context::GetCurrentContext() != 0 ||
        !EGL::Config::GetConfigs(&candidate, 1, &count) || count != 1)
        return 0;

    EGL::Config config(*candidate);
    config.SetConfigAttrib(EGL_WIDTH, width);
    config.SetConfigAttrib(EGL_HEIGHT, height);

    RG_SoftwareRenderer *renderer = new (std::nothrow) RG_SoftwareRenderer;
    if (!renderer)
        return 0;
    renderer->context = new (std::nothrow) EGL::Context(config);
    renderer->surface = new (std::nothrow) EGL::Surface(config);
    if (!renderer->context || !renderer->surface) {
        delete renderer->context;
        delete renderer->surface;
        delete renderer;
        return 0;
    }

    renderer->context->SetDrawSurface(renderer->surface);
    renderer->context->SetReadSurface(renderer->surface);
    EGL::Context::SetCurrentContext(renderer->context);
    return renderer;
}

void rg_renderer_destroy(RG_SoftwareRenderer *renderer)
{
    if (!renderer)
        return;
    EGL::Context::SetCurrentContext(0);
    delete renderer->context;
    delete renderer->surface;
    delete renderer;
}

const void *rg_renderer_pixels(const RG_SoftwareRenderer *renderer)
{
    return renderer ? renderer->surface->GetColorBuffer() : 0;
}

uint32_t rg_renderer_pitch(const RG_SoftwareRenderer *renderer)
{
    return renderer ? renderer->surface->GetWidth() * sizeof(uint16_t) : 0;
}

void rg_renderer_present(const RG_SoftwareRenderer *renderer)
{
    const RG_Port *port = rg_bound_port();
    if (renderer && port)
        port->present(port->userdata, rg_renderer_pixels(renderer),
                      renderer->surface->GetWidth(), renderer->surface->GetHeight(),
                      rg_renderer_pitch(renderer), RG_PIXEL_RGB565);
}
