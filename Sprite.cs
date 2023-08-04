

public class Sprite
{
    public Texture2D Texture;
    public Vector2 Position { get; set; }
    public Vector2 Origin { get; protected set; }
    public float Scale { get; set; }
    public Color SpriteColor { get; set;}
    private Rectangle Bounds;

    public Sprite(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;
        Origin = new(Texture.Width / 2, Texture.Height / 2);
        Scale = 1f;
        SpriteColor = Color.White;

        Bounds = new Rectangle(
            texture.Bounds.X, texture.Bounds.Y,
            (int)(texture.Bounds.Width * Scale), 
            (int)(texture.Bounds.Height * Scale));
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

    public float GetMaxY()
    {
        // Position.Y is in the center
        return Position.Y + (Scale * Texture.Height / 2f);
    }

    public Rectangle GetBounds()
    {
        // Calculate top-left corner
        Bounds.X = (int)(Position.X - Origin.X);
        Bounds.Y = (int)(Position.Y - Origin.Y);
        Bounds.Width =  (int)(Texture.Width);
        Bounds.Height = (int)(Texture.Height);
        return Bounds;
    }
}