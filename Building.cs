public class Building
{
    public Sprite sprite;

    public static Building Random()
    {
        return new Building(new Sprite(Sprites.RandomBuilding(), Vector2.Zero));
    }

    public Building(Sprite s)
    {
        sprite = s;
    }

    public void Draw()
    {
        sprite.Draw();
    }
}