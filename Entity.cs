using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

public abstract class Entity
{
    [JsonIgnore]
    public Texture2D image { get; protected set; }
    
    // The tint of the image. This will also allow us to change the transparency.
    protected Color color = Color.White;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Orientation { get; set; }
    public float Scale { get; set; }

    private Rectangle Bounds;

    public Vector2 Size
    {
        get
        {
            return image == null ? Vector2.Zero : new Vector2(image.Width, image.Height);
        }
    }

    public void SetImage(Texture2D image)
    {
        this.image = image;
        Bounds = new Rectangle(
            image.Bounds.X, image.Bounds.Y,
            (int)(image.Bounds.Width * Scale), 
            (int)(image.Bounds.Height * Scale));
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
        Globals.SpriteBatch.Draw(image, Position, null, color, Orientation, Size / 2f, Scale, 0, 0);
    }
}