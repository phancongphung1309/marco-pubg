namespace MouseStudio.Core.Input;

public static class InputState
{
    public static bool LeftMouseDown { get; set; }

    public static bool RightMouseDown { get; set; }

    public static bool BackMouseDown { get; set; }

    public static bool ForwardMouseDown { get; set; }

    public static bool MiddleMouseDown { get; set; }

    // The assigned hotkey (key or mouse button) is held.
    public static bool HotkeyDown { get; set; }
}