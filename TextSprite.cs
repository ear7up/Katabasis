public class TextSprite
{
    public SpriteFont Font;
    public Vector2 Position;
    public string Text;

    public float Scale;
    public Color FontColor;

    public TextSprite(SpriteFont font)
    {
        Font = font;
        Text = "";
        FontColor = Color.Blue;
        Position = Vector2.Zero;
        Scale = 1f;
    }

    public void Update()
    {
        
    }

    public float Width()
    {
        return Font.MeasureString(Text).X * Scale;
    }

    public void Draw()
    {
        Vector2 dimensions = Font.MeasureString(Text);
        Vector2 origin = new(dimensions.X / 2, dimensions.Y / 2);
        Globals.SpriteBatch.DrawString(Font, Text, Position, FontColor, 0f, origin, Scale, SpriteEffects.None, 0f);
    }
}