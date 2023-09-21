using System;
using System.Collections;
using System.Collections.Generic;

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

// In order based on quality; 
// There can be max 10 values for any material enum (including NONE = 0)
// A task requiring stone tools can be done with copper or bronze, for example
public enum ToolMaterial
{
    NONE = 0,
    STONE = 1,
    COPPER = 2,
    BRONZE = 3,
    IRON = 4
}

public class Goods
{
    public const float MEAT_SPOIL_RATE = 0.01f;

    // If for some reason in the future there are more than 1000 goods in a single category,
    // bump this to 10000 to make each good type have a unique integer identifier
    // Digits 4+ are the category
    // Digit 3 is the material
    // Digits 1-2 are the subtype (max 100)
    public const int MAX_GOODS_PER_CATEGORY = 1000;
    public const int MAX_GOODS_PER_MATERIAL = 100;

    public GoodsType Type { get; set; }
    public int SubType { get; set; }
    public float Quantity { get; set; }
    public int Material { get; set; }

    public static Type[] GoodsEnums = 
    {
        typeof(ProcessedFood),
        typeof(FoodAnimal),
        typeof(FoodPlant),
        typeof(Tool),
        typeof(MaterialAnimal),
        typeof(MaterialPlant),
        typeof(MaterialNatural),
        typeof(Crafted),
        typeof(War),
        typeof(Smithed),
        typeof(RawMeat)
    };

    public static string[] Categories = 
    {
        "Food (Processed)",
        "Food (Animal)",
        "Food (Produce)",
        "Tools",
        "Animal Products", 
        "Grown Materials",
        "Rocks and Minerals",
        "Craft Goods",
        "Tools of War",
        "Refined Metals",
        "Raw Meat"
    };

    public enum ProcessedFood
    {
        FLOUR, BREAD, BEER, WINE, SALTED_MEAT
    }

    // Cooked versions MUST parallel with RawMeat
    public enum FoodAnimal
    {
        PORK, BEEF, MUTTON, DUCK, FOWL, 
        GOOSE, QUAIL, GAME, FISH, GOAT,

        MILK, EGGS, HONEY
    }

    public enum FoodPlant
    {
        GARLIC, SCALLIONS, ONION, LEEK, LETTUCE, CELERY, 
        CUCUMBER, RADISH, TURNIP, GRAPES, SQUASH, MELON, 
        PEAS, LENTILS, CHICKPEAS, NUTS, OLIVE_OIL, BARLEY,
        WHEAT, WILD_EDIBLE, NONE
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
        FLAX, WOOD, REEDS, TANNINS
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
        INSTRUMENTS, YARN, LEATHER, PAPYRUS
    }

    public enum War
    {
        CHARIOT, SWORD, AXE, SLING, SHIELD, HELMET, LEATHER_ARMOR
    }

    public enum Smithed
    {
        IRON, SILVER, GOLD, COPPER, TIN, BRONZE, LEAD, NONE
    }

    public enum RawMeat
    {
        RAW_PORK, RAW_BEEF, RAW_MUTTON, RAW_DUCK, RAW_FOWL, 
        RAW_GOOSE, RAW_QUAIL, RAW_GAME, RAW_FISH, RAW_GOAT,
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
        return GetId(Type, SubType, Material);
    }

    public static int GetId(GoodsType type, int subType, int materialType)
    {
        return GetId((int)type, subType, materialType);
    }

    public static int GetId(int type, int subType, int materialType)
    {
        return (type * MAX_GOODS_PER_CATEGORY) + (materialType * MAX_GOODS_PER_MATERIAL) + subType;
    }

    // Reverse of GetId, assumes there are max 1000 goods per type category
    public static Goods FromId(int id, float quantity = 1)
    {
        int type = id / MAX_GOODS_PER_CATEGORY;
        int materialType = id % MAX_GOODS_PER_CATEGORY / MAX_GOODS_PER_MATERIAL;
        int subType = id % MAX_GOODS_PER_CATEGORY % MAX_GOODS_PER_MATERIAL;
        return new Goods((GoodsType)type, subType, quantity, materialType);
    }

    public static string NameFromid(int id)
    {
        Goods temp = FromId(id);
        string name = GetGoodsName(temp.Type, temp.SubType);
        if (temp.Type == GoodsType.TOOL)
        {
            string materialName = "";
            if (temp.Material != (int)ToolMaterial.NONE)
                materialName = Globals.Title(((ToolMaterial)temp.Material).ToString()) + " ";
            return $"{materialName}{name}";
        }
        return name;
    }

    public static int TypeFromId(int id)
    {
        return id / MAX_GOODS_PER_CATEGORY;
    }

    public static int MaterialFromId(int id)
    {
        int type = id / MAX_GOODS_PER_CATEGORY;
        int material = (type % MAX_GOODS_PER_CATEGORY) / MAX_GOODS_PER_MATERIAL;
        return material;
    }

    public static int SubTypeFromid(int id)
    {
        int type = id / MAX_GOODS_PER_CATEGORY;
        int materialType = (type % MAX_GOODS_PER_CATEGORY) / MAX_GOODS_PER_MATERIAL;
        int subType = (id % MAX_GOODS_PER_CATEGORY) % MAX_GOODS_PER_MATERIAL;
        return subType;
    }

