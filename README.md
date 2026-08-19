# BootTo.NET Project

## Debugging
1. **Open Project**: Open the `BootTo.NET.sln` solution in Visual Studio.
2. **Launch**: Select **QEMU** from the launch profile dropdown, or simply press **F5**.
<img width="1232" height="387" alt="QQ_1787146565287" src="https://github.com/user-attachments/assets/671fb673-dab9-4685-a80b-f43171ff109e" />

## Synopsis
*When will hobby OS developers realize that we don't need to implement everything from scratch? With a clean environment providing basic network, graphics, filesystem, and USB support, there's no need to build it yourself—just load a DXE driver and go ahead with your 'OS'.*  

Publishing uses the repository-local `qemu-img` to create `ConsoleApp1.vhd`, then starts the repository-local QEMU x64 emulator. QEMU, its Windows runtime dependencies, and the EDK2 UEFI firmware are included in `qemu`; no system QEMU installation and no administrator privileges are required.

Use `dotnet publish --tl:off -c Release ConsoleApp1` to publish and run with live build output. Pass `-p:RunQemu=false` to publish without starting QEMU.

`RunQemu` detects whether the bundled QEMU supports WHPX and whether the Windows hypervisor is active. It uses WHPX only when both checks pass; otherwise it falls back to TCG.

> **Performance:** Enable **Windows Hypervisor Platform** (`Windows 虚拟机监控程序平台`) in **Turn Windows features on or off**, then restart Windows. This allows QEMU to use WHPX hardware acceleration; without it, QEMU falls back to TCG and runs significantly slower.

- [Running on real hardware](#running-on-real-hardware)
- [Debugging with QEMU](#debugging-with-qemu)

<img width="1282" height="839" alt="QQ_1787098965779" src="https://github.com/user-attachments/assets/79cb6705-0e18-40cf-adaa-6ee414ddfec6" />

## NES Emulator
> **Key Mapping:** `Q` -> A | `E` -> B | `Z` -> Select | `C` -> Start | `W` `S` `A` `D` -> Directional Pad
<img width="1282" height="839" alt="QQ_1787149465359" src="https://github.com/user-attachments/assets/6265a4b7-4225-4f45-a1ea-091b929ab840" />
  
## Nyan cat
<img width="1282" height="839" alt="QQ_1786863585140" src="https://github.com/user-attachments/assets/4d7d2cce-847c-43b0-8f61-f5433d30cb28" />  

## Running on real hardware

Format a USB drive as FAT32 and copy the contents of `Drive` to its root. To use UEFI network support on real hardware, enable it in the firmware settings.

<p align="center">
  <img src="https://user-images.githubusercontent.com/54205437/188054542-60a4bc00-a2f2-462d-9602-6a55b146b127.png" />
</p>

## Debugging with QEMU

After publishing, `CopyEFI` updates `Drive/EFI/BOOT/BOOTX64.efi`, `CreateVHD` converts that directory into a dynamic VHD without mounting it, and `RunQemu` boots the VHD with the bundled EDK2 firmware. Pass `-p:RunQemu=false` to create the VHD without starting the emulator, or also pass `-p:CreateVHD=false` to skip VHD creation.

The SDL window uses QEMU's default `Left Ctrl+Left Alt+G` shortcut to release mouse and keyboard input. On Windows, select the English input method for the QEMU window because an active IME can intercept this shortcut.
