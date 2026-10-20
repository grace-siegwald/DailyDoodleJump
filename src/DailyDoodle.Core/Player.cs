using System.Numerics;

namespace DailyDoodle.Core;

/// 
/// The doodle. Falls under gravity, steers left and right, and bounces when told to.
/// 
/// 
/// The player never decides on its own when to bounce. GameWorld notices the landing
/// and the platform calls "Bounce", so each platform type can react differently.
/// 
public class Player : GameObject
{
    public const float Gravity = 1800f;

    /// Upward speed after a normal bounce. Reaches about 225 units high
    public const float JumpSpeed = 900f;

    public const float MoveSpeed = 350f;

    public static readonly Vector2 DefaultSize = new(40f, 40f);

    private float _moveDirection;

    public Player(Vector2 position) : base(position, DefaultSize)
    {
    }

    /// Units per second. Negative Y is up.
    public Vector2 Velocity { get; set; }

    /// -1 steers left, 1 steers right, 0 stands still. Values outside that range are clamped,
    /// and NaN counts as 0, so bad input can't fling the player off at huge speed 
    public float MoveDirection
    {
        get { return _moveDirection; }
        set { _moveDirection = float.IsNaN(value) ? 0f : Math.Clamp(value, -1f, 1f); }
    }

    public bool IsFalling => Velocity.Y > 0f;

    public override void Update(float deltaSeconds)
    {
        Velocity = new Vector2(MoveDirection * MoveSpeed, Velocity.Y + (Gravity * deltaSeconds));
        Position += Velocity * deltaSeconds;
    }

    /// Launches the player upward at the given speed 
    public void Bounce(float speed = JumpSpeed)
    {
        Velocity = new Vector2(Velocity.X, -speed);
    }
}
