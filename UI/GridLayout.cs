using System;
using System.Collections.Generic;

// Crappy GridLayout implementation, assumes all objects have equal dimensions
public class GridLayout : Layout
{
    public List<List<UIElement>> GridContent;

    public GridLayout(Texture2D texture = null) : base(texture)
    {
        GridContent = new();
    }

    public void Expand(int x, int y)
    {
        // Expand to y rows
        while (y >= GridContent.Count)
            GridContent.Add(new());

        // Expand to x columns
        List<UIElement> row = GridContent[y];
        while (x >= row.Count)
            row.Add(null);
    }

    public override void Hide()
    {
        Hidden = true;
        foreach (List<UIElement> row in GridContent)
            foreach (UIElement element in row)
                element.Hide();
    }

    public override void Unhide()
    {
        Hidden = false;
        foreach (List<UIElement> row in GridContent)
            foreach (UIElement element in row)
                element.Unhide();
    }

    public void SetContent(int x, int y, UIElement content)
    {
        Expand(x, y);
        GridContent[y][x] = content;
    }

    public override void Update()
    {
        foreach (List<UIElement> row in GridContent)
            foreach (UIElement element in row)
                element.Update();
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        // Draw the background content
        base.Draw(offset);

        Vector2 margin = new(GetLeftMargin(), GetTopMargin());

        List<int> columnWidths = new();
        if (GridContent.Count > 0)
        {
            int col = 0;
            foreach (UIElement _ in GridContent[0])
                columnWidths.Add(ColumnWidth(col++));
        }

        // Draw each item in the grid
        Vector2 relative = Vector2.Zero;
        foreach (List<UIElement> row in GridContent)
        {
            int col = 0;
            relative.X = 0f;
            foreach (UIElement element in row)
            {
                element.Draw(offset + relative + margin);
                relative.X += columnWidths[col++];
            }

            // Assume they're the same height
            if (row.Count > 0)
                relative.Y += row[0].Height();
        }
    }
    
    public int ColumnWidth(int col)
    {
        if (Hidden || GridContent.Count == 0)
            return 0;

        if (col >= GridContent[0].Count)
            return 0;

        float maxWidth = 0f;
        foreach (List<UIElement> row in GridContent)
            if (col < row.Count)
                maxWidth = Math.Max(maxWidth, row[col].Width());

        return (int)maxWidth;
    }

    public override void ScaleUp(float s)
    {
        base.ScaleUp(s);
        foreach (List<UIElement> row in GridContent)
            foreach (UIElement element in row)
                element.ScaleUp(s);
    }

    public override void ScaleDown(float s)
    {
        base.ScaleDown(s);
        foreach (List<UIElement> row in GridContent)
            foreach (UIElement element in row)
                element.ScaleDown(s);
    }

    public override int Width()
    {
        if (Hidden)
            return 0;

        float maxWidth = 0f;
        foreach (List<UIElement> row in GridContent)
        {
            float sumWidth = 0f;
            foreach (UIElement element in row)
                sumWidth += element.Width();
            maxWidth = Math.Max(maxWidth, sumWidth);
        }
        maxWidth += GetLeftPadding() + GetRightPadding();
        return (int)Math.Max(maxWidth, base.Width());
    }

    public override int Height()
    {
        if (Hidden)
            return 0;

        float sumHeight = 0f;
        foreach (List<UIElement> row in GridContent)
        {
            float maxHeight = 0f;
            foreach (UIElement element in row)
                maxHeight = Math.Max(maxHeight, element.Height());
            sumHeight += maxHeight;
        }
        sumHeight += GetTopPadding() + GetBottomPadding();
        return (int)Math.Max(sumHeight, base.Height());
    }
}