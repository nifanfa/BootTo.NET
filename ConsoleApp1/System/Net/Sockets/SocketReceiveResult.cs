namespace System.Net.Sockets
{
    public sealed class SocketReceiveResult
    {
        internal SocketReceiveResult(int bytesReceived, IPAddress remoteAddress, int remotePort)
        {
            BytesReceived = bytesReceived;
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
        }

        public int BytesReceived { get; }
        public IPAddress RemoteAddress { get; }
        public int RemotePort { get; }
    }
}
