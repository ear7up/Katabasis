using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public TileType Type { get; set; }
    public Player Owner { get; set; }

    public const int HIGHLIGHT_HEIGHT = 30;
    
    public const int MAX_POP = 32;
    public int Population { get; set; }

    public bool DrawBase { get; set; }
    public Sprite BaseSprite { get; protected set; }
    public Sprite BuildingSprite { get; set; }
    
    // TODO: infinite recursion when serializing tiles referencing other tiles
    [JsonIgnore]
    public Tile[] Neighbors { get; set; }
    
    public const int MAX_BUILDINGS = 6;
    public List<Building> Buildings { get; set; }

    public const float MIN_SOIL_QUALITY = 0.3f;
    public const float MAX_SOIL_QUALITY = 0.6f;
    public const float RIVER_SOIL_QUALITY_BONUS = 0.25f;
    public const float VEGETATION_SOIL_QUALITY_BONUS = 0.1f;
    public float SoilQuality { get; set; }

    public MineralType Minerals { get; set; }

    public void Save(FileStream fileStream)
    {
        // Type
        // Owner [ref Person]
        // Population
        // DrawBase
        // BaseSprite [ref Sprite]
        // BuildingSprite [ref Sprite]
        // Neighbors [ref Sprite[]]
        // Buildings [ref List<Building>]
        // SoilQuality
        // Minerals
        JsonSerializer.Serialize(fileStream, this, Globals.JsonOptions);
    }

    public void Load()
    {

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
            BaseSprite.UndoScaleDown(0.02f);
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
                BuildingSprite.UndoScaleDown(0.02f);
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

    public static Object Find(Tile start, TileFilter f, int distance = 8)
    {
        int num_tiles = (2 * distance + 1) * (2 * distance + 1);

        Tile current = start;
        Object match = f.Match(current);
        if (match != null)
            return match;

        // Take 1 step NE, then 1 step SE, then 2 steps SW, 2 steps NW, and so on
        // to complete a ring around the starting tile. Keep making larger rings
        // until all tiles within the requested radius have been claimed
        int i = 1;
        float steps = 1f;
        int direction = Globals.Rand.Next(4);
        while (i < num_tiles)
        {
            for (int j = 0; j < (int)steps && current != null; j++)
            {
                current = current.Neighbors[direction];
                match = f.Match(current);
                if (match != null)
                    return match;

                if (++i >= num_tiles)
                    break;
            }
            direction = (int)Tile.GetNextDirection(direction);
            steps += 0.5f;

            if (current == null)
                break;
        }
        return null;
    }
}