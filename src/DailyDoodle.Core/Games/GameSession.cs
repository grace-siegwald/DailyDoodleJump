namespace DailyDoodle.Core.Games;

/// <summary>
/// Which game the player has open. Registered as a single shared service,
/// so Home and Library always agree on the current game.
/// </summary>
public sealed class GameSession
{
    public GameInfo CurrentGame { get; private set; } = GameCatalog.All[0];

    /// <summary>Raised when the current game changes.</summary>
    public event Action? Changed;

    public void Select(string gameId)
    {
        var game = GameCatalog.Find(gameId)
            ?? throw new ArgumentException($"Unknown game id '{gameId}'.", nameof(gameId));

        if (game == CurrentGame)
        {
            return;
        }

        CurrentGame = game;
        Changed?.Invoke();
    }
}
