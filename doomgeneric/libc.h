#ifndef DOOMGENERIC_LIBC_H
#define DOOMGENERIC_LIBC_H

// Minimal hosted C surface for the UEFI build.  Keep standard names so Doom
// sources remain portable; backing file I/O is supplied by doomgeneric.cs.

#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>

#include "../NativeLib/printf.h"

#ifndef NULL
#define NULL ((void *) 0)
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

typedef struct doomgeneric_file_s
{
    int handle;
} FILE;

#define stdout ((FILE *) 1)
#define stderr ((FILE *) 2)

extern void *malloc(size_t size);
extern void free(void *ptr);
extern void *memcpy(void *dest, const void *src, size_t count);
extern void *memset(void *dest, int value, size_t count);
extern int BTDN_FileOpen(const char *path, const char *mode);
extern int BTDN_FileRead(int handle, void *buffer, int length);
extern int BTDN_FileWrite(int handle, const void *buffer, int length);
extern int BTDN_FileSeek(int handle, int offset, int origin);
extern int BTDN_FileTell(int handle);
extern int BTDN_FileClose(int handle);
extern int BTDN_FileFlush(int handle);
extern int BTDN_FileCreateDirectory(const char *path);
extern int BTDN_FileRemove(const char *path);
extern int BTDN_FileRename(const char *old_path, const char *new_path);

static void *memmove(void *dest, const void *src, size_t count)
{
    unsigned char *d = (unsigned char *) dest;
    const unsigned char *s = (const unsigned char *) src;

    if (d < s)
    {
        while (count-- != 0)
            *d++ = *s++;
    }
    else if (d > s)
    {
        d += count;
        s += count;
        while (count-- != 0)
            *--d = *--s;
    }

    return dest;
}

static int memcmp(const void *left, const void *right, size_t count)
{
    const unsigned char *a = (const unsigned char *) left;
    const unsigned char *b = (const unsigned char *) right;

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
    const char *p = text;
    while (*p != '\0')
        ++p;
    return (size_t) (p - text);
}

static char *strcpy(char *dest, const char *src)
{
    char *result = dest;
    while ((*dest++ = *src++) != '\0') { }
    return result;
}

static char *strncpy(char *dest, const char *src, size_t count)
{
    char *result = dest;
    while (count != 0 && *src != '\0')
    {
        *dest++ = *src++;
        --count;
    }
    while (count-- != 0)
        *dest++ = '\0';
    return result;
}

static char *strcat(char *dest, const char *src)
{
    return strcpy(dest + strlen(dest), src);
}

static char *strncat(char *dest, const char *src, size_t count)
{
    char *p = dest + strlen(dest);
    while (count-- != 0 && *src != '\0')
        *p++ = *src++;
    *p = '\0';
    return dest;
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
    return (unsigned char) *left - (unsigned char) *right;
}