    public Goods(GoodsType goodsType, int subType, float quantity = 1, int materialType = 0)
    {
        Type = goodsType;
        SubType = subType;
        Material = materialType;
        Quantity = quantity;
    }

    public Goods(Goods orig)
    {
        Type = orig.Type;
        SubType = orig.SubType;
        Material = orig.Material;
        Quantity = orig.Quantity;
    }

    public Goods(Goods orig, float quantity)
    {
        Type = orig.Type;
        SubType = orig.SubType;
        Material = orig.Material;
        Quantity = quantity;
    }

    public Goods()
    {
        Material = 0;
    }

    public static Goods Create(GoodsType type, int subType, float quantity)
    {
        Goods goods = new() {
            Type = type,
            SubType = subType,
            Quantity = quantity
        };
        return goods;
    }

    public static Goods Create(int id, float quantity)
    {
        Goods goods = new() {
            Type = (GoodsType)TypeFromId(id),
            SubType = SubTypeFromid(id),
            Quantity = quantity
        };
        return goods;
    }

    public List<int> GetMaterials()
    {
        return GetMaterials((int)Type, SubType);
    }

    public static List<int> GetMaterials(int type, int subType)
    {
        List<int> materials = new();
        if (type == (int)GoodsType.TOOL)
        {
            Tool tool = (Tool)subType;
            if (tool == Tool.AXE || tool == Tool.CHISEL || tool == Tool.HAMMER || 
                tool == Tool.HOE || tool == Tool.KNIFE || tool == Tool.PICKAXE ||
                tool == Tool.SAW || tool == Tool.SHOVEL || tool == Tool.SPEAR)
            {        
                foreach (int materialType in Enum.GetValues(typeof(ToolMaterial)))
                    materials.Add(materialType);
                materials.RemoveAt(0);
                return materials;
            }
        }

        // Default Goods.Material is 0
        materials.Add(0);
        return materials;
    }

    public static string GetGoodsName(GoodsType type, int subType)
    {
        string subTypeName = "UNDEFINED";
        switch (type)
        {
            case GoodsType.FOOD_PROCESSED: subTypeName = Enum.GetName(typeof(ProcessedFood), subType); break;
            case GoodsType.FOOD_ANIMAL: subTypeName = Enum.GetName(typeof(FoodAnimal), subType); break;
            case GoodsType.FOOD_PLANT: subTypeName = Enum.GetName(typeof(FoodPlant), subType); break;
            case GoodsType.TOOL: subTypeName = Enum.GetName(typeof(Tool), subType); break;
            case GoodsType.MATERIAL_ANIMAL: subTypeName = Enum.GetName(typeof(MaterialAnimal), subType); break;
            case GoodsType.MATERIAL_PLANT: subTypeName = Enum.GetName(typeof(MaterialPlant), subType); break;
            case GoodsType.MATERIAL_NATURAL: subTypeName = Enum.GetName(typeof(MaterialNatural), subType); break;
            case GoodsType.CRAFT_GOODS: subTypeName = Enum.GetName(typeof(Crafted), subType); break;
            case GoodsType.WAR_GOODS: subTypeName = Enum.GetName(typeof(War), subType); break;
            case GoodsType.SMITHED: subTypeName = Enum.GetName(typeof(Smithed), subType); break;
            case GoodsType.RAW_MEAT: subTypeName = Enum.GetName(typeof(RawMeat), subType); break;
        }
        subTypeName = Globals.Title(subTypeName);
        return subTypeName;
    }

    public override string ToString()
    {
        string typeName = Globals.Title(Enum.GetName(typeof(GoodsType), Type));
        string subTypeName = GetGoodsName(Type, SubType);
        //float value = Globals.Model.Market.GetPrice(GetId()) * Quantity;
        
        if (Type == GoodsType.TOOL)
        {
            string materialName = "";
            if (Material != (int)ToolMaterial.NONE)
                materialName = Globals.Title(((ToolMaterial)Material).ToString()) + " ";
            return $"{materialName}{subTypeName} x{Quantity:0.0}";
            //return $"{materialName}{subTypeName} x{Quantity:0.0} (${value:0.0})";
        }
        return $"{subTypeName} x{Quantity:0.0}";
        //return $"{subTypeName} x{Quantity:0.0} (${value:0.0})";
    }

    public string GetName()
    {
        string subTypeName = GetGoodsName(Type, SubType);
        if (Type == GoodsType.TOOL)
        {
            string materialName = "";
            if (Material != (int)ToolMaterial.NONE)
                materialName = Globals.Title(((ToolMaterial)Material).ToString()) + " ";
            return $"{materialName}{subTypeName}";
        }
        return $"{subTypeName}";
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
    public Goods Cook()
    {
        if (!IsCookable())
            return this;

        if (Type == GoodsType.FOOD_PROCESSED && SubType == (int)Goods.ProcessedFood.FLOUR)
            SubType = (int)Goods.ProcessedFood.BREAD;
        else if (Type == GoodsType.RAW_MEAT)
            Type = GoodsType.FOOD_ANIMAL;
        return this;
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
        return Quantity * Globals.Model.Market.GetPrice(GetId());
    }
}