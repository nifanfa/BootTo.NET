using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace System.Net
{
    public static class Dns
    {
        private const int DnsPort = 53;
        private const int QueryTimeoutMilliseconds = 5000;
        private const int MaximumResponseSize = 4096;

        private static readonly IPAddress s_nameServer = IPAddress.Parse("8.8.8.8");
        // A monotonically changing ID is sufficient for the single outstanding
        // query used by this compact resolver and avoids pulling Environment's
        // broader platform surface into the NativeAOT image.
        private static int s_nextQueryId = 0x4D53;

        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
            => GetHostAddressesAsync(hostNameOrAddress).GetAwaiter().GetResult();

        public static Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress)
        {
            if (hostNameOrAddress == null)
                throw new ArgumentNullException("The host name cannot be null.");
            if (hostNameOrAddress.Length == 0)
                throw new ArgumentException("The host name cannot be empty.");

            IPAddress address;
            if (IPAddress.TryParse(hostNameOrAddress, out address))
                return Task.FromResult(new IPAddress[] { address });

            return QueryHostAddressesAsync(hostNameOrAddress);
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

        private static async Task<IPAddress[]> QueryHostAddressesAsync(string hostName)
        {
            ushort queryId = unchecked((ushort)++s_nextQueryId);
            byte[] query = BuildQuery(hostName, queryId);
            byte[] response = new byte[MaximumResponseSize];
            Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            bool operationFinished = false;
            bool timedOut = false;

            Task timeout = Task.Delay(QueryTimeoutMilliseconds);
            timeout.AddContinuation(() =>
            {
                if (!operationFinished)
                {
                    timedOut = true;
                    socket.CloseAsync();
                }
            });

            try
            {
                await socket.ConnectAsync(s_nameServer, DnsPort);
                await socket.SendAsync(query);
                SocketReceiveResult result = await socket.ReceiveFromAsync(response);

                if (!result.RemoteAddress.Equals(s_nameServer) || result.RemotePort != DnsPort)
                    throw new Exception("The DNS response came from an unexpected server.");

                return ParseResponse(response, result.BytesReceived, queryId);
            }
            catch (Exception)
            {
                if (timedOut)
                    throw new Exception("The DNS query timed out.");
                throw;
            }
            finally
            {
                operationFinished = true;
                await socket.CloseAsync();
            }
        }

        private static byte[] BuildQuery(string hostName, ushort queryId)
        {
            int hostLength = hostName.Length;
            if (hostLength > 0 && hostName[hostLength - 1] == '.')
                hostLength--;
            if (hostLength == 0 || hostLength > 253)
                throw new ArgumentException("The host name must contain between 1 and 253 characters.");

            int labelLength = 0;
            for (int i = 0; i < hostLength; i++)
            {
                char character = hostName[i];
                if (character == '.')
                {
                    if (labelLength == 0 || labelLength > 63)
                        throw new ArgumentException("Each DNS label must contain between 1 and 63 characters.");
                    labelLength = 0;
                    continue;
                }

                if (character <= ' ' || character > 0x7f)
                    throw new ArgumentException("The host name contains a character that is not valid in DNS.");
                labelLength++;
            }
            if (labelLength == 0 || labelLength > 63)
                throw new ArgumentException("The final DNS label must contain between 1 and 63 characters.");

            int wireNameLength = hostLength + 2;
            if (wireNameLength > 255)
                throw new ArgumentException("The encoded DNS name exceeds the 255-byte protocol limit.");

            byte[] query = new byte[12 + wireNameLength + 4];
            WriteUInt16(query, 0, queryId);
            WriteUInt16(query, 2, 0x0100); // Recursion desired.
            WriteUInt16(query, 4, 1);

            int output = 12;
            int labelStart = 0;
            for (int i = 0; i <= hostLength; i++)
            {
                if (i != hostLength && hostName[i] != '.')
                    continue;

                int length = i - labelStart;
                query[output++] = (byte)length;
                for (int j = labelStart; j < i; j++)
                {
                    char character = hostName[j];
                    if (character >= 'A' && character <= 'Z')
                        character = (char)(character + ('a' - 'A'));
                    query[output++] = (byte)character;
                }
                labelStart = i + 1;
            }
            query[output++] = 0;
            WriteUInt16(query, output, 1); // A
            WriteUInt16(query, output + 2, 1); // IN
            return query;
        }

        private static IPAddress[] ParseResponse(byte[] response, int length, ushort queryId)
        {
            if (response == null || length < 12 || length > response.Length)
                throw new Exception("The DNS response is invalid.");
            if (ReadUInt16(response, 0) != queryId)
                throw new Exception("The DNS response has an unexpected transaction ID.");

            ushort flags = ReadUInt16(response, 2);
            if ((flags & 0x8000) == 0 || (flags & 0x7800) != 0)
                throw new Exception("The DNS response is invalid.");
            if ((flags & 0x0200) != 0)
                throw new Exception("The DNS response was truncated.");

            int responseCode = flags & 0x000f;
            if (responseCode != 0)
                throw new Exception("The DNS server returned error " + responseCode + ".");

            int questionCount = ReadUInt16(response, 4);
            int answerCount = ReadUInt16(response, 6);
            int offset = 12;
            for (int i = 0; i < questionCount; i++)
            {
                offset = SkipName(response, length, offset);
                EnsureAvailable(length, offset, 4);
                offset += 4;
            }

            List<IPAddress> addresses = new List<IPAddress>();
            for (int i = 0; i < answerCount; i++)
            {
                offset = SkipName(response, length, offset);
                EnsureAvailable(length, offset, 10);
                ushort type = ReadUInt16(response, offset);
                ushort dnsClass = ReadUInt16(response, offset + 2);
                int dataLength = ReadUInt16(response, offset + 8);
                offset += 10;
                EnsureAvailable(length, offset, dataLength);

                if (type == 1 && dnsClass == 1 && dataLength == 4)
                {
                    IPAddress address = new IPAddress(new byte[]
                    {
                        response[offset],
                        response[offset + 1],
                        response[offset + 2],
                        response[offset + 3]
                    });
                    if (!Contains(addresses, address))
                        addresses.Add(address);
                }
                offset += dataLength;
            }

            if (addresses.Count == 0)
                throw new Exception("The DNS response did not contain an IPv4 address.");
            return addresses.ToArray();
        }

        private static int SkipName(byte[] message, int length, int offset)
        {
            int labels = 0;
            while (true)
            {
                EnsureAvailable(length, offset, 1);
                int labelLength = message[offset++];
                if (labelLength == 0)
                    return offset;
                if ((labelLength & 0xc0) == 0xc0)
                {
                    EnsureAvailable(length, offset, 1);
                    int pointer = ((labelLength & 0x3f) << 8) | message[offset];
                    if (pointer >= length)
                        throw new Exception("The DNS response contains an invalid name pointer.");
                    return offset + 1;
                }
                if ((labelLength & 0xc0) != 0 || labelLength > 63)
                    throw new Exception("The DNS response contains an invalid name.");
                EnsureAvailable(length, offset, labelLength);
                offset += labelLength;
                if (++labels > 127)
                    throw new Exception("The DNS response contains an invalid name.");
            }
        }

        private static bool Contains(List<IPAddress> addresses, IPAddress address)
        {
            for (int i = 0; i < addresses.Count; i++)
                if (addresses[i].Equals(address))
                    return true;
            return false;
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
            => (ushort)((buffer[offset] << 8) | buffer[offset + 1]);

        private static void WriteUInt16(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value >> 8);
            buffer[offset + 1] = (byte)value;
        }

        private static void EnsureAvailable(int length, int offset, int count)
        {
            if (offset < 0 || count < 0 || offset > length - count)
                throw new Exception("The DNS response is truncated or malformed.");
        }
    }

    public class IPHostEntry
    {
        public IPAddress[] AddressList { get; set; } = new IPAddress[0];
        public string[] Aliases { get; set; } = new string[0];
        public string HostName { get; set; } = string.Empty;
    }
}
