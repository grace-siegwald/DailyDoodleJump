using System.Numerics;
using DailyDoodle.Core.LevelGeneration;

namespace DailyDoodle.Core.Tests;

public class PlatformSpawnerTests
{
    private const ulong Seed = 42;

    [Fact]
    public void SpawnAhead_PutsTheStartingPlatformUnderThePlayer()
    {
        var (world, _) = StartRun();

        var under = world.Objects.Single(o => o.Top == PlatformSpawner.StartPlatformTop && o.OverlapsHorizontally(world.Player));

        Assert.Equal(under.Top, world.Player.Bottom);
        Assert.IsType<Platform>(under);
    }

    [Fact]
    public void SpawnAhead_MatchesTheGeneratorsLevel()
    {
        var (world, _) = StartRun();

        // A second generator with the same seed makes the same level, so it can be the answer key.
        var expected = new LevelGenerator(Seed);
        expected.EnsureGeneratedUpTo(10_000);

        Assert.True(world.Objects.Count > 5);
        foreach (var spawned in world.Objects)
        {
            var match = expected.Platforms.Single(p =>
                (float)p.X == spawned.Left && PlatformSpawner.StartPlatformTop - (float)p.Y == spawned.Top);

            Assert.Equal((float)match.Width, spawned.Size.X);
            Assert.Equal(ExpectedClass(match.Type), spawned.GetType());
        }
    }

    [Fact]
    public void SpawnAhead_CalledAgainWithoutMoving_AddsNothing()
    {
        var (world, spawner) = StartRun();
        var count = world.Objects.Count;

        spawner.SpawnAhead(world);

        Assert.Equal(count, world.Objects.Count);
    }

    [Fact]
    public void SpawnAhead_AfterClimbing_FillsTheNewScreen()
    {
        var spawner = new PlatformSpawner(new LevelGenerator(Seed));

        // Drop the player in far above the start, so the camera jumps up to meet them.
        var world = new GameWorld(new Player(new Vector2(180f, -3000f)), []);
        spawner.SpawnAhead(world);
        world.Advance(GameWorld.StepSeconds, PlayerInput.None);
        spawner.SpawnAhead(world);

        var screenTop = world.CameraTop;
        var screenBottom = world.CameraTop + GameWorld.Height;
        Assert.Contains(world.Objects, o => o.Top >= screenTop && o.Bottom <= screenBottom);
    }

    [Fact]
    public void Player_BouncesOffTheStartingPlatform()
    {
        var (world, spawner) = StartRun();

        for (var i = 0; i < 10; i++)
        {
            world.Advance(GameWorld.StepSeconds, PlayerInput.None);
            spawner.SpawnAhead(world);
        }

        Assert.True(world.Player.Velocity.Y < 0f);
        Assert.False(world.IsGameOver);
    }

    [Fact]
    public void Constructor_WithAGeneratorThatLostItsStartingPlatform_Throws()
    {
        var used = new LevelGenerator(Seed);
        used.EnsureGeneratedUpTo(5_000);
        used.RemoveBelow(1_000);

        Assert.Throws<ArgumentException>(() => new PlatformSpawner(used));
    }

    [Fact]
    public void LevelSettings_UsesThePlayersPhysics()
    {
        // If these drift apart, the generator could make gaps the player can't jump.
        var settings = new LevelSettings();

        Assert.Equal(Player.Gravity, settings.Gravity);
        Assert.Equal(Player.JumpSpeed, settings.JumpVelocity);
        Assert.Equal(Player.MoveSpeed, settings.HorizontalSpeed);
        Assert.Equal(GameWorld.Width, settings.WorldWidth);
    }

    private static (GameWorld World, PlatformSpawner Spawner) StartRun()
    {
        var spawner = new PlatformSpawner(new LevelGenerator(Seed));
        var world = new GameWorld(new Player(spawner.PlayerStart), []);
        spawner.SpawnAhead(world);
        return (world, spawner);
    }

    private static Type ExpectedClass(PlatformType type) => type switch
    {
        PlatformType.Spring => typeof(SpringPlatform),
        PlatformType.Breakable => typeof(BreakingPlatform),
        PlatformType.Moving => typeof(MovingPlatform),
        _ => typeof(Platform),
    };
}
