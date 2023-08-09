using System;

public class UIElement
{
    public enum Direction
    {
        TOP, RIGHT, BOTTOM, LEFT
    }

    public int[] Padding;
    public int[] Margin;
    public Sprite Image;
    public UIElement Content;
    public Action OnClick;
    public Action<Object> OnHover;
    public string TooltipText;
    public bool Hidden;

    public UIElement(
        Texture2D texture, 
        float scale = 1f, 
        Action onClick = null, 
        Action<Object> onHover = null,
        string tooltip = "")
    {
        Image = new Sprite(texture, Vector2.Zero);
        Image.DrawRelativeToOrigin = false;
        Image.Scale = scale;
        OnClick = onClick;
        OnHover = onHover;
        TooltipText = tooltip;
        Hidden = false;

        Padding = new int[4] { 0, 0, 0, 0 };
        Margin = new int[4] { 10, 0, 0, 10 };
        Content = null;
    }

    public void SetContent(UIElement content)
    {
        Content = content;
    }

    public virtual void Hide()
    {
        Hidden = true;
    }

    public virtual void Unhide()
    {
        Hidden = false;
    }

    public virtual void Update()
    {
        // Don't process clicks or hovers on hidden elements
        if (Hidden)
            return;

        if (OnClick != null && InputManager.Clicked && Image.GetBounds().Contains(InputManager.MousePos))
        {
            // Consume the click event and call the OnClick function
            InputManager.ConsumeClick();
            OnClick();
            
        }
        else if (OnHover != null && Image.GetBounds().Contains(InputManager.MousePos))
        {
            OnHover(TooltipText);
        }
    }

    public virtual void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        // Assume all UIElement images inherit their position from their UI container
        Image.Position = offset;
        Image.Draw();

        if (Content != null)
        {
            // TODO: how to implement bottom and right margins?
            Vector2 marginOffset = new(Margin[(int)Direction.LEFT], Margin[(int)Direction.TOP]);

            // Content inherits container's position, add margin so it doesn't overlap borders
            Content.Image.Position = Image.Position;
            Content.Draw(marginOffset);
        }
    }

    // TODO: you can't remove padding once set
    public void SetPadding(int top = 0, int right = 0, int bottom = 0, int left = 0)
    {
        if (top != 0)
            Padding[(int)Direction.TOP] = top;
        if (right != 0)
            Padding[(int)Direction.RIGHT] = right;
        if (bottom != 0)
            Padding[(int)Direction.BOTTOM] = bottom;
        if (left != 0)
            Padding[(int)Direction.LEFT] = left;
    }

    // TODO: you can't remove margin once set
    public void SetMargin(int top = 0, int right = 0, int bottom = 0, int left = 0)
    {
        if (top != 0)
            Margin[(int)Direction.TOP] = top;
        if (right != 0)
            Margin[(int)Direction.RIGHT] = right;
        if (bottom != 0)
            Margin[(int)Direction.BOTTOM] = bottom;
        if (left != 0)
            Margin[(int)Direction.LEFT] = left;
    }

    public int Width()
    {
        return Padding[(int)Direction.LEFT] + 
               (int)(Image.GetBounds().Width) + 
               Padding[(int)Direction.RIGHT];
    }

    public int Height()
    {
        return Padding[(int)Direction.TOP] + 
               (int)(Image.GetBounds().Height) + 
               Padding[(int)Direction.BOTTOM];
    }
}