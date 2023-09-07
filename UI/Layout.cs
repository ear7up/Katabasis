using System.Collections.Generic;

public abstract class Layout : UIElement
{
    public List<UIElement> Elements;

    protected Layout(SpriteTexture texture = null) : base(texture)
    {
        Elements = new();
        //OnClick = null;
    }

    public override int GetElementCount()
    {
        return Elements.Count;
    }

    public virtual void Add(UIElement element)
    {
        Elements.Add(element);
    }

    public virtual void Add(UIElement element1, UIElement element2)
    {
        Add(element1);
        Add(element2);
    }

    public virtual void Add(UIElement element1, UIElement element2, UIElement element3)
    {
        Add(element1);
        Add(element2);
        Add(element3);
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
        if (Hidden)
            return;

        foreach (UIElement element in Elements)
            element.Update();

        // Update yourself last, we don't want to consume a click meant for a child element
        base.Update();
    }
}