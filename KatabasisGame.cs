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
        Globals.WindowSize = new(1920, 1080);
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        //ballPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
        //ballSpeed = 200f;

        Content.RootDirectory = "Content";
        Globals.Content = Content;

        _gameManager = new();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Globals.SpriteBatch = _spriteBatch;
        Sprites.Load(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Globals.Update(gameTime);
        _gameManager.Update();

        // TODO: Add your update logic here
        // MoveBall(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _gameManager.Draw();

        /*
        _spriteBatch.Begin();
        
        _spriteBatch.Draw(
            ballTexture,
            ballPosition,
            null,
            Color.White,
            0f,
            new Vector2(ballTexture.Width / 2, ballTexture.Height / 2),
            Vector2.One,
            SpriteEffects.None,
            0f
        );
        _spriteBatch.End();
        */

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
