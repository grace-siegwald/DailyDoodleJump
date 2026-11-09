using DailyDoodle.Core.LevelGeneration;

namespace DailyDoodle.Core.Tests;

public class LevelDateTests
{
    private static readonly DateOnly _today = new(2026, 11, 9);

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Choose_WithNoDate_PlaysToday(string? text)
    {
        var choice = LevelDate.Choose(text, _today);

        Assert.Equal(_today, choice.Date);
        Assert.Null(choice.Problem);
    }

    [Fact]
    public void Choose_WithAPastDay_PlaysThatDay()
    {
        var choice = LevelDate.Choose("2026-11-03", _today);

        Assert.Equal(new DateOnly(2026, 11, 3), choice.Date);
        Assert.Null(choice.Problem);
    }

    [Fact]
    public void Choose_WithToday_PlaysToday()
    {
        var choice = LevelDate.Choose("2026-11-09", _today);

        Assert.Equal(_today, choice.Date);
        Assert.Null(choice.Problem);
    }

    [Theory]
    [InlineData("2026-11-10")]
    [InlineData("2099-01-01")]
    public void Choose_WithAFutureDay_FallsBackToToday(string text)
    {
        var choice = LevelDate.Choose(text, _today);

        Assert.Equal(_today, choice.Date);
        Assert.NotNull(choice.Problem);
    }

    [Theory]
    [InlineData("yesterday")]
    [InlineData("11/03/2026")]
    [InlineData("2026-13-01")]
    [InlineData("2026-02-30")]
    [InlineData("2026-11-03T00:00")]
    public void Choose_WithAnUnreadableDate_FallsBackToToday(string text)
    {
        var choice = LevelDate.Choose(text, _today);

        Assert.Equal(_today, choice.Date);
        Assert.NotNull(choice.Problem);
    }

    [Fact]
    public void ToText_RoundTripsThroughChoose()
    {
        var day = new DateOnly(2026, 1, 5);

        Assert.Equal("2026-01-05", LevelDate.ToText(day));
        Assert.Equal(day, LevelDate.Choose(LevelDate.ToText(day), _today).Date);
    }
}
