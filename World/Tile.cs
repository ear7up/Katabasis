using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public enum TileDiscriminator
{
    Tile = 1,
    TileAnimal = 2
}

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

[JsonDerivedType(derivedType: typeof(TileAnimal), typeDiscriminator: "TileAnimal")]
public class Tile
{
    public const int MAX_POP = 32;
    public const int MAX_BUILDINGS = 6;
    public const int HIGHLIGHT_HEIGHT = 10;
    public const float MIN_SOIL_QUALITY = 0.3f;
    public const float MAX_SOIL_QUALITY = 0.6f;
    public const float RIVER_SOIL_QUALITY_BONUS = 0.25f;
    public const float VEGETATION_SOIL_QUALITY_BONUS = 0.1f;

    // Serialized content
    public TileDiscriminator Discriminator { get; set; }
    public TileType Type { get; set; }
    public Player Owner { get; set; }
    public int Population { get; set; }
    public bool DrawBase { get; set; }
    public Sprite BaseSprite { get; set; }
    public Sprite BuildingConSprite { get; set; }
    public List<Building> Buildings { get; set; }
    public float BaseSoilQuality { get; set; }
    public float SoilQuality { get; set; }
    public MineralType Minerals { get; set; }
    public float BaseResourceQuantity { get; set; }
    public float CurrentResourceQuantity { get; set; }
    public bool Explored { get; set; }
    public Sprite FogSprite { get; set; }
    public Goods.FoodPlant Plants { get; set; }
    public Sprite PlantIcon { get; set; }
    public TextSprite PlantText { get; set; }

    // Can't be saved due to cycle resolution error
    [JsonIgnore]
    public Tile[] Neighbors { get; set; }

