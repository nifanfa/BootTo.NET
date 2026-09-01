using System.Collections.Generic;
using System.Text;

namespace System
{
    // Compact URI implementation for the firmware networking stack.
    public class Uri
    {
        public static readonly string UriSchemeFile = "file";
        public static readonly string UriSchemeFtp = "ftp";
        public static readonly string UriSchemeHttp = "http";
        public static readonly string UriSchemeHttps = "https";
        public static readonly string UriSchemeWs = "ws";
        public static readonly string UriSchemeWss = "wss";
        public static readonly string SchemeDelimiter = "://";

        private string _originalString;
        private bool _isAbsolute;
        private string _scheme;
        private string _userInfo;
        private string _host;
        private UriHostNameType _hostNameType;
        private int _port;
        private int _defaultPort;
        private bool _explicitPort;
        private string _absolutePath;
        private string _query;
        private string _fragment;
        private string _absoluteUri;

        public Uri(string uriString)
            : this(uriString, UriKind.Absolute)
        {
        }

        public Uri(string uriString, UriKind uriKind)
        {
            if (uriString == null)
                throw new ArgumentNullException("The URI string cannot be null.");
            if (uriKind != UriKind.RelativeOrAbsolute && uriKind != UriKind.Absolute && uriKind != UriKind.Relative)
                throw new ArgumentException("The URI kind value is not valid.");

            _originalString = uriString;
            bool looksAbsolute = HasScheme(uriString);
            if (uriKind == UriKind.Relative || (uriKind == UriKind.RelativeOrAbsolute && !looksAbsolute))
            {
                if (uriKind == UriKind.Relative && looksAbsolute)
                    throw new UriFormatException("A relative URI cannot be created from an absolute URI.");
                _isAbsolute = false;
                return;
            }
            if (!looksAbsolute)
                throw new UriFormatException("The absolute URI is missing a valid scheme.");

            ParseAbsolute(uriString);
        }

        public Uri(Uri baseUri, string relativeUri)
        {
            if (baseUri == null)
                throw new ArgumentNullException("The base URI cannot be null.");
            if (!baseUri.IsAbsoluteUri)
                throw new ArgumentException("The base URI must be absolute.");

            string combined = Combine(baseUri, relativeUri ?? string.Empty);
            _originalString = combined;
            ParseAbsolute(combined);
        }

        public Uri(Uri baseUri, Uri relativeUri)
            : this(baseUri, relativeUri == null ? null : relativeUri.OriginalString)
        {
        }

        public string AbsolutePath
        {
            get
            {
                EnsureAbsolute();
                return _absolutePath;
            }
        }

        public string AbsoluteUri
        {
            get
            {
                EnsureAbsolute();
                return _absoluteUri;
            }
        }

        public string Authority
        {
            get
            {
                EnsureAbsolute();
                StringBuilder result = new StringBuilder();
                result.Append(_host);
                if (_explicitPort && _port != _defaultPort)
                    result.Append(':').Append(_port);
                return result.ToString();
            }
        }

        public string DnsSafeHost
        {
            get
            {
                EnsureAbsolute();
                return _hostNameType == UriHostNameType.IPv6 && _host.Length >= 2
                    ? _host.Substring(1, _host.Length - 2)
                    : _host;
            }
        }

        public string Fragment
        {
            get
            {
                EnsureAbsolute();
                return _fragment;
            }
        }

        public string Host
        {
            get
            {
                EnsureAbsolute();
                return _host;
            }
        }

        public UriHostNameType HostNameType
        {
            get
            {
                EnsureAbsolute();
                return _hostNameType;
            }
        }

        public string IdnHost => DnsSafeHost;

        public bool IsAbsoluteUri => _isAbsolute;

        public bool IsDefaultPort
        {
            get
            {
                EnsureAbsolute();
                return !_explicitPort || _port == _defaultPort;
            }
        }

        public bool IsFile
        {
            get
            {
                EnsureAbsolute();
                return EqualsIgnoreCase(_scheme, UriSchemeFile);
            }
        }

        public bool IsLoopback
        {
            get
            {
                EnsureAbsolute();
                return EqualsIgnoreCase(_host, "localhost") || _host == "127.0.0.1";
            }
        }

        public string LocalPath => AbsolutePath;

        public string OriginalString => _originalString;

        public string PathAndQuery
        {
            get
            {
                EnsureAbsolute();
                return _absolutePath + _query;
            }
        }

