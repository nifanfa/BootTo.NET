; Initial x64 interface dispatch for the single-module UEFI runtime.
; CoreRT normally routes RhpInterfaceDispatchSlow through its universal transition
; thunk and RhpCidResolve. This runtime has no transition thunk or dispatch cache,
; so the slow path calls RhpResolveInterfaceMethod directly.

OPTION PROLOGUE:NONE
OPTION EPILOGUE:NONE

EXTERN RhpResolveInterfaceMethod:PROC

.code

PUBLIC RhpInitialInterfaceDispatch
PUBLIC RhpInitialDynamicInterfaceDispatch
RhpInitialInterfaceDispatch PROC
RhpInitialDynamicInterfaceDispatch LABEL PROC
    jmp RhpInterfaceDispatchSlow
RhpInitialInterfaceDispatch ENDP

PUBLIC RhpInterfaceDispatchSlow
RhpInterfaceDispatchSlow PROC
    sub rsp, 0A8h

    mov [rsp + 20h], rcx
    mov [rsp + 28h], rdx
    mov [rsp + 30h], r8
    mov [rsp + 38h], r9
    movdqu [rsp + 40h], xmm0
    movdqu [rsp + 50h], xmm1
    movdqu [rsp + 60h], xmm2
    movdqu [rsp + 70h], xmm3

    mov rdx, r10
    call RhpResolveInterfaceMethod
    mov r11, rax

    mov rcx, [rsp + 20h]
    mov rdx, [rsp + 28h]
    mov r8, [rsp + 30h]
    mov r9, [rsp + 38h]
    movdqu xmm0, [rsp + 40h]
    movdqu xmm1, [rsp + 50h]
    movdqu xmm2, [rsp + 60h]
    movdqu xmm3, [rsp + 70h]

    add rsp, 0A8h
    jmp r11
RhpInterfaceDispatchSlow ENDP

END
