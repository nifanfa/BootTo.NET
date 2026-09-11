# IL2LLVM (prebuilt)

This directory contains the prebuilt IL2LLVM compiler used by BootTo.NET. It translates a managed assembly compiled with the repository's custom CoreLib into an LLVM native object file.

This is a compiler, not a .NET runtime and not NativeAOT. It does not produce a complete executable or EFI image by itself. A native host or linker must provide the platform entry point and every external runtime symbol used by the managed program.

## Files

| File | Purpose |
| --- | --- |
| `IL2LLVM.exe` | Translator executable |
| `libLLVM.dll` | LLVM runtime required by `IL2LLVM.exe`; keep it beside the executable |
| `IL2LLVM.pdb` | Optional debugging symbols |

`IL2LLVM.exe` and `libLLVM.dll` are a matched pair. Do not copy only the executable, rename `libLLVM.dll`, or run the executable from a directory where that DLL cannot be found.

`libLLVM.dll` is UPX-compressed so the repository can be hosted on GitHub, which rejects individual files larger than 100 MB. UPX compression is transparent to the Windows loader; keep the file intact and do not decompress, recompress, or replace it with an unrelated LLVM build.

The source project is maintained separately at [github.com/nifanfa/IL2LLVM](https://github.com/nifanfa/IL2LLVM). This directory is intentionally a binary drop and does not contain `IL2LLVM.csproj` or a managed `IL2LLVM.dll`.

## Command line

The compiler requires exactly three arguments:

```text
IL2LLVM.exe <input-assembly> <output-object> <target>[;<code-model>]
```

- `input-assembly`: the managed `.dll` produced from a project that uses the custom CoreLib.
- `output-object`: path of the native object to create. Parent directories are created by the caller/build target.
- `target`: an LLVM target triple, for example `x86_64-pc-windows-msvc` or `x86_64-unknown-linux-gnu`.
- `code-model`: optional LLVM code model: `default`, `tiny`, `small`, `kernel`, `medium`, or `large`.

When a code model is supplied, quote the complete third argument in PowerShell because `;` is a statement separator there.

## BootTo.NET console/EFI build

From the BootTo.NET repository root, build the managed project first:

```powershell
dotnet build ConsoleApp1\ConsoleApp1.csproj
```

Then invoke the binary in this directory. The repository's MSBuild targets normally perform this step automatically through `IL2LLVMTool`:

```powershell
IL2LLVM\IL2LLVM.exe `
  ConsoleApp1\bin\Debug\net10.0\ConsoleApp1.dll `
  ConsoleApp1\obj\Debug\ConsoleApp1.obj `
  x86_64-pc-windows-msvc
```

When building inside Visual Studio, `Directory.Build.targets` invokes `EfiApplication\EfiApplication.vcxproj` after the object has been generated. That native project links the object with the EFI runtime and produces:

```text
Drive\EFI\BOOT\BOOTX64.efi
```

The EFI project is the native entry point. IL2LLVM only generates the managed object; it does not start QEMU, create a FAT image, or link the EFI application.

To avoid stale objects, clean the managed project's `bin` and `obj` directories before rebuilding when changing source or target settings. Verify the timestamp of the generated `.obj` before linking.

## Linux kernel-module build

The kernel-module example must be translated with the `kernel` code model so that the generated relocations are suitable for the high kernel address range:

```powershell
dotnet build LinuxKernelModuleExample\LinuxKernelModuleExample.csproj

IL2LLVM\IL2LLVM.exe `
  LinuxKernelModuleExample\bin\Debug\net10.0\LinuxKernelModuleExample.dll `
  LinuxKernelModuleExample\bin\Debug\net10.0\LinuxKernelModuleExample.obj `
  "x86_64-unknown-linux-gnu;kernel"
```

In a Linux environment with headers matching the running kernel:

```sh
cd LinuxKernelModuleExample
make KDIRS=/lib/modules/$(uname -r)/build
sudo insmod my_module.ko
dmesg | tail -n 30
sudo rmmod my_module
```

The module runtime is in `LinuxKernelModuleExample/Runtime.c`, including the GC-frame and exception-frame helpers. The Makefile links that runtime, `runtime_jump_x86_64.S`, the generated managed object, and the kernel module entry point. It does not use the C standard library.

## Runtime boundary

CoreLib is compiled into each managed input assembly. It supplies managed type and runtime contracts, but it intentionally does not provide platform implementations. Methods imported with `[DllImport("*")]` become external symbols and must be implemented by the final native host.

Typical host-provided symbols include:

- allocation and release (`calloc`, `free`);
- exception transfer (`setjmp`, `longjmp`);
- GC-frame and exception-frame operations;
- console output and `abort`;
- any application-specific native entry points.

The compiler does not add an interop layer or select a platform runtime automatically. Runtime C code belongs to the host (EFI application, console apphost, or kernel module), while CoreLib and IL2LLVM remain platform-neutral.

## Common failures

### `libLLVM.dll` cannot be found

Run `IL2LLVM.exe` from this directory or copy `IL2LLVM.exe` and `libLLVM.dll` together. Do not invoke a nonexistent `IL2LLVM.dll` with `dotnet`.

### The output object is unchanged

Check that the input assembly was rebuilt and that the output path is the object consumed by the native linker. Delete the relevant `bin`/`obj` output and run the translator again if necessary.

### Unresolved external symbols at native link time

The managed object references a runtime or platform function that the selected host does not define. Add the implementation to the host runtime (`EfiApplication/Runtime.c`, `apphost/Runtime.c`, or `LinuxKernelModuleExample/Runtime.c`) rather than changing managed application code to work around the missing symbol.

### `System.Object` or framework assembly resolution errors

The input must be built with the repository's CoreLib and `NoStdLib` configuration. Do not pass a normal framework-dependent application to this compiler; it is not a general .NET assembly linker.

## Scope

IL2LLVM translates supported IL from the input assembly into a relocatable native object. Native linking, EFI image creation, QEMU execution, kernel-module loading, platform ABI details, and host runtime behavior remain the responsibility of the target project.
