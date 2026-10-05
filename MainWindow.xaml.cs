using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using MouseStudio.Core.Lua;
using MouseStudio.Core.Input;
using MouseStudio.Core.Localization;
using MouseStudio.Core.Profiles;
namespace MouseStudio;

public partial class MainWindow : Window
{
    private readonly MouseHook _mouseHook;
    private readonly KeyboardHook _keyboardHook;
    private readonly LuaEngine _luaEngine;
    private readonly ProfileStore _profileStore;

    private ProfileData _profiles;

    // Null while overlay mode is off.
    private OverlayWindow? _overlay;

    // Created the first time the window is hidden to the tray.
    private System.Windows.Forms.NotifyIcon? _trayIcon;

    // Set while the combo box is refilled from code, so that
    // SelectionChanged does not treat it as the user switching profile.
    private bool _updatingProfileList;

    // Edits to the configuration fields are saved once typing pauses.
    private readonly DispatcherTimer _autoSaveTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(500)
    };

    // The profile the pending edits belong to; null when nothing is pending.
    private MovementProfile? _editedProfile;

    // Set while ShowProfile fills the fields, so that is not seen as an edit.
    private bool _showingProfile;

    // Rebuilds the status line text, so it can follow a language change.
    private Func<string> _statusText = () => Loc.T("StatusStopped");

    private static readonly string ScriptsFolder =
        Path.Combine(
            AppContext.BaseDirectory,
            "Scripts"
        );

    private static readonly string ProfilesPath =
        Path.Combine(
            AppContext.BaseDirectory,
            "profiles.json"
        );

    private MovementProfile SelectedProfile =>
        (MovementProfile)ProfileComboBox.SelectedItem;

    public MainWindow()
    {
        InitializeComponent();

        _autoSaveTimer.Tick += (_, _) => FlushAutoSave();

        foreach (var box in new[]
        {
            MoveXTextBox,
            MoveYTextBox,
            IntervalTextBox,
            ActiveDurationTextBox,
            RepeatDelayTextBox
        })
        {
            box.TextChanged += ConfigField_TextChanged;
        }

        _luaEngine =
            new LuaEngine();

        _mouseHook =
            new MouseHook();

        _profileStore =
            new ProfileStore(ProfilesPath);

        _luaEngine.LogReceived +=
            OnLuaLogReceived;

        _mouseHook.MouseDown +=
            OnMouseDown;

        _mouseHook.MouseUp +=
            OnMouseUp;

        _mouseHook.Start();

        _keyboardHook =
            new KeyboardHook();

        _keyboardHook.KeyDown +=
            OnKeyDown;

        _keyboardHook.Start();

        _profiles =
            _profileStore.Load(out var loadError);

        Loc.Instance.SetLanguage(_profiles.Language);

        Loc.Instance.LanguageChanged +=
            OnLanguageChanged;

        Log(
            Loc.T("LogStarted")
        );

        if (loadError != null)
        {
            Log(
                Loc.T("LogLoadError", loadError)
            );
        }

        RefreshProfileList();

        ShowProfile(SelectedProfile);

        StartScript(SelectedProfile);
    }

    // Virtual-key codes of the profile hotkeys: F1 loads the first
    // profile in the list, F2 the second, ... F12 the twelfth.
    private const int VK_F1 = 0x70;
    private const int ProfileHotkeyCount = ProfileData.MaxProfiles;

    // Called on the UI thread by the keyboard hook: must return quickly,
    // so the profile switch is deferred and script events are only queued.
    private void OnKeyDown(int vkCode)
    {
        var index = vkCode - VK_F1;

        if (index >= 0 && index < ProfileHotkeyCount)
        {
            Dispatcher.BeginInvoke(() => LoadProfileByHotkey(index));

            return;
        }

        // Keys other than profile hotkeys also go to the script.
        _luaEngine.Dispatch(
            "KEY_PRESSED",
            vkCode
        );
    }

    private void LoadProfileByHotkey(int index)
    {
        if (index >= _profiles.Profiles.Count)
        {
            Log(
                Loc.T("LogNoProfile", index + 1)
            );

            return;
        }

        var profile = _profiles.Profiles[index];

        SelectProfile(profile);

        Log(
            Loc.T("LogHotkeyLoaded", index + 1, profile.Name)
        );
    }

    // Called on the UI thread by the mouse hook: must return quickly,
    // so the event is only queued for the Lua worker thread.
    private void OnMouseDown(int button)
    {
        _luaEngine.Dispatch(
            "MOUSE_BUTTON_PRESSED",
            button
        );
    }

    private void OnMouseUp(int button)
    {
        _luaEngine.Dispatch(
            "MOUSE_BUTTON_RELEASED",
            button
        );
    }

    protected override void OnClosing(
        CancelEventArgs e
    )
    {
        FlushAutoSave();

        base.OnClosing(e);
    }

    protected override void OnClosed(
        EventArgs e
    )
    {
        Loc.Instance.LanguageChanged -=
            OnLanguageChanged;

        _overlay?.Close();

        // Otherwise the icon lingers in the tray until hovered.
        _trayIcon?.Dispose();

        _mouseHook.Dispose();

        _keyboardHook.Dispose();

        _luaEngine.Dispose();

        base.OnClosed(e);
    }

    // ===== Profiles =====

    private void RefreshProfileList()
    {
        _updatingProfileList = true;

        ProfileComboBox.ItemsSource = null;
        ProfileComboBox.ItemsSource = _profiles.Profiles;
        ProfileComboBox.SelectedItem =
            _profiles.Find(_profiles.SelectedProfile);

        _updatingProfileList = false;

        UpdateOverlay();

        NewProfileButton.IsEnabled =
            _profiles.CanAddProfile;

        DeleteProfileButton.IsEnabled =
            _profiles.Profiles.Count > 1;
    }

    private void ShowProfile(
        MovementProfile profile
    )
    {
        _showingProfile = true;

        MoveXTextBox.Text = profile.MoveX.ToString(CultureInfo.InvariantCulture);
        MoveYTextBox.Text = profile.MoveY.ToString(CultureInfo.InvariantCulture);
        IntervalTextBox.Text = profile.Interval.ToString();
        ActiveDurationTextBox.Text = profile.ActiveDuration.ToString();
        RepeatDelayTextBox.Text = profile.RepeatDelay.ToString();

        _showingProfile = false;
    }

    // Makes the profile current and runs the script with its values.
    private void SelectProfile(
        MovementProfile profile
    )
    {
        // Keep edits made to the previous profile just before switching.
        FlushAutoSave();

        _profiles.SelectedProfile = profile.Name;

        RefreshProfileList();

        ShowProfile(profile);

        SaveProfiles();

        StartScript(profile);
    }

    private void SaveProfiles()
    {
        try
        {
            _profileStore.Save(_profiles);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log(
                Loc.T("LogSaveError", ex.Message)
            );
        }
    }

    // ===== Auto save =====

    private void ConfigField_TextChanged(
        object sender,
        TextChangedEventArgs e
    )
    {
        if (_showingProfile)
        {
            return;
        }

        _editedProfile = SelectedProfile;

        // Restart the countdown on every keystroke.
        _autoSaveTimer.Stop();
        _autoSaveTimer.Start();
    }

    // Saves pending edits and reloads the script with them. Invalid values
    // (e.g. an empty field) are not saved; the last valid ones stay in use.
    private void FlushAutoSave()
    {
        _autoSaveTimer.Stop();

        var profile = _editedProfile;

        _editedProfile = null;

        if (profile == null)
        {
            return;
        }

        if (!TryReadFields(out var fields, out var error))
        {
            Log(
                Loc.T("LogNotSaved", error)
            );

            return;
        }

        if (fields.MoveX == profile.MoveX
            && fields.MoveY == profile.MoveY
            && fields.Interval == profile.Interval
            && fields.ActiveDuration == profile.ActiveDuration
            && fields.RepeatDelay == profile.RepeatDelay)
        {
            return;
        }

        profile.MoveX = fields.MoveX;
        profile.MoveY = fields.MoveY;
        profile.Interval = fields.Interval;
        profile.ActiveDuration = fields.ActiveDuration;
        profile.RepeatDelay = fields.RepeatDelay;

        SaveProfiles();

        Log(
            Loc.T("LogSaved", profile.Name)
        );

        // Reload the script so the saved values take effect immediately.
        if (profile == SelectedProfile)
        {
            StartScript(profile);
        }
    }

    private void ProfileComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (_updatingProfileList
            || ProfileComboBox.SelectedItem is not MovementProfile profile)
        {
            return;
        }

        SelectProfile(profile);

        Log(
            Loc.T("LogSelected", profile.Name)
        );
    }

    private void NewProfile_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        if (!_profiles.CanAddProfile)
        {
            return;
        }

        var dialog = new ProfileNameDialog(
            this,
            Loc.T("NewProfileTitle"),
            "",
            name => _profiles.ValidateName(name)
        );

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        // New profiles start from the default values, not the current fields.
        var profile = MovementProfile.CreateDefault();

        profile.Name = dialog.ProfileName;

        _profiles.Profiles.Add(profile);

        SelectProfile(profile);

        Log(
            Loc.T("LogCreated", profile.Name)
        );
    }

    private void RenameProfile_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        var profile = SelectedProfile;

        var dialog = new ProfileNameDialog(
            this,
            Loc.T("RenameProfileTitle"),
            profile.Name,
            name => _profiles.ValidateName(name, renaming: profile)
        );

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var oldName = profile.Name;

        profile.Name = dialog.ProfileName;
        _profiles.SelectedProfile = profile.Name;

        RefreshProfileList();

        SaveProfiles();

        Log(
            Loc.T("LogRenamed", oldName, profile.Name)
        );
    }

    private void DeleteProfile_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        var profile = SelectedProfile;

        if (_profiles.Profiles.Count <= 1)
        {
            return;
        }

        var answer = MessageBox.Show(
            Loc.T("ConfirmDelete", profile.Name),
            "Mouse Studio",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        var index = _profiles.Profiles.IndexOf(profile);

        _profiles.Profiles.RemoveAt(index);

        SelectProfile(
            _profiles.Profiles[Math.Min(index, _profiles.Profiles.Count - 1)]
        );

        Log(
            Loc.T("LogDeleted", profile.Name)
        );
    }

    private void ExportProfiles_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        // Exports saved values, so save any pending edits first.
        FlushAutoSave();

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = Loc.T("ExportTitle"),
            Filter = Loc.T("ProfileFileFilter"),
            FileName = $"mouse-studio-profiles-{DateTime.Now:yyyy-MM-dd}.json"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ProfileStore.Export(_profiles, dialog.FileName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(Loc.T("ExportFailed", ex.Message));

            return;
        }

        Log(
            Loc.T("LogExported", _profiles.Profiles.Count, dialog.FileName)
        );
    }

    private void ImportProfiles_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("ImportTitle"),
            Filter = Loc.T("ProfileFileFilter")
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var imported = ProfileStore.Import(dialog.FileName, out var error);

        if (imported == null)
        {
            MessageBox.Show(
                error,
                "Mouse Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            return;
        }

        var answer = MessageBox.Show(
            Loc.T("ConfirmImport", _profiles.Profiles.Count, imported.Profiles.Count),
            "Mouse Studio",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        // The overlay position is an app setting, not part of the import.
        _profiles.Profiles = imported.Profiles;

        SelectProfile(_profiles.Find(imported.SelectedProfile)!);

        Log(
            Loc.T("LogImported", imported.Profiles.Count, dialog.FileName)
        );
    }

    // ===== Donate =====

    private void Donate_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        new DonateWindow(this).ShowDialog();
    }

    // ===== Language =====

    // Switches between English and Vietnamese and remembers the choice.
    private void Language_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        Loc.Instance.SetLanguage(
            Loc.Instance.Language == Loc.English ? Loc.Vietnamese : Loc.English
        );

        _profiles.Language = Loc.Instance.Language;

        SaveProfiles();

        Log(
            Loc.T("LogLanguage")
        );
    }

    // XAML texts follow the language through bindings; these are set from code.
    private void OnLanguageChanged()
    {
        UpdateOverlayButton();

        UpdateOverlay();

        StatusText.Text = _statusText();

        if (_trayIcon != null)
        {
            _trayIcon.ContextMenuStrip?.Dispose();
            _trayIcon.ContextMenuStrip = CreateTrayMenu();
        }
    }

    // ===== Overlay =====

    private void Overlay_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        if (_overlay == null)
        {
            ShowOverlay();

            HideToTray();
        }
        else
        {
            HideOverlay();
        }
    }

    private void ShowOverlay()
    {
        _overlay = new OverlayWindow(
            _profiles.OverlayLeft,
            _profiles.OverlayTop
        );

        _overlay.Moved += OnOverlayMoved;
        _overlay.OpenRequested += RestoreFromTray;

        UpdateOverlay();

        _overlay.Show();

        UpdateOverlayButton();
    }

    private void UpdateOverlayButton()
    {
        OverlayButton.Content =
            Loc.T(_overlay == null ? "ShowOverlay" : "HideOverlay");
    }

    private void UpdateOverlay()
    {
        _overlay?.SetMode(Loc.T("ModeMovement"), SelectedProfile.Name);
    }

    private void HideOverlay()
    {
        if (_overlay == null)
        {
            return;
        }

        _overlay.Close();

        _overlay = null;

        UpdateOverlayButton();
    }

    private void OnOverlayMoved()
    {
        _profiles.OverlayLeft = _overlay!.Left;
        _profiles.OverlayTop = _overlay.Top;

        SaveProfiles();
    }

    // ===== Tray =====

    // Removes the window from the taskbar; the tray icon brings it back.
    private void HideToTray()
    {
        _trayIcon ??= CreateTrayIcon();

        _trayIcon.Visible = true;

        Hide();
    }

    private void RestoreFromTray()
    {
        Show();

        WindowState = WindowState.Normal;

        Activate();

        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
        }
    }

    private System.Windows.Forms.NotifyIcon CreateTrayIcon()
    {
        var trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!),
            Text = "Mouse Studio",
            ContextMenuStrip = CreateTrayMenu()
        };

        trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RestoreFromTray();
            }
        };

        return trayIcon;
    }

    private System.Windows.Forms.ContextMenuStrip CreateTrayMenu()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();

        menu.Items.Add(Loc.T("TrayOpen"), null, (_, _) => RestoreFromTray());
        menu.Items.Add(Loc.T("HideOverlay"), null, (_, _) => HideOverlay());
        menu.Items.Add(Loc.T("TrayExit"), null, (_, _) => Close());

        return menu;
    }

    // ===== Script =====

    private void StartScript(
        MovementProfile config
    )
    {
        const string scriptName = "movement.lua";

        var scriptPath =
            Path.Combine(ScriptsFolder, scriptName);

        if (!File.Exists(scriptPath))
        {
            // Don't leave the previous profile's script running.
            _luaEngine.Stop();

            SetStatus(false, () => Loc.T("StatusNotFound", scriptName));

            MessageBox.Show(
                Loc.T("ScriptNotFound", scriptPath)
            );

            return;
        }

        // Load errors are reported through LogReceived.
        _luaEngine.Start(
            scriptPath,
            config.GetLuaGlobals()
        );

        SetStatus(true, () => Loc.T("StatusRunning", scriptName));
    }

    private void SetStatus(
        bool running,
        Func<string> text
    )
    {
        var brush = (System.Windows.Media.Brush)FindResource(
            running ? "SuccessBrush" : "MutedBrush"
        );

        StatusDot.Fill = brush;

        _statusText = text;

        StatusText.Text = text();
    }

    // The returned profile has no name; it only carries the field values.
    private bool TryReadFields(
        out MovementProfile config,
        out string error
    )
    {
        config = new MovementProfile();

        if (!TryParseMove(MoveXTextBox.Text, out var moveX))
        {
            error = Loc.T("ErrMoveX");
            return false;
        }

        if (!TryParseMove(MoveYTextBox.Text, out var moveY))
        {
            error = Loc.T("ErrMoveY");
            return false;
        }

        if (!int.TryParse(IntervalTextBox.Text, out var interval))
        {
            error = Loc.T("ErrInterval");
            return false;
        }

        if (!int.TryParse(ActiveDurationTextBox.Text, out var activeDuration))
        {
            error = Loc.T("ErrActiveDuration");
            return false;
        }

        if (!int.TryParse(RepeatDelayTextBox.Text, out var repeatDelay))
        {
            error = Loc.T("ErrRepeatDelay");
            return false;
        }

        if (activeDuration < 1)
        {
            error = Loc.T("ErrActiveDurationMin");
            return false;
        }

        if (repeatDelay < 0)
        {
            error = Loc.T("ErrRepeatDelayMin");
            return false;
        }

        if (interval < 1)
        {
            error = Loc.T("ErrIntervalMin");
            return false;
        }

        config = new MovementProfile
        {
            MoveX = moveX,
            MoveY = moveY,
            Interval = interval,
            ActiveDuration = activeDuration,
            RepeatDelay = repeatDelay
        };

        error = "";
        return true;
    }

    // Accepts "1.5" or "1,5" whatever the Windows number format is.
    private static bool TryParseMove(string text, out double value) =>
        double.TryParse(
            text.Trim().Replace(',', '.'),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        )
        && double.IsFinite(value);

    private void OnLuaLogReceived(
        string message
    )
    {
        // BeginInvoke: never block the Lua worker on the UI thread
        // (a blocking Invoke would deadlock with Dispose() during shutdown).
        Dispatcher.BeginInvoke(() =>
        {
            Log(message);
        });
    }

    private void Log(
        string message
    )
    {
        ConsoleTextBox.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"
        );

        ConsoleTextBox.ScrollToEnd();
    }

    private void ClearConsole_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        ConsoleTextBox.Clear();
    }
}
