using System;
using System.Collections;
using System.Collections.Generic;

public class InventoryPanel : CloseablePanel
{
    public const int MAX_ROWS = 12;

    public TabLayout Layout;
    public VBox PublicLayout;
    public VBox PrivateLayout;

    // Goods ID to textsprite with name/quantity
    public Dictionary<int, TextSprite> PublicGoods;
    public Dictionary<int, TextSprite> PrivateGoods;

    public InventoryPanel() : base(Sprites.TallPanel)
    {
        PublicGoods = new();
        PrivateGoods = new();

        Layout = new();
        Layout.TabBox.Image = Sprite.Create(Sprites.TabBackground, Vector2.Zero);
        Layout.TabBox.Image.DrawRelativeToOrigin = false;
        Layout.SetMargin(top: 30, left: 35);

        PublicLayout = new();
        PublicLayout.SetMargin(top: 1, left: 1);
        PublicLayout.Add(new TextSprite(Sprites.Font, text: "Treasury"));

        PrivateLayout = new();
        PrivateLayout.SetMargin(top: 1, left: 1);
        PrivateLayout.Add(new TextSprite(Sprites.Font, text: "Private Goods"));

        UIElement tab1 = new UIElement(Sprites.TabUnselected);
        tab1.AddSelectedImage(Sprites.TabSelected);
        UIElement tab2 = new UIElement(Sprites.TabUnselected);
        tab2.AddSelectedImage(Sprites.TabSelected);

        Layout.AddTab("Private Goods", tab1, PrivateLayout);
        Layout.AddTab("Treasury", tab2, PublicLayout);

        BuildCategoryLayout(PrivateLayout);
        BuildCategoryLayout(PublicLayout);

        Position = new Vector2(
            Globals.WindowSize.X / 2 - Width() / 2, 50f);
    }

    // Build an accordion with fold-out options for each GoodsType category and add it to the parent layout
    public void BuildCategoryLayout(Layout parent)
    {
        AccordionLayout accordion = new();
        accordion.SetMargin(top: 1, left: 1);
        parent.Add(accordion);

        int i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            GridLayout categoryLayout = new();
            categoryLayout.SetMargin(top: 5, left: 1);
            accordion.AddSection(Goods.Categories[i], categoryLayout);
            i++;
        }
    }

    public void UpdatePublic(Stockpile stockpile)
    {
        Update(PublicLayout, PublicGoods, stockpile);
    }

    public void UpdatePrivate(Stockpile stockpile)
    {
        Update(PrivateLayout, PrivateGoods, stockpile);
    }

    public void Update(VBox layout, Dictionary<int, TextSprite> textHash, Stockpile stockpile)
    {
        if (Hidden)
            return;

        AccordionLayout accordion = (AccordionLayout)layout.Elements[1];

        foreach (VBox container in accordion.Sections.Values)
        {
            GridLayout table = (GridLayout)container.Elements[1];
            if (table.GridContent.Count == 0)
                container.Hide();
            else
                container.Unhide();
        }

        foreach (Goods goods in stockpile)
        {
            string description = goods.ToString();

            int id = goods.GetId();
            if (!textHash.ContainsKey(id))
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

                textHash.Add(id, textSprite);
            }
            else
            {
                textHash[id].Text = description;
            }
        }

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