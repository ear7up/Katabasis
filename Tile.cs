using System.Collections;

public class Tile
{
    public Sprite BaseSprite { get; protected set; }
    public Sprite TileFeatureSprite { get; protected set; }

    // Every tile has a resource stockpile that can be used for production/consumption
    public Hashtable Stockpile;

    public enum Cardinal
    {
        NE, SE, NW, SW
    }

    public static Cardinal GetOppositeDirection(Cardinal direction)
    {
        switch (direction)
        {
            case Cardinal.NE: return Cardinal.SW;
            case Cardinal.SE: return Cardinal.NW;
            case Cardinal.NW: return Cardinal.SE;
            case Cardinal.SW: return Cardinal.NE;
        }
        return Cardinal.NE;
    }

    public Tile[] neighbors { get; set; }

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