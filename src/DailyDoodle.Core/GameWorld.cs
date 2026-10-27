using System.Numerics;

namespace DailyDoodle.Core;

/// <summary>
/// Holds everything in a level and runs the rules: movement, landing on platforms, screen wrap,
/// scrolling and game over. Uses no KNI types, so all of it can be unit tested without a browser.
/// </summary>
public class GameWorld
{
    /// <summary>Width of the play area in game units.</summary>
    public const float Width = 400f;

    /// <summary>Height of the play area in game units.</summary>
    public const float Height = 640f;

    /// <summary>
    /// Length of one physics step. The world always moves in steps of exactly this size, so the
    /// game plays the same on 60 Hz and 120 Hz screens, and every player gets the same daily level.
    /// </summary>
    public const float StepSeconds = 1f / 120f;

    /// <summary>
    /// Longest frame the world will simulate. After the browser tab was in the background, the first
    /// frame back can report several seconds; without this cap the player would fall straight through.
    /// </summary>
    public const float MaxFrameSeconds = 1f / 30f;

    // While climbing, the camera keeps the player's top edge this far below the top of the screen.
    private const float CameraFollowLine = Height * 0.4f;

    private readonly List<GameObject> _objects = [];
    private readonly float _startBottom;
    private float _unsimulatedSeconds;

    public GameWorld(Player player, IEnumerable<GameObject> objects)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(objects);

        Player = player;
        _startBottom = player.Bottom;

        foreach (var gameObject in objects)
        {
            Add(gameObject);
        }
    }

    public Player Player { get; }

    /// <summary>Everything except the player: platforms, and later enemies and items.</summary>
    public IReadOnlyList<GameObject> Objects => _objects;

    /// <summary>
    /// World Y at the top edge of the screen. Starts at 0 and only goes down (more negative) as the
    /// player climbs. To draw something, subtract this from its Y.
    /// </summary>
    public float CameraTop { get; private set; }

    /// <summary>The highest the player has got above where they started. Good basis for a score.</summary>
    public float HeightClimbed { get; private set; }

    public bool IsGameOver { get; private set; }

    public void Add(GameObject gameObject)
    {
        ArgumentNullException.ThrowIfNull(gameObject);

        if (gameObject is Player)
        {
            throw new ArgumentException("Pass the player to the constructor instead of adding it.", nameof(gameObject));
        }

        _objects.Add(gameObject);
    }

    /// <summary>
    /// Moves the world forward by the real time since the last frame. Call once per frame.
    /// </summary>
    /// <param name="deltaSeconds">Seconds since the last call. Must be zero or more.</param>
    /// <param name="input">What the player is pressing.</param>
    public void Advance(float deltaSeconds, PlayerInput input)
    {
        if (!float.IsFinite(deltaSeconds) || deltaSeconds < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Elapsed time must be zero or more.");
        }

        if (IsGameOver)
        {
            return;
        }

        Player.MoveDirection = input.Direction;

        // Run as many whole steps as fit in the time we were given. Any leftover time carries
        // over to the next frame, so no time is lost.
        _unsimulatedSeconds += Math.Min(deltaSeconds, MaxFrameSeconds);
        while (_unsimulatedSeconds >= StepSeconds && !IsGameOver)
        {
            Step(StepSeconds);
            _unsimulatedSeconds -= StepSeconds;
        }
    }

    private void Step(float deltaSeconds)
    {
        var previousBottom = Player.Bottom;

        Player.Update(deltaSeconds);
        WrapHorizontally(Player);

        foreach (var gameObject in _objects)
        {
            if (gameObject.IsActive)
            {
                gameObject.Update(deltaSeconds);
            }
        }

        // A rising player can never land, so skip the platform checks while going up.
        if (Player.IsFalling)
        {
            TryLand(previousBottom);
        }

        FollowPlayer();
        HeightClimbed = Math.Max(HeightClimbed, _startBottom - Player.Bottom);

        if (Player.Top > CameraTop + Height)
        {
            IsGameOver = true;
        }

        // Remove broken platforms, collected items and anything that scrolled off the bottom.
        // This happens after the loops above, because removing from a list while looping over it throws.
        _objects.RemoveAll(gameObject => !gameObject.IsActive || gameObject.Top > CameraTop + Height);
    }

    // Lands the player on a platform if, during this step, their feet went from above its top
    // edge to at or below it. Checking where the feet were last step (instead of just "are they
    // overlapping now") is what makes platforms one-way, and stops fast falls skipping past them.
    private void TryLand(float previousBottom)
    {
        foreach (var platform in _objects.OfType<Platform>())
        {
            var wasAbove = previousBottom <= platform.Top;
            var isNowAtOrBelow = Player.Bottom >= platform.Top;

            if (platform.IsActive && wasAbove && isNowAtOrBelow && Player.OverlapsHorizontally(platform))
            {
                // Put the feet exactly on the platform so the player doesn't sink into it.
                Player.Position = new Vector2(Player.Position.X, platform.Top - Player.Size.Y);
                platform.OnLanded(Player);
                return;
            }
        }
    }

    // Like Doodle Jump: leave one side of the screen and come back on the other.
    private static void WrapHorizontally(GameObject gameObject)
    {
        if (gameObject.Left > Width)
        {
            gameObject.Position = new Vector2(-gameObject.Size.X, gameObject.Position.Y);
        }
        else if (gameObject.Right < 0f)
        {
            gameObject.Position = new Vector2(Width, gameObject.Position.Y);
        }
    }

    // The camera only ever moves up, so falling past the bottom of the screen ends the game.
    private void FollowPlayer()
    {
        if (Player.Top < CameraTop + CameraFollowLine)
        {
            CameraTop = Player.Top - CameraFollowLine;
        }
    }
}
