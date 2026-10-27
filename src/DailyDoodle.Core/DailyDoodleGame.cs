using DailyDoodle.Core.LevelGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace DailyDoodle.Core;

/// <summary>
/// The KNI game. Every browser frame it reads input, advances the game, and draws it.
/// </summary>
/// <remarks>
/// Keep game rules out of this class. Put physics, collisions, level generation and streaks in their
/// own plain classes (no KNI types), so they can be unit tested without a browser. This class only
/// connects them to the screen.
/// </remarks>
public class DailyDoodleGame : Game
{
    /// <summary>Width of the play area in game units. Comes from <see cref="GameWorld"/> so the two always agree.</summary>
    public const int VirtualWidth = (int)GameWorld.Width;

    /// <summary>Height of the play area in game units.</summary>
    public const int VirtualHeight = (int)GameWorld.Height;

    private static readonly Color _letterboxColor = new(43, 43, 43);
    private static readonly Color _paperColor = new(250, 246, 236);
    private static readonly Color _gameOverShade = new(0, 0, 0, 110);

    // Keeps the back buffer the same size as the canvas on the page (see UpdateScreenFit).
    private readonly GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    private SpriteArt _playerArt;
    private SpriteArt _platformArt;
    private SpriteArt _springArt;
    private SpriteArt _breakingArt;
    private SpriteArt _movingArt;

    // Drawn for any object that has no art yet (a new enemy, say): a plain white square.
    private SpriteArt _missingArt;

    // Where the 400x640 play area sits on the real screen. Recalculated every frame,
    // because the canvas resizes with the browser window.
    private float _scale = 1f;
    private Vector2 _offset;

    // The current run. Replaced by StartNewRun when the player restarts.
    private GameWorld _world = null!;
    private PlatformSpawner _spawner = null!;

    public DailyDoodleGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Run exactly one Update per browser frame, with the real elapsed time.
        // Fixed-step physics happens in GameWorld.Advance, so the game plays
        // the same on 60 Hz and 120 Hz screens and can be tested without KNI.
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        Window.Title = "Daily Doodle Jump";
        StartNewRun();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // A 1x1 white texture, tinted and stretched to draw plain rectangles.
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _missingArt = new SpriteArt(_pixel, new Rectangle(0, 0, 1, 1));

