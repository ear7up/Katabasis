using System;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Katabasis;

public class GameManager
{
    public const int SECONDS_PER_DAY = 60;

    public GameModel Model;

    private static BuildingPlacer _buildingPlacer;

    private readonly Sprite _sky;
    private TextSprite _coordinateDisplay;
    private DecorationManager _decorationManager;
    private static TextSprite _debugDisplay;
    private static TextSprite _logoDisplay;
    private static TextSprite _logoDisplay2;
    private static GridLayout _buttonPanel;
    private static CloseablePanel _bottomPanel;
    private static InventoryPanel inventoryPanel;
    private static StatsPanel _statsPanel;
    private static UIElement _clockHand;
    private static PersonPanel _personPanel;
    private static TileInfoPanel _tileInfoPanel;
    private static BuildingInfoPanel buildingInfoPanel;
    private static EscapeMenuPanel escMenuPanel;
    public static MarketPanel MarketPanel;

    public GameManager()
    {
        _buildingPlacer = new();

        _sky = Sprite.Create(Sprites.Sky, Vector2.Zero);
        _sky.SetScale(2f);

        _coordinateDisplay = new(Sprites.Font, hasDropShadow: true);
        _coordinateDisplay.ScaleDown(0.2f);

        _decorationManager = new();

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

        Sprite[] hoverButtons = new Sprite[8];
        for (int i = 0; i < hoverButtons.Length; i++)
            hoverButtons[i] = Sprite.Create(Sprites.BottomLeftButtonsHover[i], Vector2.Zero);

        UIElement buildElement = new(
            Sprites.BottomLeftButtons[0], 
            onClick: BuildButton, 
            onHover: UI.SetTooltipText,
            tooltip: "(B)uild",
            hoverImage: hoverButtons[0]);
        _buttonPanel.SetContent(0, 0, buildElement);

        _buttonPanel.SetContent(1, 0, new UIElement(
            Sprites.BottomLeftButtons[1], 
            onClick: TileButton, 
            onHover: UI.SetTooltipText, 
            tooltip: "Buy (T)ile",
            hoverImage: hoverButtons[1]));

        _buttonPanel.SetContent(2, 0, new UIElement(
            Sprites.BottomLeftButtons[2], 
            onClick: Button3));

        _buttonPanel.SetContent(0, 1, new UIElement(
            Sprites.BottomLeftButtons[3], 
            onClick: ToggleMarketPanel,
            onHover: UI.SetTooltipText,
            tooltip: "(M)arket",
            hoverImage: hoverButtons[3]));

        _buttonPanel.SetContent(1, 1, new UIElement(
            Sprites.BottomLeftButtons[4], 
            onHover: UI.SetTooltipText, 
            tooltip: "Statistics(X)", 
            onClick: ToggleStatistics,
            hoverImage: hoverButtons[4]));

        _buttonPanel.SetContent(2, 1, new UIElement(
            Sprites.BottomLeftButtons[5], 
            onHover: UI.SetTooltipText, 
            tooltip: "(I)nventory", 
            onClick: ToggleGoodsDisplay,
            hoverImage: hoverButtons[5]));

        UI.AddElement(_buttonPanel, UI.Position.BOTTOM_LEFT);

        _bottomPanel = new(Sprites.BottomPanel);
        _bottomPanel.Draggable = false;
        _bottomPanel.SetMargin(left: 30, top: 60);

        GridLayout bottomPanelGrid = new();
        bottomPanelGrid.SetContent(0, 0, new UIElement(Sprites.farms[0], scale: 0.3f, 
            onClick: BuildFarm, hoverElement: new BuildingPriceDisplay(null, BuildingType.FARM)));
        bottomPanelGrid.SetContent(1, 0, new UIElement(Sprites.mines[0], scale: 0.3f, 
            onClick: BuildMine, hoverElement: new BuildingPriceDisplay(null, BuildingType.MINE)));
        bottomPanelGrid.SetContent(2, 0, new UIElement(Sprites.ranches[0], scale: 0.3f, 
            onClick: BuildRanch, hoverElement: new BuildingPriceDisplay(null, BuildingType.RANCH)));
        bottomPanelGrid.SetContent(3, 0, new UIElement(Sprites.markets[0], scale: 0.3f, 
            onClick: BuildMarket, hoverElement: new BuildingPriceDisplay(null, BuildingType.MARKET)));
        bottomPanelGrid.SetContent(4, 0, new UIElement(Sprites.decorations[0], scale: 0.4f, 
            onClick: BuildDecoration, onHover: UI.SetTooltipText, tooltip: "Decoration"));

        bottomPanelGrid.SetContent(0, 1, new UIElement(Sprites.houses[0], scale: 0.3f, 
            onClick: BuildBrickHouse, hoverElement: new BuildingPriceDisplay(null, BuildingType.HOUSE, BuildingSubType.BRICK)));
        bottomPanelGrid.SetContent(1, 1, new UIElement(Sprites.houses[0], scale: 0.3f, 
            onClick: BuildWoodHouse, hoverElement: new BuildingPriceDisplay(null, BuildingType.HOUSE, BuildingSubType.WOOD)));
        bottomPanelGrid.SetContent(2, 1, new UIElement(Sprites.barracks[0], scale: 0.3f, 
            onClick: BuildBarracks, hoverElement: new BuildingPriceDisplay(null, BuildingType.BARRACKS)));
        bottomPanelGrid.SetContent(3, 1, new UIElement(Sprites.granaries[0], scale: 0.3f, 
            onClick: BuildGranary, hoverElement: new BuildingPriceDisplay(null, BuildingType.GRANARY)));
        bottomPanelGrid.SetContent(4, 1, new UIElement(Sprites.smithies[0], scale: 0.3f, 
            onClick: BuildSmithy, hoverElement: new BuildingPriceDisplay(null, BuildingType.SMITHY)));

        _bottomPanel.Add(bottomPanelGrid);

        _bottomPanel.SetDefaultPosition(new Vector2(
            _buttonPanel.Width(), Globals.WindowSize.Y - _bottomPanel.Height() + 250));

        _bottomPanel.Hide();

        _personPanel = new(null);
        _personPanel.Hide();

        _tileInfoPanel = new();
        buildingInfoPanel = new();
        escMenuPanel = new();
        escMenuPanel.Draggable = false;

        MarketPanel = new();
        MarketPanel.Hide();

        inventoryPanel = new();
        inventoryPanel.Hide();
        
        UIElement manButton = new UIElement(Sprites.ManS);
        manButton.ScaleDown(0.9f);
        manButton.AddSelectedImage(Sprites.ManG);
        manButton.SelectedImage.ScaleDown(0.9f);

        _statsPanel = new();
    }

