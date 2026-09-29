using MouseStudio.Core.Profiles;
using Xunit;

namespace MouseStudio.Tests;

public class ProfileStoreTests : IDisposable
{
    private readonly string _dir =
        Path.Combine(Path.GetTempPath(), "MouseStudioTests", Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_dir, "profiles.json");

    public ProfileStoreTests()
    {
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void Load_MissingFile_ReturnsSingleDefaultProfile()
    {
        var store = new ProfileStore(FilePath);

        var data = store.Load(out var error);

        Assert.Null(error);
        var profile = Assert.Single(data.Profiles);
        Assert.Equal("Default", profile.Name);
        Assert.Equal(0, profile.MoveX);
        Assert.Equal(5, profile.MoveY);
        Assert.Equal(10, profile.Interval);
        Assert.Equal(5000, profile.ActiveDuration);
        Assert.Equal(1000, profile.RepeatDelay);
        Assert.Equal("Default", data.SelectedProfile);
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsProfilesAndSelection()
    {
        var store = new ProfileStore(FilePath);
        var data = new ProfileData
        {
            Profiles =
            {
                new MovementProfile { Name = "M416", MoveX = 1, MoveY = 7, Interval = 12, ActiveDuration = 4000, RepeatDelay = 500 },
                new MovementProfile { Name = "AKM", MoveX = -0.5, MoveY = 9.25, Interval = 8, ActiveDuration = 3000, RepeatDelay = 250 }
            },
            SelectedProfile = "AKM"
        };

        store.Save(data);
        var loaded = store.Load(out var error);

        Assert.Null(error);
        Assert.Equal(2, loaded.Profiles.Count);
        Assert.Equal("M416", loaded.Profiles[0].Name);
        var akm = loaded.Profiles[1];
        Assert.Equal(("AKM", -0.5, 9.25, 8, 3000, 250),
            (akm.Name, akm.MoveX, akm.MoveY, akm.Interval, akm.ActiveDuration, akm.RepeatDelay));
        Assert.Equal("AKM", loaded.SelectedProfile);
    }

    [Fact]
    public void Load_CorruptFile_ReturnsDefaultAndReportsError()
    {
        File.WriteAllText(FilePath, "{ not json");
        var store = new ProfileStore(FilePath);

        var data = store.Load(out var error);

        Assert.NotNull(error);
        Assert.Equal("Default", Assert.Single(data.Profiles).Name);
    }

    [Fact]
    public void Load_FileWithNoProfiles_ReturnsDefault()
    {
        File.WriteAllText(FilePath, """{ "Profiles": [], "SelectedProfile": null }""");
        var store = new ProfileStore(FilePath);

        var data = store.Load(out _);

        Assert.Equal("Default", Assert.Single(data.Profiles).Name);
    }

    [Fact]
    public void Load_UnknownSelection_FallsBackToFirstProfile()
    {
        var store = new ProfileStore(FilePath);
        store.Save(new ProfileData
        {
            Profiles = { new MovementProfile { Name = "A" }, new MovementProfile { Name = "B" } },
            SelectedProfile = "Gone"
        });

        var data = store.Load(out _);

        Assert.Equal("A", data.SelectedProfile);
    }

    [Fact]
    public void Export_ThenImport_RoundTripsProfilesAndSelection()
    {
        var data = new ProfileData
        {
            Profiles =
            {
                new MovementProfile { Name = "M416", MoveX = 1, MoveY = 7, Interval = 12, ActiveDuration = 4000, RepeatDelay = 500 },
                new MovementProfile { Name = "AKM", MoveX = -1, MoveY = 9, Interval = 8, ActiveDuration = 3000, RepeatDelay = 250 }
            },
            SelectedProfile = "AKM",
            OverlayLeft = 100,
            OverlayTop = 200
        };

        ProfileStore.Export(data, FilePath);
        var imported = ProfileStore.Import(FilePath, out var error);

        Assert.Null(error);
        Assert.NotNull(imported);
        Assert.Equal(new[] { "M416", "AKM" }, imported.Profiles.Select(p => p.Name));
        var akm = imported.Profiles[1];
        Assert.Equal((-1.0, 9.0, 8, 3000, 250),
            (akm.MoveX, akm.MoveY, akm.Interval, akm.ActiveDuration, akm.RepeatDelay));
        Assert.Equal("AKM", imported.SelectedProfile);
        Assert.Null(imported.OverlayLeft);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{ "Profiles": [] }""")]
    [InlineData("""{ "Profiles": [ { "Name": "" , "Interval": 1, "ActiveDuration": 1 } ] }""")]
    [InlineData("""{ "Profiles": [ { "Name": "A", "Interval": 1, "ActiveDuration": 1 }, { "Name": "a", "Interval": 1, "ActiveDuration": 1 } ] }""")]
    [InlineData("""{ "Profiles": [ { "Name": "A", "Interval": 0, "ActiveDuration": 1 } ] }""")]
    [InlineData("""{ "Profiles": [ { "Name": "A", "Interval": 1, "ActiveDuration": 1, "Feature": "Jump" } ] }""")]
    [InlineData("""{ "Profiles": [ { "Name": "A", "Interval": 1, "ActiveDuration": 1, "AutoGun": 7 } ] }""")]
    public void Import_InvalidFile_IsRejected(string json)
    {
        File.WriteAllText(FilePath, json);

        var imported = ProfileStore.Import(FilePath, out var error);

        Assert.Null(imported);
        Assert.NotNull(error);
    }

    [Fact]
    public void Import_MoreThanTwelveProfiles_IsRejected()
    {
        var data = new ProfileData();

        for (var i = 0; i <= ProfileData.MaxProfiles; i++)
        {
            data.Profiles.Add(new MovementProfile { Name = $"P{i}", Interval = 1, ActiveDuration = 1 });
        }

        ProfileStore.Export(data, FilePath);

        Assert.Null(ProfileStore.Import(FilePath, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void Import_UnknownSelection_FallsBackToFirstProfile()
    {
        File.WriteAllText(FilePath, """{ "Profiles": [ { "Name": "A", "Interval": 1, "ActiveDuration": 1 } ], "SelectedProfile": "Gone" }""");

        Assert.Equal("A", ProfileStore.Import(FilePath, out _)!.SelectedProfile);
    }

    [Fact]
    public void Load_OlderProfile_IsMovementWithAutoDefaults()
    {
        File.WriteAllText(FilePath, """{ "Profiles": [ { "Name": "Old", "Interval": 10, "ActiveDuration": 5000 } ] }""");

        var profile = Assert.Single(new ProfileStore(FilePath).Load(out var error).Profiles);

        Assert.Null(error);
        Assert.Equal(ProfileFeature.Movement, profile.Feature);
        Assert.Equal(AutoGun.M416, profile.AutoGun);
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsFeatureAndGun()
    {
        var store = new ProfileStore(FilePath);

        store.Save(new ProfileData
        {
            Profiles =
            {
                new MovementProfile
                {
                    Name = "A",
                    Feature = ProfileFeature.Auto,
                    AutoGun = AutoGun.Beryl,
                    Interval = 10,
                    ActiveDuration = 5000
                }
            }
        });

        Assert.Contains("\"Auto\"", File.ReadAllText(FilePath));

        var profile = Assert.Single(store.Load(out _).Profiles);

        Assert.Equal(ProfileFeature.Auto, profile.Feature);
        Assert.Equal(AutoGun.Beryl, profile.AutoGun);
        Assert.Equal("beryl", profile.GetLuaGlobals()["GUN_MODE"]);
        Assert.Equal("auto.lua", profile.GetScriptFileName());
    }
}
