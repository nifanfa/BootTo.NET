using System.Runtime;

namespace System
{
    public enum SpecialFolder
    {
        Desktop = 0,
        Programs = 2,
        Personal = 5,
        MyDocuments = 5,
        Favorites = 6,
        Startup = 7,
        Recent = 8,
        SendTo = 9,
        StartMenu = 11,
        MyMusic = 13,
        MyVideos = 14,
        DesktopDirectory = 16,
        MyComputer = 17,
        ApplicationData = 26,
        LocalApplicationData = 28,
        CommonApplicationData = 35,
        System = 37,
        ProgramFiles = 38,
        UserProfile = 40,
        CommonProgramFiles = 43,
    }

    [System.Flags]
    public enum SpecialFolderOption
    {
        None = 0,
        DoNotVerify = 16384,
        Create = 32768,
    }

    public enum PlatformID
    {
        Win32S = 0,
        Win32Windows = 1,
        Win32NT = 2,
        WinCE = 3,
        Unix = 4,
        Xbox = 5,
        MacOSX = 6,
        Other = 7,
    }

    public sealed class Version
    {
        public Version(int major, int minor) : this(major, minor, 0, 0) { _build = -1; _revision = -1; }
        public Version(int major, int minor, int build) : this(major, minor, build, 0) { _revision = -1; }
        public Version(int major, int minor, int build, int revision)
        {
            if (major < 0 || minor < 0 || build < -1 || revision < -1)
                throw new ArgumentException("Version components cannot be negative, except Build and Revision may be -1.");
            _major = major;
            _minor = minor;
            _build = build;
            _revision = revision;
        }

        private int _major;
        private int _minor;
        private int _build;
        private int _revision;

        public int Major => _major;
        public int Minor => _minor;
        public int Build => _build;
        public int Revision => _revision;
        public override string ToString()
        {
            if (_build < 0) return string.Concat(_major.ToString(), ".", _minor.ToString());
            if (_revision < 0) return string.Concat(_major.ToString(), ".", _minor.ToString(), ".", _build.ToString());
            return string.Concat(_major.ToString(), ".", _minor.ToString(), ".", _build.ToString(), ".", _revision.ToString());
        }

        public int CompareTo(Version value)
        {
            if (value == null) return 1;
            int result = _major - value._major;
            if (result != 0) return result;
            result = _minor - value._minor;
            if (result != 0) return result;
            result = _build - value._build;
            return result != 0 ? result : _revision - value._revision;
        }

        public bool Equals(Version value) => value != null && CompareTo(value) == 0;
        public override bool Equals(object value) => Equals(value as Version);
        public override int GetHashCode() => _major ^ (_minor << 8) ^ (_build << 16) ^ (_revision << 24);
    }

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

    public static class Environment
    {
        private static string _currentDirectory = "\\";
        private static int _exitCode;

        public static string NewLine => "\r\n";
        public static string CommandLine => string.Empty;
        public static int ExitCode { get => _exitCode; set => _exitCode = value; }
        public static int TickCount => unchecked((int)TickCount64);
        public static long TickCount64 => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        public static int ProcessorCount => 1;
        public static string MachineName => "UEFI";
        public static string UserName => string.Empty;
        public static string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("The current directory cannot be null or empty.");
                _currentDirectory = value;
            }
        }

        public static bool Is64BitProcess => true;
        public static bool Is64BitOperatingSystem => true;
        public static int SystemPageSize => 4096;
        public static long WorkingSet => 0;
        public static OperatingSystem OSVersion { get; } = new OperatingSystem(PlatformID.Other, new Version(1, 0), string.Empty);

        public static string[] GetCommandLineArgs() => new string[0];
        public static string GetEnvironmentVariable(string variable) => null;
        public static string GetFolderPath(SpecialFolder folder) => string.Empty;
        public static string GetFolderPath(SpecialFolder folder, SpecialFolderOption option) => string.Empty;
        public static void Exit(int exitCode) => _exitCode = exitCode;
        public static void FailFast(string message) => InternalCalls.__fail_fast();
    }
}
