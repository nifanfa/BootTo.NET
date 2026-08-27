# EFI Drivers

DXE drivers loaded by BootTo.NET. The load order is defined by the `DxeDrivers`
list in `ConsoleApp1/Program.EFI.cs`.

## HTTP and TLS

The application uses the firmware-provided network stack. The drivers below are
loaded in their listed order and provide HTTP/HTTPS plus the support services
required by TLS. The firmware Network Stack must be enabled and connected before
running the application.

| Driver | Purpose |
| --- | --- |
| `RngDxe.efi` | Provides the UEFI random number protocol required by TLS cryptography. |
| `Hash2DxeCrypto.efi` | Provides the UEFI Hash2 protocol and cryptographic algorithms used by TLS. |
| `DpcDxe.efi` | Provides the Deferred Procedure Call protocol used to dispatch asynchronous network completion callbacks. |
| `DnsDxe.efi` | Provides EFI DNS4 name resolution. BootTo.NET supplies `8.8.8.8` explicitly, so DnsDxe does not use its DHCP/`Ip4Config2` DNS-server discovery path. |
| `HttpUtilitiesDxe.efi` | Provides shared HTTP message and header parsing and generation utilities. |
| `TlsDxe.efi` | Provides UEFI TLS protocols and encrypted sessions. HTTPS certificate validation uses the `TlsCaCertificate` UEFI variable. This copy is built from the OpenCore UDK source with a fix for the OpenSSL empty `EX_CALLBACK` stack path. |
| `HttpDxe.efi` | Provides the UEFI HTTP protocol and binds to the firmware TCP/IP and DNS stack, including TLS for HTTPS URLs.(WARNING: this is NOT original HttpDxe.efi. HttpDns4 method is edited!) |

## Other Drivers

| Driver | Purpose |
| --- | --- |
| `AudioDxe.efi` | Provides the Acidanthera Audio I/O protocol for startup sounds and PCM playback on compatible HDA audio devices. |
| `XhciDxe.efi` | Provides the USB xHCI host controller driver for USB 3.x controllers and attached USB devices. |
| `UsbMouseDxe.efi` | Provides USB mouse input and publishes the UEFI pointer input protocol. |

## Other Files

| File | Purpose |
| --- | --- |
| `LICENSE.txt` | License information for drivers in this directory. It is not a loadable EFI driver. |

TLS trust anchors are stored separately in
`\EFI\Certificates\TlsCaCertificate.esl`; see the README in that directory.