        // Each image is a 1920x1920 square with the drawing in the middle. The rectangle is the part
        // with the drawing in it, so the drawing's top edge lines up with the object's top edge.
        // If the art is re-exported, re-measure these (or crop the PNGs and use the whole image).
        _playerArt = LoadArt("images/Player.png", new Rectangle(117, 159, 1682, 1655));
        _platformArt = LoadArt("images/Platform_Normal.png", new Rectangle(69, 620, 1783, 590));
        _springArt = LoadArt("images/Platform_Trampoline.png", new Rectangle(92, 633, 1736, 654));
        _breakingArt = LoadArt("images/Platform_Breakable.png", new Rectangle(44, 704, 1827, 566));
        _movingArt = LoadArt("images/Platform_Moveable.png", new Rectangle(132, 379, 1693, 1266));
    }

    protected override void Update(GameTime gameTime)
    {
        UpdateScreenFit();

        if (_world.IsGameOver)
        {
            if (IsRestartPressed())
            {
                StartNewRun();
            }
        }
        else
        {
            var (left, right) = ReadInput();
            _world.Advance((float)gameTime.ElapsedGameTime.TotalSeconds, PlayerInput.FromKeys(left, right));
            _spawner.SpawnAhead(_world);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_letterboxColor);

        // Everything is drawn in 400x640 game units; this matrix scales and centers them on the real screen.
        var toScreen = Matrix.CreateScale(_scale, _scale, 1f) * Matrix.CreateTranslation(_offset.X, _offset.Y, 0f);

        // NonPremultiplied, because textures loaded with Texture2D.FromStream keep plain (straight) alpha.
        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied, transformMatrix: toScreen);

        FillRectangle(Vector2.Zero, new Vector2(VirtualWidth, VirtualHeight), _paperColor);

        foreach (var gameObject in _world.Objects)
        {
            DrawArt(ArtFor(gameObject), gameObject);
        }

        // Player last, so it's drawn in front of the platforms.
        DrawArt(_playerArt, _world.Player);

        if (_world.IsGameOver)
        {
            FillRectangle(Vector2.Zero, new Vector2(VirtualWidth, VirtualHeight), _gameOverShade);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // Today's level, from the beginning. Everyone playing today gets the same platforms.
    private void StartNewRun()
    {
        _spawner = new PlatformSpawner(new LevelGenerator(DailySeed.ForToday()));
        _world = new GameWorld(new Player(_spawner.PlayerStart), []);
        _spawner.SpawnAhead(_world);
    }

    // Subclasses must come before Platform, or the compiler reports them as unreachable.
    private SpriteArt ArtFor(GameObject gameObject) => gameObject switch
    {
        SpringPlatform => _springArt,
        BreakingPlatform => _breakingArt,
        MovingPlatform => _movingArt,
        Platform => _platformArt,
        _ => _missingArt,
    };

    /// <summary>
    /// Loads a PNG from the web app's wwwroot folder.
    /// The path must be relative (no leading slash) so it also works on GitHub Pages.
    /// </summary>
    private Texture2D LoadTexture(string path)
    {
        using var stream = TitleContainer.OpenStream(path);
        return Texture2D.FromStream(GraphicsDevice, stream);
    }

    private SpriteArt LoadArt(string path, Rectangle drawingArea) => new(LoadTexture(path), drawingArea);

    private (bool Left, bool Right) ReadInput()
    {
        var keyboard = Keyboard.GetState();
        var left = keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A);
        var right = keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D);

        // On phones: holding the left or right half of the play area steers.
        foreach (var touch in TouchPanel.GetState())
        {
            if (touch.State is TouchLocationState.Pressed or TouchLocationState.Moved)
            {
                if (ToPlayArea(touch.Position).X < VirtualWidth / 2f)
                {
                    left = true;
                }
                else
                {
                    right = true;
                }
            }
        }

        return (left, right);
    }

    // Space or Enter on a keyboard; a new tap on a phone. (A finger still held down from
    // steering doesn't count, so the game doesn't restart the moment you fall.)
    private static bool IsRestartPressed()
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Enter))
        {
            return true;
        }

        foreach (var touch in TouchPanel.GetState())
        {
            if (touch.State == TouchLocationState.Pressed)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateScreenFit()
    {
        // The page sizes the canvas (wwwroot/js/game-loop.js), so the back buffer follows it.
        // Without this the back buffer keeps whatever size KNI last set from the browser window.
        var canvas = Window.ClientBounds;
        if (canvas.Width > 0 && canvas.Height > 0 &&
            (_graphics.PreferredBackBufferWidth != canvas.Width || _graphics.PreferredBackBufferHeight != canvas.Height))
        {
            _graphics.PreferredBackBufferWidth = canvas.Width;
            _graphics.PreferredBackBufferHeight = canvas.Height;
            _graphics.ApplyChanges();
        }

        var backBuffer = GraphicsDevice.PresentationParameters;
        _scale = MathF.Min(
            backBuffer.BackBufferWidth / (float)VirtualWidth,
            backBuffer.BackBufferHeight / (float)VirtualHeight);
        _offset = new Vector2(
            (backBuffer.BackBufferWidth - (VirtualWidth * _scale)) / 2f,
            (backBuffer.BackBufferHeight - (VirtualHeight * _scale)) / 2f);
    }

    // Converts a point on the real screen (a touch, a click) into game units.
    private Vector2 ToPlayArea(Vector2 screenPoint) => (screenPoint - _offset) / _scale;

    private void FillRectangle(Vector2 position, Vector2 size, Color color)
    {
        _spriteBatch.Draw(_pixel, position, null, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
    }

    // Draws an object's art as wide as the object, keeping the art's proportions, with the art's top
    // edge on the object's top edge. The camera is applied here: world Y minus the camera's Y.
    private void DrawArt(SpriteArt art, GameObject gameObject)
    {
        var screenPosition = new Vector2(gameObject.Left, gameObject.Top - _world.CameraTop);
        var scale = gameObject.Size.X / art.DrawingArea.Width;

        _spriteBatch.Draw(art.Texture, screenPosition, art.DrawingArea, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    /// <summary>A texture and the part of it that has the drawing in it.</summary>
    private readonly record struct SpriteArt(Texture2D Texture, Rectangle DrawingArea);
}
