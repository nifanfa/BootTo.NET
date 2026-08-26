using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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

    public unsafe class WebClient : IDisposable
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

                HttpResult result = Send(method, uri, _headers, body, defaultContentType);
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

        private sealed class HttpResult
        {
            internal readonly int StatusCode;
            internal readonly WebHeaderCollection Headers;
            internal readonly byte[] Body;

            internal HttpResult(int statusCode, WebHeaderCollection headers, byte[] body)
            {
                StatusCode = statusCode;
                Headers = headers;
                Body = body;
            }
        }

        private const int ReceiveBufferSize = 8192;
        private const uint RequestTimeoutMilliseconds = 30000;
        private const ulong EfiHttpError = 0x8000000000000000 | 35;

        private static EFI_GUID HttpServiceBindingProtocolGuid => new EFI_GUID(
            0xbdc8e6af, 0xd9bc, 0x4379, 0xa7, 0x2a, 0xe0, 0xc4, 0xe7, 0x5d, 0xae, 0x1c);

        private static EFI_GUID HttpProtocolGuid => new EFI_GUID(
            0x7a59b29b, 0x910b, 0x4171, 0x82, 0x42, 0xa8, 0x5a, 0x0d, 0xf2, 0x5b, 0x5b);

        private static HttpResult Send(
            string method,
            Uri uri,
            WebHeaderCollection requestHeaders,
            byte[] requestBody,
            string defaultContentType)
        {
            EFI_HTTP_METHOD httpMethod = ParseMethod(method);
            EFI_HANDLE serviceHandle = default;
            EFI_HANDLE childHandle = default;
            EFI_SERVICE_BINDING* serviceBinding = null;
            EFI_HTTP_PROTOCOL* http = null;
            bool childCreated = false;
            bool httpOpened = false;
            bool configured = false;

            try
            {
                OpenProtocol(
                    &serviceHandle,
                    &childHandle,
                    &serviceBinding,
                    &http,
                    &childCreated,
                    &httpOpened);

                EFI_HTTPv4_ACCESS_POINT accessPoint = new EFI_HTTPv4_ACCESS_POINT
                {
                    UseDefaultAddress = true
                };
                EFI_HTTP_CONFIG_DATA config = new EFI_HTTP_CONFIG_DATA
                {
                    HttpVersion = EFI_HTTP_VERSION.HttpVersion11,
                    TimeOutMillisec = RequestTimeoutMilliseconds,
                    LocalAddressIsIPv6 = false,
                    IPv4Node = &accessPoint
                };
                EFI_STATUS status = http->Configure(http, &config);
                ThrowIfError(status, "configure the EFI HTTP protocol", uri);
                configured = true;

                return SendConfigured(http, httpMethod, uri, requestHeaders, requestBody, defaultContentType);
            }
            finally
            {
                if (configured && http != null)
                    http->Configure(http, null);
                if (httpOpened)
                    gBS->CloseProtocol(
                        childHandle,
                        (EFI_GUID*)HttpProtocolGuid,
                        gImageHandle,
                        default);
                if (childCreated && serviceBinding != null)
                    serviceBinding->DestroyChild(serviceBinding, childHandle);
                if (serviceBinding != null)
                    gBS->CloseProtocol(
                        serviceHandle,
                        (EFI_GUID*)HttpServiceBindingProtocolGuid,
                        gImageHandle,
                        default);
            }
        }

        private static void OpenProtocol(
            EFI_HANDLE* serviceHandle,
            EFI_HANDLE* childHandle,
            EFI_SERVICE_BINDING** serviceBinding,
            EFI_HTTP_PROTOCOL** http,
            bool* childCreated,
            bool* httpOpened)
        {
            ulong handleCount = 0;
            EFI_HANDLE* handles = null;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                ByProtocol,
                (EFI_GUID*)HttpServiceBindingProtocolGuid,
                null,
                &handleCount,
                &handles);
            if ((ulong)status != EFI_SUCCESS || handleCount == 0)
            {
                if (handles != null)
                    gBS->FreePool(handles);
                throw new WebException(
                    "EFI HTTP service binding is unavailable. Ensure HttpUtilitiesDxe.efi, " +
                    "TlsDxe.efi, and HttpDxe.efi were loaded.");
            }

            *serviceHandle = handles[0];
            status = gBS->OpenProtocol(
                *serviceHandle,
                (EFI_GUID*)HttpServiceBindingProtocolGuid,
                (void**)serviceBinding,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            gBS->FreePool(handles);
            ThrowIfError(status, "open the EFI HTTP service binding", null);

            status = (*serviceBinding)->CreateChild(*serviceBinding, childHandle);
            ThrowIfError(status, "create an EFI HTTP child", null);
            *childCreated = true;

            status = gBS->OpenProtocol(
                *childHandle,
                (EFI_GUID*)HttpProtocolGuid,
                (void**)http,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            ThrowIfError(status, "open the EFI HTTP protocol", null);
            *httpOpened = true;
        }

        private static HttpResult SendConfigured(
            EFI_HTTP_PROTOCOL* http,
            EFI_HTTP_METHOD method,
            Uri uri,
            WebHeaderCollection requestHeaders,
            byte[] requestBody,
            string defaultContentType)
        {
            List<string> names = new List<string>();
            List<string> values = new List<string>();
            AddHeader(names, values, "Host", uri.Authority);

            for (int i = 0; i < requestHeaders.Count; i++)
            {
                string name = requestHeaders.GetKey(i);
                if (!IsManagedHeader(name))
                    AddHeader(names, values, name, requestHeaders.Get(i));
            }

            if (requestBody != null)
            {
                if (requestHeaders["Content-Type"] == null && defaultContentType != null)
                    AddHeader(names, values, "Content-Type", defaultContentType);
                AddHeader(names, values, "Content-Length", requestBody.Length.ToString());
            }
            AddHeader(names, values, "Connection", "close");

            List<byte[]> encodedNames = new List<byte[]>();
            List<byte[]> encodedValues = new List<byte[]>();
            int encodedSize = 0;
            for (int i = 0; i < names.Count; i++)
            {
                byte[] name = Encoding.UTF8.GetBytes(names[i]);
                byte[] value = Encoding.UTF8.GetBytes(values[i]);
                encodedNames.Add(name);
                encodedValues.Add(value);
                encodedSize += name.Length + value.Length + 2;
            }

            byte[] encodedHeaders = new byte[encodedSize];
            string absoluteUrl = uri.AbsoluteUri;
            fixed (byte* encodedHeaderBytes = &encodedHeaders[0])
            fixed (char* url = &absoluteUrl.FirstChar)
            fixed (byte* requestBodyPointer = requestBody)
            {
                EFI_HTTP_HEADER* nativeHeaders = stackalloc EFI_HTTP_HEADER[names.Count];
                int offset = 0;
                for (int i = 0; i < names.Count; i++)
                {
                    nativeHeaders[i].FieldName = encodedHeaderBytes + offset;
                    offset = CopyNullTerminated(encodedNames[i], encodedHeaders, offset);
                    nativeHeaders[i].FieldValue = encodedHeaderBytes + offset;
                    offset = CopyNullTerminated(encodedValues[i], encodedHeaders, offset);
                }

                EFI_HTTP_REQUEST_DATA requestData = new EFI_HTTP_REQUEST_DATA
                {
                    Method = method,
                    Url = url
                };
                EFI_HTTP_MESSAGE requestMessage = new EFI_HTTP_MESSAGE
                {
                    Data = &requestData,
                    HeaderCount = (ulong)names.Count,
                    Headers = nativeHeaders,
                    BodyLength = requestBody == null ? 0UL : (ulong)requestBody.Length,
                    Body = requestBodyPointer
                };
                EFI_HTTP_TOKEN requestToken = new EFI_HTTP_TOKEN
                {
                    Status = EFI_NOT_READY,
                    Message = &requestMessage
                };

                SubmitRequest(http, &requestToken, uri);
            }

            return ReceiveResponse(http, uri, method);
        }

        private static HttpResult ReceiveResponse(
            EFI_HTTP_PROTOCOL* http,
            Uri uri,
            EFI_HTTP_METHOD method)
        {
            EFI_HTTP_RESPONSE_DATA responseData = new EFI_HTTP_RESPONSE_DATA();
            EFI_HTTP_MESSAGE responseMessage = new EFI_HTTP_MESSAGE
            {
                Data = &responseData
            };
            EFI_HTTP_TOKEN responseToken = new EFI_HTTP_TOKEN
            {
                Status = EFI_NOT_READY,
                Message = &responseMessage
            };

            SubmitResponse(http, &responseToken, uri, true);

            WebHeaderCollection responseHeaders;
            try
            {
                responseHeaders = CopyResponseHeaders(responseMessage.Headers, responseMessage.HeaderCount);
            }
            finally
            {
                FreeResponseHeaders(responseMessage.Headers, responseMessage.HeaderCount);
                responseMessage.Headers = null;
                responseMessage.HeaderCount = 0;
            }

            int statusCode = ToNumericStatus(responseData.StatusCode);
            if (statusCode < 200 || statusCode >= 300)
                return new HttpResult(statusCode, responseHeaders, new byte[0]);
            if (method == EFI_HTTP_METHOD.HttpMethodHead ||
                (statusCode >= 100 && statusCode < 200) ||
                statusCode == 204 || statusCode == 304)
                return new HttpResult(statusCode, responseHeaders, new byte[0]);

            MemoryStream body = new MemoryStream();
            bool chunked = ContainsToken(responseHeaders["Transfer-Encoding"], "chunked");
            int contentLength = chunked ? -1 : ParseContentLength(responseHeaders["Content-Length"]);
            bool connectionClose = ContainsToken(responseHeaders["Connection"], "close");
            byte[] buffer = new byte[ReceiveBufferSize];
            fixed (byte* bufferPointer = &buffer[0])
            {
                while (true)
                {
                    responseMessage.Data = null;
                    responseMessage.Body = bufferPointer;
                    responseMessage.BodyLength = (ulong)buffer.Length;
                    responseToken.Status = EFI_NOT_READY;
                    SubmitResponse(http, &responseToken, uri, false);
                    int received = (int)responseMessage.BodyLength;
                    if (received == 0)
                        break;
                    body.Write(buffer, 0, received);
                    if (contentLength >= 0 && body.Length >= contentLength)
                        break;
                    if (chunked && IsCompleteChunkedBody(body.ToArray()))
                        break;
                    if (!chunked && contentLength < 0 && connectionClose && received < buffer.Length)
                        break;
                }
            }

            byte[] bodyBytes = body.ToArray();
            if (chunked)
                bodyBytes = DecodeChunkedBody(bodyBytes);
            return new HttpResult(statusCode, responseHeaders, bodyBytes);
        }

        private static int ParseContentLength(string value)
        {
            if (string.IsNullOrEmpty(value))
                return -1;
            int result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character < '0' || character > '9')
                    throw new WebException("The HTTP Content-Length header is invalid.");
                int digit = character - '0';
                if (result > (int.MaxValue - digit) / 10)
                    throw new WebException("The HTTP response body is too large.");
                result = result * 10 + digit;
            }
            return result;
        }

        private static bool IsCompleteChunkedBody(byte[] input)
        {
            int offset = 0;
            while (true)
            {
                int lineEnd = FindCrlf(input, offset);
                if (lineEnd < 0)
                    return false;
                int extension = IndexOf(input, (byte)';', offset, lineEnd);
                int sizeEnd = extension < 0 ? lineEnd : extension;
                int chunkSize = ParseHex(input, offset, sizeEnd);
                if (chunkSize < 0)
                    throw new WebException("The chunked HTTP response is malformed.");
                offset = lineEnd + 2;

                if (chunkSize == 0)
                {
                    while (true)
                    {
                        lineEnd = FindCrlf(input, offset);
                        if (lineEnd < 0)
                            return false;
                        if (lineEnd == offset)
                            return true;
                        offset = lineEnd + 2;
                    }
                }

                if (offset > input.Length - chunkSize - 2)
                    return false;
                offset += chunkSize;
                if (input[offset] != '\r' || input[offset + 1] != '\n')
                    throw new WebException("The chunked HTTP response is malformed.");
                offset += 2;
            }
        }

        private static void SubmitRequest(EFI_HTTP_PROTOCOL* http, EFI_HTTP_TOKEN* token, Uri uri)
        {
            bool complete = false;
            EFI_EVENT timeoutEvent = default;
            EFI_STATUS status = gBS->CreateEvent(
                EVT_NOTIFY_SIGNAL,
                TPL_CALLBACK,
                &OperationCompleted,
                &complete,
                &token->Event);
            ThrowIfError(status, "create the HTTP request event", uri);
            try
            {
                timeoutEvent = CreateTimeoutEvent(uri, "send the HTTP request");
                do
                {
                    status = http->Request(http, token);
                    if ((ulong)status != EFI_NO_MAPPING)
                        break;
                    if ((ulong)gBS->CheckEvent(timeoutEvent) == EFI_SUCCESS)
                        throw new WebException("Timed out waiting for the DHCPv4 address for " + uri.AbsoluteUri + ".");
                    http->Poll(http);
                    gBS->Stall(1000);
                }
                while (true);
                ThrowIfError(status, "submit the HTTP request", uri);
                PollUntilComplete(http, token, &complete, timeoutEvent, uri, "send the HTTP request");
                ThrowIfError(token->Status, "send the HTTP request", uri);
            }
            finally
            {
                if ((void*)timeoutEvent != null)
                    gBS->CloseEvent(timeoutEvent);
                gBS->CloseEvent(token->Event);
                token->Event = default;
            }
        }

        private static void SubmitResponse(
            EFI_HTTP_PROTOCOL* http,
            EFI_HTTP_TOKEN* token,
            Uri uri,
            bool allowHttpError)
        {
            bool complete = false;
            EFI_EVENT timeoutEvent = default;
            EFI_STATUS status = gBS->CreateEvent(
                EVT_NOTIFY_SIGNAL,
                TPL_CALLBACK,
                &OperationCompleted,
                &complete,
                &token->Event);
            ThrowIfError(status, "create the HTTP response event", uri);
            try
            {
                timeoutEvent = CreateTimeoutEvent(uri, "receive the HTTP response");
                status = http->Response(http, token);
                ThrowIfError(status, "submit the HTTP response", uri);
                PollUntilComplete(http, token, &complete, timeoutEvent, uri, "receive the HTTP response");
                if (!allowHttpError || (ulong)token->Status != EfiHttpError)
                    ThrowIfError(token->Status, "receive the HTTP response", uri);
            }
            finally
            {
                if ((void*)timeoutEvent != null)
                    gBS->CloseEvent(timeoutEvent);
                gBS->CloseEvent(token->Event);
                token->Event = default;
            }
        }

        private static EFI_EVENT CreateTimeoutEvent(Uri uri, string operation)
        {
            EFI_EVENT timeoutEvent = default;
            EFI_STATUS status = gBS->CreateEvent(
                (uint)EVT_TIMER,
                TPL_APPLICATION,
                null,
                null,
                &timeoutEvent);
            ThrowIfError(status, "create a timer to " + operation, uri);
            status = gBS->SetTimer(
                timeoutEvent,
                TimerRelative,
                (ulong)RequestTimeoutMilliseconds * 10000);
            if ((ulong)status != EFI_SUCCESS)
            {
                gBS->CloseEvent(timeoutEvent);
                ThrowIfError(status, "start a timer to " + operation, uri);
            }
            return timeoutEvent;
        }

        private static void PollUntilComplete(
            EFI_HTTP_PROTOCOL* http,
            EFI_HTTP_TOKEN* token,
            bool* complete,
            EFI_EVENT timeoutEvent,
            Uri uri,
            string operation)
        {
            while (!*complete)
            {
                if ((ulong)gBS->CheckEvent(timeoutEvent) == EFI_SUCCESS)
                {
                    http->Cancel(http, token);
                    throw new WebException("Timed out attempting to " + operation + " for " + uri.AbsoluteUri + ".");
                }

                EFI_STATUS status = http->Poll(http);
                if ((ulong)status != EFI_SUCCESS && (ulong)status != EFI_NOT_READY)
                {
                    http->Cancel(http, token);
                    ThrowIfError(status, "poll while attempting to " + operation, uri);
                }
            }
        }

        [UnmanagedCallersOnly]
        private static void OperationCompleted(EFI_EVENT eventHandle, void* context)
        {
            *(bool*)context = true;
        }

        private static WebHeaderCollection CopyResponseHeaders(EFI_HTTP_HEADER* headers, ulong count)
        {
            WebHeaderCollection result = new WebHeaderCollection();
            for (ulong i = 0; i < count; i++)
                result.Add(ReadAscii(headers[i].FieldName), ReadAscii(headers[i].FieldValue));
            return result;
        }

        private static void FreeResponseHeaders(EFI_HTTP_HEADER* headers, ulong count)
        {
            if (headers == null)
                return;
            for (ulong i = 0; i < count; i++)
            {
                if (headers[i].FieldName != null)
                    gBS->FreePool(headers[i].FieldName);
                if (headers[i].FieldValue != null)
                    gBS->FreePool(headers[i].FieldValue);
            }
            gBS->FreePool(headers);
        }

        private static string ReadAscii(byte* text)
        {
            if (text == null)
                return string.Empty;
            int length = 0;
            while (text[length] != 0)
                length++;
            if (length == 0)
                return string.Empty;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = text[i];
            return Encoding.UTF8.GetString(bytes);
        }

        private static int CopyNullTerminated(byte[] source, byte[] destination, int offset)
        {
            for (int i = 0; i < source.Length; i++)
                destination[offset++] = source[i];
            destination[offset++] = 0;
            return offset;
        }

        private static void AddHeader(List<string> names, List<string> values, string name, string value)
        {
            names.Add(name);
            values.Add(value);
        }

        private static bool IsManagedHeader(string name)
            => WebHeaderCollection.EqualsIgnoreCase(name, "Host") ||
               WebHeaderCollection.EqualsIgnoreCase(name, "Connection") ||
               WebHeaderCollection.EqualsIgnoreCase(name, "Content-Length") ||
               WebHeaderCollection.EqualsIgnoreCase(name, "Transfer-Encoding");

        private static EFI_HTTP_METHOD ParseMethod(string method)
        {
            if (WebHeaderCollection.EqualsIgnoreCase(method, "GET")) return EFI_HTTP_METHOD.HttpMethodGet;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "POST")) return EFI_HTTP_METHOD.HttpMethodPost;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "PATCH")) return EFI_HTTP_METHOD.HttpMethodPatch;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "OPTIONS")) return EFI_HTTP_METHOD.HttpMethodOptions;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "CONNECT"))
                throw new NotSupportedException("CONNECT requires proxy configuration that WebClient does not expose.");
            if (WebHeaderCollection.EqualsIgnoreCase(method, "HEAD")) return EFI_HTTP_METHOD.HttpMethodHead;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "PUT")) return EFI_HTTP_METHOD.HttpMethodPut;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "DELETE")) return EFI_HTTP_METHOD.HttpMethodDelete;
            if (WebHeaderCollection.EqualsIgnoreCase(method, "TRACE")) return EFI_HTTP_METHOD.HttpMethodTrace;
            throw new NotSupportedException("The EFI HTTP protocol does not support method " + method + ".");
        }

        private static int ToNumericStatus(EFI_HTTP_STATUS_CODE status)
        {
            int value = (int)status;
            if (value == 1) return 100;
            if (value == 2) return 101;
            if (value >= 3 && value <= 9) return 200 + value - 3;
            if (value >= 10 && value <= 16)
            {
                int[] codes = { 300, 301, 302, 303, 304, 305, 307 };
                return codes[value - 10];
            }
            if (value >= 17 && value <= 34) return 400 + value - 17;
            if (value >= 35 && value <= 40) return 500 + value - 35;
            if (value == 41) return 308;
            if (value == 42) return 429;
            return 0;
        }

        private static bool ContainsToken(string value, string token)
        {
            if (value == null)
                return false;
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

        private static byte[] DecodeChunkedBody(byte[] input)
        {
            MemoryStream output = new MemoryStream();
            int offset = 0;
            while (true)
            {
                int lineEnd = FindCrlf(input, offset);
                if (lineEnd < 0)
                    throw new WebException("The chunked HTTP response is malformed.");
                int extension = IndexOf(input, (byte)';', offset, lineEnd);
                int sizeEnd = extension < 0 ? lineEnd : extension;
                int chunkSize = ParseHex(input, offset, sizeEnd);
                offset = lineEnd + 2;
                if (chunkSize == 0)
                    return output.ToArray();
                if (chunkSize < 0 || offset > input.Length - chunkSize - 2)
                    throw new WebException("The chunked HTTP response ended unexpectedly.");
                output.Write(input, offset, chunkSize);
                offset += chunkSize;
                if (input[offset] != '\r' || input[offset + 1] != '\n')
                    throw new WebException("The chunked HTTP response is malformed.");
                offset += 2;
            }
        }

        private static int ParseHex(byte[] input, int start, int end)
        {
            if (start >= end)
                return -1;
            int result = 0;
            for (int i = start; i < end; i++)
            {
                byte value = input[i];
                int digit;
                if (value >= '0' && value <= '9') digit = value - '0';
                else if (value >= 'a' && value <= 'f') digit = value - 'a' + 10;
                else if (value >= 'A' && value <= 'F') digit = value - 'A' + 10;
                else return -1;
                if (result > (int.MaxValue - digit) / 16)
                    throw new WebException("The HTTP response body is too large.");
                result = result * 16 + digit;
            }
            return result;
        }

        private static int FindCrlf(byte[] input, int start)
        {
            for (int i = start; i + 1 < input.Length; i++)
                if (input[i] == '\r' && input[i + 1] == '\n')
                    return i;
            return -1;
        }

        private static int IndexOf(byte[] input, byte value, int start, int end)
        {
            for (int i = start; i < end; i++)
                if (input[i] == value)
                    return i;
            return -1;
        }

        private static int IndexOf(string value, char character, int start)
        {
            for (int i = start; i < value.Length; i++)
                if (value[i] == character)
                    return i;
            return -1;
        }

        private static string Trim(string value)
        {
            int start = 0;
            int end = value.Length;
            while (start < end && (value[start] == ' ' || value[start] == '\t')) start++;
            while (end > start && (value[end - 1] == ' ' || value[end - 1] == '\t')) end--;
            return value.Substring(start, end - start);
        }

        private static void ThrowIfError(EFI_STATUS status, string operation, Uri uri)
        {
            ulong value = status;
            if (value == EFI_SUCCESS)
                return;
            string message = "Failed to " + operation + " (EFI_STATUS " + value + ").";
            if (uri != null && WebHeaderCollection.EqualsIgnoreCase(uri.Scheme, Uri.UriSchemeHttps))
                message += " HTTPS requires trusted certificates in the TlsCaCertificate UEFI variable.";
            throw new WebException(message);
        }
        private enum EFI_HTTP_VERSION
        {
            HttpVersion10,
            HttpVersion11,
            HttpVersionUnsupported
        }

        private enum EFI_HTTP_METHOD
        {
            HttpMethodGet,
            HttpMethodPost,
            HttpMethodPatch,
            HttpMethodOptions,
            HttpMethodConnect,
            HttpMethodHead,
            HttpMethodPut,
            HttpMethodDelete,
            HttpMethodTrace,
            HttpMethodMax
        }

        private enum EFI_HTTP_STATUS_CODE
        {
            Unsupported,
            Continue100,
            SwitchingProtocols101,
            Ok200,
            Created201,
            Accepted202,
            NonAuthoritativeInformation203,
            NoContent204,
            ResetContent205,
            PartialContent206,
            MultipleChoices300,
            MovedPermanently301,
            Found302,
            SeeOther303,
            NotModified304,
            UseProxy305,
            TemporaryRedirect307,
            BadRequest400,
            Unauthorized401,
            PaymentRequired402,
            Forbidden403,
            NotFound404,
            MethodNotAllowed405,
            NotAcceptable406,
            ProxyAuthenticationRequired407,
            RequestTimeout408,
            Conflict409,
            Gone410,
            LengthRequired411,
            PreconditionFailed412,
            RequestEntityTooLarge413,
            RequestUriTooLarge414,
            UnsupportedMediaType415,
            RequestedRangeNotSatisfiable416,
            ExpectationFailed417,
            InternalServerError500,
            NotImplemented501,
            BadGateway502,
            ServiceUnavailable503,
            GatewayTimeout504,
            HttpVersionNotSupported505,
            PermanentRedirect308,
            TooManyRequests429
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTPv4_ACCESS_POINT
        {
            public bool UseDefaultAddress;
            public EFI_IPv4_ADDRESS LocalAddress;
            public EFI_IPv4_ADDRESS LocalSubnet;
            public ushort LocalPort;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_CONFIG_DATA
        {
            public EFI_HTTP_VERSION HttpVersion;
            public uint TimeOutMillisec;
            public bool LocalAddressIsIPv6;
            public EFI_HTTPv4_ACCESS_POINT* IPv4Node;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_REQUEST_DATA
        {
            public EFI_HTTP_METHOD Method;
            public char* Url;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_RESPONSE_DATA
        {
            public EFI_HTTP_STATUS_CODE StatusCode;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_HEADER
        {
            public byte* FieldName;
            public byte* FieldValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_MESSAGE
        {
            public void* Data;
            public ulong HeaderCount;
            public EFI_HTTP_HEADER* Headers;
            public ulong BodyLength;
            public void* Body;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_TOKEN
        {
            public EFI_EVENT Event;
            public EFI_STATUS Status;
            public EFI_HTTP_MESSAGE* Message;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EFI_HTTP_PROTOCOL
        {
            public readonly delegate* unmanaged<EFI_HTTP_PROTOCOL*, EFI_HTTP_CONFIG_DATA*, EFI_STATUS> GetModeData;
            public readonly delegate* unmanaged<EFI_HTTP_PROTOCOL*, EFI_HTTP_CONFIG_DATA*, EFI_STATUS> Configure;
            public readonly delegate* unmanaged<EFI_HTTP_PROTOCOL*, EFI_HTTP_TOKEN*, EFI_STATUS> Request;
            public readonly delegate* unmanaged<EFI_HTTP_PROTOCOL*, EFI_HTTP_TOKEN*, EFI_STATUS> Cancel;
            public readonly delegate* unmanaged<EFI_HTTP_PROTOCOL*, EFI_HTTP_TOKEN*, EFI_STATUS> Response;
            public readonly delegate* unmanaged<EFI_HTTP_PROTOCOL*, EFI_STATUS> Poll;
        }
    }
}
