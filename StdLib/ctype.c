#include <ctype.h>

int __cdecl tolower(int character)
{
    return character >= 'A' && character <= 'Z' ? character + ('a' - 'A') : character;
}

int __cdecl toupper(int character)
{
    return character >= 'a' && character <= 'z' ? character - ('a' - 'A') : character;
}

int __cdecl islower(int character)
{
    return character >= 'a' && character <= 'z';
}

int __cdecl _stricmp(const char* left, const char* right)
{
    while (*left && tolower((unsigned char)*left) == tolower((unsigned char)*right)) {
        ++left;
        ++right;
    }
    return tolower((unsigned char)*left) - tolower((unsigned char)*right);
}

char* __cdecl strlwr(char* text)
{
    char* cursor = text;
    while (*cursor) {
        *cursor = (char)tolower((unsigned char)*cursor);
        ++cursor;
    }
    return text;
}

char* __cdecl strupr(char* text)
{
    char* cursor = text;
    while (*cursor) {
        *cursor = (char)toupper((unsigned char)*cursor);
        ++cursor;
    }
    return text;
}
