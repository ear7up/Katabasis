using System;
using System.Collections.Generic;

public class FarmInfoPanel : CloseablePanel
{
    public VBox MyLayout;
    public UIElement BuildingIcon;
    public TextSprite Description;
    public GridLayout CropGrid;
    public Building FarmBuilding;
    public Dictionary<int, UIElement> PlantButtons;

    public Sprite Tall;
    public Sprite Small;

    public FarmInfoPanel() : base(Sprites.SmallPanel)
    {
        MyLayout = new();
        MyLayout.SetMargin(top: 55, left: 40);
        MyLayout.OnClick = null;

        Small = Image;
        Tall = Sprite.Create(Sprites.TallPanel, Vector2.Zero);
        Tall.DrawRelativeToOrigin = false;

        BuildingIcon = new();
        Description = new(Sprites.Font);
        Description.ScaleDown(0.3f);

        HBox topPart = new();
        topPart.Add(BuildingIcon);
        topPart.Add(Description);

        // Make a list of crop ids
        List<int> cropIds = new();
        Goods g = new();
        g.Type = GoodsType.FOOD_PLANT;
        foreach (Goods.FoodPlant plant in Enum.GetValues(typeof(Goods.FoodPlant)))
        {
            // Skip wild edible plants (not a crop)
            if (plant == Goods.FoodPlant.WILD_EDIBLE)
                continue;
            g.SubType = (int)plant;
            cropIds.Add(g.GetId());
        }

        g.Type = GoodsType.MATERIAL_PLANT;
        g.SubType = (int)Goods.MaterialPlant.FLAX;
        cropIds.Add(g.GetId());

        // Map crop ids to selection buttons
        PlantButtons = new();

        // Build the grid, assign crop ids as UserData
        CropGrid = new();
        int i = 0;
        int rows = 4;
        int columns = 6;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                UIElement cropIcon = new(Sprites.CropIcon, onClick: SwitchCrop);
                if (i < cropIds.Count)
                {
                    cropIcon.UserData = cropIds[i];
                    PlantButtons[cropIds[i]] = cropIcon;
                }
                else
                {
                    cropIcon.UserData = 0;
                }
                CropGrid.SetContent(x, y, cropIcon);
                i++;
            }
        }

        MyLayout.Add(topPart);
        MyLayout.Add(CropGrid);
        Add(MyLayout);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X - 2 * Width(), 50f));
    }

    public void SwitchCrop(Object clicked)
    {
        int plantId = (int)((UIElement)clicked).UserData;

        if (FarmBuilding != null && plantId != 0)
        {
            Farm farm = Globals.Model.FarmingingMgr.GetFarm(FarmBuilding);
            farm?.StartSowing(plantId);
        }
    }

    public void Update(Building building)
    {
        if (building == null)
            Hide();
        else
            Unhide();

        FarmBuilding = building;

        if (Hidden)
            return;

        BuildingIcon.Image = Sprite.Create(building.GetSpriteTexture(), Vector2.Zero);
        BuildingIcon.Image.SetScale(0.3f);

        Description.Text = building.Describe();

        // Gray out unavailable options
        foreach (UIElement plantButton in PlantButtons.Values)
        {
            // Clear the color
            plantButton.Image.SpriteColor = Color.White;

            int id = (int)plantButton.UserData;
            if (id == 0 || !Globals.Model.Player1.IsPlantUnlocked(id))
                plantButton.Image.SpriteColor = Color.DarkGray;
        }

        // Highlight what the farm is currently growing
        Farm farm = Globals.Model.FarmingingMgr.GetFarm(building);
        if (farm != null)
        {
            Description.Text += "\n" + farm.Describe();

            if (PlantButtons.ContainsKey(farm.PlantId))
                PlantButtons[farm.PlantId].Image.SpriteColor = Color.Brown;
        }

        // Switch image when the stockpile gets bigger than the image (~40 pixels dead space at bottom of panels)
        if (MyLayout.Height() > Sprites.SmallPanel.Texture.Height - 100)
            Image = Tall;
        else
            Image = Small;

        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
    }

    public override void ClosePanel(Object clicked)
    {
        if (Building.SelectedBuilding != null)
        {
            Building.SelectedBuilding.Selected = false;
            Building.SelectedBuilding = null;
        }
        base.ClosePanel(clicked);
    }
}