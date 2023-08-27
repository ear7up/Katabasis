public class DecorationManager
{
    public Sprite Editing;

    public DecorationManager()
    {

    }

    public void NewDecoration()
    {
        Editing = Sprite.Create(Sprites.RandomDecoration(), InputManager.MousePos);
        Editing.SpriteColor = new Color(Color.LightBlue, 0.3f);
    }

    public void Update(Map tileMap)
    {
        if (Editing == null)
            return;

        // Cancel on right click
        if (InputManager.RClicked)
        {
            Editing = null;
            return;
        }

        if (InputManager.ScrollValue > 0)
            Editing.ScaleUp(0.05f);
        else if (InputManager.ScrollValue < 0)
            Editing.ScaleDown(0.05f);

        // Consume the scroll event
        if (InputManager.ScrollValue != 0)
            InputManager.ScrollValue = 0;

        Editing.Position = InputManager.MousePos;

        if (InputManager.UnconsumedClick())
        {
            InputManager.ConsumeClick(this);
            Editing.SpriteColor = Color.White;
            tileMap.AddDecoration(Editing);
            Editing = null;
        }
    }

    public void Draw()
    {
        if (Editing == null)
            return;

        Editing.Draw();
    }
}