using System.Globalization;

namespace DailyDoodle.Core.LevelGeneration;

/// <summary>
/// Which day's level to play. Every day has its own level (see <see cref="DailySeed"/>), and
/// "today" is the UTC date, so the whole world shares one level per day.
/// </summary>
public static class LevelDate
{
    /// <summary>How a date is written in links, e.g. <c>?date=2026-11-09</c>.</summary>
    public const string Format = "yyyy-MM-dd";

    /// <summary>Today's date in UTC, matching <see cref="DailySeed.ForToday"/>.</summary>
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Writes a date the way links expect it. Always uses the invariant culture.</summary>
    public static string ToText(DateOnly date) => date.ToString(Format, CultureInfo.InvariantCulture);

    /// <summary>
    /// Works out which level a link asks for. Anything that can't be played falls back to today's
    /// level, with a short message the page can show to say why.
    /// </summary>
    /// <param name="text">The date from the link, or null when the link has none.</param>
    /// <param name="today">Today's date (pass <see cref="Today"/>; tests pass a fixed day).</param>
    public static LevelDateChoice Choose(string? text, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new LevelDateChoice(today, null);
        }

        if (!DateOnly.TryParseExact(text.Trim(), Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return new LevelDateChoice(today, "That link's date couldn't be read, so here's today's level.");
        }

        if (date > today)
        {
            return new LevelDateChoice(today, "That level isn't out yet, so here's today's level.");
        }

        return new LevelDateChoice(date, null);
    }
}

/// <summary>The level to play, and why it isn't the one that was asked for (null when it is).</summary>
public readonly record struct LevelDateChoice(DateOnly Date, string? Problem);
