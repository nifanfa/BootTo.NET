#pragma once

FILE *_fcaseopen(char const *filename, char const *mode);
#if defined(_WIN32)
#define fcaseopen fopen
#else
#define fcaseopen _fcaseopen
#endif
