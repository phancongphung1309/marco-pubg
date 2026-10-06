namespace MouseStudio.Core.Profiles;

public class MovementProfile
{
    public string Name { get; set; } = "";

    // Pixels per step; fractions build up across steps (see LuaApi).
    public double MoveX { get; set; }
    public double MoveY { get; set; }
    public int Interval { get; set; }
    public int ActiveDuration { get; set; }
    public int RepeatDelay { get; set; }

    // The globals Scripts/movement.lua reads.
    public Dictionary<string, object> GetLuaGlobals() => new()
    {
        ["MOVE_X"] = MoveX,
        ["MOVE_Y"] = MoveY,
        ["INTERVAL"] = Interval,
        ["ACTIVE_DURATION"] = ActiveDuration,
        ["REPEAT_DELAY"] = RepeatDelay
    };

    public static MovementProfile CreateDefault() => new()
    {
        Name = "Default",
        MoveX = 0,
        MoveY = 5,
        Interval = 10,
        ActiveDuration = 5000,
        RepeatDelay = 1000
    };

    // The profiles a fresh install (no profiles.json) starts with.
    public static List<MovementProfile> CreateDefaults() =>
    [
        new()
        {
            Name = "F1 - 7mm",
            MoveX = 0,
            MoveY = 6,
            Interval = 5,
            ActiveDuration = 50000,
            RepeatDelay = 50
        },
        new()
        {
            Name = "F2 - 5mm",
            MoveX = 0,
            MoveY = 5,
            Interval = 10,
            ActiveDuration = 50000,
            RepeatDelay = 5
        },
        new()
        {
            Name = "F3 - RPD",
            MoveX = 0,
            MoveY = 5,
            Interval = 10,
            ActiveDuration = 50000,
            RepeatDelay = 5
        }
    ];
}
