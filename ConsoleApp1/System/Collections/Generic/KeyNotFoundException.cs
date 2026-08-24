namespace System.Collections.Generic
{
    public class KeyNotFoundException : System.Exception
    {
        public KeyNotFoundException() : base("The given key was not present in the dictionary.")
        {
        }

        public KeyNotFoundException(string message) : base(message)
        {
        }
    }
}
