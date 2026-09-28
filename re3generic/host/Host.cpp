#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <mmsystem.h>

#include "re3generic.h"
#include "re3generic_game.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static HWND game_window;
static RG_InputEvent input_queue[256];
static unsigned input_head;
static unsigned input_tail;
static uint32_t game_width;
static uint32_t game_height;
static int mouse_x;
static int mouse_y;
static int mouse_delta_x;
static int mouse_delta_y;
static bool mouse_moved;
static FILE *pcm_file;
static HWAVEOUT audio_device;
static struct {
    WAVEHDR header;
    int16_t samples[512 * 2];
    bool prepared;
    bool queued;
} audio_blocks[8];
static unsigned next_audio_block;
static bool audio_reported;

static void push_input(RG_InputType type, int code, int value = 0, int value_y = 0)
{
    unsigned next = (input_tail + 1) % 256;
    if (next != input_head) {
        input_queue[input_tail] = { type, code, value, value_y };
        input_tail = next;
    }
}

static void clip_mouse(HWND window)
{
    RECT bounds;
    if (GetClientRect(window, &bounds)) {
        POINT top_left = { bounds.left, bounds.top };
        POINT bottom_right = { bounds.right, bounds.bottom };
        ClientToScreen(window, &top_left);
        ClientToScreen(window, &bottom_right);
        bounds = { top_left.x, top_left.y, bottom_right.x, bottom_right.y };
        ClipCursor(&bounds);
    }
}

static void move_mouse(HWND window, LPARAM position)
{
    RECT bounds;
    GetClientRect(window, &bounds);
    int client_width = bounds.right;
    int client_height = bounds.bottom;
    if (client_width <= 0 || client_height <= 0)
        return;
    int target_width = client_width;
    int target_height = client_height;
    if ((int64_t)target_width * game_height > (int64_t)target_height * game_width)
        target_width = MulDiv(target_height, game_width, game_height);
    else
        target_height = MulDiv(target_width, game_height, game_width);
    if (!target_width || !target_height)
        return;
    int x = (short)LOWORD(position) - (client_width - target_width) / 2;
    int y = (short)HIWORD(position) - (client_height - target_height) / 2;
    x = x < 0 ? 0 : x > target_width ? target_width : x;
    y = y < 0 ? 0 : y > target_height ? target_height : y;
    mouse_x = MulDiv(x, game_width, target_width);
    mouse_y = MulDiv(y, game_height, target_height);
    mouse_moved = true;
}

static void report_audio_error(const char *operation, MMRESULT result)
{
    char message[MAXERRORLENGTH] = {};
    if (waveOutGetErrorTextA(result, message, sizeof(message)) != MMSYSERR_NOERROR)
        snprintf(message, sizeof(message), "error %u", result);
    fprintf(stderr, "Audio %s: %s\n", operation, message);
}

static void push_key(RG_InputType type, WPARAM virtual_key, LPARAM key_info)
{
    unsigned next = (input_tail + 1) % 256;
    if (next == input_head)
        return;

    int code = (int)virtual_key;
    switch (virtual_key) {
    case VK_ESCAPE: code = RG_KEY_ESCAPE; break;
    case VK_UP: code = RG_KEY_UP; break;
    case VK_DOWN: code = RG_KEY_DOWN; break;
    case VK_LEFT: code = RG_KEY_LEFT; break;
    case VK_RIGHT: code = RG_KEY_RIGHT; break;
    case VK_RETURN: code = RG_KEY_ENTER; break;
    case VK_BACK: code = RG_KEY_BACKSPACE; break;
    case VK_TAB: code = RG_KEY_TAB; break;
    case VK_SHIFT:
        code = MapVirtualKeyA((key_info >> 16) & 0xFF, MAPVK_VSC_TO_VK_EX) == VK_RSHIFT
            ? RG_KEY_RIGHT_SHIFT : RG_KEY_LEFT_SHIFT;
        break;
    case VK_CONTROL: code = (key_info & 0x1000000) ? RG_KEY_RIGHT_CTRL : RG_KEY_LEFT_CTRL; break;
    case VK_MENU: code = (key_info & 0x1000000) ? RG_KEY_RIGHT_ALT : RG_KEY_LEFT_ALT; break;
    case VK_OEM_1: code = ';'; break;
    case VK_OEM_PLUS: code = '='; break;
    case VK_OEM_COMMA: code = ','; break;
    case VK_OEM_MINUS: code = '-'; break;
    case VK_OEM_PERIOD: code = '.'; break;
    case VK_OEM_2: code = '/'; break;
    case VK_OEM_4: code = '['; break;
    case VK_OEM_5: code = '\\'; break;
    case VK_OEM_6: code = ']'; break;
    case VK_OEM_7: code = '\''; break;
    default: break;
    }
    if (virtual_key >= VK_F1 && virtual_key <= VK_F12)
        code = RG_KEY_F1 + (int)(virtual_key - VK_F1);
    if (code > 255 && (code < RG_KEY_ESCAPE || code > RG_KEY_RIGHT_ALT))
        return;
    input_queue[input_tail] = { type, code, 0, 0 };
    input_tail = next;
}

