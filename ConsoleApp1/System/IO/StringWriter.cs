using System.Text;

namespace System.IO
{
    public sealed class StringWriter : TextWriter
    {
        private readonly StringBuilder _builder;
        private bool _closed;

        public StringWriter()
        {
            _builder = new StringBuilder();
        }

        public StringWriter(StringBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException("The string builder cannot be null.");
        }

        public override Encoding Encoding => Encoding.UTF8;

        public StringBuilder GetStringBuilder()
        {
            EnsureOpen();
            return _builder;
        }

        public override void Write(char value)
        {
            EnsureOpen();
            _builder.Append(value);
        }

        public override void Write(string value)
        {
            EnsureOpen();
            _builder.Append(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            EnsureOpen();
            _builder.Append(buffer, index, count);
        }

        public override string ToString()
        {
            EnsureOpen();
            return _builder.ToString();
        }

        public override void Close() => _closed = true;

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException("The string writer is closed.");
        }
    }
}
