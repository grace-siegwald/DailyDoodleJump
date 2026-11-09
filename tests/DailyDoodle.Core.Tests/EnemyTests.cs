using System.Numerics;
using DailyDoodle.Core.LevelGeneration;

namespace DailyDoodle.Core.Tests;

public class EnemyTests
{
    // An enemy that stays put (its patrol range is exactly its own width).
    private static Enemy StillEnemyAt(float x, float y) =>
        new(new Vector2(x, y), EnemyType.Slow, minX: x, maxX: x + Enemy.DefaultWidth);

    private static void Step(GameWorld world, int steps = 1)
    {
        for (var i = 0; i < steps; i++)
        {
            world.Advance(GameWorld.StepSeconds, PlayerInput.None);
        }
    }

    [Fact]
    public void FallingOntoAnEnemy_DefeatsItAndBounces()
    {
        var enemy = StillEnemyAt(176f, 300f);

        // Feet just above the enemy's drawn top, falling.
        var player = new Player(new Vector2(180f, enemy.HitTop - Player.DefaultSize.Y - 1f)) { Velocity = new Vector2(0f, 300f) };
        var world = new GameWorld(player, [enemy]);

        Step(world, 10);

        Assert.False(world.IsGameOver);
        Assert.False(enemy.IsActive);
        Assert.True(player.Velocity.Y < 0f);
    }

    [Fact]
    public void JumpingIntoAnEnemyFromBelow_EndsTheRun()
    {
        var enemy = StillEnemyAt(176f, 300f);

        // Head just below the enemy, rising fast.
        var player = new Player(new Vector2(180f, enemy.Bottom - Enemy.HitInset + 1f)) { Velocity = new Vector2(0f, -600f) };
        var world = new GameWorld(player, [enemy]);

        Step(world, 10);

        Assert.True(world.IsGameOver);
    }

    [Fact]
    public void TouchingOnlyTheEmptyEdgeOfTheBox_IsSafe()
    {
        var enemy = StillEnemyAt(200f, 300f);

        // Overlaps the enemy's box by less than HitInset on the left side.
        var player = new Player(new Vector2(enemy.Left - Player.DefaultSize.X + (Enemy.HitInset / 2f), 304f));

        Assert.True(player.Overlaps(enemy));
        Assert.False(enemy.Touches(player));
    }

    [Theory]
    [InlineData(EnemyType.Slow)]
    [InlineData(EnemyType.Medium)]
    [InlineData(EnemyType.Fast)]
    public void Enemies_StayInsideTheirPatrol(EnemyType type)
    {
        var enemy = new Enemy(new Vector2(100f, 0f), type, minX: 60f, maxX: 220f);

        for (var i = 0; i < 2_000; i++)
        {
            enemy.Update(GameWorld.StepSeconds);
            Assert.InRange(enemy.Left, 60f, 220f - Enemy.DefaultWidth);
        }
    }

    [Fact]
    public void FasterTypes_MoveFaster()
    {
        Assert.True(Enemy.SpeedOf(EnemyType.Slow) < Enemy.SpeedOf(EnemyType.Medium));
        Assert.True(Enemy.SpeedOf(EnemyType.Medium) < Enemy.SpeedOf(EnemyType.Fast));
    }

    [Fact]
    public void EnemyTypeAt_GetsHarderHigherUp()
    {
        Assert.Equal(EnemyType.Slow, PlatformSpawner.EnemyTypeAt(PlatformSpawner.EnemyStartHeight));
        Assert.Equal(EnemyType.Medium, PlatformSpawner.EnemyTypeAt(6_000));
        Assert.Equal(EnemyType.Fast, PlatformSpawner.EnemyTypeAt(9_000));
    }

    [Theory]
    [InlineData(2026, 11, 9)]
    [InlineData(2026, 11, 10)]
    [InlineData(2026, 12, 25)]
    public void DailyClimb_HasNoEnemiesInTheWarmUp_AndOnlyAFewOverall(int year, int month, int day)
    {
        var enemies = EnemiesUpTo(new DateOnly(year, month, day), 10_000);

        Assert.All(enemies, e => Assert.True(Height(e) >= PlatformSpawner.EnemyStartHeight - 60));
        Assert.InRange(enemies.Count, 0, 15);
    }

    [Fact]
    public void Enemies_AreTheSameEachTimeADayIsPlayed_ButDifferBetweenDays()
    {
        var first = EnemiesUpTo(new DateOnly(2026, 11, 9), 20_000).Select(e => e.Position).ToList();
        var again = EnemiesUpTo(new DateOnly(2026, 11, 9), 20_000).Select(e => e.Position).ToList();
        var nextDay = EnemiesUpTo(new DateOnly(2026, 11, 10), 20_000).Select(e => e.Position).ToList();

        Assert.NotEmpty(first);
        Assert.Equal(first, again);
        Assert.NotEqual(first, nextDay);
    }

    private static float Height(GameObject o) => PlatformSpawner.StartPlatformTop - o.Bottom;

    // Every enemy spawned while the camera climbs to a height, without simulating any jumping.
    private static List<Enemy> EnemiesUpTo(DateOnly date, float height)
    {
        var spawner = new PlatformSpawner(new LevelGenerator(DailySeed.ForDate(date)));
        var enemies = new List<Enemy>();

        for (var h = 0f; h <= height; h += GameWorld.Height / 2f)
        {
            // A fresh world each screen, with the player placed there, so the camera starts at that height.
            var world = new GameWorld(new Player(new Vector2(180f, spawner.PlayerStart.Y - h)), []);
            world.Advance(GameWorld.StepSeconds, PlayerInput.None);
            spawner.SpawnAhead(world);
            enemies.AddRange(world.Objects.OfType<Enemy>());
        }

        return enemies.Where(e => Height(e) <= height).ToList();
    }
}
