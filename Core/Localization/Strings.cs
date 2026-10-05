namespace MouseStudio.Core.Localization;

// Every UI text, per language. Placeholders use string.Format: {0}, {1}.
// A key missing from Vietnamese shows the English text.
internal static class Strings
{
    public static string? Get(string language, string key)
    {
        var table = language == Loc.Vietnamese ? Vi : En;

        return table.TryGetValue(key, out var text) ? text : null;
    }

    private static readonly Dictionary<string, string> En = new()
    {
        // Header
        ["Donate"] = "Donate",
        ["DonateTip"] = "Support the project",
        ["LanguageButton"] = "EN",
        ["LanguageTip"] = "Language: English. Click for Tiếng Việt.",
        ["ShowOverlay"] = "Show Overlay",
        ["HideOverlay"] = "Hide Overlay",

        // Profiles
        ["Profile"] = "Profile",
        ["Import"] = "Import",
        ["ImportTip"] = "Replace all profiles with the ones in a file",
        ["Export"] = "Export",
        ["ExportTip"] = "Save all profiles to a file",
        ["ProfileListTip"] = "F1 loads the 1st profile, F2 the 2nd, … (max 12).",
        ["New"] = "New",
        ["Rename"] = "Rename",
        ["Delete"] = "Delete",

        // Movement settings
        ["MovementSettings"] = "Movement settings",
        ["MovementHint"] = "Saved automatically. Runs while the trigger is held.",
        ["HotKey"] = "Hotkey",
        ["HotKeyHint"] = "Saved automatically. Shared by all profiles.",
        ["HotKeyTip"] = "Single key: hold one hotkey. Combo: hold a key for some time, then press another.",
        ["ModeSingle"] = "Single key",
        ["ModeCombo"] = "Combo",
        ["ComboStep1"] = "1 · Hold",
        ["ComboStep1Hint"] = "Key or mouse button to hold first",
        ["ComboStep2"] = "2 · For at least",
        ["ComboStep2Hint"] = "How long step 1 must be held",
        ["ComboStep3"] = "3 · Then press",
        ["ComboStep3Hint"] = "Runs while held; letting go of 1 or 3 stops it",
        ["ComboKeyTip"] = "Click, then press a key or any mouse button. Esc cancels.",
        ["MouseLeft"] = "Mouse Left",
        ["MouseRight"] = "Mouse Right",
        ["HotkeyTip"] = "Click, then press a key or a mouse button (middle, back or forward). Esc cancels.",
        ["HotkeyPress"] = "Press a key…",
        ["SingleKey"] = "Key to hold",
        ["HotkeyHint"] = "Runs the movement while held",
        ["MouseMiddle"] = "Mouse Middle",
        ["MouseBack"] = "Mouse Back",
        ["MouseForward"] = "Mouse Forward",
        ["MoveX"] = "Horizontal movement",
        ["MoveXHint"] = "Negative = left, positive = right",
        ["MoveY"] = "Vertical movement",
        ["MoveYHint"] = "Positive values move downward",
        ["Interval"] = "Interval",
        ["IntervalHint"] = "Delay between each movement step",
        ["ActiveDuration"] = "Active duration",
        ["ActiveDurationHint"] = "Maximum time each activation can run",
        ["RepeatDelay"] = "Repeat delay",
        ["RepeatDelayHint"] = "Wait time before the next activation",

        // Console and status
        ["Console"] = "Console",
        ["Clear"] = "Clear",
        ["StatusStopped"] = "Stopped",
        ["StatusRunning"] = "Movement running · {0}",
        ["StatusNotFound"] = "Stopped · {0} not found",

        // Log
        ["LogStarted"] = "Mouse Studio started.",
        ["LogLoadError"] = "ERROR: {0} Starting with the default profile.",
        ["LogNoProfile"] = "F{0}: no profile #{0}.",
        ["LogHotkeyLoaded"] = "F{0}: profile loaded: {1}",
        ["LogSaveError"] = "ERROR: Could not save profiles: {0}",
        ["LogNotSaved"] = "Not saved: {0}",
        ["LogSaved"] = "Profile saved: {0}",
        ["LogSelected"] = "Profile selected: {0}",
        ["LogCreated"] = "Profile created: {0}",
        ["LogRenamed"] = "Profile renamed: {0} -> {1}",
        ["LogDeleted"] = "Profile deleted: {0}",
        ["LogExported"] = "Exported {0} profile(s) to {1}",
        ["LogImported"] = "Imported {0} profile(s) from {1}",
        ["LogLanguage"] = "Language: English",
        ["LogHotkeySet"] = "Hotkey: {0}",
        ["LogHotkeyCancelled"] = "Hotkey unchanged.",
        ["LogHotkeyNotAllowed"] = "{0} cannot be the hotkey (F1-F12 load profiles).",
        ["LogTriggerSingle"] = "Trigger: single key ({0})",
        ["LogComboSet"] = "Trigger: combo, hold {0} for {1} ms, then press {2}",
        ["LogComboSameKey"] = "Combo steps 1 and 3 must use different keys.",
        ["ErrHoldMs"] = "Hold time must be a number from 0 to {0} ms.",

        // Field errors
        ["ErrMoveX"] = "Horizontal movement must be a number.",
        ["ErrMoveY"] = "Vertical movement must be a number.",
        ["ErrInterval"] = "Interval must be a number.",
        ["ErrActiveDuration"] = "Active duration must be a number.",
        ["ErrRepeatDelay"] = "Repeat delay must be a number.",
        ["ErrActiveDurationMin"] = "Active duration must be >= 1 ms.",
        ["ErrRepeatDelayMin"] = "Repeat delay must be >= 0 ms.",
        ["ErrIntervalMin"] = "Interval must be >= 1 ms.",

        // Profile names
        ["ErrNameEmpty"] = "Profile name cannot be empty.",
        ["ErrNameTaken"] = "A profile named \"{0}\" already exists.",

        // Profile files
        ["ErrReadFile"] = "Could not read {0}: {1}",
        ["ErrNoProfiles"] = "The file contains no profiles.",
        ["ErrTooManyProfiles"] = "The file contains {0} profiles; the maximum is {1}.",
        ["ErrUnnamedProfile"] = "The file contains a profile without a name.",
        ["ErrDuplicateProfile"] = "The file contains two profiles named \"{0}\".",
        ["ErrInvalidValues"] = "Profile \"{0}\" has invalid values.",

        // Dialogs
        ["NewProfileTitle"] = "New Profile",
        ["RenameProfileTitle"] = "Rename Profile",
        ["ProfileName"] = "Profile name",
        ["Ok"] = "OK",
        ["Cancel"] = "Cancel",
        ["Close"] = "Close",
        ["ConfirmDelete"] = "Delete profile \"{0}\"?",
        ["ExportTitle"] = "Export Profiles",
        ["ImportTitle"] = "Import Profiles",
        ["ProfileFileFilter"] = "Mouse Studio profiles (*.json)|*.json|All files (*.*)|*.*",
        ["ExportFailed"] = "Could not export profiles: {0}",
        ["ConfirmImport"] = "Replace all {0} current profile(s) with the {1} profile(s) from this file?\n\nCurrent profiles will be lost.",
        ["ScriptNotFound"] = "Lua script not found: {0}",
        ["UnexpectedError"] = "Unexpected error:\n{0}\n\nDetails were written to:\n{1}",

        // Donate
        ["DonateTitle"] = "Donate",
        ["DonateHeading"] = "Support Mouse Studio",
        ["DonateText"] = "If the app helps you, scan this code with your banking app (VietQR / Napas 247).",

        // Overlay and tray
        ["OverlayMode"] = "Mode: {0}",
        ["OverlayProfile"] = "Profile {0}",
        ["ModeMovement"] = "Movement",
        ["OverlayTip"] = "Drag to move, double-click to open",
        ["TrayOpen"] = "Open Mouse Studio",
        ["TrayExit"] = "Exit",
    };

