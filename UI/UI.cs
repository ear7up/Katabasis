using System;
using System.Collections.Generic;

static class UI
{
    public static List<UIElement> Top;
    public static List<UIElement> TopLeft;
    public static List<UIElement> BottomLeft;
    public static List<UIElement> TopRight;
    public static List<UIElement> BottomRight;

    public static UIElement Tooltip;
    public static TextSprite TooltipText;

    public enum Position
    {
        TOP, TOP_LEFT, BOTTOM_LEFT, TOP_RIGHT, BOTTOM_RIGHT
    }

    public static void Init()
    {
        Top = new();
        TopLeft = new();
        BottomLeft = new();
        TopRight = new();
        BottomRight = new();
        Tooltip = new(/*Sprites.Tooltip*/);
        TooltipText = new TextSprite(Sprites.Font);
        TooltipText.ScaleUp(0.1f);
    }

    public static void AddElement(UIElement element, Position position)
    {
        switch (position)
        {
            case Position.TOP         : Top.Add(element); break;
            case Position.TOP_LEFT    : TopLeft.Add(element); break;
            case Position.BOTTOM_LEFT : BottomLeft.Add(element); break;
            case Position.TOP_RIGHT   : TopRight.Add(element); break;
            case Position.BOTTOM_RIGHT: BottomRight.Add(element); break;
        }
    }

    public static void Update()
    {
        // Tooltip text needs to be cleared to disable hover,
        // must be cleared before updating UI elements, which will update on hover
        TooltipText.Text = "";

        if (Tooltip.Image != null)
            Tooltip.Image.Position = InputManager.ScreenMousePos + new Vector2(15f, -15f);

        List<UIElement>[] all = { Top, TopLeft, TopRight, BottomLeft, BottomRight };
        foreach (List<UIElement> list in all)
            foreach (UIElement element in list)
                element.Update();
    }

    public static void ScaleUp(float s)
    {
        List<UIElement>[] all = { Top, TopLeft, TopRight, BottomLeft, BottomRight };
        foreach (List<UIElement> list in all)
            foreach (UIElement element in list)
                element.ScaleUp(s);
    }

    public static void ScaleDown(float s)
    {
        List<UIElement>[] all = { Top, TopLeft, TopRight, BottomLeft, BottomRight };
        foreach (List<UIElement> list in all)
            foreach (UIElement element in list)
                element.ScaleDown(s);
    }

    public static void Draw()
    {
        // Shift each image over to the right
        Vector2 relative = new Vector2(0f, 0f);
        foreach (UIElement element in TopLeft)
        {
            element.Draw(relative);
            relative.X += element.Width();
        }

        // Let elements at the top just overlap with top-left and top-right elements
        // this will probably just be a top status-bar
        relative.X = 0;
        foreach (UIElement element in Top)
        {
            element.Draw(relative);
            relative.X += element.Width();
        }

        // Shift each image over to the right, draw relative to window height
        relative.X = 0;
        foreach (UIElement element in BottomLeft)
        {
            relative.Y = Globals.WindowSize.Y - element.Height();
            element.Draw(relative);
            relative.X += element.Width();
        }

        // Shift each image to the left
        relative.X = Globals.WindowSize.X;
        relative.Y = 0;
        foreach (UIElement element in TopRight)
        {
            relative.X -= element.Width();
            element.Draw(relative);
        }

        // Shift each image to the left
        relative.X = Globals.WindowSize.X;
        foreach (UIElement element in BottomRight)
        {
            relative.X -= element.Width();
            relative.Y = Globals.WindowSize.Y - element.Height();
            element.Draw(relative);
        }

        // Resize tooltip box to fit text
        if (Tooltip.Image != null)
        {
            float ratio = (float)TooltipText.Width() / Tooltip.Image.Texture.Width;
            Tooltip.Image.SetScaleX(ratio + 0.07f);
        }

        // Draw tooltip at cursor if the text is set
        if (TooltipText.Text.Length > 0)
        {
            Tooltip.Draw(InputManager.ScreenMousePos + new Vector2(15f, -15f));
            TooltipText.Draw(InputManager.ScreenMousePos + new Vector2(16f, -14f));
        }
    }

    public static void SetTooltipText(object obj)
    {
        TooltipText.Text = obj.ToString();
    }
}