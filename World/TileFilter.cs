using System;

public class TileFilter
{
    public TileType FilterTileType;
    public BuildingType FilterBuildingType;
    public BuildingSubType FilterBuildingSubType;
    public bool FindResource;

    public TileFilter(
        TileType tileType = TileType.NONE,
        BuildingType buildingType = BuildingType.NONE,
        BuildingSubType buildingSubType = BuildingSubType.NONE,
        bool findResource = false)
    {
        FilterTileType = tileType;
        FilterBuildingType = buildingType;
        FilterBuildingSubType = buildingSubType;
        FindResource = findResource;
    }

    // Returns a tile if searching by tile type and one matches
    // Returns a building if searching by building type and one matches
    // returns null if no match is found
    public virtual Object Match(Tile t)
    {
        if (t == null)
            return null;

        if (FilterTileType != TileType.NONE)
        {
            // TileType filter may allow multiple types, e.g. TileType.PIG | TileType.COW
            if (!FilterTileType.HasFlag(t.Type))
               return null;
            if (FindResource && !t.HasResource())
                return null;
        }

        if (FilterBuildingType == BuildingType.NONE) 
            return t;

        foreach (Building b in t.Buildings)
        {
            // Building is under construction
            if (b.BuildProgress < 1f)
                continue;

            // Continue, wrong type
            if (!Building.EquivalentType(b.Type, FilterBuildingType))
                continue;

            // Continue, building is already at capacity
            if (b.CurrentUsers.Count >= b.MaxUsers)
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

    public string Describe()
    {
        string description = "a valid ";
        if (FilterBuildingType != BuildingType.NONE)
        {
            description = Globals.Title(FilterBuildingType.ToString());
            if (FilterBuildingSubType != BuildingSubType.NONE)
                description += " (" + Globals.Title(FilterBuildingSubType.ToString()) + ")";
        }

        if (FilterTileType != TileType.NONE)
        {
            string tileDesc = Globals.Title(FilterTileType.ToString());
            if (FilterBuildingType != BuildingType.NONE)
                description += " on a ";
            description += tileDesc + " tile";
        }

        return description;
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

public class TileFilterHover : TileFilter
{
    public override object Match(Tile t)
    {
        if (t == null)
            return null;
        if (t.ContainsSimple(InputManager.MousePos))
            return t;
        return null;
    }
}