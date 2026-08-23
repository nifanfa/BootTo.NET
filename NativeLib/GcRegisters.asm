; Capture all general-purpose registers that can contain managed references.
; Microsoft x64 ABI: RCX points at GarbageCollector.RegisterSnapshot.

OPTION PROLOGUE:NONE
OPTION EPILOGUE:NONE

.code

PUBLIC RhpCaptureRegisters
RhpCaptureRegisters PROC
    mov [rcx + 00h], rax
    mov [rcx + 08h], rbx
    mov [rcx + 10h], rcx
    mov [rcx + 18h], rdx
    mov [rcx + 20h], rsi
    mov [rcx + 28h], rdi
    mov [rcx + 30h], rbp
    lea rax, [rsp + 08h]
    mov [rcx + 38h], rax
    mov [rcx + 40h], r8
    mov [rcx + 48h], r9
    mov [rcx + 50h], r10
    mov [rcx + 58h], r11
    mov [rcx + 60h], r12
    mov [rcx + 68h], r13
    mov [rcx + 70h], r14
    mov [rcx + 78h], r15
    ret
RhpCaptureRegisters ENDP

END
