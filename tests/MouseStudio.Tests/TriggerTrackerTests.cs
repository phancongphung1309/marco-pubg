using MouseStudio.Core.Input;
using MouseStudio.Core.Profiles;
using Xunit;

namespace MouseStudio.Tests;

public class TriggerTrackerTests
{
    private const HotkeyKind M = HotkeyKind.Mouse;

    private long _now;

    private readonly List<string> _events = new();

    private TriggerTracker Create(TriggerMode mode)
    {
        var tracker = new TriggerTracker(() => _now);

        tracker.Pressed += () => _events.Add("pressed");
        tracker.Released += () => _events.Add("released");

        // Combo: hold right 50 ms, then left.
        tracker.Configure(mode, Hotkey.CreateDefault(), ComboTrigger.CreateDefault());

        return tracker;
    }

    [Fact]
    public void Hotkey_PressAndRelease_RaisesBoth()
    {
        var tracker = Create(TriggerMode.Hotkey);

        tracker.OnDown(M, Hotkey.MouseForward);
        Assert.True(tracker.IsDown);

        tracker.OnUp(M, Hotkey.MouseForward);
        Assert.False(tracker.IsDown);

        Assert.Equal(new[] { "pressed", "released" }, _events);
    }

    [Fact]
    public void Hotkey_OtherButton_IsIgnored()
    {
        var tracker = Create(TriggerMode.Hotkey);

        tracker.OnDown(M, Hotkey.MouseBack);

        Assert.False(tracker.IsDown);
        Assert.Empty(_events);
    }

    [Fact]
    public void Combo_HoldLongEnoughThenPress_Triggers()
    {
        var tracker = Create(TriggerMode.Combo);

        tracker.OnDown(M, Hotkey.MouseRight);
        _now += 50;
        tracker.OnDown(M, Hotkey.MouseLeft);

        Assert.True(tracker.IsDown);
    }

    [Fact]
    public void Combo_PressTooSoon_DoesNotTrigger()
    {
        var tracker = Create(TriggerMode.Combo);

        tracker.OnDown(M, Hotkey.MouseRight);
        _now += 49;
        tracker.OnDown(M, Hotkey.MouseLeft);

        Assert.False(tracker.IsDown);
    }

    [Fact]
    public void Combo_PressWithoutHold_DoesNotTrigger()
    {
        var tracker = Create(TriggerMode.Combo);

        _now += 5000;
        tracker.OnDown(M, Hotkey.MouseLeft);

        Assert.False(tracker.IsDown);
    }

    [Fact]
    public void Combo_HoldReleasedBeforePress_DoesNotTrigger()
    {
        var tracker = Create(TriggerMode.Combo);

        tracker.OnDown(M, Hotkey.MouseRight);
        _now += 2000;
        tracker.OnUp(M, Hotkey.MouseRight);
        tracker.OnDown(M, Hotkey.MouseLeft);

        Assert.False(tracker.IsDown);
    }

    [Theory]
    [InlineData(Hotkey.MouseLeft)]
    [InlineData(Hotkey.MouseRight)]
    public void Combo_ReleasingEitherKey_Releases(int button)
    {
        var tracker = Create(TriggerMode.Combo);

        tracker.OnDown(M, Hotkey.MouseRight);
        _now += 50;
        tracker.OnDown(M, Hotkey.MouseLeft);
        tracker.OnUp(M, button);

        Assert.False(tracker.IsDown);
        Assert.Equal(new[] { "pressed", "released" }, _events);
    }

    [Fact]
    public void Combo_SecondPressWhileStillHolding_TriggersAgain()
    {
        var tracker = Create(TriggerMode.Combo);

        tracker.OnDown(M, Hotkey.MouseRight);
        _now += 50;
        tracker.OnDown(M, Hotkey.MouseLeft);
        tracker.OnUp(M, Hotkey.MouseLeft);
        tracker.OnDown(M, Hotkey.MouseLeft);

        Assert.True(tracker.IsDown);
    }

    [Fact]
    public void Configure_WhileDown_Releases()
    {
        var tracker = Create(TriggerMode.Hotkey);

        tracker.OnDown(M, Hotkey.MouseForward);
        tracker.Configure(TriggerMode.Combo, Hotkey.CreateDefault(), ComboTrigger.CreateDefault());

        Assert.False(tracker.IsDown);
        Assert.Equal(new[] { "pressed", "released" }, _events);
    }
}
