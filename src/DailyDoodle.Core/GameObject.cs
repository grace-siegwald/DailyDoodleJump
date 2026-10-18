using System.Numerics;

namespace DailyDoodle.Core;

/// 
/// Base class for everything that exists in the game world: the player, platforms, enemies, items.
/// 
/// Positions and sizes are in game units (the play area is 400 x 640). Y grows downward, like on
/// screen, so climbing higher means Y gets smaller and eventually negative.
///
/// In short, a game object has:
///     Position
///     Size
///     State (isActive)
///     Sides (Left, Right, Top, Bottom)
///     Overlapping Check (overlapping with other GameObject)
///     

public abstract class GameObject
{
    protected GameObject(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    /// Top-left corner of the object
    public Vector2 Position { get; set; }

    /// Width (X) and height (Y)
    public Vector2 Size { get; }

    /// Set to false when the object should be removed, like a broken platform or a collected item
    public bool IsActive { get; set; } = true;

    public float Left
    {
        get { return Position.X; }
    }

    public float Right
    {
        get { return Position.X + Size.X; }
    }

    public float Top
    {
        get { return Position.Y; }
    }

    public float Bottom
    {
        get { return Position.Y + Size.Y; }
    }

    /// True if the two objects' boxes overlap. Touching edges don't count 
    public bool Overlaps(GameObject other)
    {
        return OverlapsHorizontally(other) && Bottom > other.Top && Top < other.Bottom;
    }

    /// True if the two objects share some horizontal space, whatever their heights 
    public bool OverlapsHorizontally(GameObject other)
    {
        return Right > other.Left && Left < other.Right;
    }

    /// Moves or animates the object. Called once per physics step. Does nothing by default 
    public virtual void Update(float deltaTime)
    {
        /// Need deltaTime to time correct all movement for differing framerates
    }

    private static bool IsFinite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
}
