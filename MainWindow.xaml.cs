using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using MouseStudio.Core.Lua;
using MouseStudio.Core.Input;
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

        Log(
            "Mouse Studio started."
        );

        _profiles =
            _profileStore.Load(out var loadError);

        if (loadError != null)
        {
            Log(
                $"ERROR: {loadError} Starting with the default profile."
            );
        }

        RefreshProfileList();

        ShowProfile(SelectedProfile);

        StartScript(SelectedProfile);
    }

    // Virtual-key codes of the profile hotkeys: F1 loads the first
    // profile in the list, F2 the second, ... F12 the twelfth.
    // While an Auto profile is selected, F1 switches its gun instead
    // and F2 to F12 do nothing.
    private const int VK_F1 = 0x70;
    private const int ProfileHotkeyCount = ProfileData.MaxProfiles;

    // Called on the UI thread by the keyboard hook: must return quickly,
    // so the profile switch is deferred and script events are only queued.
    private void OnKeyDown(int vkCode)
    {
        var index = vkCode - VK_F1;

        if (index >= 0 && index < ProfileHotkeyCount)
        {
            if (SelectedProfile.Feature != ProfileFeature.Auto)
            {
                Dispatcher.BeginInvoke(() => LoadProfileByHotkey(index));
            }
            else if (index == 0)
            {
                Dispatcher.BeginInvoke(ToggleGunByHotkey);
            }

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
                $"F{index + 1}: no profile #{index + 1}."
            );

            return;
        }

        var profile = _profiles.Profiles[index];

        SelectProfile(profile);

        Log(
            $"F{index + 1}: profile loaded: {profile.Name}"
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

        MovementFeatureButton.IsChecked = profile.Feature == ProfileFeature.Movement;
        AutoFeatureButton.IsChecked = profile.Feature == ProfileFeature.Auto;

        GunM416Button.IsChecked = profile.AutoGun == AutoGun.M416;
        GunBerylButton.IsChecked = profile.AutoGun == AutoGun.Beryl;

        MovementPanel.Visibility =
            profile.Feature == ProfileFeature.Movement ? Visibility.Visible : Visibility.Collapsed;
        AutoPanel.Visibility =
            profile.Feature == ProfileFeature.Auto ? Visibility.Visible : Visibility.Collapsed;

        _showingProfile = false;
    }

    // ===== Feature =====

    private void Feature_Checked(
        object sender,
        RoutedEventArgs e
    )
    {
        if (_showingProfile)
        {
            return;
        }

        var feature = sender == AutoFeatureButton
            ? ProfileFeature.Auto
            : ProfileFeature.Movement;

        var profile = SelectedProfile;

        if (profile.Feature == feature)
        {
            return;
        }

        // Keep movement edits typed just before switching.
        FlushAutoSave();

        profile.Feature = feature;

        ShowProfile(profile);

        UpdateOverlay();

        SaveProfiles();

        Log(
            $"Profile {profile.Name}: {feature}"
        );

        StartScript(profile);
    }

    // ===== Auto =====

    private void AutoOption_Checked(
        object sender,
        RoutedEventArgs e
    )
    {
        if (_showingProfile)
        {
            return;
        }

        var profile = SelectedProfile;

        profile.AutoGun = sender == GunBerylButton
            ? AutoGun.Beryl
            : AutoGun.M416;

        ApplyAutoSettings(profile);
    }

    // F1 on an Auto profile: M416 <-> Beryl.
    private void ToggleGunByHotkey()
    {
        var profile = SelectedProfile;

        // The profile may have changed since the key was pressed.
        if (profile.Feature != ProfileFeature.Auto)
        {
            return;
        }

        profile.AutoGun =
            profile.AutoGun == AutoGun.M416 ? AutoGun.Beryl : AutoGun.M416;

        ShowProfile(profile);

        ApplyAutoSettings(profile);
    }

    // Saves the Auto settings and hands them to the running script
    // without restarting it.
    private void ApplyAutoSettings(
        MovementProfile profile
    )
    {
        SaveProfiles();

        UpdateOverlay();

        _luaEngine.SetGlobals(profile.GetLuaGlobals());

        Log(
            $"Profile {profile.Name}: {DescribeAuto(profile)}"
        );
    }

    private static string DescribeAuto(
        MovementProfile profile
    ) =>
        $"Auto · {profile.AutoGun}";

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
                $"ERROR: Could not save profiles: {ex.Message}"
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
                $"Not saved: {error}"
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
            $"Profile saved: {profile.Name}"
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
            $"Profile selected: {profile.Name}"
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
            "New Profile",
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
            $"Profile created: {profile.Name}"
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
            "Rename Profile",
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
            $"Profile renamed: {oldName} -> {profile.Name}"
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
            $"Delete profile \"{profile.Name}\"?",
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
            $"Profile deleted: {profile.Name}"
        );
    }

    private const string ProfileFileFilter =
        "Mouse Studio profiles (*.json)|*.json|All files (*.*)|*.*";

    private void ExportProfiles_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        // Exports saved values, so save any pending edits first.
        FlushAutoSave();

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Export Profiles",
            Filter = ProfileFileFilter,
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
            MessageBox.Show($"Could not export profiles: {ex.Message}");

            return;
        }

        Log(
            $"Exported {_profiles.Profiles.Count} profile(s) to {dialog.FileName}"
        );
    }

    private void ImportProfiles_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Profiles",
            Filter = ProfileFileFilter
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
            $"Replace all {_profiles.Profiles.Count} current profile(s) with the "
                + $"{imported.Profiles.Count} profile(s) from this file?\n\n"
                + "Current profiles will be lost.",
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
            $"Imported {imported.Profiles.Count} profile(s) from {dialog.FileName}"
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

        OverlayButton.Content = "Hide Overlay";
    }

    private void UpdateOverlay()
    {
        var profile = SelectedProfile;

        if (profile.Feature == ProfileFeature.Auto)
        {
            _overlay?.SetMode($"Auto - {profile.AutoGun}", null);
        }
        else
        {
            _overlay?.SetMode("Movement", profile.Name);
        }
    }

    private void HideOverlay()
    {
        if (_overlay == null)
        {
            return;
        }

        _overlay.Close();

        _overlay = null;

        OverlayButton.Content = "Show Overlay";
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
        var menu = new System.Windows.Forms.ContextMenuStrip();

        menu.Items.Add("Open Mouse Studio", null, (_, _) => RestoreFromTray());
        menu.Items.Add("Hide Overlay", null, (_, _) => HideOverlay());
        menu.Items.Add("Exit", null, (_, _) => Close());

        var trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!),
            Text = "Mouse Studio",
            ContextMenuStrip = menu
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

    // ===== Script =====

    private void StartScript(
        MovementProfile config
    )
    {
        var scriptName = config.GetScriptFileName();

        var scriptPath =
            Path.Combine(ScriptsFolder, scriptName);

        if (!File.Exists(scriptPath))
        {
            // Don't leave the previous profile's script running.
            _luaEngine.Stop();

            SetStatus(false, $"Stopped · {scriptName} not found");

            MessageBox.Show(
                $"Lua script not found: {scriptPath}"
            );

            return;
        }

        // Load errors are reported through LogReceived.
        _luaEngine.Start(
            scriptPath,
            config.GetLuaGlobals()
        );

        SetStatus(true, $"{config.Feature} running · {scriptName}");
    }

    private void SetStatus(
        bool running,
        string text
    )
    {
        var brush = (System.Windows.Media.Brush)FindResource(
            running ? "SuccessBrush" : "MutedBrush"
        );

        StatusDot.Fill = brush;

        StatusText.Text = text;
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
            error = "MOVE_X must be a number.";
            return false;
        }

        if (!TryParseMove(MoveYTextBox.Text, out var moveY))
        {
            error = "MOVE_Y must be a number.";
            return false;
        }

        if (!int.TryParse(IntervalTextBox.Text, out var interval))
        {
            error = "INTERVAL must be a number.";
            return false;
        }

        if (!int.TryParse(ActiveDurationTextBox.Text, out var activeDuration))
        {
            error = "Active Duration must be a number.";
            return false;
        }

        if (!int.TryParse(RepeatDelayTextBox.Text, out var repeatDelay))
        {
            error = "Repeat Delay must be a number.";
            return false;
        }

        if (activeDuration < 1)
        {
            error = "Active Duration must be >= 1 ms.";
            return false;
        }

        if (repeatDelay < 0)
        {
            error = "Repeat Delay must be >= 0 ms.";
            return false;
        }

        if (interval < 1)
        {
            error = "INTERVAL must be >= 1.";
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
