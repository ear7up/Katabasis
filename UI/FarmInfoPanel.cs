using System;

public class FarmInfoPanel : CloseablePanel
{
    public VBox MyLayout;
    public UIElement BuildingIcon;
    public TextSprite Description;
    public GridLayout CropGrid;
    public Building FarmBuilding;

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

        CropGrid = new();
        int rows = 4;
        int columns = 6;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                UIElement cropIcon = new(Sprites.CropIcon, onClick: SwitchCrop);
                cropIcon.UserData = (int)(y * rows + x);
                CropGrid.SetContent(x, y, cropIcon);
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

        if (FarmBuilding != null)
        {
            Farm farm = Globals.Model.FarmingingMgr.GetFarm(FarmBuilding);
            // TODO: farm.StartSowing( plantId -> plant type )
            // how to handle switching?
            // should farms start sowing after harvesting finishes?
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

        Farm farm = Globals.Model.FarmingingMgr.GetFarm(building);
        if (farm != null)
        {
            Description.Text += "\n" + farm.Describe();
            // TODO: Highlight icon currently growing using farm.PlantId
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