static LRESULT CALLBACK window_proc(HWND window, UINT message, WPARAM wparam, LPARAM lparam)
{
    switch (message) {
    case WM_CREATE: {
        RAWINPUTDEVICE mouse = { 0x01, 0x02, 0, window };
        RegisterRawInputDevices(&mouse, 1, sizeof(mouse));
        return 0;
    }
    case WM_INPUT: {
        RAWINPUT input;
        UINT size = sizeof(input);
        if (GetRawInputData((HRAWINPUT)lparam, RID_INPUT, &input, &size, sizeof(RAWINPUTHEADER)) != (UINT)-1 &&
            input.header.dwType == RIM_TYPEMOUSE && !(input.data.mouse.usFlags & MOUSE_MOVE_ABSOLUTE) &&
            (input.data.mouse.lLastX || input.data.mouse.lLastY)) {
            mouse_delta_x += input.data.mouse.lLastX;
            mouse_delta_y -= input.data.mouse.lLastY;
        }
        return DefWindowProcA(window, message, wparam, lparam);
    }
    case WM_MOUSEMOVE: move_mouse(window, lparam); return 0;
    case WM_LBUTTONDOWN: push_input(RG_INPUT_MOUSE_BUTTON_DOWN, RG_MOUSE_LEFT); return 0;
    case WM_LBUTTONUP: push_input(RG_INPUT_MOUSE_BUTTON_UP, RG_MOUSE_LEFT); return 0;
    case WM_RBUTTONDOWN: push_input(RG_INPUT_MOUSE_BUTTON_DOWN, RG_MOUSE_RIGHT); return 0;
    case WM_RBUTTONUP: push_input(RG_INPUT_MOUSE_BUTTON_UP, RG_MOUSE_RIGHT); return 0;
    case WM_MBUTTONDOWN: push_input(RG_INPUT_MOUSE_BUTTON_DOWN, RG_MOUSE_MIDDLE); return 0;
    case WM_MBUTTONUP: push_input(RG_INPUT_MOUSE_BUTTON_UP, RG_MOUSE_MIDDLE); return 0;
    case WM_XBUTTONDOWN:
    case WM_XBUTTONUP:
        push_input(message == WM_XBUTTONDOWN ? RG_INPUT_MOUSE_BUTTON_DOWN : RG_INPUT_MOUSE_BUTTON_UP,
                   HIWORD(wparam) == XBUTTON1 ? RG_MOUSE_X1 : RG_MOUSE_X2);
        return TRUE;
    case WM_MOUSEWHEEL: push_input(RG_INPUT_MOUSE_WHEEL, 0, (short)HIWORD(wparam)); return 0;
    case WM_SETFOCUS: clip_mouse(window); return 0;
    case WM_KILLFOCUS:
        ClipCursor(NULL);
        input_head = input_tail = 0;
        mouse_moved = false;
        mouse_delta_x = mouse_delta_y = 0;
        push_input(RG_INPUT_MOUSE_RESET, 0);
        return 0;
    case WM_MOVE:
    case WM_SIZE:
        if (GetFocus() == window) clip_mouse(window);
        break;
    case WM_SETCURSOR:
        if (LOWORD(lparam) == HTCLIENT) { SetCursor(NULL); return TRUE; }
        break;
    case WM_KEYDOWN:
    case WM_SYSKEYDOWN: push_key(RG_INPUT_KEY_DOWN, wparam, lparam); return 0;
    case WM_KEYUP:
    case WM_SYSKEYUP: push_key(RG_INPUT_KEY_UP, wparam, lparam); return 0;
    case WM_CLOSE: DestroyWindow(window); return 0;
    case WM_DESTROY: ClipCursor(NULL); game_window = NULL; PostQuitMessage(0); return 0;
    default: break;
    }
    return DefWindowProcA(window, message, wparam, lparam);
}

static int poll_input(void *, RG_InputEvent *event)
{
    if (input_head != input_tail) {
        *event = input_queue[input_head];
        input_head = (input_head + 1) % 256;
        return 1;
    }
    if (mouse_moved) {
        *event = { RG_INPUT_MOUSE_MOVE, 0, mouse_x, mouse_y };
        mouse_moved = false;
        return 1;
    }
    if (mouse_delta_x || mouse_delta_y) {
        *event = { RG_INPUT_MOUSE_DELTA, 0, mouse_delta_x, mouse_delta_y };
        mouse_delta_x = mouse_delta_y = 0;
        return 1;
    }
    return 0;
}

