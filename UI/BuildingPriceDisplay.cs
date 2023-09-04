using System.Collections.Generic;

public class BuildingPriceDisplay : UIElement
{
    public VBox Layout;
    public BuildingType Type;
    public BuildingSubType SubType;
    public List<Goods> RequiredGoods;
    public TextSprite LaborPrice;
    public TextSprite MaterialsPrice;

    public BuildingPriceDisplay(
        SpriteTexture texture,
        BuildingType buildingType,
        BuildingSubType subType = BuildingSubType.NONE) : base(texture)
    {
        Type = buildingType;
        SubType = subType;

        RequiredGoods = BuildingProduction.GetRequirements(buildingType, subType).GoodsRequirement.ToList();

        string name = Globals.Title(Type.ToString());
        if (subType != BuildingSubType.NONE)
            name = Globals.Title(SubType.ToString()) + " " + name;

        TextSprite typeText = new(Sprites.Font, text: name);
        typeText.ScaleDown(0.2f);

        UIElement coinIcon = new(Sprites.Coin, 0.3f);
        LaborPrice = new(Sprites.SmallFont);

        MaterialsPrice = new(Sprites.SmallFont);

        HBox priceLayout1 = new();
        priceLayout1.Add(coinIcon);
        priceLayout1.Add(LaborPrice);

        HBox priceLayout2 = new();
        priceLayout2.Add(coinIcon);
        priceLayout2.Add(MaterialsPrice);

        Layout = new();
        Layout.Add(typeText);
        Layout.Add(priceLayout1);
        Layout.Add(priceLayout2);
    }

    public override void Update()
    {
        LaborPrice.Text = $"Labor: ${(int)(Building.LaborCost(Type, SubType) + 1)}";
        MaterialsPrice.Text = $"Materials: ~${(int)(Building.MaterialCost(RequiredGoods) + 1)}";
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        Layout.Draw(offset);
    }
}