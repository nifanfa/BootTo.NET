# TLS CA Certificates

When the firmware does not already provide the edk2 `TlsCaCertificate` UEFI
variable, `TlsCaCertificate.esl` is installed before the network DXE drivers
are loaded. Existing firmware trust settings are preserved. The file contains
X.509 roots in the standard EFI signature-list format expected by `HttpDxe`.

The bundled database currently contains `AAA Certificate Services`, exported
from the Windows Local Machine trusted root store. Its SHA-1 thumbprint is
`D1EB23A46D17D68FD92564C2F1F1601764D8E349`.

Run `Export-TlsCaCertificate.ps1` from PowerShell to regenerate the ESL from
the Windows trusted root store. Pass one or more SHA-1 thumbprints with
`-Thumbprint` to build a database containing other roots:

```powershell
.\Export-TlsCaCertificate.ps1 -Thumbprint `
    D1EB23A46D17D68FD92564C2F1F1601764D8E349, `
    <another-root-thumbprint>
```

Each certificate is encoded in its own `EFI_SIGNATURE_LIST`, and the lists are
concatenated into `TlsCaCertificate.esl` as required by edk2.

This deliberately small database fits firmware variable-size limits and
validates the SSL.com certificate chain currently served by `example.com`.
It is not a general-purpose Web PKI root store. Replace the ESL with the roots
required by the deployment environment when other HTTPS endpoints are used.
