using System;
using System.Collections;
using System.Collections.Generic;

public class StatsPanel : CloseablePanel
{
    public TabLayout Layout;
    public VBox OverviewLayout;
    public TextSprite OverviewText;
    public VBox DemographicsLayout;
    public GridLayout DemographicsTable;

    public StatsPanel() : base(Sprites.SmallPanel)
    {
        // Tab layout with two tabs: Overview and Demographics
        Hidden = true;
        Layout = new();
        Layout.TabBox.Image = Sprite.Create(Sprites.TabBackground, Vector2.Zero);
        Layout.TabBox.Image.DrawRelativeToOrigin = false;
        Layout.SetMargin(top: 30, left: 35);

        // Overview will contain a SpriteText label followed by a SpriteText description
        OverviewLayout = new();
        OverviewLayout.SetMargin(top: 1, left: 1);
        OverviewLayout.Add(new TextSprite(Sprites.Font, text: "Overview"));

        OverviewText = new TextSprite(Sprites.Font);
        OverviewText.ScaleDown(0.45f);
        OverviewLayout.Add(OverviewText);

        // Demographics will contain a SpriteText label followed by a GridLayout containing
        // 10 rows for ages 0-10, 11-20, etc. with the number of females in col 0, males in col 1
        DemographicsLayout = new();
        DemographicsLayout.SetMargin(top: 1, left: 1);
        DemographicsLayout.Add(new TextSprite(Sprites.Font, text: "Demographics"));

        DemographicsTable = new();
        for (int y = 0; y < 10; y++)
        {
            TextSprite ageGroupLabel = new(Sprites.Font, text: $"{10 * y}-{10 * y + 9}");
            ageGroupLabel.ScaleDown(0.45f);

            HBox fLayout = new();
            UIElement female = new(Sprites.VerticalBar);
            female.Image.SpriteColor = Color.Pink;
            fLayout.Add(female);

            HBox mLayout = new();
            UIElement male = new(Sprites.VerticalBar);
            male.Image.SpriteColor = Color.LightBlue;
            mLayout.Add(male);

            DemographicsTable.SetContent(0, y, ageGroupLabel);
            DemographicsTable.SetContent(1, y, mLayout);
            DemographicsTable.SetContent(2, y, fLayout);
        }
        DemographicsLayout.Add(DemographicsTable);

        // Tab layout will have two buttons which will determine which of the two layouts to show
        UIElement tab1 = new UIElement(Sprites.TabUnselected);
        tab1.AddSelectedImage(Sprites.TabSelected);
        UIElement tab2 = new UIElement(Sprites.TabUnselected);
        tab2.AddSelectedImage(Sprites.TabSelected);

        Layout.AddTab("Overview", tab1, OverviewLayout);
        Layout.AddTab("Demographics", tab2, DemographicsLayout);

        Position = new Vector2(
            Globals.WindowSize.X / 2 - Width() / 2, 50f);
    }

    public void Update(string overview, List<Person> people)
    {
        if (Hidden)
            return;

        OverviewText.Text = overview;

        // Reset scale for items on each row
        for (int y = 0; y < 10; y++)
        {
            ((HBox)DemographicsTable.GridContent[y][1]).Elements[0].Image.Scale = Vector2.One;
            ((HBox)DemographicsTable.GridContent[y][2]).Elements[0].Image.Scale = Vector2.One;
        }

        // Adjust scale for each age group based on the number of men/women in it
        foreach (Person person in people)
        {
            int index = (int)Math.Min(person.Age / 10, 9);
            ((HBox)DemographicsTable.GridContent[index][(int)person.Gender + 1]).Elements[0].Image.Scale += new Vector2(1f, 0f);
        }

        const int ROW_WIDTH = 200;

        // Set margins on the left column to make the bars grow from the center
        // Update scale to 
        for (int y = 0; y < 10; y++)
        {
            HBox mLayout = (HBox)DemographicsTable.GridContent[y][1];
            UIElement male = mLayout.Elements[0];
            male.Image.SetScaleX(ROW_WIDTH * 2 * male.Image.Scale.X / people.Count);
            mLayout.SetMargin(left: ROW_WIDTH - (int)male.Image.Scale.X);
            mLayout.SetPadding(left: ROW_WIDTH - (int)male.Image.Scale.X);

            HBox fLayout = (HBox)DemographicsTable.GridContent[y][2];
            UIElement female = fLayout.Elements[0];
            female.Image.SetScaleX(ROW_WIDTH * 2 * female.Image.Scale.X / people.Count);
        }
        
        Layout.Update();
        base.Update();
    }

    public override void Update()
    {
        if (Hidden)
            return;

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
}