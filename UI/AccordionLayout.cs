using System.Collections;
using System.Collections.Generic;

public class AccordionLayout : VBox
{
    public Hashtable Sections;
    public Layout Active;

    public AccordionLayout()
    {
        Sections = new();
        Active = null;
    }

    public void AddSection(string name, Layout layout)
    {
        // Make a container with a 
        VBox container = new();
        TextSprite label = new(Sprites.Font, text: name);
        label.ScaleDown(0.2f);
        label.OnClick = ToggleSection;
        container.Add(label);
        container.Add(layout);
        container.SetPadding(bottom: 15);
        layout.Hide();

        Sections[name] = container;
        Elements.Add(container);

        if (Active == null)
        {
            ToggleSection(name);
            Active = container;
        }
    }

    // Luckily the text of the TextSprite is also the section name
    public void ToggleSection(object clicked)
    {
        InputManager.ConsumeClick();
        ToggleSection(((TextSprite)clicked).Text);
    }

    public void ToggleSection(string name)
    {
        Layout layout = (Layout)Sections[name];
        if (Active != null && Active != layout)
            Active.Elements[1].Hide();

        if (layout.Elements[1].Hidden)
            layout.Elements[1].Unhide();
        else
            layout.Elements[1].Hide();

        Active = layout;
    }
}