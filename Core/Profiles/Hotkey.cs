namespace MouseStudio.Core.Profiles;

public enum HotkeyKind
{
    Mouse,
    Keyboard
}

// A key or mouse button used to trigger the movement.
public class Hotkey
{
    // Mouse buttons use the script numbering: 1 left, 2 right,
    // 3 middle, 4 back, 5 forward.
    public const int MouseLeft = 1;
    public const int MouseRight = 2;
    public const int MouseMiddle = 3;
    public const int MouseBack = 4;
    public const int MouseForward = 5;

    // F1 to F12 load profiles; Escape cancels assigning a hotkey.
    private const int VK_F1 = 0x70;
    private const int VK_F12 = 0x7B;
    private const int VK_ESCAPE = 0x1B;

    public HotkeyKind Kind { get; set; }

    // The mouse button number, or the virtual-key code.
    public int Code { get; set; }

    public static Hotkey CreateDefault() => Mouse(MouseForward);

    public static Hotkey Mouse(int button) => new()
    {
        Kind = HotkeyKind.Mouse,
        Code = button
    };

    // Left and right are only allowed in a combo: held alone they would
    // fight with the left clicks the script sends itself.
    public static bool IsAllowed(HotkeyKind kind, int code, bool allowLeftRight = false) => kind switch
    {
        HotkeyKind.Mouse => code is MouseMiddle or MouseBack or MouseForward
            || (allowLeftRight && code is MouseLeft or MouseRight),
        HotkeyKind.Keyboard => code is > 0 and < 0xFF
            and not VK_ESCAPE
            and not (>= VK_F1 and <= VK_F12),
        _ => false
    };

    public bool IsValid => IsAllowed(Kind, Code);

    public bool Matches(HotkeyKind kind, int code) =>
        Kind == kind && Code == code;

    public bool SameAs(Hotkey other) =>
        Matches(other.Kind, other.Code);
}
