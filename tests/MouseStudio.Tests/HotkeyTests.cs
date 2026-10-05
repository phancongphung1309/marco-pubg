using MouseStudio.Core.Profiles;
using Xunit;

namespace MouseStudio.Tests;

public class HotkeyTests
{
    [Theory]
    [InlineData(Hotkey.MouseMiddle)]
    [InlineData(Hotkey.MouseBack)]
    [InlineData(Hotkey.MouseForward)]
    public void IsAllowed_MiddleAndSideButtons_AreAccepted(int button)
    {
        Assert.True(Hotkey.IsAllowed(HotkeyKind.Mouse, button));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void IsAllowed_LeftAndRightButtons_AreRejected(int button)
    {
        Assert.False(Hotkey.IsAllowed(HotkeyKind.Mouse, button));
    }

    [Theory]
    [InlineData(0x70)] // F1
    [InlineData(0x7B)] // F12
    [InlineData(0x1B)] // Escape
    public void IsAllowed_ProfileKeysAndEscape_AreRejected(int vkCode)
    {
        Assert.False(Hotkey.IsAllowed(HotkeyKind.Keyboard, vkCode));
    }

    [Theory]
    [InlineData(0x56)] // V
    [InlineData(0x14)] // Caps Lock
    [InlineData(0xA0)] // Left Shift
    public void IsAllowed_OtherKeys_AreAccepted(int vkCode)
    {
        Assert.True(Hotkey.IsAllowed(HotkeyKind.Keyboard, vkCode));
    }
}
