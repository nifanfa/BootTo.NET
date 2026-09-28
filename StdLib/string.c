#include <stddef.h>
#include <stdint.h>
#include <string.h>

void* __cdecl memmove(void* destination, const void* source, size_t count)
{
    unsigned char* output = (unsigned char*)destination;
    const unsigned char* input = (const unsigned char*)source;
    size_t index;
    if ((uintptr_t)output - (uintptr_t)input >= count) {
        for (index = 0; index < count; ++index)
            output[index] = input[index];
    } else {
        while (count != 0) {
            --count;
            output[count] = input[count];
        }
    }
    return destination;
}

size_t __cdecl strlen(const char* text)
{
    const char* cursor = text;
    while (*cursor)
        ++cursor;
    return (size_t)(cursor - text);
}

char* __cdecl strcpy(char* destination, const char* source)
{
    char* cursor = destination;
    while ((*cursor++ = *source++) != 0)
        ;
    return destination;
}

char* __cdecl strncpy(char* destination, const char* source, size_t count)
{
    size_t index;
    for (index = 0; index < count && source[index]; ++index)
        destination[index] = source[index];
    for (; index < count; ++index)
        destination[index] = 0;
    return destination;
}

int __cdecl strcmp(const char* left, const char* right)
{
    while (*left && *left == *right) {
        ++left;
        ++right;
    }
    return (unsigned char)*left - (unsigned char)*right;
}

int __cdecl strncmp(const char* left, const char* right, size_t count)
{
    size_t index;
    for (index = 0; index < count; ++index) {
        if (left[index] != right[index] || left[index] == 0)
            return (unsigned char)left[index] - (unsigned char)right[index];
    }
    return 0;
}

char* __cdecl strchr(const char* text, int character)
{
    do {
        if (*text == (char)character)
            return (char*)text;
    } while (*text++);
    return 0;
}

char* __cdecl strrchr(const char* text, int character)
{
    const char* result = 0;
    do {
        if (*text == (char)character)
            result = text;
    } while (*text++);
    return (char*)result;
}

char* __cdecl strstr(const char* text, const char* needle)
{
    const char* beginning;
    const char* match;
    if (!*needle)
        return (char*)text;
    for (; *text; ++text) {
        beginning = text;
        match = needle;
        while (*beginning && *match && *beginning == *match) {
            ++beginning;
            ++match;
        }
        if (!*match)
            return (char*)text;
    }
    return 0;
}

char* __cdecl strncat(char* destination, const char* source, size_t count)
{
    char* cursor = destination + strlen(destination);
    size_t index;
    for (index = 0; index < count && source[index]; ++index)
        cursor[index] = source[index];
    cursor[index] = 0;
    return destination;
}

char* __cdecl strtok(char* text, const char* delimiters)
{
    static char* next;
    char* start = text ? text : next;
    if (!start)
        return 0;
    while (*start && strchr(delimiters, *start))
        ++start;
    if (!*start) {
        next = 0;
        return 0;
    }
    next = start;
    while (*next && !strchr(delimiters, *next))
        ++next;
    if (*next)
        *next++ = 0;
    else
        next = 0;
    return start;
}