static int strncmp(const char *left, const char *right, size_t count)
{
    while (count-- != 0)
    {
        if (*left != *right)
            return (unsigned char) *left - (unsigned char) *right;
        if (*left == '\0')
            return 0;
        ++left;
        ++right;
    }
    return 0;
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

static int isalpha(int character)
{
    return (character >= 'A' && character <= 'Z') ||
           (character >= 'a' && character <= 'z');
}

static int isdigit(int character)
{
    return character >= '0' && character <= '9';
}

static int isupper(int character)
{
    return character >= 'A' && character <= 'Z';
}

static int isspace(int character)
{
    return character == ' ' || character == '\t' || character == '\n' ||
           character == '\r' || character == '\f' || character == '\v';
}

static int isprint(int character)
{
    return character >= 0x20 && character < 0x7f;
}

static char *strchr(const char *text, int character)
{
    while (*text != '\0')
    {
        if (*text == (char) character)
            return (char *) text;
        ++text;
    }
    return character == '\0' ? (char *) text : NULL;
}

static char *strrchr(const char *text, int character)
{
    const char *result = NULL;
    do
    {
        if (*text == (char) character)
            result = text;
    }
    while (*text++ != '\0');
    return (char *) result;
}

static char *strstr(const char *haystack, const char *needle)
{
    size_t needle_length = strlen(needle);
    if (needle_length == 0)
        return (char *) haystack;
    while (*haystack != '\0')
    {
        if (strncmp(haystack, needle, needle_length) == 0)
            return (char *) haystack;
        ++haystack;
    }
    return NULL;
}

static char *strdup(const char *text)
{
    size_t size = strlen(text) + 1;
    char *copy = (char *) malloc(size);
    if (copy != NULL)
        memcpy(copy, text, size);
    return copy;
}

static int strcasecmp(const char *left, const char *right)
{
    while (tolower((unsigned char) *left) == tolower((unsigned char) *right))
    {
        if (*left == '\0')
            return 0;
        ++left;
        ++right;
    }
    return tolower((unsigned char) *left) - tolower((unsigned char) *right);
}

static int strncasecmp(const char *left, const char *right, size_t count)
{
    while (count-- != 0)
    {
        int difference = tolower((unsigned char) *left) - tolower((unsigned char) *right);
        if (difference != 0 || *left == '\0')
            return difference;
        ++left;
        ++right;
    }
    return 0;
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

static int atoi(const char *text)
{
    int sign = 1;
    int value = 0;

    while (isspace((unsigned char) *text))
        ++text;
    if (*text == '-' || *text == '+')
        sign = *text++ == '-' ? -1 : 1;
    while (isdigit((unsigned char) *text))
    {
        if (value > INT_MAX / 10 ||
            (value == INT_MAX / 10 && *text - '0' > 7))
            return sign > 0 ? INT_MAX : INT_MIN;
        value = value * 10 + *text++ - '0';
    }
    return value * sign;
}

static unsigned long strtoul(const char *text, char **end, int base)
{
    unsigned long value = 0;
    int negative = 0;
    int digit;

    while (isspace((unsigned char) *text))
        ++text;
    if (*text == '-' || *text == '+')
        negative = *text++ == '-';
    if ((base == 0 || base == 16) && text[0] == '0' &&
        (text[1] == 'x' || text[1] == 'X'))
    {
        base = 16;
        text += 2;
    }
    if (base == 0)
        base = *text == '0' ? 8 : 10;
    while (*text != '\0')
    {
        if (isdigit((unsigned char) *text))
            digit = *text - '0';
        else if (isalpha((unsigned char) *text))
            digit = tolower((unsigned char) *text) - 'a' + 10;
        else
            break;
        if (digit >= base)
            break;
        if (value > ((unsigned long) -1 - (unsigned long) digit) / (unsigned long) base)
            value = (unsigned long) -1;
        else
            value = value * (unsigned long) base + (unsigned long) digit;
        ++text;
    }
    if (end != NULL)
        *end = (char *) text;
    return negative ? ~value + 1 : value;
}

static double atof(const char *text)
{
    double value = 0.0;
    double fraction = 0.1;
    int sign = 1;

    while (isspace((unsigned char) *text))
        ++text;
    if (*text == '-' || *text == '+')
        sign = *text++ == '-' ? -1 : 1;
    while (isdigit((unsigned char) *text))
        value = value * 10.0 + (double) (*text++ - '0');
    if (*text == '.')
    {
        ++text;
        while (isdigit((unsigned char) *text))
        {
            value += (double) (*text++ - '0') * fraction;
            fraction *= 0.1;
        }
    }
    return value * sign;
}

static void *calloc(size_t count, size_t size)
{
    void *memory;
    if (size != 0 && count > (size_t) -1 / size)
        return NULL;
    memory = malloc(count * size);
    if (memory != NULL)
        memset(memory, 0, count * size);
    return memory;
}

// Doom only reallocates its optional checksum bookkeeping.  Preserve the
// allocation contract without depending on the Visual C runtime allocator.
static void *realloc(void *memory, size_t size)
{
    void *replacement = malloc(size);
    if (replacement != NULL && memory != NULL)
        free(memory);
    return replacement;
}

static FILE *fopen(const char *path, const char *mode)
{
    int handle = BTDN_FileOpen(path, mode);
    FILE *file;

    if (handle <= 0)
        return NULL;
    file = (FILE *) malloc(sizeof(*file));
    if (file == NULL)
    {
        BTDN_FileClose(handle);
        return NULL;
    }
    file->handle = handle;
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
    int bytes;
    size_t requested;
    if (file == NULL || size == 0 || count == 0 || count > (size_t) -1 / size)
        return 0;
    requested = size * count;
    if (requested > INT_MAX)
        requested = INT_MAX;
    bytes = BTDN_FileRead(file->handle, buffer, (int) requested);
    return bytes <= 0 ? 0 : (size_t) bytes / size;
}

static size_t fwrite(const void *buffer, size_t size, size_t count, FILE *file)
{
    int bytes;
    size_t requested;
    if (file == NULL || size == 0 || count == 0 || count > (size_t) -1 / size)
        return 0;
    requested = size * count;
    if (requested > INT_MAX)
        requested = INT_MAX;
    bytes = BTDN_FileWrite(file->handle, buffer, (int) requested);
    return bytes <= 0 ? 0 : (size_t) bytes / size;
}

static int fseek(FILE *file, long offset, int origin)
{
    return file == NULL || BTDN_FileSeek(file->handle, offset, origin) < 0 ? -1 : 0;
}

static long ftell(FILE *file)
{
    return file == NULL ? -1 : BTDN_FileTell(file->handle);
}

static int fflush(FILE *file)
{
    return file == stdout || file == stderr ? 0 :
        (file == NULL ? EOF : BTDN_FileFlush(file->handle));
}

static int putchar(int character)
{
    _putchar((char) character);
    return (unsigned char) character;
}

static int puts(const char *text)
{
    while (*text != '\0')
        _putchar(*text++);
    _putchar('\n');
    return 0;
}

static int vfprintf(FILE *file, const char *format, va_list arguments)
{
    (void) file;
    return vprintf_(format, arguments);
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

static char *fgets(char *buffer, int count, FILE *file)
{
    int index = 0;
    if (buffer == NULL || file == NULL || count <= 1)
        return NULL;
    while (index < count - 1)
    {
        if (BTDN_FileRead(file->handle, buffer + index, 1) != 1)
            break;
        if (buffer[index++] == '\n')
            break;
    }
    if (index == 0)
        return NULL;
    buffer[index] = '\0';
    return buffer;
}

static int sscanf(const char *text, const char *format, ...)
{
    va_list arguments;
    int *integer;
    unsigned int *unsigned_integer;
    int base = 10;
    char *end;

    while (isspace((unsigned char) *format))
        ++format;
    while (isspace((unsigned char) *text))
        ++text;
    if (*format++ != '%')
        return 0;
    if (*format == 'i')
        base = 0;
    else if (*format == 'x' || *format == 'X')
        base = 16;
    else if (*format == 'o')
        base = 8;
    else if (*format != 'd')
        return 0;
    va_start(arguments, format);
    if (*format == 'x' || *format == 'X' || *format == 'o')
    {
        unsigned_integer = va_arg(arguments, unsigned int *);
        *unsigned_integer = (unsigned int) strtoul(text, &end, base);
    }
    else
    {
        integer = va_arg(arguments, int *);
        *integer = (int) strtoul(text, &end, base);
    }
    va_end(arguments);
    return end == text ? 0 : 1;
}

static int mkdir(const char *path)
{
    return BTDN_FileCreateDirectory(path);
}

static int remove(const char *path)
{
    return BTDN_FileRemove(path);
}

static int rename(const char *old_path, const char *new_path)
{
    return BTDN_FileRename(old_path, new_path);
}

static char *getenv(const char *name)
{
    (void) name;
    return NULL;
}

static void exit(int status)
{
    (void) status;
}

static int system(const char *command)
{
    (void) command;
    return -1;
}

static int fileno(FILE *file)
{
    return file == NULL ? -1 : file->handle;
}

static int isatty(int handle)
{
    (void) handle;
    return 0;
}

#endif
