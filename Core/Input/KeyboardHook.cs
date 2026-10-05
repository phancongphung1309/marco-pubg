using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseStudio.Core.Input;

// Global low-level keyboard hook. Keys are observed, never swallowed,
// so the focused application (e.g. the game) still receives them.
public class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private IntPtr _hookId = IntPtr.Zero;

    private readonly LowLevelKeyboardProc _hookCallback;

    // Keys currently held, so auto-repeat does not raise KeyDown again.
    private readonly HashSet<int> _pressed = new();

    // Raised once per physical press, with the virtual-key code.
    public event Action<int>? KeyDown;

    // Raised when a held key is released, with the virtual-key code.
    public event Action<int>? KeyUp;

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public KeyboardHook()
    {
        _hookCallback = HookCallback;
    }

    public void Start()
    {
        if (_hookId != IntPtr.Zero)
        {
            return;
        }

        using var process = Process.GetCurrentProcess();

        using var module = process.MainModule;

        _hookId = SetWindowsHookEx(
            WH_KEYBOARD_LL,
            _hookCallback,
            GetModuleHandle(module?.ModuleName),
            0
        );
    }

    public void Stop()
    {
        if (_hookId == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_hookId);

        _hookId = IntPtr.Zero;

        _pressed.Clear();
    }

    private IntPtr HookCallback(
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    )
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();

            var vkCode = (int)Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam).vkCode;

            switch (message)
            {
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    if (_pressed.Add(vkCode))
                    {
                        KeyDown?.Invoke(vkCode);
                    }
                    break;

                case WM_KEYUP:
                case WM_SYSKEYUP:
                    if (_pressed.Remove(vkCode))
                    {
                        KeyUp?.Invoke(vkCode);
                    }
                    break;
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Stop();
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    );

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hMod,
        uint dwThreadId
    );

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}
