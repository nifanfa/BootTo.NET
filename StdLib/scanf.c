#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>
#include <stdlib.h>

static int space(int character)
{
    return character == ' ' || (character >= '\t' && character <= '\r');
}

static int digit(int character, int base)
{
    int result;
    if (character >= '0' && character <= '9')
        result = character - '0';
    else if (character >= 'a' && character <= 'z')
        result = character - 'a' + 10;
    else if (character >= 'A' && character <= 'Z')
        result = character - 'A' + 10;
    else
        return -1;
    return result < base ? result : -1;
}

static void store_integer(void* destination, uint64_t number, int size)
{
    if (size == 1)
        *(signed char*)destination = (signed char)number;
    else if (size == 2)
        *(short*)destination = (short)number;
    else if (size == 8)
        *(int64_t*)destination = (int64_t)number;
    else
        *(int*)destination = (int)number;
}

int __cdecl __stdio_common_vsscanf(uint64_t options, const char* buffer,
    size_t buffer_count, const char* format, void* locale, va_list args)
{
    const char* cursor = buffer;
    int assignments = 0;
    (void)options;
    (void)buffer_count;
    (void)locale;
    if (!buffer || !format)
        return -1;
    while (*format) {
        int width = 0;
        int suppress = 0;
        int size = 4;
        char conversion;
        if (space((unsigned char)*format)) {
            while (space((unsigned char)*format))
                ++format;
            while (space((unsigned char)*cursor))
                ++cursor;
            continue;
        }
        if (*format != '%') {
            if (*cursor != *format)
                break;
            ++cursor;
            ++format;
            continue;
        }
        ++format;
        if (*format == '%') {
            if (*cursor++ != '%')
                break;
            ++format;
            continue;
        }
        if (*format == '*') {
            suppress = 1;
            ++format;
        }
        while (*format >= '0' && *format <= '9') {
            if (width < 1000000)
                width = width * 10 + *format - '0';
            ++format;
        }
        if (*format == 'h') {
            size = 2;
            ++format;
            if (*format == 'h') {
                size = 1;
                ++format;
            }
        } else if (*format == 'l') {
            ++format;
            if (*format == 'l') {
                size = 8;
                ++format;
            } else
                size = 8;
        } else if (*format == 'z' || *format == 't' || *format == 'j') {
            size = 8;
            ++format;
        } else if (*format == 'I') {
            ++format;
            if (format[0] == '6' && format[1] == '4') {
                size = 8;
                format += 2;
            }
        }
        conversion = *format++;
        if (conversion == 'n') {
            if (!suppress)
                store_integer(va_arg(args, void*), (size_t)(cursor - buffer), size);
            continue;
        }
        if (conversion != 'c' && conversion != '[' && conversion != '%' && conversion != '\0')
            while (space((unsigned char)*cursor))
                ++cursor;
        if (!*cursor && conversion != 'n')
            return assignments ? assignments : -1;
        if (conversion == 's' || conversion == 'c' || conversion == '[') {
            const char* beginning = cursor;
            int negate = 0;
            const char* set_begin = 0;
            const char* set_end = 0;
            char* output = suppress ? 0 : va_arg(args, char*);
            if (conversion == '[') {
                if (*format == '^') {
                    negate = 1;
                    ++format;
                }
                set_begin = format;
                while (*format && *format != ']')
                    ++format;
                set_end = format;
                if (*format)
                    ++format;
            }
            if (!width)
                width = conversion == 'c' ? 1 : 1000000;
            while (*cursor && width > 0) {
                int matches = 0;
                const char* current;
                if (conversion == 's' && space((unsigned char)*cursor))
                    break;
                if (conversion == '[') {
                    for (current = set_begin; current < set_end; ++current)
                        if (*current == *cursor)
                            matches = 1;
                    if (matches == negate)
                        break;
                }
                if (output)
                    *output++ = *cursor;
                ++cursor;
                --width;
            }
            if (cursor == beginning)
                break;
            if (output && conversion != 'c')
                *output = 0;
        } else if (conversion == 'd' || conversion == 'i' || conversion == 'u' ||
                   conversion == 'o' || conversion == 'x' || conversion == 'X' || conversion == 'p') {
            const char* beginning = cursor;
            int base = conversion == 'o' ? 8 : (conversion == 'x' || conversion == 'X' || conversion == 'p' ? 16 : 10);
            int negative = 0;
            int found = 0;
            uint64_t value = 0;
            if (!width)
                width = 1000000;
            if ((*cursor == '-' || *cursor == '+') && width > 0) {
                negative = *cursor == '-';
                ++cursor;
                --width;
            }
            if (conversion == 'i' && *cursor == '0')
                base = cursor[1] == 'x' || cursor[1] == 'X' ? 16 : 8;
            if (base == 16 && cursor[0] == '0' && (cursor[1] == 'x' || cursor[1] == 'X') && width >= 2) {
                cursor += 2;
                width -= 2;
            }
            while (*cursor && width > 0 && digit((unsigned char)*cursor, base) >= 0) {
                value = value * base + digit((unsigned char)*cursor++, base);
                ++found;
                --width;
            }
            if (!found) {
                cursor = beginning;
                break;
            }
            if (!suppress)
                store_integer(va_arg(args, void*), negative ? 0 - value : value,
                    conversion == 'p' ? 8 : size);
        } else if (conversion == 'f' || conversion == 'F' || conversion == 'e' ||
                   conversion == 'E' || conversion == 'g' || conversion == 'G') {
            char number[128];
            char* parsed;
            size_t available = 0;
            double value;
            if (!width || width >= sizeof(number))
                width = sizeof(number) - 1;
            while (cursor[available] && available < (size_t)width && !space((unsigned char)cursor[available])) {
                number[available] = cursor[available];
                ++available;
            }
            number[available] = 0;
            value = strtod(number, &parsed);
            if (parsed == number)
                break;
            cursor += parsed - number;
            if (!suppress) {
                if (size == 8)
                    *va_arg(args, double*) = value;
                else
                    *va_arg(args, float*) = (float)value;
            }
        } else
            break;
        if (!suppress)
            ++assignments;
    }
    return assignments;
}
