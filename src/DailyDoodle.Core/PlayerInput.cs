namespace DailyDoodle.Core;

/// <summary>
/// What the player is pressing this frame. <see cref="DailyDoodleGame"/> builds it from the keyboard
/// and touch screen; tests build it directly.
/// </summary>
/// <param name="Direction">-1 for left, 1 for right, 0 for neither.</param>
public readonly record struct PlayerInput(float Direction)
{
    public static PlayerInput None => default;

    public static PlayerInput FromKeys(bool left, bool right) =>
        new((right ? 1f : 0f) - (left ? 1f : 0f));
}
