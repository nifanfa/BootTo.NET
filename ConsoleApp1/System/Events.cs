namespace System
{
    public delegate void EventHandler(object? sender, EventArgs e);
    public delegate void EventHandler<TEventArgs>(object? sender, TEventArgs e) where TEventArgs : EventArgs;
    public class EventArgs
    {
        public static readonly EventArgs Empty = new EventArgs();
    }
}

