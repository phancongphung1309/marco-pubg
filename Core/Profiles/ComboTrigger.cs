namespace MouseStudio.Core.Profiles;

public enum TriggerMode
{
    // One hotkey, held.
    Hotkey,

    // Hold one key long enough, then press another.
    Combo
}

// E.g. aim first (hold right click for 50 ms), then fire (left click):
// the movement runs while the second key is held.
public class ComboTrigger
{
    public const int MaxHoldMs = 60000;

    public Hotkey? HoldKey { get; set; }

    public int HoldMs { get; set; }

    public Hotkey? PressKey { get; set; }

    public static ComboTrigger CreateDefault() => new()
    {
        HoldKey = Hotkey.Mouse(Hotkey.MouseRight),
        HoldMs = 50,
        PressKey = Hotkey.Mouse(Hotkey.MouseLeft)
    };

    public static bool IsValidHoldMs(int ms) => ms is >= 0 and <= MaxHoldMs;

    public bool IsValid =>
        HoldKey is { } hold
        && PressKey is { } press
        && Hotkey.IsAllowed(hold.Kind, hold.Code, allowLeftRight: true)
        && Hotkey.IsAllowed(press.Kind, press.Code, allowLeftRight: true)
        && !hold.SameAs(press)
        && IsValidHoldMs(HoldMs);
}
