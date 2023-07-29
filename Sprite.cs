

public class Sprite
{
    private readonly Texture2D _texture;
    public Vector2 Position { get; set; }
    public Vector2 Origin { get; protected set; }
    public float Scale { get; set; }
    public Color SpriteColor { get; set;}

    public Sprite(Texture2D texture, Vector2 position)
    {
        _texture = texture;
        Position = position;
        Origin = new(_texture.Width / 2, _texture.Height / 2);
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
        Globals.SpriteBatch.Draw(_texture, Position, null, SpriteColor, 0f, Origin, Scale, SpriteEffects.None, 0f);
    }
}