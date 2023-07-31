public abstract class TileFilter
{
    public abstract bool Match(Tile t);
}

// Match suitable home tiles (population < MAX)
public class TileFilterHome : TileFilter
{
    public override bool Match(Tile t)
    {
        return t != null && t.Population < Tile.MAX_POP;
    }
}