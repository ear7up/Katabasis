using System;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Katabasis;

public class GameManager
{
    public const int SECONDS_PER_DAY = 60;

    // Serialized content
    public Map TileMap { get; set; }
    public Camera GameCamera { get; set; }
    public Player Player1 { get; set; }
    public Market Market { get; set; }
    public float TimeOfDay { get; set; }

    private readonly Sprite _sky;
    private TextSprite _coordinateDisplay;
    private static TextSprite _debugDisplay;
    private static TextSprite _logoDisplay;
    private static TextSprite _logoDisplay2;
    private static GridLayout _buttonPanel;
    private static GridLayout _bottomPanel;
    private static GridLayout _inventoryPanel;
    private static TabLayout _statsPanel;
    private static TextSprite _statsOverviewText;
    private static TextSprite _inventoryText1;
    private static TextSprite _inventoryText2;
    private static UIElement _clockHand;
    private static PersonPanel _personPanel;

    public static MarketPanel MarketPanel;

    public GameManager()
    {
        TimeOfDay = 0f;

        _sky = new Sprite(Globals.Content.Load<Texture2D>("sky"), Vector2.Zero);
        _sky.SetScale(2f);
        TileMap = new();
        TileMap.Generate();
        GameCamera = Camera.Create(KatabasisGame.Viewport, TileMap.Origin);

        Player1 = Player.Create(TileMap.GetOriginTile());
        Player1.Kingdom.Init();

        // Only one market will exist at any time
        Market = new();
        Market.SetAttributes(Player1.Kingdom);
        Globals.Market = Market;

        _coordinateDisplay = new(Sprites.Font, hasDropShadow: true);
        _coordinateDisplay.ScaleDown(0.2f);

        _debugDisplay = new(Sprites.Font);
        _debugDisplay.Position = new Vector2(30f, 30f);
        _debugDisplay.ScaleDown(0.3f);

        _logoDisplay = new(Sprites.Font2);
        _logoDisplay.Text = "Katabasis";
        _logoDisplay.FontColor = Color.White;
        
        _logoDisplay2 = new(Sprites.Font2);
        _logoDisplay2.Text = "Katabasis";
        _logoDisplay2.FontColor = Color.Black;

        UI.Init();

        _clockHand = new UIElement(Sprites.ClockHand, scale: 0.5f);
        _clockHand.Image.DrawRelativeToOrigin = true;

        OverlapLayout clockLayout = new();
        clockLayout.Add(new UIElement(Sprites.Clock, scale: 0.5f, onClick: TogglePause));
        clockLayout.Add(_clockHand);
        UI.AddElement(clockLayout, UI.Position.TOP_RIGHT);

        // Create the bottom left panel with a 2x3 grid of clickable buttons
        _buttonPanel = new(Sprites.BottomLeftPanel);
        _buttonPanel.SetMargin(left: 49, top: 70);
        _buttonPanel.SetPadding(right: -20, bottom: -170);

        UIElement buildElement = new(
            Sprites.BottomLeftButtons[0], 
            onClick: BuildButton, 
            onHover: UI.SetTooltipText);
        buildElement.TooltipText = "(B)uild";

        _buttonPanel.SetContent(0, 0, buildElement);
        _buttonPanel.SetContent(1, 0, new UIElement(Sprites.BottomLeftButtons[1], 
            onClick: TileButton, onHover: UI.SetTooltipText, tooltip: "Buy (T)ile"));
        _buttonPanel.SetContent(2, 0, new UIElement(Sprites.BottomLeftButtons[2], onClick: Button3));
        _buttonPanel.SetContent(0, 1, new UIElement(Sprites.BottomLeftButtons[3], onClick: Button4));
        _buttonPanel.SetContent(1, 1, new UIElement(Sprites.BottomLeftButtons[4], 
            onHover: UI.SetTooltipText, tooltip: "Statistics(X)", onClick: ToggleStatistics));
        _buttonPanel.SetContent(2, 1, new UIElement(Sprites.BottomLeftButtons[5], 
            onHover: UI.SetTooltipText, tooltip: "(I)nventory", onClick: ToggleGoodsDisplay));

        UI.AddElement(_buttonPanel, UI.Position.BOTTOM_LEFT);

        _bottomPanel = new(Sprites.BottomPanel);
        _bottomPanel.SetPadding(bottom: -250);
        _bottomPanel.SetMargin(left: 30, top: 60);
        _bottomPanel.SetContent(0, 0, new UIElement(Sprites.farms[0], scale: 0.3f, 
            onClick: BuildFarm, onHover: UI.SetTooltipText, tooltip: "Farm"));
        _bottomPanel.SetContent(1, 0, new UIElement(Sprites.mines[0], scale: 0.3f, 
            onClick: BuildMine, onHover: UI.SetTooltipText, tooltip: "Mine"));
        _bottomPanel.SetContent(2, 0, new UIElement(Sprites.ranches[0], scale: 0.3f, 
            onClick: BuildRanch, onHover: UI.SetTooltipText, tooltip: "Ranch"));
        _bottomPanel.SetContent(3, 0, new UIElement(Sprites.markets[0], scale: 0.3f, 
            onClick: BuildMarket, onHover: UI.SetTooltipText, tooltip: "Market"));

        _bottomPanel.SetContent(0, 1, new UIElement(Sprites.houses[0], scale: 0.3f, 
            onClick: BuildHouse, onHover: UI.SetTooltipText, tooltip: "House"));
        _bottomPanel.SetContent(1, 1, new UIElement(Sprites.barracks[0], scale: 0.3f, 
            onClick: BuildBarracks, onHover: UI.SetTooltipText, tooltip: "Barracks"));
        _bottomPanel.SetContent(2, 1, new UIElement(Sprites.granaries[0], scale: 0.3f, 
            onClick: BuildGranary, onHover: UI.SetTooltipText, tooltip: "Granary"));
        _bottomPanel.SetContent(3, 1, new UIElement(Sprites.smithies[0], scale: 0.3f, 
            onClick: BuildSmithy, onHover: UI.SetTooltipText, tooltip: "Smithy"));
        _bottomPanel.Hide();

        _personPanel = new(null);
        _personPanel.Hide();

        MarketPanel = new();
        MarketPanel.Hide();

        UI.AddElement(_bottomPanel, UI.Position.BOTTOM_LEFT);

        _inventoryPanel = new(Sprites.TallPanel);
        _inventoryPanel.SetMargin(top: 80, left: 40);
        _inventoryText1 = new(Sprites.Font);
        _inventoryText1.ScaleDown(0.4f);
        _inventoryText2 = new(Sprites.Font);
        _inventoryText2.ScaleDown(0.4f);
        _inventoryPanel.SetContent(0, 0, _inventoryText1);
        _inventoryPanel.SetContent(1, 0, _inventoryText2);
        _inventoryPanel.Hide();

        _statsOverviewText = new TextSprite(Sprites.Font);
        _statsOverviewText.ScaleDown(0.4f);
        
        UIElement manButton = new UIElement(Sprites.ManS);
        manButton.ScaleDown(0.9f);
        manButton.AddSelectedImage(Sprites.ManG);
        manButton.SelectedImage.ScaleDown(0.9f);

        _statsPanel = new();
        _statsPanel.Image = new Sprite(Sprites.TallPanel, Vector2.Zero);
        _statsPanel.Image.DrawRelativeToOrigin = false;
        _statsPanel.SetMargin(top: 50, left: 30);
        _statsPanel.AddTab("Overview", manButton, _statsOverviewText);
        _statsPanel.Hide();
    }

