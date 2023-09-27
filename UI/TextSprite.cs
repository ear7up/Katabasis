using System.Text.Json.Serialization;

public class TextSprite : UIElement, Drawable
{
    [JsonIgnore]
    public SpriteFont Font;

    [JsonIgnore]
    public SpriteFont Shadow;

    public string Text { get; set; }

    public Color FontColor;
    public Color DropShadowColor;
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

    public TextSprite(SpriteFont font, bool hasDropShadow = false, string text = "") : base()
    {
        Font = font;
        Text = text;
        HasDropShadow = hasDropShadow;
        Position = Vector2.Zero;
        FontColor = Color.Black;
        DropShadowColor = Color.White;
        Transparency = 1f;
    }

    public TextSprite(SpriteFont font, Color color, string text = "") : this(font, false, text)
    {
        FontColor = color;
        HasDropShadow = false;
    }

    public TextSprite(SpriteFont font, Color color, Color dropShadowColor, string text = "") : this(font, true, text)
    {
        FontColor = color;
        DropShadowColor = dropShadowColor;
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
            Globals.SpriteBatch.DrawString(Font, Text, offset + GetPadding() + new Vector2(1f, 1f), 
                DropShadowColor * Transparency, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
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