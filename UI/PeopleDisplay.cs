using System;
using System.Collections.Generic;
using Katabasis;
using ProfessionExtension;

public class PeopleDisplay : GridLayout
{
    public static Color TextColor = Color.SaddleBrown;

    public CloseablePanel Parent;
    public List<UIElement> ColumnHeaders;

    public PeopleDisplay(CloseablePanel parent, SpriteTexture texture = null) : base(texture)
    {
        HasHeader = true;
        ColumnHeaders = new();
        ElementsPerPage = 10;
        
        TextSprite icon = new(Sprites.Font, TextColor, text: "Icon");
        ColumnHeaders.Add(icon);

        TextSprite name = new(Sprites.Font, TextColor, text: "Name");
        name.OnClick = SortByName;
        ColumnHeaders.Add(name);

        TextSprite profession = new(Sprites.Font, TextColor, text: "Profession");
        profession.OnClick = SortByProfession;
        ColumnHeaders.Add(profession);

        TextSprite hunger = new(Sprites.Font, TextColor, text: "Hunger");
        hunger.OnClick = SortByHunger;
        ColumnHeaders.Add(hunger);

        TextSprite task = new(Sprites.Font, TextColor, text: "Task");
        ColumnHeaders.Add(task);

        foreach (TextSprite textSprite in ColumnHeaders)
            textSprite.SetPadding(right: 15, bottom: 15);

        Parent = parent;
    }

    public void Update(List<Person> people)
    {
        base.Update();

        Clear();

        int row = 0;
        int col = 0;
        foreach (TextSprite columnHeader in ColumnHeaders)
            SetContent(col++, row, columnHeader);

        row = 1;
        foreach(Person person in people)
        {
            col = 0;
            UIElement icon = new(person.GetSpriteTexture(), 0.1f, onClick: JumpToPerson);
            icon.UserData = person;

            SetContent(col++, row, icon);
            SetContent(col++, row, new TextSprite(Sprites.SmallFont, TextColor, text: person.Name));
            SetContent(col++, row, new TextSprite(Sprites.SmallFont, TextColor, text: person.Profession.Describe()));
            SetContent(col++, row, new TextSprite(Sprites.SmallFont, TextColor, text: person.Hunger.ToString()));
            SetContent(col++, row, new TextSprite(Sprites.SmallFont, TextColor, text: person.DescribeCurrentTask()));
            row++;
        }
    }

    public void JumpToPerson(Object clicked)
    {
        Person clickedPerson = (Person)((UIElement)clicked).UserData;
        GameManager.SetPersonTracking(clickedPerson);

        Parent.ClosePanel(clicked);

        Globals.Model.GameCamera.Follow(clickedPerson);
    }

    public void SortByName(Object clicked)
    {

    }

    public void SortByProfession(Object clicked)
    {

    }

    public void SortByHunger(Object clicked)
    {

    }
}