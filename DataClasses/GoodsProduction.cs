using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GoodsRequirement
{
    public Hashtable Options;
    public bool And;
    public GoodsRequirement(Goods goods1, Goods goods2 = null, Goods goods3 = null, Goods goods4 = null, bool and = false)
    {
        Options = new();
        Options.Add(goods1.GetId(), goods1);
        if (goods2 != null)
            Options.Add(goods2.GetId(), goods2);
        if (goods3 != null)
            Options.Add(goods3.GetId(), goods3);
        if (goods4 != null)
            Options.Add(goods4.GetId(), goods4);
        And = and;
    }

    // TODO: this handles one-of, but what about requiring multiple ingredients?
    public bool IsSatisfiedBy(Goods goods)
    {
        return Options.Contains(goods.GetId());
    }

    public void Add(Goods goods)
    {
        Options.Add(goods.GetId(), goods);
    }

    // Return all goods if `And`, otherwise a random choice
    public List<Goods> ToList()
    {
        List<Goods> goods = new();

        foreach (Goods g in Options.Values)
            goods.Add(g);

        if (!this.And)
        {
            Goods choice = goods[Globals.Rand.Next(goods.Count)];
            goods.Clear();
            goods.Add(choice);
        }
        return goods;
    }

    public override string ToString()
    {
        string options = string.Join(", ", Options.Values.Cast<Goods>().Select(x  => x.ToString()).ToArray());
        return $"GoodsRequirement: [{options}]";
    }
}

public class ToolRequirement
{
    public Goods.Tool Tool;
    public ToolMaterial Material;

    public ToolRequirement(Goods.Tool tool = Goods.Tool.NONE, ToolMaterial material = ToolMaterial.NONE)
    {
        Tool = tool;
        Material = material;

        // No such thing as a "none" pickaxe, for example, so convert these to stone
        if (GoodsInfo.GetHasMaterial(Goods.GetId(GoodsType.TOOL, (int)tool, (int)material)) & material == ToolMaterial.NONE)
        {
            Material = ToolMaterial.STONE;
        }
    }
}

public class GoodsProduction
{
    public static Hashtable Requirements;
    public static Hashtable GoodsBySkill;
    public static List<List<int>> MostProfitable;

    public static string Print()
    {
        return string.Join(",", Requirements.Values.Cast<object>().Select(x  => x.ToString()).ToArray());
    }
    
    // If the given skill isn't used to produce anything, return goods with no skill requirements
    public static List<int> GetGoodsMadeUsingSkill(SkillLevel skill)
    {
        List<int> produceable = new();
        List<int> goods = (List<int>)GoodsBySkill[skill.skill];
        if (goods == null)
            return produceable;

        foreach (int g in goods)
        {
            ProductionRequirements req = (ProductionRequirements)Requirements[g];
            if (req.SkillRequirement == null || req.SkillRequirement.level <= skill.level)
                produceable.Add(g);
        }
        return produceable;
    }

    public static int MostProfitableUsing(SkillLevel s)
    {
        if ((int)s.skill > MostProfitable.Count)
            return -1;

        // Ordered by most profitable, so pick the first one allowed by skill level
        foreach (int goodsId in MostProfitable[(int)s.skill])
        {
            SkillLevel req = ((ProductionRequirements)Requirements[goodsId]).SkillRequirement;
            if (req == null || req.level <= s.level)
                return goodsId;
        }
        return -1;
    }

    public static float CalculateTimeToProduce(int goodsId, float quantity, int level)
    {
        float timeToProduce = GoodsInfo.GetTime(goodsId) * quantity;
        timeToProduce *= (200 - level) / 200f;
        return timeToProduce;
    }

