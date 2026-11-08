namespace DailyDoodle.Core.LevelGeneration;

/// <summary>
/// Deterministic random number generator (SplitMix64).
/// We use our own instead of System.Random because System.Random's output for a
/// given seed is not guaranteed to stay the same across .NET versions or platforms.
/// For a "daily" level, every player must get the exact same sequence.
/// </summary>
public sealed class SeededRandom
{
    private ulong _state;

    public SeededRandom(ulong seed) => _state = seed;

    public ulong NextUInt64()
    {
        ulong z = _state += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    /// <summary>Uniform double in [0, 1).</summary>
    public double NextDouble() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));

    /// <summary>Uniform double in [min, max).</summary>
    public double Range(double min, double max) => min + (max - min) * NextDouble();

    /// <summary>True with probability p.</summary>
    public bool Chance(double p) => NextDouble() < p;
}
