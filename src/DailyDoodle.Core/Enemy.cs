using System.Numerics;

namespace DailyDoodle.Core;

/// <summary>
/// An enemy that patrols left and right.
/// Touching it ends the run.
/// </summary>
public class Enemy : GameObject
{
    public const float DefaultWidth = 48f;
    public const float DefaultHeight = 48f;

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

        _velocityX = enemyType switch
        {
            EnemyType.Slow => 40f,
            EnemyType.Medium => 75f,
            EnemyType.Fast => 120f,
            _ => 40f
        };
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