static uint64_t ticks_ms(void *) { return GetTickCount64(); }

static void present(void *, const void *pixels, uint32_t width, uint32_t height,
                    uint32_t pitch, RG_PixelFormat format)
{
    if (!game_window || format != RG_PIXEL_RGB565 || pitch != width * 2)
        return;
    struct {
        BITMAPINFOHEADER header;
        DWORD masks[3];
    } bitmap = {};
    bitmap.header.biSize = sizeof(BITMAPINFOHEADER);
    bitmap.header.biWidth = width;
    bitmap.header.biHeight = (LONG)height;
    bitmap.header.biPlanes = 1;
    bitmap.header.biBitCount = 16;
    bitmap.header.biCompression = BI_BITFIELDS;
    bitmap.masks[0] = 0xF800;
    bitmap.masks[1] = 0x07E0;
    bitmap.masks[2] = 0x001F;
    RECT bounds;
    GetClientRect(game_window, &bounds);
    int target_width = bounds.right;
    int target_height = bounds.bottom;
    if ((int64_t)target_width * height > (int64_t)target_height * width)
        target_width = MulDiv(target_height, width, height);
    else
        target_height = MulDiv(target_width, height, width);
    int left = (bounds.right - target_width) / 2;
    int top = (bounds.bottom - target_height) / 2;
    HDC device = GetDC(game_window);
    if (left || top || target_width != bounds.right || target_height != bounds.bottom)
        FillRect(device, &bounds, (HBRUSH)GetStockObject(BLACK_BRUSH));
    SetStretchBltMode(device, COLORONCOLOR);
    StretchDIBits(device, left, top, target_width, target_height,
                  0, 0, width, height, pixels, (BITMAPINFO *)&bitmap,
                  DIB_RGB_COLORS, SRCCOPY);
    ReleaseDC(game_window, device);
}

static void *file_open(void *, const char *path) { return fopen(path, "rb"); }

static uint64_t file_size(void *, void *handle)
{
    FILE *file = (FILE *)handle;
    __int64 original = _ftelli64(file);
    _fseeki64(file, 0, SEEK_END);
    __int64 size = _ftelli64(file);
    _fseeki64(file, original, SEEK_SET);
    return size < 0 ? 0 : (uint64_t)size;
}

static size_t file_read_at(void *, void *handle, uint64_t offset,
                           void *buffer, size_t bytes)
{
    FILE *file = (FILE *)handle;
    if (_fseeki64(file, offset, SEEK_SET) != 0)
        return 0;
    return fread(buffer, 1, bytes, file);
}

static void file_close(void *, void *handle) { fclose((FILE *)handle); }

static void submit_pcm(void *, const int16_t *samples, uint32_t frames, uint32_t)
{
    if (pcm_file)
        fwrite(samples, sizeof(int16_t) * 2, frames, pcm_file);
    if (!audio_device)
        return;
    while (frames) {
        unsigned block = next_audio_block;
        unsigned searched = 0;
        while (searched < 8 && (!audio_blocks[block].prepared ||
               (audio_blocks[block].queued &&
                !(audio_blocks[block].header.dwFlags & WHDR_DONE)))) {
            block = (block + 1) % 8;
            ++searched;
        }
        if (searched == 8)
            return;
        uint32_t count = frames < 512 ? frames : 512;
        memcpy(audio_blocks[block].samples, samples, count * 2 * sizeof(int16_t));
        audio_blocks[block].header.dwBufferLength = count * 2 * sizeof(int16_t);
        MMRESULT result = waveOutWrite(audio_device, &audio_blocks[block].header,
                                       sizeof(WAVEHDR));
        if (result != MMSYSERR_NOERROR) {
            report_audio_error("write failed", result);
            return;
        }
        if (!audio_reported) {
            for (uint32_t index = 0; index < count * 2; ++index) {
                if (samples[index]) {
                    fprintf(stderr, "Audio: first non-silent effects buffer submitted to waveOut\n");
                    audio_reported = true;
                    break;
                }
            }
        }
        audio_blocks[block].queued = true;
        next_audio_block = (block + 1) % 8;
        samples += count * 2;
        frames -= count;
    }
}