        public int Port
        {
            get
            {
                EnsureAbsolute();
                return _port;
            }
        }

        public string Query
        {
            get
            {
                EnsureAbsolute();
                return _query;
            }
        }

        public string Scheme
        {
            get
            {
                EnsureAbsolute();
                return _scheme;
            }
        }

        public string UserInfo
        {
            get
            {
                EnsureAbsolute();
                return _userInfo;
            }
        }

        public static bool TryCreate(string uriString, UriKind uriKind, out Uri result)
        {
            result = null;
            if (uriString == null)
                return false;
            try
            {
                result = new Uri(uriString, uriKind);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static bool TryCreate(Uri baseUri, string relativeUri, out Uri result)
        {
            result = null;
            if (baseUri == null)
                return false;
            try
            {
                result = new Uri(baseUri, relativeUri);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static bool TryCreate(Uri baseUri, Uri relativeUri, out Uri result)
            => TryCreate(baseUri, relativeUri == null ? null : relativeUri.OriginalString, out result);

        public static bool CheckSchemeName(string schemeName)
        {
            if (string.IsNullOrEmpty(schemeName) || !IsAsciiLetter(schemeName[0]))
                return false;
            for (int i = 1; i < schemeName.Length; i++)
            {
                char character = schemeName[i];
                if (!IsAsciiLetter(character) && !IsAsciiDigit(character) &&
                    character != '+' && character != '-' && character != '.')
                    return false;
            }
            return true;
        }

        public static UriHostNameType CheckHostName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return UriHostNameType.Unknown;
            if (name[0] == '[' && name[name.Length - 1] == ']')
                return UriHostNameType.IPv6;

            bool couldBeIPv4 = true;
            bool couldBeDns = true;
            for (int i = 0; i < name.Length; i++)
            {
                char character = name[i];
                if (!IsAsciiDigit(character) && character != '.')
                    couldBeIPv4 = false;
                if (!IsAsciiLetter(character) && !IsAsciiDigit(character) &&
                    character != '.' && character != '-' && character != '_')
                    couldBeDns = false;
            }
            if (couldBeIPv4 && Net.IPAddress.TryParse(name, out _))
                return UriHostNameType.IPv4;
            return couldBeDns ? UriHostNameType.Dns : UriHostNameType.Basic;
        }

        public string GetLeftPart(UriPartial part)
        {
            EnsureAbsolute();
            if (part == UriPartial.Scheme)
                return _scheme + "://";

            string authority = BuildFullAuthority();
            if (part == UriPartial.Authority)
                return _scheme + "://" + authority;
            if (part == UriPartial.Path)
                return _scheme + "://" + authority + _absolutePath;
            if (part == UriPartial.Query)
                return _scheme + "://" + authority + _absolutePath + _query;
            throw new ArgumentException("The URI partial value is not supported.");
        }

        public override string ToString() => _isAbsolute ? _absoluteUri : _originalString;

        public override bool Equals(object obj)
        {
            Uri other = obj as Uri;
            if (other == null || _isAbsolute != other._isAbsolute)
                return false;
            return (_isAbsolute ? _absoluteUri : _originalString) ==
                (other._isAbsolute ? other._absoluteUri : other._originalString);
        }

        public override int GetHashCode()
            => (_isAbsolute ? _absoluteUri : _originalString).GetHashCode();

        public static bool operator ==(Uri left, Uri right)
        {
            if (ReferenceEquals(left, right))
                return true;
            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(Uri left, Uri right) => !(left == right);

        private void ParseAbsolute(string value)
        {
            int schemeEnd = IndexOf(value, ':', 0);
            if (schemeEnd <= 0)
                throw new UriFormatException("The absolute URI is missing a valid scheme.");

            string scheme = value.Substring(0, schemeEnd);
            if (!CheckSchemeName(scheme))
                throw new UriFormatException("Invalid URI scheme.");
            _scheme = LowerAscii(scheme);
            _defaultPort = GetDefaultPort(_scheme);

            int authorityStart = schemeEnd + 1;
            if (authorityStart + 1 >= value.Length || value[authorityStart] != '/' || value[authorityStart + 1] != '/')
                throw new UriFormatException("This URI implementation requires an authority.");
            authorityStart += 2;
            int authorityEnd = FindFirst(value, authorityStart, '/', '?', '#');
            if (authorityEnd == authorityStart)
                throw new UriFormatException("The URI host is empty.");

            ParseAuthority(value.Substring(authorityStart, authorityEnd - authorityStart));

            int queryStart = IndexOf(value, '?', authorityEnd);
            int fragmentStart = IndexOf(value, '#', authorityEnd);
            int pathEnd = value.Length;
            if (queryStart >= 0 && queryStart < pathEnd)
                pathEnd = queryStart;
            if (fragmentStart >= 0 && fragmentStart < pathEnd)
                pathEnd = fragmentStart;

            string path = value.Substring(authorityEnd, pathEnd - authorityEnd);
            _absolutePath = path.Length == 0 ? "/" : NormalizePath(path);
            if (_absolutePath[0] != '/')
                _absolutePath = "/" + _absolutePath;

            int queryEnd = fragmentStart >= 0 ? fragmentStart : value.Length;
            _query = queryStart >= 0 && queryStart < queryEnd
                ? value.Substring(queryStart, queryEnd - queryStart)
                : string.Empty;
            _fragment = fragmentStart >= 0 ? value.Substring(fragmentStart) : string.Empty;
            ValidateTail(_absolutePath);
            ValidateTail(_query);
            ValidateTail(_fragment);

            _isAbsolute = true;
            _absoluteUri = _scheme + "://" + BuildFullAuthority() + _absolutePath + _query + _fragment;
        }

        private void ParseAuthority(string authority)
        {
            int at = LastIndexOf(authority, '@');
            string hostAndPort;
            if (at >= 0)
            {
                _userInfo = authority.Substring(0, at);
                hostAndPort = authority.Substring(at + 1);
            }
            else
            {
                _userInfo = string.Empty;
                hostAndPort = authority;
            }

            if (hostAndPort.Length == 0)
                throw new UriFormatException("The URI host is empty.");

            string host;
            string portText = null;
            if (hostAndPort[0] == '[')
            {
                int closeBracket = IndexOf(hostAndPort, ']', 1);
                if (closeBracket < 0)
                    throw new UriFormatException("Invalid IPv6 host.");
                host = hostAndPort.Substring(0, closeBracket + 1);
                if (closeBracket + 1 < hostAndPort.Length)
                {
                    if (hostAndPort[closeBracket + 1] != ':')
                        throw new UriFormatException("Unexpected characters follow the IPv6 host.");
                    portText = hostAndPort.Substring(closeBracket + 2);
                }
            }
            else
            {
                int colon = LastIndexOf(hostAndPort, ':');
                if (colon >= 0)
                {
                    if (IndexOf(hostAndPort, ':', 0) != colon)
                        throw new UriFormatException("IPv6 hosts must be enclosed in brackets.");
                    host = hostAndPort.Substring(0, colon);
                    portText = hostAndPort.Substring(colon + 1);
                }
                else
                {
                    host = hostAndPort;
                }
            }

            if (host.Length == 0)
                throw new UriFormatException("The URI host is empty.");
            for (int i = 0; i < host.Length; i++)
                if (host[i] <= ' ' || host[i] == '/' || host[i] == '\\' || host[i] == '?' || host[i] == '#')
                    throw new UriFormatException("Invalid URI host.");

            _host = LowerAscii(host);
            _hostNameType = CheckHostName(_host);
            _explicitPort = portText != null;
            if (_explicitPort)
            {
                _port = ParsePort(portText);
            }
            else
            {
                _port = _defaultPort;
            }
        }

        private string BuildFullAuthority()
        {
            StringBuilder result = new StringBuilder();
            if (_userInfo.Length != 0)
                result.Append(_userInfo).Append('@');
            result.Append(_host);
            if (_explicitPort && _port != _defaultPort)
                result.Append(':').Append(_port);
            return result.ToString();
        }

        private static string Combine(Uri baseUri, string relative)
        {
            if (HasScheme(relative))
                return new Uri(relative, UriKind.Absolute).AbsoluteUri;

            string prefix = baseUri.Scheme + "://" + baseUri.BuildFullAuthority();
            if (relative.Length >= 2 && relative[0] == '/' && relative[1] == '/')
                return baseUri.Scheme + ":" + relative;
            if (relative.Length == 0)
                return baseUri.GetLeftPart(UriPartial.Query);
            if (relative[0] == '#')
                return baseUri.GetLeftPart(UriPartial.Query) + relative;
            if (relative[0] == '?')
                return baseUri.GetLeftPart(UriPartial.Path) + relative;

            int suffixStart = FindFirst(relative, 0, '?', '#', '\0');
            string relativePath = relative.Substring(0, suffixStart);
            string suffix = relative.Substring(suffixStart);
            string path;
            if (relativePath.Length > 0 && relativePath[0] == '/')
            {
                path = relativePath;
            }
            else
            {
                string basePath = baseUri.AbsolutePath;
                int slash = LastIndexOf(basePath, '/');
                path = basePath.Substring(0, slash + 1) + relativePath;
            }
            return prefix + NormalizePath(path) + suffix;
        }

        private static string NormalizePath(string path)
        {
            bool rooted = path.Length > 0 && path[0] == '/';
            bool trailingSlash = path.Length > 1 && path[path.Length - 1] == '/';
            List<string> segments = new List<string>();
            int start = rooted ? 1 : 0;
            for (int i = start; i <= path.Length; i++)
            {
                if (i != path.Length && path[i] != '/')
                    continue;
                string segment = path.Substring(start, i - start);
                if (segment == "..")
                {
                    if (segments.Count > 0)
                        segments.RemoveAt(segments.Count - 1);
                }
                else if (segment.Length != 0 && segment != ".")
                {
                    segments.Add(segment);
                }
                start = i + 1;
            }

            StringBuilder result = new StringBuilder();
            if (rooted)
                result.Append('/');
            for (int i = 0; i < segments.Count; i++)
            {
                if (i > 0)
                    result.Append('/');
                result.Append(segments[i]);
            }
            if (trailingSlash && result.Length > 0 && result[result.Length - 1] != '/')
                result.Append('/');
            return result.Length == 0 && rooted ? "/" : result.ToString();
        }

        private static bool HasScheme(string value)
        {
            if (string.IsNullOrEmpty(value) || !IsAsciiLetter(value[0]))
                return false;
            for (int i = 1; i < value.Length; i++)
            {
                char character = value[i];
                if (character == ':')
                    return true;
                if (!IsAsciiLetter(character) && !IsAsciiDigit(character) &&
                    character != '+' && character != '-' && character != '.')
                    return false;
            }
            return false;
        }

        private static int GetDefaultPort(string scheme)
        {
            if (EqualsIgnoreCase(scheme, UriSchemeHttp) || EqualsIgnoreCase(scheme, UriSchemeWs))
                return 80;
            if (EqualsIgnoreCase(scheme, UriSchemeHttps) || EqualsIgnoreCase(scheme, UriSchemeWss))
                return 443;
            if (EqualsIgnoreCase(scheme, UriSchemeFtp))
                return 21;
            return -1;
        }

        private static int ParsePort(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new UriFormatException("Invalid URI port.");
            int port = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (!IsAsciiDigit(value[i]))
                    throw new UriFormatException("Invalid URI port.");
                port = port * 10 + value[i] - '0';
                if (port > 65535)
                    throw new UriFormatException("Invalid URI port.");
            }
            return port;
        }

        private static void ValidateTail(string value)
        {
            for (int i = 0; i < value.Length; i++)
                if (value[i] <= ' ')
                    throw new UriFormatException("The URI contains a control character.");
        }

        private void EnsureAbsolute()
        {
            if (!_isAbsolute)
                throw new InvalidOperationException("This operation requires an absolute URI.");
        }

        private static string LowerAscii(string value)
        {
            StringBuilder result = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character >= 'A' && character <= 'Z')
                    character = (char)(character + ('a' - 'A'));
                result.Append(character);
            }
            return result.ToString();
        }

        private static bool EqualsIgnoreCase(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
            {
                char a = left[i];
                char b = right[i];
                if (a >= 'A' && a <= 'Z')
                    a = (char)(a + ('a' - 'A'));
                if (b >= 'A' && b <= 'Z')
                    b = (char)(b + ('a' - 'A'));
                if (a != b)
                    return false;
            }
            return true;
        }

        private static bool IsAsciiLetter(char character)
            => (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');

        private static bool IsAsciiDigit(char character) => character >= '0' && character <= '9';

        private static int IndexOf(string value, char character, int start)
        {
            for (int i = start; i < value.Length; i++)
                if (value[i] == character)
                    return i;
            return -1;
        }

        private static int LastIndexOf(string value, char character)
        {
            for (int i = value.Length - 1; i >= 0; i--)
                if (value[i] == character)
                    return i;
            return -1;
        }

        private static int FindFirst(string value, int start, char first, char second, char third)
        {
            for (int i = start; i < value.Length; i++)
                if (value[i] == first || value[i] == second || value[i] == third)
                    return i;
            return value.Length;
        }
    }
}
