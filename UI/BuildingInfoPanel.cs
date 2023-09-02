using System;

public class BuildingInfoPanel : CloseablePanel
{
    public VBox Layout;
    public HBox TopPart;
    public StockpileDisplay StockpileLayout;

    public UIElement BuildingIcon;
    public TextSprite Description;

    public Sprite Tall;
    public Sprite Small;

    public BuildingInfoPanel() : base(Sprites.SmallPanel)
    {
        Small = Image;
        Tall = Sprite.Create(Sprites.TallPanel, Vector2.Zero);
        Tall.DrawRelativeToOrigin = false;

        Layout = new();
        Layout.SetMargin(top: 50, left: 40);

        TopPart = new();
        StockpileLayout = new("Inventory");

        BuildingIcon = new();

        Description = new(Sprites.Font);
        Description.ScaleDown(0.3f);

        TopPart.Add(BuildingIcon);
        TopPart.Add(Description);

        Layout.Add(TopPart);
        Layout.Add(StockpileLayout);

        StockpileLayout.SetPadding(bottom: 20);

        Position = new Vector2(
            Globals.WindowSize.X - Width(), 50f);
    }

    public void Update(Building building)
    {
        if (building == null)
            Hide();
        else
            Unhide();

        if (Hidden)
            return;

        BuildingIcon.Image = Sprite.Create(building.GetSpriteTexture(), Vector2.Zero);
        BuildingIcon.Image.SetScale(0.3f);

        Description.Text = building.Describe();

        // Making a new one is easier than clearing out data from the old one when selecting a new stockpile
        if (StockpileLayout.StockpileRef != building.Stockpile)
        {
            StockpileLayout = new("Inventory");
            Layout.Elements[1] = StockpileLayout;
        }

        // Don't bother displaying or updating if the building stockpile is empty
        if (building.Stockpile.IsEmpty())
        {
            StockpileLayout.Hide();
        }
        else
        {
            StockpileLayout.Unhide();
            StockpileLayout.Update(building.Stockpile);
        }

        // Switch image when the stockpile gets bigger than the image (~40 pixels dead space at bottom of panels)
        if (Layout.Height() > Small.GetBounds().Height - 100)
            Image = Tall;
        else
            Image = Small;

        Layout.Update();
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
        Layout.Draw(offset);
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