    public void SetGameModel(GameModel gameModel)
    {
        Model = gameModel;
    }

    public void BuildFarm(Object clicked) { Build(BuildingType.FARM); }
    public void BuildMine(Object clicked) { Build(BuildingType.MINE); }
    public void BuildRanch(Object clicked) { Build(BuildingType.RANCH); }
    public void BuildMarket(Object clicked) { Build(BuildingType.MARKET); }

    public void BuildBarracks(Object clicked) { Build(BuildingType.BARRACKS); }
    public void BuildBrickHouse(Object clicked) { Build(BuildingType.HOUSE, BuildingSubType.BRICK); }
    public void BuildWoodHouse(Object clicked) { Build(BuildingType.HOUSE, BuildingSubType.WOOD); }
    public void BuildGranary(Object clicked) { Build(BuildingType.GRANARY); }
    public void BuildSmithy(Object clicked) { Build(BuildingType.SMITHY); }

    public void BuildDecoration(Object clicked)
    {
        _decorationManager.NewDecoration();
    }

    public void Build(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
    {
        if (InputManager.Mode == InputManager.BUILD_MODE)
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
        }
        else
        {
            _buildingPlacer.CreateEditBuilding(buildingType, subType);
            InputManager.SwitchToMode(InputManager.BUILD_MODE);
        }
    }