    public static float CalculateProfitability(int goodsId, int level)
    {
        float profit = Globals.Market.Prices[goodsId];
        GoodsRequirement req = ((ProductionRequirements)Requirements[goodsId]).GoodsRequirement;

        float timeToProduce = CalculateTimeToProduce(goodsId, 1f, level);

        // No goods required, pure profit
        if (req == null)
            return profit / timeToProduce;

        float cheapestMaterialCost = 99999999f;
        float materialCost = 0f;
        foreach (Goods good in req.Options)
        {
            float reqPrice = Globals.Market.Prices[good.GetId()];
            cheapestMaterialCost = Math.Min(cheapestMaterialCost, reqPrice);
            materialCost += reqPrice;
        }

        if (req.And)
            profit -= materialCost;
        else
            profit -= cheapestMaterialCost;

        return profit / timeToProduce;
    }

    public static void UpdateProfitability()
    {
        MostProfitable.Clear();
        for (int skillId = 0; skillId < Enum.GetValues(typeof(Skill)).Length; skillId++)
        {
            SkillLevel slevel = new() { skill = (Skill)skillId, level = 100 };
            List<int> goodsIds = GetGoodsMadeUsingSkill(slevel);
            goodsIds.OrderBy(x => CalculateProfitability(x, level: 100));
            MostProfitable.Add(goodsIds);
        }
    }

