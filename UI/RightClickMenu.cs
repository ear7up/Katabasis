using System;
using System.Collections.Generic;

public class RightClickMenu : VBox
{
    public class ClickPosition
    {
        public float X;
        public float Y;
        public ClickPosition(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    public ClickPosition WorldPos;
    public List<UIElement> Options;

    public RightClickMenu(SpriteTexture texture) : base(texture)
    {
        SetMargin(left: 1, top: 30);

        Options = new();
        WorldPos = new(0f, 0f);
        Hide();
    }

    public void AddOption(UIElement option, Action<Object> callback = null)
    {
        option.SetPadding(left: 20);
        option.SetPadding(bottom: 5);
        option.OnClick = callback;
        option.UserData = WorldPos;

        OverlapLayout olayout = new(Sprites.MenuItem);
        olayout.HoverImage = Sprite.Create(Sprites.MenuItemHover, Vector2.Zero);
        olayout.HoverImage.DrawRelativeToOrigin = false;
        olayout.SetMargin(top: 1, left: 1);
        olayout.Add(option);

        olayout.OnClick = callback;
        olayout.UserData = WorldPos;

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
            WorldPos.X = InputManager.MousePos.X;
            WorldPos.Y = InputManager.MousePos.Y;
            Position = InputManager.ScreenMousePos + new Vector2(0f, 5f);
            Unhide();
        }

        if (!Hidden && InputManager.UnconsumedKeypress(Keys.Escape))
        {
            Hide();
            InputManager.ConsumeKeypress(Keys.Escape, this);
        }

        base.Update();

        if (InputManager.Clicked)
            Hide();
    }
}