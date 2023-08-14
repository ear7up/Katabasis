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
    ANIMAL, PIG, COW, SHEEP, DUCK, DONKEY, GOAT, GAZELLE, /*GIRAFFE,*/ ELEPHANT, FOWL, GOOSE, QUAIL, WILD_ANIMAL,
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

    public const int HIGHLIGHT_HEIGHT = 30;
    
    public const int MAX_POP = 32;
    public int Population { get; set; }

    public bool DrawBase;
    public Sprite BaseSprite { get; protected set; }
    public Sprite BuildingSprite;
    
    public Tile[] Neighbors { get; set; }
    
    public const int MAX_BUILDINGS = 6;
    public List<Building> Buildings { get; set; }

    public const float MIN_SOIL_QUALITY = 0.3f;
    public const float MAX_SOIL_QUALITY = 0.6f;
    public const float RIVER_SOIL_QUALITY_BONUS = 0.25f;
    public const float VEGETATION_SOIL_QUALITY_BONUS = 0.1f;
    public float SoilQuality { get; set; }

    public MineralType Minerals;

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

    public Tile(TileType type, Vector2 position, Texture2D baseTexture, Texture2D buildingTexture)
    {
        Type = type;
        Owner = null;
        Neighbors = new Tile[4];
        Population = 0;
        Buildings = new();
        DrawBase = true;

        BaseSprite = new Sprite(baseTexture, position);
        if (BuildingSprite != null)
            BuildingSprite = new Sprite(buildingTexture, position);
        
        SoilQuality = Globals.Rand.NextFloat(MIN_SOIL_QUALITY, MAX_SOIL_QUALITY);
        if (type == TileType.VEGETATION)
            SoilQuality += VEGETATION_SOIL_QUALITY_BONUS;

        Minerals = MineralType.NONE;
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

    public bool NeighborHasDifferentOwner(Cardinal direction)
    {
        return Neighbors[(int)direction] == null || Neighbors[(int)direction].Owner != Owner;
    }

    // Draw tile with borders
    public void DrawOwnedTile()
    {
        if (DrawBase)
        {
            Color temp = BaseSprite.SpriteColor;
            BaseSprite.SpriteColor = Color.SteelBlue;
            BaseSprite.Draw();

            BaseSprite.ScaleDown(0.02f);
            BaseSprite.SpriteColor = temp;
            BaseSprite.Draw();
            BaseSprite.ScaleUp(0.02f);
        }

        if (BuildingSprite != null)
        {
            if (DrawBase)
            {
                BuildingSprite.Draw();    
            }
            else
            {
                Color temp = BuildingSprite.SpriteColor;
                BuildingSprite.SpriteColor = Color.SteelBlue;
                BuildingSprite.Draw();

                BuildingSprite.ScaleDown(0.02f);
                BuildingSprite.SpriteColor = temp;
                BuildingSprite.Draw();
                BuildingSprite.ScaleUp(0.02f);
            }
        }
    }

    public enum DisplayType
    {
        SOIL_QUALITY,
        MINERALS,
        PLACING_RANCH,
        BUYING_TILE,
        DEFAULT
    }

    public void Draw(DisplayType displayType)
    {
        if (displayType == DisplayType.SOIL_QUALITY)
        {
            float max = MAX_SOIL_QUALITY + (2 * RIVER_SOIL_QUALITY_BONUS) + VEGETATION_SOIL_QUALITY_BONUS;
            Color c = new Color(0.25f, MathHelper.Clamp((SoilQuality / max) + 0.2f, 0.5f, 1.0f), 0.25f);
            BaseSprite.SpriteColor = c;
            if (BuildingSprite != null)
                BuildingSprite.SpriteColor = BaseSprite.SpriteColor;
        }
        else if (displayType == DisplayType.MINERALS)
        {
            BaseSprite.SpriteColor = MineralInfo.GetColor(Minerals);
            if (BuildingSprite != null)
                BuildingSprite.SpriteColor = BaseSprite.SpriteColor;
        }
        else if (displayType == DisplayType.PLACING_RANCH)
        {
            if (Type == TileType.WILD_ANIMAL)
                BaseSprite.SpriteColor = Color.LightGreen;
        }
        else if (displayType == DisplayType.BUYING_TILE)
        {
            foreach (Tile neighbor in Neighbors)
            {
                if (neighbor != null && Owner == null && neighbor.Owner != null)
                {
                    BaseSprite.SpriteColor = Color.SteelBlue;
                    break;
                }
            }
        }
        else
        {
            BaseSprite.SpriteColor = Color.White;
            if (BuildingSprite != null)
                BuildingSprite.SpriteColor = Color.White;
        }

        if (Owner != null && Config.ShowBorders)
        {
            DrawOwnedTile();
            return;
        }

        if (DrawBase)
            BaseSprite.Draw();
        if (BuildingSprite != null)
            BuildingSprite.Draw();
    }

    public void Highlight()
    {
        Vector2 pos = BaseSprite.Position;
        pos.Y -= HIGHLIGHT_HEIGHT;
        BaseSprite.Position = pos;
        foreach (Building b in Buildings)
        {
            Vector2 bpos = b.Sprite.Position;
            bpos.Y -= HIGHLIGHT_HEIGHT;
            b.Sprite.Position = bpos;
        }
    }

    public void AddBuilding(Building building)
    {
        Buildings.Add(building);

        // If the building replaces the whole tile, set it as a sprite to be drawn on the tile layer
        // otherwise, added it to the ybuffer so overlaps are avoidable
        if (building.IsWholeTile())
        {
            BuildingSprite = building.Sprite;
            BuildingSprite.Position = BaseSprite.Position;
        }
        else
        {
            Globals.Ybuffer.Add(building);
        }

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

    public void DailyUpdate()
    {
        foreach (Building b in Buildings)
            b.DailyUpdate();
    }

    public void Unhighlight()
    {
        Vector2 pos = BaseSprite.Position;
        pos.Y += HIGHLIGHT_HEIGHT;
        BaseSprite.Position = pos;

        foreach (Building b in Buildings)
        {
            Vector2 bpos = b.Sprite.Position;
            bpos.Y += HIGHLIGHT_HEIGHT;
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

    public float Wealth()
    {
        float sum = 0f;
        foreach (Building b in Buildings)
            sum += b.Wealth();
        return sum;
    }
}