    public static void Init()
    {
        Requirements = new();
        GoodsBySkill = new();
        MostProfitable = new();

        // TODO: Set quantity on goods in GoodsRequirement for non 1:1 conversions

        ProductionRequirements r = null;
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
            toolRequirement: new ToolRequirement(Goods.Tool.SEWING_KIT),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 30)));

        // Ivory combs? I'm guessing they're hard to make, so high crafting skill
        g.SubType = (int)Goods.Crafted.COMBS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.IVORY)),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 60),
            toolRequirement: new ToolRequirement(Goods.Tool.KNIFE)));

        // Instruments are VERY hard to make, made from wood, bone, or ivory
        g.SubType = (int)Goods.Crafted.INSTRUMENTS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD),
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.BONE),
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.IVORY)),
            toolRequirement: new ToolRequirement(Goods.Tool.KNIFE),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 80)));

        // Hard to make, requieres hammer, possibly also a chisel?
        g.SubType = (int)Goods.Crafted.JEWELRY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.SMITHED, (int)Goods.Smithed.GOLD),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.LAPIS_LAZULI),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.MALACHITE),
                new Goods(GoodsType.SMITHED, (int)Goods.Smithed.SILVER)),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 70)));

        // Leather requires animal hides, should it require a tannery building?
        g.SubType = (int)Goods.Crafted.LEATHER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.HIDE)),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 40)));

        // Linen is crafted from flax using a loom, pretty easy
        g.SubType = (int)Goods.Crafted.LINEN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.FLAX)),
            toolRequirement: new ToolRequirement(Goods.Tool.LOOM),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 20)));
        
        // Pottery is made from clay, very easy
        g.SubType = (int)Goods.Crafted.POTTERY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY)),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 10)));

        // TODO: Should a workshop building be required?
        g.SubType = (int)Goods.Crafted.STATUES;

        GoodsRequirement statueMaterials = new(new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE));
        statueMaterials.Add(new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.SANDSTONE));
        statueMaterials.Add(new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY));
        statueMaterials.Add(new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.IVORY));
        statueMaterials.Add(new Goods(GoodsType.SMITHED, (int)Goods.Smithed.COPPER));
        statueMaterials.Add(new Goods(GoodsType.SMITHED, (int)Goods.Smithed.GOLD));
        statueMaterials.Add(new Goods(GoodsType.SMITHED, (int)Goods.Smithed.SILVER));
        statueMaterials.Add(new Goods(GoodsType.SMITHED, (int)Goods.Smithed.LEAD));

        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: statueMaterials,
            toolRequirement: new ToolRequirement(Goods.Tool.CHISEL),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 50)));

        // Wool + loom -> yarn
        g.SubType = (int)Goods.Crafted.YARN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_ANIMAL, (int)Goods.MaterialAnimal.WOOL)),
            toolRequirement: new ToolRequirement(Goods.Tool.LOOM),
            levelRequirement: SkillLevel.Create(Skill.CRAFTING, 10)));

        // crafting: reeds + knife -> papyrus
        g.SubType = (int)Goods.Crafted.PAPYRUS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.REEDS)),
            toolRequirement: new ToolRequirement(Goods.Tool.KNIFE)));

        g.Type = GoodsType.FOOD_ANIMAL;

        // raw beef -> beef
        g.SubType = (int)Goods.FoodAnimal.BEEF;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_BEEF)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 10)));

        // raw duck -> duck
        g.SubType = (int)Goods.FoodAnimal.DUCK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_DUCK)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 10)));

        // farm -> eggs
        g.SubType = (int)Goods.FoodAnimal.EGGS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.GOOSE,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 10)));

        // raw fish -> fish
        g.SubType = (int)Goods.FoodAnimal.FISH;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_FISH)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 10)));

        // raw game -> game
        g.SubType = (int)Goods.FoodAnimal.GAME;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_GAME)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 10)));

        // raw goose -> goose
        g.SubType = (int)Goods.FoodAnimal.GOOSE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_GOOSE)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 10)));

        // farm + forest -> honey
        g.SubType = (int)Goods.FoodAnimal.HONEY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.FOREST,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 40)));

        // ranch + cows -> milk
        g.SubType = (int)Goods.FoodAnimal.MILK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.COW,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // raw mutton -> mutton
        g.SubType = (int)Goods.FoodAnimal.MUTTON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_MUTTON)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 20)));

        // raw fowl -> fowl
        g.SubType = (int)Goods.FoodAnimal.FOWL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_FOWL)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 20)));

        // raw pork -> pork
        g.SubType = (int)Goods.FoodAnimal.PORK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_PORK)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 20)));

        // raw quail -> quail
        g.SubType = (int)Goods.FoodAnimal.QUAIL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.RAW_QUAIL)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 20)));

        g.Type = GoodsType.FOOD_PLANT;

        // FARMING: FARM -> plants
        foreach (Goods.FoodPlant type in Enum.GetValues(typeof(Goods.FoodPlant)))
        {
            if (type == Goods.FoodPlant.NONE)
                continue;

            g.SubType = (int)type;
            Requirements.Add(g.GetId(), new ProductionRequirements(
                buildingRequirement: BuildingType.FARM,
                toolRequirement: new ToolRequirement(Goods.Tool.HOE),
                levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));
        }

        // Scavenge for food on a vegetation tile
        // Does not require a farm, uses cooking level 1 to make hungry villagers more likely to do it
        g.SubType = (int)Goods.FoodPlant.WILD_EDIBLE;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.BuildingRequirement = BuildingType.NONE;
        r.SkillRequirement = SkillLevel.Create(Skill.COOKING, 1);
        r.TileRequirement = TileType.VEGETATION;
        r.ToolRequirement = null;

        g.Type = GoodsType.FOOD_PROCESSED;

        // barley + pottery -> beer
        g.SubType = (int)Goods.ProcessedFood.BEER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.BARLEY),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.POTTERY),
                and: true),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 30)));

        // wheat -> flour
        g.SubType = (int)Goods.ProcessedFood.FLOUR;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.WHEAT)),
            buildingRequirement: BuildingType.GRANARY,
            levelRequirement: SkillLevel.Create(Skill.COOKING, 10)));

        // flour -> bread
        g.SubType = (int)Goods.ProcessedFood.BREAD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PROCESSED, (int)Goods.ProcessedFood.FLOUR)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 20)));

        // grapes -> wine
        g.SubType = (int)Goods.ProcessedFood.WINE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.GRAPES)),
            levelRequirement: SkillLevel.Create(Skill.COOKING, 30)));

        g.Type = GoodsType.MATERIAL_ANIMAL;

        // farming: ranch (any animal) -> bone
        g.SubType = (int)Goods.MaterialAnimal.BONE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // farming: ranch + cow -> hide
        g.SubType = (int)Goods.MaterialAnimal.HIDE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.COW,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // hunting: spear + elephant -> ivory
        g.SubType = (int)Goods.MaterialAnimal.IVORY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            toolRequirement: new ToolRequirement(Goods.Tool.SPEAR),
            tileRequirement: TileType.ELEPHANT,
            levelRequirement: SkillLevel.Create(Skill.HUNTING, 40)));

        // farming: ranch + sheep -> wool
        g.SubType = (int)Goods.MaterialAnimal.WOOL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.SHEEP,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        g.Type = GoodsType.MATERIAL_NATURAL;

        // desert: clay (gatherable by hand from any desert tile)
        g.SubType = (int)Goods.MaterialNatural.CLAY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.DESERT));

        // river: flint?
        g.SubType = (int)Goods.MaterialNatural.FLINT;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.RIVER));

        // mining: hills + pickaxe + mine -> lapis lazuli
        g.SubType = (int)Goods.MaterialNatural.LAPIS_LAZULI;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.LAPIS_LAZULI_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE, ToolMaterial.COPPER),
            levelRequirement: SkillLevel.Create(Skill.MINING, 60)));

        // mining: hills + pickaxe + mine -> malachite
        g.SubType = (int)Goods.MaterialNatural.MALACHITE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.MALACHITE_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE, ToolMaterial.COPPER),
            levelRequirement: SkillLevel.Create(Skill.MINING, 50)));

        // mining: volcano + pickaxe -> obsidian (extremely rare in Egypt)
        g.SubType = (int)Goods.MaterialNatural.OBSIDIAN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.DORMANT_VOLCANO,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE),
            levelRequirement: SkillLevel.Create(Skill.MINING, 10)));

        // mining: hills + pickaxe + mine -> raw copper
        g.SubType = (int)Goods.MaterialNatural.RAW_COPPER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.COPPER_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE),
            levelRequirement: SkillLevel.Create(Skill.MINING, 20)));

        // mining: hills + pickaxe + mine -> raw gold
        g.SubType = (int)Goods.MaterialNatural.RAW_GOLD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.GOLD_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE, ToolMaterial.COPPER),
            levelRequirement: SkillLevel.Create(Skill.MINING, 60)));

        // mining: hills + pickaxe + mine -> raw iron
        g.SubType = (int)Goods.MaterialNatural.RAW_IRON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.IRON_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE, ToolMaterial.COPPER),
            levelRequirement: SkillLevel.Create(Skill.MINING, 30)));

        // mining: hills + pickaxe + mine -> raw lead
        g.SubType = (int)Goods.MaterialNatural.RAW_LEAD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.LEAD_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE, ToolMaterial.COPPER),
            levelRequirement: SkillLevel.Create(Skill.MINING, 20)));

        // mining: hills + pickaxe + mine -> raw silver
        g.SubType = (int)Goods.MaterialNatural.RAW_SILVER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.SILVER_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE, ToolMaterial.COPPER),
            levelRequirement: SkillLevel.Create(Skill.MINING, 50)));

        // mining: hills + pickaxe + mine -> raw tin
        g.SubType = (int)Goods.MaterialNatural.RAW_TIN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.TIN_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE),
            levelRequirement: SkillLevel.Create(Skill.MINING, 10)));

        // mining: hills + pickaxe + mine -> salt
        g.SubType = (int)Goods.MaterialNatural.SALT;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            buildingSubTypeRequirement: BuildingSubType.SALT_MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE),
            levelRequirement: SkillLevel.Create(Skill.MINING, 10)));

        // mining: hills + pickaxe + mine -> sandstone
        g.SubType = (int)Goods.MaterialNatural.SANDSTONE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            toolRequirement: new ToolRequirement(Goods.Tool.PICKAXE),
            levelRequirement: SkillLevel.Create(Skill.MINING, 10)));

        // (simple gathering) hills -> stone
        g.SubType = (int)Goods.MaterialNatural.STONE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.HILLS,
            levelRequirement: SkillLevel.Create(Skill.MINING, 1)));

        g.Type = GoodsType.MATERIAL_PLANT;

        // forestry: forest + axe -> wood
        g.SubType = (int)Goods.MaterialPlant.WOOD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.FOREST,
            toolRequirement: new ToolRequirement(Goods.Tool.AXE),
            levelRequirement: SkillLevel.Create(Skill.FORESTRY, 10)));

        // farming: farm -> flax
        g.SubType = (int)Goods.MaterialPlant.FLAX;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // gathering: river -> reeds
        g.SubType = (int)Goods.MaterialPlant.REEDS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.RIVER));

        g.Type = GoodsType.RAW_MEAT;

        // farming: farm + cows -> raw beef
        g.SubType = (int)Goods.RawMeat.RAW_BEEF;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.COW,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // farming: farm + ducks -> raw duck
        g.SubType = (int)Goods.RawMeat.RAW_DUCK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.DUCK,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // fishing: river + net -> raw fish
        g.SubType = (int)Goods.RawMeat.RAW_FISH;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.RIVER,
            toolRequirement: new ToolRequirement(Goods.Tool.FISHING_NET),
            levelRequirement: SkillLevel.Create(Skill.FISHING, 10)));

        // hunting: wild animals + spear -> raw game
        g.SubType = (int)Goods.RawMeat.RAW_GAME;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.WILD_ANIMAL,
            toolRequirement: new ToolRequirement(Goods.Tool.SPEAR),
            levelRequirement: SkillLevel.Create(Skill.HUNTING, 10)));

        // farming: RANCH + goose -> raw goose
        g.SubType = (int)Goods.RawMeat.RAW_GOOSE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.GOOSE,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // farming: RANCH + sheep -> raw mutton
        g.SubType = (int)Goods.RawMeat.RAW_MUTTON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.SHEEP,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // farming: RANCH + fowl -> raw fowl
        g.SubType = (int)Goods.RawMeat.RAW_FOWL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.FOWL,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // farming: RANCH + pigs -> raw pork
        g.SubType = (int)Goods.RawMeat.RAW_PORK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.PIG,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        // farming: RANCH + quail -> raw qual
        g.SubType = (int)Goods.RawMeat.RAW_QUAIL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.RANCH,
            tileRequirement: TileType.QUAIL,
            levelRequirement: SkillLevel.Create(Skill.FARMING, 20)));

        g.Type = GoodsType.SMITHED;

        // smithing: raw tin + raw copper -> bronze
        g.SubType = (int)Goods.Smithed.BRONZE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_TIN),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_COPPER),
                and: true),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 30)));

        // smithing: raw copper -> copper
        g.SubType = (int)Goods.Smithed.COPPER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_COPPER)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 10)));

        // smithing: raw gold -> gold
        g.SubType = (int)Goods.Smithed.GOLD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_GOLD)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 20)));

        // smithing: raw iron -> iron
        g.SubType = (int)Goods.Smithed.IRON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_IRON)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 20)));

        // smithing: raw lead -> lead
        g.SubType = (int)Goods.Smithed.LEAD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_LEAD)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 20)));

        // smithing: raw silver -> silver
        g.SubType = (int)Goods.Smithed.SILVER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_SILVER)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 20)));

        // smithing: raw tin -> tin
        g.SubType = (int)Goods.Smithed.TIN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_TIN)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: SkillLevel.Create(Skill.SMITHING, 20)));

        g.Type = GoodsType.TOOL;

        // smithing: stone -> stone tool
        // smithing: [iron OR bronze OR copper] + smithy + hammer -> metal tool
        foreach (Goods.Tool type in Enum.GetValues(typeof(Goods.Tool)))
        {
            g.SubType = (int)type;

            List<int> materials = Goods.GetMaterials((int)g.Type, g.SubType);

            // Note: GetMaterials will return materialType = 0 for non-metal tools
            foreach (int materialType in materials)
            {
                g.Material = materialType;
                int madeFromSubType = 0;
                switch ((ToolMaterial)materialType)
                {
                    case ToolMaterial.COPPER: madeFromSubType = (int)Goods.Smithed.COPPER; break;
                    case ToolMaterial.BRONZE: madeFromSubType = (int)Goods.Smithed.BRONZE; break;
                    case ToolMaterial.IRON: madeFromSubType = (int)Goods.Smithed.IRON; break;
                    default: madeFromSubType = (int)Goods.MaterialNatural.STONE; break;
                }

                BuildingType buildingReq = BuildingType.NONE;
                ToolRequirement toolReq = null;
                int levelReq = 10;

                // Stone tools require no building or tool to make
                // Metal tools require a smithy and hammer
                GoodsType madeFromType = GoodsType.MATERIAL_NATURAL;
                if ((ToolMaterial)materialType != ToolMaterial.STONE)
                {
                    madeFromType = GoodsType.SMITHED;
                    buildingReq = BuildingType.SMITHY;
                    toolReq = new ToolRequirement(Goods.Tool.HAMMER);
                    levelReq = 15 * (int)materialType;
                }

                Requirements.Add(g.GetId(), new ProductionRequirements(
                    goodsRequirement: new GoodsRequirement(
                        new Goods((GoodsType)madeFromType, madeFromSubType)),
                    buildingRequirement: buildingReq,
                    toolRequirement: toolReq,
                    levelRequirement: SkillLevel.Create(Skill.SMITHING, levelReq)));
            }
        }

        // Reset material type
        g.Material = 0;

        // Don't let people build the "NONE" tool
        g.SubType = (int)Goods.Tool.NONE;
        Requirements.Remove(g.GetId());

        // smithing: clay + smithy + hammer -> kiln
        g.SubType = (int)Goods.Tool.KILN;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY));

        // crafting: linen + sewing kit -> fishing net
        g.SubType = (int)Goods.Tool.FISHING_NET;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.Crafted.LINEN));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = new ToolRequirement(Goods.Tool.SEWING_KIT);
        r.SkillRequirement = SkillLevel.Create(Skill.CRAFTING, 30);

        // crafting: bricks -> furnace
        g.SubType = (int)Goods.Tool.FURNACE;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.Crafted.BRICKS));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = null;
        r.SkillRequirement = SkillLevel.Create(Skill.CRAFTING, 30);

        // crafting: wood -> loom
        g.SubType = (int)Goods.Tool.LOOM;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialPlant.WOOD));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = new ToolRequirement(Goods.Tool.SAW);
        r.SkillRequirement = SkillLevel.Create(Skill.CRAFTING, 30);

        g.Type = GoodsType.WAR_GOODS;

        // smithing: [copper OR bronze OR iron] + smithy + hammer -> war tool
        foreach (Goods.War type in Enum.GetValues(typeof(Goods.War)))
        {
            g.SubType = (int)type;
            Requirements.Add(g.GetId(), new ProductionRequirements(
                goodsRequirement: new GoodsRequirement(
                    new Goods(GoodsType.SMITHED, (int)Goods.Smithed.IRON),
                    new Goods(GoodsType.SMITHED, (int)Goods.Smithed.BRONZE),
                    new Goods(GoodsType.SMITHED, (int)Goods.Smithed.COPPER)),
                buildingRequirement: BuildingType.SMITHY,
                toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
                levelRequirement: SkillLevel.Create(Skill.SMITHING, 30)));
        }

        // crafting: leather + knife -> sling
        g.SubType = (int)Goods.War.SLING;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LEATHER));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = new ToolRequirement(Goods.Tool.KNIFE);

        // shields may also be wood
        g.SubType = (int)Goods.War.SHIELD;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement.Add(new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD));

        // Map skill enum values -> list of goods ids
        foreach (int goodsId in Requirements.Keys)
        {
            ProductionRequirements req = (ProductionRequirements)Requirements[goodsId];
            Skill skill = Skill.NONE;
            if (req.SkillRequirement != null)
                skill = req.SkillRequirement.skill;

            List<int> goodsIds = (List<int>)GoodsBySkill[skill];

            if (goodsIds == null)
            {
                goodsIds = new();
                GoodsBySkill[skill] = goodsIds;
            }

            goodsIds.Add(goodsId);
        }
    }
}