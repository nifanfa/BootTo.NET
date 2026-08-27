# Bundled QEMU runtime

This directory contains the Windows x64 QEMU and 7-Zip files required to build and run BootTo.NET without system QEMU or 7-Zip installations.

- Version: QEMU 11.0.92
- Source installer: `qemu-w64-setup-20260729.exe` from `https://qemu.weilnetz.de/w64/2026/`
- Installer SHA-256: `F88141CCB5597CEB7BED58FFB6CD173D3FC14233772BC6EDFF6583C7B4BB816C`
- QEMU license: GPL-2.0; see `COPYING`, `COPYING.LIB`, and `firmware/edk2-licenses.txt`
- 7-Zip license: LGPL-2.1-or-later with unRAR restrictions; see `7-Zip-LICENSE.txt`

The directory contains `qemu-img.exe`, `qemu-system-x86_64.exe`, their recursively resolved local DLL dependencies, `7z.exe`, `7z.dll`, x64 EDK2 firmware, and the ROM files used by the project. `firmware/edk2-i386-vars.fd` is the upstream variable-store template shared by the IA32 and X64 OVMF builds; the project copies it to the build output before QEMU starts so the bundled template remains unchanged. Other architecture emulators, unused QEMU utilities, non-x64 firmware, documentation, and development files from the installer were omitted.

`SHA256SUMS.txt` records the SHA-256 hash of every bundled runtime and firmware file.
