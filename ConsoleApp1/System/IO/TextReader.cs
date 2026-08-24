using System.Text;

namespace System.IO
{
    public abstract class TextReader : IDisposable
    {
        public virtual int Read() => throw new NotSupportedException("This text reader does not implement Read.");

        public virtual int Read(char[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("The read buffer cannot be null.");
            return Read(buffer, 0, buffer.Length);
        }

        public virtual int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The read buffer cannot be null.");
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The read buffer offset and count do not describe a valid range.");

            int read = 0;
            while (read < count)
            {
                int value = Read();
                if (value < 0)
                    break;
                buffer[index + read++] = (char)value;
            }
            return read;
        }

        public virtual int ReadBlock(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The read buffer cannot be null.");
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The read buffer offset and count do not describe a valid range.");

            int total = 0;
            while (total < count)
            {
                int read = Read(buffer, index + total, count - total);
                if (read == 0)
                    break;
                total += read;
            }
            return total;
        }

        public virtual string ReadLine()
        {
            StringBuilder line = new StringBuilder();
            while (true)
            {
                int value = Read();
                if (value < 0)
                    return line.Length == 0 ? null : line.ToString();
                if (value == '\r')
                {
                    int next = Read();
                    if (next >= 0 && next != '\n')
                        throw new NotSupportedException("A carriage return must be followed by a line feed.");
                    return line.ToString();
                }
                if (value == '\n')
                    return line.ToString();
                line.Append((char)value);
            }
        }

        public virtual string ReadToEnd()
        {
            StringBuilder result = new StringBuilder();
            char[] buffer = new char[256];
            int read;
            while ((read = Read(buffer, 0, buffer.Length)) != 0)
                result.Append(buffer, 0, read);
            return result.ToString();
        }

        public virtual void Close() { }
        public virtual void Dispose() => Close();
    }
}
