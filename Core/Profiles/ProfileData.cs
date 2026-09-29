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
            return "Profile name cannot be empty.";
        }

        var existing = Find(name);

        if (existing != null && existing != renaming)
        {
            return $"A profile named \"{existing.Name}\" already exists.";
        }

        return null;
    }
}