int main(int argc, char **argv)
{
    SetProcessDPIAware();
    uint32_t width = 320;
    uint32_t height = 240;
    if (argc == 4) {
        char *width_end;
        char *height_end;
        unsigned long requested_width = strtoul(argv[2], &width_end, 10);
        unsigned long requested_height = strtoul(argv[3], &height_end, 10);
        if (!*argv[2] || *width_end || requested_width == 0 || requested_width > 1024 ||
            !*argv[3] || *height_end || requested_height == 0 || requested_height > 1024) {
            fprintf(stderr, "Resolution must be between 1 and 1024 pixels per axis\n");
            return 1;
        }
        width = (uint32_t)requested_width;
        height = (uint32_t)requested_height;
    }
    if ((argc != 2 && argc != 4) || !SetCurrentDirectoryA(argv[1])) {
        fprintf(stderr, "Usage: re3host <GTA III game directory> [width height]\n");
        return 1;
    }
    game_width = width;
    game_height = height;
    FILE *image = fopen("models\\gta3.img", "rb");
    FILE *data = fopen("data\\gta3.dat", "rb");
    if (!image || !data) {
        fprintf(stderr, "Missing original GTA III models/gta3.img or data/gta3.dat\n");
        if (image) fclose(image);
        if (data) fclose(data);
        return 1;
    }
    fclose(image);
    fclose(data);
    const char *pcm_path = getenv("RE3GENERIC_PCM_FILE");
    if (pcm_path && *pcm_path) {
        pcm_file = fopen(pcm_path, "wb");
        if (!pcm_file) {
            fprintf(stderr, "Cannot open PCM output file: %s\n", pcm_path);
            return 1;
        }
    }
    WAVEFORMATEX audio_format = {};
    audio_format.wFormatTag = WAVE_FORMAT_PCM;
    audio_format.nChannels = 2;
    audio_format.nSamplesPerSec = 32000;
    audio_format.wBitsPerSample = 16;
    audio_format.nBlockAlign = 4;
    audio_format.nAvgBytesPerSec = 128000;
    MMRESULT audio_result = waveOutOpen(&audio_device, WAVE_MAPPER, &audio_format, 0, 0,
                                       CALLBACK_NULL);
    if (audio_result == MMSYSERR_NOERROR) {
        fprintf(stderr, "Audio: waveOut opened at 32000 Hz, stereo 16-bit\n");
        for (unsigned index = 0; index < 8; ++index) {
            audio_blocks[index].header.lpData = (LPSTR)audio_blocks[index].samples;
            audio_blocks[index].header.dwBufferLength = sizeof(audio_blocks[index].samples);
            audio_result = waveOutPrepareHeader(audio_device, &audio_blocks[index].header, sizeof(WAVEHDR));
            audio_blocks[index].prepared = audio_result == MMSYSERR_NOERROR;
            if (!audio_blocks[index].prepared)
                report_audio_error("prepare failed", audio_result);
        }
    } else {
        audio_device = NULL;
        report_audio_error("device unavailable; continuing without playback", audio_result);
    }

    WNDCLASSA window_class = {};
    window_class.lpfnWndProc = window_proc;
    window_class.hInstance = GetModuleHandleA(NULL);
    window_class.lpszClassName = "re3generic-window";
    if (!RegisterClassA(&window_class))
        return 1;
    DWORD style = WS_OVERLAPPEDWINDOW | WS_VISIBLE;
    RECT initial_bounds = { 0, 0, (LONG)width, (LONG)height };
    AdjustWindowRect(&initial_bounds, style, FALSE);
    game_window = CreateWindowA(window_class.lpszClassName, "re3generic",
                                style, CW_USEDEFAULT, CW_USEDEFAULT,
                                initial_bounds.right - initial_bounds.left,
                                initial_bounds.bottom - initial_bounds.top, NULL, NULL,
                                window_class.hInstance, NULL);
    if (!game_window)
        return 1;

    RG_Port port = {};
    port.ticks_ms = ticks_ms;
    port.present = present;
    port.poll_input = poll_input;
    port.file_open = file_open;
    port.file_size = file_size;
    port.file_read_at = file_read_at;
    port.file_close = file_close;
    port.submit_pcm = audio_device || pcm_file ? submit_pcm : NULL;
    if (!rg_bind_port(&port) || !rg_game_init(width, height)) {
        fprintf(stderr, "Game initialization failed\n");
        return 1;
    }

    MSG message;
    while (game_window) {
        while (PeekMessageA(&message, NULL, 0, 0, PM_REMOVE)) {
            if (message.message == WM_QUIT) {
                game_window = NULL;
                break;
            }
            TranslateMessage(&message);
            DispatchMessageA(&message);
        }
        if (game_window && !rg_game_step())
            DestroyWindow(game_window);
    }
    rg_game_shutdown();
    if (audio_device) {
        waveOutReset(audio_device);
        for (unsigned index = 0; index < 8; ++index)
            if (audio_blocks[index].prepared)
                waveOutUnprepareHeader(audio_device, &audio_blocks[index].header, sizeof(WAVEHDR));
        waveOutClose(audio_device);
    }
    if (pcm_file)
        fclose(pcm_file);
    return 0;
}
