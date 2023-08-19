using System;

public class Button : UIElement
{
    public Texture2D DefaultTexture;
    public Texture2D HoverTexture;
    public Texture2D PushedTexture;

    public Button(
        Texture2D texture,
        Texture2D hoverTexture = null,
        Texture2D pushedTexture = null,
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
            Image.Texture = DefaultTexture;
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
            Image.Texture = PushedTexture;
        }
        else if (OnHover != null)
        {
            OnHover(TooltipText);
            if (HoverTexture != null)
                Image.Texture = HoverTexture;
        }
        else
        {
            Image.Texture = DefaultTexture;
        }
    }
}