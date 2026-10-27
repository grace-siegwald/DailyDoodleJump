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

    private Texture2D _playerTexture = null!;
    private Texture2D _platformTexture = null!;
    private Texture2D _springTexture = null!;
    private Texture2D _breakingTexture = null!;
    private Texture2D _movingTexture = null!;

    // Where the 400x640 play area sits on the real screen. Recalculated every frame,
    // because the canvas resizes with the browser window.
    private float _scale = 1f;
    private Vector2 _offset;

    // Where the canvas is on the web page, in CSS pixels. KNI reports touches in page coordinates,
    // not canvas coordinates, so this is needed to tell which half of the game a finger is on.
    // Updated every frame by the page (see SetCanvasPlacement).
    private Vector2 _canvasTopLeft;
    private float _canvasCssWidth;

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

        // These images are cropped tight to the drawing and sized for the game (see art/README.md).
        // Full-size originals would be drawn so much smaller than they are that most of their
        // pixels get skipped, which breaks up the outlines.
        _playerTexture = LoadTexture("images/Player.png");
        _platformTexture = LoadTexture("images/Platform_Normal.png");
        _springTexture = LoadTexture("images/Platform_Trampoline.png");
        _breakingTexture = LoadTexture("images/Platform_Breakable.png");
        _movingTexture = LoadTexture("images/Platform_Moveable.png");
    }

    /// <summary>
    /// Tells the game where its canvas is on the web page, in CSS pixels.
    /// The page (Home.razor) calls this every frame, before <see cref="Game.Tick"/>.
    /// </summary>
    public void SetCanvasPlacement(float left, float top, float width)
    {
        // Ignore nonsense (a canvas that isn't laid out yet has width 0) and keep the last good values.
        if (!float.IsFinite(left) || !float.IsFinite(top) || !float.IsFinite(width) || width <= 0f)
        {
            return;
        }

        _canvasTopLeft = new Vector2(left, top);
        _canvasCssWidth = width;
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
            DrawArt(TextureFor(gameObject), gameObject);
        }

        // Player last, so it's drawn in front of the platforms.
        DrawArt(_playerTexture, _world.Player);

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
    // Anything without art yet (a new enemy, say) is drawn as a plain white square.
    private Texture2D TextureFor(GameObject gameObject) => gameObject switch
    {
        SpringPlatform => _springTexture,
        BreakingPlatform => _breakingTexture,
        MovingPlatform => _movingTexture,
        Platform => _platformTexture,
        _ => _pixel,
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

    private (bool Left, bool Right) ReadInput()
    {
        var keyboard = Keyboard.GetState();
        var left = keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A);
        var right = keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D);

        // Until the page has said where the canvas is, there's no way to place a touch.
        if (_canvasCssWidth <= 0f)
        {
            return (left, right);
        }

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

    // Converts a touch (web page coordinates, in CSS pixels) into game units.
    private Vector2 ToPlayArea(Vector2 pagePoint) => (pagePoint - _canvasTopLeft) * (VirtualWidth / _canvasCssWidth);

    private void FillRectangle(Vector2 position, Vector2 size, Color color)
    {
        _spriteBatch.Draw(_pixel, position, null, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
    }

    // Draws an object's art as wide as the object, keeping the art's proportions, with the art's top
    // edge on the object's top edge. The camera is applied here: world Y minus the camera's Y.
    private void DrawArt(Texture2D texture, GameObject gameObject)
    {
        var screenPosition = new Vector2(gameObject.Left, gameObject.Top - _world.CameraTop);
        var scale = gameObject.Size.X / texture.Width;

        _spriteBatch.Draw(texture, screenPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
