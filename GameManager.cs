using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace Katabasis;

public class GameManager
{
    private readonly Sprite _sky;
    private readonly Map _map;
    private readonly Camera _camera;
    private Player _player1;

    public const int SECONDS_PER_DAY = 60;
    private float TimeOfDay = 0;

    private TextSprite _coordinateDisplay;
    private static TextSprite _debugDisplay;
    private static TextSprite _logoDisplay;
    private static TextSprite _logoDisplay2;
    private static GridLayout _bottomLeftPanel;
    private static GridLayout _bottomPanel;
    
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
        _debugDisplay.Scale = 0.7f;

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

        UI.AddElement(new UIElement(Sprites.Clock, scale: 0.5f, onClick: TogglePause), 
            UI.Position.TOP_RIGHT);

        // Create the bottom left panel with a 2x3 grid of clickable buttons
        Action[] buttonActions = { BuildButton, TileButton, Button3, Button4, Button5, Button6 };
        _bottomLeftPanel = new(Sprites.BottomLeftPanel);
        _bottomLeftPanel.SetMargin(left: 49, top: 70);
        _bottomLeftPanel.SetPadding(right: -20, bottom: -170);

        UIElement buildElement = new(
            Sprites.BottomLeftButtons[0], 
            onClick: BuildButton, 
            onHover: UI.SetTooltipText);
        buildElement.TooltipText = "Build (B)";

        _bottomLeftPanel.SetContent(0, 0, buildElement);
        _bottomLeftPanel.SetContent(1, 0, new UIElement(Sprites.BottomLeftButtons[1], 
            onClick: TileButton, onHover: UI.SetTooltipText, tooltip: "Acquire Tile (T)"));
        _bottomLeftPanel.SetContent(2, 0, new UIElement(Sprites.BottomLeftButtons[2], onClick: Button3));
        _bottomLeftPanel.SetContent(0, 1, new UIElement(Sprites.BottomLeftButtons[3], onClick: Button4));
        _bottomLeftPanel.SetContent(1, 1, new UIElement(Sprites.BottomLeftButtons[4], onClick: Button5));
        _bottomLeftPanel.SetContent(2, 1, new UIElement(Sprites.BottomLeftButtons[5], onClick: Button6));

        UI.AddElement(_bottomLeftPanel, UI.Position.BOTTOM_LEFT);

        _bottomPanel = new(Sprites.BottomPanel);
        _bottomPanel.SetPadding(bottom: -250);
        _bottomPanel.SetMargin(left: 30, top: 60);
        _bottomPanel.SetContent(0, 0, new UIElement(Sprites.farms[0], scale: 0.3f, 
            onClick: BuildFarm, onHover: UI.SetTooltipText, tooltip: "Farm"));
        _bottomPanel.SetContent(1, 0, new UIElement(Sprites.mines[0], scale: 0.3f, 
            onClick: BuildMine, onHover: UI.SetTooltipText, tooltip: "Mine"));
        _bottomPanel.SetContent(2, 0, new UIElement(Sprites.ranches[0], scale: 0.3f, 
            onClick: BuildRanch, onHover: UI.SetTooltipText, tooltip: "Ranch"));
        _bottomPanel.SetContent(0, 1, new UIElement(Sprites.barracks[0], scale: 0.3f, 
            onClick: BuildBarracks, onHover: UI.SetTooltipText, tooltip: "Barracks"));
        _bottomPanel.SetContent(1, 1, new UIElement(Sprites.houses[0], scale: 0.3f, 
            onClick: BuildHouse, onHover: UI.SetTooltipText, tooltip: "House"));
        _bottomPanel.SetContent(2, 1, new UIElement(Sprites.granaries[0], scale: 0.3f, 
            onClick: BuildGranary, onHover: UI.SetTooltipText, tooltip: "Granary"));
        _bottomPanel.Hide();

        UI.AddElement(_bottomPanel, UI.Position.BOTTOM_LEFT);

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

