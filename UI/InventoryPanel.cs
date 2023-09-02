using System;
using System.Collections;
using System.Collections.Generic;

public class InventoryPanel : CloseablePanel
{
    public const int MAX_ROWS = 12;

    public TabLayout Layout;
    public StockpileDisplay PublicStockpileLayout;
    public StockpileDisplay PrivateStockpileLayout;

    public InventoryPanel() : base(Sprites.TallPanel)
    {
        PublicStockpileLayout = new("Treasury");
        PrivateStockpileLayout = new("Private Goods");

        Layout = new();
        Layout.TabBox.Image = Sprite.Create(Sprites.TabBackground, Vector2.Zero);
        Layout.TabBox.Image.DrawRelativeToOrigin = false;
        Layout.SetMargin(top: 30, left: 35);

        UIElement tab1 = new UIElement(Sprites.TabUnselected);
        tab1.AddSelectedImage(Sprites.TabSelected);
        UIElement tab2 = new UIElement(Sprites.TabUnselected);
        tab2.AddSelectedImage(Sprites.TabSelected);

        Layout.AddTab("Private Goods", tab1, PrivateStockpileLayout);
        Layout.AddTab("Treasury", tab2, PublicStockpileLayout);

        Position = new Vector2(
            Globals.WindowSize.X / 2 - Width() / 2, 50f);
    }

    public void Update(Stockpile publicStockpile, Stockpile privateStockpile)
    {
        if (Hidden)
            return;

        PublicStockpileLayout.Update(publicStockpile);
        PrivateStockpileLayout.Update(privateStockpile);

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