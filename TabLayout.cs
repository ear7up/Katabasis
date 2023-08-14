using System;
using System.Collections;

public class TabLayout : Layout
{
    public VBox Layout;
    public HBox TabBox;
    public Hashtable Tabs;
    public string CurrentTab;

    public TabLayout() : base()
    {
        Layout = new();
        TabBox = new();
        Layout.Add(TabBox);
        CurrentTab = "";
    }

    public override int Width()
    {
        return Layout.Width();
    }

    public override int Height()
    {
        return Layout.Height();
    }

    public override void Draw(Vector2 offset)
    {
        Vector2 margin = new Vector2(GetLeftMargin(), GetTopMargin());
        Layout.Draw(offset + margin);
    }

    public void AddTab(string tabName, UIElement button, UIElement buttonSelected, UIElement content)
    {
        button.Name = tabName;
        button.OnClick = SwitchTab;
        TabBox.Add(button);

        Tabs[button.Name] = content;
        content.Hide();

        if (CurrentTab.Length == 0)
            SwitchTab(button);
    }

    public void SwitchTab(Object clicked)
    {
        UIElement current = (UIElement)Tabs[CurrentTab];
        if (current != null)
            current.Hide();

        UIElement button = (UIElement)clicked;
        UIElement content = (UIElement)Tabs[button.Name];
        if (content != null)
            content.Unhide();
    }

    public override void Update()
    {
        Layout.Update();
    }
}