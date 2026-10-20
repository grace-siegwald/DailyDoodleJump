namespace DailyDoodle.Core.Games;

/// <summary>One playable game in the library.</summary>
/// <param name="Route">Relative app route that opens the game ("" = home page).</param>
public sealed record GameInfo(string Id, string Title, string Description, string Route);
