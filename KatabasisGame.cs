using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Katabasis;

public class KatabasisGame : Game
{
    // some helpful static properties
    public static KatabasisGame Instance { get; private set; }
    public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
    public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
    //public static GameTime GameTime { get; private set; }
    private GameManager _gameManager;
    //public static ParticleManager<ParticleState> ParticleManager { get; private set; }
    //public static Grid Grid { get; private set; }

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public KatabasisGame()
    {
        Instance = this;

        _graphics = new GraphicsDeviceManager(this);
        Globals.WindowSize = new(1600, 900);
        _graphics.PreferredBackBufferWidth = Globals.WindowSize.X;
        _graphics.PreferredBackBufferHeight = Globals.WindowSize.Y;

        IsMouseVisible = true;
    }

    public void ToggleFullscreen()
    {
        if (_graphics.IsFullScreen)
        {
            Globals.WindowSize = new(1600, 900);
            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 900;
        }
        else
        {
            Globals.WindowSize = new(GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height);
        }
        _graphics.ToggleFullScreen();
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Content.RootDirectory = "Content";
        Globals.Content = Content;
        Globals.Rand = new Random();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Globals.SpriteBatch = _spriteBatch;
        Sprites.Load(Content);

        _gameManager = new();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Globals.Update(gameTime);
        _gameManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _gameManager.Draw();
        base.Draw(gameTime);
    }
}
