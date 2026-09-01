namespace System.Net
{
    public class IPHostEntry
    {
        public IPAddress[] AddressList { get; set; } = new IPAddress[0];
        public string[] Aliases { get; set; } = new string[0];
        public string HostName { get; set; } = string.Empty;
    }
}
