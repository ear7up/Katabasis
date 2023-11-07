using System;
using System.IO;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Katabasis;

public enum SaveSlot
{
    DEFAULT,
    AUTO,
    QUICK,
    SLOT1,
    SLOT2,
    SLOT3
}

public class KatabasisGame : Game
{
    // some helpful static properties
    public static KatabasisGame Instance { get; private set; }
    public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
    public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
    public static string CurrentSaveName { get; set; }
    
    private Song _defaultBGM;

    public GameModel _gameModel;
    public GameManager _gameManager;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private bool _fullscreen;
    private double _secondsSinceLastSave;

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
        CurrentSaveName = SaveSlot.AUTO.ToString();
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

        // Reposition elements hidden off-screen
        _gameManager.ResizeScreen();
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
        SoundEffects.Load(Content);

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
        FoodInfo.Init();

        Mouse.SetCursor(MouseCursor.FromTexture2D(Sprites.Cursor.Texture, 0, 0));

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

        // Auto-save (if enabled)
        _secondsSinceLastSave += gameTime.ElapsedGameTime.TotalSeconds;
        if (_secondsSinceLastSave >= Config.AutoSaveFrequencySeconds && Config.AutoSaveFrequencySeconds > 0)
        {
            _secondsSinceLastSave = 0;
            Save(SaveSlot.AUTO);
        }

        if (InputManager.SavePressed)
            Save();

        if (InputManager.LoadPressed)
            Load();

        Globals.Update(gameTime);
        _gameManager.Update(gameTime);

        base.Update(gameTime);
    }

    public static string GetFilename(SaveSlot slot)
    {
        return $"{slot.ToString()}.json";
    }

    public string GetLastModified(SaveSlot slot)
    {
        if (!File.Exists(GetFilename(slot)))
            return "";

        DateTime lastModified = File.GetLastWriteTime(GetFilename(slot));
        return lastModified.ToString();
    }

    public void Save(SaveSlot slot = SaveSlot.DEFAULT)
    {
        string filename = CurrentSaveName;
        if (slot != SaveSlot.DEFAULT)
            filename = slot.ToString();

        FileStream fileStream = File.Create($"{filename}.json");
        JsonSerializer.Serialize(fileStream, _gameModel, Globals.JsonOptionsS);
        fileStream.Close();

        if (slot != SaveSlot.AUTO)
            CurrentSaveName = filename;
    }

    public void Load(SaveSlot slot = SaveSlot.DEFAULT)
    {
        string filename = CurrentSaveName;
        if (slot != SaveSlot.DEFAULT)
            filename = slot.ToString();

        // Clear out the old objects being drawn from the global buffers
        Globals.Ybuffer.Clear();
        Globals.TextBuffer.Clear();
        
        string jsonText = File.ReadAllText($"{filename}.json");
        Building.IdCounter = 0;
        _gameModel = JsonSerializer.Deserialize<GameModel>(jsonText, Globals.JsonOptionsS);
        _gameModel.InitLoaded();

        _gameManager.SetGameModel(_gameModel);

        if (slot != SaveSlot.AUTO)
            CurrentSaveName = filename;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _gameManager.Draw();
        base.Draw(gameTime);
    }
}
