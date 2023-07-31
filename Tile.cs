using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Tile
{
    public const int MAX_POP = 8;

    public Sprite BaseSprite { get; protected set; }
    public Sprite TileFeatureSprite { get; protected set; }
    public int Population { get; set; }

    // Every tile has a resource stockpile that can be used for production/consumption
    public Hashtable Stockpile;

    public enum Cardinal
    {
        // Do not reorder
        NE, SE, SW, NW
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

    public static int GetNextDirection(int direction)
    {
        return (direction + 1) % 4;
    }

    public Tile[] neighbors { get; set; }

    public Tile(Vector2 position, Texture2D baseTexture, Texture2D tileFeatureTexture)
    {
        neighbors = new Tile[4];
        Population = 0;

        BaseSprite = new Sprite(baseTexture, position);
        if (TileFeatureSprite != null)
        {
            TileFeatureSprite = new Sprite(tileFeatureTexture, position);
        }

        Stockpile = new();
    }

    public Vector2 GetPosition()
    {
        return BaseSprite.Position;
    }

    public Vector2 GetOrigin()
    {
        return BaseSprite.Origin;
    }

    public void SetPosition(Vector2 newPos)
    {
        BaseSprite.Position = newPos;
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

    // Breadth-first search for a tile based on critera in TileFilter.Match
    // Max depth defaults to 50
    public static Tile FindTile(Tile start, TileFilter f, int maxDepth = 50)
    {
        Stack<Tile> searchStack = new();
        searchStack.Push(start);

        int n = -1;
        while (searchStack.Count > 0)
        {
            Tile t = searchStack.Pop();

            if (n++ / 4 > maxDepth)
            {
                return null;
            }

            // Randomize the search order so that it's not biased in one direction
            foreach (int i in Enumerable.Range(0, t.neighbors.Length).OrderBy(x => Globals.Rand.Next()))
            {
                Tile neighbor = t.neighbors[i];
                if (f.Match(neighbor))
                {
                    return neighbor;
                }
                searchStack.Push(neighbor);
            }
        }

        return null;
    }
}