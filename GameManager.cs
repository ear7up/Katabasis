using System;
using System.Collections.Generic;

namespace Katabasis;

public class GameManager
{
    private readonly Sprite _sky;
    private readonly Map _map;
    private readonly Camera _camera;
    private List<Person> _people;

    public const int SECONDS_PER_DAY = 60;
    private float TimeOfDay = 0;

    public TextSprite _coordinateDisplay;
    public static TextSprite _debugDisplay;
    
    public bool TEST = false;

    public GameManager()
    {
        _sky = new Sprite(Globals.Content.Load<Texture2D>("sky"), Vector2.Zero);
        _sky.Scale = 2f;
        _map = new();
        _camera = new(KatabasisGame.Viewport, _map.Origin);
        //_camera.SetBounds(_map.MapSize, _map.TileSize);
        _people = new();
        _coordinateDisplay = new(Sprites.Font);
        _debugDisplay = new(Sprites.Font);
        _debugDisplay.Position = Vector2.Zero;

        Goods.CalcGoodsTypecounts();
        GoodsProduction.Init();
        GoodsInfo.Init();

        if (TEST)
        {
            RunTests();
        }
        else
        {
            const int NUM_PEOPLE = 100;
            for (int i = 0 ; i < NUM_PEOPLE; i++)
                _people.Add(Person.CreatePerson(_map.Origin, _map.GetOriginTile()));
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

    public void Update(GameTime gameTime)
    {
        InputManager.Update();
        _camera.UpdateCamera(KatabasisGame.Viewport);

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(InputManager.MousePos, Matrix.Invert(_camera.Transform));

        // Only the camera and inputs should update when paused
        if (InputManager.Paused)
            return;

        Globals.Update(gameTime);

        Person clickedPerson = null;
        foreach (Person p in _people)
        {
            if (p.CheckIfClicked())
            {
                clickedPerson = p;
                break;
            }
        }

        if (InputManager.Clicked && clickedPerson == null)
            _camera.Unfollow();
        else if (InputManager.Clicked)
            _camera.Follow(clickedPerson);

        foreach (Person p in _people)
            p.Update();

        _map.Update();

        // Give each person a "daily" update for tasks that don't need to be constantly checked
        TimeOfDay += Globals.Time;
        if (TimeOfDay > SECONDS_PER_DAY)
        {
            TimeOfDay = 0f;
            foreach (Person p in _people)
                p.DailyUpdate();
        }

        // Write the current world coordinate at the mouse position
        _coordinateDisplay.Text = $"({_coordinateDisplay.Position.X:0.00}, {_coordinateDisplay.Position.Y:0.00})";  
        _coordinateDisplay.Position = InputManager.MousePos;
        
        // Write information about the currently selected person to the top left
        if (_camera.Following != null)
            _debugDisplay.Text = _camera.Following.ToString();
        else
            _debugDisplay.Text = "";
        _debugDisplay.Position.X = _camera.VisibleArea.X + 15;
        _debugDisplay.Position.Y = _camera.VisibleArea.Y + 15;
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

        // Draw the current coordinates at the cursor location
        _coordinateDisplay.Draw();
        _debugDisplay.Draw();

        Globals.SpriteBatch.End();

        // Draw the UI on top of the map, do not apply transformations to it
    }
}