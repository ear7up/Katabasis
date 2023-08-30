using System;

public class CloseablePanel : UIElement
{
    public UIElement XButton;

    public  CloseablePanel(SpriteTexture texture) : base(texture)
    {
        XButton = new UIElement(Sprites.XButton, 1f, ClosePanel);
        XButton.HoverImage = Sprite.Create(Sprites.XButtonHover, Vector2.Zero);
    }

    public virtual void ClosePanel(Object clicked)
    {
        Hide();
    }

    public override void Draw(Vector2 offset)
    {
        base.Draw(offset);

        XButton.Position = new Vector2(Width() - XButton.Width(), 0f) + offset;
        XButton.Draw(XButton.Position);
    }

    public override void Update()
    {
        if (Hidden)
            return;
        XButton.Update();
        base.Update();
    }

    public void ScaleDownCloseButton(float s)
    {
        XButton.ScaleDown(s);
        XButton.HoverImage.ScaleDown(s);
    }
}