using System;
using System.Collections.Generic;

public class PeoplePanel : CloseablePanel
{
    public PeopleDisplay PeopleTable;

    public PeoplePanel() : base(Sprites.BigPanel)
    {
        SetMargin(top: 50, left: 40);

        PeopleTable = new(parent: this);

        // Hack to make Hovering work better when scrolling on the panel
        PeopleTable.SetPadding(right: 400);

        Add(PeopleTable);
    
        SetDefaultPosition(new Vector2(Globals.WindowSize.X - Width(), 50f));
    }

    public void Update(List<Person> people)
    {
        if (Hidden)
            return;

        PeopleTable.Update(people);
        base.Update();
    }
}