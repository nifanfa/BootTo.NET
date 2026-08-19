; Runtime helpers emitted directly by the Native AOT compiler.

OPTION PROLOGUE:NONE
OPTION EPILOGUE:NONE

PAGE_SIZE EQU 1000h

.code

; R11 points to the lowest address in the stack frame being allocated.
; Probe every intervening page without changing RSP or R11.
PUBLIC RhpStackProbe
RhpStackProbe PROC
    mov     rax, rsp
    and     rax, -PAGE_SIZE

ProbeLoop:
    sub     rax, PAGE_SIZE
    test    dword ptr [rax], eax
    cmp     rax, r11
    jg      ProbeLoop
    ret
RhpStackProbe ENDP

END
