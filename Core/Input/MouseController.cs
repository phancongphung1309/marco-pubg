using System.Runtime.InteropServices;

namespace MouseStudio.Core.Input;

public static class MouseController
{
    private const uint MOUSEEVENTF_LEFTDOWN =
    0x0002;

    private const uint MOUSEEVENTF_LEFTUP =
        0x0004;

    private const uint MOUSEEVENTF_RIGHTDOWN =
        0x0008;

    private const uint MOUSEEVENTF_RIGHTUP =
        0x0010;

    private const int INPUT_MOUSE = 0;

    private const uint MOUSEEVENTF_MOVE = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;

        public int dy;

        public uint mouseData;

        public uint dwFlags;

        public uint time;

        public IntPtr dwExtraInfo;
    }

    [DllImport(
        "user32.dll",
        SetLastError = true
    )]
    private static extern uint SendInput(
        uint nInputs,
        INPUT[] pInputs,
        int cbSize
    );

    public static void MoveRelative(
        int x,
        int y
    )
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,

            mi = new MOUSEINPUT
            {
                dx = x,

                dy = y,

                dwFlags =
                    MOUSEEVENTF_MOVE
            }
        };

        SendInput(
            1,
            new[] { input },
            Marshal.SizeOf<INPUT>()
        );
    }

    private static void SendMouseFlag(
    uint flag
)
{
    var input = new INPUT
    {
        type = INPUT_MOUSE,

        mi = new MOUSEINPUT
        {
            dwFlags = flag
        }
    };

    SendInput(
        1,
        new[] { input },
        Marshal.SizeOf<INPUT>()
    );
}

public static void LeftDown()
{
    SendMouseFlag(
        MOUSEEVENTF_LEFTDOWN
    );
}

public static void LeftUp()
{
    SendMouseFlag(
        MOUSEEVENTF_LEFTUP
    );
}

public static void LeftClick()
{
    LeftDown();

    Thread.Sleep(20);

    LeftUp();
}

public static void RightDown()
{
    SendMouseFlag(
        MOUSEEVENTF_RIGHTDOWN
    );
}

public static void RightUp()
{
    SendMouseFlag(
        MOUSEEVENTF_RIGHTUP
    );
}

public static void RightClick()
{
    RightDown();

    Thread.Sleep(20);

    RightUp();
}
}
