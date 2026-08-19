namespace System
{
    public readonly struct ConsoleKeyEvent
    {
        private readonly ConsoleKeyInfo _keyInfo;
        private readonly bool _isKeyDown;

        public ConsoleKeyEvent(ConsoleKeyInfo keyInfo, bool isKeyDown)
        {
            _keyInfo = keyInfo;
            _isKeyDown = isKeyDown;
        }

        public ConsoleKeyInfo KeyInfo => _keyInfo;

        public ConsoleKey Key => _keyInfo.Key;

        public char KeyChar => _keyInfo.KeyChar;

        public ConsoleModifiers Modifiers => _keyInfo.Modifiers;

        public bool IsKeyDown => _isKeyDown;

        public bool IsKeyUp => !_isKeyDown;
    }
}
