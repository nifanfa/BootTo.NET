#ifndef QUAKEGENERIC_LIBC_H
#define QUAKEGENERIC_LIBC_H

#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>

#include "../NativeLib/printf.h"

#ifndef NULL
#define NULL ((void *)0)
#endif

#ifndef EOF
#define EOF (-1)
#endif

#ifndef SEEK_SET
#define SEEK_SET 0
#define SEEK_CUR 1
#define SEEK_END 2
#endif

#ifndef INT_MAX
#define INT_MAX 2147483647
#define INT_MIN (-2147483647 - 1)
#endif

typedef struct quakegeneric_file_s
{
    int handle;
    int eof;
    int error;
    int ungot;
} FILE;

#define stdout ((FILE *)1)
#define stderr ((FILE *)2)

typedef struct quakegeneric_jmp_buf_s
{
    uint64_t rbx;
    uint64_t rsp;
    uint64_t rbp;
    uint64_t rsi;
    uint64_t rdi;
    uint64_t r12;
    uint64_t r13;
    uint64_t r14;
    uint64_t r15;
    uint64_t rip;
    uint32_t mxcsr;
    uint16_t fp_control;
    uint8_t reserved[10];
    uint8_t xmm6_to_xmm15[160];
} quakegeneric_jmp_state;

typedef quakegeneric_jmp_state jmp_buf[1];

#if UINTPTR_MAX > 0xffffffffU
#define QG_LARGE_LOCAL static
#else
#define QG_LARGE_LOCAL
#endif

extern int qg_setjmp(quakegeneric_jmp_state *state);
__declspec(noreturn) extern void qg_longjmp(quakegeneric_jmp_state *state, int value);
#define setjmp(state) qg_setjmp(state)
#define longjmp(state, value) qg_longjmp(state, value)

extern void *malloc(size_t size);
extern void free(void *memory);
extern void *memcpy(void *destination, const void *source, size_t count);
extern void *memset(void *destination, int value, size_t count);

extern int BTDN_FileOpen(const char *path, const char *mode);
extern int BTDN_FileRead(int handle, void *buffer, int length);
extern int BTDN_FileWrite(int handle, const void *buffer, int length);
extern int BTDN_FileSeek(int handle, int offset, int origin);
extern int BTDN_FileTell(int handle);
extern int BTDN_FileClose(int handle);
extern int BTDN_FileFlush(int handle);
extern int BTDN_FileExists(const char *path);
extern int BTDN_FileCreateDirectory(const char *path);
extern int BTDN_FileRemove(const char *path);
extern int BTDN_FileRename(const char *old_path, const char *new_path);

extern double BTDN_MathSin(double value);
extern double BTDN_MathCos(double value);
extern double BTDN_MathTan(double value);
extern double BTDN_MathAtan(double value);
extern double BTDN_MathAtan2(double y, double x);
extern double BTDN_MathSqrt(double value);
extern double BTDN_MathPow(double x, double y);
extern double BTDN_MathFloor(double value);
extern double BTDN_MathCeiling(double value);
extern void BTDN_RequestQuit(void);
__declspec(noreturn) extern void BTDN_ExitQuake(int status);

static void *memmove(void *destination, const void *source, size_t count)
{
    unsigned char *dest = (unsigned char *)destination;
    const unsigned char *src = (const unsigned char *)source;

    if (dest < src)
    {
        while (count-- != 0)
            *dest++ = *src++;
    }
    else if (dest > src)
    {
        dest += count;
        src += count;
        while (count-- != 0)
            *--dest = *--src;
    }
    return destination;
}

static int memcmp(const void *left, const void *right, size_t count)
{
    const unsigned char *a = (const unsigned char *)left;
    const unsigned char *b = (const unsigned char *)right;
    while (count-- != 0)
    {
        if (*a != *b)
            return *a - *b;
        ++a;
        ++b;
    }
    return 0;
}

static size_t strlen(const char *text)
{
    const char *end = text;
    while (*end != '\0')
        ++end;
    return (size_t)(end - text);
}

static char *strcpy(char *destination, const char *source)
{
    char *result = destination;
    while ((*destination++ = *source++) != '\0') { }
    return result;
}

static char *strncpy(char *destination, const char *source, size_t count)
{
    char *result = destination;
    while (count != 0 && *source != '\0')
    {
        *destination++ = *source++;
        --count;
    }
    while (count-- != 0)
        *destination++ = '\0';
    return result;
}

