using System.Numerics;

namespace DailyDoodle.Core;

/// <summary>
/// A normal platform. Bounces the player at the usual jump speed.
/// </summary>
/// <remarks>
/// Platforms are one-way: the player jumps up through them and only lands when falling onto the top.
/// <see cref="GameWorld"/> handles that check; subclasses only decide what a landing does.
/// </remarks>
public class Platform : GameObject
{
    public const float DefaultWidth = 60f;

    /// <summary>Height of every platform's landing box. The art can be drawn taller than this.</summary>
    public const float Thickness = 12f;

    public static readonly Vector2 DefaultSize = new(DefaultWidth, Thickness);

    /// <param name="position">Top-left corner.</param>
    /// <param name="width">How wide it is. The level generator makes platforms narrower as the climb gets harder.</param>
    public Platform(Vector2 position, float width = DefaultWidth)
        : base(position, new Vector2(width, Thickness))
    {
    }

    /// <summary>Called by <see cref="GameWorld"/> when the player falls onto the top of this platform.</summary>
    public virtual void OnLanded(Player player)
    {
        player.Bounce();
    }
}

/// <summary>A platform with a spring: launches the player much higher than normal.</summary>
public class SpringPlatform(Vector2 position, float width = Platform.DefaultWidth) : Platform(position, width)
{
    /// <summary>Reaches about 625 units high, nearly a full screen.</summary>
    public const float SpringJumpSpeed = 1500f;

    public override void OnLanded(Player player)
    {
        player.Bounce(SpringJumpSpeed);
    }
}

/// <summary>A platform that breaks when landed on. The player doesn't bounce and keeps falling.</summary>
public class BreakingPlatform(Vector2 position, float width = Platform.DefaultWidth) : Platform(position, width)
{
    public override void OnLanded(Player player)
    {
        IsActive = false;
    }
}

/// <summary>A platform that slides back and forth between two X positions.</summary>
public class MovingPlatform : Platform
{
    public const float DefaultSpeed = 80f;

    private readonly float _minX;
    private readonly float _maxX;
    private float _velocityX;

    /// <param name="position">Starting top-left corner.</param>
    /// <param name="minX">Leftmost X the platform's left edge can reach.</param>
    /// <param name="maxX">Rightmost X the platform's right edge can reach.</param>
    /// <param name="speed">Units per second. Must be greater than zero.</param>
    /// <param name="width">How wide it is.</param>
    public MovingPlatform(Vector2 position, float minX, float maxX, float speed = DefaultSpeed, float width = DefaultWidth)
        : base(position, width)
    {
        if (!(maxX - minX >= Size.X))
        {
            throw new ArgumentException("The range must be at least as wide as the platform.", nameof(maxX));
        }

        if (!float.IsFinite(speed) || speed <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), speed, "Speed must be greater than zero.");
        }

        _minX = minX;
        _maxX = maxX;
        _velocityX = speed;
    }

    public override void Update(float deltaSeconds)
    {
        var x = Position.X + (_velocityX * deltaSeconds);

        // Turn around at either end of the range.
        if (x < _minX)
        {
            x = _minX;
            _velocityX = Math.Abs(_velocityX);
        }
        else if (x + Size.X > _maxX)
        {
            x = _maxX - Size.X;
            _velocityX = -Math.Abs(_velocityX);
        }

        Position = new Vector2(x, Position.Y);
    }
}
