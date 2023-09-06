using System.Collections.Generic;

public class RightClickMenu : VBox
{
    public List<UIElement> Options;

    public RightClickMenu(SpriteTexture texture) : base(texture)
    {
        SetMargin(left: 1, top: 30);

        Options = new();
        Hide();
    }

    public void AddOption(UIElement option)
    {
        option.SetPadding(left: 20);
        option.SetPadding(bottom: 5);

        OverlapLayout olayout = new(Sprites.MenuItem);
        olayout.HoverImage = Sprite.Create(Sprites.MenuItemHover, Vector2.Zero);
        olayout.HoverImage.DrawRelativeToOrigin = false;
        olayout.SetMargin(top: 1, left: 1);
        olayout.Add(option);

        Elements.Add(olayout);
    }

    public void RemoveOption(UIElement option)
    {
        Elements.Remove(option);
    }

    public override void Update()
    {
        if (InputManager.UnconsumedRClick())
        {
            Position = InputManager.ScreenMousePos + new Vector2(0f, 5f);
            Unhide();
        }

        if (InputManager.Clicked)
            Hide();

        if (!Hidden && InputManager.UnconsumedKeypress(Keys.Escape))
        {
            Hide();
            InputManager.ConsumeKeypress(Keys.Escape, this);
        }

        base.Update();
    }
}