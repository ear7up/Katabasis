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
    
    private Song _defaultBGM;
    public GameManager _gameManager;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public KatabasisGame()
    {
        Instance = this;

        _graphics = new GraphicsDeviceManager(this);
        Globals.WindowSize = new(1600, 900);
        _graphics.PreferredBackBufferWidth = Globals.WindowSize.X;
        _graphics.PreferredBackBufferHeight = Globals.WindowSize.Y;
        _graphics.HardwareModeSwitch = false;

        IsMouseVisible = true;
    }

    public void ToggleFullscreen()
    {
        _graphics.ToggleFullScreen();
        _graphics.ApplyChanges();
        Globals.WindowSize = new(
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
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

        _defaultBGM = Globals.Content.Load<Song>("Desert-City");
        MediaPlayer.Play(_defaultBGM);
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.2f;
        MediaPlayer.MediaStateChanged += SongRestarted;

        _gameManager = new();
    }

    void SongRestarted(object sender, System.EventArgs e)
    {
        // MediaPlayer.Play(_defaultBGM);
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
