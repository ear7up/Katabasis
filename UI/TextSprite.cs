using System.Text.Json.Serialization;

public class TextSprite : UIElement, Drawable
{
    [JsonIgnore]
    public SpriteFont Font;

    [JsonIgnore]
    public SpriteFont Shadow;

    public string Text { get; set; }

    public Color FontColor { get; set; }
    public bool HasDropShadow { get; set; }

    public TextSprite(SpriteFont font, bool hasDropShadow = true, string text = "") : base()
    {
        Font = font;
        Text = text;
        HasDropShadow = hasDropShadow;
        Position = Vector2.Zero;

        if (HasDropShadow)
            FontColor = Color.White;
        else
            FontColor = Color.Blue;
        
    }

    public override int Width()
    {
        return (int)(
            GetLeftPadding() + 
            Font.MeasureString(Text).X * Scale.X) + 
            GetRightPadding();
    }

    public override int Height()
    {
        return (int)(
            GetTopPadding() + 
            Font.MeasureString(Text).Y * Scale.Y) + 
            GetBottomPadding();
    }

    public override Rectangle GetBounds()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, Width(), Height());
    }

    public override void Draw(Vector2 offset)
    {
        base.Draw(offset);

        if (Hidden)
            return;

        Position = offset;

        // Draw using layerDepth = 1f, draw text above everything else on layer 0 (default)
        if (HasDropShadow)
        {
            Globals.SpriteBatch.DrawString(Font, Text, offset + new Vector2(2f, 2f), 
                Color.Black, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
        }

        Globals.SpriteBatch.DrawString(Font, Text, offset, 
            FontColor, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
    }

    public void Draw()
    {
        Draw(Position);
    }

    public float GetMaxY()
    {
        return Position.Y + Height();
    }
}