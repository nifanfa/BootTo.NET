using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Net
{
    public static class Dns
    {
        private const byte IpProtocolUdp = 17;
        private const uint RetryCount = 2;
        private const uint RetryIntervalSeconds = 2;

        private static EFI_GUID Dns4ServiceBindingProtocolGuid => new EFI_GUID(
            0xb625b186, 0xe063, 0x44f7, 0x89, 0x05, 0x6a, 0x74, 0xdc, 0x6f, 0x52, 0xb4);

        private static EFI_GUID Dns4ProtocolGuid => new EFI_GUID(
            0xae3d28cc, 0xe05b, 0x4fa1, 0xa0, 0x11, 0x7e, 0xb5, 0x5a, 0x3f, 0x14, 0x01);

        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
            => GetHostAddressesAsync(hostNameOrAddress).GetAwaiter().GetResult();

        public static Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress)
        {
            if (hostNameOrAddress == null)
                throw new ArgumentNullException("The host name cannot be null.");
            if (hostNameOrAddress.Length == 0)
                throw new ArgumentException("The host name cannot be empty.");

            if (IPAddress.TryParse(hostNameOrAddress, out IPAddress address))
                return Task.FromResult(new IPAddress[] { address });

            try
            {
                return Task.FromResult(ResolveHostName(hostNameOrAddress));
            }
            catch (Exception exception)
            {
                return Task.FromException<IPAddress[]>(exception);
            }
        }

        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
            => GetHostEntryAsync(hostNameOrAddress).GetAwaiter().GetResult();

        public static async Task<IPHostEntry> GetHostEntryAsync(string hostNameOrAddress)
        {
            IPAddress[] addresses = await GetHostAddressesAsync(hostNameOrAddress);
            return new IPHostEntry
            {
                HostName = hostNameOrAddress,
                Aliases = new string[0],
                AddressList = addresses
            };
        }

        public static string GetHostName() => "localhost";

        private static unsafe IPAddress[] ResolveHostName(string hostName)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                throw new Exception("The network is unavailable.");

            EFI_HANDLE serviceHandle = default;
            EFI_HANDLE childHandle = default;
            EFI_SERVICE_BINDING* serviceBinding = null;
            EFI_DNS4_PROTOCOL* dns = null;
            EFI_EVENT completionEvent = default;
            DNS_HOST_TO_ADDR_DATA* response = null;
            bool childCreated = false;
            bool dnsOpened = false;
            bool configured = false;

            try
            {
                OpenDnsProtocol(
                    &serviceHandle,
                    &childHandle,
                    &serviceBinding,
                    &dns,
                    &childCreated,
                    &dnsOpened);

                //
                // Always use Google Public DNS. Some DHCP servers do not provide option 6,
                // and some firmware does not expose EFI_IP4_CONFIG2_PROTOCOL.
                // https://github.com/nifanfa/udk/blob/184fd25cffe9c7118d93398c3046c713dfec6ea7/NetworkPkg/HttpDxe/HttpDns.c#L57
                //
                EFI_IPv4_ADDRESS dnsServer = default;
                dnsServer.Addr[0] = 8;
                dnsServer.Addr[1] = 8;
                dnsServer.Addr[2] = 8;
                dnsServer.Addr[3] = 8;

                EFI_DNS4_CONFIG_DATA config = new EFI_DNS4_CONFIG_DATA
                {
                    DnsServerListCount = 1,
                    DnsServerList = &dnsServer,
                    UseDefaultSetting = true,
                    EnableDnsCache = true,
                    Protocol = IpProtocolUdp,
                    RetryCount = RetryCount,
                    RetryInterval = RetryIntervalSeconds
                };
                EFI_STATUS status = dns->Configure(dns, &config);
                ThrowIfError(status, "configure EFI DNS4");
                configured = true;

                bool complete = false;
                EFI_DNS4_COMPLETION_TOKEN token = new EFI_DNS4_COMPLETION_TOKEN
                {
                    Status = EFI_NOT_READY,
                    RetryCount = RetryCount,
                    RetryInterval = RetryIntervalSeconds
                };
                status = gBS->CreateEvent(
                    EVT_NOTIFY_SIGNAL,
                    TPL_CALLBACK,
                    &OperationCompleted,
                    &complete,
                    &completionEvent);
                ThrowIfError(status, "create the EFI DNS4 completion event");
                token.Event = completionEvent;

                fixed (char* hostNamePointer = &hostName.FirstChar)
                    status = dns->HostNameToIp(dns, hostNamePointer, &token);
                ThrowIfError(status, "submit the EFI DNS4 query");

                while (!complete)
                    dns->Poll(dns);

                response = token.H2AData;
                ThrowIfError(token.Status, "resolve the host name through EFI DNS4");
                if (response == null || response->IpCount == 0 || response->IpList == null)
                    throw new Exception("EFI DNS4 returned no IPv4 addresses.");
                if (response->IpCount > int.MaxValue)
                    throw new Exception("EFI DNS4 returned too many IPv4 addresses.");

                IPAddress[] addresses = new IPAddress[(int)response->IpCount];
                for (int i = 0; i < addresses.Length; i++)
                {
                    EFI_IPv4_ADDRESS* source = response->IpList + i;
                    addresses[i] = new IPAddress(new byte[]
                    {
                        source->Addr[0],
                        source->Addr[1],
                        source->Addr[2],
                        source->Addr[3]
                    });
                }
                return addresses;
            }
            finally
            {
                if ((void*)completionEvent != null)
                    gBS->CloseEvent(completionEvent);
                if (response != null)
                {
                    if (response->IpList != null)
                        gBS->FreePool(response->IpList);
                    gBS->FreePool(response);
                }
                if (configured && dns != null)
                    dns->Configure(dns, null);
                if (dnsOpened)
                    gBS->CloseProtocol(
                        childHandle,
                        (EFI_GUID*)Dns4ProtocolGuid,
                        gImageHandle,
                        default);
                if (childCreated && serviceBinding != null)
                    serviceBinding->DestroyChild(serviceBinding, childHandle);
                if (serviceBinding != null)
                    gBS->CloseProtocol(
                        serviceHandle,
                        (EFI_GUID*)Dns4ServiceBindingProtocolGuid,
                        gImageHandle,
                        default);
            }
        }

        private static unsafe void OpenDnsProtocol(
            EFI_HANDLE* serviceHandle,
            EFI_HANDLE* childHandle,
            EFI_SERVICE_BINDING** serviceBinding,
            EFI_DNS4_PROTOCOL** dns,
            bool* childCreated,
            bool* dnsOpened)
        {
            ulong handleCount = 0;
            EFI_HANDLE* handles = null;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                ByProtocol,
                (EFI_GUID*)Dns4ServiceBindingProtocolGuid,
                null,
                &handleCount,
                &handles);
            if ((ulong)status != EFI_SUCCESS || handleCount == 0)
            {
                if (handles != null)
                    gBS->FreePool(handles);
                throw new Exception("EFI DNS4 is unavailable. Ensure DnsDxe.efi was loaded.");
            }

            *serviceHandle = handles[0];
            status = gBS->OpenProtocol(
                *serviceHandle,
                (EFI_GUID*)Dns4ServiceBindingProtocolGuid,
                (void**)serviceBinding,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            gBS->FreePool(handles);
            ThrowIfError(status, "open the EFI DNS4 service binding");

            status = (*serviceBinding)->CreateChild(*serviceBinding, childHandle);
            ThrowIfError(status, "create an EFI DNS4 child");
            *childCreated = true;

            status = gBS->OpenProtocol(
                *childHandle,
                (EFI_GUID*)Dns4ProtocolGuid,
                (void**)dns,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            ThrowIfError(status, "open the EFI DNS4 protocol");
            *dnsOpened = true;
        }

        [UnmanagedCallersOnly]
        private static unsafe void OperationCompleted(EFI_EVENT eventHandle, void* context)
        {
            *(bool*)context = true;
        }

        private static void ThrowIfError(EFI_STATUS status, string operation)
        {
            ulong value = status;
            if (value != EFI_SUCCESS)
                throw new Exception("Failed to " + operation + " (EFI_STATUS " + value + ").");
        }
    }

    public class IPHostEntry
    {
        public IPAddress[] AddressList { get; set; } = new IPAddress[0];
        public string[] Aliases { get; set; } = new string[0];
        public string HostName { get; set; } = string.Empty;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DNS4_CONFIG_DATA
{
    public ulong DnsServerListCount;
    public EFI_IPv4_ADDRESS* DnsServerList;
    public bool UseDefaultSetting;
    public bool EnableDnsCache;
    public byte Protocol;
    public EFI_IPv4_ADDRESS StationIp;
    public EFI_IPv4_ADDRESS SubnetMask;
    public ushort LocalPort;
    public uint RetryCount;
    public uint RetryInterval;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct DNS_HOST_TO_ADDR_DATA
{
    public uint IpCount;
    public EFI_IPv4_ADDRESS* IpList;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DNS4_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
    public uint RetryCount;
    public uint RetryInterval;
    public DNS_HOST_TO_ADDR_DATA* H2AData;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DNS4_PROTOCOL
{
    public readonly void* GetModeData;
    public readonly delegate* unmanaged<EFI_DNS4_PROTOCOL*, EFI_DNS4_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_DNS4_PROTOCOL*, char*, EFI_DNS4_COMPLETION_TOKEN*, EFI_STATUS> HostNameToIp;
    public readonly void* IpToHostName;
    public readonly void* GeneralLookUp;
    public readonly void* UpdateDnsCache;
    public readonly delegate* unmanaged<EFI_DNS4_PROTOCOL*, EFI_STATUS> Poll;
    public readonly delegate* unmanaged<EFI_DNS4_PROTOCOL*, EFI_DNS4_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
}
