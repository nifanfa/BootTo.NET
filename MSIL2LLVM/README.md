# MSIL2LLVM

BootTo.NET's prebuilt IL-to-LLVM compiler. It converts an assembly built with
the repository's custom CoreLib into a native object file. It is not a runtime
or linker.

Keep `MSIL2LLVM.exe` and `libLLVM.dll` together when running the compiler.

```text
MSIL2LLVM.exe <input-assembly> <output-object> <target>[;<code-model>]
```

The input must use this repository's CoreLib and `NoStdLib` configuration.
BootTo.NET's MSBuild targets invoke MSIL2LLVM automatically; the host project
provides linking, the native entry point, and runtime symbols.

Source: [nifanfa/MSIL2LLVM](https://github.com/nifanfa/MSIL2LLVM).
