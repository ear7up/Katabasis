using System;

public class BuildingInfo
{
    public static BuildingInfo[] Data;

    public float TimeToProduce;
    public int MaxUsers;

    public BuildingInfo(BuildingType type)
    {
        TimeToProduce = 30f;
        switch (type)
        {
            case BuildingType.MARKET: MaxUsers = 999999; break;
            case BuildingType.WOOD_HOUSE: MaxUsers = 8; break;
            case BuildingType.STONE_HOUSE: MaxUsers = 8; break;
            case BuildingType.BARRACKS: MaxUsers = 50; break;
            default: MaxUsers = 4; break;
        }
    }

    public static void Init()
    {
        Array buildingTypes = Enum.GetValues(typeof(BuildingType));
        Data = new BuildingInfo[buildingTypes.Length];

        foreach (BuildingType type in buildingTypes)
        {
            Data[(int)type] = new BuildingInfo(type);
        }
    }

    public static float GetBuildTime(BuildingType buildingType)
    {
        return Data[(int)buildingType].TimeToProduce;
    }

    public static int GetMaxUsers(BuildingType buildingType)
    {
        return Data[(int)buildingType].MaxUsers;
    }
}