    public void BuildFarm() { Build(BuildingType.FARM); }
    public void BuildMine() { Build(BuildingType.MINE); }
    public void BuildRanch() { Build(BuildingType.RANCH); }
    public void BuildBarracks() { Build(BuildingType.BARRACKS); }
    public void BuildHouse() { Build(BuildingType.STONE_HOUSE); }
    public void BuildGranary() { Build(BuildingType.GRANARY); }

    public void Build(BuildingType buildingType)
    {
        if (InputManager.Mode == InputManager.BUILD_MODE)
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
        }
        else
        {
            _map.CreateEditBuilding(buildingType);
            InputManager.SwitchToMode(InputManager.BUILD_MODE);
        }
    }

    public void BuildButton()
    {
        if (_bottomPanel.Hidden)
        {
            _bottomPanel.Unhide();
        }
        else
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            _map.ClearEditBuilding();
            _bottomPanel.Hide();
        }
    }

    public void TileButton()
    {
        if (InputManager.Mode == InputManager.TILE_MODE)
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
        }
        else
        {
            InputManager.SwitchToMode(InputManager.TILE_MODE);
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

    public void Button3()
    {
        Console.WriteLine("Button 3 pressed");
    }

    public void Button4()
    {
        Console.WriteLine("Button 4 pressed");
    }

    public void Button5()
    {
        Console.WriteLine("Button 5 pressed");
    }

    public void Button6()
    {
        Console.WriteLine("Button 6 pressed");
    }

    public void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);

        InputManager.Update();
        _camera.UpdateCamera(KatabasisGame.Viewport);
        UI.Update();
        
        if (InputManager.BPressed)
            BuildButton();

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(InputManager.MousePos, Matrix.Invert(_camera.Transform));

        // Write the current world coordinate at the mouse position
        _coordinateDisplay.Text = $"({_coordinateDisplay.Position.X:0.0}, {_coordinateDisplay.Position.Y:0.0})";  
        _coordinateDisplay.Position = InputManager.ScreenMousePos + new Vector2(15f, 15f);

        HandlePersonFollowing();

        // Anything after this return will be pauseable
        if (InputManager.Paused)
            return;

        _player1.Update();
        _map.Update();

        HandleTileAcquisition();

        // Give a "daily" update for tasks that don't need to be constantly checked
        TimeOfDay += Globals.Time;
        if (TimeOfDay > SECONDS_PER_DAY)
        {
            TimeOfDay = 0f;
            _map.DailyUpdate();
            _player1.DailyUpdate();
            foreach (Person p in _player1.Kingdom.People)
                p.DailyUpdate();
        }
    }

    public void HandlePersonFollowing()
    {
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

        // Write statistics to debug
        _debugDisplay.Text = 
            $"Public Wealth: {(int)_player1.Kingdom.PublicWealth()}\n" +
            $"Private Wealth: {(int)_player1.Kingdom.PrivateWealth()}\n";

        // Write information about the currently selected person to the top left
        if (_camera.Following != null)
            _debugDisplay.Text += _camera.Following.ToString();
        else
            _debugDisplay.Text += "";

        _logoDisplay.Position.X = Globals.WindowSize.X - _logoDisplay.Width() - 30;
        _logoDisplay2.Position.X = _logoDisplay.Position.X + 5;
    }

    public void HandleTileAcquisition()
    {
        if (InputManager.Clicked && _map.HighlightedTile != null)
        {
            if (_player1.Kingdom.TryToAcquireTile(_map.HighlightedTile))
            {
                _map.UnhighlightTile();
                InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            }
            else
            {
                _map.MakeHighlightTileRed();
            }
        }
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

        // Draw logo and shadow
        _logoDisplay2.Draw();
        _logoDisplay.Draw();

        // Draw the user interface
        UI.Draw();

        // Draw the current coordinates at the cursor location
        _coordinateDisplay.Draw();

        // Draw debug text
        _debugDisplay.Draw();

        Globals.SpriteBatch.End();
    }
}