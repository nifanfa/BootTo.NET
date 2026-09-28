# Bundled QEMU runtime

This directory contains the Windows x64 QEMU, 7-Zip, and fstool files required to build and run BootTo.NET without system QEMU, 7-Zip, or FAT formatting tools.

`fstool.exe` is the prebuilt Windows x64 binary from fstool v0.4.35; its MIT license is included in `FSTOOL-LICENSE.txt`.

- Version: QEMU 11.0.92
- Source installer: `qemu-w64-setup-20260729.exe` from `https://qemu.weilnetz.de/w64/2026/`
- Installer SHA-256: `F88141CCB5597CEB7BED58FFB6CD173D3FC14233772BC6EDFF6583C7B4BB816C`
- QEMU license: GPL-2.0; see `COPYING`, `COPYING.LIB`, and `firmware/edk2-licenses.txt`
- 7-Zip license: LGPL-2.1-or-later with unRAR restrictions; see `7-Zip-LICENSE.txt`

The directory contains `qemu-system-x86_64.exe`, its recursively resolved local DLL dependencies, `fstool.exe`, `7z.exe`, `7z.dll`, x64 EDK2 firmware, and the ROM files used by the project. `firmware/edk2-i386-vars.fd` is the upstream variable-store template shared by the IA32 and X64 OVMF builds; the project copies it to the build output before QEMU starts so the bundled template remains unchanged. Other architecture emulators, unused QEMU utilities, non-x64 firmware, documentation, and development files from the installer were omitted.
