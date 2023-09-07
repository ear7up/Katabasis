using System;
using System.Collections.Generic;

public class PeoplePanel : CloseablePanel
{
    public TabLayout MyLayout;
    public PeopleDisplay PeopleView;
    public MilitaryDisplay MilitaryView;

    public PeoplePanel() : base(Sprites.BigPanel)
    {
        SetMargin(top: 50, left: 40);

        PeopleView = new(parent: this);
        MilitaryView = new(parent: this);

        MyLayout = new();

        UIElement tab1 = new(Sprites.TabUnselected);
        tab1.SelectedImage = Sprite.Create(Sprites.TabSelected, Vector2.Zero);
        tab1.SelectedImage.DrawRelativeToOrigin = false;

        UIElement tab2 = new(Sprites.TabUnselected);
        tab2.SelectedImage = Sprite.Create(Sprites.TabSelected, Vector2.Zero);
        tab2.SelectedImage.DrawRelativeToOrigin = false;

        MyLayout.AddTab("People Table", tab1, PeopleView);
        MyLayout.AddTab("Army Table", tab2, MilitaryView);

        Add(MyLayout);
    
        SetDefaultPosition(new Vector2(Globals.WindowSize.X - Width(), 50f));
    }

    public override void Update()
    {
        if (Hidden)
            return;

        PeopleView.Update(Globals.Model.Player1.Kingdom.People);
        MilitaryView.Update(Globals.Model.Player1.Kingdom.Army.Soldiers);
        base.Update();
    }
}