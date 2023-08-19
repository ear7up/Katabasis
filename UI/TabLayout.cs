using System;
using System.Collections;

public class TabLayout : Layout
{
    public VBox Layout;
    public HBox TabBox;
    public Hashtable Tabs;
    public string CurrentTab;
    public UIElement CurrentButton;

    public TabLayout() : base()
    {
        Layout = new();
        TabBox = new();
        TabBox.SetPadding(bottom: 5);
        TabBox.SetMargin(top: 1, left: 1);
        Tabs = new();
        Layout.Add(TabBox);
        CurrentTab = "";
        CurrentButton = null;
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
        base.Draw(offset);

        if (Hidden)
            return;

        Vector2 margin = new Vector2(GetLeftMargin(), GetTopMargin());
        Layout.Draw(offset + margin);
    }

    public void AddTab(string tabName, UIElement button, UIElement content)
    {
        button.SetMargin(top: 1);
        button.Name = tabName;
        button.OnClick = SwitchTab;
        TabBox.Add(button);

        Tabs[button.Name] = content;
        content.Hide();
        Layout.Add(content);

        if (CurrentTab.Length == 0)
            SwitchTab(button);
    }

    public void SwitchTab(Object clicked)
    {
        UIElement current = (UIElement)Tabs[CurrentTab];
        if (current != null)
            current.Hide();

        if (CurrentButton != null)
            CurrentButton.IsSelected = false;

        UIElement button = (UIElement)clicked;
        button.IsSelected = true;

        UIElement content = (UIElement)Tabs[button.Name];
        if (content != null)
            content.Unhide();

        CurrentTab = button.Name;
        CurrentButton = button;
    }

    public override void Update()
    {
        Layout.Update();
        base.Update();
    }
}