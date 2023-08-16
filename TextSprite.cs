public class TextSprite : UIElement
{
    public SpriteFont Font;
    public SpriteFont Shadow;
    public Vector2 Position;
    public string Text;

    public Color FontColor;
    public bool HasDropShadow;

    public TextSprite(SpriteFont font, bool hasDropShadow = true, string text = "") : base()
    {
        Font = font;
        Text = text;
        HasDropShadow = hasDropShadow;

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

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        if (HasDropShadow)
        {
            Globals.SpriteBatch.DrawString(Font, Text, offset + new Vector2(2f, 2f), 
                Color.Black, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }

        Globals.SpriteBatch.DrawString(Font, Text, offset, FontColor, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
    }
}