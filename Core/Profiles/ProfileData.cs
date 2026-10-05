using MouseStudio.Core.Localization;

namespace MouseStudio.Core.Profiles;

// Everything stored in profiles.json.
public class ProfileData
{
    // One profile per hotkey, F1 to F12.
    public const int MaxProfiles = 12;

    public List<MovementProfile> Profiles { get; set; } = new();

    public string? SelectedProfile { get; set; }

    // Where the overlay was last dragged to; null until it has been moved.
    public double? OverlayLeft { get; set; }

    public double? OverlayTop { get; set; }

    // UI language code ("en" or "vi"); null means English.
    public string? Language { get; set; }

    public bool CanAddProfile => Profiles.Count < MaxProfiles;

    public MovementProfile? Find(string? name) =>
        Profiles.FirstOrDefault(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    // Returns an error message, or null when the name can be used.
    // Pass the profile being renamed so it may keep (or re-case) its own name.
    public string? ValidateName(string name, MovementProfile? renaming = null)
    {
        name = name.Trim();

        if (name.Length == 0)
        {
            return Loc.T("ErrNameEmpty");
        }

        var existing = Find(name);

        if (existing != null && existing != renaming)
        {
            return Loc.T("ErrNameTaken", existing.Name);
        }

        return null;
    }
}
