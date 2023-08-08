using System;
using System.Collections.Generic;

namespace Katabasis;

public class GameManager
{
    private readonly Sprite _sky;
    private readonly Map _map;
    private readonly Camera _camera;
    private Player _player1;

    public const int SECONDS_PER_DAY = 60;
    private float TimeOfDay = 0;

    public TextSprite _coordinateDisplay;
    public static TextSprite _debugDisplay;
    public static TextSprite _logoDisplay;
    public static TextSprite _logoDisplay2;
    
    public bool TEST = false;

    public GameManager()
    {
        _sky = new Sprite(Globals.Content.Load<Texture2D>("sky"), Vector2.Zero);
        _sky.Scale = 2f;
        _map = new();
        _camera = new(KatabasisGame.Viewport, _map.Origin);

        _player1 = new(_map.GetOriginTile());

        _coordinateDisplay = new(Sprites.Font);
        _coordinateDisplay.Scale = 0.7f;

        _debugDisplay = new(Sprites.Font);
        _debugDisplay.Position = new Vector2(30f, 30f);

        _logoDisplay = new(Sprites.Font2);
        _logoDisplay.Text = "Katabasis";
        _logoDisplay.FontColor = Color.White;
        _logoDisplay.Position.Y = Globals.WindowSize.Y - _logoDisplay.Height() - 30;
        
        _logoDisplay2 = new(Sprites.Font2);
        _logoDisplay2.Text = "Katabasis";
        _logoDisplay2.Position.Y = Globals.WindowSize.Y - 95;
        _logoDisplay2.FontColor = Color.Black;
        _logoDisplay2.Position.Y = _logoDisplay.Position.Y + 5;

        Goods.CalcGoodsTypecounts();
        GoodsProduction.Init();
        BulidingProduction.Init();
        GoodsInfo.Init();
        BuildingInfo.Init();

        UI.Init();
        UI.AddElement(new UIElement(Sprites.BottomLeftPanel, scale: 1.1f), UI.Position.BOTTOM_LEFT);
        UI.AddElement(new UIElement(Sprites.Clock, scale: 0.5f, onClick: TogglePause), UI.Position.TOP_RIGHT);

        if (TEST)
        {
            RunTests();
        }
        else
        {
            const int NUM_PEOPLE = 100;
            for (int i = 0 ; i < NUM_PEOPLE; i++)
            {
                Person person = Person.CreatePerson(_map.Origin, _map.GetOriginTile());
                _player1.Kingdom.AddPerson(person);
            }
        }
    }

    public void RunTests()
    {
        //MarketTests.RunTests();
        TasksTest.RunTests(_map);
    }

    public static int CompareDrawable(Drawable a, Drawable b)
    {
        float ya = a.GetMaxY();
        float yb = b.GetMaxY();
        if (ya > yb)
            return 1;
        else if (yb > ya)
            return -1;
        return 0;
    }

    public void TogglePause()
    {
        InputManager.Paused = !InputManager.Paused;
    }

    public void Update(GameTime gameTime)
    {
        InputManager.Update();
        _camera.UpdateCamera(KatabasisGame.Viewport);
        UI.Update();

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(InputManager.MousePos, Matrix.Invert(_camera.Transform));

        // Only the camera and inputs should update when paused
        if (InputManager.Paused)
            return;

        Globals.Update(gameTime);

        // Check is a person was clicked in this frame
        Person clickedPerson = null;
        foreach (Person p in _player1.Kingdom.People)
        {
            if (p.CheckIfClicked())
            {
                clickedPerson = p;
                break;
            }
        }

        // If the player clicked off, return camera control, otherwise follow the player
        if (InputManager.Clicked && clickedPerson == null)
            _camera.Unfollow();
        else if (InputManager.Clicked)
            _camera.Follow(clickedPerson);

        _player1.Update();
        _map.Update();

        // Give each person a "daily" update for tasks that don't need to be constantly checked
        TimeOfDay += Globals.Time;
        if (TimeOfDay > SECONDS_PER_DAY)
        {
            TimeOfDay = 0f;
            foreach (Person p in _player1.Kingdom.People)
                p.DailyUpdate();
        }

        // Write the current world coordinate at the mouse position
        _coordinateDisplay.Text = $"({_coordinateDisplay.Position.X:0.0}, {_coordinateDisplay.Position.Y:0.0})";  
        _coordinateDisplay.Position = InputManager.ScreenMousePos + new Vector2(15f, 15f);
        
        // Write information about the currently selected person to the top left
        if (_camera.Following != null)
            _debugDisplay.Text = _camera.Following.ToString();
        else
            _debugDisplay.Text = "";

        _logoDisplay.Position.X = Globals.WindowSize.X - _logoDisplay.Width() - 30;
        _logoDisplay2.Position.X = _logoDisplay.Position.X + 5;
    }

    public void Draw()
    {
        // Draw background independent of transformations
        Globals.SpriteBatch.Begin();
        _sky.Draw();
        Globals.SpriteBatch.End();

        Globals.SpriteBatch.Begin(transformMatrix: _camera.Transform);
        
        // Tiles belong on the bottom
        _map.DrawTiles();

        Globals.Ybuffer.Sort(CompareDrawable);
        
        // Draw elements by their furthest Y coordinate
        foreach (Drawable d in Globals.Ybuffer)
            d.Draw();

        // Draw the UI on top
        _map.DrawUI();

        Globals.SpriteBatch.End();

        // Begin screen region draw batch (not affected by camera transforms)
        Globals.SpriteBatch.Begin();

        // Draw the current coordinates at the cursor location
        _coordinateDisplay.Draw();

        // Draw debug text
        _debugDisplay.Draw();

        // Draw logo and shadow
        _logoDisplay2.Draw();
        _logoDisplay.Draw();

        // Draw the user interface
        UI.Draw();

        Globals.SpriteBatch.End();
    }
}