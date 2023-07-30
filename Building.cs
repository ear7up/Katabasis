public class Building
{
    public Sprite sprite;

    public static Building Random()
    {
        Sprite s = new Sprite(Sprites.RandomBuilding(), Vector2.Zero);
        s.ScaleDown(0.7f);
        return new Building(s);
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