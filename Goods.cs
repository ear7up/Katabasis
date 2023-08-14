using System;
using System.Collections;

public enum GoodsType
{
    FOOD_PROCESSED = 0,
    FOOD_ANIMAL = 1,
    FOOD_PLANT = 2,
    TOOL = 3,
    MATERIAL_ANIMAL = 4,
    MATERIAL_PLANT = 5,
    MATERIAL_NATURAL = 6,
    CRAFT_GOODS = 7,
    WAR_GOODS = 8,
    SMITHED = 9,
    RAW_MEAT = 10,
    NONE = 11
}

public class Goods
{
    public const float MEAT_SPOIL_RATE = 0.01f;

    // If for some reason in the future there are more than 1000 goods in a single category,
    // bump this to 10000 to make each good type have a unique integer identifier
    public const int MAX_GOODS_PER_CATEGORY = 1000;

    public GoodsType Type { get; set; }
    public int SubType { get; set; }
    public float Quantity { get; set; }

    public enum ProcessedFood
    {
        FLOUR, BREAD, BEER, WINE, SALTED_MEAT
    }

    // Cooked versions MUST parallel with RawMeat
    public enum FoodAnimal
    {
        PORK, BEEF, MUTTON, DUCK, FOWL, 
        GOOSE, QUAIL, GAME, FISH, 

        MILK, EGGS, HONEY
    }

    public enum FoodPlant
    {
        GARLIC, SCALLIONS, ONION, LEEK, LETTUCE, CELERY, 
        CUCUMBER, RADISH, TURNIP, GRAPES, GOURD, MELON, 
        PEAS, LENTILS, CHICKPEAS, NUTS, OLIVE_OIL, BARLEY,
        WHEAT, WILD_EDIBLE
    }

    // Consider making a separate enum for stone tools
    // And make simple production requirements use them instead
    // so that some things can be made without a smithy and tools

    public enum Tool
    {
        PICKAXE, SHOVEL, HAMMER, KILN, FURNACE, SAW, KNIFE, 
        SPEAR, FISHING_NET, AXE, SEWING_KIT, LOOM, CHISEL, HOE, NONE
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

    public enum RawMeat
    {
        RAW_PORK, RAW_BEEF, RAW_MUTTON, RAW_DUCK, RAW_FOWL, 
        RAW_GOOSE, RAW_QUAIL, RAW_GAME, RAW_FISH
    }

    public static int NUM_GOODS_TYPES = 0;
    public static int GOODS_PER_TYPE = 0;

    // Figute out how many types of goods there are, and the maximum number of goods in any given type
    // this will help create a square matrix for calculating demand for goods
    public static void CalcGoodsTypecounts()
    {
        NUM_GOODS_TYPES = Enum.GetValues(typeof(GoodsType)).Length;
        GOODS_PER_TYPE = 19;
    }

    // Decrease the quantity of the object by its use rate
    // TODO: How do we destroy goods when they reach 0 quantity? Do we need to?
    public void Use(float numTimes = 1f)
    {
        float useRate = GoodsInfo.GetUseRate(this);
        Quantity = Math.Max(Quantity - useRate * numTimes, 0);
    }

    // Return a unique identifier for each good
    public int GetId()
    {
        return (int)Type * MAX_GOODS_PER_CATEGORY + SubType;
    }

    public static int GetId(GoodsType type, int subType)
    {
        return (int)type * MAX_GOODS_PER_CATEGORY + subType;
    }

    // Reverse of GetId, assumes there are max 1000 goods per type category
    public static Goods FromId(int id)
    {
        return new Goods((GoodsType)(id / MAX_GOODS_PER_CATEGORY), id % MAX_GOODS_PER_CATEGORY);
    }

    public static int TypeFromId(int id)
    {
        return id / MAX_GOODS_PER_CATEGORY;
    }

    public static int SubTypeFromid(int id)
    {
        return id % MAX_GOODS_PER_CATEGORY;
    }

    public Goods(GoodsType goodsType, int subType, int quantity = 1)
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
        string typeName = Globals.Title(Enum.GetName(typeof(GoodsType), Type));
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
            case GoodsType.SMITHED: subTypeName = Enum.GetName(typeof(Smithed), SubType); break;
            case GoodsType.RAW_MEAT: subTypeName = Enum.GetName(typeof(RawMeat), SubType); break;
        }
        subTypeName = Globals.Title(subTypeName);
        float value = Market.CheckPrice(GetId()) * Quantity;
        //return $"{typeName}.{subTypeName} x{(int)Quantity} (${value}";
        return $"{subTypeName} x{(int)Quantity} (${(int)value})";
    }

    // Subtract and return as much of the requested quantity if possible
    public float Take(float quantity)
    {
        float before = Quantity;
        Quantity -= MathHelper.Min(Quantity, quantity);
        return before - Quantity;
    }

    // Don't actually take, just return the amount that can be taken
    public float Borrow(float quantity)
    {
        return MathHelper.Min(Quantity, quantity);
    }
    
    public void Update()
    {
        Quantity -= GoodsInfo.GetDecayRate(this) * Globals.Time;
    }

    // Convenience functions to compare with subtypes
    public bool IsProcessedFood(ProcessedFood t)
    {
        return Type == GoodsType.FOOD_PROCESSED && (ProcessedFood)SubType == t;
    }

    public bool IsEdible()
    {
        return IsEdible(Type, SubType);
    }

    public static bool IsEdible(GoodsType type, int subType)
    {
        // Don't eat uncooked flour, you can get salmonella
        return 
            type == GoodsType.FOOD_ANIMAL || 
            type == GoodsType.FOOD_PLANT || 
            (type == GoodsType.FOOD_PROCESSED && subType != (int)Goods.ProcessedFood.FLOUR);
    }

    public bool IsCookable()
    {
        return 
            Type == GoodsType.RAW_MEAT || 
            (Type == GoodsType.FOOD_PROCESSED && SubType == (int)Goods.ProcessedFood.FLOUR);
    }

    // Converts a cookable good into its cooked version
    public void Cook()
    {
        if (!IsCookable())
            return;

        if (Type == GoodsType.FOOD_PROCESSED && SubType == (int)Goods.ProcessedFood.FLOUR)
            SubType = (int)Goods.ProcessedFood.BREAD;
        else if (Type == GoodsType.RAW_MEAT)
            Type = GoodsType.FOOD_ANIMAL;
    }

    public bool IsTool()
    {
        return Type == GoodsType.TOOL;
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

    public float Value()
    {
        return Quantity; /* TODO: times market value */
    }
}