using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MouseStudio.Core.Input;

public class MouseHook : IDisposable
{

    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;

    private const ushort XBUTTON1 = 0x0001;
    private const ushort XBUTTON2 = 0x0002;

    private const int WH_MOUSE_LL = 14;

    // Set on events sent by SendInput, e.g. the script's own clicks.
    private const uint LLMHF_INJECTED = 0x01;

    private const int WM_LBUTTONDOWN =
        0x0201;

    private const int WM_LBUTTONUP =
        0x0202;

    private const int WM_RBUTTONDOWN =
        0x0204;

    private const int WM_RBUTTONUP =
        0x0205;

    private const int WM_MBUTTONDOWN =
        0x0207;

    private const int WM_MBUTTONUP =
        0x0208;

    private IntPtr _hookId =
        IntPtr.Zero;

    private readonly LowLevelMouseProc
        _hookCallback;

    public event Action<int>?
        MouseDown;

    public event Action<int>?
        MouseUp;

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    public MouseHook()
    {
        _hookCallback =
            HookCallback;
    }

    public void Start()
    {
        if (
            _hookId != IntPtr.Zero
        )
        {
            return;
        }

        _hookId =
            SetHook(
                _hookCallback
            );
    }

    public void Stop()
    {
        if (
            _hookId == IntPtr.Zero
        )
        {
            return;
        }

        UnhookWindowsHookEx(
            _hookId
        );

        _hookId =
            IntPtr.Zero;
    }

    private static IntPtr SetHook(
        LowLevelMouseProc proc
    )
    {
        using var process =
            Process.GetCurrentProcess();

        using var module =
            process.MainModule;

        return SetWindowsHookEx(
            WH_MOUSE_LL,
            proc,
            GetModuleHandle(
                module?.ModuleName
            ),
            0
        );
    }

    private IntPtr HookCallback(
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    )
    {
        // Only physical buttons count, as in Logitech scripts: otherwise the
        // script releasing left during a cooldown would look like the user
        // letting go of a left-click trigger.
        if (nCode >= 0
            && (Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam).flags & LLMHF_INJECTED) == 0)
        {
            var message = wParam.ToInt32();

            switch (message)
            {
                case WM_LBUTTONDOWN:
                    InputState.LeftMouseDown = true;
                    MouseDown?.Invoke(1);
                    break;

                case WM_LBUTTONUP:
                    InputState.LeftMouseDown = false;
                    MouseUp?.Invoke(1);
                    break;

                case WM_RBUTTONDOWN:
                    InputState.RightMouseDown = true;
                    MouseDown?.Invoke(2);
                    break;

                case WM_RBUTTONUP:
                    InputState.RightMouseDown = false;
                    MouseUp?.Invoke(2);
                    break;

                case WM_MBUTTONDOWN:
                    InputState.MiddleMouseDown = true;
                    MouseDown?.Invoke(3);
                    break;

                case WM_MBUTTONUP:
                    InputState.MiddleMouseDown = false;
                    MouseUp?.Invoke(3);
                    break;

                case WM_XBUTTONDOWN:
                    {
                        var info =
                            Marshal.PtrToStructure<MSLLHOOKSTRUCT>(
                                lParam
                            );

                        var xButton =
                            (ushort)(
                                (info.mouseData >> 16)
                                & 0xFFFF
                            );

                        if (xButton == XBUTTON1)
                        {
                            InputState.BackMouseDown = true;

                            MouseDown?.Invoke(4);
                        }
                        else if (xButton == XBUTTON2)
                        {
                            InputState.ForwardMouseDown = true;

                            MouseDown?.Invoke(5);
                        }

                        break;
                    }

                case WM_XBUTTONUP:
                    {
                        var info =
                            Marshal.PtrToStructure<MSLLHOOKSTRUCT>(
                                lParam
                            );

                        var xButton =
                            (ushort)(
                                (info.mouseData >> 16)
                                & 0xFFFF
                            );

                        if (xButton == XBUTTON1)
                        {
                            InputState.BackMouseDown = false;

                            MouseUp?.Invoke(4);
                        }
                        else if (xButton == XBUTTON2)
                        {
                            InputState.ForwardMouseDown = false;

                            MouseUp?.Invoke(5);
                        }

                        break;
                    }
            }
        }

        return CallNextHookEx(
            _hookId,
            nCode,
            wParam,
            lParam
        );
    }
    
    public void Dispose()
    {
        Stop();
    }

    private delegate IntPtr
        LowLevelMouseProc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam
        );

    [DllImport(
        "user32.dll"
    )]
    private static extern IntPtr
        SetWindowsHookEx(
            int idHook,
            LowLevelMouseProc lpfn,
            IntPtr hMod,
            uint dwThreadId
        );

    [DllImport(
        "user32.dll"
    )]
    private static extern bool
        UnhookWindowsHookEx(
            IntPtr hhk
        );

    [DllImport(
        "user32.dll"
    )]
    private static extern IntPtr
        CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam
        );

    [DllImport(
        "kernel32.dll",
        CharSet = CharSet.Auto,
        SetLastError = true
    )]
    private static extern IntPtr
        GetModuleHandle(
            string? lpModuleName
        );
}