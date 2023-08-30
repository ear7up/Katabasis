using System;
using System.IO;
using System.Text.Json;
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

    public GameModel _gameModel;
    public GameManager _gameManager;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private bool _fullscreen;

    public KatabasisGame()
    {
        Instance = this;

        _graphics = new GraphicsDeviceManager(this);
        Globals.WindowSize = new(1600, 900);
        _graphics.PreferredBackBufferWidth = Globals.WindowSize.X;
        _graphics.PreferredBackBufferHeight = Globals.WindowSize.Y;
        _graphics.HardwareModeSwitch = false;
        _fullscreen = false;

        IsMouseVisible = true;
    }

    public void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        _graphics.ToggleFullScreen();
        _graphics.ApplyChanges();

        if (_fullscreen)
            Globals.WindowSize = new(
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        else
            Globals.WindowSize = new(1600, 900);
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
        MediaPlayer.Volume = Config.MusicVolume;
        MediaPlayer.MediaStateChanged += SongRestarted;

        Goods.CalcGoodsTypecounts();
        GoodsInfo.Init();

        // GoodsProduction relies on GoodsInfo.Init
        GoodsProduction.Init();
        BuildingProduction.Init();
        MineralInfo.Init();
        BuildingInfo.Init();

        _gameModel = new();
        _gameModel.InitNew();

        _gameManager = new();
        _gameManager.SetGameModel(_gameModel);
    }

    void SongRestarted(object sender, System.EventArgs e)
    {
        // MediaPlayer.Play(_defaultBGM);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        //    Exit();

        if (InputManager.SavePressed)
            Save();

        if (InputManager.LoadPressed)
            Load();

        Globals.Update(gameTime);
        _gameManager.Update(gameTime);

        base.Update(gameTime);
    }

    public void Save()
    {
        FileStream fileStream = File.Create("save.json");
        JsonSerializer.Serialize(fileStream, _gameModel, Globals.JsonOptionsS);
        fileStream.Close();
    }

    public void Load()
    {
        // Clear out the old objects being drawn from the global buffers
        Globals.Ybuffer.Clear();
        Globals.TextBuffer.Clear();
        
        string jsonText = File.ReadAllText("save.json");
        _gameModel = JsonSerializer.Deserialize<GameModel>(jsonText, Globals.JsonOptionsS);
        _gameModel.InitLoaded();

        _gameManager.SetGameModel(_gameModel);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _gameManager.Draw();
        base.Draw(gameTime);
    }
}
