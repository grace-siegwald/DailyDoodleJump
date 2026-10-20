namespace DailyDoodle.Core.LevelGeneration;

/// <summary>
/// Tuning knobs. The physics values MUST match the ones your game loop uses,
/// because the generator uses them to guarantee every gap is jumpable.
/// Units: pixels and seconds.
/// </summary>
public sealed record LevelSettings
{
    public double WorldWidth { get; init; } = 400;
    public double Gravity { get; init; } = 1500;          // px/s^2
    public double JumpVelocity { get; init; } = 800;      // px/s, upward speed right after a bounce
    public double HorizontalSpeed { get; init; } = 300;   // px/s, player's max sideways speed
    public bool HorizontalWrap { get; init; } = true;     // Doodle Jump style: exit left, appear right

    public double PlatformWidth { get; init; } = 60;
    public double MinGap { get; init; } = 30;

    /// <summary>Fraction of the theoretical max we actually allow (leaves room for human error).</summary>
    public double SafetyMargin { get; init; } = 0.85;

    /// <summary>Height at which difficulty reaches 100%.</summary>
    public double HeightForMaxDifficulty { get; init; } = 20000;

    /// <summary>Peak of a normal jump: v^2 / 2g.</summary>
    public double MaxJumpHeight => JumpVelocity * JumpVelocity / (2 * Gravity);

    /// <summary>Largest vertical gap the generator will ever create between path platforms.</summary>
    public double SafeMaxGap => MaxJumpHeight * SafetyMargin;
}
