
using System;

public class GoodsInfo
{
    private static GoodsInfo[][] Data;

    // Qualities of each type of good
    public float TimeToProduce { get; protected set; }
    public float DecayRate { get; protected set; }
    public int DefaultProductionQuanity { get; protected set; }
    public float UseRate { get; protected set; }
    public int Satiation { get; protected set; }
    public int Experience { get; protected set; }

    public GoodsInfo(Goods g) : this(g.Type, g.SubType) { }

    public GoodsInfo(GoodsType type, int subType)
    {
        // How many seconds to produce 1 unit of this in a task
        TimeToProduce = 1f;

        // How fast the Update() function should destory this
        DecayRate = 0f;

        // How many of these to make in a task
        DefaultProductionQuanity = 20;

        // How quickly the Use() function should destroy this
        UseRate = 0f;

        // How much hunger is satiated by eating this
        Satiation = 0;

        // How much to increase the odds of a level up when producing this good
        Experience = 1;

        // Set broad defaults by type (can be overriden later in Init function)
        switch (type)
        {
            // Only food goods naturally decay
            case GoodsType.FOOD_ANIMAL: 
            {
                if ((Goods.FoodAnimal)subType == Goods.FoodAnimal.HONEY)
                    DecayRate = 0f;
                else
                    DecayRate = 0.005f;
                Satiation = 10;
                UseRate = 1f;
                break;
            }
            case GoodsType.FOOD_PLANT:
            {
                DecayRate = 0.002f; 
                Satiation = 5;
                // Scavenged wild plants are less satiating
                if (subType == (int)Goods.FoodPlant.WILD_EDIBLE)
                    Satiation = 2;
                UseRate = 1f;
                break;
            }
            case GoodsType.FOOD_PROCESSED: 
            {
                DecayRate = 0.001f; 
                Satiation = 8;
                UseRate = 1f;
                break;
            }
            case GoodsType.RAW_MEAT: DecayRate = 0.01f; break;

            case GoodsType.TOOL: UseRate = 0.001f; break;
            case GoodsType.CRAFT_GOODS: UseRate = 0.001f; break;
            case GoodsType.WAR_GOODS: UseRate = 0.001f; break;
        }

        // If it takes a long time to use the object, only produce a few at a time and grant bonus skill xp
        if (UseRate < 0.005f && DecayRate == 0f)
        {
            DefaultProductionQuanity = 4;
            Experience = 5;
        }
    }

    public static void Init()
    {
        Array goodsTypes = Enum.GetValues(typeof(GoodsType));
        Data = new GoodsInfo[goodsTypes.Length][];

        foreach (int type in goodsTypes)
        {
            Type x = typeof(Goods.ProcessedFood);
            switch ((GoodsType)type)
            {
                case GoodsType.FOOD_PROCESSED: x = typeof(Goods.ProcessedFood); break;
                case GoodsType.FOOD_ANIMAL: x = typeof(Goods.FoodAnimal); break;
                case GoodsType.FOOD_PLANT: x = typeof(Goods.FoodPlant); break;
                case GoodsType.TOOL: x = typeof(Goods.Tool); break;
                case GoodsType.MATERIAL_ANIMAL: x = typeof(Goods.MaterialAnimal); break;
                case GoodsType.MATERIAL_PLANT: x = typeof(Goods.MaterialPlant); break;
                case GoodsType.MATERIAL_NATURAL: x = typeof(Goods.MaterialNatural); break;
                case GoodsType.CRAFT_GOODS: x = typeof(Goods.Crafted); break;
                case GoodsType.WAR_GOODS: x = typeof(Goods.War); break;
                case GoodsType.SMITHED: x = typeof(Goods.Smithed); break;
                case GoodsType.RAW_MEAT: x = typeof(Goods.RawMeat); break;
            }

            Data[type] = new GoodsInfo[Enum.GetValues(x).Length];
            
            foreach (int subType in Enum.GetValues(x))
            {
                Data[type][subType] = new GoodsInfo((GoodsType)type, subType);
            }
        }
    }

    // Get the time it takes to produce 1f unit of the good
    public static float GetTime(Goods g)
    {
        return Data[(int)g.Type][g.SubType].TimeToProduce;
    }

    public static float GetDecayRate(Goods g)
    {
        return Data[(int)g.Type][g.SubType].DecayRate;
    }

    public static int GetDefaultProductionQuantity(Goods g)
    {
        return Data[(int)g.Type][g.SubType].DefaultProductionQuanity;
    }

    public static float GetUseRate(Goods g)
    {
        return Data[(int)g.Type][g.SubType].UseRate;
    }

    public static int GetSatiation(Goods g)
    {
        return Data[(int)g.Type][g.SubType].Satiation;
    }

    public static int GetExperience(Goods g)
    {
        return Data[(int)g.Type][g.SubType].Experience;
    }

    // Skill modifiers?
}