using System;
using System.Collections;
using System.Collections.Generic;

public class InventoryPanel : CloseablePanel
{
    public const int MAX_ROWS = 12;

    public TabLayout MyLayout;
    public StockpileDisplay PublicStockpileLayout;
    public StockpileDisplay PrivateStockpileLayout;

    public InventoryPanel() : base(Sprites.TallPanel)
    {
        PublicStockpileLayout = new("Treasury");
        PrivateStockpileLayout = new("Private Goods");

        MyLayout = new();
        MyLayout.TabBox.Image = Sprite.Create(Sprites.TabBackground, Vector2.Zero);
        MyLayout.TabBox.Image.DrawRelativeToOrigin = false;
        MyLayout.SetMargin(top: 30, left: 35);

        UIElement tab1 = new UIElement(Sprites.TabUnselected);
        tab1.AddSelectedImage(Sprites.TabSelected);
        UIElement tab2 = new UIElement(Sprites.TabUnselected);
        tab2.AddSelectedImage(Sprites.TabSelected);

        MyLayout.AddTab("Private Goods", tab1, PrivateStockpileLayout);
        MyLayout.AddTab("Treasury", tab2, PublicStockpileLayout);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X / 2 - Width() / 2, 50f));
    }

    public void Update(Stockpile publicStockpile, Stockpile privateStockpile)
    {
        if (Hidden)
            return;

        PublicStockpileLayout.Update(publicStockpile);
        PrivateStockpileLayout.Update(privateStockpile);

        MyLayout.Update();
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
        MyLayout.Draw(offset);
    }
}