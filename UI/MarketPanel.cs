using System;
using System.Collections;
using System.Collections.Generic;

public class MarketPanel : UIElement
{
    public TabLayout Layout;
    
    // Map goods ids to OverlapLayouts containig price information
    public Hashtable PriceDisplayHash;

    public VBox PriceLayout;
    public VBox SellingLayout;

    public MarketPanel() : base(Sprites.TallPanel)
    {
        Layout = new();
        Layout.TabBox.Image = Sprite.Create(Sprites.TabBackground, Vector2.Zero);
        Layout.TabBox.Image.DrawRelativeToOrigin = false;
        Layout.SetMargin(top: 30, left: 35);

        PriceLayout = new();
        PriceLayout.SetMargin(top: 1, left: 1);
        PriceLayout.Add(new TextSprite(Sprites.Font, text: "Market Prices"));
        SellingLayout = new();
        SellingLayout.Add(new TextSprite(Sprites.Font, text: "Sell Orders"));

        UIElement tab1 = new UIElement(Sprites.TabUnselected);
        tab1.AddSelectedImage(Sprites.TabSelected);
        UIElement tab2 = new UIElement(Sprites.TabUnselected);
        tab2.AddSelectedImage(Sprites.TabSelected);

        Layout.AddTab("Market Prices", tab1, PriceLayout);
        Layout.AddTab("Sell Orders", tab2, SellingLayout);

        PriceDisplayHash = new();

        const int GOODS_ROWS = 14;

        AccordionLayout categoryAccordion = new();
        categoryAccordion.SetMargin(top: 1, left: 1);
        PriceLayout.Add(categoryAccordion);

        int i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            int j = 0;
            int k = 0;
            GridLayout categoryLayout = new();
            categoryLayout.SetMargin(top: 5, left: 1);
            foreach (Object subType in Enum.GetValues(goodsType))
            {
                if (goodsType == typeof(Goods.Tool) && j == (int)Goods.Tool.NONE)
                    continue;

                foreach (int materialType in Goods.GetMaterials(i, j))
                {
                    string materialName = "";
                    if (materialType != (int)ToolMaterial.NONE)
                        materialName = Globals.Title(((ToolMaterial)materialType).ToString()) + " ";
                    string name = $"{materialName}{subType.ToString()}";

                    TextSprite nameText = new TextSprite(Sprites.Font, text: Globals.Title(name));
                    nameText.SetPadding(right: 10);
                    nameText.ScaleDown(0.45f);

                    UIElement priceBar = new(Sprites.VerticalBar);
                    priceBar.Image.SpriteColor = Color.Green;
                    priceBar.SetPadding(right: 10);

                    TextSprite priceText = new TextSprite(Sprites.Font, text: "0.0");
                    priceText.ScaleDown(0.45f);

                    OverlapLayout priceDisplay = new();
                    priceDisplay.Add(priceBar);
                    priceDisplay.Add(priceText);
                    PriceDisplayHash[Goods.GetId(i, j, materialType)] = priceDisplay;

                    int row = k % GOODS_ROWS;
                    int col = k / GOODS_ROWS * 2;
                    categoryLayout.SetContent(col, row, nameText);
                    categoryLayout.SetContent(col + 1, row, priceDisplay);
                    k++;
                }
                j++;
            }

            categoryAccordion.AddSection(Goods.Categories[i], categoryLayout);
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
                if (goodsType == typeof(Goods.Tool) && j == (int)Goods.Tool.NONE)
                    continue;

                foreach (int materialType in Goods.GetMaterials(i, j))
                {
                    int id = Goods.GetId(i, j, materialType);
                    OverlapLayout priceDisplay = (OverlapLayout)PriceDisplayHash[id];

                    UIElement priceBar = priceDisplay.Elements[0];
                    TextSprite priceText = (TextSprite)priceDisplay.Elements[1];

                    // Write the current price and draw a bar indicating how its
                    // current price relates to the baseline price
                    float price = Globals.Market.Prices[id];
                    float defaultPrice = GoodsInfo.GetDefaultPrice(id);
                    priceText.Text = $"{price:0.0}";
                    priceBar.Image.SetScaleX(25f);

                    if (price == defaultPrice)
                        priceBar.Image.SpriteColor = Color.Goldenrod;
                    else if (price > defaultPrice)
                        priceBar.Image.SpriteColor = Color.Green;
                    else
                        priceBar.Image.SpriteColor = Color.Red;
                }
                j++;
            }
            i++;
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