static char *strcat(char *destination, const char *source)
{
    return strcpy(destination + strlen(destination), source);
}

static int strcmp(const char *left, const char *right)
{
    while (*left == *right)
    {
        if (*left == '\0')
            return 0;
        ++left;
        ++right;
    }
    return (unsigned char)*left - (unsigned char)*right;
}

static int strncmp(const char *left, const char *right, size_t count)
{
    while (count-- != 0)
    {
        if (*left != *right)
            return (unsigned char)*left - (unsigned char)*right;
        if (*left == '\0')
            return 0;
        ++left;
        ++right;
    }
    return 0;
}

static char *strchr(const char *text, int character)
{
    while (*text != '\0')
    {
        if (*text == (char)character)
            return (char *)text;
        ++text;
    }
    return character == '\0' ? (char *)text : NULL;
}

static char *strrchr(const char *text, int character)
{
    const char *result = NULL;
    do
    {
        if (*text == (char)character)
            result = text;
    }
    while (*text++ != '\0');
    return (char *)result;
}

static char *strstr(const char *haystack, const char *needle)
{
    size_t length = strlen(needle);
    if (length == 0)
        return (char *)haystack;
    while (*haystack != '\0')
    {
        if (strncmp(haystack, needle, length) == 0)
            return (char *)haystack;
        ++haystack;
    }
    return NULL;
}

static int isspace(int character)
{
    return character == ' ' || character == '\t' || character == '\n' ||
           character == '\r' || character == '\f' || character == '\v';
}

static int isdigit(int character)
{
    return character >= '0' && character <= '9';
}

static int isalpha(int character)
{
    return (character >= 'A' && character <= 'Z') ||
           (character >= 'a' && character <= 'z');
}

static int isprint(int character)
{
    return character >= 0x20 && character < 0x7f;
}

static int tolower(int character)
{
    return character >= 'A' && character <= 'Z'
        ? character + ('a' - 'A')
        : character;
}

static int toupper(int character)
{
    return character >= 'a' && character <= 'z'
        ? character - ('a' - 'A')
        : character;
}

static int abs(int value)
{
    return value < 0 ? -value : value;
}

static double fabs(double value)
{
    union { double value; uint64_t bits; } number = { value };
    number.bits &= ~(UINT64_C(1) << 63);
    return number.value;
}

static double sin(double value) { return BTDN_MathSin(value); }
static double cos(double value) { return BTDN_MathCos(value); }
static double tan(double value) { return BTDN_MathTan(value); }
static double atan(double value) { return BTDN_MathAtan(value); }
static double atan2(double y, double x) { return BTDN_MathAtan2(y, x); }
static double sqrt(double value) { return BTDN_MathSqrt(value); }
static double pow(double x, double y) { return BTDN_MathPow(x, y); }
static double floor(double value) { return BTDN_MathFloor(value); }
static double ceil(double value) { return BTDN_MathCeiling(value); }

static int atoi(const char *text)
{
    int sign = 1;
    int value = 0;
    while (isspace((unsigned char)*text))
        ++text;
    if (*text == '-' || *text == '+')
        sign = *text++ == '-' ? -1 : 1;
    while (isdigit((unsigned char)*text))
    {
        int digit = *text++ - '0';
        if (value > (INT_MAX - digit) / 10)
            return sign > 0 ? INT_MAX : INT_MIN;
        value = value * 10 + digit;
    }
    return value * sign;
}

static double atof(const char *text)
{
    double value = 0.0;
    double fraction = 0.1;
    int sign = 1;
    int exponent = 0;
    int exponent_sign = 1;

    while (isspace((unsigned char)*text))
        ++text;
    if (*text == '-' || *text == '+')
        sign = *text++ == '-' ? -1 : 1;
    while (isdigit((unsigned char)*text))
        value = value * 10.0 + (double)(*text++ - '0');
    if (*text == '.')
    {
        ++text;
        while (isdigit((unsigned char)*text))
        {
            value += (double)(*text++ - '0') * fraction;
            fraction *= 0.1;
        }
    }
    if (*text == 'e' || *text == 'E')
    {
        ++text;
        if (*text == '-' || *text == '+')
            exponent_sign = *text++ == '-' ? -1 : 1;
        while (isdigit((unsigned char)*text))
            exponent = exponent * 10 + *text++ - '0';
        while (exponent-- > 0)
            value *= exponent_sign > 0 ? 10.0 : 0.1;
    }
    return value * sign;
}

