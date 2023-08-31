using System;

public class UIElement
{
    public enum Direction
    {
        TOP, RIGHT, BOTTOM, LEFT
    }

    private int[] _padding;
    private int[] _margin;
    public Vector2 Scale { get; set; }
    public Sprite Image;
    public bool Hovering;
    public Sprite HoverImage;
    public Sprite SelectedImage;
    public Vector2 Position { get; set; }
    public Action<Object> OnClick;
    public Action<Object> OnHover;
    public string TooltipText;
    public bool Hidden { get; set; }
    public string Name;
    public bool IsSelected { get; set; }
    

    public UIElement(
        SpriteTexture texture = null,
        float scale = 1f, 
        Action<Object> onClick = null, 
        Action<Object> onHover = null,
        string tooltip = "")
    {
        if (texture != null)
        {
            Image = Sprite.Create(texture, Vector2.Zero);
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

        HoverImage = null;
        Hovering = false;

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

    public virtual void ToggleHidden()
    {
        if (Hidden)
            Unhide();
        else
            Hide();
    }

    public virtual int GetElementCount()
    {
        return 1;
    }

    public virtual void Update()
    {
        // Don't process clicks or hovers on hidden elements
        if (Hidden)
            return;

        Hovering = false;

        if (OnClick != null && InputManager.UnconsumedClick() && 
            GetBounds().Contains(InputManager.ScreenMousePos))
        {
            // Consume the click event and call the OnClick function
            InputManager.ConsumeClick(this);
            OnClick(this);
        }
        else if (GetBounds().Contains(InputManager.ScreenMousePos))
        {
            if (OnHover != null)
                OnHover(TooltipText);
            Hovering = true;
        }
    }

    public virtual Rectangle GetBounds()
    {
        Vector2 pos = GetRealPosition();
        if (Image == null)
            return new Rectangle((int)pos.X, (int)pos.Y, Width(), Height());
        return Image.GetBounds();
    }

    public Vector2 GetPadding()
    {
        return new Vector2(GetLeftPadding(), GetTopPadding());
    }

    public Vector2 GetRealPosition()
    {
        return Position + GetPadding();
    }

    public virtual void Draw(Vector2 offset)
    {
        Position = new Vector2(offset.X, offset.Y);

        if (Hidden)
            return;

        if (Image != null)
            Image.Position = offset;
        if (SelectedImage != null)
            SelectedImage.Position = offset;

        Sprite draw = Image;
        if (IsSelected && SelectedImage != null)
            draw = SelectedImage;
        else if (Hovering && HoverImage != null)
            draw = HoverImage;

        Vector2 padding = new Vector2(GetLeftPadding(), GetTopPadding());

        // Assume all UIElement images inherit their position from their UI container
        if (draw != null)
        {
            draw.Position = offset + padding;
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

    public void AddSelectedImage(SpriteTexture texture)
    {
        Sprite selected = Sprite.Create(texture, Vector2.Zero);
        selected.DrawRelativeToOrigin = false;
        SelectedImage = selected;
    }

    public void ConsumeClick(Object clicked)
    {
        InputManager.ConsumeClick(this);
    }
}