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
    public static readonly Vector2 DefaultSize = new(60f, 12f);

    public Platform(Vector2 position)
        : base(position, DefaultSize)
    {
    }

    /// <summary>Called by <see cref="GameWorld"/> when the player falls onto the top of this platform.</summary>
    public virtual void OnLanded(Player player)
    {
        player.Bounce();
    }
}
