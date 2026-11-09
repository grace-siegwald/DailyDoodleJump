using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DailyDoodle.Core.Progress;

/// <summary>
/// What one player has done on each day's level: their best height and whether they reached the
/// finish line. Plain data with no browser types, so it can be unit tested; the web app saves it
/// with <see cref="ToJson"/> and loads it with <see cref="FromJson"/>.
/// </summary>
public sealed class PlayerProgress
{
    private const int FileVersion = 1;

    private readonly Dictionary<DateOnly, DayRecord> _days = [];

    /// <summary>Every day that has been played, by level date.</summary>
    public IReadOnlyDictionary<DateOnly, DayRecord> Days => _days;

    /// <summary>Days whose finish line has been reached, whenever that happened.</summary>
    public IReadOnlySet<DateOnly> FinishedDays => _days.Where(d => d.Value.Finished).Select(d => d.Key).ToHashSet();

    /// <summary>What has been done on one day's level. A day never played gives <see cref="DayRecord.None"/>.</summary>
    public DayRecord this[DateOnly levelDate] => _days.GetValueOrDefault(levelDate, DayRecord.None);

    /// <summary>Saves the result of one run.</summary>
    /// <param name="levelDate">Which day's level was played.</param>
    /// <param name="height">How high the run got, in game units. Must be zero or more.</param>
    /// <param name="finished">True if the run reached the finish line.</param>
    /// <param name="today">Today's date. A level finished on its own day counts towards the streak.</param>
    /// <returns>True if this run set a new best height for that day.</returns>
    public bool RecordRun(DateOnly levelDate, float height, bool finished, DateOnly today)
    {
        if (!float.IsFinite(height) || height < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be zero or more.");
        }

        if (levelDate > today)
        {
            throw new ArgumentOutOfRangeException(nameof(levelDate), levelDate, "That day's level isn't out yet.");
        }

        var before = this[levelDate];
        _days[levelDate] = new DayRecord(
            BestHeight: Math.Max(before.BestHeight, height),
            Finished: before.Finished || finished,
            FinishedOnTheDay: before.FinishedOnTheDay || (finished && levelDate == today));

        return height > before.BestHeight;
    }

    /// <summary>
    /// How many days in a row the player has finished the level on the day it came out, counting back
    /// from today. If today isn't finished yet, the streak from yesterday is still alive and counts.
    /// Finishing a past level later fills in the calendar but doesn't rebuild a streak.
    /// </summary>
    public int CurrentStreak(DateOnly today)
    {
        var day = this[today].FinishedOnTheDay ? today : today.AddDays(-1);
        var streak = 0;

        while (this[day].FinishedOnTheDay)
        {
            streak++;
            day = day.AddDays(-1);
        }

        return streak;
    }

    /// <summary>Writes the progress as JSON, for saving.</summary>
    public string ToJson()
    {
        var file = new ProgressFile
        {
            Version = FileVersion,
            Days = _days
                .OrderBy(d => d.Key)
                .ToDictionary(
                    d => d.Key.ToString(LevelGeneration.LevelDate.Format, CultureInfo.InvariantCulture),
                    d => (DayEntry?)new DayEntry { Best = MathF.Floor(d.Value.BestHeight), Finished = d.Value.Finished, OnTheDay = d.Value.FinishedOnTheDay }),
        };

        return JsonSerializer.Serialize(file, ProgressJsonContext.Default.ProgressFile);
    }

    /// <summary>
    /// Reads progress saved by <see cref="ToJson"/>. Missing or unreadable data gives empty progress,
    /// and any single entry that doesn't make sense (a bad date, a negative height) is skipped,
    /// so a damaged save never stops the game from loading.
    /// </summary>
    public static PlayerProgress FromJson(string? json)
    {
        var progress = new PlayerProgress();
        if (string.IsNullOrWhiteSpace(json))
        {
            return progress;
        }

        ProgressFile? file;
        try
        {
            file = JsonSerializer.Deserialize(json, ProgressJsonContext.Default.ProgressFile);
        }
        catch (JsonException)
        {
            return progress;
        }

        if (file?.Days is null)
        {
            return progress;
        }

        foreach (var (key, entry) in file.Days)
        {
            if (entry is null ||
                !DateOnly.TryParseExact(key, LevelGeneration.LevelDate.Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                !float.IsFinite(entry.Best) || entry.Best < 0f)
            {
                continue;
            }

            // "On the day" only makes sense for a finished level.
            progress._days[date] = new DayRecord(entry.Best, entry.Finished, entry.Finished && entry.OnTheDay);
        }

        return progress;
    }

    // The saved shape. Short property names keep the save small.
    internal sealed class ProgressFile
    {
        public int Version { get; set; }

        public Dictionary<string, DayEntry?>? Days { get; set; }
    }

    internal sealed class DayEntry
    {
        public float Best { get; set; }

        public bool Finished { get; set; }

        public bool OnTheDay { get; set; }
    }
}

/// <summary>What has been done on one day's level.</summary>
/// <param name="BestHeight">Highest any run on that level got, in game units.</param>
/// <param name="Finished">True if any run reached the finish line.</param>
/// <param name="FinishedOnTheDay">True if the finish line was reached on the level's own day (counts towards the streak).</param>
public readonly record struct DayRecord(float BestHeight, bool Finished, bool FinishedOnTheDay)
{
    /// <summary>A day that hasn't been played.</summary>
    public static DayRecord None => default;
}

// Generates the JSON code at build time. Needed because the published web app is trimmed,
// and trimming removes the reflection that JsonSerializer would otherwise use.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PlayerProgress.ProgressFile))]
internal sealed partial class ProgressJsonContext : JsonSerializerContext;
