using System;

public enum BuildingType
{
    MARKET,
    WOOD_HOUSE,
    STONE_HOUSE,
    LUMBERMILL,
    FORGE,
    FARM,
    FARM_RIVER,
    RANCH,
    MINE,
    SMITHY,
    BARRACKS,
    GRANARY,
    NONE
}

public class Building : Drawable
{
    public static int IdCounter = 0;

    public int Id;
    public Tile Location;
    public BuildingType Type;
    public Sprite Sprite;

    public int CurrentUsers;
    public int MaxUsers;

    public static Building Random(BuildingType type = BuildingType.NONE, bool temporary = false)
    {
        // Random building type (excluding NONE)
        if (type == BuildingType.NONE)
        {
            Array buildingTypes = Enum.GetValues(typeof(BuildingType));
            type = (BuildingType)Globals.Rand.Next(buildingTypes.Length - 1);
        }

        Building b = CreateBuilding(null, type);
        return b;
    }

    protected Building(
        Tile location, 
        Sprite sprite,
        BuildingType buildingType = BuildingType.NONE)
    {
        Id = IdCounter++;
        Location = location;
        Sprite = sprite;
        Type = buildingType;
        MaxUsers = BuildingInfo.GetMaxUsers(buildingType);
    }

    public static Building CreateBuilding(Tile location, BuildingType buildingType = BuildingType.NONE)
    {
        // Try lay out the building in the empty space on the diamond based on building count
        Vector2 position = Vector2.Zero;

        if (location != null)
        {
            position.X = location.GetPosition().X;
            position.Y = location.GetPosition().Y;
        }

        // TODO: need sprites for all building types
        Sprite sprite = new Sprite(Sprites.RandomBuilding(buildingType), position);
        Building b = new Building(location, sprite, buildingType);

        if (location != null)
            location.AddBuilding(b);

        return b;    
    }

    public bool IsWholeTile()
    {
        return Type == BuildingType.FARM || 
               Type == BuildingType.FARM_RIVER || 
               Type == BuildingType.MINE || 
               Type == BuildingType.RANCH;
    }
    
    public void StartUsing()
    {
        CurrentUsers++;
    }

    public void StopUsing()
    {
        CurrentUsers--;
    }

    public static bool EquivalentType(BuildingType a, BuildingType b)
    {
        if (a == b)
            return true;
        if (a == BuildingType.FARM && b == BuildingType.FARM_RIVER)
            return true;
        if (a == BuildingType.FARM_RIVER && b == BuildingType.FARM)
            return true;
        return false;
    }

    public static bool ValidPlacement(Building building, Tile location)
    {
        if (location == null)
            return false;
        if (building.Type == BuildingType.MINE && location.Type != TileType.HILLS)
            return false;
        if (building.Type == BuildingType.FARM_RIVER && location.Type != TileType.RIVER)
            return false;
        if (building.Type == BuildingType.RANCH && location.Type != TileType.WILD_ANIMAL)
            return false;
        if (location.Buildings.Count > 0)
            return false;
        return true;
    }

    public static bool ConfirmBuilding(Building building, Tile location)
    {
        if (location == null)
            return false;

        // Force farms to be river type on rivers
        if (location.Type == TileType.RIVER && building.Type == BuildingType.FARM)
        {
            building.Type = BuildingType.FARM_RIVER;
            building.Sprite.Texture = Sprites.GetRiverFarmSprite();
        }

        if (ValidPlacement(building, location))
        {
            location.AddBuilding(building);
            return true;
        }
        return false;
    }

    public void Update()
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE && InputManager.Clicked)
        {
            if (Sprite.GetBounds().Contains(InputManager.MousePos))
                Console.WriteLine("Building clicked: " + this.ToString() + $"(max_y = {this.GetMaxY()})");
        }
    }

    public override string ToString()
    {
        return base.ToString() + " " + Sprite.Position.ToString() + " " + Type.ToString();
    }

    public void Draw()
    {
        Sprite.Draw();
    }

    public float GetMaxY()
    {
        // For perspective, let Person sprites be drawn over top of the bottom 30% of the building
        return Sprite.GetMaxY() - (Sprite.Scale * Sprite.Texture.Height * 0.3f) + (Id * 0.001f);
    }
}