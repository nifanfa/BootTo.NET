[CmdletBinding()]
param(
    [string[]] $Thumbprint = @(
        "D1EB23A46D17D68FD92564C2F1F1601764D8E349"
    ),

    [string] $OutputPath = (Join-Path $PSScriptRoot "TlsCaCertificate.esl")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$efiCertX509Guid = [Guid]"a5c059a1-94e4-4aa7-87b5-ab155c2bf072"
$signatureOwner = [Guid]"fd2340d0-3dab-4349-a6c7-3b4f12b48eae"
$stores = @(
    "Cert:\LocalMachine\Root",
    "Cert:\CurrentUser\Root"
)

function Find-RootCertificate([string] $requestedThumbprint) {
    $normalized = $requestedThumbprint.Replace(" ", "").ToUpperInvariant()
    foreach ($store in $stores) {
        $path = Join-Path $store $normalized
        if (Test-Path -LiteralPath $path) {
            return Get-Item -LiteralPath $path
        }
    }

    throw "Trusted root certificate $normalized was not found in Windows."
}

$output = [IO.MemoryStream]::new()
$writer = [IO.BinaryWriter]::new($output)

try {
    foreach ($requestedThumbprint in $Thumbprint) {
        $certificate = Find-RootCertificate $requestedThumbprint
        $der = $certificate.RawData

        # X.509 entries normally use one EFI_SIGNATURE_LIST per certificate.
        $writer.Write($efiCertX509Guid.ToByteArray())
        $writer.Write([uint32](28 + 16 + $der.Length))
        $writer.Write([uint32]0)
        $writer.Write([uint32](16 + $der.Length))
        $writer.Write($signatureOwner.ToByteArray())
        $writer.Write($der)

        $simpleName = $certificate.GetNameInfo(
            [Security.Cryptography.X509Certificates.X509NameType]::SimpleName,
            $false)
        Write-Host "Added: $simpleName ($($certificate.Thumbprint))"
    }

    $writer.Flush()
    [IO.File]::WriteAllBytes($OutputPath, $output.ToArray())
    Write-Host "Created: $OutputPath ($($output.Length) bytes)"
}
finally {
    $writer.Dispose()
    $output.Dispose()
}
