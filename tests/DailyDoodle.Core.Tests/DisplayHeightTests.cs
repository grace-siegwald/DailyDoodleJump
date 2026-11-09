namespace DailyDoodle.Core.Tests;

public class DisplayHeightTests
{
    [Theory]
    [InlineData(0f, 0)]
    [InlineData(9.9f, 0)]
    [InlineData(10f, 1)]
    [InlineData(1234.5f, 123)]
    [InlineData(10_000f, 1_000)]
    public void From_CountsStepsOfTenUnits(float units, int expected)
    {
        Assert.Equal(expected, DisplayHeight.From(units));
    }

    [Theory]
    [InlineData(-50f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void From_WithAHeightThatMakesNoSense_IsZero(float units)
    {
        Assert.Equal(0, DisplayHeight.From(units));
    }

    [Fact]
    public void Format_UsesAThousandsSeparator()
    {
        Assert.Equal("1,000", DisplayHeight.Format(10_000f));
        Assert.Equal("42", DisplayHeight.Format(425f));
    }

    [Fact]
    public void TheDailyFinishLine_ShowsAsOneThousand()
    {
        Assert.Equal(1_000, DisplayHeight.From((float)new LevelGeneration.LevelSettings().FinishHeight));
    }
}
