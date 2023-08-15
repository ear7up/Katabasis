using System;

public class OverlapLayout : Layout
{
    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        // Assume all UIElement images inherit their position from their UI container
        if (Image != null)
        {
            Image.Position = offset;
            Image.Draw();
        }

        foreach (UIElement element in Elements)
            element.Draw(offset);
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