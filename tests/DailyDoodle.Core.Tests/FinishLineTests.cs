using System.Numerics;

namespace DailyDoodle.Core.Tests;

public class FinishLineTests
{
    // A wide platform with the player standing on it, like the start of a run.
    private static GameWorld WorldWithFinishAt(float finishHeight)
    {
        var platform = new Platform(new Vector2(0f, 500f), width: GameWorld.Width);
        var player = new Player(new Vector2(180f, platform.Top - Player.DefaultSize.Y));
        return new GameWorld(player, [platform], finishHeight);
    }

    private static void Run(GameWorld world, float seconds)
    {
        for (var t = 0f; t < seconds; t += GameWorld.StepSeconds)
        {
            world.Advance(GameWorld.StepSeconds, PlayerInput.None);
        }
    }

    [Fact]
    public void ReachingTheFinishLine_WinsTheRun()
    {
        // A normal jump goes about 225 units up, so a line 100 units up is reached on the first bounce.
        var world = WorldWithFinishAt(100f);

        Run(world, 1f);

        Assert.True(world.IsWon);
        Assert.False(world.IsGameOver);
        Assert.True(world.HasEnded);
        Assert.Equal(100f, world.HeightClimbed);
    }

    [Fact]
    public void AfterWinning_NothingMoves()
    {
        var world = WorldWithFinishAt(100f);
        Run(world, 1f);
        var where = world.Player.Position;

        Run(world, 1f);

        Assert.Equal(where, world.Player.Position);
    }

    [Fact]
    public void FinishLineOutOfReach_IsNotWon()
    {
        var world = WorldWithFinishAt(5_000f);

        Run(world, 3f);

        Assert.False(world.IsWon);
        Assert.False(world.HasEnded);
        Assert.InRange(world.HeightClimbed, 1f, 300f);
    }

    [Fact]
    public void WithoutAFinishLine_TheLevelNeverEnds()
    {
        var platform = new Platform(new Vector2(0f, 500f), width: GameWorld.Width);
        var world = new GameWorld(new Player(new Vector2(180f, platform.Top - Player.DefaultSize.Y)), [platform]);

        Run(world, 3f);

        Assert.False(world.IsWon);
        Assert.Equal(float.PositiveInfinity, world.FinishHeight);
    }

    [Fact]
    public void FallingOffTheScreen_IsGameOverNotAWin()
    {
        // No platforms at all, so the player just falls.
        var world = new GameWorld(new Player(new Vector2(180f, 300f)), [], finishHeight: 100f);

        Run(world, 3f);

        Assert.True(world.IsGameOver);
        Assert.False(world.IsWon);
    }

    [Fact]
    public void FinishLineY_IsFinishHeightAboveTheStart()
    {
        var world = WorldWithFinishAt(1_000f);

        Assert.Equal(world.Player.Bottom - 1_000f, world.FinishLineY);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-50f)]
    [InlineData(float.NaN)]
    public void FinishLineAtOrBelowTheStart_Throws(float finishHeight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WorldWithFinishAt(finishHeight));
    }
}
