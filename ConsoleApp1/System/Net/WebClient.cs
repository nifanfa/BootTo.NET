using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace System.Net
{
    // A small, case-insensitive collection for request and response headers.
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
        public WebException(string message) : base(message)
        {
        }

        internal WebException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }

    // HTTP/1.1 client for the firmware networking stack.
    public class WebClient : IDisposable
    {
        private const int ReceiveBufferSize = 8192;
        private const int MaximumHeaderSize = 65536;

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
            try
            {
                return SendRequest("GET", address, null, null);
            }
            finally
            {
                EndOperation();
            }
        }

        public string DownloadString(string address)
        {
            return _encoding.GetString(DownloadData(address));
        }

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

        public Stream OpenRead(string address)
            => new MemoryStream(DownloadData(address));

        public byte[] UploadData(string address, byte[] data)
            => UploadData(address, null, data);

        public byte[] UploadData(string address, string method, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("The upload data cannot be null.");
            if (string.IsNullOrEmpty(method))
                method = "POST";

            BeginOperation();
            try
            {
                return SendRequest(method, address, data, "application/octet-stream");
            }
            finally
            {
                EndOperation();
            }
        }

        public string UploadString(string address, string data)
            => UploadString(address, null, data);

        public string UploadString(string address, string method, string data)
        {
            if (data == null)
                throw new ArgumentNullException("The upload string cannot be null.");
            if (string.IsNullOrEmpty(method))
                method = "POST";

            byte[] response = UploadDataInternal(method, address, _encoding.GetBytes(data), "text/plain; charset=utf-8");
            return _encoding.GetString(response);
        }

        public Task<byte[]> DownloadDataTaskAsync(string address)
        {
            try
            {
                return Task.FromResult(DownloadData(address));
            }
            catch (Exception exception)
            {
                return Task.FromException<byte[]>(exception);
            }
        }

        public Task<string> DownloadStringTaskAsync(string address)
        {
            try
            {
                return Task.FromResult(DownloadString(address));
            }
            catch (Exception exception)
            {
                return Task.FromException<string>(exception);
            }
        }

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
        {
            try
            {
                return Task.FromResult(UploadData(address, method, data));
            }
            catch (Exception exception)
            {
                return Task.FromException<byte[]>(exception);
            }
        }

        public Task<string> UploadStringTaskAsync(string address, string data)
            => UploadStringTaskAsync(address, null, data);

        public Task<string> UploadStringTaskAsync(string address, string method, string data)
        {
            try
            {
                return Task.FromResult(UploadString(address, method, data));
            }
            catch (Exception exception)
            {
                return Task.FromException<string>(exception);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private byte[] UploadDataInternal(string method, string address, byte[] data, string defaultContentType)
        {
            BeginOperation();
            try
            {
                return SendRequest(method, address, data, defaultContentType);
            }
            finally
            {
                EndOperation();
            }
        }

        private byte[] SendRequest(string method, string address, byte[] body, string defaultContentType)
        {
            if (string.IsNullOrEmpty(method) || ContainsControlCharacter(method))
                throw new ArgumentException("The HTTP method must be non-empty and contain no control characters.");

            ParsedUrl url;
            Socket socket = null;
            try
            {
                url = ParseUrl(ResolveAddress(address));
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(url.Address, url.Port);
                byte[] request = BuildRequest(method, url, body, defaultContentType);
                SendAll(socket, request);
                return ReadResponse(socket);
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new WebException("The HTTP request failed: " + exception.Message);
            }
            finally
            {
                if (socket != null)
                    socket.Close();
            }
        }

        private byte[] BuildRequest(string method, ParsedUrl url, byte[] body, string defaultContentType)
        {
            StringBuilder header = new StringBuilder(256);
            header.Append(method).Append(' ').Append(url.PathAndQuery).Append(" HTTP/1.1\r\n");
            header.Append("Host: ").Append(url.Host);
            if (url.Port != 80)
                header.Append(':').Append(url.Port);
            header.Append("\r\n");

            for (int i = 0; i < _headers.Count; i++)
            {
                string name = _headers.GetKey(i);
                if (IsManagedHeader(name))
                    continue;
                header.Append(name).Append(": ").Append(_headers.Get(i)).Append("\r\n");
            }

            if (body != null)
            {
                if (_headers["Content-Type"] == null && defaultContentType != null)
                    header.Append("Content-Type: ").Append(defaultContentType).Append("\r\n");
                header.Append("Content-Length: ").Append(body.Length).Append("\r\n");
            }

            header.Append("Connection: close\r\n\r\n");
            byte[] headerBytes = Encoding.UTF8.GetBytes(header.ToString());
            if (body == null || body.Length == 0)
                return headerBytes;

            byte[] request = new byte[headerBytes.Length + body.Length];
            for (int i = 0; i < headerBytes.Length; i++)
                request[i] = headerBytes[i];
            for (int i = 0; i < body.Length; i++)
                request[headerBytes.Length + i] = body[i];
            return request;
        }

        private static void SendAll(Socket socket, byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                byte[] part;
                if (offset == 0)
                {
                    part = data;
                }
                else
                {
                    int count = data.Length - offset;
                    part = new byte[count];
                    for (int i = 0; i < count; i++)
                        part[i] = data[offset + i];
                }

                int sent = socket.SendAsync(part).GetAwaiter().GetResult();
                if (sent <= 0)
                    throw new IOException("The HTTP request body could not be sent.");
                offset += sent;
            }
        }

        private byte[] ReadResponse(Socket socket)
        {
            ByteBuffer buffer = new ByteBuffer();
            byte[] receiveBuffer = new byte[ReceiveBufferSize];
            int headerEnd;
            while ((headerEnd = buffer.FindHeaderEnd()) < 0)
            {
                if (buffer.Length > MaximumHeaderSize)
                    throw new WebException("The HTTP response headers are too large.");
                int received = socket.Receive(receiveBuffer);
                if (received == 0)
                    throw new WebException("The HTTP response ended before the headers were received.");
                buffer.Append(receiveBuffer, received);
            }

            ResponseInfo response = ParseResponse(buffer.ToArray(0, headerEnd));
            _responseHeaders = response.Headers;
            buffer.Consume(headerEnd + 4);

            if (response.StatusCode < 200 || response.StatusCode >= 300)
                throw new WebException("The HTTP server returned status " + response.StatusCode + ".", response.StatusCode);

            MemoryStream body = new MemoryStream();
            if (response.Chunked)
            {
                ReadChunkedBody(socket, buffer, body, receiveBuffer);
            }
            else if (response.ContentLength >= 0)
            {
                ReadFixedBody(socket, buffer, body, response.ContentLength, receiveBuffer);
            }
            else
            {
                ReadUntilClose(socket, buffer, body, receiveBuffer);
            }
            return body.ToArray();
        }

        private static void ReadFixedBody(Socket socket, ByteBuffer buffer, MemoryStream body, int length, byte[] receiveBuffer)
        {
            int remaining = length;
            while (remaining > 0)
            {
                if (buffer.Length == 0)
                {
                    int received = socket.Receive(receiveBuffer);
                    if (received == 0)
                        throw new WebException("The HTTP response body is shorter than Content-Length.");
                    buffer.Append(receiveBuffer, received);
                }

                int count = remaining < buffer.Length ? remaining : buffer.Length;
                buffer.CopyTo(body, count);
                buffer.Consume(count);
                remaining -= count;
            }
        }

        private static void ReadUntilClose(Socket socket, ByteBuffer buffer, MemoryStream body, byte[] receiveBuffer)
        {
            while (true)
            {
                if (buffer.Length > 0)
                {
                    buffer.CopyTo(body, buffer.Length);
                    buffer.Consume(buffer.Length);
                }

                int received = socket.Receive(receiveBuffer);
                if (received == 0)
                    return;
                buffer.Append(receiveBuffer, received);
            }
        }

        private static void ReadChunkedBody(Socket socket, ByteBuffer buffer, MemoryStream body, byte[] receiveBuffer)
        {
            while (true)
            {
                string line = ReadLine(socket, buffer, receiveBuffer);
                int separator = IndexOf(line, ';', 0);
                if (separator >= 0)
                    line = line.Substring(0, separator);
                int size = ParseHex(line);

                if (size == 0)
                {
                    do
                    {
                        line = ReadLine(socket, buffer, receiveBuffer);
                    }
                    while (line.Length != 0);
                    return;
                }

                int remaining = size;
                while (remaining > 0)
                {
                    if (buffer.Length == 0)
                    {
                        int received = socket.Receive(receiveBuffer);
                        if (received == 0)
                            throw new WebException("The chunked HTTP response ended unexpectedly.");
                        buffer.Append(receiveBuffer, received);
                    }

                    int count = remaining < buffer.Length ? remaining : buffer.Length;
                    buffer.CopyTo(body, count);
                    buffer.Consume(count);
                    remaining -= count;
                }

                EnsureBytes(socket, buffer, 2, receiveBuffer);
                if (buffer.GetByte(0) != '\r' || buffer.GetByte(1) != '\n')
                    throw new WebException("The chunked HTTP response is malformed.");
                buffer.Consume(2);
            }
        }

        private static string ReadLine(Socket socket, ByteBuffer buffer, byte[] receiveBuffer)
        {
            while (true)
            {
                int end = buffer.FindCrlf();
                if (end >= 0)
                {
                    string result = Encoding.UTF8.GetString(buffer.ToArray(0, end));
                    buffer.Consume(end + 2);
                    return result;
                }

                int received = socket.Receive(receiveBuffer);
                if (received == 0)
                    throw new WebException("The chunked HTTP response ended unexpectedly.");
                buffer.Append(receiveBuffer, received);
            }
        }

        private static void EnsureBytes(Socket socket, ByteBuffer buffer, int count, byte[] receiveBuffer)
        {
            while (buffer.Length < count)
            {
                int received = socket.Receive(receiveBuffer);
                if (received == 0)
                    throw new WebException("The chunked HTTP response ended unexpectedly.");
                buffer.Append(receiveBuffer, received);
            }
        }

        private static ResponseInfo ParseResponse(byte[] headerBytes)
        {
            string header = Encoding.UTF8.GetString(headerBytes);
            int firstLineEnd = IndexOf(header, "\r\n", 0);
            if (firstLineEnd < 0)
                throw new WebException("The HTTP response status line is missing.");

            string statusLine = header.Substring(0, firstLineEnd);
            int statusStart = IndexOf(statusLine, ' ', 0);
            if (statusStart < 0)
                throw new WebException("The HTTP response status line is malformed.");
            while (statusStart < statusLine.Length && statusLine[statusStart] == ' ')
                statusStart++;
            int statusCode = ParseDecimal(statusLine, statusStart, 3);
            if (statusCode < 0)
                throw new WebException("The HTTP response status line is malformed.");

            WebHeaderCollection headers = new WebHeaderCollection();
            int contentLength = -1;
            bool hasContentLength = false;
            bool chunked = false;
            int lineStart = firstLineEnd + 2;
            while (lineStart < header.Length)
            {
                int lineEnd = IndexOf(header, "\r\n", lineStart);
                if (lineEnd < 0 || lineEnd == lineStart)
                    break;

                int colon = IndexOf(header, ':', lineStart);
                if (colon < lineStart || colon > lineEnd)
                    throw new WebException("The HTTP response contains a malformed header.");

                string name = header.Substring(lineStart, colon - lineStart);
                string value = Trim(header.Substring(colon + 1, lineEnd - colon - 1));
                headers.Add(name, value);
                if (WebHeaderCollection.EqualsIgnoreCase(name, "Content-Length"))
                {
                    contentLength = ParseDecimal(value, 0, value.Length);
                    hasContentLength = true;
                }
                else if (WebHeaderCollection.EqualsIgnoreCase(name, "Transfer-Encoding") && ContainsToken(value, "chunked"))
                    chunked = true;
                lineStart = lineEnd + 2;
            }

            if (hasContentLength && contentLength < 0)
                throw new WebException("The HTTP response contains an invalid Content-Length.");
            return new ResponseInfo(statusCode, headers, contentLength, chunked);
        }

        private Uri ResolveAddress(string address)
        {
            if (address == null)
                throw new ArgumentNullException("The request address cannot be null.");
            Uri result;
            if (Uri.TryCreate(address, UriKind.Absolute, out result))
                return result;
            if (string.IsNullOrEmpty(_baseAddress))
                throw new NotSupportedException("The address is relative and no base address has been configured.");
            return new Uri(new Uri(_baseAddress, UriKind.Absolute), address);
        }

        private static ParsedUrl ParseUrl(Uri uri)
        {
            ValidateHttpUri(uri);
            string host = uri.Host;
            int port = uri.Port;

            IPAddress ipAddress;
            if (WebHeaderCollection.EqualsIgnoreCase(host, "localhost"))
                ipAddress = IPAddress.Loopback;
            else if (!IPAddress.TryParse(host, out ipAddress))
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                if (addresses.Length == 0)
                    throw new WebException("The host name could not be resolved.");
                ipAddress = addresses[0];
            }
            return new ParsedUrl(ipAddress, host, uri.Authority, port, uri.PathAndQuery);
        }

        private static void ValidateHttpUri(Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri ||
                !WebHeaderCollection.EqualsIgnoreCase(uri.Scheme, Uri.UriSchemeHttp))
                throw new NotSupportedException("Only absolute HTTP URIs are supported.");
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

        private static bool IsManagedHeader(string name)
            => WebHeaderCollection.EqualsIgnoreCase(name, "Host") ||
               WebHeaderCollection.EqualsIgnoreCase(name, "Connection") ||
               WebHeaderCollection.EqualsIgnoreCase(name, "Content-Length") ||
               WebHeaderCollection.EqualsIgnoreCase(name, "Transfer-Encoding");

        private static bool ContainsToken(string value, string token)
        {
            int start = 0;
            while (start < value.Length)
            {
                int comma = IndexOf(value, ',', start);
                int end = comma < 0 ? value.Length : comma;
                if (WebHeaderCollection.EqualsIgnoreCase(Trim(value.Substring(start, end - start)), token))
                    return true;
                start = end + 1;
            }
            return false;
        }

        private static int ParseHex(string value)
        {
            value = Trim(value);
            if (value.Length == 0)
                throw new WebException("The chunked HTTP response contains an invalid chunk size.");

            int result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                int digit;
                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'a' && c <= 'f')
                    digit = c - 'a' + 10;
                else if (c >= 'A' && c <= 'F')
                    digit = c - 'A' + 10;
                else
                    throw new WebException("The chunked HTTP response contains an invalid chunk size.");
                if (result > (int.MaxValue - digit) / 16)
                    throw new WebException("The HTTP response body is too large.");
                result = result * 16 + digit;
            }
            return result;
        }

        private static int ParseDecimal(string value, int start, int count)
        {
            if (value == null || start < 0 || count <= 0 || start > value.Length - count)
                return -1;
            int result = 0;
            for (int i = start; i < start + count; i++)
            {
                char c = value[i];
                if (c < '0' || c > '9' || result > (int.MaxValue - (c - '0')) / 10)
                    return -1;
                result = result * 10 + c - '0';
            }
            return result;
        }

        private static string Trim(string value)
        {
            int start = 0;
            int end = value.Length;
            while (start < end && (value[start] == ' ' || value[start] == '\t'))
                start++;
            while (end > start && (value[end - 1] == ' ' || value[end - 1] == '\t'))
                end--;
            return value.Substring(start, end - start);
        }

        private static bool ContainsControlCharacter(string value)
        {
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\r' || value[i] == '\n' || value[i] == ' ' || value[i] == '\t')
                    return true;
            return false;
        }

        private static int IndexOf(string value, char character, int start)
        {
            for (int i = start; i < value.Length; i++)
                if (value[i] == character)
                    return i;
            return -1;
        }

        private static int IndexOf(string value, string sequence, int start)
        {
            if (sequence.Length == 0)
                return start;
            for (int i = start; i <= value.Length - sequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < sequence.Length; j++)
                    if (value[i + j] != sequence[j])
                    {
                        match = false;
                        break;
                    }
                if (match)
                    return i;
            }
            return -1;
        }

        private sealed class ParsedUrl
        {
            internal readonly IPAddress Address;
            internal readonly string Host;
            internal readonly string Authority;
            internal readonly int Port;
            internal readonly string PathAndQuery;

            internal ParsedUrl(IPAddress address, string host, string authority, int port, string pathAndQuery)
            {
                Address = address;
                Host = host;
                Authority = authority;
                Port = port;
                PathAndQuery = pathAndQuery;
            }
        }

        private sealed class ResponseInfo
        {
            internal readonly int StatusCode;
            internal readonly WebHeaderCollection Headers;
            internal readonly int ContentLength;
            internal readonly bool Chunked;

            internal ResponseInfo(int statusCode, WebHeaderCollection headers, int contentLength, bool chunked)
            {
                StatusCode = statusCode;
                Headers = headers;
                ContentLength = contentLength;
                Chunked = chunked;
            }
        }

        private sealed class ByteBuffer
        {
            private byte[] _buffer = new byte[ReceiveBufferSize];
            private int _offset;
            private int _length;

            internal int Length => _length;

            internal void Append(byte[] source, int count)
            {
                if (count <= 0)
                    return;
                EnsureCapacity(_length + count);
                for (int i = 0; i < count; i++)
                    _buffer[_offset + _length + i] = source[i];
                _length += count;
            }

            internal int FindHeaderEnd()
            {
                for (int i = 0; i <= _length - 4; i++)
                    if (_buffer[_offset + i] == '\r' && _buffer[_offset + i + 1] == '\n' &&
                        _buffer[_offset + i + 2] == '\r' && _buffer[_offset + i + 3] == '\n')
                        return i;
                return -1;
            }

            internal int FindCrlf()
            {
                for (int i = 0; i <= _length - 2; i++)
                    if (_buffer[_offset + i] == '\r' && _buffer[_offset + i + 1] == '\n')
                        return i;
                return -1;
            }

            internal byte GetByte(int index)
            {
                if ((uint)index >= (uint)_length)
                    throw new ArgumentException("The response buffer index is outside the available data.");
                return _buffer[_offset + index];
            }

            internal byte[] ToArray(int start, int count)
            {
                if (start < 0 || count < 0 || start > _length - count)
                    throw new ArgumentException("The response buffer range is invalid.");
                byte[] result = new byte[count];
                for (int i = 0; i < count; i++)
                    result[i] = _buffer[_offset + start + i];
                return result;
            }

            internal void CopyTo(MemoryStream destination, int count)
            {
                if (count < 0 || count > _length)
                    throw new ArgumentException("The requested response copy length is invalid.");
                destination.Write(_buffer, _offset, count);
            }

            internal void Consume(int count)
            {
                if (count < 0 || count > _length)
                    throw new ArgumentException("The response consume length is invalid.");
                _offset += count;
                _length -= count;
                if (_length == 0)
                    _offset = 0;
                else if (_offset > _buffer.Length / 2)
                    Compact();
            }

            private void EnsureCapacity(int required)
            {
                if (_offset + required <= _buffer.Length)
                    return;
                Compact();
                if (required <= _buffer.Length)
                    return;
                int capacity = _buffer.Length * 2;
                if (capacity < required)
                    capacity = required;
                byte[] resized = new byte[capacity];
                for (int i = 0; i < _length; i++)
                    resized[i] = _buffer[_offset + i];
                _buffer = resized;
                _offset = 0;
            }

            private void Compact()
            {
                for (int i = 0; i < _length; i++)
                    _buffer[i] = _buffer[_offset + i];
                _offset = 0;
            }
        }
    }
}
