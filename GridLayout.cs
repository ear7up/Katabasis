using System.Collections.Generic;

// Crappy GridLayout implementation, assumes all objects have equal dimensions
public class GridLayout : UIElement
{
    public List<List<UIElement>> GridContent;

    public GridLayout(Texture2D texture) : base(texture)
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
    }

    public override void Draw(Vector2 offset)
    {
        // Draw the background content
        base.Draw(offset);

        // Draw each item in the grid
        Vector2 relative = Vector2.Zero;
        foreach (List<UIElement> row in GridContent)
        {
            relative.X = 0f;
            foreach (UIElement element in row)
            {
                // Left-align all the objects, don't bother aligning them to a grid properly
                element.Draw(offset + relative + new Vector2(Margin[(int)Direction.LEFT], Margin[(int)Direction.TOP]));
                relative.X += element.Width();
            }

            // Assume they're the same height
            relative.Y += row[0].Height();
        }
    }
}