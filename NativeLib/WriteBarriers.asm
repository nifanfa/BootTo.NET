; NativeAOT emits calls to these helpers using the CoreRT write-barrier ABI.
; This GC is non-moving and has no generations, so only the reference store is
; required. These must remain leaf functions because helper calls do not reserve
; normal Win64 outgoing shadow space.

OPTION PROLOGUE:NONE
OPTION EPILOGUE:NONE

.code

PUBLIC RhpAssignRef
RhpAssignRef PROC
    mov qword ptr [rcx], rdx
    ret
RhpAssignRef ENDP

PUBLIC RhpCheckedAssignRef
RhpCheckedAssignRef PROC
    mov qword ptr [rcx], rdx
    ret
RhpCheckedAssignRef ENDP

; CoreRT by-ref copy convention:
;   RSI = address of source reference
;   RDI = address of destination reference
; Both pointers are advanced by one reference on return.
PUBLIC RhpByRefAssignRef
RhpByRefAssignRef PROC
    mov rcx, qword ptr [rsi]
    mov qword ptr [rdi], rcx
    add rsi, 8
    add rdi, 8
    ret
RhpByRefAssignRef ENDP

END
