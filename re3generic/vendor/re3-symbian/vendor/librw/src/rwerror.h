#pragma once

namespace rw {

struct Error
{
	uint32 plugin;
	uint32 code;
};

void setError(Error *e);
Error *getError(Error *e);

#define _ERRORCODE(code, ...) code
char *dbgsprint(uint32 code, ...);

/* ecode is supposed to be in format "(errorcode, printf-arguments..)" */
#define RWERROR(ecode) do{          \
	rw::Error _e;                   \
	_e.plugin = PLUGIN_ID;      \
	_e.code = _ERRORCODE ecode; \
	rw::setError(&_e);              \
}while(0)

}
