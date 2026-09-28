#include "re3generic.h"

static RG_Port bound_port;
static int port_is_bound;

int rg_bind_port(const RG_Port *port)
{
    if (port == NULL || port->ticks_ms == NULL || port->present == NULL ||
        port->poll_input == NULL ||
        port->file_open == NULL || port->file_size == NULL ||
        port->file_read_at == NULL || port->file_close == NULL)
        return 0;

    bound_port = *port;
    port_is_bound = 1;
    return 1;
}

const RG_Port *rg_bound_port(void)
{
    return port_is_bound ? &bound_port : NULL;
}
