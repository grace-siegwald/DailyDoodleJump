using System.Numerics;

namespace DailyDoodle.Core.Tests;

public class CurrentHeightTests
{
    private static GameWorld StandingOnAPlatform()
    {
        var platform = new Platform(new Vector2(0f, 500f), width: GameWorld.Width);
        return new GameWorld(new Player(new Vector2(180f, platform.Top - Player.DefaultSize.Y)), [platform]);
    }

    private static void Run(GameWorld world, float seconds)
    {
        for (var t = 0f; t < seconds; t += GameWorld.StepSeconds)
        {
            world.Advance(GameWorld.StepSeconds, PlayerInput.None);
        }
    }

    [Fact]
    public void AtTheStart_IsZero()
    {
        Assert.Equal(0f, StandingOnAPlatform().CurrentHeight);
    }

    [Fact]
    public void RisesWithAJump_ThenComesBackDown()
    {
        var world = StandingOnAPlatform();

        // A normal jump peaks after half a second (900 / 1800), about 225 units up.
        Run(world, 0.5f);
        Assert.InRange(world.CurrentHeight, 200f, 230f);

        Run(world, 0.45f);
        Assert.InRange(world.CurrentHeight, 0f, 50f);
        Assert.True(world.HeightClimbed > world.CurrentHeight);
    }

    [Fact]
    public void FallingPastTheStart_GoesBelowZero()
    {
        var world = new GameWorld(new Player(new Vector2(180f, 100f)), []);

        Run(world, 0.5f);

        Assert.True(world.CurrentHeight < 0f);
        Assert.Equal(0, DisplayHeight.From(world.CurrentHeight));
    }
}
