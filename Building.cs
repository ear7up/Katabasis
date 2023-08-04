using System;

public enum BuildingType
{
    MARKET,
    WOOD_HOUSE,
    STONE_HOUSE,
    LUMBERMILL,
    FORGE,
    FARM,
    RANCH,
    MINE,
    SMITHY,
    NONE
}

public class Building : Drawable
{
    public static int IdCounter = 0;

    public int Id;
    public Tile Location;
    public BuildingType BuildingType;
    public Sprite Sprite;

    public static Building Random(bool temporary = false)
    {
        Sprite sprite = new Sprite(Sprites.RandomBuilding(), Vector2.Zero);
        sprite.ScaleDown(0.7f);

        Building b = new Building(null, sprite);

        if (!temporary)
            Globals.Ybuffer.Add(b);

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
        BuildingType = buildingType;
    }

    public static Building CreateBuilding(
        Tile location, 
        BuildingType buildingType = BuildingType.NONE, 
        Sprite sprite = null)
    {
        // Try lay out the buliding in the empty space on the diamond based on buliding count
        Vector2 position = new Vector2();
        position.X = location.GetPosition().X;
        position.Y = location.GetPosition().Y;

        switch (location.Buildings.Count)
        {
            case 0: position += new Vector2(0, -Map.TileSize.Y / 2); break;
            case 1: position += new Vector2(Map.TileSize.X / 2, 0); break;
            case 2: position += new Vector2(0, Map.TileSize.X / 2); break;
            case 3: position += new Vector2(-Map.TileSize.X / 2, 0); break;
        }

        // TODO: need sprites for all building types
        switch (buildingType)
        {
            default: sprite = new Sprite(Sprites.RandomBuilding(), position); break;
        }

        // TODO: this building won't get Updated
        Building b = new Building(location, sprite, buildingType);
        b.Sprite.ScaleDown(0.7f);
        location.AddBuilding(b);
        Globals.Ybuffer.Add(b);
        return b;    
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
        return base.ToString() + " " + Sprite.Position.ToString() + " " + BuildingType.ToString();
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