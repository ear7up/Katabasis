using System;
using System.Collections;

public enum GoodsType
{
    FOOD_PROCESSED = 0,
    FOOD_ANIMAL,
    FOOD_PLANT,
    TOOL,
    MATERIAL_ANIMAL,
    MATERIAL_PLANT,
    MATERIAL_NATURAL,
    CRAFT_GOODS,
    WAR_GOODS,
    SMITHED,
    NONE
}

public class Goods
{
    public GoodsType Type { get; set; }
    public int SubType { get; set; }
    public int Quantity { get; set; }

    public enum ProcessedFood
    {
        BREAD, BEER, WINE
    }

    public enum FoodAnimal
    {
        PORK, BEEF, MUTTON, DUCK, PIGEON, GOOSE, PARTRIDGE, 
        QUAIL, GAME, FISH, MILK, EGGS, HONEY
    }

    public enum FoodPlant
    {
        GARLIC, SCALLIONS, ONION, LEEK, LETTUCE, CELERY, 
        CUCUMBER, RADISH, TURNIP, GRAPES, GOURD, MELON, 
        PEAS, LENTILS, CHICKPEAS, NUTS, OLIVE_OIL
    }

    public enum Tool
    {
        PICKAXE, SHOVEL, HAMMER, KILN, FURNACE, SAW, KNIFE, 
        SPEAR, FISHING_NET, AXE, SEWING_KIT, LOOM, CHISEL, NONE
    }

    public enum MaterialAnimal
    {
        IVORY, BONE, HIDE, WOOL
    }

    public enum MaterialPlant
    {
        FLAX, CEDAR, EBONY, PAPYRUS
    }

    public enum MaterialNatural
    {
        SALT, FLINT, STONE, CLAY, RAW_IRON, RAW_COPPER, RAW_SILVER, RAW_GOLD, 
        RAW_LEAD, MALACHITE, LAPIS_LAZULI, OBSIDIAN, RAW_TIN,
        SANDSTONE
    }

    public enum Crafted
    {
        CLOTHING, BRICKS, LINEN, COMBS, JEWELRY, POTTERY, STATUES, 
        INSTRUMENTS, YARN, LEATHER
    }

    public enum War
    {
        CHARIOT, SWORD, AXE, SLING, SHIELD, HELMET
    }

    public enum Smithed
    {
        IRON, SILVER, GOLD, COPPER, TIN, BRONZE, LEAD
    }

    // Percentage of one unit of the good consumed per day of use (generally % of 1kg)
    // Each row represents a category, if the category doesn't have enough items it will be filled with 0f
    public float[,] USE_RATE = {
        {/*BREAD*/1f, /*BEER*/1f, /*WINE*/1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f},

        {/*PORK*/0.1f, /*BEEF*/0.1f, /*MUTTON*/0.1f, /*DUCK*/0.2f, /*PIGEON*/0.2f, /*GOOSE*/0.2f, /*PARTRIDGE*/0.2f, 
         /*QUAIL*/0.2f, /*GAME*/0.1f, /*FISH*/0.3f, /*MILK*/0.1f, /*EGGS*/0.1f, /*HONEY*/0.05f, 0f, 0f, 0f, 0f},

        {/*GARLIC*/0.05f, /*SCALLIONS*/0.05f, /*ONION*/0.05f, /*LEEK*/0.1f, /*LETTUCE*/0.1f, /*CELERY*/0.1f,
         /*CUCUMBER*/0.1f, /*RADISH*/0.15f, /*TURNIP*/0.15f, /*GRAPES*/0.15f, /*GOURD*/0.2f, /*MELON*/0.2f, 
         /*PEAS*/0.2f, /*LENTILS*/0.2f, /*CHICKPEAS*/0.2f, /*NUTS*/0.15f, /*OLIVE_OIL*/0.05f},

        {/*PICKAXE*/0.005f, /*SHOVEL*/0f, /*HAMMER*/0.05f, /*KILN*/0f, /*FURNACE*/0f, /*SAW*/0.005f, /*KNIFE*/0.005f,
         /*SPEAR*/0f, /*FISHING_NET*/0.005f, /*AXE*/0.05f, /*SEWING_KIT*/0.05f, /*LOOM*/0.05f, /*CHISEL*/0.05f, 0f, 0f, 0f, 0f},

        // These materials are stable, only consumed when converted into something else
        {/*IVORY*/0f, /*BONE*/0f, /*HIDE*/0f, /*WOOL*/0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f},

        {/*FLAX*/0f, /*CEDAR*/0f, /*EBONY*/0f, /*PAPYRUS*/0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f},

        {/*SALT*/0.05f, /*FLINT*/0f, /*STONE*/0f, /*CLAY*/0f, /*IRON*/0f, /*COPPER*/0f, /*SILVER*/0f, /*GOLD*/0f,
         /*LEAD*/0f, /*MALACHITE*/0f, /*LAPIS_LAZULI*/0f, /*OBSIDIAN*/0f, /*TIN*/0f, /*SANDSTONE*/0f, 0f, 0f, 0f},

        {/*CLOTHING*/0.005f, /*BRICKS*/0f, /*COMBS*/0.005f, /*JEWELRY*/0f, /*POTTERY*/0f, /*STATUES*/0f, 
         /*INSTRUMENTS*/0.005f, /*YARN*/0f, /*LEATHER*/0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f},

        {/*CHARIOT*/0.005f, /*SWORD*/0.005f, /*AXE*/0.005f, /*SLING*/0.005f, /*SHIELD*/0.005f, /*SHIELD*/0.005f, /*HELMET*/0.005f,
         0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f},

        {/*IRON*/0f, /*SILVER*/0f, /*GOLD*/0f, /*COPPER*/0f, /*TIN*/0f, /*BRONZE*/0f, /*LEAD*/0f, 
         0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}
    };

