#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

extern int BTDN_FileOpen(const char* path, const char* mode);
extern int BTDN_FileRead(int handle, void* buffer, int length);
extern int BTDN_FileWrite(int handle, const void* buffer, int length);
extern int BTDN_FileSeek(int handle, int offset, int origin);
extern int BTDN_FileTell(int handle);
extern int BTDN_FileClose(int handle);
extern int BTDN_FileFlush(int handle);
extern int BTDN_FileRename(const char* old_path, const char* new_path);
extern int BTDN_DirectoryExists(const char* path);
extern int vsnprintf_(char* buffer, size_t count, const char* format, va_list args);
extern void _putchar(char character);

typedef struct stream_state {
    int handle;
    int end_of_file;
    int error;
    int pushed_back;
    size_t read_position;
    size_t read_size;
    unsigned char read_buffer[4096];
} stream_state;

static stream_state standard_streams[3];
static char current_directory[512] = "\\";

static stream_state* state(FILE* stream)
{
    return (stream_state*)stream;
}

static int resolve_path(const char* path, char* resolved, size_t capacity)
{
    size_t length = 0;
    if (!path || !*path)
        return -1;
    if (*path != '\\' && *path != '/') {
        while (current_directory[length] && length + 1 < capacity) {
            resolved[length] = current_directory[length];
            ++length;
        }
    }
    while (*path && length + 1 < capacity) {
        resolved[length++] = *path == '/' ? '\\' : *path;
        ++path;
    }
    if (*path || !length)
        return -1;
    resolved[length] = 0;
    return 0;
}

FILE* __cdecl fopen(const char* path, const char* mode)
{
    char resolved[512];
    stream_state* stream;
    int handle;
    if (!mode || resolve_path(path, resolved, sizeof(resolved)))
        return 0;
    handle = BTDN_FileOpen(resolved, mode);
    if (!handle)
        return 0;
    stream = (stream_state*)calloc(1, sizeof(*stream));
    if (!stream) {
        BTDN_FileClose(handle);
        return 0;
    }
    stream->handle = handle;
    stream->pushed_back = -1;
    return (FILE*)stream;
}

int __cdecl fclose(FILE* file)
{
    stream_state* stream = state(file);
    int result;
    if (!stream || stream >= standard_streams && stream < standard_streams + 3)
        return EOF;
    result = BTDN_FileClose(stream->handle);
    free(stream);
    return result;
}

size_t __cdecl fread(void* buffer, size_t size, size_t count, FILE* file)
{
    stream_state* stream = state(file);
    size_t total;
    size_t received = 0;
    size_t available;
    int read;
    if (!stream || !size || !count || count > INT32_MAX / size || !buffer || stream->handle == 0)
        return 0;
    total = size * count;
    if (stream->pushed_back != -1) {
        *(unsigned char*)buffer = (unsigned char)stream->pushed_back;
        stream->pushed_back = -1;
        received = 1;
    }
    while (received < total) {
        available = stream->read_size - stream->read_position;
        if (available) {
            size_t copied = total - received < available ? total - received : available;
            memcpy((unsigned char*)buffer + received, stream->read_buffer + stream->read_position, copied);
            stream->read_position += copied;
            received += copied;
            continue;
        }
        if (total - received >= sizeof(stream->read_buffer)) {
            read = BTDN_FileRead(stream->handle, (unsigned char*)buffer + received, (int)(total - received));
            if (read < 0) {
                stream->error = 1;
                break;
            }
            received += read;
            if (read == 0)
                break;
        } else {
            read = BTDN_FileRead(stream->handle, stream->read_buffer, sizeof(stream->read_buffer));
            if (read < 0) {
                stream->error = 1;
                break;
            }
            stream->read_position = 0;
            stream->read_size = read;
            if (read == 0)
                break;
        }
    }
    if (!stream->error)
        stream->end_of_file = received < total;
    return received / size;
}

size_t __cdecl fwrite(const void* buffer, size_t size, size_t count, FILE* file)
{
    stream_state* stream = state(file);
    size_t total;
    int written;
    if (!stream || !size || !count || count > INT32_MAX / size || !buffer)
        return 0;
    total = size * count;
    if (stream->handle == 0) {
        const unsigned char* bytes = (const unsigned char*)buffer;
        size_t index;
        for (index = 0; index < total; ++index)
            _putchar((char)bytes[index]);
        return count;
    }
    if (stream->read_size != stream->read_position || stream->pushed_back != -1) {
        int unread = (int)(stream->read_size - stream->read_position) + (stream->pushed_back != -1);
        if (BTDN_FileSeek(stream->handle, -unread, SEEK_CUR) < 0) {
            stream->error = 1;
            return 0;
        }
    }
    stream->read_position = stream->read_size = 0;
    stream->pushed_back = -1;
    written = BTDN_FileWrite(stream->handle, buffer, (int)total);
    if (written < 0)
        stream->error = 1;
    return written < 0 ? 0 : (size_t)written / size;
}