    // Called when creating a new game, adds people to the world
    public void InitNew()
    {
        const int NUM_PEOPLE = 100;
        for (int i = 0 ; i < NUM_PEOPLE; i++)
        {
            Person person = Person.CreatePerson(TileMap.Origin, TileMap.GetOriginTile());
            person.Money = Globals.Rand.Next(20, 50);
            Player1.Kingdom.AddPerson(person);
        }
    }

    // If loading from a save file, set static variables and calculate unserialized content
    public void InitLoaded()
    {
        Globals.Market = Market;
        TileMap.ComputeNeighbors();
    }

    public void BuildFarm(Object clicked) { Build(BuildingType.FARM); }
    public void BuildMine(Object clicked) { Build(BuildingType.MINE); }
    public void BuildRanch(Object clicked) { Build(BuildingType.RANCH); }
    public void BuildMarket(Object clicked) { Build(BuildingType.MARKET); }

    public void BuildBarracks(Object clicked) { Build(BuildingType.BARRACKS); }
    public void BuildHouse(Object clicked) { Build(BuildingType.HOUSE); }
    public void BuildGranary(Object clicked) { Build(BuildingType.GRANARY); }
    public void BuildSmithy(Object clicked) { Build(BuildingType.SMITHY); }

    public void Build(BuildingType buildingType)
    {
        if (InputManager.Mode == InputManager.BUILD_MODE)
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
        }
        else
        {
            TileMap.CreateEditBuilding(buildingType);
            InputManager.SwitchToMode(InputManager.BUILD_MODE);
        }
    }

    public void BuildButton(Object clicked)
    {
        if (_bottomPanel.Hidden)
        {
            _bottomPanel.Unhide();
        }
        else
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            TileMap.ClearEditBuilding();
            _bottomPanel.Hide();
        }
    }

    public void TileButton(Object clicked)
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
        TasksTest.RunTests(TileMap);
    }

    public void SetPersonTracking(Person p)
    {
        _personPanel.SetPerson(p);
        if (p == null)
            _personPanel.Hide();
        else
            _personPanel.Unhide();
    }

    public static void ToggleMarketPanel()
    {
        if (MarketPanel.Hidden)
            MarketPanel.Unhide();
        else
            MarketPanel.Hide();
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

    public void TogglePause(Object clicked)
    {
        InputManager.Paused = !InputManager.Paused;
    }

    public void Button3(Object clicked)
    {
        Console.WriteLine("Button 3 pressed");
    }

    public void Button4(Object clicked)
    {
        Console.WriteLine("Button 4 pressed");
    }

    public void ToggleStatistics(Object clicked)
    {
        _statsOverviewText.Text = Player1.Kingdom.Statistics();
        if (_statsPanel.Hidden)
            _statsPanel.Unhide();
        else
            _statsPanel.Hide();
    }

    public void ToggleGoodsDisplay(Object clicked)
    {
        if (_inventoryPanel.Hidden)
            _inventoryPanel.Unhide();
        else
            _inventoryPanel.Hide();
    }

    public void HandleInventoryDisplay()
    {
        string goods = Player1.Kingdom.PrivateGoods();
        string[] lines = goods.Split('\n');
        
        // Currently supports showing up to 72 goods
        string goods1 = "";
        string goods2 = "";
        for (int i = 0; i < lines.Length; i++)
            if (i < 35)
                goods1 += lines[i] + "\n";
            else if (i < 73)
                goods2 += lines[i] + "\n";
        _inventoryText1.Text = goods1;
        _inventoryText2.Text = goods2;
    }

    public void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);

        InputManager.Update();
        GameCamera.UpdateCamera(KatabasisGame.Viewport);

        if (InputManager.PlusPressed)
        {
            UI.ScaleUp(0.05f);
            _buttonPanel.ScaleUp(0.05f);
        }
        else if (InputManager.MinusPressed)
        {
            UI.ScaleDown(0.05f);
            _buttonPanel.ScaleDown(0.05f);
        }
        
        if (InputManager.BPressed)
            BuildButton(null);

        if (InputManager.IPressed)
            ToggleGoodsDisplay(null);

        if (InputManager.XPressed)
            ToggleStatistics(null);

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(InputManager.MousePos, Matrix.Invert(GameCamera.Transform));

        HandlePersonFollowing();
        _personPanel.Update();
        MarketPanel.Update();

        // Update UI last (pop-up panels are on top, they should get clicks first)
        UI.Update();

        // Anything after this return will be pauseable
        if (InputManager.Paused)
            return;

        Player1.Update();
        TileMap.Update();
        Market.Update();

        // TODO: Write code to support click and drag on UIElements
        _inventoryPanel.Update();
        HandleInventoryDisplay();

        HandleTileAcquisition();

        // Give a "daily" update for tasks that don't need to be constantly checked
        TimeOfDay += Globals.Time;
        if (TimeOfDay > SECONDS_PER_DAY)
        {
            TimeOfDay = 0f;
            TileMap.DailyUpdate();
            Player1.DailyUpdate();
            foreach (Person p in Player1.Kingdom.People)
                p.DailyUpdate();
        }

        _clockHand.Image.Rotation = MathHelper.TwoPi * (TimeOfDay / SECONDS_PER_DAY);
    }

    public void HandlePersonFollowing()
    {
        // Check is a person was clicked in this frame
        Person clickedPerson = null;
        foreach (Person p in Player1.Kingdom.People)
        {
            if (p.CheckIfClicked())
            {
                clickedPerson = p;
                break;
            }
        }

        // If the player clicked off, return camera control, otherwise follow the player
        if (InputManager.UnconsumedClick() && clickedPerson == null)
        {
            SetPersonTracking(null);
            GameCamera.Unfollow();
        }
        else if (clickedPerson != null)
        {
            SetPersonTracking(clickedPerson);
            GameCamera.Follow(clickedPerson);
        }

        // Write statistics to debug
        _debugDisplay.Text = 
            $"Public Wealth: {(int)Player1.Kingdom.PublicWealth()}\n" +
            $"Private Wealth: {(int)Player1.Kingdom.PrivateWealth()}\n";

        // Write information about the currently selected person to the top left
        /*
        if (_camera.Following != null)
            _debugDisplay.Text += _camera.Following.ToString();
        else
            _debugDisplay.Text += "";
        */

        _logoDisplay.Position = new Vector2(
            Globals.WindowSize.X - _logoDisplay.Width() - 30,
            _logoDisplay.Position.Y);

        _logoDisplay2.Position = new Vector2(
            _logoDisplay.Position.X + 5,
            _logoDisplay.Position.Y);
    }

    public void HandleTileAcquisition()
    {
        if (InputManager.UnconsumedClick() && TileMap.HighlightedTile != null)
        {
            InputManager.ConsumeClick(this);

            if (Player1.Kingdom.TryToAcquireTile(TileMap.HighlightedTile))
            {
                TileMap.UnhighlightTile();
                InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            }
            else
            {
                TileMap.MakeHighlightTileRed();
            }
        }
    }

    public void Draw()
    {
        // Draw background independent of transformations
        Globals.SpriteBatch.Begin();
        _sky.Draw();
        Globals.SpriteBatch.End();

        Globals.SpriteBatch.Begin(transformMatrix: GameCamera.Transform);
        
        // Tiles belong on the bottom
        TileMap.DrawTiles();

        Globals.Ybuffer.Sort(CompareDrawable);
        
        // Draw elements by their furthest Y coordinate
        foreach (Drawable d in Globals.Ybuffer)
            d.Draw();

        // Draw text on top
        foreach (Drawable d in Globals.TextBuffer)
            d.Draw();

        // Draw the UI on top
        TileMap.DrawUI();

        Globals.SpriteBatch.End();

        // Begin screen region draw batch (not affected by camera transforms)
        Globals.SpriteBatch.Begin();

        // Draw logo and shadow
        Vector2 logoOffset = new Vector2(
            Globals.WindowSize.X - _logoDisplay.Width() - 30,
            Globals.WindowSize.Y - _logoDisplay.Height() - 30); 
        _logoDisplay2.Draw(logoOffset + new Vector2(5f, 5f));
        _logoDisplay.Draw(logoOffset);

        // Draw the user interface
        UI.Draw();

        // Draw the popup interface
        _inventoryPanel.Draw(new Vector2(
            Globals.WindowSize.X / 2 - _inventoryPanel.Width() / 2, 50f));

        _statsPanel.Draw(new Vector2(
            Globals.WindowSize.X / 2 - _statsPanel.Width() / 2, 50f));

        _personPanel.Draw(new Vector2(
            Globals.WindowSize.X - _personPanel.Width(), 50f));

        MarketPanel.Draw(new Vector2(
            Globals.WindowSize.X - MarketPanel.Width(), 50f));

        // Draw the current coordinates at the cursor location
        _coordinateDisplay.Text = $"({InputManager.ScreenMousePos.X:0.0}, {InputManager.ScreenMousePos.Y:0.0})";
        _coordinateDisplay.Draw(InputManager.ScreenMousePos + new Vector2(15f, 15f));

        // Draw debug text
        _debugDisplay.Draw(new Vector2(30f, 30f));

        Globals.SpriteBatch.End();
    }
}