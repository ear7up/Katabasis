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

public class Building
{
    // Physical location of the market
    public Tile Location;
    public BuildingType BuildingType;
    public Sprite Sprite;

    public static Building Random()
    {
        Sprite sprite = new Sprite(Sprites.RandomBuilding(), Vector2.Zero);
        sprite.ScaleDown(0.7f);
        return new Building(null, sprite);
    }

    public Building(Tile location, Sprite sprite, BuildingType buildingType = BuildingType.NONE)
    {
        Location = location;
        Sprite = sprite;
        BuildingType = buildingType;
    }

    public void Draw()
    {
        Sprite.Draw();
    }

    public float GetMaxY()
    {
        return Sprite.Position.Y + (Sprite.Scale * Sprite.Texture.Height);
    }
}