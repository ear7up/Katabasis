using System;
using Katabasis;

public class BuildingInfoPanel : CloseablePanel
{
    public VBox MyLayout;
    public HBox TopPart;
    public StockpileDisplay StockpileLayout;

    public UIElement BuildingIcon;
    public TextSprite Description;
    public VBox OccupantsLayout;

    public Sprite Tall;
    public Sprite Small;

    public BuildingInfoPanel() : base(Sprites.SmallPanel)
    {
        Small = Image;
        Tall = Sprite.Create(Sprites.TallPanel, Vector2.Zero);
        Tall.DrawRelativeToOrigin = false;

        MyLayout = new();
        MyLayout.SetMargin(top: 50, left: 40);

        TopPart = new();
        StockpileLayout = new("Inventory");

        BuildingIcon = new();

        Description = new(Sprites.Font);
        Description.ScaleDown(0.3f);

        TopPart.Add(BuildingIcon);
        TopPart.Add(Description);

        OccupantsLayout = new();

        MyLayout.Add(TopPart);
        MyLayout.Add(StockpileLayout);
        MyLayout.Add(OccupantsLayout);

        StockpileLayout.SetPadding(bottom: 20);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X - 2 * Width(), 50f));
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
            MyLayout.Elements[1] = StockpileLayout;
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
        if (MyLayout.Height() > Small.GetBounds().Height - 100)
            Image = Tall;
        else
            Image = Small;

        // Update layout before replacing elements so that they can register the fact that they've been clicked
        MyLayout.Update();

        OccupantsLayout.Elements = new();
        int i = 0;
        GridLayout grid = new();
        foreach (Person person in building.CurrentUsers)
        {
            HBox personLayout = new();

            UIElement personIcon = new(person.GetSpriteTexture(), 0.1f, onClick: JumpToPerson);
            personIcon.UserData = person;
            personLayout.Add(personIcon);

            TextSprite personName = new(Sprites.SmallFont, text: person.Name);
            personLayout.Add(personName);
            
            personLayout.SetPadding(left: 10);
            grid.SetContent(i % 2, i / 2, personLayout);

            i++;
        }

        OccupantsLayout.Add(grid);

        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
        MyLayout.Draw(offset);
    }

    public void JumpToPerson(Object clicked)
    {
        Person clickedPerson = (Person)((UIElement)clicked).UserData;
        GameManager.SetPersonTracking(clickedPerson);

        // Uncomment if you want this to close the current panel
        // ClosePanel(clicked);

        // Uncomment if you want the game camera to follow the person once clicked
        // Globals.Model.GameCamera.Follow(clickedPerson);
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