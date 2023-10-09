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

    // Managers
    private static BuildingPlacer _buildingPlacer;
    private static DecorationManager _decorationManager;

    // Misc UI elements
    private readonly Sprite _sky;
    private TextSprite _coordinateDisplay;
    private static TextSprite _debugDisplay;
    private static TextSprite _logoDisplay;
    private static TextSprite _logoDisplay2;
    private static UIElement _clockHand;

    // Pop-up panels
    private static GridLayout _buttonPanel;
    private static BuildingPlacerPanel _bottomPanel;
    private static InventoryPanel inventoryPanel;
    private static StatsPanel _statsPanel;
    private static PersonPanel _personPanel;
    private static PeoplePanel _peoplePanel;
    private static TileInfoPanel _tileInfoPanel;
    private static BuildingInfoPanel buildingInfoPanel;
    private static EscapeMenuPanel escMenuPanel;
    private static FarmInfoPanel farmInfoPanel;
    private static SenetPanel senetPanel;
    public static MarketPanel MarketPanel;

    private static RightClickMenu _rightClickMenu;

    public GameManager()
    {
        _buildingPlacer = new();

        _sky = Sprite.Create(Sprites.Sky, Vector2.Zero);
        _sky.SetScale(2f);

        _coordinateDisplay = new(Sprites.Font, Color.White, Color.Black);
        _coordinateDisplay.ScaleDown(0.4f);

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
        _buttonPanel.SetPadding(bottom: -170);

        UIElement buildElement = new(
            Sprites.BottomLeftButtons[0], 
            onClick: BuildButton, 
            onHover: UI.SetTooltipText,
            tooltip: "(B)uild",
            hoverImage: Sprites.BottomLeftButtonsHover[0]);
        _buttonPanel.SetContent(0, 0, buildElement);

        _buttonPanel.SetContent(1, 0, new UIElement(
            Sprites.BottomLeftButtons[1], 
            onClick: TileButton, 
            onHover: UI.SetTooltipText, 
            tooltip: "Buy (T)ile",
            hoverImage: Sprites.BottomLeftButtonsHover[1]));

        _buttonPanel.SetContent(2, 0, new UIElement(
            Sprites.BottomLeftButtons[2],
            onHover: UI.SetTooltipText,
            tooltip: "(P)eople", 
            onClick: TogglePeoplePanel,
            hoverImage: Sprites.BottomLeftButtonsHover[2]));

        _buttonPanel.SetContent(0, 1, new UIElement(
            Sprites.BottomLeftButtons[3], 
            onClick: ToggleMarketPanel,
            onHover: UI.SetTooltipText,
            tooltip: "(M)arket",
            hoverImage: Sprites.BottomLeftButtonsHover[3]));

        _buttonPanel.SetContent(1, 1, new UIElement(
            Sprites.BottomLeftButtons[4], 
            onHover: UI.SetTooltipText, 
            tooltip: "Statistics(X)", 
            onClick: ToggleStatistics,
            hoverImage: Sprites.BottomLeftButtonsHover[4]));

        _buttonPanel.SetContent(2, 1, new UIElement(
            Sprites.BottomLeftButtons[5], 
            onHover: UI.SetTooltipText, 
            tooltip: "(I)nventory", 
            onClick: ToggleGoodsDisplay,
            hoverImage: Sprites.BottomLeftButtonsHover[5]));

        UI.AddElement(_buttonPanel, UI.Position.BOTTOM_LEFT);

        _bottomPanel = new(Sprites.BottomPanel);
        UI.AddElement(_bottomPanel, UI.Position.BOTTOM_LEFT);

        _personPanel = new(null);
        _personPanel.Hide();

        _peoplePanel = new();
        _peoplePanel.Hide();

        _tileInfoPanel = new();
        buildingInfoPanel = new();
        escMenuPanel = new();
        escMenuPanel.Draggable = false;
        farmInfoPanel = new();
        senetPanel = new();

        MarketPanel = new();
        MarketPanel.Hide();

        inventoryPanel = new();
        inventoryPanel.Hide();
        
        UIElement manButton = new UIElement(Sprites.ManS);
        manButton.ScaleDown(0.9f);
        manButton.AddSelectedImage(Sprites.ManG);
        manButton.SelectedImage.ScaleDown(0.9f);

        _statsPanel = new();

        // Temporary options
        _rightClickMenu = new(Sprites.RightClickMenu);
        _rightClickMenu.AddOption(new TextSprite(Sprites.SmallFont, Color.White, text: "Deploy Army"), DeployArmy);
        _rightClickMenu.AddOption(new TextSprite(Sprites.SmallFont, Color.White, text: "Undeploy"), CancelDeployment);
        _rightClickMenu.AddOption(new TextSprite(Sprites.SmallFont, Color.White, text: "Option 3"));
        _rightClickMenu.AddOption(new TextSprite(Sprites.SmallFont, Color.White, text: "Option 4"));

    }

    public void SetGameModel(GameModel gameModel)
    {
        Model = gameModel;
    }

    public void DeployArmy(Object clicked)
    {
        UIElement obj = (UIElement)clicked;
        RightClickMenu.ClickPosition worldPos = (RightClickMenu.ClickPosition)obj.UserData;

        Vector2 v = new(worldPos.X, worldPos.Y);
        Tile tile = Globals.Model.TileMap.TileAtPos(v);
        if (tile == null)
            return;

        Globals.Model.Player1.Kingdom.Army.Deploy(v);
    }

    public void CancelDeployment(Object clicked)
    {
        Globals.Model.Player1.Kingdom.Army.CancelDeployment();
    }

    public static void BuildFarm(Object clicked) { Build(BuildingType.FARM); }
    public static void BuildMine(Object clicked) { Build(BuildingType.MINE); }
    public static void BuildRanch(Object clicked) { Build(BuildingType.RANCH); }
    public static void BuildMarket(Object clicked) { Build(BuildingType.MARKET); }

    public static void BuildBarracks(Object clicked) { Build(BuildingType.BARRACKS); }
    public static void BuildBrickHouse(Object clicked) { Build(BuildingType.HOUSE, BuildingSubType.BRICK); }
    public static void BuildWoodHouse(Object clicked) { Build(BuildingType.HOUSE, BuildingSubType.WOOD); }
    public static void BuildGranary(Object clicked) { Build(BuildingType.GRANARY); }
    public static void BuildSmithy(Object clicked) { Build(BuildingType.SMITHY); }
    public static void BuildTemple(Object clicked) { Build(BuildingType.TEMPLE); }
    public static void BuildOven(Object clicked) { Build(BuildingType.OVEN); }
    public static void BuildPyramid(Object clicked) { Build(BuildingType.PYRAMID); }
    public static void BuildTannery(Object clicked) { Build(BuildingType.TANNERY); }
    public static void BuildTavern(Object clicked) { Build(BuildingType.TAVERN); }

    public static void BuildDecoration(Object clicked)
    {
        int i = (int)((UIElement)clicked).UserData;
        _decorationManager.NewDecoration(i);
    }

    public static void Build(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
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

    public static void StoneButtonPress(Object clicked)
    {
        if (clicked != null)
            SoundEffects.Play(SoundEffects.StoneButtonPress);
    }

    public void BuildButton(Object clicked = null)
    {
        StoneButtonPress(clicked);
        if (!_bottomPanel.Hidden)
        {
            InputManager.SwitchToMode(InputManager.CAMERA_MODE);
            _buildingPlacer.ClearEditBuilding();
        }
        TogglePanel(_bottomPanel);
    }

    public void TileButton(Object clicked)
    {
        StoneButtonPress(clicked);
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

    public void TogglePause(Object clicked = null)
    {
        StoneButtonPress(clicked);
        InputManager.Paused = !InputManager.Paused;
    }

    public void TogglePeoplePanel(Object clicked = null)
    {
        StoneButtonPress(clicked);
        TogglePanel(_peoplePanel);
    }

    public void ToggleStatistics(Object clicked = null)
    {
        StoneButtonPress(clicked);
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
        StoneButtonPress(clicked);
        TogglePanel(inventoryPanel);
    }

    public static void ToggleMarketPanel(Object clicked = null)
    {
        StoneButtonPress(clicked);
        SoundEffects.Play(SoundEffects.MoneySound);
        TogglePanel(MarketPanel);
    }

    public static void TogglePanel(CloseablePanel panel)
    {
        // Don't bug out by hiding the panel mid-animation
        if (panel.AnimationVelocity != Vector2.Zero)
            return;

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

        hidden = senetPanel.Hidden;
        senetPanel.Hidden = false;
        senetPanel.SetDefaultPosition(new Vector2(Globals.WindowSize.X / 2 - senetPanel.Width() / 2, 50f));
        senetPanel.Hidden = hidden;
    }

    public void HandleBuildingSelection()
    {
        if (Building.SelectedBuilding != null && InputManager.UnconsumedKeypress(Keys.Escape))
        {
            Building.SelectedBuilding = null;
            InputManager.ConsumeKeypress(Keys.Escape, this);
        }

        if (Building.SelectedBuilding == null)
        {
            buildingInfoPanel.Update(null);
            farmInfoPanel.Update(null);
            senetPanel.Update(null);
        }
        else if (
            Building.SelectedBuilding.Type == BuildingType.FARM ||
            Building.SelectedBuilding.Type == BuildingType.FARM_RIVER)
        {
            farmInfoPanel.Update(Building.SelectedBuilding);
        }
        else if (Building.SelectedBuilding.Type == BuildingType.TEMPLE)
        {
            senetPanel.Update(Building.SelectedBuilding);
        }
        else if (
            Building.SelectedBuilding.Type == BuildingType.MARKET || 
            Building.SelectedBuilding.Type == BuildingType.CITY)
        {
            buildingInfoPanel.Update(null);
        }
        else
        {
            buildingInfoPanel.Update(Building.SelectedBuilding);
        }
    }

    public void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);

        if (InputManager.UnconsumedKeypress(Keys.OemPlus))
        {
            UI.ScaleUp(0.05f);
            _buttonPanel.ScaleUp(0.05f);
        }
        else if (InputManager.UnconsumedKeypress(Keys.OemMinus))
        {
            UI.ScaleDown(0.05f);
            _buttonPanel.ScaleDown(0.05f);
        }
        
        if (InputManager.UnconsumedKeypress(Keys.B))
            BuildButton();

        if (InputManager.UnconsumedKeypress(Keys.I))
            ToggleGoodsDisplay();

        if (InputManager.UnconsumedKeypress(Keys.X))
            ToggleStatistics();

        if (InputManager.UnconsumedKeypress(Keys.M))
            ToggleMarketPanel();
        
        if (InputManager.UnconsumedKeypress(Keys.P))
            TogglePeoplePanel();

        if (InputManager.UnconsumedKeypress(Keys.F))
            Config.ShowFog = !Config.ShowFog;

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(
            InputManager.MousePos, Matrix.Invert(Model.GameCamera.Transform));

        InputManager.WorldMousePos = Vector2.Transform(
            InputManager.MousePos, Matrix.Invert(Model.GameCamera.Transform));

        _decorationManager.Update(Model.TileMap);
        _buildingPlacer.Update();
        
        _personPanel.Update();
        _peoplePanel.Update();
        _statsPanel.Update();
        MarketPanel.Update();
        inventoryPanel.Update();
        _tileInfoPanel.UpdateTileData(Model.TileMap.HighlightedTile);

        _rightClickMenu.Update();

        HandleBuildingSelection();

        // Last panel to update (other panels or right-cick menu may consume Escape keypress)
        escMenuPanel.Update();

        // Update UI last (pop-up panels are on top, they should get clicks first)
        UI.Update();

        HandlePersonFollowing();
        HandleDebugText();

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
        Model.FarmingingMgr.Update();

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
        if (!InputManager.UnconsumedClick())
            return;

        // Check is a person was clicked in this frame
        Person clickedPerson = null;
        foreach (Person p in Model.Player1.Kingdom.People)
        {
            // TODO: improve efficiency, maybe use TileAtPos to search only people in the clicked tile
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
    }

    public void HandleDebugText()
    {
        // Write statistics to debug
        _debugDisplay.Text = 
            $"Public Wealth: {(int)Model.Player1.Kingdom.PublicWealth()}\n"; /* +
            $"Private Wealth: {(int)Model.Player1.Kingdom.PrivateWealth()}\n"; */

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
        inventoryPanel.Draw(inventoryPanel.Position);
        MarketPanel.Draw(MarketPanel.Position);
        _statsPanel.Draw(_statsPanel.Position);
        _peoplePanel.Draw(_peoplePanel.Position);
        buildingInfoPanel.Draw(buildingInfoPanel.Position);
        farmInfoPanel.Draw(farmInfoPanel.Position);
        senetPanel.Draw(senetPanel.Position);
        _personPanel.Draw(_personPanel.Position);
        _tileInfoPanel.Draw(new Vector2(
            Globals.WindowSize.X - _tileInfoPanel.Width(), 260f));
        escMenuPanel.Draw(escMenuPanel.Position);

        _rightClickMenu.Draw(_rightClickMenu.Position);

        // Draw tooltips above everything
        UI.DrawTooltip();

        // Draw the current coordinates at the cursor location
        _coordinateDisplay.Text = $"({InputManager.WorldMousePos.X:0.0}, {InputManager.WorldMousePos.Y:0.0})";
        _coordinateDisplay.Draw(InputManager.ScreenMousePos + new Vector2(30f, 30f));

        // Draw debug text
        _debugDisplay.Draw(new Vector2(30f, 30f));

        Globals.SpriteBatch.End();
    }
}