/* BootTo.NET video and input backend for Quake Generic. */

#include "quakedef.h"
#include "quakegeneric.h"

#define KEY_QUEUE_SIZE 64
#define QUAKE_MEMORY_SIZE (32 * 1024 * 1024)

static unsigned short key_queue[KEY_QUEUE_SIZE];
static unsigned int key_queue_read;
static unsigned int key_queue_write;
static unsigned char current_palette[768];
static int mouse_x;
static int mouse_y;
static int mouse_buttons;
static jmp_buf quake_exit_state;
static int quake_call_active;

extern void QG_PresentFrame(const unsigned char *pixels,
                            const unsigned char *palette,
                            int width,
                            int height);
extern int QG_PollKey(int *pressed, int *key);
extern int QG_PollMouse(int *buttons, int *delta_x, int *delta_y);
extern byte *r_stack_start;

__declspec(noreturn) void BTDN_ExitQuake(int status)
{
    (void)status;
    BTDN_RequestQuit();
    if (quake_call_active)
        longjmp(quake_exit_state, 1);

    for (;;) { }
}

void QG_Tick(double duration)
{
    int stack_marker;

    if (setjmp(quake_exit_state) != 0)
    {
        quake_call_active = 0;
        return;
    }

    quake_call_active = 1;
    r_stack_start = (byte *)&stack_marker;
    Host_Frame(duration);
    quake_call_active = 0;
}

void QG_Create(int argc, char *argv[])
{
    static quakeparms_t parms;

    if (setjmp(quake_exit_state) != 0)
    {
        quake_call_active = 0;
        return;
    }

    quake_call_active = 1;
    parms.memsize = QUAKE_MEMORY_SIZE;
    parms.membase = malloc(parms.memsize);
    if (parms.membase == NULL)
        Sys_Error("Unable to allocate %i bytes for Quake", parms.memsize);
    parms.basedir = ".";

    COM_InitArgv(argc, argv);
    parms.argc = com_argc;
    parms.argv = com_argv;

    printf("Host_Init\n");
    Host_Init(&parms);
    Cbuf_AddText("bind \"w\" \"+forward\"\n"
                 "bind \"s\" \"+back\"\n"
                 "bind \"a\" \"+moveleft\"\n"
                 "bind \"d\" \"+moveright\"\n"
                 "bind \"SPACE\" \"+jump\"\n"
                 "bind \"SHIFT\" \"+speed\"\n"
                 "bind \"MOUSE1\" \"+attack\"\n"
                 "+mlook\n");
    quake_call_active = 0;
}

static void queue_key(int pressed, int key)
{
    unsigned int next;
    if (key <= 0 || key > 255)
        return;

    next = (key_queue_write + 1) % KEY_QUEUE_SIZE;
    if (next == key_queue_read)
        key_queue_read = (key_queue_read + 1) % KEY_QUEUE_SIZE;
    key_queue[key_queue_write] = (unsigned short)(((pressed != 0) << 8) | key);
    key_queue_write = next;
}

static int convert_key(int key)
{
    switch (key)
    {
    case 8: return K_BACKSPACE;
    case 9: return K_TAB;
    case 13: return K_ENTER;
    case 19: return K_PAUSE;
    case 27: return K_ESCAPE;
    case 32: return K_SPACE;
    case 33: return K_PGUP;
    case 34: return K_PGDN;
    case 35: return K_END;
    case 36: return K_HOME;
    case 37: return K_LEFTARROW;
    case 38: return K_UPARROW;
    case 39: return K_RIGHTARROW;
    case 40: return K_DOWNARROW;
    case 45: return K_INS;
    case 46: return K_DEL;
    case 112: return K_F1;
    case 113: return K_F2;
    case 114: return K_F3;
    case 115: return K_F4;
    case 116: return K_F5;
    case 117: return K_F6;
    case 118: return K_F7;
    case 119: return K_F8;
    case 120: return K_F9;
    case 121: return K_F10;
    case 122: return K_F11;
    case 123: return K_F12;
    case 160:
    case 161: return K_SHIFT;
    case 162:
    case 163: return K_CTRL;
    case 164:
    case 165: return K_ALT;
    case 186: return ';';
    case 187: return '=';
    case 188: return ',';
    case 189: return '-';
    case 190: return '.';
    case 191: return '/';
    case 192: return '`';
    case 219: return '[';
    case 220: return '\\';
    case 221: return ']';
    case 222: return '\'';
    default:
        if (key >= 'A' && key <= 'Z')
            return key - 'A' + 'a';
        return key >= ' ' && key <= '~' ? key : 0;
    }
}

static void poll_mouse(void)
{
    int buttons;
    int delta_x;
    int delta_y;
    int changed;

    changed = QG_PollMouse(&buttons, &delta_x, &delta_y);
    if (!changed)
        return;

    mouse_x += delta_x;
    mouse_y += delta_y;
    if ((buttons & 1) != (mouse_buttons & 1))
        queue_key(buttons & 1, K_MOUSE1);
    if ((buttons & 2) != (mouse_buttons & 2))
        queue_key(buttons & 2, K_MOUSE2);
    if ((buttons & 4) != (mouse_buttons & 4))
        queue_key(buttons & 4, K_MOUSE3);
    mouse_buttons = buttons;
}

void QG_Init(void)
{
    key_queue_read = 0;
    key_queue_write = 0;
    mouse_x = 0;
    mouse_y = 0;
    mouse_buttons = 0;
    memset(key_queue, 0, sizeof(key_queue));
    memset(current_palette, 0, sizeof(current_palette));
}

int QG_GetKey(int *down, int *key)
{
    int native_down;
    int native_key;
    unsigned short event;

    if (QG_PollKey(&native_down, &native_key))
        queue_key(native_down, convert_key(native_key));
    poll_mouse();

    if (key_queue_read == key_queue_write)
        return 0;
    event = key_queue[key_queue_read];
    key_queue_read = (key_queue_read + 1) % KEY_QUEUE_SIZE;
    *down = event >> 8;
    *key = event & 0xff;
    return 1;
}

void QG_GetMouseMove(int *x, int *y)
{
    poll_mouse();
    *x = mouse_x;
    *y = mouse_y;
    mouse_x = 0;
    mouse_y = 0;
}

void QG_GetJoyAxes(float *axes)
{
    int index;
    for (index = 0; index < QUAKEGENERIC_JOY_MAX_AXES; ++index)
        axes[index] = 0.0f;
}

void QG_Quit(void)
{
    BTDN_RequestQuit();
}

void QG_DrawFrame(void *pixels)
{
    QG_PresentFrame((const unsigned char *)pixels,
                    current_palette,
                    QUAKEGENERIC_RES_X,
                    QUAKEGENERIC_RES_Y);
}

void QG_SetPalette(unsigned char palette[768])
{
    memcpy(current_palette, palette, sizeof(current_palette));
}
