using System.Collections.Generic;

public class BuildingPriceDisplay : UIElement
{
    public VBox Layout;
    public BuildingType Type;
    public List<Goods> RequiredGoods;
    public TextSprite LaborPrice;
    public TextSprite MaterialsPrice;

    public BuildingPriceDisplay(
        SpriteTexture texture,
        BuildingType buildingType) : base(texture)
    {
        Type = buildingType;
        RequiredGoods = BuildingProduction.GetRequirements(buildingType).GoodsRequirement.ToList();

        TextSprite typeText = new(Sprites.Font, text: Globals.Title(Type.ToString()));
        typeText.ScaleDown(0.2f);

        UIElement coinIcon = new(Sprites.Coin, 0.3f);
        LaborPrice = new(Sprites.Font);
        LaborPrice.ScaleDown(0.45f);

        MaterialsPrice = new(Sprites.Font);
        MaterialsPrice.ScaleDown(0.45f);

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
        LaborPrice.Text = $"Labor: ${(int)(Building.LaborCost(Type) + 1)}";
        MaterialsPrice.Text = $"Materials: ~${(int)(Building.MaterialCost(RequiredGoods) + 1)}";
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        Layout.Draw(offset);
    }
}