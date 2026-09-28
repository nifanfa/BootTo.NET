#include "stdafx.h"
#include "Context.h"

namespace EGL {

static Context *current_context;

void Context::SetCurrentContext(Context *context)
{
    if (current_context == context)
        return;
    if (current_context)
        current_context->SetCurrent(false);
    current_context = context;
    if (current_context)
        current_context->SetCurrent(true);
}

Context *Context::GetCurrentContext()
{
    return current_context;
}

}
