using System.Collections.Generic;

public abstract class Layout : UIElement
{
    public List<UIElement> Elements;

    protected Layout(Texture2D texture = null) : base(texture)
    {
        Elements = new();
    }

    public virtual void Add(UIElement element)
    {
        Elements.Add(element);
    }

    public override void ScaleUp(float s)
    {
        base.ScaleUp(s);
        foreach (UIElement element in Elements)
            element.ScaleUp(s);
    }

    public override void ScaleDown(float s)
    {
        base.ScaleDown(s);
        Scale -= new Vector2(s, s);
        foreach (UIElement element in Elements)
            element.ScaleDown(s);
    }

    public UIElement Pop()
    {
        if (Elements.Count == 0)
            return null;
        UIElement last = Elements[Elements.Count - 1];
        Elements.RemoveAt(Elements.Count - 1);
        return last;
    }

    public override void Update()
    {
        foreach (UIElement element in Elements)
            element.Update();
    }
}