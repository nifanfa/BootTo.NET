namespace System
{
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
}
