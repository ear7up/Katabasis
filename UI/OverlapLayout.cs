using System;
using System.Collections.Generic;

public class OverlapLayout : Layout
{
    public OverlapLayout(SpriteTexture texture = null) : base(texture)
    {

    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);

        Vector2 margin = new Vector2(GetLeftMargin(), GetTopMargin());

        foreach (UIElement element in Elements)
            element.Draw(offset + margin);
    }

    public override int Width()
    {
        if (Elements.Count == 0 || Hidden)
            return 0;

        int maxWidth = 0;
        foreach (UIElement element in Elements)
        {
            int width = element.Width();
            maxWidth = Math.Max(maxWidth, width);
        }

        if (Image != null)
            maxWidth = Math.Max(maxWidth, Image.GetBounds().Width);

        return maxWidth;
    }

    public override int Height()
    {
        if (Elements.Count == 0 || Hidden)
            return 0;

        int maxHeight = 0;
        foreach (UIElement element in Elements)
        {
            int height = element.Height();
            maxHeight = Math.Max(maxHeight, height);
        }

        if (Image != null)
            maxHeight = Math.Max(maxHeight, Image.GetBounds().Height);

        return maxHeight;
    }
}