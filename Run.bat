@echo off
if not exist "C:\Program Files\Intel\HAXM\IntelHaxm.sys" (
	echo "Please install Intel Hardware Accelerated Execution Manager (HAXM) in order to speed up QEMU https://github.com/intel/haxm/releases"
	pause
	exit
)
if not exist "C:\Program Files\qemu\qemu-system-x86_64.exe" (
	echo "Please install QEMU in order to debug MOOS!(do not modify the path) https://www.qemu.org/download/#windows"
	pause
	exit
)
if not exist "%cd%\Drive\EFI\BOOT\BOOTX64.efi" (
	echo "Do not running this batch file manually!"
	pause
	exit
)
"C:\Program Files\qemu\qemu-system-x86_64.exe" -accel hax -m 1024 -smp 2 -k en-gb -boot d -d guest_errors -serial stdio -device AC97 -rtc base=localtime -bios OVMF.fd -drive file=fat:rw:Drive -net nic,model=rtl8139 -net tap,ifname=tap