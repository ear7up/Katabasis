using System.Text.Json.Serialization;

public class TextSprite : UIElement, Drawable
{
    [JsonIgnore]
    public SpriteFont Font;

    [JsonIgnore]
    public SpriteFont Shadow;

    public string Text { get; set; }

    public Color FontColor;
    public bool HasDropShadow { get; set; }

    public float Transparency { get; set; }

    public TextSprite()
    {
        Font = Sprites.Font;
        Shadow = Sprites.Font;
        HasDropShadow = true;
        Position = Vector2.Zero;
        Text = "";
        FontColor = Color.White;
        Transparency = 1f;
    }

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

        Transparency = 1f;
    }

    public TextSprite(SpriteFont font, Color color, string text = "") : this(font, true, text)
    {
        FontColor = color;
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
        Vector2 pos = Position + GetPadding();
        return new Rectangle((int)pos.X, (int)pos.Y, Width(), Height());
    }

    public override void Draw(Vector2 offset)
    {
        base.Draw(offset + GetPadding());

        if (Hidden)
            return;

        Position = offset;

        // Draw using layerDepth = 1f, draw text above everything else on layer 0 (default)
        if (HasDropShadow  && Transparency == 1f)
        {
            Color shadowColor = Color.Black * Transparency;
            Globals.SpriteBatch.DrawString(Font, Text, offset + GetPadding() + new Vector2(1f, 1f), 
                shadowColor, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
        }

        Globals.SpriteBatch.DrawString(Font, Text, offset + GetPadding(), 
            FontColor * Transparency, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
    }

    public void Draw()
    {
        Draw(Position + GetPadding());
    }

    public float GetMaxY()
    {
        return Position.Y + Height();
    }
}