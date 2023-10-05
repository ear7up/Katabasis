using System;

public class BuildingInfo
{
    public static BuildingInfo[] Data;

    public float TimeToProduce;
    public int MaxUsers;

    public BuildingInfo(int buildingId)
    {
        BuildingType type = Building.TypeFromId(buildingId);

        TimeToProduce = 30f;
        switch (type)
        {
            case BuildingType.MARKET: MaxUsers = 999999; break;
            case BuildingType.HOUSE: MaxUsers = 8; break;
            case BuildingType.BARRACKS: MaxUsers = 50; break;
            case BuildingType.SMITHY: MaxUsers = 2; break;
            case BuildingType.FORGE: MaxUsers = 2; break;
            case BuildingType.PYRAMID: TimeToProduce = 120f; break;
            default: MaxUsers = 4; break;
        }
    }

    public static void Init()
    {
        Array buildingTypes = Enum.GetValues(typeof(BuildingType));
        Array subTypes = Enum.GetValues(typeof(BuildingSubType));

        Data = new BuildingInfo[buildingTypes.Length * Building.MAX_BUILDING_SUBTYPES];

        foreach (BuildingType type in buildingTypes)
            foreach (BuildingSubType subType in subTypes)
                Data[Building.GetId(type, subType)] = new BuildingInfo(Building.GetId(type, subType));
    }

    public static float GetBuildTime(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
    {
        return Data[Building.GetId(buildingType, subType)].TimeToProduce;
    }

    public static float GetBuildTime(int id)
    {
        return Data[id].TimeToProduce;
    }

    public static int GetMaxUsers(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
    {
        return Data[(int)Building.GetId(buildingType, subType)].MaxUsers;
    }

    public static int GetMaxUsers(int id)
    {
        return Data[id].MaxUsers;
    }
}