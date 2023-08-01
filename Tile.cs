using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum TileType
{
    DESERT,
    RIVER,
    FOREST,
    OASIS,
    HILLS,
    ANIMAL,
    DORMANT_VOLCANO,
    NONE
}

public enum Cardinal
{
    // Do not reorder
    NE, SE, SW, NW
}

public class Tile
{
    public TileType Type;
    
    public const int MAX_POP = 8;
    public int Population { get; set; }

    public Sprite BaseSprite { get; protected set; }
    public Sprite TileFeatureSprite { get; protected set; }
    
    public Tile[] Neighbors { get; set; }
    
    public List<Building> Buildings { get; set; }

    public const float MIN_SOIL_QUALITY = 0.2f;
    public const float MAX_SOIL_QUALITY = 0.6f;
    public const float RIVER_SOIL_QUALITY_BONUS = 0.2f;
    public float SoilQuality { get; set; }
    
    // Every tile has a resource stockpile that can be used for production/consumption
    public Hashtable Stockpile;

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

    public Tile(Vector2 position, Texture2D baseTexture, Texture2D tileFeatureTexture)
    {
        Type = TileType.DESERT;
        Neighbors = new Tile[4];
        Population = 0;
        Buildings = new();

        BaseSprite = new Sprite(baseTexture, position);
        if (TileFeatureSprite != null)
        {
            TileFeatureSprite = new Sprite(tileFeatureTexture, position);
        }

        Stockpile = new();
        SoilQuality = Globals.Rand.NextFloat(MIN_SOIL_QUALITY, MAX_SOIL_QUALITY);
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

    // Takes goods from the stockpile, sets quantity to the amount taken (may be less than requested)
    public void TakeFromStockpile(Goods goods)
    {
        Goods available = (Goods)Stockpile[goods.GetId()];
        if (available != null)
            goods.Quantity = available.Take(goods.Quantity);
        else
            goods.Quantity = 0;
    }

    // Add quantity to stockpile if the good already exists, otherwise adds the good in the specified quantity
    public void AddToStockpile(Goods goods)
    {
        Goods current = (Goods)Stockpile[goods.GetId()];
        if (current != null)
            current.Quantity += goods.Quantity;
        else
            Stockpile.Add(goods.GetId(), new Goods(goods));    
        goods.Quantity = 0;
    }

    public void AddBuilding(Building b)
    {
        Buildings.Add(b);
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
        foreach (Building b in Buildings)
        {
            Vector2 bpos = b.Sprite.Position;
            bpos.Y -= 150;
            b.Sprite.Position = bpos;
        }
    }

    public void Unhighlight()
    {
        Vector2 pos = BaseSprite.Position;
        pos.Y += 150;
        BaseSprite.Position = pos;

        foreach (Building b in Buildings)
        {
            Vector2 bpos = b.Sprite.Position;
            bpos.Y += 150;
            b.Sprite.Position = bpos;
        }
    }

    // Breadth-first search for a tile based on critera in TileFilter.Match
    // Max depth defaults to 50
    public static Object Find(Tile start, TileFilter f, int maxDepth = 50)
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
            foreach (int i in Enumerable.Range(0, t.Neighbors.Length).OrderBy(x => Globals.Rand.Next()))
            {
                Tile neighbor = t.Neighbors[i];
                Object match = f.Match(neighbor);
                if (match != null)
                    return match;
                searchStack.Push(neighbor);
            }
        }

        return null;
    }
}