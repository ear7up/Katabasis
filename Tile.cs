public class Tile
{
    public Sprite BaseSprite { get; protected set; }
    public Sprite TileFeatureSprite { get; protected set; }

    public enum Cardinal
    {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3
    }

    public static Cardinal GetOppositeDirection(Cardinal direction)
    {
        switch (direction)
        {
            case Cardinal.NORTH: return Cardinal.SOUTH;
            case Cardinal.EAST: return Cardinal.WEST;
            case Cardinal.SOUTH: return Cardinal.NORTH;
            case Cardinal.WEST: return Cardinal.EAST;
        }
        return Cardinal.NORTH;
    }

    public Tile[] neighbors { get; protected set; }

    public Tile(Vector2 position, Texture2D baseTexture, Texture2D tileFeatureTexture)
    {
        neighbors = new Tile[4];

        BaseSprite = new Sprite(baseTexture, position);
        if (TileFeatureSprite != null)
        {
            TileFeatureSprite = new Sprite(tileFeatureTexture, position);
        }
    }

    public void Draw()
    {
        BaseSprite.Draw();
        if (TileFeatureSprite != null)
        {
            TileFeatureSprite.Draw();
        }
    }

    public void Highlight()
    {
        Vector2 pos = BaseSprite.Position;
        pos.Y -= 150;
        BaseSprite.Position = pos;
    }

    public void Unhighlight()
    {
        Vector2 pos = BaseSprite.Position;
        pos.Y += 150;
        BaseSprite.Position = pos;
    }
}