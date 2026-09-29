using MouseStudio.Core.Input;

namespace MouseStudio.Core.Lua;

public class LuaApi
{
    private readonly Action<string> _logger;

    private readonly CancellationToken _cancellation;

    // The fraction of a pixel not yet sent, carried to the next move.
    private double _remainderX;

    private double _remainderY;

    public LuaApi(
        Action<string> logger,
        CancellationToken cancellation
    )
    {
        _logger = logger;

        _cancellation = cancellation;
    }

    // Every API call checks this, so a stopped script ends at its next call
    // (e.g. inside a "while IsMouseButtonPressed(1) do ... Sleep() end" loop).
    private void ThrowIfStopped()
    {
        _cancellation.ThrowIfCancellationRequested();
    }

    // Accepts fractions: 0.5 per call moves 1 pixel every second call.
    public void MoveMouseRelative(
        double x,
        double y
    )
    {
        ThrowIfStopped();

        _remainderX += x;
        _remainderY += y;

        var dx = (int)Math.Truncate(_remainderX);
        var dy = (int)Math.Truncate(_remainderY);

        _remainderX -= dx;
        _remainderY -= dy;

        if (dx == 0 && dy == 0)
        {
            return;
        }

        MouseController.MoveRelative(
            dx,
            dy
        );
    }

    public void Sleep(
        int milliseconds
    )
    {
        ThrowIfStopped();

        // Wakes up early when the script is stopped.
        _cancellation.WaitHandle.WaitOne(
            Math.Max(milliseconds, 0)
        );

        ThrowIfStopped();
    }

    public void OutputLogMessage(
        string message
    )
    {
        ThrowIfStopped();

        _logger(message);
    }

    public void ClickMouseButton(
        int button
    )
    {
        ThrowIfStopped();

        switch (button)
        {
            case 1:

                MouseController.LeftClick();

                break;

            case 2:

                MouseController.RightClick();

                break;
        }
    }

    public bool IsMouseButtonPressed(int button)
    {
        ThrowIfStopped();

        return button switch
        {
            1 => InputState.LeftMouseDown,

            2 => InputState.RightMouseDown,

            4 => InputState.BackMouseDown,

            5 => InputState.ForwardMouseDown,

            _ => false
        };
    }

    public void PressMouseButton(int button)
    {
        ThrowIfStopped();

        switch (button)
        {
            case 1:
                MouseController.LeftDown();
                break;

            case 2:
                MouseController.RightDown();
                break;
        }
    }

    public void ReleaseMouseButton(int button)
    {
        ThrowIfStopped();

        switch (button)
        {
            case 1:
                MouseController.LeftUp();
                break;

            case 2:
                MouseController.RightUp();
                break;
        }
    }

    // "capslock", "numlock" or "scrolllock", as in Logitech scripts.
    public bool IsKeyLockOn(string key)
    {
        ThrowIfStopped();

        return LockKeys.IsOn(key)
            ?? throw new ArgumentException(
                $"IsKeyLockOn: unknown key \"{key}\" (use capslock, numlock or scrolllock)."
            );
    }

    public long GetTickCount()
    {
        ThrowIfStopped();

        return Environment.TickCount64;
    }
}