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
    /// <summary>Width of the play area in game units. Art is made at 2x this size.</summary>
    public const int VirtualWidth = 400;

    /// <summary>Height of the play area in game units.</summary>
    public const int VirtualHeight = 640;

    // Longest frame the game will simulate. After the tab was in the background,
    // the first frame back can report a gap of several seconds.
    private const float MaxFrameSeconds = 1f / 30f;

    // Trial-run box settings. Delete these once GameWorld is plugged in.
    private const float BoxSize = 40f;
    private const float FloorY = VirtualHeight - 80f;
    private const float Gravity = 1800f;
    private const float JumpSpeed = -900f;
    private const float MoveSpeed = 350f;

    private static readonly Color _letterboxColor = new(43, 43, 43);
    private static readonly Color _paperColor = new(250, 246, 236);
    private static readonly Color _inkColor = new(60, 60, 60);

    // Must exist for the game to get a graphics device, even though nothing reads it yet.
    private readonly GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private Texture2D _playerTexture = null!;

    // Where the 400x640 play area sits on the real screen. Recalculated every frame,
    // because the canvas resizes with the browser window.
    private float _scale = 1f;
    private Vector2 _offset;

    // Trial-run stand-in for the player. Replace with GameWorld.
    private Vector2 _boxPosition = new(180f, 300f);
    private float _boxVelocityY;

    public DailyDoodleGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Run exactly one Update per browser frame, with the real elapsed time.
        // Fixed-step physics belongs in GameWorld.Advance, so the game plays
        // the same on 60 Hz and 120 Hz screens and can be tested without KNI.
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        Window.Title = "Daily Doodle Jump";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // A 1x1 white texture, tinted and stretched to draw plain rectangles.
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _playerTexture = LoadTexture("images/player.png");
    }

    protected override void Update(GameTime gameTime)
    {
        var dt = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, MaxFrameSeconds);

        UpdateScreenFit();
        var (left, right) = ReadInput();

        // --- Trial run: a box that bounces and steers. Replace with _world.Advance(dt, input). ---
        var direction = (right ? 1f : 0f) - (left ? 1f : 0f);
        _boxPosition.X += direction * MoveSpeed * dt;
        _boxVelocityY += Gravity * dt;
        _boxPosition.Y += _boxVelocityY * dt;

        if (_boxPosition.Y + BoxSize >= FloorY)
        {
            _boxPosition.Y = FloorY - BoxSize;
            _boxVelocityY = JumpSpeed;
        }

        // Wrap around the side edges, like Doodle Jump.
        if (_boxPosition.X > VirtualWidth)
        {
            _boxPosition.X = -BoxSize;
        }
        else if (_boxPosition.X < -BoxSize)
        {
            _boxPosition.X = VirtualWidth;
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
        FillRectangle(new Vector2(0f, FloorY), new Vector2(VirtualWidth, 4f), _inkColor);
        DrawSprite(_playerTexture, _boxPosition, new Vector2(BoxSize, BoxSize));

        _spriteBatch.End();

        base.Draw(gameTime);
    }

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

    private void UpdateScreenFit()
    {
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

    // Draws a texture stretched to a size in game units (2x art ends up drawn at half its pixel size).
    private void DrawSprite(Texture2D texture, Vector2 position, Vector2 size)
    {
        var scale = new Vector2(size.X / texture.Width, size.Y / texture.Height);
        _spriteBatch.Draw(texture, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
