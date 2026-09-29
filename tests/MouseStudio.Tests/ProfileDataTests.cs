using MouseStudio.Core.Profiles;
using Xunit;

namespace MouseStudio.Tests;

public class ProfileDataTests
{
    private static ProfileData TwoProfiles() => new()
    {
        Profiles = { new MovementProfile { Name = "M416" }, new MovementProfile { Name = "AKM" } }
    };

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateName_Blank_IsRejected(string name)
    {
        Assert.NotNull(TwoProfiles().ValidateName(name));
    }

    [Theory]
    [InlineData("AKM")]
    [InlineData("akm")]
    [InlineData("  AKM ")]
    public void ValidateName_Duplicate_IsRejectedIgnoringCaseAndSpaces(string name)
    {
        Assert.NotNull(TwoProfiles().ValidateName(name));
    }

    [Fact]
    public void ValidateName_NewName_IsAccepted()
    {
        Assert.Null(TwoProfiles().ValidateName("Beryl"));
    }

    [Fact]
    public void ValidateName_RenamingToOwnName_IsAccepted()
    {
        var data = TwoProfiles();

        Assert.Null(data.ValidateName("akm", renaming: data.Profiles[1]));
    }

    [Fact]
    public void CanAddProfile_IsFalseOnceTwelveProfilesExist()
    {
        var data = new ProfileData();

        for (var i = 0; i < ProfileData.MaxProfiles - 1; i++)
        {
            data.Profiles.Add(new MovementProfile { Name = $"P{i}" });
        }

        Assert.True(data.CanAddProfile);

        data.Profiles.Add(new MovementProfile { Name = "Last" });

        Assert.False(data.CanAddProfile);
    }
}
