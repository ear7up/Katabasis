using System.Collections;

public class Tile
{
    public Sprite BaseSprite { get; protected set; }
    public Sprite TileFeatureSprite { get; protected set; }

    // Every tile has a resource stockpile that can be used for production/consumption
    public Hashtable Stockpile;

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

        Stockpile = new();
    }

    public override string ToString()
    {
        return $"Tile(pos={BaseSprite.Position})";
    }

    // Take quantity of specific good type from stockpile, or whatever is left if there's not enough
    public int TakeFromStockpile(int subType, int quantity)
    {
        Goods goods = (Goods)Stockpile[subType];
        return goods != null ? goods.Take(quantity) : 0;
    }

    // Add quantity to stockpile if the good already exists, otherwise adds the good in the specified quantity
    public void AddToStockpile(Goods.GoodsType type, int subType, int quantity)
    {
        Goods goods = (Goods)Stockpile[subType];
        if (goods != null)
            goods.Quantity += quantity;
        else
            Stockpile.Add(subType, new Goods(type, subType, quantity));
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