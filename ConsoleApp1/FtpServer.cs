using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public sealed class FtpServer
{
    private readonly string _rootDirectory;
    private readonly int _port;
    private readonly int _passivePortStart;
    private readonly int _passivePortEnd;
    private Socket _listener;
    private Task _acceptTask;
    private int _nextPassivePort;
    private bool _running;

    public FtpServer(string userName, string password)
        : this(@"\", userName, password)
    {
    }

    public FtpServer(string rootDirectory, string userName, string password, int port = 21,
        int passivePortStart = 50000, int passivePortEnd = 50010)
    {
        if (string.IsNullOrEmpty(rootDirectory))
            throw new ArgumentException("The FTP root directory cannot be empty.");
        if (string.IsNullOrEmpty(userName))
            throw new ArgumentException("The FTP user name cannot be empty.");
        if (password == null)
            throw new ArgumentNullException("The FTP password cannot be null.");
        if ((uint)port > ushort.MaxValue || port == 0)
            throw new ArgumentException("The FTP control port must be between 1 and 65535.");
        if (passivePortStart <= 0 || passivePortEnd < passivePortStart || passivePortEnd > ushort.MaxValue)
            throw new ArgumentException("The FTP passive port range is invalid.");

        _rootDirectory = NormalizeRootDirectory(rootDirectory);
        _port = port;
        _passivePortStart = passivePortStart;
        _passivePortEnd = passivePortEnd;
        _nextPassivePort = passivePortStart;
        UserName = userName;
        Password = password;
    }

    public string RootDirectory => _rootDirectory;
    public int Port => _port;
    public bool IsRunning => _running;

    // Set this only when the server is behind NAT and the control connection's
    // local address is not reachable by the FTP client.
    public IPAddress PassiveAddress { get; set; }

    public string UserName { get; }
    public string Password { get; }

    public void Start()
    {
        if (_running)
            throw new InvalidOperationException("The FTP server is already running.");
        if (!Directory.Exists(_rootDirectory))
            throw new IOException("The FTP root directory does not exist.");

        Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        try
        {
            listener.Bind(IPAddress.Any, _port);
        }
        catch
        {
            try { listener.CloseAsync().GetAwaiter().GetResult(); }
            catch { }
            throw;
        }

        _listener = listener;
        _running = true;
        _acceptTask = StartAcceptLoopAsync(listener);
    }

    public Task RunAsync()
    {
        Start();
        return _acceptTask;
    }

    public async Task StopAsync()
    {
        if (!_running)
            return;

        _running = false;
        Socket listener = _listener;
        _listener = null;
        if (listener != null)
        {
            try { await listener.CloseAsync(); }
            catch { }
        }
    }

    private async Task AcceptLoopAsync()
    {
        while (_running && _listener != null)
        {
            try
            {
                Socket control = await _listener.AcceptAsync();
                _ = new FtpSession(this, control).RunAsync();
            }
            catch
            {
                if (_running)
                    await Task.Delay(100);
            }
        }
    }

    private async Task StartAcceptLoopAsync(Socket listener)
    {
        try
        {
            await listener.ListenAsync(4);
            if (_running && _listener == listener)
                await AcceptLoopAsync();
        }
        catch
        {
            if (_listener == listener)
            {
                _listener = null;
                _running = false;
            }
        }
    }

    private Socket CreatePassiveListener(out int port)
    {
        for (int attempt = 0; attempt <= _passivePortEnd - _passivePortStart; attempt++)
        {
            port = _nextPassivePort;
            _nextPassivePort++;
            if (_nextPassivePort > _passivePortEnd)
                _nextPassivePort = _passivePortStart;

            Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(IPAddress.Any, port);
                listener.Listen(1);
                return listener;
            }
            catch
            {
                try { listener.CloseAsync().GetAwaiter().GetResult(); }
                catch { }
            }
        }

        port = 0;
        return null;
    }

    private static string NormalizeRootDirectory(string value)
    {
        string root = value.Replace('/', '\\');
        if (root.Length == 0)
            return "\\";
        if (!IsSeparator(root[0]))
            root = "\\" + root;
        while (root.Length > 1 && IsSeparator(root[root.Length - 1]))
            root = root.Substring(0, root.Length - 1);
        return root;
    }

    private static bool IsSeparator(char value)
        => value == '\\' || value == '/';

    private sealed class FtpSession
    {
        private const int ControlBufferSize = 8192;
        private const int TransferBufferSize = 4096;
        private static readonly string[] MonthNames =
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        private readonly FtpServer _server;
        private readonly Socket _control;
        private readonly byte[] _controlBuffer = new byte[ControlBufferSize];
        private int _controlLength;
        private string _currentDirectory = "\\";
        private string _userName;
        private string _renameSource;
        private bool _authenticated;
        private Socket _passiveListener;

        internal FtpSession(FtpServer server, Socket control)
        {
            _server = server;
            _control = control;
        }

        internal async Task RunAsync()
        {
            try
            {
                await ReplyAsync(220, "BootTo.NET FTP service ready.");
                while (true)
                {
                    string line = await ReadLineAsync();
                    if (line == null)
                        break;
                    if (line.Length == 0)
                        continue;

                    int separator = line.IndexOf(' ');
                    string command = (separator < 0 ? line : line.Substring(0, separator)).ToUpperInvariant();
                    string argument = separator < 0 ? string.Empty : line.Substring(separator + 1).Trim();

                    try
                    {
                        if (await ExecuteCommandAsync(command, argument))
                            break;
                    }
                    catch (Exception)
                    {
                        await ReplyAsync(451, "Requested action aborted. Local error in processing.");
                    }
                }
            }
            catch
            {
            }
            finally
            {
                await ClosePassiveListenerAsync();
                try { await _control.CloseAsync(); }
                catch { }
            }
        }

        private async Task<bool> ExecuteCommandAsync(string command, string argument)
        {
            switch (command)
            {
                case "USER":
                    _authenticated = false;
                    _userName = argument;
                    _renameSource = null;
                    if (EqualsIgnoreCase(argument, _server.UserName))
                        await ReplyAsync(331, "User name accepted, password required.");
                    else
                        await ReplyAsync(530, "Invalid user name.");
                    return false;

                case "PASS":
                    if (_userName == null || !EqualsIgnoreCase(_userName, _server.UserName))
                    {
                        await ReplyAsync(530, "Login incorrect.");
                        return false;
                    }
                    if (_server.Password != null && argument != _server.Password)
                    {
                        await ReplyAsync(530, "Login incorrect.");
                        return false;
                    }
                    _authenticated = true;
                    await ReplyAsync(230, "User logged in.");
                    return false;

                case "QUIT":
                    await ReplyAsync(221, "Goodbye.");
                    return true;

                case "SYST":
                    await ReplyAsync(215, "UNIX Type: L8");
                    return false;

                case "FEAT":
                    await SendTextAsync("211-Features:\r\n UTF8\r\n EPSV\r\n PASV\r\n MLSD\r\n SIZE\r\n MDTM\r\n211 End\r\n");
                    return false;

                case "NOOP":
                    await ReplyAsync(200, "NOOP command successful.");
                    return false;

                case "HELP":
                    await ReplyAsync(214, "USER PASS PWD CWD CDUP TYPE PASV EPSV LIST NLST MLSD RETR STOR APPE DELE MKD RMD RNFR RNTO SIZE MDTM QUIT");
                    return false;

                case "OPTS":
                    await ReplyAsync(200, "UTF8 options accepted.");
                    return false;

                case "AUTH":
                case "PBSZ":
                case "PROT":
                    await ReplyAsync(502, "TLS is not supported.");
                    return false;
            }

            if (!_authenticated)
            {
                await ReplyAsync(530, "Please log in with USER and PASS.");
                return false;
            }

            switch (command)
            {
                case "PWD":
                    await ReplyAsync(257, "\"" + ToFtpPath(_currentDirectory) + "\" is the current directory.");
                    break;

                case "CWD":
                    await ChangeDirectoryAsync(argument);
                    break;

                case "CDUP":
                    await ChangeDirectoryAsync("..");
                    break;

                case "TYPE":
                    if (argument == "A" || argument == "I")
                        await ReplyAsync(200, "Type set to " + argument + ".");
                    else
                        await ReplyAsync(504, "Unsupported transfer type.");
                    break;

                case "MODE":
                    await ReplyAsync(argument == "S" ? 200 : 504, argument == "S" ? "Stream mode enabled." : "Unsupported transfer mode.");
                    break;

                case "STRU":
                    await ReplyAsync(argument == "F" ? 200 : 504, argument == "F" ? "File structure enabled." : "Unsupported file structure.");
                    break;

                case "PASV":
                    await EnterPassiveModeAsync(false);
                    break;

                case "EPSV":
                    await EnterPassiveModeAsync(true);
                    break;

                case "LIST":
                    await SendDirectoryListingAsync(argument, false, false);
                    break;

                case "NLST":
                    await SendDirectoryListingAsync(argument, true, false);
                    break;

                case "MLSD":
                    await SendDirectoryListingAsync(argument, false, true);
                    break;

                case "RETR":
                    await SendFileAsync(argument);
                    break;

                case "STOR":
                    await ReceiveFileAsync(argument, false);
                    break;

                case "APPE":
                    await ReceiveFileAsync(argument, true);
                    break;

                case "DELE":
                    await DeleteFileAsync(argument);
                    break;

                case "MKD":
                    await MakeDirectoryAsync(argument);
                    break;

                case "RMD":
                    await RemoveDirectoryAsync(argument);
                    break;

                case "RNFR":
                    await RenameFromAsync(argument);
                    break;

                case "RNTO":
                    await RenameToAsync(argument);
                    break;

                case "SIZE":
                    await SendFileSizeAsync(argument);
                    break;

                case "MDTM":
                    await SendModificationTimeAsync(argument);
                    break;

                case "STAT":
                    await ReplyAsync(211, "FTP service is running.");
                    break;

                case "ABOR":
                    await ClosePassiveListenerAsync();
                    await ReplyAsync(225, "No transfer is in progress.");
                    break;

                case "REIN":
                    _authenticated = false;
                    _userName = null;
                    _renameSource = null;
                    _currentDirectory = "\\";
                    await ClosePassiveListenerAsync();
                    await ReplyAsync(220, "Service ready for new user.");
                    break;

                default:
                    await ReplyAsync(502, "Command not implemented.");
                    break;
            }

            return false;
        }

        private async Task ChangeDirectoryAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out string virtualPath) || !Directory.Exists(path))
            {
                await ReplyAsync(550, "Directory is unavailable.");
                return;
            }

            _currentDirectory = virtualPath;
            await ReplyAsync(250, "Directory changed to " + ToFtpPath(_currentDirectory) + ".");
        }

        private async Task EnterPassiveModeAsync(bool extended)
        {
            await ClosePassiveListenerAsync();
            _passiveListener = _server.CreatePassiveListener(out int port);
            if (_passiveListener == null)
            {
                await ReplyAsync(425, "Cannot open passive data connection.");
                return;
            }

            if (extended)
            {
                await ReplyAsync(229, "Entering Extended Passive Mode (|||" + port + "|).");
                return;
            }

            IPAddress address = _server.PassiveAddress ?? _control.LocalAddress;
            if (address == null || address.Equals(IPAddress.Any))
            {
                await ClosePassiveListenerAsync();
                await ReplyAsync(425, "The passive address is unavailable. Set FtpServer.PassiveAddress.");
                return;
            }

            byte[] bytes = address.GetAddressBytes();
            await ReplyAsync(
                227,
                "Entering Passive Mode (" + bytes[0] + "," + bytes[1] + "," + bytes[2] + "," + bytes[3] + "," +
                (port >> 8) + "," + (port & 0xFF) + ").");
        }

        private async Task SendDirectoryListingAsync(string argument, bool namesOnly, bool machineReadable)
        {
            string requestedPath = argument.StartsWith("-") ? string.Empty : argument;
            if (!TryResolvePath(requestedPath, out string path, out _))
            {
                await ReplyAsync(550, "Invalid path.");
                return;
            }

            bool file = File.Exists(path);
            bool directory = Directory.Exists(path);
            if (!file && !directory)
            {
                await ReplyAsync(550, "Path is unavailable.");
                return;
            }

            Socket data = await BeginDataTransferAsync();
            if (data == null)
                return;

            try
            {
                if (file)
                {
                    await SendListingEntryAsync(data, path, false, namesOnly, machineReadable);
                }
                else
                {
                    string[] directories = Directory.GetDirectories(path);
                    for (int i = 0; i < directories.Length; i++)
                        await SendListingEntryAsync(data, directories[i], true, namesOnly, machineReadable);

                    string[] files = Directory.GetFiles(path);
                    for (int i = 0; i < files.Length; i++)
                        await SendListingEntryAsync(data, files[i], false, namesOnly, machineReadable);
                }
            }
            finally
            {
                await CloseSocketAsync(data);
            }

            await ReplyAsync(226, "Directory transfer complete.");
        }

        private async Task SendListingEntryAsync(Socket data, string path, bool directory, bool namesOnly, bool machineReadable)
        {
            string name = Path.GetFileName(path);
            FileInfo info = new FileInfo(path);
            string line;
            if (namesOnly)
            {
                line = name;
            }
            else if (machineReadable)
            {
                long size = directory ? 0 : info.Length;
                line = "type=" + (directory ? "dir" : "file") + ";size=" + size + ";modify=" +
                    FormatFtpTimestamp(info.LastWriteTime) + "; " + name;
            }
            else
            {
                long size = directory ? 0 : info.Length;
                line = (directory ? "drwxr-xr-x" : "-rw-r--r--") + " 1 owner group " + size + " " +
                    FormatDirectoryListingTime(info.LastWriteTime) + " " + name;
            }

            await SendAllAsync(data, Encoding.UTF8.GetBytes(line + "\r\n"));
        }

        private async Task SendFileAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out _) || !File.Exists(path))
            {
                await ReplyAsync(550, "File is unavailable.");
                return;
            }

            Socket data = await BeginDataTransferAsync();
            if (data == null)
                return;

            try
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    byte[] buffer = new byte[TransferBufferSize];
                    for (;;)
                    {
                        int count = await stream.ReadAsync(buffer);
                        if (count == 0)
                            break;
                        if (count == buffer.Length)
                        {
                            await SendAllAsync(data, buffer);
                        }
                        else
                        {
                            byte[] tail = new byte[count];
                            for (int i = 0; i < count; i++)
                                tail[i] = buffer[i];
                            await SendAllAsync(data, tail);
                        }
                    }
                }
                finally
                {
                    stream.Close();
                }
            }
            finally
            {
                await CloseSocketAsync(data);
            }

            await ReplyAsync(226, "File transfer complete.");
        }

        private async Task ReceiveFileAsync(string argument, bool append)
        {
            if (!TryResolvePath(argument, out string path, out _))
            {
                await ReplyAsync(550, "Invalid file path.");
                return;
            }

            Socket data = await BeginDataTransferAsync();
            if (data == null)
                return;

            try
            {
                FileStream stream = new FileStream(
                    path,
                    append ? FileMode.Append : FileMode.Create,
                    FileAccess.Write,
                    FileShare.None);
                try
                {
                    byte[] buffer = new byte[TransferBufferSize];
                    for (;;)
                    {
                        int count = await data.ReceiveAsync(buffer);
                        if (count == 0)
                            break;
                        if (count == buffer.Length)
                        {
                            await stream.WriteAsync(buffer);
                        }
                        else
                        {
                            byte[] part = new byte[count];
                            for (int i = 0; i < count; i++)
                                part[i] = buffer[i];
                            await stream.WriteAsync(part);
                        }
                    }
                    await stream.FlushAsync();
                }
                finally
                {
                    stream.Close();
                }
            }
            finally
            {
                await CloseSocketAsync(data);
            }

            await ReplyAsync(226, "File transfer complete.");
        }

        private async Task DeleteFileAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out _) || !File.Exists(path))
            {
                await ReplyAsync(550, "File is unavailable.");
                return;
            }

            File.Delete(path);
            await ReplyAsync(File.Exists(path) ? 550 : 250, File.Exists(path) ? "File could not be deleted." : "File deleted.");
        }

        private async Task MakeDirectoryAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out string virtualPath) || Directory.Exists(path))
            {
                await ReplyAsync(550, "Directory already exists or path is invalid.");
                return;
            }

            Directory.CreateDirectory(path);
            await ReplyAsync(257, "\"" + ToFtpPath(virtualPath) + "\" created.");
        }

        private async Task RemoveDirectoryAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out string virtualPath) || virtualPath == "\\" || !Directory.Exists(path))
            {
                await ReplyAsync(550, "Directory is unavailable.");
                return;
            }

            Directory.Delete(path);
            await ReplyAsync(250, "Directory removed.");
        }

        private async Task RenameFromAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out _) || !File.Exists(path))
            {
                await ReplyAsync(550, "File is unavailable.");
                return;
            }

            _renameSource = path;
            await ReplyAsync(350, "File exists, ready for destination name.");
        }

        private async Task RenameToAsync(string argument)
        {
            if (_renameSource == null)
            {
                await ReplyAsync(503, "RNFR is required before RNTO.");
                return;
            }
            if (!TryResolvePath(argument, out string path, out _) || File.Exists(path) || Directory.Exists(path))
            {
                await ReplyAsync(550, "Destination is unavailable.");
                return;
            }

            try
            {
                File.Move(_renameSource, path);
                await ReplyAsync(250, "File renamed.");
            }
            finally
            {
                _renameSource = null;
            }
        }

        private async Task SendFileSizeAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out _) || !File.Exists(path))
            {
                await ReplyAsync(550, "File is unavailable.");
                return;
            }

            await ReplyAsync(213, new FileInfo(path).Length.ToString());
        }

        private async Task SendModificationTimeAsync(string argument)
        {
            if (!TryResolvePath(argument, out string path, out _) || !File.Exists(path))
            {
                await ReplyAsync(550, "File is unavailable.");
                return;
            }

            DateTime time = new FileInfo(path).LastWriteTime;
            await ReplyAsync(213, FormatFtpTimestamp(time));
        }

        private async Task<Socket> BeginDataTransferAsync()
        {
            await ReplyAsync(150, "Opening data connection.");
            Socket data = await OpenDataConnectionAsync();
            if (data == null)
                await ReplyAsync(425, "Cannot open data connection.");
            return data;
        }

        private async Task<Socket> OpenDataConnectionAsync()
        {
            Socket passive = _passiveListener;
            _passiveListener = null;
            if (passive != null)
            {
                try
                {
                    return await passive.AcceptAsync();
                }
                catch
                {
                    return null;
                }
                finally
                {
                    await CloseSocketAsync(passive);
                }
            }

            return null;
        }

        private async Task ClosePassiveListenerAsync()
        {
            Socket listener = _passiveListener;
            _passiveListener = null;
            if (listener != null)
                await CloseSocketAsync(listener);
        }

        private async Task<string> ReadLineAsync()
        {
            for (;;)
            {
                for (int i = 0; i < _controlLength; i++)
                {
                    if (_controlBuffer[i] != (byte)'\n')
                        continue;

                    int lineLength = i;
                    if (lineLength > 0 && _controlBuffer[lineLength - 1] == (byte)'\r')
                        lineLength--;

                    string line = Encoding.UTF8.GetString(_controlBuffer, 0, lineLength);
                    int remaining = _controlLength - i - 1;
                    for (int j = 0; j < remaining; j++)
                        _controlBuffer[j] = _controlBuffer[i + 1 + j];
                    _controlLength = remaining;
                    return line;
                }

                if (_controlLength == _controlBuffer.Length)
                    throw new IOException("The FTP command line is too long.");

                byte[] receiveBuffer = new byte[1024];
                int received = await _control.ReceiveAsync(receiveBuffer);
                if (received == 0)
                    return null;
                if (received > _controlBuffer.Length - _controlLength)
                    throw new IOException("The FTP command line is too long.");

                for (int i = 0; i < received; i++)
                    _controlBuffer[_controlLength++] = receiveBuffer[i];
            }
        }

        private Task ReplyAsync(int code, string text)
            => SendTextAsync(code.ToString() + " " + text + "\r\n");

        private Task SendTextAsync(string text)
            => SendAllAsync(_control, Encoding.UTF8.GetBytes(text));

        private static async Task SendAllAsync(Socket socket, byte[] buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                byte[] pending;
                if (offset == 0)
                {
                    pending = buffer;
                }
                else
                {
                    pending = new byte[buffer.Length - offset];
                    for (int i = 0; i < pending.Length; i++)
                        pending[i] = buffer[offset + i];
                }

                int sent = await socket.SendAsync(pending);
                if (sent <= 0)
                    throw new IOException("The FTP connection closed during a write.");
                offset += sent;
            }
        }

        private static async Task CloseSocketAsync(Socket socket)
        {
            if (socket == null)
                return;
            try { await socket.CloseAsync(); }
            catch { }
        }

        private bool TryResolvePath(string value, out string path, out string virtualPath)
        {
            path = null;
            virtualPath = null;
            value = value ?? string.Empty;
            List<string> components = new List<string>();
            bool absolute = value.Length > 0 && IsSeparator(value[0]);
            if (!absolute && !AppendPathComponents(components, _currentDirectory))
                return false;
            if (!AppendPathComponents(components, value))
                return false;

            StringBuilder virtualBuilder = new StringBuilder();
            virtualBuilder.Append('\\');
            for (int i = 0; i < components.Count; i++)
            {
                if (i != 0)
                    virtualBuilder.Append('\\');
                virtualBuilder.Append(components[i]);
            }
            virtualPath = virtualBuilder.ToString();

            StringBuilder physicalBuilder = new StringBuilder(_server._rootDirectory);
            for (int i = 0; i < components.Count; i++)
            {
                if (!IsSeparator(physicalBuilder[physicalBuilder.Length - 1]))
                    physicalBuilder.Append('\\');
                physicalBuilder.Append(components[i]);
            }
            path = physicalBuilder.ToString();
            return true;
        }

        private static bool AppendPathComponents(List<string> components, string value)
        {
            int start = 0;
            for (int i = 0; i <= value.Length; i++)
            {
                if (i != value.Length && !IsSeparator(value[i]))
                    continue;

                int length = i - start;
                if (length > 0)
                {
                    string part = value.Substring(start, length);
                    if (part == ".")
                    {
                    }
                    else if (part == "..")
                    {
                        if (components.Count > 0)
                            components.RemoveAt(components.Count - 1);
                    }
                    else
                    {
                        if (part.IndexOf(':') >= 0)
                            return false;
                        components.Add(part);
                    }
                }
                start = i + 1;
            }
            return true;
        }

        private static string ToFtpPath(string value)
            => value == "\\" ? "/" : value.Replace('\\', '/');

        private static bool IsSeparator(char value)
            => value == '\\' || value == '/';

        private static bool EqualsIgnoreCase(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
            {
                char a = left[i];
                char b = right[i];
                if (a >= 'a' && a <= 'z') a = (char)(a - ('a' - 'A'));
                if (b >= 'a' && b <= 'z') b = (char)(b - ('a' - 'A'));
                if (a != b)
                    return false;
            }
            return true;
        }

        private static string FormatDecimal(int value, int minimumDigits)
        {
            string text = value.ToString();
            while (text.Length < minimumDigits)
                text = "0" + text;
            return text;
        }

        private static string FormatFtpTimestamp(DateTime time)
            => FormatDecimal(time.Year, 4) + FormatDecimal(time.Month, 2) + FormatDecimal(time.Day, 2) +
                FormatDecimal(time.Hour, 2) + FormatDecimal(time.Minute, 2) + FormatDecimal(time.Second, 2);

        private static string FormatDirectoryListingTime(DateTime time)
            => MonthNames[time.Month - 1] + " " + (time.Day < 10 ? " " : string.Empty) + time.Day + " " +
                FormatDecimal(time.Hour, 2) + ":" + FormatDecimal(time.Minute, 2);
    }
}
