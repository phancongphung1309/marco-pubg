namespace MouseStudio.Core.Profiles;

// What a profile runs.
public enum ProfileFeature
{
    // Scripts/movement.lua with the movement values below.
    Movement,

    // Scripts/auto.lua with the Auto values below.
    Auto
}

public enum AutoGun
{
    M416,
    Beryl
}

public class MovementProfile
{
    public string Name { get; set; } = "";

    public ProfileFeature Feature { get; set; } = ProfileFeature.Movement;

    // ===== Movement =====

    // Pixels per step; fractions build up across steps (see LuaApi).
    public double MoveX { get; set; }
    public double MoveY { get; set; }
    public int Interval { get; set; }
    public int ActiveDuration { get; set; }
    public int RepeatDelay { get; set; }

    // ===== Auto =====

    public AutoGun AutoGun { get; set; } = AutoGun.M416;

    // A method, not a property, so it is not written to profiles.json.
    public string GetScriptFileName() =>
        Feature == ProfileFeature.Auto ? "auto.lua" : "movement.lua";

    // The globals the script reads (both scripts get all of them).
    public Dictionary<string, object> GetLuaGlobals() => new()
    {
        ["MOVE_X"] = MoveX,
        ["MOVE_Y"] = MoveY,
        ["INTERVAL"] = Interval,
        ["ACTIVE_DURATION"] = ActiveDuration,
        ["REPEAT_DELAY"] = RepeatDelay,
        ["GUN_MODE"] = AutoGun.ToString().ToLowerInvariant()
    };

    public static MovementProfile CreateDefault() => new()
    {
        Name = "Default",
        Feature = ProfileFeature.Movement,
        MoveX = 0,
        MoveY = 5,
        Interval = 10,
        ActiveDuration = 5000,
        RepeatDelay = 1000
    };
}
