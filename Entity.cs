using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

public abstract class Entity
{
    [JsonIgnore]
    public Texture2D image { get; protected set; }

    // Serialized content (restore texture using TexturePath at load time)
    private string TexturePath;
    public string TexturePathSerial { 
        get { 
            return TexturePath; 
        }
        set {
            TexturePath = value;
            image = Sprites.GetTexture(TexturePath);
        }
    }
    
    // The tint of the image. This will also allow us to change the transparency.
    protected Color color = Color.White;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Orientation { get; set; }
    public float Scale { get; set; }
    public bool Hidden { get; set; }

    private Rectangle Bounds;

    public Vector2 Size
    {
        get
        {
            return image == null ? Vector2.Zero : new Vector2(image.Width, image.Height);
        }
    }

    public void SetImage(SpriteTexture spriteTexture)
    {
        image = spriteTexture.Texture;
        TexturePath = spriteTexture.Path;
        Bounds = new Rectangle(
            image.Bounds.X, image.Bounds.Y,
            (int)(image.Bounds.Width * Scale), 
            (int)(image.Bounds.Height * Scale));
    }

    public SpriteTexture GetSpriteTexture()
    {
        return new SpriteTexture(TexturePath, image);
    }

    public void SetNewSpriteTexture(SpriteTexture spriteTexture)
    {
        image = spriteTexture.Texture;
        TexturePath = spriteTexture.Path;
    }

    public Rectangle GetBounds()
    {
        Bounds.X = (int)Position.X;
        Bounds.Y = (int)Position.Y;
        Bounds.Width =  (int)(image.Bounds.Width * Scale);
        Bounds.Height = (int)(image.Bounds.Height * Scale);
        return Bounds;
    }

    public abstract void Update();

    public virtual void Draw()
    {
        if (!Hidden)
            Globals.SpriteBatch.Draw(image, Position, null, color, Orientation, Size / 2f, Scale, 0, 0);
    }
}