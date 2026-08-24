; Minimal UEFI exception bridge for the CoreRT/Native AOT EH metadata.
; RhpThrowEx captures the throwing frame, RhThrowEx finds a matching
; typed handler, and this bridge invokes the generated catch funclet.

OPTION PROLOGUE:NONE
OPTION EPILOGUE:NONE

EXTERN RhThrowEx : PROC
EXTERN RhRethrow : PROC
EXTERN RhEndCatch : PROC

.code

PUBLIC RhpThrowEx
RhpThrowEx PROC
    ; The caller's return address is still at [rsp]. Keep the caller frame
    ; intact while the managed dispatcher reads the PE exception tables.
    mov     r10, rsp
    sub     rsp, 128h

    ; ExInfo starts at rsp + 20h. The following scratch qword stores the
    ; exception object while the managed dispatcher runs.
    mov     rax, [r10]
    mov     [rsp + 20h], rax
    lea     rax, [r10 + 08h]
    mov     [rsp + 28h], rax
    mov     [rsp + 30h], rbp
    mov     qword ptr [rsp + 38h], 0
    mov     [rsp + 40h], rbx
    mov     [rsp + 48h], rsi
    mov     [rsp + 50h], rdi
    mov     [rsp + 58h], r12
    mov     [rsp + 60h], r13
    mov     [rsp + 68h], r14
    mov     [rsp + 70h], r15

    movdqu  xmmword ptr [rsp + 78h], xmm6
    movdqu  xmmword ptr [rsp + 88h], xmm7
    movdqu  xmmword ptr [rsp + 98h], xmm8
    movdqu  xmmword ptr [rsp + 0A8h], xmm9
    movdqu  xmmword ptr [rsp + 0B8h], xmm10
    movdqu  xmmword ptr [rsp + 0C8h], xmm11
    movdqu  xmmword ptr [rsp + 0D8h], xmm12
    movdqu  xmmword ptr [rsp + 0E8h], xmm13
    movdqu  xmmword ptr [rsp + 0F8h], xmm14
    movdqu  xmmword ptr [rsp + 108h], xmm15
    mov     [rsp + 118h], rcx

    mov     rcx, [rsp + 118h]
    lea     rdx, [rsp + 20h]
    call    RhThrowEx

    lea     r10, [rsp + 20h]
    mov     r11, [r10 + 18h]
    test    r11, r11
    jz      Unhandled

    ; Restore the target frame's nonvolatile register homes before entering
    ; the funclet. The funclet ABI receives the establisher SP in RCX and the
    ; exception object in RDX. It returns its continuation in RAX.
    mov     rbx, [r10 + 20h]
    mov     rsi, [r10 + 28h]
    mov     rdi, [r10 + 30h]
    mov     r12, [r10 + 38h]
    mov     r13, [r10 + 40h]
    mov     r14, [r10 + 48h]
    mov     r15, [r10 + 50h]
    movdqu  xmm6,  xmmword ptr [r10 + 58h]
    movdqu  xmm7,  xmmword ptr [r10 + 68h]
    movdqu  xmm8,  xmmword ptr [r10 + 78h]
    movdqu  xmm9,  xmmword ptr [r10 + 88h]
    movdqu  xmm10, xmmword ptr [r10 + 98h]
    movdqu  xmm11, xmmword ptr [r10 + 0A8h]
    movdqu  xmm12, xmmword ptr [r10 + 0B8h]
    movdqu  xmm13, xmmword ptr [r10 + 0C8h]
    movdqu  xmm14, xmmword ptr [r10 + 0D8h]
    movdqu  xmm15, xmmword ptr [r10 + 0E8h]

    mov     rbp, [r10 + 10h]
    mov     rcx, [r10 + 08h]
    mov     rdx, [rsp + 118h]
    call    r11

    ; A normally completed catch is no longer active. Preserve the funclet's
    ; continuation while the managed active-ExInfo stack is updated.
    mov     [rsp + 120h], rax
    call    RhEndCatch
    mov     r10, [rsp + 120h]
    mov     rsp, [rsp + 28h]
    jmp     r10

