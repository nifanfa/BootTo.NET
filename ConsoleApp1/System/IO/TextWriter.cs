namespace System.IO
{
    public abstract class TextWriter : IDisposable
    {
        public abstract Text.Encoding Encoding { get; }
        public virtual string NewLine { get; set; } = "\r\n";

        public virtual void Write(char value) { }
        public virtual void Write(string value)
        {
            if (value == null)
                return;
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public virtual void Write(char[] buffer)
        {
            if (buffer == null)
                return;
            Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The write buffer cannot be null.");
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The write buffer offset and count do not describe a valid range.");
            for (int i = 0; i < count; i++)
                Write(buffer[index + i]);
        }

        public virtual void Write(bool value) => Write(value.ToString());
        public virtual void Write(sbyte value) => Write(value.ToString());
        public virtual void Write(short value) => Write(value.ToString());
        public virtual void Write(ushort value) => Write(value.ToString());
        public virtual void Write(int value) => Write(value.ToString());
        public virtual void Write(long value) => Write(value.ToString());
        public virtual void Write(uint value) => Write(value.ToString());
        public virtual void Write(ulong value) => Write(value.ToString());
        public virtual void Write(float value) => Write(value.ToString());
        public virtual void Write(double value) => Write(value.ToString());
        public virtual void Write(object value) => Write(value?.ToString());

        public virtual void Write(string format, object arg0) => Write(string.Format(format, arg0));
        public virtual void Write(string format, object arg0, object arg1) => Write(string.Format(format, arg0, arg1));
        public virtual void Write(string format, params object[] args) => Write(string.Format(format, args));

        public virtual void WriteLine() => Write(NewLine);
        public virtual void WriteLine(string value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(char value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(char[] buffer) { Write(buffer); Write(NewLine); }
        public virtual void WriteLine(bool value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(sbyte value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(short value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(ushort value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(int value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(long value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(uint value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(ulong value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(float value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(double value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(object value) { Write(value); Write(NewLine); }
        public virtual void WriteLine(string format, object arg0) { Write(format, arg0); Write(NewLine); }
        public virtual void WriteLine(string format, object arg0, object arg1) { Write(format, arg0, arg1); Write(NewLine); }
        public virtual void WriteLine(string format, params object[] args) { Write(format, args); Write(NewLine); }

        public virtual void Flush() { }
        public virtual void Close() { }
        public virtual void Dispose() => Close();
    }
}
