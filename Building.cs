using System;

public enum BuildingType
{
    CITY,
    MARKET,
    HOUSE,
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

public enum BuildingSubType
{
    GOLD_MINE,
    SILVER_MINE,
    COPPER_MINE,
    LEAD_MINE,
    MALACHITE_MINE,
    LAPIS_LAZULI_MINE,
    TIN_MINE,
    IRON_MINE,
    SALT_MINE,
    NONE
}

public class Building : Drawable
{
    public static int IdCounter = 0;

    public int Id;
    public Tile Location;
    public BuildingType Type;
    public BuildingSubType SubType;
    public Sprite Sprite;

    public int CurrentUsers;
    public int MaxUsers;
    public bool Selected;

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
        SubType = BuildingSubType.NONE;
        MaxUsers = BuildingInfo.GetMaxUsers(buildingType);
        Selected = false;
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

        if (!b.IsWholeTile())
            sprite.Scale = 0.4f;

        if (location != null)
            location.AddBuilding(b);

        return b;    
    }

    public bool IsWholeTile()
    {
        return Type == BuildingType.FARM || 
               Type == BuildingType.FARM_RIVER || 
               Type == BuildingType.MINE || 
               Type == BuildingType.RANCH || 
               Type == BuildingType.CITY ||
               Type == BuildingType.MARKET;
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

    public const int MAX_BUILDINGS_PER_TILE = 4;

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
        if (location.Buildings.Count >= MAX_BUILDINGS_PER_TILE)
            return false;
        if (building.IsWholeTile() && location.Buildings.Count != 0)
            return false;
        if (location.Owner == null)
            return false;
        foreach (Building b in location.Buildings)
            if (b.IsWholeTile())
                return false;
        return true;
    }

    public static bool ConfirmBuilding(Building building, Tile location)
    {
        if (location == null || building == null)
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
            building.Location = location;

            if (building.Type == BuildingType.MINE)
            {
                switch (location.Minerals)
                {
                    case MineralType.IRON: building.SubType = BuildingSubType.IRON_MINE; break;
                    case MineralType.COPPER: building.SubType = BuildingSubType.COPPER_MINE; break;
                    case MineralType.GOLD: building.SubType = BuildingSubType.GOLD_MINE; break;
                    case MineralType.LAPIS_LAZULI: building.SubType = BuildingSubType.LAPIS_LAZULI_MINE; break;
                    case MineralType.LEAD: building.SubType = BuildingSubType.LEAD_MINE; break;
                    case MineralType.MALACHITE: building.SubType = BuildingSubType.MALACHITE_MINE; break;
                    case MineralType.SILVER: building.SubType = BuildingSubType.SILVER_MINE; break;
                    case MineralType.TIN: building.SubType = BuildingSubType.TIN_MINE; break;
                    case MineralType.SALT: building.SubType = BuildingSubType.SALT_MINE; break;
                }
            }
            return true;
        }
        return false;
    }

    public void Update()
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE && InputManager.Clicked)
        {
            if (Sprite.GetBounds().Contains(InputManager.MousePos))
            {
                Console.WriteLine("Building clicked: " + this.ToString() + $"(max_y = {this.GetMaxY()})");
                Selected = true;
            }
            else
            {
                Selected = false;
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + " " + Sprite.Position.ToString() + " " + Type.ToString() + " " +
            $"users=({CurrentUsers}/{MaxUsers})";
    }

    public void Draw()
    {
        if (Selected)
        {
            Sprite.SpriteColor = Color.Cyan;
            Sprite.Scale += 0.025f;
            Sprite.Draw();

            Sprite.SpriteColor = Color.White;
            Sprite.Scale -= 0.025f;
        }

        Sprite.Draw();
    }

    public float GetMaxY()
    {
        // For perspective, let Person sprites be drawn over top of the bottom 30% of the building
        return Sprite.GetMaxY() - (Sprite.Scale * Sprite.Texture.Height * 0.3f) + (Id * 0.001f);
    }
}