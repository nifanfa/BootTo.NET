using System.Net.Sockets;

namespace System.Net.NetworkInformation
{
    public abstract class NetworkInterface
    {
        public static bool GetIsNetworkAvailable()
        {
            Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Connect(IPAddress.Parse("8.8.8.8"), 53);
            }
            catch
            {
                return false;
            }
            finally
            {
                socket.Close();
            }
            return true;
        }
    }
}
