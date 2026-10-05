using MouseStudio.Core.Profiles;

namespace MouseStudio.Core.Input;

// Turns physical key and button events into "trigger held" / "released",
// for either a single hotkey or a combo. Not thread-safe: the hooks call it
// on the UI thread.
public class TriggerTracker
{
    private readonly Func<long> _clock;

    private TriggerMode _mode;

    private Hotkey _hotkey = Hotkey.CreateDefault();

    private ComboTrigger _combo = ComboTrigger.CreateDefault();

    // When the combo's hold key went down; null while it is up.
    private long? _holdStart;

    public bool IsDown { get; private set; }

    public event Action? Pressed;

    public event Action? Released;

    // `clock` returns milliseconds; defaults to Environment.TickCount64.
    public TriggerTracker(Func<long>? clock = null)
    {
        _clock = clock ?? (() => Environment.TickCount64);
    }

    // The combo is read on every event, so later edits to its HoldMs
    // apply without calling this again.
    public void Configure(TriggerMode mode, Hotkey hotkey, ComboTrigger combo)
    {
        Release();

        _mode = mode;
        _hotkey = hotkey;
        _combo = combo;
        _holdStart = null;
    }

    public void OnDown(HotkeyKind kind, int code)
    {
        if (_mode == TriggerMode.Hotkey)
        {
            if (_hotkey.Matches(kind, code))
            {
                Press();
            }

            return;
        }

        if (_combo.HoldKey?.Matches(kind, code) == true)
        {
            _holdStart = _clock();
        }
        else if (_combo.PressKey?.Matches(kind, code) == true
            && _holdStart is long start
            && _clock() - start >= _combo.HoldMs)
        {
            Press();
        }
    }

    public void OnUp(HotkeyKind kind, int code)
    {
        if (_mode == TriggerMode.Hotkey)
        {
            if (_hotkey.Matches(kind, code))
            {
                Release();
            }

            return;
        }

        // Letting go of either combo key stops the movement.
        if (_combo.HoldKey?.Matches(kind, code) == true)
        {
            _holdStart = null;

            Release();
        }
        else if (_combo.PressKey?.Matches(kind, code) == true)
        {
            Release();
        }
    }

    private void Press()
    {
        if (IsDown)
        {
            return;
        }

        IsDown = true;

        Pressed?.Invoke();
    }

    private void Release()
    {
        if (!IsDown)
        {
            return;
        }

        IsDown = false;

        Released?.Invoke();
    }
}
