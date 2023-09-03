using System;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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
    public Vector2 DefaultPosition { get; set; }
    public Action<Object> OnClick;
    public Action<Object> OnHover;
    public string TooltipText;
    public bool Hidden { get; set; }
    public string Name;
    public bool IsSelected { get; set; }
    public UIElement HoverElement;
    public Vector2 AnimationVelocity { get; set; }
    public Vector2 AnimationAcceleration { get; set; }
    public Vector2 AnimationDestination { get; set; }
    public Action AnimationOnComplete { get; set; }
    public object UserData; 

    public UIElement(
        SpriteTexture texture = null,
        float scale = 1f, 
        Action<Object> onClick = null, 
        Action<Object> onHover = null,
        string tooltip = "",
        UIElement hoverElement = null,
        Sprite hoverImage = null)
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
        HoverElement = hoverElement;
        HoverImage = hoverImage;
        Name = "";
        Hidden = false;
        SelectedImage = null;

        Hovering = false;

        IsSelected = false;
        Position = Vector2.Zero;
        
        _padding = new int[4] { 0, 0, 0, 0 };
        _margin = new int[4] { 10, 0, 0, 10 };
        Scale = Vector2.One;

        AnimationAcceleration = Vector2.Zero;
        AnimationVelocity = Vector2.Zero;
        AnimationDestination = Vector2.Zero;
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

    public void SetDefaultPosition (Vector2 pos)
    {
        DefaultPosition = pos;
        Position = pos;
    }

    public virtual void Update()
    {
        // Don't process clicks or hovers on hidden elements
        if (Hidden)
            return;

        AnimationMove();

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

        if (Hovering && HoverElement != null)
            HoverElement.Update();
    }

    public virtual Rectangle GetBounds()
    {
        Vector2 pos = GetRealPosition();
        if (Image == null)
            return new Rectangle((int)pos.X, (int)pos.Y, Width(), Height());

        Rectangle imageBounds = Image.GetBounds();
        return new Rectangle((int)pos.X, (int)pos.Y, imageBounds.Width, imageBounds.Height);
    }

    public Vector2 GetPadding()
    {
        return new Vector2(GetLeftPadding(), GetTopPadding());
    }

    public Vector2 GetRealPosition()
    {
        return Position + GetPadding();
    }

    public void SetAnimation(
        Vector2 destination, 
        float speed, 
        float acceleration,
        Action onComplete = null)
    {
        AnimationDestination = destination;

        AnimationVelocity = destination - Position;
        AnimationVelocity.Normalize();
        AnimationVelocity *= speed;

        AnimationAcceleration = acceleration * AnimationVelocity;

        AnimationOnComplete = onComplete;
    }

    public void AnimationMove()
    {
        if (AnimationVelocity != Vector2.Zero)
        {
            float distance = Vector2.Distance(Position, AnimationDestination);
            float stepSize = AnimationVelocity.Length() * Globals.Time;

            if (distance < stepSize)
            {
                AnimationVelocity *= distance / stepSize;
                Position += AnimationVelocity * Globals.Time;

                AnimationVelocity = Vector2.Zero;
                AnimationDestination = Vector2.Zero;

                AnimationOnComplete?.Invoke();
            }
            else
            {
                Position += AnimationVelocity * Globals.Time;
                AnimationVelocity += AnimationAcceleration;
            }
        }
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

        if (Hovering && HoverElement != null)
        {
            HoverElement.Position = offset;
            HoverElement.Draw(HoverElement.Position);
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
        if (Hidden)
            return 0;

        int w = 0;
        if (Image != null)
            w = Image.GetBounds().Width;

        return GetLeftPadding() + w + GetRightPadding();
    }

    public virtual int Height()
    {
        if (Hidden)
            return 0;

        int h = 0;
        if (Image != null)
            h = Image.GetBounds().Height;

        return GetTopPadding() + h + GetBottomPadding();
    }

    public virtual void ScaleUp(float s)
    {
        Scale *= new Vector2(1 + s, 1 + s);
        if (Image != null)
            Image.ScaleUp(s);

        if (HoverImage != null)
            HoverImage.ScaleUp(s);

        if (SelectedImage != null)
            SelectedImage.ScaleUp(s);

        if (HoverElement != null)
            HoverElement.ScaleUp(s);
    }

    public virtual void ScaleDown(float s)
    {
        Scale *= new Vector2(1 - s, 1 - s);
        if (Image != null)
            Image.ScaleDown(s);

        if (HoverImage != null)
            HoverImage.ScaleDown(s);

        if (SelectedImage != null)
            SelectedImage.ScaleDown(s);

        if (HoverElement != null)
            HoverElement.ScaleDown(s);
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