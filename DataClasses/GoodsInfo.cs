
using System;

public class GoodsInfo
{
    private static GoodsInfo[] Data;

    // Qualities of each type of good
    public float TimeToProduce { get; protected set; }
    public float DecayRate { get; protected set; }
    public float DefaultProductionQuanity { get; protected set; }
    public float UseRate { get; protected set; }
    public int Satiation { get; protected set; }
    public int Experience { get; protected set; }
    public float DefaultPrice { get; protected set; }
    public bool HasMaterial { get; protected set; }
    public int MarketSoftLimit { get; protected set; }

    public GoodsInfo(Goods g) : this(g.Type, g.SubType, g.Material) { }

    public GoodsInfo(GoodsType type, int subType, int materialType)
    {
        // How many seconds to produce 1 unit of this in a task
        TimeToProduce = 1f;

        // How fast the Update() function should destory this
        DecayRate = 0f;

        // How many of these to make in a task
        DefaultProductionQuanity = 20f;

        // How quickly the Use() function should destroy this
        UseRate = 0f;

        // How much hunger is satiated by eating this
        Satiation = 0;

        // How much to increase the odds of a level up when producing this good
        Experience = 1;

        // How much does it cost
        DefaultPrice = 1.5f;

        // Whether the good has a material type
        HasMaterial = false;

        // Try not to produce more if the market already has this much for sale
        MarketSoftLimit = 300;

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
                Satiation = 7;
                UseRate = 1f;
                DefaultPrice = 5f;
                break;
            }
            case GoodsType.FOOD_PLANT:
            {
                DecayRate = 0.002f; 
                Satiation = 5;
                // Scavenged wild plants are less satiating
                if (subType == (int)Goods.FoodPlant.WILD_EDIBLE)
                    Satiation = 2;
                // Don't eat raw wheat or barley
                else if (subType == (int)Goods.FoodPlant.WHEAT || subType == (int)Goods.FoodPlant.BARLEY)
                    Satiation = 0;
                UseRate = 1f;
                DefaultPrice = 2f;
                break;
            }
            case GoodsType.FOOD_PROCESSED: 
            {
                DecayRate = 0.001f; 
                Satiation = 8;
                UseRate = 1f;
                DefaultPrice = 5f;
                
                if (subType == (int)Goods.ProcessedFood.FLOUR)
                {
                    DefaultPrice = 3f;
                    Satiation = 0;
                }
                else if (subType == (int)Goods.ProcessedFood.BREAD)
                {
                    Satiation = 10;
                }
                break;
            }
            case GoodsType.RAW_MEAT:
            {
                DecayRate = 0.01f; 
                DefaultPrice = 4f;
                break;
            }
            case GoodsType.TOOL: 
            {
                UseRate = 0.001f; 
                DefaultPrice = 2f; // need to distinguish stone tools from metal
                if (subType == (int)Goods.Tool.AXE || subType == (int)Goods.Tool.CHISEL ||
                    subType == (int)Goods.Tool.HAMMER || subType == (int)Goods.Tool.HOE ||
                    subType == (int)Goods.Tool.KNIFE || subType == (int)Goods.Tool.PICKAXE ||
                    subType == (int)Goods.Tool.SAW || subType == (int)Goods.Tool.SHOVEL || 
                    subType == (int)Goods.Tool.SPEAR)
                {
                    HasMaterial = true;
                }
                MarketSoftLimit = 50;
                break;
            }
            case GoodsType.CRAFT_GOODS: 
            {
                switch ((Goods.Crafted)subType)
                {
                    // Simple craft goods like bricks and pottery should be cheap
                    // Complex goods like jewelry and instruments should be expensive
                    case Goods.Crafted.BRICKS: DefaultPrice = 1.8f; MarketSoftLimit = 3000; break;
                    case Goods.Crafted.PAPYRUS: DefaultPrice = 1.5f; break;
                    case Goods.Crafted.POTTERY: DefaultPrice = 2f; break;
                    default: DefaultPrice = 5f; break;
                }
                UseRate = 0.001f; 
                break;
            }
            case GoodsType.CONSUMER:
            {
                DefaultPrice = 8f;
                MarketSoftLimit = 50;
                break;
            }
            case GoodsType.WAR_GOODS:
            {
                switch ((Goods.War)subType)
                {
                    case Goods.War.SLING: DefaultPrice = 3f; break;
                    case Goods.War.CHARIOT: DefaultPrice = 12f; break;
                    default: DefaultPrice = 6f; break;
                }
                UseRate = 0.001f;
                break;
            }
            case GoodsType.SMITHED:
            {
                switch ((Goods.Smithed)subType)
                {
                    case Goods.Smithed.COPPER: DefaultPrice = 3f; break;
                    case Goods.Smithed.BRONZE: DefaultPrice = 4.5f; break;
                    case Goods.Smithed.TIN: DefaultPrice = 2f; break;
                    case Goods.Smithed.IRON: DefaultPrice = 4f; break;
                    case Goods.Smithed.LEAD: DefaultPrice = 2f; break;
                    case Goods.Smithed.GOLD: DefaultPrice = 20f; break;
                    case Goods.Smithed.SILVER: DefaultPrice = 10f; break;
                }
                break;
            }
            case GoodsType.MATERIAL_ANIMAL:
            {
                switch ((Goods.MaterialAnimal)subType)
                {
                    case Goods.MaterialAnimal.BONE: DefaultPrice = 1.5f; break;
                    case Goods.MaterialAnimal.HIDE: DefaultPrice = 3.5f; break;
                    case Goods.MaterialAnimal.IVORY: DefaultPrice = 7f; break;
                    case Goods.MaterialAnimal.WOOL: DefaultPrice = 2.5f; break;
                }
                break;
            }
            case GoodsType.MATERIAL_NATURAL:
            {
                switch ((Goods.MaterialNatural)subType)
                {
                    case Goods.MaterialNatural.CLAY: DefaultPrice = 1.1f; MarketSoftLimit = 1000; break;
                    case Goods.MaterialNatural.FLINT: DefaultPrice = 1.2f; break;
                    case Goods.MaterialNatural.LAPIS_LAZULI: DefaultPrice = 7f; break;
                    case Goods.MaterialNatural.MALACHITE: DefaultPrice = 5.5f; break;
                    case Goods.MaterialNatural.OBSIDIAN: DefaultPrice = 6.5f; break;
                    case Goods.MaterialNatural.RAW_COPPER: DefaultPrice = 2.5f; break;
                    case Goods.MaterialNatural.RAW_GOLD: DefaultPrice = 18f; break;
                    case Goods.MaterialNatural.RAW_IRON: DefaultPrice = 3f; break;
                    case Goods.MaterialNatural.RAW_LEAD: DefaultPrice = 1.5f; break;
                    case Goods.MaterialNatural.RAW_SILVER: DefaultPrice = 8f; break;
                    case Goods.MaterialNatural.RAW_TIN: DefaultPrice = 1.5f; break;
                    case Goods.MaterialNatural.SALT: DefaultPrice = 2.5f; break;
                    case Goods.MaterialNatural.SANDSTONE: DefaultPrice = 1.6f; MarketSoftLimit = 3000; break;
                    case Goods.MaterialNatural.STONE: DefaultPrice = 1.1f; MarketSoftLimit = 3000; break;
                }
                break;
            }
            case GoodsType.MATERIAL_PLANT:
            {
                switch ((Goods.MaterialPlant)subType)
                {
                    case Goods.MaterialPlant.WOOD: DefaultPrice = 1.8f; MarketSoftLimit = 2000; break;
                    case Goods.MaterialPlant.FLAX: DefaultPrice = 2f; break;
                    case Goods.MaterialPlant.REEDS: DefaultPrice = 1f; break;
                }
                break;
            }
        }

        // Value units are in seconds, so these are the same thing at init time
        TimeToProduce = DefaultPrice;

        // Make food faster to produce
        if (Goods.IsCookable(type, subType))
            TimeToProduce /= 2;

        if (type == GoodsType.FOOD_PLANT && subType == (int)Goods.FoodPlant.WILD_EDIBLE)
            TimeToProduce *= 2;

        // Make game take longer to make hunting take longer than farming animals
        if (type == GoodsType.RAW_MEAT && subType == (int)Goods.RawMeat.RAW_GAME)
            TimeToProduce *= 1.5f;

        // Try to limit seconds per production task
        DefaultProductionQuanity = 20f / TimeToProduce;

        // If it takes a long time to use the object, only produce a few at a time and grant bonus skill xp
        if (UseRate < 0.005f && DecayRate == 0f)
        {
            if (type == GoodsType.TOOL)
                DefaultProductionQuanity = 1;
            else
                DefaultProductionQuanity = 4;
            Experience = 5;
        }

        if (type == GoodsType.TOOL)
        {
            // Stone worth 30% more than wood, copper 30% more than stone, etc.
            DefaultPrice = 1f * (float)Math.Pow(1.3f, materialType); 

            // Each tool tier reduces use rate by 5%
            UseRate = 0.07f * (float)Math.Pow(0.95f, materialType);

            if (materialType == (int)ToolMaterial.STONE)
                Experience = 1;
        }
    }

    public static void Init()
    {
        Array goodsTypes = Enum.GetValues(typeof(GoodsType));
        Data = new GoodsInfo[goodsTypes.Length * Goods.MAX_GOODS_PER_CATEGORY];
        for (int id = 0; id < Data.Length; id++) 
        {
            Data[id] = new GoodsInfo(
                (GoodsType)Goods.TypeFromId(id), 
                Goods.SubTypeFromid(id),
                Goods.MaterialFromId(id));
        }
    }

    // Get the time it takes to produce 1f unit of the good
    public static float GetTime(Goods g)
    {
        return Data[g.GetId()].TimeToProduce;
    }

    public static float GetTime(int goodsId)
    {
        return Data[goodsId].TimeToProduce;
    }

    public static float GetDecayRate(Goods g)
    {
        return Data[g.GetId()].DecayRate;
    }

    public static float GetDefaultProductionQuantity(Goods g)
    {
        return Data[g.GetId()].DefaultProductionQuanity;
    }

    public static float GetUseRate(Goods g)
    {
        return Data[g.GetId()].UseRate;
    }

    public static int GetSatiation(Goods g)
    {
        return Data[g.GetId()].Satiation;
    }

    public static int GetSatiation(int id)
    {
        return Data[id].Satiation;
    }

    public static int GetExperience(Goods g)
    {
        return Data[g.GetId()].Experience;
    }

    public static float GetDefaultPrice(Goods g)
    {
        return Data[g.GetId()].DefaultPrice;
    }

    public static float GetDefaultPrice(int goodsId)
    {
        if (goodsId < Data.Length)
            return Data[goodsId].DefaultPrice;
        return 0f;
    }

    public static bool GetHasMaterial(int goodsId)
    {
        if (goodsId < Data.Length)
            return Data[goodsId].HasMaterial;
        return false;
    }

    public static int GetMarketSoftLimit(int goodsId)
    {
        if (goodsId < Data.Length)
            return Data[goodsId].MarketSoftLimit;
        return 300;
    }
}