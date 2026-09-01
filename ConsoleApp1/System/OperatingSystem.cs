namespace System
{
    public sealed class OperatingSystem
    {
        private readonly PlatformID _platform;
        private readonly Version _version;
        private readonly string _servicePack;

        public OperatingSystem(PlatformID platform, Version version, string servicePack)
        {
            _platform = platform;
            _version = version ?? throw new ArgumentNullException("The operating system version cannot be null.");
            _servicePack = servicePack ?? string.Empty;
        }

        public PlatformID Platform => _platform;
        public Version Version => _version;
        public string ServicePack => _servicePack;
        public override string ToString() => string.Concat(_platform.ToString(), " ", _version.ToString(), _servicePack);
    }
}
