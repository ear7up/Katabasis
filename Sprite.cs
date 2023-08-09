

public class Sprite
{
    public Texture2D Texture;
    public Vector2 Position { get; set; }
    public bool DrawRelativeToOrigin;
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
        DrawRelativeToOrigin = true;

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
        Globals.SpriteBatch.Draw(
            Texture, Position, null, 
            SpriteColor, 0f, 
            (DrawRelativeToOrigin) ? Origin : Vector2.Zero, 
            Scale, SpriteEffects.None, 0f);
    }

    public float GetMaxY()
    {
        // Position.Y is in the center
        return (DrawRelativeToOrigin) ? 
                    Position.Y + (Scale * Texture.Height / 2f) : 
                    Position.Y + (Scale * Texture.Height);
    }

    public Rectangle GetBounds()
    {
        // Calculate top-left corner
        if (DrawRelativeToOrigin)
        {
            Bounds.X = (int)(Position.X - Origin.X);
            Bounds.Y = (int)(Position.Y - Origin.Y);
        }
        else
        {
            Bounds.X = (int)Position.X;
            Bounds.Y = (int)Position.Y;
        }
        
        Bounds.Width =  (int)(Texture.Width * Scale);
        Bounds.Height = (int)(Texture.Height * Scale);
        return Bounds;
    }
}