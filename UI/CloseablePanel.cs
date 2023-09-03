using System;

public class CloseablePanel : VBox
{
    public UIElement TopBar;
    public UIElement XButton;
    public Vector2 DragStart;
    public bool Draggable;

    public CloseablePanel(SpriteTexture texture, bool draggable = true) : base(texture)
    {
        Draggable = draggable;
        DragStart = Vector2.Zero;
        XButton = new UIElement(Sprites.XButton, 1f, ClosePanel);
        XButton.HoverImage = Sprite.Create(Sprites.XButtonHover, Vector2.Zero);

        // Invisible element representing the draggable window bar at the top
        TopBar = new();
        TopBar.SetPadding(bottom: 50, right: Width());
    }

    public virtual void ClosePanel(Object clicked)
    {
        Hide();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;

        base.Draw(offset);

        XButton.Position = new Vector2(Width() - XButton.Width(), 0f) + offset;
        XButton.Draw(XButton.Position);

        // Keep the top bar at the top-left of the panel
        TopBar.Position = offset;
    }

    public override void Update()
    {
        if (Hidden)
            return;

        if (Draggable)
            HandleDragInputs();

        XButton.Update();
        base.Update();
    }

    public void HandleDragInputs()
    {
        bool mouseOverImage = TopBar.GetBounds().Contains(InputManager.ScreenMousePos);

        if (InputManager.ClickAndHold && mouseOverImage)
        {
            DragStart = InputManager.ScreenMousePos;
            InputManager.ConsumeHold(this);
        }

        if (DragStart != Vector2.Zero && !InputManager.MouseDown)
            DragStart = Vector2.Zero;

        if (InputManager.MouseDown && DragStart != Vector2.Zero)
        {
            Position += InputManager.ScreenMousePos - DragStart;;
            DragStart = InputManager.ScreenMousePos;
        }
    }
}