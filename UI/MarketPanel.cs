using System;
using System.Collections;
using System.Collections.Generic;

public class MarketPanel : UIElement
{
    public class ThreeBuyButtons
    {
        public UIElement Buy1Button;
        public UIElement Buy10Button;
        public UIElement Buy100Button;
        public ThreeBuyButtons(UIElement buy1Button, UIElement buy10Button, UIElement buy100Button)
        {
            Buy100Button = buy100Button;
            Buy10Button = buy10Button;
            Buy1Button = buy1Button;
        }
    }

    // Goods id -> buy buttons
    public Dictionary<int, ThreeBuyButtons> BuyButtons;

    public TabLayout Layout;
    
    // Map goods ids to OverlapLayouts containig price information
    public Hashtable PriceDisplayHash;

    public VBox PriceLayout;
    public VBox SellingLayout;

    AccordionLayout SellingAccordion;

    public MarketPanel() : base(Sprites.TallPanel)
    {
        BuyButtons = new();
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

        // Build the layout for sell orders grouped by GoodsType category
        SellingAccordion = new();
        SellingAccordion.SetMargin(top: 1, left: 1);
        SellingLayout.Add(SellingAccordion);

        i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            GridLayout categoryLayout = new();
            categoryLayout.SetMargin(top: 5, left: 1);
            SellingAccordion.AddSection(Goods.Categories[i], categoryLayout);
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

        i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            VBox container = (VBox)SellingAccordion.Sections[Goods.Categories[i]];
            GridLayout categoryLayout = (GridLayout)container.Elements[1];
            categoryLayout.Clear();
            i++;
        }

        // const int GOODS_ROWS = 14;

        i = 0;
        foreach (KeyValuePair<int,List<MarketOrder>> kv in Globals.Market.SellOrders)
        {
            int category = Goods.TypeFromId(kv.Key);
            VBox container = (VBox)SellingAccordion.Sections[Goods.Categories[category]];
            GridLayout categoryLayout = (GridLayout)container.Elements[1];

            float quantity = 0f;
            List<MarketOrder> selling = kv.Value;
            foreach (MarketOrder order in selling)
                quantity += order.Goods.Quantity;

            string name = Goods.GetGoodsName((GoodsType)category, Goods.SubTypeFromid(kv.Key));
            int row = categoryLayout.GridContent.Count;
            //int col = i / GOODS_ROWS * 2;
            
            TextSprite nameText = new(Sprites.Font, text: name);
            nameText.ScaleDown(0.45f);
            nameText.SetPadding(right: 10);
            
            TextSprite quantityText = new(Sprites.Font, text: $"{quantity:0.0}");
            quantityText.ScaleDown(0.45f);
            quantityText.SetPadding(right: 10);

            float price = Globals.Market.GetPrice(kv.Key);
            TextSprite priceText = new(Sprites.Font, text: $"${price:0.0}");
            priceText.ScaleDown(0.45f);
            priceText.SetPadding(right: 10);

            string idText = $"{kv.Key}";
            if (!BuyButtons.ContainsKey(kv.Key))
            {
                UIElement buy1Button = new(Sprites.Buy1, 1f, Buy1);
                buy1Button.Name = idText;
                buy1Button.SetPadding(right: 5);

                UIElement buy10Button = new(Sprites.Buy10, 1f, Buy10);
                buy10Button.Name = idText;
                buy10Button.SetPadding(right: 5);

                UIElement buy100Button = new(Sprites.Buy100, 1f, Buy100);
                buy100Button.Name = idText;

                BuyButtons[kv.Key] = new ThreeBuyButtons(buy1Button, buy10Button, buy100Button);
            }

            ThreeBuyButtons buttons = BuyButtons[kv.Key];

            categoryLayout.SetContent(0, row, nameText);
            categoryLayout.SetContent(1, row, quantityText);
            categoryLayout.SetContent(2, row, priceText);
            categoryLayout.SetContent(3, row, buttons.Buy1Button);
            categoryLayout.SetContent(4, row, buttons.Buy10Button);
            categoryLayout.SetContent(5, row, buttons.Buy100Button);
            i++;
        }

        i = 0;
        foreach (Type goodsType in Goods.GoodsEnums)
        {
            VBox container = (VBox)SellingAccordion.Sections[Goods.Categories[i]];
            GridLayout categoryLayout = (GridLayout)container.Elements[1];
            if (categoryLayout.GridContent.Count == 0)
            {
                TextSprite noSellOrders = new(Sprites.Font, text: "No sell orders");
                noSellOrders.ScaleDown(0.45f);
                categoryLayout.SetContent(0, 0, noSellOrders);
            }
            i++;
        }

        Layout.Update();
        base.Update();
    }

    public void Buy1(Object clicked)
    {
        Buy(clicked, 1);
    }

    public void Buy10(Object clicked)
    {
        Buy(clicked, 10);
    }

    public void Buy100(Object clicked)
    {
        Buy(clicked, 100);
    }

    public void Buy(Object clicked, int n)
    {
        UIElement button = (UIElement)clicked;
        int id = Int32.Parse(button.Name);

        MarketOrder order = MarketOrder.Create(Globals.Player1.Person, true, Goods.FromId(id, n));
        Globals.Market.PlaceBuyOrder(order);
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);
        Layout.Draw(offset);
    }
}