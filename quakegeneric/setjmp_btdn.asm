.code

qg_setjmp PROC
    mov     [rcx+0], rbx
    lea     rax, [rsp+8]
    mov     [rcx+8], rax
    mov     [rcx+16], rbp
    mov     [rcx+24], rsi
    mov     [rcx+32], rdi
    mov     [rcx+40], r12
    mov     [rcx+48], r13
    mov     [rcx+56], r14
    mov     [rcx+64], r15
    mov     rax, [rsp]
    mov     [rcx+72], rax
    stmxcsr dword ptr [rcx+80]
    fnstcw  word ptr [rcx+84]
    movdqu  xmmword ptr [rcx+96], xmm6
    movdqu  xmmword ptr [rcx+112], xmm7
    movdqu  xmmword ptr [rcx+128], xmm8
    movdqu  xmmword ptr [rcx+144], xmm9
    movdqu  xmmword ptr [rcx+160], xmm10
    movdqu  xmmword ptr [rcx+176], xmm11
    movdqu  xmmword ptr [rcx+192], xmm12
    movdqu  xmmword ptr [rcx+208], xmm13
    movdqu  xmmword ptr [rcx+224], xmm14
    movdqu  xmmword ptr [rcx+240], xmm15
    xor     eax, eax
    ret
qg_setjmp ENDP

qg_longjmp PROC
    mov     r10, rcx
    mov     eax, edx
    test    eax, eax
    jne     qg_longjmp_value_ready
    mov     eax, 1
qg_longjmp_value_ready:
    mov     rbx, [r10+0]
    mov     rbp, [r10+16]
    mov     rsi, [r10+24]
    mov     rdi, [r10+32]
    mov     r12, [r10+40]
    mov     r13, [r10+48]
    mov     r14, [r10+56]
    mov     r15, [r10+64]
    ldmxcsr dword ptr [r10+80]
    fldcw   word ptr [r10+84]
    movdqu  xmm6, xmmword ptr [r10+96]
    movdqu  xmm7, xmmword ptr [r10+112]
    movdqu  xmm8, xmmword ptr [r10+128]
    movdqu  xmm9, xmmword ptr [r10+144]
    movdqu  xmm10, xmmword ptr [r10+160]
    movdqu  xmm11, xmmword ptr [r10+176]
    movdqu  xmm12, xmmword ptr [r10+192]
    movdqu  xmm13, xmmword ptr [r10+208]
    movdqu  xmm14, xmmword ptr [r10+224]
    movdqu  xmm15, xmmword ptr [r10+240]
    mov     r11, [r10+72]
    mov     rsp, [r10+8]
    jmp     r11
qg_longjmp ENDP

END
