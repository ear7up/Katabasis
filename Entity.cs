using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

public abstract class Entity
{
    public Texture2D image { get; private set; }
    
    // The tint of the image. This will also allow us to change the transparency.
    protected Color color = Color.White;	

    public Vector2 Position, Velocity;
    public float Orientation;
    public float Radius = 20;	// used for circular collision detection
    protected float Scale;
    //public bool IsExpired;		// true if the entity was destroyed and should be deleted.

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
        Radius = image.Width / 2f;
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
        Globals.SpriteBatch.Draw(image, Position, null, color, Orientation, Size / 2f, 1f, 0, 0);
    }
}