    public void BuildButton(Object clicked = null)
    {
        if (!_bottomPanel.Hidden)
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            _buildingPlacer.ClearEditBuilding();
        }
        TogglePanel(_bottomPanel);
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
        TasksTest.RunTests(Model.TileMap);
    }

    public static void SetPersonTracking(Person p)
    {
        _personPanel.SetPerson(p);
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

    public void ToggleStatistics(Object clicked = null)
    {
        TogglePanel(_statsPanel);
        _statsPanel.Update(Globals.Model.Player1.Kingdom.Statistics(), Globals.Model.Player1.Kingdom.People);
    }

    public void ToggleEscMenu()
    {
        InputManager.Paused = !InputManager.Paused;
        TogglePanel(escMenuPanel);
    }

    public void ToggleGoodsDisplay(Object clicked = null)
    {
        TogglePanel(inventoryPanel);
    }

    public static void ToggleMarketPanel(Object clicked = null)
    {
        TogglePanel(MarketPanel);
    }

    public static void TogglePanel(UIElement panel)
    {
        if (panel.Hidden)
        {
            panel.Unhide();
            panel.Position = new Vector2(panel.Position.X, Globals.WindowSize.Y);
            panel.SetAnimation(panel.DefaultPosition, 55f, 13f);
        }
        else
        {
            panel.DefaultPosition = panel.Position;
            Vector2 pos = new Vector2(panel.Position.X, Globals.WindowSize.Y);
            panel.SetAnimation(pos, 55f, 13f, panel.Hide);
        }
    }

    public void ResizeScreen()
    {
        CloseablePanel[] panels = { _bottomPanel, escMenuPanel, inventoryPanel, _statsPanel, MarketPanel };
        foreach (CloseablePanel panel in panels)
            if (panel.Hidden)
                panel.Position = new Vector2(panel.Position.X, Globals.WindowSize.Y);

        bool hidden = escMenuPanel.Hidden;
        escMenuPanel.Hidden = false;
        // Centered vertically and horizontally
        escMenuPanel.SetDefaultPosition(new Vector2(
            Globals.WindowSize.X / 2 - escMenuPanel.Width() / 2,
            Globals.WindowSize.Y / 2 - escMenuPanel.Height() / 2));
        escMenuPanel.Hidden = hidden;

        // Stick to the bottom, but cut off a bit of the extra
        hidden = _bottomPanel.Hidden;
        _bottomPanel.Hidden = false;
        _bottomPanel.SetDefaultPosition(new Vector2(
            _buttonPanel.Width(), Globals.WindowSize.Y - _bottomPanel.Height() + 250));
        _bottomPanel.Hidden = hidden;
    }

    public void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);

        if (InputManager.WasPressed(Keys.OemPlus))
        {
            UI.ScaleUp(0.05f);
            _buttonPanel.ScaleUp(0.05f);
        }
        else if (InputManager.WasPressed(Keys.OemMinus))
        {
            UI.ScaleDown(0.05f);
            _buttonPanel.ScaleDown(0.05f);
        }
        
        if (InputManager.WasPressed(Keys.B))
            BuildButton();

        if (InputManager.WasPressed(Keys.I))
            ToggleGoodsDisplay();

        if (InputManager.WasPressed(Keys.X))
            ToggleStatistics();

        if (InputManager.WasPressed(Keys.M))
            ToggleMarketPanel();

        if (InputManager.WasPressed(Keys.F))
            Config.ShowFog = !Config.ShowFog;

        escMenuPanel.Update();

        if (InputManager.WasPressed(Keys.Escape))
            ToggleEscMenu();

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(
            InputManager.MousePos, Matrix.Invert(Model.GameCamera.Transform));

        InputManager.WorldMousePos = Vector2.Transform(
            InputManager.MousePos, Matrix.Invert(Model.GameCamera.Transform));

        _decorationManager.Update(Model.TileMap);
        _buildingPlacer.Update();
        
        _bottomPanel.Update();
        _personPanel.Update();
        _statsPanel.Update();
        MarketPanel.Update();
        inventoryPanel.Update(Model.Player1.Kingdom.Treasury, Model.Player1.Kingdom.PrivateGoods());
        _tileInfoPanel.UpdateTileData(Model.TileMap.HighlightedTile);

        if (Building.SelectedBuilding != null && Building.SelectedBuilding.Type == BuildingType.MARKET)
            buildingInfoPanel.Update(null);
        else
            buildingInfoPanel.Update(Building.SelectedBuilding);

        // Update UI last (pop-up panels are on top, they should get clicks first)
        UI.Update();

        HandlePersonFollowing();

        // Give the other interfaces a chance to consume camera inputs, update this last
        Model.GameCamera.UpdateCamera(KatabasisGame.Viewport);

        // Anything after this return will be pauseable
        if (InputManager.Paused)
            return;

        // Calculate which tasks are the most profitable
        GoodsProduction.UpdateProfitability();

        Model.Player1.Update();
        Model.TileMap.Update();
        Model.Market.Update();
        Model.ConstructionQueue.Update();

        HandleTileAcquisition();

        // Give a "daily" update for tasks that don't need to be constantly checked
        Model.TimeOfDay += Globals.Time;
        if (Model.TimeOfDay > SECONDS_PER_DAY)
        {
            Model.TimeOfDay = 0f;
            Model.TileMap.DailyUpdate();
            Model.Player1.DailyUpdate();
            Model.Market.DailyUpdate();
            foreach (Person p in Model.Player1.Kingdom.People)
                p.DailyUpdate();
        }

        _clockHand.Image.Rotation = MathHelper.TwoPi * (Model.TimeOfDay / SECONDS_PER_DAY);
    }

    public void HandlePersonFollowing()
    {
        // Check is a person was clicked in this frame
        Person clickedPerson = null;
        foreach (Person p in Model.Player1.Kingdom.People)
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
            Model.GameCamera.Unfollow();
        }
        else if (clickedPerson != null)
        {
            SetPersonTracking(clickedPerson);
            Model.GameCamera.Follow(clickedPerson);
        }

        // Write statistics to debug
        _debugDisplay.Text = 
            $"Public Wealth: {(int)Model.Player1.Kingdom.PublicWealth()}\n" +
            $"Private Wealth: {(int)Model.Player1.Kingdom.PrivateWealth()}\n";

        _logoDisplay.Position = new Vector2(
            Globals.WindowSize.X - _logoDisplay.Width() - 30,
            _logoDisplay.Position.Y);

        _logoDisplay2.Position = new Vector2(
            _logoDisplay.Position.X + 5,
            _logoDisplay.Position.Y);
    }

    public void HandleTileAcquisition()
    {
        if (InputManager.UnconsumedClick() && Model.TileMap.HighlightedTile != null)
        {
            InputManager.ConsumeClick(this);

            if (Model.Player1.Kingdom.TryToAcquireTile(Model.TileMap.HighlightedTile))
            {
                Model.TileMap.UnhighlightTile();
                InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            }
            else
            {
                Model.TileMap.MakeHighlightTileRed();
            }
        }
    }

    public Tile.DisplayType GetTileDisplayType()
    {
        Building b = _buildingPlacer._editBuilding;

        Tile.DisplayType displayType = Tile.DisplayType.DEFAULT;
        if (b != null && b.Type == BuildingType.FARM)
            displayType = Tile.DisplayType.SOIL_QUALITY;
        else if (b != null && b.Type == BuildingType.MINE)
            displayType = Tile.DisplayType.MINERALS;
        else if (b != null && b.Type == BuildingType.RANCH)
            displayType = Tile.DisplayType.PLACING_RANCH;
        else if (InputManager.Mode == InputManager.TILE_MODE)
            displayType = Tile.DisplayType.BUYING_TILE;

        return displayType;
    }

    public void Draw()
    {
        // Draw background independent of transformations
        Globals.SpriteBatch.Begin();
        _sky.Draw();
        Globals.SpriteBatch.End();

        Globals.SpriteBatch.Begin(transformMatrix: Model.GameCamera.Transform);
        
        // Tiles belong on the bottom
        Model.TileMap.DrawTiles(GetTileDisplayType());

        Globals.Ybuffer.Sort(CompareDrawable);
        
        // Draw elements by their furthest Y coordinate
        foreach (Drawable d in Globals.Ybuffer)
            d.Draw();

        // Draw text on top
        foreach (Drawable d in Globals.TextBuffer)
            d.Draw();

        _decorationManager.Draw();

        // Draw the UI on top
        Model.TileMap.DrawUI();

        _buildingPlacer.DrawUI();

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

        // Draw the popup interfaces
        _bottomPanel.Draw(_bottomPanel.Position);

        inventoryPanel.Draw(inventoryPanel.Position);

        MarketPanel.Draw(MarketPanel.Position);

        _statsPanel.Draw(_statsPanel.Position);

        buildingInfoPanel.Draw(buildingInfoPanel.Position);
        _personPanel.Draw(_personPanel.Position);

        _tileInfoPanel.Draw(new Vector2(
            Globals.WindowSize.X - _tileInfoPanel.Width(), 260f));

        escMenuPanel.Draw(escMenuPanel.Position);

        // Draw the current coordinates at the cursor location
        _coordinateDisplay.Text = $"({InputManager.ScreenMousePos.X:0.0}, {InputManager.ScreenMousePos.Y:0.0})";
        _coordinateDisplay.Draw(InputManager.ScreenMousePos + new Vector2(15f, 15f));

        // Draw debug text
        _debugDisplay.Draw(new Vector2(30f, 30f));

        Globals.SpriteBatch.End();
    }
}