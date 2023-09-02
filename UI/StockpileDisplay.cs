using System;
using System.Collections;
using System.Collections.Generic;

public class StockpileDisplay : VBox
{
    public const int MAX_ROWS = 12;

    // Goods ID to textsprite with name/quantity
    public Dictionary<int, TextSprite> TextLookup;

    public Stockpile StockpileRef;

    public StockpileDisplay(string label) : base()
    {
        TextLookup = new();

        SetMargin(top: 1, left: 1);
        Add(new TextSprite(Sprites.Font, text: label));
        BuildCategoryLayout();
    }

    // Build an accordion with fold-out options for each GoodsType category and add it to the parent layout
    public void BuildCategoryLayout()
    {
        AccordionLayout accordion = new();
        accordion.SetMargin(top: 1, left: 1);
        Add(accordion);

        int i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            GridLayout categoryLayout = new();
            categoryLayout.SetMargin(top: 5, left: 1);
            accordion.AddSection(Goods.Categories[i], categoryLayout);
            i++;
        }
    }

    public void Update(Stockpile stockpile)
    {
        StockpileRef = stockpile;

        if (Hidden)
            return;

        AccordionLayout accordion = (AccordionLayout)Elements[1];

        // Hide each group with no goods
        foreach (VBox container in accordion.Sections.Values)
        {
            GridLayout table = (GridLayout)container.Elements[1];
            if (table.GridContent.Count == 0)
                container.Hide();
            else
                container.Unhide();
        }

        // Add/update each good in the appropriate accordion layout by dictionary lookup
        foreach (Goods goods in stockpile)
        {
            string description = goods.ToString();

            int id = goods.GetId();
            if (!TextLookup.ContainsKey(id))
            {
                TextSprite textSprite = new(Sprites.Font, text: description);
                textSprite.ScaleDown(0.45f);
                textSprite.SetPadding(right: 10);

                string category = Goods.Categories[(int)goods.Type];
                VBox container = (VBox)accordion.Sections[category];
                GridLayout table = (GridLayout)container.Elements[1];

                int row = table.GridContent.Count % MAX_ROWS;
                int col = table.GridContent.Count / MAX_ROWS;
                table.SetContent(col, row, textSprite);

                TextLookup.Add(id, textSprite);
            }
            else
            {
                TextLookup[id].Text = description;
            }
        }
        
        Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
    }
}