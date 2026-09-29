# Mouse Studio

A Windows desktop app (WPF, .NET 8) that runs Lua mouse scripts, in the style of Logitech G Hub scripts. It captures mouse and keyboard input with global hooks and hands the events to a Lua script, which can move the cursor and press buttons.

It has two features, chosen per profile:

- **Movement**: hold the **Forward** side mouse button to hold LEFT and drag the cursor by a fixed step. It stops after a set time and starts again after a delay, for as long as Forward is held.
- **Auto**: hold **RIGHT** to aim, then **LEFT** to fire. The cursor is pulled down following a recoil table for the selected gun (M416 or Beryl).

![Mouse Studio in Auto mode, with the Beryl gun selected and the script log in the console](app_screenshot.png)

## Requirements

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build (the published single-file build needs no install)
- Administrator rights: the app asks for them on start (`app.manifest`), so its hooks also work while an elevated game window has focus

## Build and run

```powershell
dotnet build MouseStudio.csproj
dotnet run --project MouseStudio.csproj
```

The scripts in `Scripts/` are copied next to the executable on build.

### Publish a single executable

```powershell
dotnet publish MouseStudio.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -p:DebugType=None -o Marco-Pubg
```

This writes `MouseStudio.exe` and the `Scripts` folder to `Marco-Pubg/`. Ship the whole folder.

### Tests

```powershell
dotnet test tests/MouseStudio.Tests
```

The xUnit tests cover profile data, name checks, and saving, loading, importing and exporting profiles.

## How to use

### 1. Start the app

Run `MouseStudio.exe` and accept the administrator prompt. Keep the `Scripts` folder next to the executable: the app runs the scripts in it.

On first start the app creates one profile, **Default**, in Movement mode. The status line at the bottom turns green and reads `Movement running · movement.lua` once the script is running. The console under it shows what the app and the script are doing.

### 2. Choose a feature

Click one of the two cards at the top:

- **Movement**: drags the cursor while you hold the Forward side button.
- **Auto**: pulls the cursor down while you aim and fire.

The choice is saved on the current profile, and the matching script starts right away. There is no Start button: the script runs as long as the app is open.

### 3. Use Movement

1. Set the values in the Movement panel:

   | Setting | Meaning | Default |
   | --- | --- | --- |
   | Horizontal movement | Pixels moved right per step (negative moves left). Fractions such as `0.5` work: they add up across steps. | 0 |
   | Vertical movement | Pixels moved down per step (negative moves up) | 5 |
   | Interval | Delay between two steps, in ms (at least 1) | 10 |
   | Active duration | How long each drag lasts, in ms (at least 1) | 5000 |
   | Repeat delay | Pause before the next drag, in ms (0 or more) | 1000 |

   Changes are saved half a second after you stop typing, and the script reloads with them. An invalid value (empty, or out of range) is not saved: the console says why, and the last valid values stay in use.

2. **Hold the Forward side button** (button 5, the front thumb button) on the mouse. While it is held, the app:
   1. holds the left button down and moves the cursor by one step every Interval, for Active duration;
   2. releases the left button and waits Repeat delay;
   3. starts again from step 1.

3. **Release Forward** to stop at once. The left button is released too.

### 4. Manage Movement profiles

Profiles let you keep several sets of Movement values and switch between them quickly. They are in the Movement panel:

- **New**: add a profile (up to 12). It starts with default values.
- **Rename**: change the name of the selected profile. Names must be unique.
- **Delete**: remove the selected profile, after a confirmation. The last profile cannot be deleted.
- **Drop-down list**: select the profile to use.
- **F1 to F12**: load the 1st to the 12th profile of the list, from anywhere, even while a game has focus. The order is the order of the list.

To back up profiles or move them to another PC:

- **Export** saves all profiles to a `.json` file.
- **Import** loads a `.json` file made by Export. The file is checked first; if it is valid, the app asks before it **replaces all current profiles** with the ones in the file.

Profiles are stored in `profiles.json` next to the executable.

### 5. Use Auto

1. Click the **Auto** card, then pick the gun: **M416** or **Beryl**.
2. In game, **hold RIGHT** to aim, then **hold LEFT** to fire. The cursor is pulled down following the recoil table of the selected gun, until LEFT is released.
3. Press **F1** to switch between M416 and Beryl without leaving the game. The console and the overlay show the new gun.

While Auto is selected, F2 to F12 do nothing and the profile list is hidden. To change profile, click **Movement** first.

The recoil tables are at the top of `Scripts/auto.lua`, if you want to tune them.

### 6. Play with the overlay

Click **Show Overlay** before starting the game:

- A small always-on-top box shows the current mode (and the profile, or the gun in Auto).
- The main window is hidden to the notification area (tray).
- Drag the box with the left button to move it. Its position is remembered.
- Double-click the box, or click the tray icon, to bring the main window back.
- Right-click the tray icon for **Open Mouse Studio**, **Hide Overlay** and **Exit**.

Click **Hide Overlay** to close the box.

### Troubleshooting

- **Nothing happens in game**: make sure the app runs as administrator, and that the status line is green. Check the console for script errors.
- **`Lua script not found`**: the `Scripts` folder is missing next to `MouseStudio.exe`. Copy it back from the published folder.
- **A value will not save**: read the `Not saved:` line in the console; it names the field and the allowed range.
- **Wrong direction**: use negative values for Horizontal (left) or Vertical (up) movement.

## Scripts

A script defines `OnEvent(event, arg)`, which Mouse Studio calls with:

| Event | `arg` |
| --- | --- |
| `PROFILE_ACTIVATED` | `0`, when the script starts |
| `MOUSE_BUTTON_PRESSED` / `MOUSE_BUTTON_RELEASED` | The button: 1 left, 2 right, 4 back, 5 forward |
| `KEY_PRESSED` | The virtual-key code (F1 to F12 are kept for profile hotkeys) |

The profile's settings are set as globals before the script runs: `MOVE_X`, `MOVE_Y`, `INTERVAL`, `ACTIVE_DURATION`, `REPEAT_DELAY` and `GUN_MODE` (`"m416"` or `"beryl"`).

Functions available to scripts (`Core/Lua/LuaApi.cs`):

| Function | Description |
| --- | --- |
| `MoveMouseRelative(x, y)` | Move the cursor; fractions carry over to the next call |
| `PressMouseButton(b)` / `ReleaseMouseButton(b)` | Hold or release button 1 (left) or 2 (right) |
| `ClickMouseButton(b)` | Click button 1 or 2 |
| `IsMouseButtonPressed(b)` | Whether button 1, 2, 4 or 5 is held |
| `IsKeyLockOn(key)` | `"capslock"`, `"numlock"` or `"scrolllock"` |
| `Sleep(ms)` | Wait; ends early when the script is stopped |
| `GetTickCount()` | Milliseconds since Windows started |
| `OutputLogMessage(text)` | Write to the app console |

`movement.lua` runs Movement profiles and `auto.lua` runs Auto profiles. The recoil tables are at the top of `auto.lua`.

## Project layout

```
MainWindow.xaml(.cs)       Main window: profiles, features, console
OverlayWindow.xaml(.cs)    Always-on-top status overlay
ProfileNameDialog.xaml     New / rename profile dialog
Core/Input/                Global mouse and keyboard hooks, mouse output
Core/Lua/                  Lua engine (NLua) and the script API
Core/Profiles/             Profile model and profiles.json storage
Scripts/                   Lua scripts copied next to the executable
tests/MouseStudio.Tests/   xUnit tests
```