    private static readonly Dictionary<string, string> Vi = new()
    {
        // Header
        ["Donate"] = "Ủng hộ",
        ["DonateTip"] = "Ủng hộ dự án",
        ["LanguageButton"] = "VI",
        ["LanguageTip"] = "Ngôn ngữ: Tiếng Việt. Bấm để chuyển sang English.",
        ["ShowOverlay"] = "Hiện Overlay",
        ["HideOverlay"] = "Ẩn Overlay",

        // Profiles
        ["Profile"] = "Hồ sơ",
        ["Import"] = "Nhập",
        ["ImportTip"] = "Thay toàn bộ hồ sơ bằng các hồ sơ trong một tệp",
        ["Export"] = "Xuất",
        ["ExportTip"] = "Lưu toàn bộ hồ sơ ra một tệp",
        ["ProfileListTip"] = "F1 tải hồ sơ thứ 1, F2 hồ sơ thứ 2, … (tối đa 12).",
        ["New"] = "Mới",
        ["Rename"] = "Đổi tên",
        ["Delete"] = "Xóa",

        // Movement settings
        ["MovementSettings"] = "Cài đặt di chuyển",
        ["MovementHint"] = "Tự động lưu. Chạy khi giữ phím kích hoạt.",
        ["HotKey"] = "Hotkey",
        ["HotKeyHint"] = "Tự động lưu. Dùng chung cho mọi hồ sơ.",
        ["HotKeyTip"] = "Một phím: giữ một phím nóng. Tổ hợp: giữ một phím đủ lâu, rồi nhấn phím khác.",
        ["ModeSingle"] = "Một phím",
        ["ModeCombo"] = "Tổ hợp",
        ["ComboStep1"] = "1 · Giữ",
        ["ComboStep1Hint"] = "Phím hoặc nút chuột cần giữ trước",
        ["ComboStep2"] = "2 · Trong ít nhất",
        ["ComboStep2Hint"] = "Thời gian phải giữ bước 1",
        ["ComboStep3"] = "3 · Rồi nhấn",
        ["ComboStep3Hint"] = "Chạy khi đang giữ; thả bước 1 hoặc 3 sẽ dừng",
        ["ComboKeyTip"] = "Bấm vào, rồi nhấn một phím hoặc nút chuột bất kỳ. Esc để hủy.",
        ["MouseLeft"] = "Chuột trái",
        ["MouseRight"] = "Chuột phải",
        ["HotkeyTip"] = "Bấm vào, rồi nhấn một phím hoặc nút chuột (giữa, back hoặc forward). Esc để hủy.",
        ["HotkeyPress"] = "Nhấn một phím…",
        ["SingleKey"] = "Phím cần giữ",
        ["HotkeyHint"] = "Chạy di chuyển khi đang giữ",
        ["MouseMiddle"] = "Chuột giữa",
        ["MouseBack"] = "Chuột Back",
        ["MouseForward"] = "Chuột Forward",
        ["MoveX"] = "Di chuyển ngang",
        ["MoveXHint"] = "Âm = sang trái, dương = sang phải",
        ["MoveY"] = "Di chuyển dọc",
        ["MoveYHint"] = "Giá trị dương kéo xuống dưới",
        ["Interval"] = "Chu kỳ",
        ["IntervalHint"] = "Thời gian chờ giữa mỗi bước di chuyển",
        ["ActiveDuration"] = "Thời gian kích hoạt",
        ["ActiveDurationHint"] = "Thời gian tối đa của mỗi lần kích hoạt",
        ["RepeatDelay"] = "Độ trễ lặp lại",
        ["RepeatDelayHint"] = "Thời gian chờ trước lần kích hoạt tiếp theo",

        // Console and status
        ["Console"] = "Console",
        ["Clear"] = "Xóa",
        ["StatusStopped"] = "Đã dừng",
        ["StatusRunning"] = "Movement đang chạy · {0}",
        ["StatusNotFound"] = "Đã dừng · không tìm thấy {0}",

        // Log
        ["LogStarted"] = "Mouse Studio đã khởi động.",
        ["LogLoadError"] = "LỖI: {0} Dùng hồ sơ mặc định.",
        ["LogNoProfile"] = "F{0}: không có hồ sơ số {0}.",
        ["LogHotkeyLoaded"] = "F{0}: đã tải hồ sơ: {1}",
        ["LogSaveError"] = "LỖI: Không thể lưu hồ sơ: {0}",
        ["LogNotSaved"] = "Chưa lưu: {0}",
        ["LogSaved"] = "Đã lưu hồ sơ: {0}",
        ["LogSelected"] = "Đã chọn hồ sơ: {0}",
        ["LogCreated"] = "Đã tạo hồ sơ: {0}",
        ["LogRenamed"] = "Đã đổi tên hồ sơ: {0} -> {1}",
        ["LogDeleted"] = "Đã xóa hồ sơ: {0}",
        ["LogExported"] = "Đã xuất {0} hồ sơ ra {1}",
        ["LogImported"] = "Đã nhập {0} hồ sơ từ {1}",
        ["LogLanguage"] = "Ngôn ngữ: Tiếng Việt",
        ["LogHotkeySet"] = "Phím nóng: {0}",
        ["LogHotkeyCancelled"] = "Giữ nguyên phím nóng.",
        ["LogHotkeyNotAllowed"] = "Không thể dùng {0} làm phím nóng (F1-F12 dùng để tải hồ sơ).",
        ["LogTriggerSingle"] = "Kích hoạt: một phím ({0})",
        ["LogComboSet"] = "Kích hoạt: tổ hợp, giữ {0} trong {1} ms, rồi nhấn {2}",
        ["LogComboSameKey"] = "Bước 1 và bước 3 của tổ hợp phải dùng hai phím khác nhau.",
        ["ErrHoldMs"] = "Thời gian giữ phải là số từ 0 đến {0} ms.",

        // Field errors
        ["ErrMoveX"] = "Di chuyển ngang phải là một số.",
        ["ErrMoveY"] = "Di chuyển dọc phải là một số.",
        ["ErrInterval"] = "Chu kỳ phải là một số.",
        ["ErrActiveDuration"] = "Thời gian kích hoạt phải là một số.",
        ["ErrRepeatDelay"] = "Độ trễ lặp lại phải là một số.",
        ["ErrActiveDurationMin"] = "Thời gian kích hoạt phải >= 1 ms.",
        ["ErrRepeatDelayMin"] = "Độ trễ lặp lại phải >= 0 ms.",
        ["ErrIntervalMin"] = "Chu kỳ phải >= 1 ms.",

        // Profile names
        ["ErrNameEmpty"] = "Tên hồ sơ không được để trống.",
        ["ErrNameTaken"] = "Đã có hồ sơ tên \"{0}\".",

        // Profile files
        ["ErrReadFile"] = "Không thể đọc {0}: {1}",
        ["ErrNoProfiles"] = "Tệp không chứa hồ sơ nào.",
        ["ErrTooManyProfiles"] = "Tệp chứa {0} hồ sơ; tối đa là {1}.",
        ["ErrUnnamedProfile"] = "Tệp chứa một hồ sơ không có tên.",
        ["ErrDuplicateProfile"] = "Tệp chứa hai hồ sơ cùng tên \"{0}\".",
        ["ErrInvalidValues"] = "Hồ sơ \"{0}\" có giá trị không hợp lệ.",

        // Dialogs
        ["NewProfileTitle"] = "Hồ sơ mới",
        ["RenameProfileTitle"] = "Đổi tên hồ sơ",
        ["ProfileName"] = "Tên hồ sơ",
        ["Ok"] = "OK",
        ["Cancel"] = "Hủy",
        ["Close"] = "Đóng",
        ["ConfirmDelete"] = "Xóa hồ sơ \"{0}\"?",
        ["ExportTitle"] = "Xuất hồ sơ",
        ["ImportTitle"] = "Nhập hồ sơ",
        ["ProfileFileFilter"] = "Hồ sơ Mouse Studio (*.json)|*.json|Tất cả tệp (*.*)|*.*",
        ["ExportFailed"] = "Không thể xuất hồ sơ: {0}",
        ["ConfirmImport"] = "Thay toàn bộ {0} hồ sơ hiện tại bằng {1} hồ sơ trong tệp này?\n\nCác hồ sơ hiện tại sẽ bị mất.",
        ["ScriptNotFound"] = "Không tìm thấy script Lua: {0}",
        ["UnexpectedError"] = "Lỗi không mong muốn:\n{0}\n\nChi tiết đã được ghi vào:\n{1}",

        // Donate
        ["DonateTitle"] = "Ủng hộ",
        ["DonateHeading"] = "Ủng hộ Mouse Studio",
        ["DonateText"] = "Nếu ứng dụng hữu ích với bạn, hãy quét mã này bằng ứng dụng ngân hàng (VietQR / Napas 247).",

        // Overlay and tray
        ["OverlayMode"] = "Chế độ: {0}",
        ["OverlayProfile"] = "Hồ sơ {0}",
        ["ModeMovement"] = "Movement",
        ["OverlayTip"] = "Kéo để di chuyển, nhấp đúp để mở",
        ["TrayOpen"] = "Mở Mouse Studio",
        ["TrayExit"] = "Thoát",
    };
}
