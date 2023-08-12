using System;
using System.Linq;

public enum MineralType
{
    IRON,
    COPPER,
    SILVER,
    GOLD,
    LEAD,
    MALACHITE,
    LAPIS_LAZULI,
    TIN,
    SALT,
    NONE
}

public class MineralInfo
{
    private static MineralInfo[] Data;

    public Color MineralColor;
    public MineralType Type;
    public float Rarity;

    public MineralInfo(MineralType type)
    {
        Type = type;

        // These should add up to about 1.0; the remainder will be added to copper
        switch (type)
        {
            case MineralType.GOLD: 
            {
                MineralColor = Color.Goldenrod;
                Rarity = 0.01f; 
                break;
            }
            case MineralType.SILVER: 
            {
                MineralColor = Color.Silver;
                Rarity = 0.03f; 
                break;
            }
            case MineralType.MALACHITE: 
            {
                MineralColor = Color.LightSeaGreen;
                Rarity = 0.05f; 
                break;
            }
            case MineralType.LAPIS_LAZULI: 
            {
                MineralColor = Color.RoyalBlue;
                Rarity = 0.07f; 
                break;
            }
            case MineralType.IRON: 
            {
                MineralColor = Color.LightPink;
                Rarity = 0.15f; 
                break;
            }
            case MineralType.COPPER: 
            {
                MineralColor = Color.Tomato;
                Rarity = 0.2f; 
                break;
            }
            case MineralType.LEAD: 
            {
                MineralColor = Color.DimGray;
                Rarity = 0.1f; 
                break;
            }
            case MineralType.TIN: 
            {
                MineralColor = Color.Gainsboro;
                Rarity = 0.1f; 
                break;
            }
            case MineralType.SALT:
            {
                MineralColor = Color.White;
                Rarity = 0.15f; 
                break;  
            }
            case MineralType.NONE: 
            {
                MineralColor = Color.White;
                Rarity = 0.1f; 
                break;
            }
            default: 
            {
                MineralColor = Color.White;
                Rarity = 0.10f; 
                break;
            }
        }
    }

    public static MineralType Random()
    {
        float sum = 0f;
        float r = Globals.Rand.NextFloat(0f, 1f);
        foreach (MineralInfo info in Data.OrderBy(x => x.Rarity))
        {
            sum += info.Rarity;
            if (r < sum)
                return info.Type;
        }
        return MineralType.COPPER;
    }

    public static void Init()
    {
        Array mineralTypes = Enum.GetValues(typeof(MineralType));
        Data = new MineralInfo[mineralTypes.Length];

        foreach (MineralType type in mineralTypes)
            Data[(int)type] = new(type);
    }

    public static Color GetColor(MineralType type)
    {
        return Data[(int)type].MineralColor;
    }
}