int __cdecl fseek(FILE* file, long offset, int origin)
{
    stream_state* stream = state(file);
    int64_t adjusted = offset;
    if (!stream || !stream->handle)
        return -1;
    if (origin == SEEK_CUR)
        adjusted -= (int64_t)(stream->read_size - stream->read_position) + (stream->pushed_back != -1);
    if (adjusted < INT32_MIN || adjusted > INT32_MAX || BTDN_FileSeek(stream->handle, (int)adjusted, origin) < 0) {
        stream->error = 1;
        return -1;
    }
    stream->read_position = stream->read_size = 0;
    stream->end_of_file = 0;
    stream->pushed_back = -1;
    return 0;
}

long __cdecl ftell(FILE* file)
{
    stream_state* stream = state(file);
    int position = stream && stream->handle ? BTDN_FileTell(stream->handle) : -1;
    return position < 0 ? -1 : position - (long)(stream->read_size - stream->read_position) - (stream->pushed_back != -1);
}

int __cdecl feof(FILE* file)
{
    stream_state* stream = state(file);
    return stream ? stream->end_of_file : 0;
}

int __cdecl fflush(FILE* file)
{
    stream_state* stream = state(file);
    return !stream || !stream->handle ? 0 : BTDN_FileFlush(stream->handle);
}

int __cdecl fgetc(FILE* file)
{
    unsigned char character;
    return fread(&character, 1, 1, file) == 1 ? character : EOF;
}

int __cdecl fputc(int character, FILE* file)
{
    unsigned char byte = (unsigned char)character;
    return fwrite(&byte, 1, 1, file) == 1 ? byte : EOF;
}

int __cdecl ungetc(int character, FILE* file)
{
    stream_state* stream = state(file);
    if (!stream || character == EOF || stream->pushed_back != -1)
        return EOF;
    stream->end_of_file = 0;
    stream->pushed_back = (unsigned char)character;
    return stream->pushed_back;
}

FILE* __cdecl __acrt_iob_func(unsigned index)
{
    return index < 3 ? (FILE*)&standard_streams[index] : 0;
}

int __cdecl rename(const char* old_path, const char* new_path)
{
    char old_resolved[512];
    char new_resolved[512];
    if (resolve_path(old_path, old_resolved, sizeof(old_resolved)) ||
        resolve_path(new_path, new_resolved, sizeof(new_resolved)))
        return -1;
    return BTDN_FileRename(old_resolved, new_resolved);
}

int __cdecl chdir(const char* path)
{
    char resolved[512];
    size_t length;
    if (!path || strcmp(path, ".") == 0)
        return path ? 0 : -1;
    if (resolve_path(path, resolved, sizeof(resolved)))
        return -1;
    length = strlen(resolved);
    if (resolved[length - 1] != '\\') {
        if (length + 1 >= sizeof(resolved))
            return -1;
        resolved[length++] = '\\';
        resolved[length] = 0;
    }
    if (!BTDN_DirectoryExists(resolved))
        return -1;
    memcpy(current_directory, resolved, length + 1);
    return 0;
}

char* __cdecl _getcwd(char* buffer, int capacity)
{
    size_t length = strlen(current_directory);
    if (length > 1 && current_directory[length - 1] == '\\')
        --length;
    if (!buffer) {
        buffer = (char*)malloc(length + 1);
        capacity = (int)length + 1;
    }
    if (!buffer || capacity <= 0 || length + 1 > (size_t)capacity)
        return 0;
    memcpy(buffer, current_directory, length);
    buffer[length] = 0;
    return buffer;
}

int BTDN_ChangeDirectory(const char* path) { return chdir(path); }

int __cdecl __stdio_common_vsprintf(uint64_t options, char* buffer, size_t capacity,
    const char* format, void* locale, va_list args)
{
    (void)options;
    (void)locale;
    return vsnprintf_(buffer, capacity, format, args);
}

int __cdecl __stdio_common_vfprintf(uint64_t options, FILE* file, const char* format,
    void* locale, va_list args)
{
    char* buffer;
    int required;
    int result;
    va_list copied;
    (void)options;
    (void)locale;
    copied = args;
    required = vsnprintf_(0, 0, format, copied);
    if (required < 0 || required == INT32_MAX)
        return -1;
    buffer = (char*)malloc((size_t)required + 1);
    if (!buffer)
        return -1;
    copied = args;
    vsnprintf_(buffer, (size_t)required + 1, format, copied);
    result = fwrite(buffer, 1, required, file) == (size_t)required ? required : -1;
    free(buffer);
    return result;
}
