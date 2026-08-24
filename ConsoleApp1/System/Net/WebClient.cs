using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    public sealed class WebHeaderCollection
    {
        private sealed class Header
        {
            internal readonly string Name;
            internal string Value;

            internal Header(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        private readonly List<Header> _headers = new List<Header>();
        public int Count => _headers.Count;

        public string this[string name]
        {
            get => Get(name);
            set => Set(name, value);
        }

        public void Add(string name, string value)
        {
            Validate(name, value);
            int index = Find(name);
            if (index >= 0)
                _headers[index].Value = _headers[index].Value + ", " + value;
            else
                _headers.Add(new Header(name, value));
        }

        public void Set(string name, string value)
        {
            Validate(name, value);
            int index = Find(name);
            if (index >= 0)
                _headers[index].Value = value;
            else
                _headers.Add(new Header(name, value));
        }

        public string Get(string name)
        {
            if (name == null)
                throw new ArgumentNullException("The header name cannot be null.");
            int index = Find(name);
            return index < 0 ? null : _headers[index].Value;
        }

        public string GetKey(int index)
        {
            if ((uint)index >= (uint)_headers.Count)
                throw new ArgumentException("The header index is outside the collection.");
            return _headers[index].Name;
        }

        public string Get(int index)
        {
            if ((uint)index >= (uint)_headers.Count)
                throw new ArgumentException("The header index is outside the collection.");
            return _headers[index].Value;
        }

        public bool Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException("The header name cannot be null.");
            int index = Find(name);
            if (index < 0)
                return false;
            _headers.RemoveAt(index);
            return true;
        }

        public void Clear() => _headers.Clear();

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < _headers.Count; i++)
                result.Append(_headers[i].Name).Append(": ").Append(_headers[i].Value).Append("\r\n");
            return result.ToString();
        }

        private int Find(string name)
        {
            for (int i = 0; i < _headers.Count; i++)
                if (EqualsIgnoreCase(_headers[i].Name, name))
                    return i;
            return -1;
        }

        private static void Validate(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || value == null)
                throw new ArgumentException("The header name must be non-empty and the value cannot be null.");

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c <= ' ' || c == ':' || c == '\r' || c == '\n')
                    throw new ArgumentException("The header name contains an invalid character.");
            }
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\r' || value[i] == '\n')
                    throw new ArgumentException("The header value cannot contain CR or LF characters.");
        }

        internal static bool EqualsIgnoreCase(string left, string right)
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
    }

    public class WebException : Exception
    {
        public WebException(string message) : base(message) { }

        internal WebException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }

    public class WebClient : IDisposable
    {
        private WebHeaderCollection _headers = new WebHeaderCollection();
        private Encoding _encoding = Encoding.UTF8;
        private string _baseAddress;
        private WebHeaderCollection _responseHeaders;
        private bool _busy;
        private bool _disposed;

        public Encoding Encoding
        {
            get => _encoding;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("The response encoding cannot be null.");
                _encoding = value;
            }
        }

        public string BaseAddress
        {
            get => _baseAddress ?? string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _baseAddress = null;
                    return;
                }
                Uri uri = new Uri(value, UriKind.Absolute);
                ValidateHttpUri(uri);
                _baseAddress = uri.AbsoluteUri;
            }
        }

        public WebHeaderCollection Headers
        {
            get => _headers;
            set => _headers = value ?? new WebHeaderCollection();
        }

        public WebHeaderCollection ResponseHeaders => _responseHeaders;
        public bool IsBusy => _busy;

        public byte[] DownloadData(string address)
        {
            BeginOperation();
            try { return SendRequest("GET", address, null, null); }
            finally { EndOperation(); }
        }

        public string DownloadString(string address)
            => _encoding.GetString(DownloadData(address));

        public void DownloadFile(string address, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("The destination file name cannot be null.");
            byte[] data = DownloadData(address);
            FileStream stream = null;
            try
            {
                stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.Write(data, 0, data.Length);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        public Stream OpenRead(string address) => new MemoryStream(DownloadData(address));

        public byte[] UploadData(string address, byte[] data) => UploadData(address, null, data);

        public byte[] UploadData(string address, string method, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("The upload data cannot be null.");
            if (string.IsNullOrEmpty(method))
                method = "POST";
            return UploadDataInternal(method, address, data, "application/octet-stream");
        }

        public string UploadString(string address, string data) => UploadString(address, null, data);

        public string UploadString(string address, string method, string data)
        {
            if (data == null)
                throw new ArgumentNullException("The upload string cannot be null.");
            if (string.IsNullOrEmpty(method))
                method = "POST";
            byte[] response = UploadDataInternal(
                method, address, _encoding.GetBytes(data), "text/plain; charset=utf-8");
            return _encoding.GetString(response);
        }

        public Task<byte[]> DownloadDataTaskAsync(string address) => Complete(() => DownloadData(address));
        public Task<string> DownloadStringTaskAsync(string address) => Complete(() => DownloadString(address));

        public Task DownloadFileTaskAsync(string address, string fileName)
        {
            try
            {
                DownloadFile(address, fileName);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }

        public Task<byte[]> UploadDataTaskAsync(string address, byte[] data)
            => UploadDataTaskAsync(address, null, data);

        public Task<byte[]> UploadDataTaskAsync(string address, string method, byte[] data)
            => Complete(() => UploadData(address, method, data));

        public Task<string> UploadStringTaskAsync(string address, string data)
            => UploadStringTaskAsync(address, null, data);

        public Task<string> UploadStringTaskAsync(string address, string method, string data)
            => Complete(() => UploadString(address, method, data));

        public void Dispose() => _disposed = true;

        private byte[] UploadDataInternal(string method, string address, byte[] data, string defaultContentType)
        {
            BeginOperation();
            try { return SendRequest(method, address, data, defaultContentType); }
            finally { EndOperation(); }
        }

        private byte[] SendRequest(string method, string address, byte[] body, string defaultContentType)
        {
            if (string.IsNullOrEmpty(method) || ContainsControlCharacter(method))
                throw new ArgumentException("The HTTP method must be non-empty and contain no control characters.");

            Uri uri = ResolveAddress(address);
            ValidateHttpUri(uri);
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                    throw new Exception("The network is unavailable.");

                HttpResult result = Http.Send(method, uri, _headers, body, defaultContentType);
                _responseHeaders = result.Headers;
                if (result.StatusCode < 200 || result.StatusCode >= 300)
                    throw new WebException(
                        "The HTTP server returned status " + result.StatusCode + ".", result.StatusCode);
                return result.Body;
            }
            catch (WebException) { throw; }
            catch (Exception exception)
            {
                throw new WebException("The HTTP request failed: " + exception.Message);
            }
        }

        private Uri ResolveAddress(string address)
        {
            if (address == null)
                throw new ArgumentNullException("The request address cannot be null.");
            if (Uri.TryCreate(address, UriKind.Absolute, out Uri result))
                return result;
            if (string.IsNullOrEmpty(_baseAddress))
                throw new NotSupportedException("The address is relative and no base address has been configured.");
            return new Uri(new Uri(_baseAddress, UriKind.Absolute), address);
        }

        private static void ValidateHttpUri(Uri uri)
        {
            bool http = uri != null && WebHeaderCollection.EqualsIgnoreCase(uri.Scheme, Uri.UriSchemeHttp);
            bool https = uri != null && WebHeaderCollection.EqualsIgnoreCase(uri.Scheme, Uri.UriSchemeHttps);
            if (uri == null || !uri.IsAbsoluteUri || (!http && !https))
                throw new NotSupportedException("Only absolute HTTP and HTTPS URIs are supported.");
            if (uri.UserInfo.Length != 0 || uri.HostNameType == UriHostNameType.IPv6 || uri.Port < 1)
                throw new NotSupportedException("The URI contains unsupported user info, IPv6, or port settings.");
        }

        private void BeginOperation()
        {
            if (_disposed)
                throw new InvalidOperationException("The WebClient has already been disposed.");
            if (_busy)
                throw new NotSupportedException("Another WebClient operation is already in progress.");
            _busy = true;
            _responseHeaders = null;
        }

        private void EndOperation() => _busy = false;

        private static bool ContainsControlCharacter(string value)
        {
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\r' || value[i] == '\n' || value[i] == ' ' || value[i] == '\t')
                    return true;
            return false;
        }

        private static Task<T> Complete<T>(Func<T> operation)
        {
            try { return Task.FromResult(operation()); }
            catch (Exception exception) { return Task.FromException<T>(exception); }
        }
    }
}
