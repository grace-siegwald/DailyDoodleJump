using System.Numerics;

namespace DailyDoodle.Core;

/// <summary>
/// An enemy that patrols left and right. Landing on it from above defeats it and bounces the player;
/// touching it any other way ends the run.
/// </summary>
public class Enemy : GameObject
{
    public const float DefaultWidth = 48f;
    public const float DefaultHeight = 48f;

    /// <summary>
    /// How far inside its box an enemy actually has to be touched. The drawings (a square, a triangle,
    /// an X) don't fill their box, so touching an empty corner shouldn't end the run.
    /// </summary>
    public const float HitInset = 8f;

    private readonly float _minX;
    private readonly float _maxX;

    private float _velocityX;

    public EnemyType EnemyType { get; }

    public Enemy(
        Vector2 position,
        EnemyType enemyType,
        float minX,
        float maxX)
        : base(
            position,
            new Vector2(DefaultWidth, DefaultHeight))
    {
        EnemyType = enemyType;

        _minX = minX;
        _maxX = maxX;

        _velocityX = SpeedOf(enemyType);
    }

    /// <summary>Top of the part of the enemy that counts as touching it.</summary>
    public float HitTop => Top + HitInset;

    /// <summary>Patrol speed in units per second.</summary>
    public static float SpeedOf(EnemyType enemyType) => enemyType switch
    {
        EnemyType.Medium => 60f,
        EnemyType.Fast => 90f,
        _ => 40f,
    };

    /// <summary>True if <paramref name="other"/> touches the drawn part of the enemy (see <see cref="HitInset"/>).</summary>
    public bool Touches(GameObject other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return other.Right > Left + HitInset && other.Left < Right - HitInset &&
               other.Bottom > Top + HitInset && other.Top < Bottom - HitInset;
    }

    public override void Update(float deltaSeconds)
    {
        var x = Position.X + (_velocityX * deltaSeconds);

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
