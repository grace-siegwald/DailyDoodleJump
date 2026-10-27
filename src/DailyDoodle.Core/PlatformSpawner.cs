using System.Numerics;
using DailyDoodle.Core.LevelGeneration;

// The level generator has its own Platform type (a plain description: where, how wide, what kind).
// This alias tells it apart from the game's Platform class, which is the thing the player lands on.
using GeneratedPlatform = DailyDoodle.Core.LevelGeneration.Platform;

namespace DailyDoodle.Core;

/// <summary>
/// Feeds platforms from the <see cref="LevelGenerator"/> into a <see cref="GameWorld"/> as the player climbs.
/// </summary>
/// <remarks>
/// The generator and the world measure positions differently:
/// <list type="bullet">
/// <item>Generator: Y grows upward, 0 is the top of the starting platform.</item>
/// <item>World: Y grows downward, like the screen. 0 is the top of the first screen.</item>
/// </list>
/// This class is the only place that converts between the two.
/// </remarks>
public sealed class PlatformSpawner
{
    /// <summary>World Y of the top of the starting platform: 80 units above the bottom of the first screen.</summary>
    public const float StartPlatformTop = GameWorld.Height - 80f;

    private readonly LevelGenerator _generator;

    // The generator numbers platforms 0, 1, 2... in the order it makes them.
    // Everything numbered below this has already been added to the world.
    private int _nextIdToSpawn;

    /// <param name="generator">A new generator. One that has already dropped its starting platform is refused.</param>
    public PlatformSpawner(LevelGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        var start = generator.Platforms.FirstOrDefault(p => p.Id == 0)
            ?? throw new ArgumentException("Pass a new LevelGenerator that still has its starting platform.", nameof(generator));

        _generator = generator;

        // Stand the player in the middle of the starting platform, feet on its top.
        var startCenterX = (float)(start.X + (start.Width / 2));
        PlayerStart = new Vector2(
            startCenterX - (Player.DefaultSize.X / 2f),
            StartPlatformTop - Player.DefaultSize.Y);
    }

    /// <summary>Where to put the player at the start of a run.</summary>
    public Vector2 PlayerStart { get; }

    /// <summary>
    /// Adds every platform up to one screen above what's on screen, so platforms never pop into view,
    /// and lets the generator forget platforms that have scrolled off the bottom.
    /// Call once when the run starts, then after every <see cref="GameWorld.Advance"/>.
    /// </summary>
    public void SpawnAhead(GameWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var screenTopHeight = ToGeneratorHeight(world.CameraTop);
        var screenBottomHeight = ToGeneratorHeight(world.CameraTop + GameWorld.Height);

        _generator.EnsureGeneratedUpTo(screenTopHeight + GameWorld.Height);

        var nextId = _nextIdToSpawn;
        foreach (var generated in _generator.Platforms)
        {
            if (generated.Id >= _nextIdToSpawn)
            {
                world.Add(ToGameObject(generated));
                nextId = Math.Max(nextId, generated.Id + 1);
            }
        }

        _nextIdToSpawn = nextId;

        // The world removes its own copies once they're off screen; this frees the generator's.
        _generator.RemoveBelow(screenBottomHeight);
    }

    // Generator height (up from the starting platform) <-> world Y (down from the top of the first screen).
    private static double ToGeneratorHeight(float worldY) => StartPlatformTop - worldY;

    private static float ToWorldY(double height) => StartPlatformTop - (float)height;

    private static Platform ToGameObject(GeneratedPlatform generated)
    {
        var position = new Vector2((float)generated.X, ToWorldY(generated.Y));
        var width = (float)generated.Width;

        return generated.Type switch
        {
            PlatformType.Spring => new SpringPlatform(position, width),
            PlatformType.Breakable => new BreakingPlatform(position, width),
            PlatformType.Moving => new MovingPlatform(
                position,
                minX: (float)(generated.X - generated.MoveRange),
                maxX: (float)(generated.X + generated.Width + generated.MoveRange),
                speed: (float)generated.MoveSpeed,
                width: width),
            _ => new Platform(position, width),
        };
    }
}
