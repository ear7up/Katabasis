using System;

public abstract class TileFilter
{
    public abstract Object Match(Tile t);
}

// Match suitable home tiles (population < MAX)
public class TileFilterHome : TileFilter
{
    public override Object Match(Tile t)
    {
        if (t != null && t.Population < Tile.MAX_POP)
        {
            return t;
        }
        return null;
    }
}

// Find a building
public class TileFilterBuilding : TileFilter
{
    public BuildingType BuildingType;
    public TileFilterBuilding(BuildingType buildingType)
    {
        BuildingType = buildingType;
    }

    public override Object Match(Tile t)
    {
        foreach (Building b in t.Buildings)
        {
            if (Building.EquivalentType(b.Type, BuildingType) && b.CurrentUsers < b.MaxUsers)
            {
                return b;
            }
        }
        return null;
    }
}

// Find a building
public class TileFilterBuildingOnTile : TileFilter
{
    public BuildingType BuildingType;
    public TileType TileType;
    public TileFilterBuildingOnTile(BuildingType buildingType, TileType tileType)
    {
        BuildingType = buildingType;
        TileType = tileType;
    }

    public override Object Match(Tile t)
    {
        if (t.Type != TileType)
            return null;

        foreach (Building b in t.Buildings)
            if (Building.EquivalentType(b.Type, BuildingType) && b.CurrentUsers < b.MaxUsers)
                return new BuildingAndTile(b, t);

        return null;
    }
}

public class TileFilterByType : TileFilter
{
    public TileType TileType;
    public TileFilterByType(TileType tileType)
    {
        TileType = tileType;
    }

    public override Object Match(Tile t)
    {
        if (t.Type == TileType)
        {
            return t;
        }
        return null;
    }
}