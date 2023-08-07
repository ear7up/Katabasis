using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum TileType
{
    DESERT,
    RIVER,
    FOREST,
    VEGETATION,
    OASIS,
    HILLS,
    // Only add new animals between ANIMAL and WILD_ANIMAL
    ANIMAL, PIG, COW, SHEEP, DUCK, DONKEY, GOAT, GAZELLE, GIRAFFE, ELEPHANT, FOWL, GOOSE, QUAIL, WILD_ANIMAL,
    DORMANT_VOLCANO,
    CAMP,
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
    public Player Owner;
    
    public const int MAX_POP = 8;
    public int Population { get; set; }

    public bool DrawBase;
    public Sprite BaseSprite { get; protected set; }
    public Sprite BulidingSprite;
    
    public Tile[] Neighbors { get; set; }
    
    public const int MAX_BUILDINGS = 6;
    public List<Building> Buildings { get; set; }

    public const float MIN_SOIL_QUALITY = 0.2f;
    public const float MAX_SOIL_QUALITY = 0.6f;
    public const float RIVER_SOIL_QUALITY_BONUS = 0.2f;
    public float SoilQuality { get; set; }
    
    // Every tile has a resource stockpile that can be used for production/consumption
    public Stockpile Stockpile;

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

    public Tile(TileType type, Vector2 position, Texture2D baseTexture, Texture2D tileFeatureTexture)
    {
        Type = type;
        Owner = null;
        Neighbors = new Tile[4];
        Population = 0;
        Buildings = new();
        DrawBase = true;

        BaseSprite = new Sprite(baseTexture, position);
        if (BulidingSprite != null)
            BulidingSprite = new Sprite(tileFeatureTexture, position);

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

    public void Draw()
    {
        if (DrawBase)
            BaseSprite.Draw();

        if (BulidingSprite != null)
            BulidingSprite.Draw();
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

    public void AddBuilding(Building building)
    {
        Buildings.Add(building);
        BulidingSprite = building.Sprite;
        BulidingSprite.Position = BaseSprite.Position;

        // Replace hills with mines otherwise the overlap looks bad
        if (building.Type == BuildingType.MINE)
            DrawBase = false;
        
        // Adding a ranch converts the tile from WILD_ANIMAL to ANIMAL (no hunting)
        if (building.Type == BuildingType.RANCH && Type == TileType.WILD_ANIMAL)
            Type = ((TileAnimal)this).AnimalType;
    }

    public virtual void Update()
    {
        foreach (Building b in Buildings)
            b.Update();
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
                return null;

            if (t == null)
                continue;

            // Randomize the search order so that it's not biased in one direction
            foreach (int i in Enumerable.Range(0, t.Neighbors.Length).OrderBy(x => Globals.Rand.Next()))
            {
                Tile neighbor = t.Neighbors[i];
                if (neighbor != null)
                {
                    Object match = f.Match(neighbor);
                    if (match != null)
                        return match;
                }
                searchStack.Push(neighbor);
            }
        }
        return null;
    }
}