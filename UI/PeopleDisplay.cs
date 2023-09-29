using System;
using System.Collections.Generic;
using Katabasis;
using ProfessionExtension;

public class PeopleDisplay : GridLayout
{
    public static Color TextColor = Color.Black;

    public class PeopleColumn
    {
        public TextSprite HeaderSprite;
        public Func<Person, UIElement> GetValueFunction;

        public PeopleColumn(string name, Func<Person, UIElement> getValue, Action<Object> onClick = null)
        {
            HeaderSprite = new(Sprites.Font, TextColor, Color.Black, name);
            HeaderSprite.OnClick = onClick;
            GetValueFunction = getValue;
        }
    }

    public int MyPage;
    public CloseablePanel Parent;
    public List<PeopleColumn> PersonColumns;

    public PeopleDisplay(CloseablePanel parent, SpriteTexture texture = null) : base(texture)
    {
        HasHeader = true;
        PersonColumns = new();

        MyPage = 1;
        ElementsPerPage = 18;
        MinHeight = 500;
        MinWidth = 700;
        
        PersonColumns.Add(new PeopleColumn("Icon", GetIcon));
        PersonColumns.Add(new PeopleColumn("Name", GetName, SortByName));
        PersonColumns.Add(new PeopleColumn("Profession", GetProfession, SortByProfession));
        PersonColumns.Add(new PeopleColumn("Hunger", GetHunger, SortByHunger));
        PersonColumns.Add(new PeopleColumn("Task", GetTask));

        foreach (PeopleColumn column in PersonColumns)
            column.HeaderSprite.SetPadding(right: 15, bottom: 15);

        Parent = parent;
    }

    public void AddColumn(string name, Func<Person, UIElement> columnCallback, Action<Object> onClick = null)
    {
        PeopleColumn col = new(name, columnCallback, onClick);
        col.HeaderSprite.SetPadding(right: 10);
        PersonColumns.Add(col);
    }

    public void RemoveColumnAt(int index)
    {
        PersonColumns.RemoveAt(index);
    }

    public UIElement GetIcon(Person person)
    {
        UIElement icon = new(person.GetSpriteTexture(), 0.05f, onClick: JumpToPerson);
        icon.UserData = person;
        return icon;
    }

    public UIElement GetName(Person person)
    {
        TextSprite name = new(Sprites.SmallFont, TextColor, text: person.Name);
        name.SetPadding(right: 10);
        return name;
    }

    public UIElement GetProfession(Person person)
    {
        return new TextSprite(Sprites.SmallFont, TextColor, text: person.Profession.Describe());
    }

    public UIElement GetHunger(Person person)
    {
        string hungerText = "Not hungry";
        if (person.Hunger >= Person.STARVING)
            hungerText = "Starving";
        else if (person.Hunger >= Person.STARVING / 2)
            hungerText = "Very hungry";
        else if (person.Hunger > Person.DAILY_HUNGER)
            hungerText = "Hungry";
        hungerText += $" ({person.Hunger})";
        TextSprite hunger = new(Sprites.SmallFont, TextColor, text: hungerText);
        hunger.SetPadding(right: 15);
        return hunger;
    }

    public UIElement GetTask(Person person)
    {
        TextSprite task = new(Sprites.SmallFont, TextColor, text: person.DescribeCurrentTask());
        task.SetPadding(right: 10);
        return task;
    }

    // Override to let this class manage its own paging
    /*
    public override void ChangePageOnScroll()
    {
        if (!Hovering)
            return;

        if (InputManager.ScrollValue < 0)
            MyPage++;
        else if (InputManager.ScrollValue > 0 && MyPage > 1)
            MyPage--;

        InputManager.ScrollValue = 0;
    }
    */

    public void Update(List<Person> people)
    {
        NumberOfDataRows = people.Count;
        base.Update();

        Clear();

        int row = 0;
        int col = 0;
        foreach (PeopleColumn column in PersonColumns)
            SetContent(col++, row, column.HeaderSprite);

        // We don't want to store all people in GridContent, we only want to store the displayed content
        row = 1;
        for (int i = (Page - 1) * ElementsPerPage; i < Page * ElementsPerPage && i < people.Count; i++)
        {
            Person person = people[i];
            col = 0;
            foreach (PeopleColumn column in PersonColumns)
            {
                SetContent(col, row, column.GetValueFunction(person));
                col++;
            }
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