

public class Sprite
{
    public Texture2D Texture;
    public Vector2 Position { get; set; }
    public bool DrawRelativeToOrigin;
    public Vector2 Origin { get; protected set; }
    public Vector2 Scale { get; set; }
    public Color SpriteColor { get; set;}
    private Rectangle Bounds;

    public Sprite(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;
        Origin = new(Texture.Width / 2, Texture.Height / 2);
        Scale = new Vector2(1f, 1f);
        SpriteColor = Color.White;
        DrawRelativeToOrigin = true;

        Bounds = new Rectangle(
            texture.Bounds.X, texture.Bounds.Y,
            (int)(texture.Bounds.Width * Scale.X), 
            (int)(texture.Bounds.Height * Scale.Y));
    }

    public void ScaleUp(float s)
    {
        Scale += new Vector2(s, s);
    }

    public void ScaleDown(float s)
    {
        Scale -= new Vector2(s, s);
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
                    Position.Y + (Scale.X * Texture.Height / 2f) : 
                    Position.Y + (Scale.Y * Texture.Height);
    }

    public Rectangle GetBounds()
    {
        // Calculate top-left corner
        if (DrawRelativeToOrigin)
        {
            Bounds.X = (int)(Position.X - Texture.Width * Scale.X / 2f);
            Bounds.Y = (int)(Position.Y - Texture.Height * Scale.Y / 2f);
        }
        else
        {
            Bounds.X = (int)Position.X;
            Bounds.Y = (int)Position.Y;
        }
        
        Bounds.Width =  (int)(Texture.Width * Scale.X);
        Bounds.Height = (int)(Texture.Height * Scale.Y);
        return Bounds;
    }

    public void SetScale(float s)
    {
        Scale = new Vector2(s, s);
    }

    public void SetScaleX(float s)
    {
        Scale = new Vector2(s, Scale.Y);
    }

    public void SetScaleY(float s)
    {
        Scale = new Vector2(Scale.X, s);
    }
}