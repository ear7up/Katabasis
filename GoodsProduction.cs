using System.Collections;
using System.Linq;

public class GoodsRequirement
{
    Hashtable Options;
    public GoodsRequirement(Goods goods1, Goods goods2 = null, Goods goods3 = null, Goods goods4 = null)
    {
        Options = new();
        Options.Add(goods1.GetId(), goods1);
        if (goods2 != null)
            Options.Add(goods2.GetId(), goods2);
        if (goods3 != null)
            Options.Add(goods3.GetId(), goods3);
        if (goods4 != null)
            Options.Add(goods4.GetId(), goods4);
    }

    // TODO: this handles one-of, but what about requiring multiple ingredients?
    public bool IsSatisfiedBy(Goods goods)
    {
        return Options.Contains(goods.GetId());
    }

    public override string ToString()
    {
        string options = string.Join(", ", Options.Values.Cast<Goods>().Select(x  => x.ToString()).ToArray());
        return $"GoodsRequirement: [{options}]";
    }
}

public class ProductionRequirements
{
    SkillLevel SkillRequirement;
    Goods.Tool ToolRequirement;
    TileType TileRequirement;
    BuildingType BuildingRequirement;
    GoodsRequirement GoodsRequirement;

    public ProductionRequirements(
        SkillLevel levelRequirement = null, 
        Goods.Tool toolRequirement = Goods.Tool.NONE,
        TileType tileRequirement = TileType.NONE,
        BuildingType buildingRequirement = BuildingType.NONE,
        GoodsRequirement goodsRequirement = null)
    {
        SkillRequirement = levelRequirement;
        ToolRequirement = toolRequirement;
        TileRequirement = tileRequirement;
        BuildingRequirement = buildingRequirement;
        GoodsRequirement = goodsRequirement;
    }

    public override string ToString()
    {
        return $"ProductionRequirements(\n" +
            $"  {SkillRequirement}\n" + 
            $"  {ToolRequirement}\n" +
            $"  {TileRequirement}\n" +
            $"  {BuildingRequirement}\n" +
            $"  {GoodsRequirement}\n" +
            ")";
    }
}

public class GoodsProduction
{
    public static Hashtable Requirements;

    public static string Print()
    {
        return string.Join(",", Requirements.Values.Cast<object>().Select(x  => x.ToString()).ToArray());
    }

    public static void Init()
    {
        Requirements = new();

        Goods g = new(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS);

        // Bricks require clay only
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY))));
        
        // Clothing requires linen, yarn, or leather as well as a sewing kit and a low crafting skill
        g.SubType = (int)Goods.Crafted.CLOTHING;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LINEN),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.YARN),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LEATHER)),
            toolRequirement: Goods.Tool.SEWING_KIT,
            levelRequirement: new SkillLevel(Skill.CRAFTING, 30)));

        // Ivory combs? I'm guessing they're hard to make, so high crafting skill
        g.SubType = (int)Goods.Crafted.COMBS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.IVORY)),
            levelRequirement: new SkillLevel(Skill.CRAFTING, 60),
            toolRequirement: Goods.Tool.KNIFE));

        // Instruments are VERY hard to make, made from wood, bone, or ivory
        g.SubType = (int)Goods.Crafted.INSTRUMENTS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR),
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.EBONY),
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.BONE),
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.IVORY)),
            toolRequirement: Goods.Tool.KNIFE,
            levelRequirement: new SkillLevel(Skill.CRAFTING, 80)));

        // Hard to make, requieres hammer, possibly also a chisel?
        g.SubType = (int)Goods.Crafted.JEWELRY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.SMITHED, (int)Goods.Smithed.GOLD),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.LAPIS_LAZULI),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.MALACHITE),
                new Goods(GoodsType.SMITHED, (int)Goods.Smithed.SILVER)),
            toolRequirement: Goods.Tool.HAMMER,
            levelRequirement: new SkillLevel(Skill.CRAFTING, 70)));

        // Leather requires animal hides, should it require a tannery buliding?
        g.SubType = (int)Goods.Crafted.LEATHER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.HIDE)),
            toolRequirement: Goods.Tool.HAMMER,
            levelRequirement: new SkillLevel(Skill.CRAFTING, 40)));

        // Linen is crafted from flax using a loom, pretty easy
        g.SubType = (int)Goods.Crafted.LINEN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.FLAX)),
            toolRequirement: Goods.Tool.LOOM,
            levelRequirement: new SkillLevel(Skill.CRAFTING, 20)));
        
        // Pottery is made from clay, very easy
        g.SubType = (int)Goods.Crafted.POTTERY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY)),
            levelRequirement: new SkillLevel(Skill.CRAFTING, 10)));

        // Statues are made of sandstone, clay, copper, or bronze with a chisel
        // TODO: Should a workshop building be required?
        g.SubType = (int)Goods.Crafted.STATUES;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.SANDSTONE),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY),
                new Goods(GoodsType.SMITHED, (int)Goods.Smithed.COPPER),
                new Goods(GoodsType.SMITHED, (int)Goods.Smithed.BRONZE)),
            toolRequirement: Goods.Tool.CHISEL,
            levelRequirement: new SkillLevel(Skill.CRAFTING, 50)));

        // Wool is made from yarn, easy
        g.SubType = (int)Goods.Crafted.YARN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.WOOL)),
            levelRequirement: new SkillLevel(Skill.CRAFTING, 10)));
    }
}