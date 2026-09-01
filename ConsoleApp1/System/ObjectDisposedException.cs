namespace System
{
    public class ObjectDisposedException : InvalidOperationException
    {
        public ObjectDisposedException(string objectName)
            : base("The object has been disposed." + (objectName == null ? string.Empty : " " + objectName))
        {
        }
    }
}
