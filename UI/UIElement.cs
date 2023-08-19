using System;

public class UIElement
{
    public enum Direction
    {
        TOP, RIGHT, BOTTOM, LEFT
    }

    private int[] _padding;
    private int[] _margin;
    public Vector2 Scale;
    public Sprite Image;
    public Sprite SelectedImage;
    public Vector2 Position;
    public Action<Object> OnClick;
    public Action<Object> OnHover;
    public string TooltipText;
    public bool Hidden;
    public string Name;
    public bool IsSelected;
    

    public UIElement(
        Texture2D texture = null,
        float scale = 1f, 
        Action<Object> onClick = null, 
        Action<Object> onHover = null,
        string tooltip = "")
    {
        if (texture != null)
        {
            Image = new Sprite(texture, Vector2.Zero);
            Image.DrawRelativeToOrigin = false;
            Image.SetScale(scale);
        }
        else
        {
            Image = null;
        }

        OnClick = onClick;
        if (OnClick == null)
            OnClick = ConsumeClick;

        OnHover = onHover;
        TooltipText = tooltip;
        Name = "";
        Hidden = false;
        SelectedImage = null;
        IsSelected = false;
        Position = Vector2.Zero;
        
        _padding = new int[4] { 0, 0, 0, 0 };
        _margin = new int[4] { 10, 0, 0, 10 };
        Scale = Vector2.One;
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

        if (OnClick != null && InputManager.UnconsumedClick() && 
            GetBounds().Contains(InputManager.ScreenMousePos))
        {
            // Consume the click event and call the OnClick function
            InputManager.ConsumeClick();
            OnClick(this);
        }
        else if (OnHover != null && GetBounds().Contains(InputManager.ScreenMousePos))
        {
            OnHover(TooltipText);
        }
    }

    public virtual Rectangle GetBounds()
    {
        if (Image == null)
            return new Rectangle((int)Position.X, (int)Position.Y, Width(), Height());
        return Image.GetBounds();
    }

    public virtual void Draw(Vector2 offset)
    {
        Position = offset;

        if (Hidden)
            return;

        if (Image != null)
            Image.Position = offset;
        if (SelectedImage != null)
            SelectedImage.Position = offset;

        Sprite draw = Image;
        if (IsSelected && SelectedImage != null)
            draw = SelectedImage;

        // Assume all UIElement images inherit their position from their UI container
        if (draw != null)
        {
            draw.Position = offset;
            if (draw.DrawRelativeToOrigin)
                draw.Position += draw.Origin * draw.Scale;
            draw.Draw();
        }
    }

    // TODO: you can't remove padding once set
    public void SetPadding(int top = 0, int right = 0, int bottom = 0, int left = 0)
    {
        if (top != 0)
            _padding[(int)Direction.TOP] = top;
        if (right != 0)
            _padding[(int)Direction.RIGHT] = right;
        if (bottom != 0)
            _padding[(int)Direction.BOTTOM] = bottom;
        if (left != 0)
            _padding[(int)Direction.LEFT] = left;
    }

    public int GetTopMargin()
    {
        return (int)(_margin[(int)Direction.TOP] * Scale.Y);
    }

    public int GetLeftMargin()
    {
        return (int)(_margin[(int)Direction.LEFT] * Scale.X);
    }

    public int GetBottomMargin()
    {
        return (int)(_margin[(int)Direction.BOTTOM] * Scale.Y);
    }

    public int GetRightMargin()
    {
        return (int)(_margin[(int)Direction.RIGHT] * Scale.X);
    }

    public int GetTopPadding()
    {
        return (int)(_padding[(int)Direction.TOP] * Scale.Y);
    }

    public int GetLeftPadding()
    {
        return (int)(_padding[(int)Direction.LEFT] * Scale.X);
    }

    public int GetBottomPadding()
    {
        return (int)(_padding[(int)Direction.BOTTOM] * Scale.Y);
    }

    public int GetRightPadding()
    {
        return (int)(_padding[(int)Direction.RIGHT] * Scale.X);
    }

    // TODO: you can't remove margin once set
    public void SetMargin(int top = 0, int right = 0, int bottom = 0, int left = 0)
    {
        if (top != 0)
            _margin[(int)Direction.TOP] = top;
        if (right != 0)
            _margin[(int)Direction.RIGHT] = right;
        if (bottom != 0)
            _margin[(int)Direction.BOTTOM] = bottom;
        if (left != 0)
            _margin[(int)Direction.LEFT] = left;
    }

    public virtual int Width()
    {
        if (Image == null || Hidden)
            return 0;

        return GetLeftPadding() + Image.GetBounds().Width + GetRightPadding();
    }

    public virtual int Height()
    {
        if (Image == null || Hidden)
            return 0;

        return GetTopPadding() + Image.GetBounds().Height + GetBottomPadding();
    }

    public virtual void ScaleUp(float s)
    {
        Scale *= new Vector2(1 + s, 1 + s);
        if (Image != null)
            Image.ScaleUp(s);
    }

    public virtual void ScaleDown(float s)
    {
        Scale *= new Vector2(1 - s, 1 - s);
        if (Image != null)
            Image.ScaleDown(s);
    }

    public void AddSelectedImage(Texture2D texture)
    {
        Sprite selected = new Sprite(texture, Vector2.Zero);
        selected.DrawRelativeToOrigin = false;
        SelectedImage = selected;
    }

    public void ConsumeClick(Object clicked)
    {
        InputManager.ConsumeClick();
    }
}