using System.Text;

namespace System.Net
{
    public sealed class IPAddress
    {
        private readonly uint _address;

        public IPAddress(long address)
        {
            if (address < 0 || address > uint.MaxValue)
                throw new ArgumentException("The IPv4 address value must be between 0 and UInt32.MaxValue.");

            _address = (uint)address;
        }

        public IPAddress(byte[] address)
        {
            if (address == null)
                throw new ArgumentNullException("The IPv4 address bytes cannot be null.");
            if (address.Length != 4)
                throw new ArgumentException("An IPv4 address must contain exactly four bytes.");

            _address = (uint)(address[0] |
                ((uint)address[1] << 8) |
                ((uint)address[2] << 16) |
                ((uint)address[3] << 24));
        }

        private IPAddress(uint address)
        {
            _address = address;
        }

        public static readonly IPAddress Any = new IPAddress(0);
        public static readonly IPAddress Loopback = new IPAddress(0x0100007F);
        public static readonly IPAddress Broadcast = new IPAddress(0xFFFFFFFF);
        public static readonly IPAddress None = Broadcast;

        public long Address => _address;

        public byte[] GetAddressBytes()
        {
            return new byte[]
            {
                (byte)_address,
                (byte)(_address >> 8),
                (byte)(_address >> 16),
                (byte)(_address >> 24)
            };
        }

        public static IPAddress Parse(string ipString)
        {
            IPAddress address;
            if (!TryParse(ipString, out address))
                throw new FormatException("The string is not a valid IPv4 address.");
            return address;
        }

        public static bool TryParse(string ipString, out IPAddress address)
        {
            address = null;
            if (string.IsNullOrEmpty(ipString))
                return false;

            uint value = 0;
            int component = 0;
            int componentDigits = 0;
            int componentIndex = 0;

            for (int i = 0; i < ipString.Length; i++)
            {
                char c = ipString[i];
                if (c >= '0' && c <= '9')
                {
                    component = component * 10 + c - '0';
                    if (component > 255)
                        return false;
                    componentDigits++;
                    if (componentDigits > 3)
                        return false;
                    continue;
                }

                if (c != '.' || componentDigits == 0 || componentIndex >= 3)
                    return false;

                value |= (uint)component << (componentIndex * 8);
                component = 0;
                componentDigits = 0;
                componentIndex++;
            }

            if (componentDigits == 0)
                return false;

            if (componentIndex != 3)
                return false;

            value |= (uint)component << 24;
            address = new IPAddress(value);
            return true;
        }

        public override string ToString()
        {
            byte[] bytes = GetAddressBytes();
            StringBuilder result = new StringBuilder(16);
            result.Append(bytes[0].ToString()).Append('.');
            result.Append(bytes[1].ToString()).Append('.');
            result.Append(bytes[2].ToString()).Append('.');
            result.Append(bytes[3].ToString());
            return result.ToString();
        }

        public override bool Equals(object obj)
            => obj is IPAddress && ((IPAddress)obj)._address == _address;

        public override int GetHashCode()
            => unchecked((int)(_address ^ (_address >> 16)));

    }
}
