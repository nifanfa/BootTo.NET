namespace System.Net.Sockets
{
    public sealed class SocketException : Exception
    {
        public SocketException(EFI_STATUS status) : base("The UEFI socket operation failed with status " + (ulong)status + ".")
            => Status = status;

        public SocketException(EFI_STATUS status, string message) : base(message)
            => Status = status;

        public EFI_STATUS Status { get; }
    }
}
