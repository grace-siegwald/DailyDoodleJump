namespace DailyDoodle.Core.LevelGeneration;

/// <summary>
/// Infinite, seeded level generator.
///
/// Mental model: it lays down a "guaranteed path" one platform at a time, where each
/// platform is reachable from the previous one given the jump physics. Then it sprinkles
/// extras (easy filler platforms early, breakable decoys later) around that path.
///
/// Generation is lazy: call EnsureGeneratedUpTo(cameraTop + buffer) every frame and
/// RemoveBelow(cameraBottom - buffer) to drop old platforms. Because platforms are always
/// generated in the same order from the same seed, the level is identical no matter how
/// often or in what increments you call it.
/// </summary>
public sealed class LevelGenerator
{
    private readonly LevelSettings _s;
    private readonly SeededRandom _rng;
    private readonly List<Platform> _platforms = new();
    private Platform _lastPathPlatform;
    private int _nextId;

    public LevelGenerator(ulong seed, LevelSettings? settings = null)
    {
        _s = settings ?? new LevelSettings();
        _rng = new SeededRandom(seed);

        // Wide, safe starting platform centered under the player.
        double startWidth = _s.PlatformWidth * 1.5;
        _lastPathPlatform = Add((_s.WorldWidth - startWidth) / 2, 0, startWidth, PlatformType.Normal);
    }

    public LevelSettings Settings => _s;

    /// <summary>All live platforms, ordered bottom to top.</summary>
    public IReadOnlyList<Platform> Platforms => _platforms;

    /// <summary>Highest Y generated so far.</summary>
    public double GeneratedHeight => _lastPathPlatform.Y;

    /// <summary>0 at the start, 1 at HeightForMaxDifficulty and above.</summary>
    public double Difficulty(double height) => Math.Clamp(height / _s.HeightForMaxDifficulty, 0, 1);

    public void EnsureGeneratedUpTo(double height)
    {
        while (_lastPathPlatform.Y < height)
        {
            GenerateNextStep();
        }
    }

    /// <summary>Drops platforms below a world Y (off-screen). Returns how many were removed.</summary>
    public int RemoveBelow(double y) => _platforms.RemoveAll(p => p.Y < y);

    private void GenerateNextStep()
    {
        Platform prev = _lastPathPlatform;
        double d = Difficulty(prev.Y);

        // 1. Type of the next path platform (never Breakable: you can't bounce off it).
        PlatformType type = PickPathType(d);

        // 2. Vertical gap: grows with difficulty but never exceeds a safe jump.
        //    Moving platforms get a smaller cap because their position is harder to judge.
        double cap = type == PlatformType.Moving ? _s.SafeMaxGap * 0.8 : _s.SafeMaxGap;
        double maxGap = Math.Min(Lerp(_s.SafeMaxGap * 0.45, _s.SafeMaxGap, d), cap);
        double gap = _rng.Range(_s.MinGap, Math.Max(_s.MinGap, maxGap));
        double y = prev.Y + gap;

        // 3. Platforms shrink as difficulty rises.
        double width = Lerp(_s.PlatformWidth, _s.PlatformWidth * 0.7, d);

        // 4. Horizontal position: within what the player can travel sideways during the jump.
        double reach = HorizontalReach(gap);
        double center = prev.X + prev.Width / 2 + _rng.Range(-reach, reach);
        if (_s.HorizontalWrap)
        {
            center = Wrap(center);
        }
        center = Math.Clamp(center, width / 2, _s.WorldWidth - width / 2);

        double moveRange = 0, moveSpeed = 0;
        if (type == PlatformType.Moving)
        {
            moveRange = Lerp(30, 100, d);
            moveSpeed = Lerp(40, 120, d);
            // Keep the whole oscillation on screen.
            center = Math.Clamp(center, width / 2 + moveRange, _s.WorldWidth - width / 2 - moveRange);
        }

        // 5. Extras between prev and the new platform.
        AddExtras(prev.Y, y, d);

        _lastPathPlatform = Add(center - width / 2, y, width, type, moveRange, moveSpeed);
    }

    private void AddExtras(double fromY, double toY, double d)
    {
        double span = toY - fromY;
        if (span < 40)
        {
            return; // no room
        }

        // Early on: bonus solid platforms make the climb forgiving.
        if (_rng.Chance(Lerp(0.6, 0.0, d)))
        {
            AddRandomAt(_rng.Range(fromY + 20, toY - 20), PlatformType.Normal);
        }

        // Later: breakable decoys that look like a way up but aren't.
        if (_rng.Chance(Lerp(0.1, 0.5, d)))
        {
            AddRandomAt(_rng.Range(fromY + 20, toY - 20), PlatformType.Breakable);
        }
    }

    private void AddRandomAt(double y, PlatformType type)
    {
        double width = _s.PlatformWidth;
        double x = _rng.Range(0, _s.WorldWidth - width);
        Add(x, y, width, type);
    }

    private PlatformType PickPathType(double d)
    {
        double springChance = 0.05;
        double movingChance = Lerp(0.0, 0.35, d);
        double r = _rng.NextDouble();
        if (r < springChance)
        {
            return PlatformType.Spring;
        }
        return r < springChance + movingChance ? PlatformType.Moving : PlatformType.Normal;
    }

    /// <summary>
    /// How far sideways the player can get while rising dy and coming back down onto it.
    /// Solve dy = v*t - g*t^2/2 for the later root: t = (v + sqrt(v^2 - 2*g*dy)) / g.
    /// </summary>
    private double HorizontalReach(double dy)
    {
        double v = _s.JumpVelocity, g = _s.Gravity;
        double disc = v * v - 2 * g * dy;
        if (disc < 0)
        {
            return 0;
        }
        double airtime = (v + Math.Sqrt(disc)) / g;
        return _s.HorizontalSpeed * airtime * _s.SafetyMargin;
    }

    private Platform Add(double x, double y, double width, PlatformType type,
                         double moveRange = 0, double moveSpeed = 0)
    {
        var p = new Platform(_nextId++, x, y, width, type, moveRange, moveSpeed);

        // Extras are generated below the next path platform, so insert sorted by Y.
        int i = _platforms.Count;
        while (i > 0 && _platforms[i - 1].Y > y)
        {
            i--;
        }
        _platforms.Insert(i, p);
        return p;
    }

    private double Wrap(double x)
    {
        double w = _s.WorldWidth;
        return ((x % w) + w) % w;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;
}
