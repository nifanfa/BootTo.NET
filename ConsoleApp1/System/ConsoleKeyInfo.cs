namespace System
{
    public readonly struct ConsoleKeyInfo
    {
        private readonly char _keyChar;
        private readonly ConsoleKey _key;
        private readonly ConsoleModifiers _mods;

        public ConsoleKeyInfo(char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
        {
            _keyChar = keyChar;
            _key = key;
            _mods = 0;

            if (shift)
                _mods |= ConsoleModifiers.Shift;
            if (alt)
                _mods |= ConsoleModifiers.Alt;
            if (control)
                _mods |= ConsoleModifiers.Control;
        }

        public char KeyChar => _keyChar;

        public ConsoleKey Key => _key;

        public ConsoleModifiers Modifiers => _mods;
    }
}
