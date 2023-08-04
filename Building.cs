using System;

public enum BuildingType
{
    MARKET,
    HOUSE,
    LUMBERMILL,
    FORGE,
    FARM,
    MINE,
    SMITHY,
    NONE
}

public class Building : Drawable
{
    // Physical location of the market
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

    protected Building(Tile location, Sprite sprite, BuildingType buildingType = BuildingType.NONE)
    {
        Location = location;
        Sprite = sprite;
        BuildingType = buildingType;
    }

    public static Building CreateBuilding(Tile location, Sprite sprite, BuildingType buildingType = BuildingType.NONE)
    {
        Building b = new Building(location, sprite, buildingType);
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
        return Sprite.GetMaxY() - (Sprite.Scale * Sprite.Texture.Height * 0.3f);
    }
}