public class TextSprite
{
    public SpriteFont Font;
    public Vector2 Position;
    public string Text;

    public TextSprite(SpriteFont font)
    {
        Font = font;
        Text = "";
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        Globals.SpriteBatch.DrawString(Font, Text, Position, Color.Blue);
    }
}