Unhandled:
    ; RhThrowEx has already reported the exception. There is no firmware
    ; process to terminate, so keep the EFI image in a deterministic halt.
    int     3
    jmp     Unhandled
RhpThrowEx ENDP

; void RhpRethrow()
; Like CoreRT's helper, this captures a fresh context at the rethrow site.
; RhRethrow retrieves the active exception from the reduced single-threaded
; ExInfo state and returns it in RAX for the selected catch funclet.
PUBLIC RhpRethrow
RhpRethrow PROC
    mov     r10, rsp
    sub     rsp, 128h

    mov     rax, [r10]
    mov     [rsp + 20h], rax
    lea     rax, [r10 + 08h]
    mov     [rsp + 28h], rax
    mov     [rsp + 30h], rbp
    mov     qword ptr [rsp + 38h], 0
    mov     [rsp + 40h], rbx
    mov     [rsp + 48h], rsi
    mov     [rsp + 50h], rdi
    mov     [rsp + 58h], r12
    mov     [rsp + 60h], r13
    mov     [rsp + 68h], r14
    mov     [rsp + 70h], r15

    movdqu  xmmword ptr [rsp + 78h], xmm6
    movdqu  xmmword ptr [rsp + 88h], xmm7
    movdqu  xmmword ptr [rsp + 98h], xmm8
    movdqu  xmmword ptr [rsp + 0A8h], xmm9
    movdqu  xmmword ptr [rsp + 0B8h], xmm10
    movdqu  xmmword ptr [rsp + 0C8h], xmm11
    movdqu  xmmword ptr [rsp + 0D8h], xmm12
    movdqu  xmmword ptr [rsp + 0E8h], xmm13
    movdqu  xmmword ptr [rsp + 0F8h], xmm14
    movdqu  xmmword ptr [rsp + 108h], xmm15

    lea     rcx, [rsp + 20h]
    call    RhRethrow
    mov     [rsp + 118h], rax

    lea     r10, [rsp + 20h]
    mov     r11, [r10 + 18h]
    test    r11, r11
    jz      RethrowUnhandled

    mov     rbx, [r10 + 20h]
    mov     rsi, [r10 + 28h]
    mov     rdi, [r10 + 30h]
    mov     r12, [r10 + 38h]
    mov     r13, [r10 + 40h]
    mov     r14, [r10 + 48h]
    mov     r15, [r10 + 50h]
    movdqu  xmm6,  xmmword ptr [r10 + 58h]
    movdqu  xmm7,  xmmword ptr [r10 + 68h]
    movdqu  xmm8,  xmmword ptr [r10 + 78h]
    movdqu  xmm9,  xmmword ptr [r10 + 88h]
    movdqu  xmm10, xmmword ptr [r10 + 98h]
    movdqu  xmm11, xmmword ptr [r10 + 0A8h]
    movdqu  xmm12, xmmword ptr [r10 + 0B8h]
    movdqu  xmm13, xmmword ptr [r10 + 0C8h]
    movdqu  xmm14, xmmword ptr [r10 + 0D8h]
    movdqu  xmm15, xmmword ptr [r10 + 0E8h]

    mov     rbp, [r10 + 10h]
    mov     rcx, [r10 + 08h]
    mov     rdx, [rsp + 118h]
    call    r11

    mov     [rsp + 120h], rax
    call    RhEndCatch
    mov     r10, [rsp + 120h]
    mov     rsp, [rsp + 28h]
    jmp     r10

RethrowUnhandled:
    int     3
    jmp     RethrowUnhandled
RhpRethrow ENDP

PUBLIC RhpCallFilterFunclet
RhpCallFilterFunclet PROC
    ; RCX: exception object, RDX: filter address, R8: establisher SP,
    ; R9: establisher frame pointer. The filter returns its decision in RAX.
    push    rbp
    mov     rbp, r9
    mov     r10, rdx
    mov     rdx, rcx
    mov     rcx, r8
    sub     rsp, 20h
    call    r10
    add     rsp, 20h
    pop     rbp
    ret
RhpCallFilterFunclet ENDP

END
