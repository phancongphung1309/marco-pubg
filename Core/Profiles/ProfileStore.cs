using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MouseStudio.Core.Profiles;

public class ProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;

    public ProfileStore(string filePath)
    {
        _filePath = filePath;
    }

    // Never fails: a missing or unreadable file yields a single default profile.
    // `error` is set only when an existing file could not be read.
    public ProfileData Load(out string? error)
    {
        error = null;

        ProfileData? data = null;

        if (File.Exists(_filePath))
        {
            try
            {
                data = JsonSerializer.Deserialize<ProfileData>(
                    File.ReadAllText(_filePath),
                    JsonOptions
                );
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                error = $"Could not read {_filePath}: {ex.Message}";
            }
        }

        data ??= new ProfileData();

        if (data.Profiles.Count == 0)
        {
            data.Profiles.Add(MovementProfile.CreateDefault());
        }

        foreach (var profile in data.Profiles)
        {
            if (!Enum.IsDefined(profile.Feature))
            {
                profile.Feature = ProfileFeature.Movement;
            }

            if (!Enum.IsDefined(profile.AutoGun))
            {
                profile.AutoGun = AutoGun.M416;
            }
        }

        data.SelectedProfile =
            (data.Find(data.SelectedProfile) ?? data.Profiles[0]).Name;

        return data;
    }

    public void Save(ProfileData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_filePath))!);

        File.WriteAllText(
            _filePath,
            JsonSerializer.Serialize(data, JsonOptions)
        );
    }

    // Writes every profile and the selection to a file the user picked.
    // App settings such as the overlay position are left out.
    public static void Export(ProfileData data, string filePath)
    {
        new ProfileStore(filePath).Save(new ProfileData
        {
            Profiles = data.Profiles,
            SelectedProfile = data.SelectedProfile
        });
    }

    // Reads a file written by Export. Unlike Load, it never falls back to a
    // default: it returns null (and sets `error`) unless the file holds a
    // complete, valid profile list.
    public static ProfileData? Import(string filePath, out string? error)
    {
        ProfileData? data;

        try
        {
            data = JsonSerializer.Deserialize<ProfileData>(
                File.ReadAllText(filePath),
                JsonOptions
            );
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            error = $"Could not read {filePath}: {ex.Message}";
            return null;
        }

        error = Validate(data);

        if (error != null)
        {
            return null;
        }

        var imported = new ProfileData { Profiles = data!.Profiles };

        imported.SelectedProfile =
            (imported.Find(data.SelectedProfile) ?? imported.Profiles[0]).Name;

        return imported;
    }

    private static string? Validate(ProfileData? data)
    {
        if (data == null || data.Profiles == null || data.Profiles.Count == 0)
        {
            return "The file contains no profiles.";
        }

        if (data.Profiles.Count > ProfileData.MaxProfiles)
        {
            return $"The file contains {data.Profiles.Count} profiles; the maximum is {ProfileData.MaxProfiles}.";
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var profile in data.Profiles)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
            {
                return "The file contains a profile without a name.";
            }

            if (!names.Add(profile.Name.Trim()))
            {
                return $"The file contains two profiles named \"{profile.Name}\".";
            }

            if (profile.Interval < 1 || profile.ActiveDuration < 1 || profile.RepeatDelay < 0)
            {
                return $"Profile \"{profile.Name}\" has invalid values.";
            }

            if (!Enum.IsDefined(profile.Feature) || !Enum.IsDefined(profile.AutoGun))
            {
                return $"Profile \"{profile.Name}\" has invalid values.";
            }
        }

        return null;
    }
}
