using System;
using System.Collections.Generic;

public class MilitaryDisplay : VBox
{
    public PeopleDisplay SoldierTable;

    public MilitaryDisplay(CloseablePanel parent, SpriteTexture texture = null) : base(texture)
    {
        HBox buttons = new();
        buttons.SetMargin(top: 1, left: 1);

        Button conscript1 = new(Sprites.MenuButton, Sprites.MenuButtonHover, buttonText: "Conscript 1", onClick: Conscript1);
        Button conscript10 = new(Sprites.MenuButton, Sprites.MenuButtonHover, buttonText: "Conscript 10", onClick: Conscript10);
        buttons.Add(conscript1, conscript10);

        Button dismiss1 = new(Sprites.MenuButton, Sprites.MenuButtonHover, buttonText: "Dismiss 1", onClick: Dismiss1);
        Button dismiss10 = new(Sprites.MenuButton, Sprites.MenuButtonHover, buttonText: "Dismiss 10", onClick: Dismiss10);
        buttons.Add(dismiss1, dismiss10);

        Add(buttons);

        SoldierTable = new(parent);
        SoldierTable.ElementsPerPage = 12;

        // Remove profession (they're all soldiers)
        SoldierTable.RemoveColumnAt(2);
        SoldierTable.AddColumn("Fighting", GetFightingSkill);
        SoldierTable.AddColumn("Dismiss", GetDismissButton);
        Add(SoldierTable);
    }

    public UIElement GetFightingSkill(Person person)
    {
        string level = $"{person.Skills[(int)Skill.FIGHTING].level}";
        TextSprite fighting = new(Sprites.SmallFont, text: level);
        fighting.FontColor = PeopleDisplay.TextColor;
        return fighting;
    }

    public UIElement GetDismissButton(Person person)
    {
        Button dismiss = new(Sprites.MenuButton, Sprites.MenuButtonHover, buttonText: "Dismiss", onClick: DismissPerson);
        dismiss.SetScale(0.5f);
        dismiss.ButtonText.SetPadding(top: 3, left: 10);
        dismiss.ButtonElement.UserData = person;
        return dismiss;
    }

    public void DismissPerson(Object clicked)
    {
        UIElement button = (UIElement)clicked;
        Globals.Model.Player1.Kingdom.Army.Dismiss((Person)button.UserData);
    }

    public void Update(List<Person> soldiers)
    {
        SoldierTable.Update(soldiers);
        base.Update();
    }

    public void Conscript1(Object clicked) { Conscript(1); }
    public void Conscript10(Object clicked) { Conscript(10); }
    public void Conscript(int n)
    {
        Globals.Model.Player1.Kingdom.Army.ConscriptN(n);
    }

    public void Dismiss1(Object clicked) { Dismiss(1); }
    public void Dismiss10(Object clicked) { Dismiss(10); }
    public void Dismiss(int n)
    {
        Globals.Model.Player1.Kingdom.Army.DismissN(n);
    }
}