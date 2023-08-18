using System;
using System.Collections;
using System.Collections.Generic;

public class MarketPanel : UIElement
{
    public VBox Layout;
    
    // Map goods ids to OverlapLayouts containig price information
    public Hashtable PriceDisplayHash;

    public VBox PriceLayout;
    public VBox SellingLayout;

    public MarketPanel() : base(Sprites.TallPanel)
    {
        Layout = new();
        PriceLayout = new();
        SellingLayout = new();

        Layout.Add(new TextSprite(Sprites.Font, text: "Market Prices"));
        Layout.Add(PriceLayout);
        Layout.Add(new TextSprite(Sprites.Font, text: "Sell Orders"));
        Layout.Add(SellingLayout);

        PriceDisplayHash = new();

        const int GOODS_ROWS = 4;

        int i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            if (goodsType == null)
                break;

            int j = 0;
            GridLayout categoryLayout = new();
            foreach (Object subType in Enum.GetValues(goodsType))
            {
                TextSprite nameText = new TextSprite(Sprites.Font, text: Globals.Title(subType.ToString()));
                nameText.SetPadding(right: 20);
                nameText.ScaleDown(0.4f);

                UIElement priceBar = new(Sprites.VerticalBar);
                priceBar.Image.SpriteColor = Color.Green;

                TextSprite priceText = new TextSprite(Sprites.Font, text: "0.0");
                priceText.ScaleDown(0.4f);

                OverlapLayout priceDisplay = new();
                priceDisplay.Add(priceBar);
                priceDisplay.Add(priceText);
                PriceDisplayHash[Goods.GetId(i, j)] = priceDisplay;

                int row = j % GOODS_ROWS;
                int col = j / GOODS_ROWS * 2;
                categoryLayout.SetContent(col, row, nameText);
                categoryLayout.SetContent(col + 1, row, priceDisplay);
                j++;
            }
            PriceLayout.Add(categoryLayout);
            i++;
        }
    }

    public override void Update()
    {
        if (Hidden)
            return;

        int i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            int j = 0;
            foreach (Object subType in Enum.GetValues(goodsType))
            {
                int id = Goods.GetId(i, j);
                OverlapLayout priceDisplay = (OverlapLayout)PriceDisplayHash[id];
                UIElement priceBar = priceDisplay.Elements[0];
                TextSprite priceText = (TextSprite)priceDisplay.Elements[1];

                // Write the current price and draw a bar indicating how its
                // current price relates to the baseline price
                float price = Market.Prices[id];
                float defaultPrice = GoodsInfo.GetDefaultPrice(id);
                priceText.Text = $"{price:0.0}";
                priceBar.Image.SetScaleX(20f * price / defaultPrice);
                if (price == defaultPrice)
                    priceBar.Image.SpriteColor = Color.Green;
                else if (price > defaultPrice)
                    priceBar.Image.SpriteColor = Color.Blue;
                else
                    priceBar.Image.SpriteColor = Color.Red;
                j++;
            }
            i++;
        }
        Layout.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
        Layout.Draw(offset);
    }
}