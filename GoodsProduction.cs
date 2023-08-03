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

public class GoodsProduction
{
    public static Hashtable Requirements;
    public static Hashtable GoodsBySkill;

    public static string Print()
    {
        return string.Join(",", Requirements.Values.Cast<object>().Select(x  => x.ToString()).ToArray());
    }
    
    // If the given skill isn't used to produce anything, return goods with no skill requirements
    public static List<int> GetGoodsMadeUsingSkill(Skill skill)
    {
        List<int> goods = (List<int>)GoodsBySkill[skill];
        if (goods == null)
        {
            goods = (List<int>)GoodsBySkill[Skill.NONE];
        }
        return goods;
    }

    public static void Init()
    {
        Requirements = new();
        GoodsBySkill = new();

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

        g.Type = GoodsType.FOOD_ANIMAL;

        // raw beef -> beef
        g.SubType = (int)Goods.FoodAnimal.BEEF;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.BEEF)),
            levelRequirement: new SkillLevel(Skill.COOKING, 10)));

        // raw duck -> duck
        g.SubType = (int)Goods.FoodAnimal.DUCK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.DUCK)),
            levelRequirement: new SkillLevel(Skill.COOKING, 10)));

        // farm -> eggs
        g.SubType = (int)Goods.FoodAnimal.EGGS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            levelRequirement: new SkillLevel(Skill.FARMING, 10)));

        // raw fish -> fish
        g.SubType = (int)Goods.FoodAnimal.FISH;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.FISH)),
            levelRequirement: new SkillLevel(Skill.COOKING, 10)));

        // raw game -> game
        g.SubType = (int)Goods.FoodAnimal.GAME;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.GAME)),
            levelRequirement: new SkillLevel(Skill.COOKING, 10)));

        // raw goose -> goose
        g.SubType = (int)Goods.FoodAnimal.GOOSE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.GOOSE)),
            levelRequirement: new SkillLevel(Skill.COOKING, 10)));

        // farm + forest -> honey
        g.SubType = (int)Goods.FoodAnimal.HONEY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.FOREST,
            levelRequirement: new SkillLevel(Skill.FARMING, 40)));

        // farm + animals -> milk
        g.SubType = (int)Goods.FoodAnimal.MILK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // raw mutton -> mutton
        g.SubType = (int)Goods.FoodAnimal.MUTTON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.MUTTON)),
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // raw partridge -> partridge
        g.SubType = (int)Goods.FoodAnimal.PARTRIDGE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.PARTRIDGE)),
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // raw pigeon -> pigeon
        g.SubType = (int)Goods.FoodAnimal.PIGEON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.PIGEON)),
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // raw pork -> pork
        g.SubType = (int)Goods.FoodAnimal.PORK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.PORK)),
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // raw quail -> quail
        g.SubType = (int)Goods.FoodAnimal.QUAIL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.RAW_MEAT, (int)Goods.RawMeat.QUAIL)),
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        g.Type = GoodsType.FOOD_PLANT;

        // FARMING: FARM -> plants
        foreach (Goods.FoodPlant type in Enum.GetValues(typeof(Goods.FoodPlant)))
        {
            g.SubType = (int)type;
            Requirements.Add(g.GetId(), new ProductionRequirements(
                buildingRequirement: BuildingType.FARM,
                levelRequirement: new SkillLevel(Skill.FARMING, 20)));
        }

        // No skill or building required to forage for wild plants, does need a tile with vegetation, though
        g.SubType = (int)Goods.FoodPlant.WILD_EDIBLE;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.BuildingRequirement = BuildingType.NONE;
        r.SkillRequirement = null;
        r.TileRequirement = TileType.VEGETATION;

        g.Type = GoodsType.FOOD_PROCESSED;

        // barley -> beer
        g.SubType = (int)Goods.ProcessedFood.BEER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.BARLEY)),
            levelRequirement: new SkillLevel(Skill.COOKING, 30)));

        // wheat -> bread
        g.SubType = (int)Goods.ProcessedFood.BREAD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.WHEAT)),
            levelRequirement: new SkillLevel(Skill.COOKING, 10)));

        // grapes -> wine
        g.SubType = (int)Goods.ProcessedFood.WINE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.GRAPES)),
            levelRequirement: new SkillLevel(Skill.COOKING, 30)));

        g.Type = GoodsType.MATERIAL_ANIMAL;

        // farming: farm + animals -> bone
        g.SubType = (int)Goods.MaterialAnimal.BONE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> hide
        g.SubType = (int)Goods.MaterialAnimal.HIDE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // hunting: animals -> ivory
        g.SubType = (int)Goods.MaterialAnimal.IVORY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.HUNTING, 40)));

        // farming: farm + animals -> wool
        g.SubType = (int)Goods.MaterialAnimal.WOOL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

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
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 60)));

        // mining: hills + pickaxe + mine -> malachite
        g.SubType = (int)Goods.MaterialNatural.MALACHITE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 50)));

        // mining: volcano + pickaxe -> obsidian (extremely rare in Egypt)
        g.SubType = (int)Goods.MaterialNatural.OBSIDIAN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.DORMANT_VOLCANO,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 10)));

        // mining: hills + pickaxe + mine -> raw copper
        g.SubType = (int)Goods.MaterialNatural.RAW_COPPER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 20)));

        // mining: hills + pickaxe + mine -> raw gold
        g.SubType = (int)Goods.MaterialNatural.RAW_GOLD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 60)));

        // mining: hills + pickaxe + mine -> raw iron
        g.SubType = (int)Goods.MaterialNatural.RAW_IRON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 30)));

        // mining: hills + pickaxe + mine -> raw lead
        g.SubType = (int)Goods.MaterialNatural.RAW_LEAD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 20)));

        // mining: hills + pickaxe + mine -> raw silver
        g.SubType = (int)Goods.MaterialNatural.RAW_SILVER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 50)));

        // mining: hills + pickaxe + mine -> raw tin
        g.SubType = (int)Goods.MaterialNatural.RAW_TIN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 10)));

        // mining: hills + pickaxe + mine -> salt
        g.SubType = (int)Goods.MaterialNatural.SALT;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 10)));

        // mining: hills + pickaxe + mine -> sandstone
        g.SubType = (int)Goods.MaterialNatural.SANDSTONE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.MINE,
            tileRequirement: TileType.HILLS,
            toolRequirement: Goods.Tool.PICKAXE,
            levelRequirement: new SkillLevel(Skill.MINING, 10)));

        // (simple gathering) hills -> stone
        g.SubType = (int)Goods.MaterialNatural.STONE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.HILLS));

        g.Type = GoodsType.MATERIAL_PLANT;

        // forestry: forest + axe -> cedar
        g.SubType = (int)Goods.MaterialPlant.CEDAR;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.FOREST,
            toolRequirement: Goods.Tool.AXE,
            levelRequirement: new SkillLevel(Skill.FORESTRY, 10)));

        // forestry: forest + axe -> ebony
        g.SubType = (int)Goods.MaterialPlant.EBONY;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.FOREST,
            toolRequirement: Goods.Tool.AXE,
            levelRequirement: new SkillLevel(Skill.FORESTRY, 10)));

        // farming: farm -> flax
        g.SubType = (int)Goods.MaterialPlant.FLAX;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // gathering: river -> papyrus
        g.SubType = (int)Goods.MaterialPlant.PAPYRUS;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.RIVER));

        g.Type = GoodsType.RAW_MEAT;

        // farming: farm + animals -> raw beef
        g.SubType = (int)Goods.RawMeat.BEEF;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> raw duck
        g.SubType = (int)Goods.RawMeat.DUCK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // fishing: river + net -> raw fish
        g.SubType = (int)Goods.RawMeat.FISH;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.RIVER,
            toolRequirement: Goods.Tool.FISHING_NET,
            levelRequirement: new SkillLevel(Skill.FISHING, 10)));

        // hunting: animals + spear -> raw game
        g.SubType = (int)Goods.RawMeat.GAME;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            tileRequirement: TileType.ANIMAL,
            toolRequirement: Goods.Tool.SPEAR,
            levelRequirement: new SkillLevel(Skill.HUNTING, 10)));

        // farming: farm + animals -> raw goose
        g.SubType = (int)Goods.RawMeat.GOOSE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> raw mutton
        g.SubType = (int)Goods.RawMeat.MUTTON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> raw partridge
        g.SubType = (int)Goods.RawMeat.PARTRIDGE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> raw pigeon
        g.SubType = (int)Goods.RawMeat.PIGEON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> raw pork
        g.SubType = (int)Goods.RawMeat.PORK;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        // farming: farm + animals -> raw qual
        g.SubType = (int)Goods.RawMeat.QUAIL;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            buildingRequirement: BuildingType.FARM,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.FARMING, 20)));

        g.Type = GoodsType.SMITHED;

        // smithing: raw tin + raw copper -> bronze
        g.SubType = (int)Goods.Smithed.BRONZE;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_TIN),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_COPPER),
                and: true),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 30)));

        // smithing: raw copper -> copper
        g.SubType = (int)Goods.Smithed.COPPER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_COPPER)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 10)));

        // smithing: raw gold -> gold
        g.SubType = (int)Goods.Smithed.GOLD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_GOLD)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 20)));

        // smithing: raw iron -> iron
        g.SubType = (int)Goods.Smithed.IRON;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_IRON)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 20)));

        // smithing: raw lead -> lead
        g.SubType = (int)Goods.Smithed.LEAD;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_LEAD)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 20)));

        // smithing: raw silver -> silver
        g.SubType = (int)Goods.Smithed.SILVER;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_SILVER)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 20)));

        // smithing: raw tin -> tin
        g.SubType = (int)Goods.Smithed.TIN;
        Requirements.Add(g.GetId(), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.RAW_TIN)),
            buildingRequirement: BuildingType.SMITHY,
            levelRequirement: new SkillLevel(Skill.SMITHING, 20)));

        g.Type = GoodsType.TOOL;

        // For now, allow tools to be made of stone only with no tools required
        // smithing: [iron OR bronze OR copper] + smithy + hammer -> tool
        foreach (Goods.Tool type in Enum.GetValues(typeof(Goods.Tool)))
        {
            g.SubType = (int)type;
            Requirements.Add(g.GetId(), new ProductionRequirements(
                goodsRequirement: new GoodsRequirement(
                    new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE)),
                    //new Goods(GoodsType.SMITHED, (int)Goods.Smithed.IRON),
                    //new Goods(GoodsType.SMITHED, (int)Goods.Smithed.BRONZE),
                    //new Goods(GoodsType.SMITHED, (int)Goods.Smithed.COPPER)),
                //buildingRequirement: BuildingType.SMITHY,
                //toolRequirement: Goods.Tool.HAMMER,
                levelRequirement: new SkillLevel(Skill.SMITHING, 10)));
        }

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
        r.ToolRequirement = Goods.Tool.SEWING_KIT;
        r.SkillRequirement = new SkillLevel(Skill.CRAFTING, 30);

        // crafting: bricks -> furnace
        g.SubType = (int)Goods.Tool.FURNACE;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.Crafted.BRICKS));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = Goods.Tool.NONE;
        r.SkillRequirement = new SkillLevel(Skill.CRAFTING, 30);

        // crafting: [cedar OR ebony] -> loom
        g.SubType = (int)Goods.Tool.LOOM;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialPlant.CEDAR),
            new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialPlant.EBONY));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = Goods.Tool.SAW;
        r.SkillRequirement = new SkillLevel(Skill.CRAFTING, 30);

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
                toolRequirement: Goods.Tool.HAMMER,
                levelRequirement: new SkillLevel(Skill.SMITHING, 30)));
        }

        // crafting: leather + knife -> sling
        g.SubType = (int)Goods.War.SLING;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement = new GoodsRequirement(
            new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LEATHER));
        r.BuildingRequirement = BuildingType.NONE;
        r.ToolRequirement = Goods.Tool.KNIFE;

        // shields may also be wood
        g.SubType = (int)Goods.War.SHIELD;
        r = (ProductionRequirements)Requirements[g.GetId()];
        r.GoodsRequirement.Add(new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR));
        r.GoodsRequirement.Add(new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.EBONY));

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