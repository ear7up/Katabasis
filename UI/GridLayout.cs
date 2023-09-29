using System;
using System.Collections.Generic;
using System.Reflection;

// Crappy GridLayout implementation, assumes all objects have equal dimensions
public class GridLayout : Layout
{
    // Don't add to this directly, always use SetContent(x, y) otherwise Rows/Columns can be wrong
    public List<List<UIElement>> GridContent;

    public int Rows;
    public int Columns;

    public bool HasHeader;
    public int ElementsPerPage;
    public int Page;
    public TextSprite PageText;
    public HBox PageNumberLayout;

    public int MinHeight;
    public int MinWidth;

    // Set this for tables that have more data rows than actual display rows
    public int NumberOfDataRows;

    public GridLayout(SpriteTexture texture = null) : base(texture)
    {
        GridContent = new();
        PageText = new(Sprites.Font, Color.Black);
        PageText.SetPadding(left: 5, right: 5);
        PageText.Scale = new Vector2(0.9f, 0.9f);

        PageNumberLayout = new();
        PageNumberLayout.Add(new UIElement(Sprites.ArrowLeft, onClick: PageBack));
        PageNumberLayout.Add(PageText);
        PageNumberLayout.Add(new UIElement(Sprites.ArrowRight, onClick: PageForward));

        Page = 1;
    }

    public override int GetElementCount()
    {
        return Rows * Columns;
    }

    public void Expand(int x, int y)
    {
        // Expand to y rows
        while (y >= GridContent.Count)
            GridContent.Add(new());
        Rows = Math.Max(Rows, y + 1);

        // Expand to x columns
        List<UIElement> row = GridContent[y];
        while (x >= row.Count)
            row.Add(null);
        Columns = Math.Max(Columns, x + 1);
    }

    public void Clear()
    {
        GridContent.Clear();
        Columns = 0;
        Rows = 0;
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

        if (ElementsPerPage > 0)
        {
            ChangePageOnScroll();

            int numPages = (int)Math.Max(1, Math.Ceiling((float)NumberOfDataRows / ElementsPerPage));
            PageText.Text = $"{Page} / {numPages}";
            PageNumberLayout.Update();
        }

        if (MinHeight > 0)
        {
            int contentHeight = Height() - GetBottomPadding();
            if (contentHeight < MinHeight)
                SetPadding(bottom: MinHeight - contentHeight);
            else
                SetPadding(bottom: 1);
        }

        if (MinWidth > 0)
        {
            int contentWidth = Width() - GetRightPadding();
            if (contentWidth < MinWidth)
                SetPadding(right: MinWidth - contentWidth);
            else
                SetPadding(right: 1);
        }

        base.Update();
    }

    public virtual void ChangePageOnScroll()
    {
        if (!Hovering)
            return;

        if (InputManager.ScrollValue < 0)
            PageForward();
        else if (InputManager.ScrollValue > 0)
            PageBack();

        InputManager.ScrollValue = 0;
    }

    public void PageBack(Object clicked = null)
    {
        if (Page > 1)
            Page--;
    }

    public void PageForward(Object clicked = null)
    {
        if (Page < (float)NumberOfDataRows / ElementsPerPage)
            Page++;
    }

    public override void Draw(Vector2 offset)
    {
        // Draw the background content
        base.Draw(offset);

        if (Hidden)
            return;

        Vector2 margin = new(GetLeftMargin(), GetTopMargin());

        List<int> columnWidths = new();
        if (GridContent.Count > 0)
        {
            for (int col = 0; col < Columns; col++)
                columnWidths.Add(ColumnWidth(col));
        }

        // Draw each item in the grid
        Vector2 relative = Vector2.Zero;
        int rowNumber = 0;

        // Fake starting on the proper page
        if (NumberOfDataRows > 0)
            rowNumber = ElementsPerPage * (Page - 1);
        else if (HasHeader)
            rowNumber--;

        

        foreach (List<UIElement> row in GridContent)
        {
            // If pagination is enabled, skip rows before the page, break after the page
            if (ElementsPerPage > 0 && rowNumber > -1)
            {
                if (rowNumber >= Page * ElementsPerPage)
                    break;

                if  (rowNumber < (Page - 1) * ElementsPerPage)
                {
                    rowNumber++;
                    continue;
                }
            }

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

            rowNumber++;
        }

        if (ElementsPerPage > 0)
        {
            // Draw in the bottom right corner
            PageNumberLayout.Draw(offset + new Vector2(
                Width() - PageNumberLayout.Width(), Height() - PageNumberLayout.Height()));
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
            if (col < row.Count && row[col] != null)
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

        int width = 0;
        for (int col = 0; col < Columns; col++)
            width += ColumnWidth(col);

        return width + GetLeftPadding() + GetRightPadding() + GetLeftMargin() + GetRightMargin();
        //return (int)Math.Max(maxWidth, base.Width());
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