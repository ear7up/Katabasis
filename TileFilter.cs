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

// Find a tile with a market in it
public class TileFilterMarket : TileFilter
{
    public override Object Match(Tile t)
    {
        foreach (Building b in t.Buildings)
        {
            if (b.GetType() == typeof(Market))
            {
                return b;
            }
        }
        return null;
    }
}