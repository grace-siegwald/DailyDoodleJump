namespace DailyDoodle.Core.LevelGeneration;

public enum PlatformType
{
    /// <summary>Solid, stationary.</summary>
    Normal,
    /// <summary>Slides left/right by MoveRange around its X.</summary>
    Moving,
    /// <summary>Breaks when landed on; never part of the guaranteed path.</summary>
    Breakable,
    /// <summary>Launches the player higher than a normal bounce.</summary>
    Spring,
}

/// <summary>
/// A platform in world space. Y grows UPWARD (0 = starting platform).
/// X is the left edge. Convert to screen space when rendering:
/// screenY = viewportHeight - (Y - cameraY).
/// </summary>
public sealed record Platform(
    int Id,
    double X,
    double Y,
    double Width,
    PlatformType Type,
    double MoveRange = 0,
    double MoveSpeed = 0);