static void *calloc(size_t count, size_t size)
{
    void *memory;
    if (size != 0 && count > (size_t)-1 / size)
        return NULL;
    memory = malloc(count * size);
    if (memory != NULL)
        memset(memory, 0, count * size);
    return memory;
}

static unsigned int quakegeneric_rand_state = 1;

static void srand(unsigned int seed)
{
    quakegeneric_rand_state = seed;
}

static int rand(void)
{
    quakegeneric_rand_state = quakegeneric_rand_state * 1103515245U + 12345U;
    return (int)((quakegeneric_rand_state >> 16) & 0x7fffU);
}

static FILE *fopen(const char *path, const char *mode)
{
    int handle = BTDN_FileOpen(path, mode);
    FILE *file;
    if (handle <= 0)
        return NULL;

    file = (FILE *)malloc(sizeof(*file));
    if (file == NULL)
    {
        BTDN_FileClose(handle);
        return NULL;
    }
    file->handle = handle;
    file->eof = 0;
    file->error = 0;
    file->ungot = EOF;
    return file;
}

static int fclose(FILE *file)
{
    int result;
    if (file == NULL || file == stdout || file == stderr)
        return EOF;
    result = BTDN_FileClose(file->handle);
    free(file);
    return result;
}

static size_t fread(void *buffer, size_t size, size_t count, FILE *file)
{
    size_t requested;
    int bytes;
    unsigned char *destination = (unsigned char *)buffer;

    if (file == NULL || file == stdout || file == stderr || size == 0 || count == 0 ||
        count > (size_t)-1 / size)
        return 0;
    requested = size * count;
    if (requested > INT_MAX)
        requested = INT_MAX;

    bytes = 0;
    if (file->ungot != EOF && requested != 0)
    {
        *destination++ = (unsigned char)file->ungot;
        file->ungot = EOF;
        ++bytes;
        --requested;
    }
    if (requested != 0)
    {
        int read = BTDN_FileRead(file->handle, destination, (int)requested);
        if (read < 0)
        {
            file->error = 1;
            return 0;
        }
        bytes += read;
        if ((size_t)read < requested)
            file->eof = 1;
    }
    return (size_t)bytes / size;
}

static size_t fwrite(const void *buffer, size_t size, size_t count, FILE *file)
{
    size_t requested;
    int bytes;
    if (file == NULL || file == stdout || file == stderr || size == 0 || count == 0 ||
        count > (size_t)-1 / size)
        return 0;
    requested = size * count;
    if (requested > INT_MAX)
        requested = INT_MAX;
    bytes = BTDN_FileWrite(file->handle, buffer, (int)requested);
    if (bytes < 0)
    {
        file->error = 1;
        return 0;
    }
    return (size_t)bytes / size;
}

static int fseek(FILE *file, long offset, int origin)
{
    if (file == NULL || file == stdout || file == stderr ||
        BTDN_FileSeek(file->handle, offset, origin) < 0)
        return -1;
    file->eof = 0;
    file->ungot = EOF;
    return 0;
}

static long ftell(FILE *file)
{
    long position;
    if (file == NULL || file == stdout || file == stderr)
        return -1;
    position = BTDN_FileTell(file->handle);
    if (position >= 0 && file->ungot != EOF)
        --position;
    return position;
}

static int fflush(FILE *file)
{
    return file == stdout || file == stderr ? 0 :
        (file == NULL ? EOF : BTDN_FileFlush(file->handle));
}

static int fgetc(FILE *file)
{
    unsigned char character;
    int result;
    if (file == NULL || file == stdout || file == stderr)
        return EOF;
    if (file->ungot != EOF)
    {
        result = file->ungot;
        file->ungot = EOF;
        return result;
    }
    result = BTDN_FileRead(file->handle, &character, 1);
    if (result == 1)
        return character;
    if (result == 0)
        file->eof = 1;
    else
        file->error = 1;
    return EOF;
}

static int getc(FILE *file)
{
    return fgetc(file);
}

static int ungetc(int character, FILE *file)
{
    if (file == NULL || character == EOF || file->ungot != EOF)
        return EOF;
    file->ungot = (unsigned char)character;
    file->eof = 0;
    return file->ungot;
}

static int feof(FILE *file)
{
    return file != NULL && file != stdout && file != stderr ? file->eof : 0;
}

static int ferror(FILE *file)
{
    return file != NULL && file != stdout && file != stderr ? file->error : 0;
}

