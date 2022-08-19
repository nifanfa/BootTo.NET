# BootTo.NET Project

- [Running on real hardware](#running-on-real-hardware)
- [Debugging on qemu](#debugging-on-qemu)

## Running on real hardware
if you want to running BootTo.NET on real hardware. go BIOS and enable this to enable network support for UEFI
<p align="center">
  <img src="https://user-images.githubusercontent.com/54205437/188054542-60a4bc00-a2f2-462d-9602-6a55b146b127.png" />
</p>

## Debugging on qemu
before using the repo. you have to install:  
qemu(https://www.qemu.org/download/#windows)  
intel-haxm(https://github.com/intel/haxm/releases)  
windows tap driver(tap-windows-9.21.2.exe)  
and then open windows control panel (control panel->network and internet->network connection)  
rename this adapter to tap  
<p align="center">
  <img src="https://user-images.githubusercontent.com/54205437/187689564-2ee015f6-1bea-47ac-9f4a-fafa55d3704f.png" />
</p>
and then select a network adapter that have connected to internet, right click->properties, share, check 'Allow other network users to connect through this computer’s Internet connection' and select tap  
<br />
<br />
<p align="center">
  <img src="https://user-images.githubusercontent.com/54205437/187690650-2d1d2284-0085-4c34-be87-7bf8c502f2ed.png" />
</p>