    public static int NUM_GOODS_TYPES = 0;
    public static int GOODS_PER_TYPE = 0;

    // Figute out how many types of goods there are, and the maximum number of goods in any given type
    // this will help create a square matrix for calculating demand for goods
    public static void CalcGoodsTypecounts()
    {
        NUM_GOODS_TYPES = Enum.GetValues(typeof(GoodsType)).Length;

        Type[] enums = { typeof(ProcessedFood) };
        int max = 0;
        foreach (Type t in enums)
        {
            max = Math.Max(max, Enum.GetValues(t).Length);
        }
        GOODS_PER_TYPE = max;
    }

    // Return a unique identifier for each good
    public int GetId()
    {
        return (int)Type * NUM_GOODS_TYPES + (int)SubType;
    }

    public Goods(GoodsType goodsType, int subType, int quantity = 0)
    {
        Type = goodsType;
        SubType = subType;
        Quantity = quantity;
    }

    public Goods(Goods orig)
    {
        Type = orig.Type;
        SubType = orig.SubType;
        Quantity = orig.Quantity;
    }

    public override string ToString()
    {
        string typeName = Enum.GetName(typeof(GoodsType), Type);
        string subTypeName = "UNDEFINED";
        switch (Type)
        {
            case GoodsType.FOOD_PROCESSED: subTypeName = Enum.GetName(typeof(ProcessedFood), SubType); break;
            case GoodsType.FOOD_ANIMAL: subTypeName = Enum.GetName(typeof(FoodAnimal), SubType); break;
            case GoodsType.FOOD_PLANT: subTypeName = Enum.GetName(typeof(FoodPlant), SubType); break;
            case GoodsType.TOOL: subTypeName = Enum.GetName(typeof(Tool), SubType); break;
            case GoodsType.MATERIAL_ANIMAL: subTypeName = Enum.GetName(typeof(MaterialAnimal), SubType); break;
            case GoodsType.MATERIAL_PLANT: subTypeName = Enum.GetName(typeof(MaterialPlant), SubType); break;
            case GoodsType.MATERIAL_NATURAL: subTypeName = Enum.GetName(typeof(MaterialNatural), SubType); break;
            case GoodsType.CRAFT_GOODS: subTypeName = Enum.GetName(typeof(Crafted), SubType); break;
            case GoodsType.WAR_GOODS: subTypeName = Enum.GetName(typeof(War), SubType); break;
        }
        return $"Goods(type={typeName}, subType={subTypeName}, quantity={Quantity})";
    }

    // Subtract and return as much of the requested quantity if possible
    public int Take(int quantity)
    {
        int before = Quantity;
        Quantity = MathHelper.Max(Quantity, Quantity - quantity);
        return before - Quantity;
    }

    // Convenience functions to compare with subtypes
    public bool IsProcessedFood(ProcessedFood t)
    {
        return Type == GoodsType.FOOD_PROCESSED && (ProcessedFood)SubType == t;
    }

    public bool IsMeat(FoodAnimal t)
    {
        return Type == GoodsType.FOOD_PROCESSED && (FoodAnimal)SubType == t;
    }

    public bool IsFoodPlant(FoodPlant t)
    {
        return Type == GoodsType.FOOD_PLANT && (FoodPlant)SubType == t;
    }

    public bool IsTool(Tool t)
    {
        return Type == GoodsType.TOOL && (Tool)SubType == t;
    }

    public bool IsMaterialAnimal(MaterialAnimal t)
    {
        return Type == GoodsType.MATERIAL_ANIMAL && (MaterialAnimal)SubType == t;
    }

    public bool IsMaterialWood(MaterialPlant t)
    {
        return Type == GoodsType.MATERIAL_PLANT && (MaterialPlant)SubType == t;
    }

    public bool IsCraftGood(Crafted t)
    {
        return Type == GoodsType.CRAFT_GOODS && (Crafted)SubType == t;
    }

    public bool IsWarGood(War t)
    {
        return Type == GoodsType.WAR_GOODS && (War)SubType == t;
    }
}