using DailyDoodle.Core.LevelGeneration;
using DailyDoodle.Core.Progress;
using Microsoft.JSInterop;

namespace DailyDoodle.Web.Services;

/// <summary>
/// Keeps the player's <see cref="PlayerProgress"/> in the browser's localStorage, so finished days and
/// best heights survive a reload. There's no server: progress lives on this device only.
/// </summary>
/// <remarks>
/// If storage is blocked (some private browsing modes) or the save is damaged, the game still works;
/// progress is kept in memory until the page closes.
/// </remarks>
public sealed class ProgressStore(IJSRuntime js)
{
    private const string StorageKey = "dailyDoodleJump.progress";

    private PlayerProgress? _progress;

    /// <summary>Raised after a run is saved, so pages showing progress can update.</summary>
    public event Action? Changed;

    /// <summary>The saved progress. Loaded from storage the first time it's needed.</summary>
    public PlayerProgress Progress => _progress ??= Load();

    /// <summary>Saves one run's result. See <see cref="PlayerProgress.RecordRun"/>.</summary>
    /// <returns>True if the run set a new best height for that day.</returns>
    public bool RecordRun(DateOnly levelDate, float height, bool finished)
    {
        var today = LevelDate.Today;

        // A level from "the future" can only happen if the clock changed mid-run. Don't save it.
        if (levelDate > today)
        {
            return false;
        }

        // The game never reports a bad height, but a save must never be able to crash a frame.
        var safeHeight = float.IsFinite(height) ? Math.Max(0f, height) : 0f;
        var isNewBest = Progress.RecordRun(levelDate, safeHeight, finished, today);
        Save();
        Changed?.Invoke();
        return isNewBest;
    }

    private PlayerProgress Load()
    {
        try
        {
            return PlayerProgress.FromJson(Storage?.Invoke<string?>("localStorage.getItem", StorageKey));
        }
        catch (JSException)
        {
            return new PlayerProgress();
        }
    }

    private void Save()
    {
        try
        {
            Storage?.InvokeVoid("localStorage.setItem", StorageKey, Progress.ToJson());
        }
        catch (JSException)
        {
            // Storage is blocked or full. Keep going with the in-memory copy.
        }
    }

    // In the browser (WebAssembly) JS calls can run synchronously, which lets the game's
    // frame callback save a run without waiting. Anywhere else, progress stays in memory.
    private IJSInProcessRuntime? Storage => js as IJSInProcessRuntime;
}
