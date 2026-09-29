using System.Runtime.InteropServices;

namespace MouseStudio.Core.Input;

// Toggle state of the lock keys (the light on the keyboard).
public static class LockKeys
{
    private const int VK_CAPITAL = 0x14;
    private const int VK_NUMLOCK = 0x90;
    private const int VK_SCROLL = 0x91;

    // Accepts the names used by Logitech scripts: "capslock", "numlock", "scrolllock".
    // Returns null for any other name.
    public static bool? IsOn(string key)
    {
        int? vk = key.Trim().ToLowerInvariant() switch
        {
            "capslock" => VK_CAPITAL,
            "numlock" => VK_NUMLOCK,
            "scrolllock" => VK_SCROLL,
            _ => null
        };

        if (vk == null)
        {
            return null;
        }

        // The low bit is the toggle state.
        return (GetKeyState(vk.Value) & 1) != 0;
    }

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);
}
