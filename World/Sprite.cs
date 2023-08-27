

using System.Text.Json.Serialization;

public class SpriteTexture
{
    public string Path;
    public Texture2D Texture;
    public SpriteTexture(string path, Texture2D texture)
    {
        Path = path;
        Texture = texture;
    }
}

public class Sprite : Drawable
{
    [JsonIgnore]
    public Texture2D Texture;
    
    // Serialized content (restore texture using TexturePath at load time)
    private string TexturePath;
    public string TexturePathSerial { 
        get { 
            return TexturePath; 
        }
        set {
            TexturePath = value;
            Texture = Sprites.GetTexture(TexturePath);
        }
    }
    public Vector2 Position { get; set; }
    public bool DrawRelativeToOrigin { get; set; }
    public Vector2 Origin { get; set; }
    public Vector2 Scale { get; set; }
    public Color SpriteColor { get; set; }
    public float Rotation { get; set; }
    
    public Rectangle Bounds;

    public Sprite()
    {
        Scale = new Vector2(1f, 1f);
        SpriteColor = Color.White;
        Rotation = 0f;
        DrawRelativeToOrigin = true;
    }

    public static Sprite Create(SpriteTexture spriteTexture, Vector2 position)
    {
        Sprite sprite = new();
        sprite.Texture = spriteTexture.Texture;
        sprite.TexturePath = spriteTexture.Path;
        sprite.Position = position;
        sprite.Origin = new(sprite.Texture.Width / 2, sprite.Texture.Height / 2);

        sprite.Bounds = new Rectangle(
            spriteTexture.Texture.Bounds.X, spriteTexture.Texture.Bounds.Y,
            (int)(spriteTexture.Texture.Bounds.Width * sprite.Scale.X), 
            (int)(spriteTexture.Texture.Bounds.Height * sprite.Scale.Y));
        return sprite;
    }

    public void SetNewSpriteTexture(SpriteTexture spriteTexture)
    {
        Texture = spriteTexture.Texture;
        TexturePath = spriteTexture.Path;
    }

    public void ScaleUp(float s)
    {
        Scale *= new Vector2(1 + s, 1 + s);
    }

    public void ScaleDown(float s)
    {
        Scale *= new Vector2(1 - s, 1 - s);
    }

    public void UndoScaleUp(float s)
    {
        Scale /= new Vector2(1 + s, 1 + s);
    }

    public void UndoScaleDown(float s)
    {
        Scale /= new Vector2(1 - s, 1 - s);
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(
            Texture, Position, null, 
            SpriteColor, Rotation,
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

    public bool Contains(Vector2 pos)
    {
        Rectangle bounds = GetBounds();

        // Shave off the width, most of it is transparent
        bounds.Inflate(Bounds.Width * -0.4f, 0f);
        return bounds.Contains(pos);
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