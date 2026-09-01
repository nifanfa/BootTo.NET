using System.Runtime;

namespace System
{
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
