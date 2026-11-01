namespace DailyDoodle.Core.Games;

/// <summary>
/// The real list of games in the app. The Library page is built from this,
/// so add an entry here when a new game exists.
/// </summary>
public static class GameCatalog
{
    public const string DailyDoodleJumpId = "daily-doodle-jump";

    public static IReadOnlyList<GameInfo> All { get; } =
    [
        new(DailyDoodleJumpId,
            "Daily Doodle Jump",
            "Climb today's level. Everyone gets the same one each day.",
            Route: ""),
    ];

    public static GameInfo? Find(string id) => All.FirstOrDefault(g => g.Id == id);
}
