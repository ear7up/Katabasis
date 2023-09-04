using System;
using System.Linq.Expressions;


public class ScrollPane : VBox
{
    public Vector2 ScrollOffset;
    public int MyHeight;
    public int MyMinWidth;
    public UIElement ScrollBar;

    public ScrollPane(int height, int minWidth, SpriteTexture texture = null) : base(texture)
    {
        ScrollOffset = Vector2.Zero;
        MyHeight = height;
        ScrollBar = new();
        MyMinWidth = minWidth;
    }

    public override int Height()
    {
        if (Hidden)
            return 0;
        return MyHeight;
    }

    public override int Width()
    {
        if (Hidden)
            return 0;
        return Math.Max(base.Width(), MyMinWidth);
    }

    public override void Draw(Vector2 offset)
    {
        Position = new Vector2(offset.X, offset.Y);

        if (Hidden)
            return;

        if (Image != null)
        {
            Image.Position = offset;
            Image.Draw();
        }

        Vector2 relative = new Vector2(GetLeftMargin(), GetTopMargin());
        Vector2 pos = relative;
        foreach (UIElement element in Elements)
        {
            int eHeight = element.Height();
            pos.Y += eHeight;

            // We've scrolled past this element
            if (pos.Y < ScrollOffset.Y)
                continue;

            // Everything past this point is not in the scroll window
            if (pos.Y > ScrollOffset.Y + MyHeight)
                break;

            element.Draw(offset + relative);
            relative.Y += eHeight;
        }
    }

    public override void Update()
    {
        base.Update();

        if (!Hovering)
            return;

        if (InputManager.ScrollValue < 0 && ScrollOffset.Y < base.Height() - MyHeight)
            ScrollOffset.Y += MyHeight;
        else if (InputManager.ScrollValue > 0 && ScrollOffset.Y > 0)
            ScrollOffset.Y -= MyHeight;

        InputManager.ScrollValue = 0;

        // TODO: Update ScrollBar
    }
}