    public Tile()
    {
        Discriminator = TileDiscriminator.Tile;
        Owner = null;
        Neighbors = new Tile[] { null, null, null, null};
        Population = 0;
        Buildings = new();
        DrawBase = true;
        Minerals = MineralType.NONE;
        Plants = Goods.FoodPlant.NONE;
        BaseResourceQuantity = 0f;
        CurrentResourceQuantity = 0f;
        Explored = false;
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

    public static Tile Create(
        TileType type, 
        Vector2 position, 
        SpriteTexture baseTexture)
    {
        Tile tile = new();
        tile.SetAttributes(type, position, baseTexture);
        return tile;
    }

    public virtual void SetAttributes(
        TileType type, 
        Vector2 position, 
        SpriteTexture baseTexture)
    {
        SetTileType(type);
        BaseSprite = Sprite.Create(baseTexture, position);

        BaseSoilQuality = Globals.Rand.NextFloat(MIN_SOIL_QUALITY, MAX_SOIL_QUALITY);

        if (type == TileType.VEGETATION)
            BaseSoilQuality += VEGETATION_SOIL_QUALITY_BONUS;

        SoilQuality = BaseSoilQuality;

        FogSprite = Sprite.Create(Sprites.RandomFog(), position);
        FogSprite.SpriteColor = new Color(255f, 255f, 255f, 0.85f);
    }

    public static bool CanHaveResource(TileType type)
    {
        return 
            type == TileType.WILD_ANIMAL ||
            type == TileType.ELEPHANT ||
            type == TileType.HILLS ||
            type == TileType.FOREST;
    }

    public void SetPlantType(Goods.FoodPlant plantType)
    {
        Plants = plantType;
        SpriteTexture texture = null;
        switch (Plants)
        {
            case Goods.FoodPlant.GARLIC: texture = Sprites.Garlic; break;
            case Goods.FoodPlant.SCALLIONS: texture = Sprites.Scallions; break;
            case Goods.FoodPlant.ONION: texture = Sprites.Onion; break;
            case Goods.FoodPlant.LEEK: texture = Sprites.Leeks; break;
            case Goods.FoodPlant.LETTUCE: texture = Sprites.Lettuce; break;
            case Goods.FoodPlant.CELERY: texture = Sprites.Celery; break;
            case Goods.FoodPlant.CUCUMBER: texture = Sprites.Cucumber; break;
            case Goods.FoodPlant.TURNIP: texture = Sprites.Turnips; break;
            case Goods.FoodPlant.GRAPES: texture = Sprites.Grapes; break;
            case Goods.FoodPlant.GOURD: texture = Sprites.Gourd; break;
            case Goods.FoodPlant.MELON: texture = Sprites.Melon; break;
            case Goods.FoodPlant.PEAS: texture = Sprites.Peas; break;
            case Goods.FoodPlant.LENTILS: texture = Sprites.Lentils; break;
            case Goods.FoodPlant.CHICKPEAS: texture = Sprites.Chickpeas; break;
            //case Goods.FoodPlant.NUTS
            case Goods.FoodPlant.OLIVE_OIL: texture = Sprites.OliveOil; break;
            case Goods.FoodPlant.BARLEY: texture = Sprites.Barley; break;
            case Goods.FoodPlant.WHEAT: texture = Sprites.Wheat; break;
        }

        if (texture != null)
        {
            PlantIcon = Sprite.Create(texture, Vector2.Zero);
            PlantIcon.ScaleDown(0.8f);
            InitPlantText();
        }
    }

    public void InitPlantText()
    {
        Rectangle iconBounds = PlantIcon.GetBounds();
        Rectangle tileBounds = BaseSprite.GetBounds();

        float x = tileBounds.X + tileBounds.Width / 2 + 10;
        float y = tileBounds.Y + tileBounds.Height / 2 - iconBounds.Height / 2;
        PlantIcon.Position = new Vector2(x, y);

        PlantText = new TextSprite(Sprites.Font, text: Globals.Title(Plants.ToString()));
        PlantText.ScaleUp(0.3f);
        x = tileBounds.X + tileBounds.Width / 2 - PlantText.Width() / 2 + 10;
        y += 40f;
        PlantText.Position = new Vector2(x, y);
        PlantText.Hidden = true;
    }

    public static bool IsAnimal(TileType type)
    {
        return
            type == TileType.WILD_ANIMAL || 
            type == TileType.COW || 
            type == TileType.DONKEY ||
            type == TileType.DUCK ||
            type == TileType.ELEPHANT ||
            type == TileType.FOWL ||
            type == TileType.GOAT || 
            type == TileType.GAZELLE ||
            type == TileType.GOOSE ||
            type == TileType.PIG ||
            type == TileType.QUAIL ||
            type == TileType.SHEEP;
    }

    public void SetTileType(TileType type)
    {
        BaseResourceQuantity = 0f;
        CurrentResourceQuantity = 0f;

        if (CanHaveResource(type))
        {
            BaseResourceQuantity = 1f;
            CurrentResourceQuantity = 1f;
        }
        
        if (type != TileType.HILLS)
            Minerals = MineralType.NONE;

        if (type != TileType.VEGETATION)
        {
            Plants = Goods.FoodPlant.NONE;
            PlantIcon = null;
            PlantText = null;
        }

        Type = type;
    }

    public void MakeRiver()
    {
        SetTileType(TileType.RIVER);
        BaseSprite.SetNewSpriteTexture(Sprites.RandomRiver());

        // Rivers improve the soil quality of neighboring tiles, overlap is intentional (river itself gets 2x bonus)
        foreach (Tile neighbor in Neighbors)
        {
            if (neighbor != null)
            {
                neighbor.BaseSoilQuality += Tile.RIVER_SOIL_QUALITY_BONUS;
                neighbor.SoilQuality = neighbor.BaseSoilQuality;
            }
        }
    }

    public bool HasResource()
    {
        return CurrentResourceQuantity > 0f;
    }

    // For now, just allow resource tiles to be used 1000 times before exhausting
    public void TakeResource()
    {
        CurrentResourceQuantity = Math.Max(0f, CurrentResourceQuantity - 0.001f); 
    }

    // Reduce soil quality after completing a farming task on this tile
    public void Farm()
    {
        SoilQuality = Math.Max(0.1f, SoilQuality - 0.001f);
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

    public virtual string GetResource()
    {
        string resource = "";
        switch (Type)
        {
            case TileType.DESERT: resource = "Clay"; break;
            case TileType.RIVER: resource = "Reeds"; break;
            case TileType.COW: resource = "Cows"; break;
            case TileType.DONKEY: resource = "Donkeys"; break;
            case TileType.DORMANT_VOLCANO: resource = "Obisdian"; break;
            case TileType.DUCK: resource = "Ducks"; break;
            case TileType.ELEPHANT: resource = "Elephant"; break;
            case TileType.FOREST: resource = "Trees"; break;
            case TileType.FOWL: resource = "Fowl"; break;
            case TileType.GAZELLE: resource = "Gazelle"; break;
            case TileType.GOAT: resource = "Goats"; break;
            case TileType.GOOSE: resource = "Geese"; break;
            case TileType.HILLS: resource = "Stone"; break;
            case TileType.PIG: resource = "Pigs"; break;
            case TileType.QUAIL: resource = "Quail"; break;
            case TileType.SHEEP: resource = "Sheep"; break;
            case TileType.VEGETATION: resource = "Edible plants"; break;
            case TileType.WILD_ANIMAL: resource = "Wild game"; break;
        }

        if (Minerals != MineralType.NONE)
            resource = Globals.Title(Minerals.ToString());
        else if (Plants != Goods.FoodPlant.NONE)
            resource = Globals.Title(Plants.ToString());

        return resource;
    }

    public string Describe()
    {
        string resource = GetResource();

        string buildingsDesc = "";
        Dictionary<string, int> buildingCounts = new();
        foreach (Building building in Buildings)
        {
            string key = Globals.Title(building.Type.ToString());
            if (buildingCounts.ContainsKey(key))
                buildingCounts[key]++;
            else
                buildingCounts[key] = 1;
        }

        if (Buildings.Count == 0)
            buildingsDesc = "None";
        else
            buildingsDesc = "\n";

        foreach (KeyValuePair<string, int> keyValuePair in buildingCounts)
            buildingsDesc += $"  {keyValuePair.Key} x{keyValuePair.Value}" + "\n";
        buildingsDesc.TrimEnd();

        string description = 
            $"Type: {Globals.Title(Type.ToString())}\n" + 
            $"Resources: {resource}\n" + 
            $"Population: {Population}\n" + 
            $"Buildings: {buildingsDesc}";
        return description;
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

        if (Buildings.Count >= 3)
        {
            Sprites.Paved.Position = BaseSprite.Position;
            Sprites.Paved.Draw();
        }

        Sprite buildingSprite = GetBuildingSprite();

        if (buildingSprite != null)
        {
            if (DrawBase)
            {
                Buildings[0].Draw();
            }
            else
            {
                Color temp = buildingSprite.SpriteColor;
                buildingSprite.SpriteColor = Color.SteelBlue;
                Buildings[0].Draw();

                buildingSprite.ScaleDown(0.02f);
                buildingSprite.SpriteColor = temp;
                Buildings[0].Draw();
                buildingSprite.UndoScaleDown(0.02f);
            }
        }
    }

    public Sprite GetBuildingSprite()
    {
        if (Buildings.Count == 1 && Buildings[0].IsWholeTile())
            return Buildings[0].GetSprite();
        return null;
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
        Sprite buildingSprite = GetBuildingSprite();

        if (displayType == DisplayType.SOIL_QUALITY)
        {
            float max = GetMaxSoilQuality();
            Color c = new Color(0.25f, MathHelper.Clamp((SoilQuality / max) + 0.2f, 0.5f, 1.0f), 0.25f);
            BaseSprite.SpriteColor = c;
            if (buildingSprite != null)
                buildingSprite.SpriteColor = BaseSprite.SpriteColor;
        }
        else if (displayType == DisplayType.MINERALS)
        {
            BaseSprite.SpriteColor = MineralInfo.GetColor(Minerals);
            if (buildingSprite != null)
                buildingSprite.SpriteColor = BaseSprite.SpriteColor;
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
            if (buildingSprite != null)
                buildingSprite.SpriteColor = Color.White;
        }

        if (Owner != null && Config.ShowBorders)
        {
            DrawOwnedTile();
        }
        else
        {
            if (DrawBase)
                BaseSprite.Draw();
            if (buildingSprite != null)
                Buildings[0].Draw();
        }

        if (displayType == DisplayType.SOIL_QUALITY && PlantIcon != null)
        {
            PlantIcon.Draw();
            PlantText.Unhide();
        }
        else if (PlantText != null)
        {
            PlantText.Hide();
        }
    }

    public static float GetMaxSoilQuality()
    {
        return MAX_SOIL_QUALITY + (2 * RIVER_SOIL_QUALITY_BONUS) + VEGETATION_SOIL_QUALITY_BONUS;
    }

    public void DrawTopLayer()
    {
        if (!Explored && FogSprite != null && Config.ShowFog)
            FogSprite.Draw();

        if (PlantText != null)
            PlantText.Draw(PlantText.Position);
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

    public bool ContainsSimple(Vector2 pos)
    {
        // Simple check when near the center of the tile sprite
        Vector2 center = GetOrigin() + new Vector2(0, -Map.VerticalOverlap);
        float dist = Vector2.Distance(pos, BaseSprite.Position);
        if (dist < 200)
            return true;
        return false;
    }

    public bool Contains(Vector2 pos)
    {
        if (ContainsSimple(pos))
            return true;

        // More complex, diamond shaped check
        Rectangle bounds = BaseSprite.GetBounds();
        BaseSprite.Contains(pos);

        float x = bounds.X;
        float y = bounds.Y - Map.VerticalOverlap;

        float height = bounds.Height;
        float width = bounds.Width;

        // Not within the width of the box
        if (pos.X < x || pos.X > x + width)
            return false;

        // Not within the height of the box
        if (pos.Y < y || pos.Y > y + height - Map.VerticalOverlap)
            return false;

        // Exclude the overlapping bit at the bottom
        int top = (pos.Y < (y + height - Map.VerticalOverlap) / 2f) ? 1 : 0;
        int left = (pos.X < x + width / 2f) ? 2 : 0;

        // ~0.63
        float slope = (float)(height - Map.VerticalOverlap) / width; 
        float halfHeight = (height - Map.VerticalOverlap) / 2f;

        // Conditions are different within each quadrant of the diamond
        return (top + left) switch
        {
            // bottom right
            0 => pos.X < (halfHeight - pos.Y) * (1 / slope),
            // top right
            1 => pos.X * slope < pos.Y,
            // bottom left
            2 => pos.X * slope > pos.Y,
            // top left
            3 => pos.X > (halfHeight - pos.Y) * (1 / slope),
            _ => false,
        };

    }

    public void AddBuilding(Building building)
    {
        Buildings.Add(building);

        // If the building replaces the whole tile, set it as a sprite to be drawn on the tile layer
        // otherwise, added it to the ybuffer so overlaps are avoidable
        if (building.IsWholeTile())
            building.SetPosition(BaseSprite.Position);
        building.AddToYBuffer();

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

        if (Type == TileType.WILD_ANIMAL || Type == TileType.ELEPHANT)
        {
            // Replenish by 1% every 100 seconds
            CurrentResourceQuantity = Math.Min(
                CurrentResourceQuantity + (0.01f / 100f) * Globals.Time, BaseResourceQuantity);
        }

        // Replenish soil quality by 1% every 100 seconds
        SoilQuality = Math.Min(SoilQuality + (0.01f / 100f) * Globals.Time, BaseSoilQuality);
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
        if (start == null)
            return null;

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
                if (match != null && (!Config.ShowFog || current.Explored))
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

    public virtual void Explore()
    {
        Explored = true;
    }
}