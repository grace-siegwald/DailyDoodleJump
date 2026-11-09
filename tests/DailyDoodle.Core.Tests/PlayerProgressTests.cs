using DailyDoodle.Core.Progress;

namespace DailyDoodle.Core.Tests;

public class PlayerProgressTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    [Fact]
    public void ADayNeverPlayed_HasNoRecord()
    {
        var progress = new PlayerProgress();

        Assert.Equal(DayRecord.None, progress[Today]);
        Assert.Empty(progress.FinishedDays);
        Assert.Equal(0, progress.CurrentStreak(Today));
    }

    [Fact]
    public void RecordRun_KeepsTheBestHeight()
    {
        var progress = new PlayerProgress();

        Assert.True(progress.RecordRun(Today, 500f, finished: false, Today));
        Assert.False(progress.RecordRun(Today, 300f, finished: false, Today));
        Assert.True(progress.RecordRun(Today, 800f, finished: false, Today));

        Assert.Equal(800f, progress[Today].BestHeight);
    }

    [Fact]
    public void RecordRun_OnceFinished_StaysFinished()
    {
        var progress = new PlayerProgress();

        progress.RecordRun(Today, 10_000f, finished: true, Today);
        progress.RecordRun(Today, 200f, finished: false, Today);

        Assert.True(progress[Today].Finished);
        Assert.Contains(Today, progress.FinishedDays);
    }

    [Fact]
    public void FinishingAPastLevelLater_MarksTheDayButNotTheStreak()
    {
        var progress = new PlayerProgress();
        var lastWeek = Today.AddDays(-7);

        progress.RecordRun(lastWeek, 10_000f, finished: true, Today);

        Assert.True(progress[lastWeek].Finished);
        Assert.False(progress[lastWeek].FinishedOnTheDay);
        Assert.Equal(0, progress.CurrentStreak(Today));
    }

    [Fact]
    public void CurrentStreak_CountsDaysInARowEndingToday()
    {
        var progress = FinishedOnTheirOwnDays(Today, Today.AddDays(-1), Today.AddDays(-2), Today.AddDays(-4));

        Assert.Equal(3, progress.CurrentStreak(Today));
    }

    [Fact]
    public void CurrentStreak_IsStillAliveBeforeTodayIsPlayed()
    {
        var progress = FinishedOnTheirOwnDays(Today.AddDays(-1), Today.AddDays(-2));

        Assert.Equal(2, progress.CurrentStreak(Today));
    }

    [Fact]
    public void CurrentStreak_MissingYesterday_StartsAgain()
    {
        var progress = FinishedOnTheirOwnDays(Today.AddDays(-2), Today.AddDays(-3));

        Assert.Equal(0, progress.CurrentStreak(Today));
    }

    [Fact]
    public void CurrentStreak_AnUnfinishedDayBreaksIt()
    {
        var progress = FinishedOnTheirOwnDays(Today.AddDays(-2));
        progress.RecordRun(Today.AddDays(-1), 4_000f, finished: false, Today.AddDays(-1));

        Assert.Equal(0, progress.CurrentStreak(Today));
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void RecordRun_WithABadHeight_Throws(float height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerProgress().RecordRun(Today, height, false, Today));
    }

    [Fact]
    public void RecordRun_ForAFutureLevel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerProgress().RecordRun(Today.AddDays(1), 10f, false, Today));
    }

    [Fact]
    public void Json_RoundTrips()
    {
        var progress = FinishedOnTheirOwnDays(Today, Today.AddDays(-1));
        progress.RecordRun(Today.AddDays(-5), 1_234.9f, finished: false, Today);
        progress.RecordRun(Today.AddDays(-9), 10_000f, finished: true, Today);

        var loaded = PlayerProgress.FromJson(progress.ToJson());

        Assert.Equal(progress.Days.Count, loaded.Days.Count);
        Assert.Equal(1_234f, loaded[Today.AddDays(-5)].BestHeight);
        Assert.True(loaded[Today.AddDays(-9)].Finished);
        Assert.False(loaded[Today.AddDays(-9)].FinishedOnTheDay);
        Assert.Equal(2, loaded.CurrentStreak(Today));
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"days\": 5}")]
    [InlineData("[1, 2, 3]")]
    [InlineData("{\"days\": {\"2026-11-09\": {\"best\": \"high\"}}}")]
    public void FromJson_WithMissingOrDamagedData_GivesEmptyProgress(string? json)
    {
        var progress = PlayerProgress.FromJson(json);

        Assert.Empty(progress.Days);
    }

    [Fact]
    public void FromJson_SkipsOnlyTheEntriesThatDontMakeSense()
    {
        const string json = """
            {
              "version": 1,
              "days": {
                "2026-11-08": { "best": 900, "finished": true, "onTheDay": true },
                "yesterday": { "best": 900, "finished": true, "onTheDay": true },
                "2026-11-07": { "best": -20, "finished": false, "onTheDay": false },
                "2026-11-06": null,
                "2026-11-05": { "best": 300, "finished": false, "onTheDay": true }
              }
            }
            """;

        var progress = PlayerProgress.FromJson(json);

        Assert.Equal(2, progress.Days.Count);
        Assert.True(progress[new DateOnly(2026, 11, 8)].FinishedOnTheDay);

        // "On the day" without finishing can't happen, so it's not trusted.
        Assert.False(progress[new DateOnly(2026, 11, 5)].FinishedOnTheDay);
    }

    private static PlayerProgress FinishedOnTheirOwnDays(params DateOnly[] days)
    {
        var progress = new PlayerProgress();
        foreach (var day in days)
        {
            progress.RecordRun(day, 10_000f, finished: true, today: day);
        }

        return progress;
    }
}
