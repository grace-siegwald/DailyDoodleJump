namespace DailyDoodle.Core;

/// <summary>Where a run is: still climbing, or over.</summary>
public enum RunState
{
    /// <summary>The player is still climbing.</summary>
    Playing,

    /// <summary>The player fell off the bottom of the screen or touched an enemy.</summary>
    Fell,

    /// <summary>The player reached the finish line.</summary>
    Won,
}
