

public class Sprite
{
    public Texture2D Texture;
    public Vector2 Position { get; set; }
    public Vector2 Origin { get; protected set; }
    public float Scale { get; set; }
    public Color SpriteColor { get; set;}

    public Sprite(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;
        Origin = new(Texture.Width / 2, Texture.Height / 2);
        Scale = 1f;
        SpriteColor = Color.White;
    }

    public void ScaleUp(float s)
    {
        Scale += s;
    }

    public void ScaleDown(float s)
    {
        Scale -= s;
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, SpriteColor, 0f, Origin, Scale, SpriteEffects.None, 0f);
    }
}