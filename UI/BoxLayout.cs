using System;
using System.Collections.Generic;

public class BoxLayout : Layout
{
    public enum LayoutDirection { VERTICAL, HORIZONTAL }

    public LayoutDirection Layout;

    protected BoxLayout(LayoutDirection layout) : base()
    {
        Layout = layout;
    }

    public override int Width()
    {
        if (Elements.Count == 0 || Hidden)
            return 0;

        int sumWidth = 0;
        int maxWidth = 0;
        foreach (UIElement element in Elements)
        {
            int width = element.Width();
            maxWidth = Math.Max(maxWidth, width);
            sumWidth += width;
        }

        if (Image != null)
            maxWidth = Math.Max(maxWidth, GetBounds().Width);

        int realWidth = 0;
        if (Layout == LayoutDirection.VERTICAL)
            realWidth = maxWidth;
        else if (Layout == LayoutDirection.HORIZONTAL)
            realWidth = sumWidth;

        return GetLeftPadding() + realWidth + GetRightPadding();
    }

    public override int Height()
    {
        if (Elements.Count == 0 || Hidden)
            return 0;

        int sumHeight = 0;
        int maxHeight = 0;
        foreach (UIElement element in Elements)
        {
            int height = element.Height();
            maxHeight = Math.Max(maxHeight, height);
            sumHeight += height;
        }

        if (Image != null)
            maxHeight = Math.Max(maxHeight, GetBounds().Height);

        int realHeight = 0;
        if (Layout == LayoutDirection.VERTICAL)
            realHeight = sumHeight;
        else if (Layout == LayoutDirection.HORIZONTAL)
            realHeight = maxHeight;

        return GetTopPadding() + realHeight + GetBottomPadding();
    }

    public override void Draw(Vector2 offset)
    {
        base.Draw(offset);

        if (Hidden)
            return;

        Vector2 margin = new Vector2(GetLeftMargin(), GetTopMargin());

        // Draw each element along the x-axis for HORIZONTAL, y-axis for VERTICAL
        Vector2 relative = Vector2.Zero;
        foreach (UIElement element in Elements)
        {
            element.Draw(offset + margin + relative);

            if (Layout == LayoutDirection.HORIZONTAL)
                relative.X += element.Width();
            else if (Layout == LayoutDirection.VERTICAL)
                relative.Y += element.Height();
        }
    }
}

public class HBox : BoxLayout
{
    public HBox() : base(LayoutDirection.HORIZONTAL)
    {

    }
}

public class VBox : BoxLayout
{
    public VBox() : base(LayoutDirection.VERTICAL)
    {

    }
}