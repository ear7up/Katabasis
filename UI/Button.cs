using System;

public class Button : OverlapLayout
{
    public SpriteTexture DefaultTexture;
    public SpriteTexture PushedTexture;

    public UIElement ButtonElement;
    public TextSprite ButtonText;

    public Button(
        SpriteTexture texture,
        SpriteTexture hoverTexture = null,
        SpriteTexture pushedTexture = null,
        string buttonText = null,
        float scale = 1f, 
        Action<Object> onClick = null, 
        Action<Object> onHover = null,
        string tooltip = "") : base()
    {
        DefaultTexture = hoverTexture;
        PushedTexture = pushedTexture;

        ButtonElement = new(texture, scale, onClick, onHover, hoverImage: hoverTexture);
        Add(ButtonElement);

        if (buttonText != null)
        {
            ButtonText = new(Sprites.SmallFont, Color.White, Color.Black, text: buttonText);
            ButtonText.SetPadding(top: 10, left: 15);
            ButtonText.OnClick = null;
            Add(ButtonText);
        }
    }

    public void SetScale(float s)
    {
        ButtonElement.Image.SetScale(s);
        ButtonElement.HoverImage.SetScale(s);
    }

    public override void Update()
    {
        // Don't process clicks or hovers on hidden elements
        if (Hidden)
            return;

        base.Update();
        
        // If the mouse is down while hovering over the button, switch to the pushed texture
        if (PushedTexture != null && Hovering && InputManager.MouseDown)
        {
            // Set pushed texture while mouse is held down over the image
            ButtonElement.HoverImage.SetNewSpriteTexture(PushedTexture);
        }
        else
        {
            ButtonElement.HoverImage.SetNewSpriteTexture(DefaultTexture);
        }
    }
}