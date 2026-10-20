using System.Globalization;

namespace DailyDoodle.Core.LevelGeneration;

/// <summary>
/// Turns a calendar date into a seed, so everyone playing on the same day gets the same level
/// with no server needed (works on GitHub Pages).
/// </summary>
public static class DailySeed
{
    /// <param name="salt">Change this to reshuffle every day's level (e.g. for a new season).</param>
    public static ulong ForDate(DateOnly date, string salt = "DailyDoodleJump")
    {
        // InvariantCulture matters: some cultures use non-Gregorian calendars,
        // which would give players in those locales a different seed.
        string key = $"{salt}:{date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
        return Fnv1a64(key);
    }

    /// <summary>Today's seed, using UTC so the whole world shares one level per day.</summary>
    public static ulong ForToday() => ForDate(DateOnly.FromDateTime(DateTime.UtcNow));

    // Stable hash. Do NOT use string.GetHashCode(): it is randomized per process in .NET.
    private static ulong Fnv1a64(string text)
    {
        ulong hash = 14695981039346656037UL;
        foreach (char c in text)
        {
            hash ^= c;
            hash *= 1099511628211UL;
        }
        return hash;
    }
}