static int vfprintf(FILE *file, const char *format, va_list arguments)
{
    char buffer[4096];
    int length;
    if (file == stdout || file == stderr)
        return vprintf_(format, arguments);
    if (file == NULL)
        return -1;

    length = vsnprintf_(buffer, sizeof(buffer), format, arguments);
    if (length < 0)
        return length;
    if (length >= (int)sizeof(buffer))
        length = (int)sizeof(buffer) - 1;
    return BTDN_FileWrite(file->handle, buffer, length) == length ? length : -1;
}

static int fprintf(FILE *file, const char *format, ...)
{
    int result;
    va_list arguments;
    va_start(arguments, format);
    result = vfprintf(file, format, arguments);
    va_end(arguments);
    return result;
}

static int vsprintf(char *buffer, const char *format, va_list arguments)
{
    return vsnprintf_(buffer, (size_t)-1, format, arguments);
}

static int qg_skip_input_space(FILE *file)
{
    int character;
    do
    {
        character = fgetc(file);
    }
    while (character != EOF && isspace(character));
    if (character != EOF)
        ungetc(character, file);
    return character;
}

static int qg_read_scan_token(FILE *file, char *buffer, int capacity)
{
    int length = 0;
    int character;
    if (qg_skip_input_space(file) == EOF)
        return 0;
    while ((character = fgetc(file)) != EOF && !isspace(character))
    {
        if (length + 1 < capacity)
            buffer[length++] = (char)character;
    }
    if (character != EOF)
        ungetc(character, file);
    buffer[length] = '\0';
    return length;
}

static int fscanf(FILE *file, const char *format, ...)
{
    va_list arguments;
    int assigned = 0;
    int input_ended = 0;
    char token[128];

    va_start(arguments, format);
    while (*format != '\0')
    {
        int character;
        int width = 0;
        if (isspace((unsigned char)*format))
        {
            while (isspace((unsigned char)*format))
                ++format;
            if (qg_skip_input_space(file) == EOF)
                input_ended = 1;
            continue;
        }
        if (*format != '%')
        {
            character = fgetc(file);
            if (character == EOF)
                input_ended = 1;
            if (character != (unsigned char)*format)
            {
                if (character != EOF)
                    ungetc(character, file);
                break;
            }
            ++format;
            continue;
        }

        ++format;
        while (isdigit((unsigned char)*format))
            width = width * 10 + *format++ - '0';

        if (*format == 's')
        {
            char *destination = va_arg(arguments, char *);
            int length = 0;
            int limit = width > 0 ? width : INT_MAX;
            if (qg_skip_input_space(file) == EOF)
            {
                input_ended = 1;
                break;
            }
            while (length < limit && (character = fgetc(file)) != EOF && !isspace(character))
                destination[length++] = (char)character;
            if (character != EOF && isspace(character))
                ungetc(character, file);
            destination[length] = '\0';
            if (length == 0)
                break;
            ++assigned;
        }
        else if (*format == 'f')
        {
            float *destination = va_arg(arguments, float *);
            if (qg_read_scan_token(file, token, sizeof(token)) == 0)
            {
                input_ended = 1;
                break;
            }
            *destination = (float)atof(token);
            ++assigned;
        }
        else if (*format == 'd' || *format == 'i')
        {
            int *destination = va_arg(arguments, int *);
            if (qg_read_scan_token(file, token, sizeof(token)) == 0)
            {
                input_ended = 1;
                break;
            }
            *destination = atoi(token);
            ++assigned;
        }
        else if (*format == 'u')
        {
            unsigned int *destination = va_arg(arguments, unsigned int *);
            if (qg_read_scan_token(file, token, sizeof(token)) == 0)
            {
                input_ended = 1;
                break;
            }
            *destination = (unsigned int)atoi(token);
            ++assigned;
        }
        else if (*format == '%')
        {
            character = fgetc(file);
            if (character != '%')
            {
                if (character == EOF)
                    input_ended = 1;
                else
                    ungetc(character, file);
                break;
            }
        }
        else
        {
            break;
        }
        ++format;
    }
    va_end(arguments);
    return assigned == 0 && input_ended ? EOF : assigned;
}

static int remove(const char *path)
{
    return BTDN_FileRemove(path);
}

static int rename(const char *old_path, const char *new_path)
{
    return BTDN_FileRename(old_path, new_path);
}

static char *strerror(int error)
{
    (void)error;
    return "file operation failed";
}

static __declspec(noreturn) void exit(int status)
{
    BTDN_ExitQuake(status);
}

#endif
