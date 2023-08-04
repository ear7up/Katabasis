public class TextSprite
{
    public SpriteFont Font;
    public Vector2 Position;
    public string Text;

    public TextSprite(SpriteFont font)
    {
        Font = font;
    }

    public void Update()
    {
        Position = InputManager.MousePos;
        Text = $"({Position.X:0.00}, {Position.Y:0.00})";
    }

    public void Draw()
    {
        Globals.SpriteBatch.DrawString(Font, Text, Position, Color.Blue);
    }
}