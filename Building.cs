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

    public static Building Random(bool temporary = false)
    {
        // Random building type (excluding NONE)
        Array buildingTypes = Enum.GetValues(typeof(BuildingType));
        int i = Globals.Rand.Next(buildingTypes.Length - 1);

        //Sprite sprite = new Sprite(Sprites.RandomBuilding(), Vector2.Zero);
        //sprite.ScaleDown(0.7f);
        //Building b = new Building(null, sprite);

        Building b = CreateBuilding(null, (BuildingType)i);

        //if (!temporary)
        //    Globals.Ybuffer.Add(b);

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
    }

    public static Building CreateBuilding(
        Tile location, 
        BuildingType buildingType = BuildingType.NONE, 
        Sprite sprite = null)
    {
        // Try lay out the buliding in the empty space on the diamond based on buliding count
        Vector2 position = Vector2.Zero;

        if (location != null)
        {
            position.X = location.GetPosition().X;
            position.Y = location.GetPosition().Y;
        }

        /*
        switch (location.Buildings.Count)
        {
            case 0: position += new Vector2(0, -Map.TileSize.Y / 2); break;
            case 1: position += new Vector2(Map.TileSize.X / 2, 0); break;
            case 2: position += new Vector2(0, Map.TileSize.X / 2); break;
            case 3: position += new Vector2(-Map.TileSize.X / 2, 0); break;
        }
        */

        // TODO: need sprites for all building types
        switch (buildingType)
        {
            default: sprite = new Sprite(Sprites.RandomBuilding(buildingType), position); break;
        }

        // TODO: this building won't get Update() called on it
        Building b = new Building(location, sprite, buildingType);
        //b.Sprite.ScaleDown(0.7f);

        if (location != null)
            location.AddBuilding(b);

        //Globals.Ybuffer.Add(b);

        return b;    
    }

    public static bool ConfirmBuilding(Building building, Tile location)
    {
        // Force farms to be river type on rivers
        if (location.Type == TileType.RIVER && building.Type == BuildingType.FARM)
        {
            building.Type = BuildingType.FARM_RIVER;
            building.Sprite.Texture = Sprites.GetRiverFarmSprite();
        }

        // Do not allow incompatible tile/building combinations
        if (building.Type == BuildingType.MINE && location.Type != TileType.HILLS)
            return false;
        else if (building.Type == BuildingType.FARM_RIVER && location.Type != TileType.RIVER)
            return false;
        else if (building.Type == BuildingType.RANCH && location.Type != TileType.WILD_ANIMAL)
            return false;

        if (location.Buildings.Count == 0)
        {
            building.Location = location;
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