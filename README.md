# BootTo.NET Project

## Building

Open `BootTo.NET.slnx` in Visual Studio and build the `x64` configuration. The managed project is compiled to IL, IL2LLVM produces `ConsoleApp1.obj`, and `EfiApplication` links it with the native libraries into `Drive/EFI/BOOT/BOOTX64.efi`.  
  
You can simply start ConsoleApp1 or press F5. msbuild will handle the rest.
<p align="center">
  <img alt="QQ_1787932865567" src="https://github.com/user-attachments/assets/aedd8930-933e-41c8-90c0-48376e5329e0" />
</p>

## Synopsis
*When will hobby OS developers realize that we don't need to implement everything from scratch? With a clean environment providing basic network, graphics, filesystem, and USB support, there's no need to build it yourself—just load a DXE driver and go ahead with your 'OS'.*  

The repository includes QEMU, its Windows runtime dependencies, and EDK2 firmware; no system QEMU installation is required. After building the solution, run the `RunQemu` target from `ConsoleApp1.csproj` to create `Drive.img` and boot it.

QEMU mounts the bundled UEFI variable template through a temporary snapshot. Variable changes are discarded when QEMU exits, so stale physical-network settings cannot override the `-netdev user` DHCP configuration and no per-build `.vars.fd` file is created.

`RunQemu` detects whether the bundled QEMU supports WHPX and whether the Windows hypervisor is active. It uses WHPX only when both checks pass; otherwise it falls back to TCG.

> **Performance:** Enable **Windows Hypervisor Platform** (`Windows 虚拟机监控程序平台`) in **Turn Windows features on or off**, then restart Windows. This allows QEMU to use WHPX hardware acceleration; without it, QEMU falls back to TCG and runs significantly slower.

- [Running on real hardware](#running-on-real-hardware)
- [Debugging with QEMU](#debugging-with-qemu)

# Tested and working on real hardware
<p align="center">
  <img alt="image" src="https://github.com/user-attachments/assets/ab65cd9b-521a-4177-af3e-3ef58b57ca76" />
</p>

# Quake generic (True 3D)
> **Key Mapping:** `W` / `S` -> Move Forward / Backward | `A` / `D` -> Strafe Left / Right | `Space` -> Jump / Swim Up | `Left Shift` -> Sprint | `Mouse Left Click` -> Attack | `Mouse Movement` -> Look / Camera | `Esc` -> Menu / Back | `Enter` -> Confirm | `1–8` -> Switch Weapon | `` ` `` / `~` -> Toggle Console
<p align="center">
  <img alt="QQ_1787901663795" src="https://github.com/user-attachments/assets/a4b9f0c2-99da-469f-bd60-3342b0a4359d" />
</p>

# DOOM generic
> **Key Mapping:** `W` `S` `A` `D` -> Movement | `Mouse Movement` -> Look / Camera | `Mouse Left Click` -> Fire | `E` -> Use / Open Door | `Left Shift` -> Sprint | `Enter` -> Confirm | `Esc` -> Menu / Back
<p align="center">
  <img alt="QQ_1787916501069" src="https://github.com/user-attachments/assets/18d12961-0bce-4130-9d15-50307a11ff36" />
</p>

## NES Emulator
> **Key Mapping:** `Q` -> A | `E` -> B | `Z` -> Select | `C` -> Start | `W` `S` `A` `D` -> Directional Pad
<p align="center">
  <img alt="QQ_1787844001027" src="https://github.com/user-attachments/assets/3eb68764-f95c-4112-8e7b-36ee941d25e3" />
</p>
  
## Running on real hardware

Format a USB drive as FAT32, copy the contents of `Drive` to its root, and boot from it. To use UEFI network support on real hardware, enable it in the BIOS, as shown in the image below.

<p align="center">
  <img src="https://user-images.githubusercontent.com/54205437/188054542-60a4bc00-a2f2-462d-9602-6a55b146b127.png" />
</p>

## Debugging with QEMU

After building `EfiApplication`, invoke the `RunQemu` target. It packages `Drive` as a writable FAT disk image and boots it with the bundled EDK2 firmware.

The SDL window uses QEMU's default `Left Ctrl+Left Alt+G` shortcut to release mouse and keyboard input. On Windows, select the English input method for the QEMU window because an active IME can intercept this shortcut.
