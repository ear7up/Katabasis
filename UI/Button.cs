using System;

public class Button : UIElement
{
    public SpriteTexture DefaultTexture;
    public SpriteTexture HoverTexture;
    public SpriteTexture PushedTexture;

    public Button(
        SpriteTexture texture,
        SpriteTexture hoverTexture = null,
        SpriteTexture pushedTexture = null,
        float scale = 1f, 
        Action<Object> onClick = null, 
        Action<Object> onHover = null,
        string tooltip = "") : base(texture, scale, onClick, onHover)
    {
        DefaultTexture = texture;
        HoverTexture = hoverTexture;
        PushedTexture = pushedTexture;
    }

    public override void Update()
    {
        // Don't process clicks or hovers on hidden elements
        if (Hidden || Image == null)
            return;

        bool mouseOverImage = Image.GetBounds().Contains(InputManager.MousePos);

        if (!mouseOverImage)
        {
            Image.SetNewSpriteTexture(DefaultTexture);
            return;
        }

        if (OnClick != null && InputManager.UnconsumedClick())
        {
            // Consume the click event and call the OnClick function
            InputManager.ConsumeClick(this);
            OnClick(this);
        }
        
        if (PushedTexture != null && InputManager.MouseDown)
        {
            // Set pushed texture while mouse is held down over the image
            Image.SetNewSpriteTexture(PushedTexture);
        }
        else if (OnHover != null)
        {
            OnHover(TooltipText);
            if (HoverTexture != null)
                Image.SetNewSpriteTexture(HoverTexture);
        }
        else
        {
            Image.SetNewSpriteTexture(DefaultTexture);
        }
    }
}