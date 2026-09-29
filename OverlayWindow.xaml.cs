using System.Windows;
using System.Windows.Input;

namespace MouseStudio;

// Small always-on-top label, top-left of the primary screen by default.
// Drag it with the left mouse button to move it; double-click it to open
// the main window. It never takes focus, so the game keeps receiving the
// keyboard.
public partial class OverlayWindow : Window
{
    private const double ScreenMargin = 15;

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    // Raised after the user drops the overlay at a new position.
    public event Action? Moved;

    // Raised when the user double-clicks the overlay.
    public event Action? OpenRequested;

    // A saved position is used only if it is still on one of the screens.
    public OverlayWindow(double? left, double? top)
    {
        InitializeComponent();

        if (left is double x && top is double y && IsOnScreen(x, y))
        {
            Left = x;
            Top = y;
        }
        else
        {
            var workArea = SystemParameters.WorkArea;

            Left = workArea.Left + ScreenMargin;
            Top = workArea.Top + ScreenMargin;
        }
    }

    // `profileName` is shown on a second line; null hides that line.
    public void SetMode(string mode, string? profileName)
    {
        ModeText.Text = $"Mode: {mode}";

        ProfileText.Text = $"Profile {profileName}";

        ProfileText.Visibility =
            profileName == null ? Visibility.Collapsed : Visibility.Visible;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.ClickCount == 2)
        {
            OpenRequested?.Invoke();

            return;
        }

        var oldLeft = Left;
        var oldTop = Top;

        // Returns when the button is released.
        DragMove();

        if (Left != oldLeft || Top != oldTop)
        {
            Moved?.Invoke();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;

        SetWindowLong(
            handle,
            GWL_EXSTYLE,
            GetWindowLong(handle, GWL_EXSTYLE)
                | WS_EX_TOOLWINDOW
                | WS_EX_NOACTIVATE
        );
    }

    private static bool IsOnScreen(double left, double top) =>
        left >= SystemParameters.VirtualScreenLeft
        && top >= SystemParameters.VirtualScreenTop
        && left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - ScreenMargin
        && top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - ScreenMargin;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
