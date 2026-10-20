using DailyDoodle.Core.LevelGeneration;

namespace DailyDoodle.Core.Tests;

public class LevelGeneratorTests
{
    [Fact]
    public void SameSeed_ProducesIdenticalLevel()
    {
        var a = new LevelGenerator(seed: 42);
        var b = new LevelGenerator(seed: 42);
        a.EnsureGeneratedUpTo(10_000);
        b.EnsureGeneratedUpTo(10_000);

        Assert.Equal(a.Platforms, b.Platforms);
    }

    [Fact]
    public void IncrementalGeneration_MatchesOneShot()
    {
        var oneShot = new LevelGenerator(seed: 7);
        oneShot.EnsureGeneratedUpTo(10_000);

        var stepwise = new LevelGenerator(seed: 7);
        for (double h = 0; h <= 10_000; h += 137)
        {
            stepwise.EnsureGeneratedUpTo(h);
        }
        stepwise.EnsureGeneratedUpTo(10_000);

        Assert.Equal(oneShot.Platforms, stepwise.Platforms);
    }

    [Fact]
    public void DifferentDays_ProduceDifferentLevels()
    {
        var monday = new LevelGenerator(DailySeed.ForDate(new DateOnly(2026, 10, 19)));
        var tuesday = new LevelGenerator(DailySeed.ForDate(new DateOnly(2026, 10, 20)));
        monday.EnsureGeneratedUpTo(2_000);
        tuesday.EnsureGeneratedUpTo(2_000);

        Assert.NotEqual(monday.Platforms, tuesday.Platforms);
    }

    [Fact]
    public void DailySeed_IsStable()
    {
        // If this ever changes, every player's past daily levels change too.
        Assert.Equal(DailySeed.ForDate(new DateOnly(2026, 10, 19)),
                     DailySeed.ForDate(new DateOnly(2026, 10, 19)));
    }

    [Theory]
    [InlineData(1UL)]
    [InlineData(123UL)]
    [InlineData(99999UL)]
    [InlineData(ulong.MaxValue)]
    public void EveryGap_IsJumpable(ulong seed)
    {
        var gen = new LevelGenerator(seed);
        gen.EnsureGeneratedUpTo(50_000);

        var standable = gen.Platforms.Where(p => p.Type != PlatformType.Breakable).ToList();
        for (int i = 1; i < standable.Count; i++)
        {
            double gap = standable[i].Y - standable[i - 1].Y;
            Assert.True(gap <= gen.Settings.MaxJumpHeight,
                $"Gap of {gap:F1}px at y={standable[i].Y:F0} exceeds max jump {gen.Settings.MaxJumpHeight:F1}px");
        }
    }

    [Fact]
    public void PlatformsStayInsideWorld()
    {
        var gen = new LevelGenerator(seed: 5);
        gen.EnsureGeneratedUpTo(30_000);

        foreach (var p in gen.Platforms)
        {
            Assert.InRange(p.X - p.MoveRange, -0.001, gen.Settings.WorldWidth);
            Assert.InRange(p.X + p.Width + p.MoveRange, 0, gen.Settings.WorldWidth + 0.001);
        }
    }

    [Fact]
    public void RemoveBelow_DropsOffscreenPlatforms()
    {
        var gen = new LevelGenerator(seed: 3);
        gen.EnsureGeneratedUpTo(5_000);
        gen.RemoveBelow(4_000);

        Assert.All(gen.Platforms, p => Assert.True(p.Y >= 4_000));
    }
}
