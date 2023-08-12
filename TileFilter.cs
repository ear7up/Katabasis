using System;

public class TileFilter
{
    public TileType FilterTileType;
    public BuildingType FilterBuildingType;
    public BuildingSubType FilterBuildingSubType;

    public TileFilter(
        TileType tileType = TileType.NONE,
        BuildingType buildingType = BuildingType.NONE,
        BuildingSubType buildingSubType = BuildingSubType.NONE)
    {
        FilterTileType = tileType;
        FilterBuildingType = buildingType;
        FilterBuildingSubType = buildingSubType;
    }

    // Returns a tile if searching by tile type and one matches
    // Returns a building if searching by building type and one matches
    // returns null if no match is found
    public virtual Object Match(Tile t)
    {
        if (FilterTileType != TileType.NONE && t.Type != FilterTileType)
            return null;
        if (FilterBuildingType == BuildingType.NONE) 
            return t;

        foreach (Building b in t.Buildings)
        {
            // Continue, wrong type
            if (!Building.EquivalentType(b.Type, FilterBuildingType))
                continue;

            // Continue, building is already at capacity
            if (b.CurrentUsers >= b.MaxUsers)
                continue;

            // Right type, no subtype required
            if (FilterBuildingSubType == BuildingSubType.NONE)
                return b;

            // Right type and subtype
            if (b.SubType == FilterBuildingSubType)
                return b;
        }
        return null;
    }
}

// Match suitable home tiles (population < MAX)
public class TileFilterHome : TileFilter
{
    public override Object Match(Tile t)
    {
        if (t != null && t.Population < Tile.MAX_POP)
            return t;
        return null;
    }
}