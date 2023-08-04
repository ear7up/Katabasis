using System;

public class BuildingInfo
{
    public static BuildingInfo[] Data;

    public float TimeToProduce;

    public BuildingInfo()
    {
        TimeToProduce = 10f;
    }

    public static void Init()
    {
        Array buildingTypes = Enum.GetValues(typeof(BuildingType));
        Data = new BuildingInfo[buildingTypes.Length];

        foreach (int type in buildingTypes)
        {
            Data[type] = new BuildingInfo();
        }
    }

    public static float GetBuildTime(BuildingType buildingType)
    {
        return Data[(int)buildingType].TimeToProduce;
    }
}