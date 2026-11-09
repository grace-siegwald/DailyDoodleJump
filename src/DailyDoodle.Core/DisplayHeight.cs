using System.Globalization;

namespace DailyDoodle.Core;

/// <summary>
/// Turns heights in game units into the number players see. The player is 40 units tall,
/// so raw units feel inflated; one displayed step is 10 units, which makes the finish line 1,000.
/// </summary>
public static class DisplayHeight
{
    /// <summary>Game units per displayed step.</summary>
    public const float UnitsPerStep = 10f;

    /// <summary>A height as players see it. Below the start, or not a number, counts as 0.</summary>
    public static int From(float units) =>
        float.IsFinite(units) && units > 0f ? (int)(units / UnitsPerStep) : 0;

    /// <summary>A height as players see it, with a thousands separator (e.g. "1,000").</summary>
    public static string Format(float units) => From(units).ToString("N0", CultureInfo.InvariantCulture);
}
