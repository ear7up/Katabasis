using System.Collections;
using System.Collections.Generic;

public class AccordionLayout : VBox
{
    public Hashtable Sections;
    public Layout Active;

    public Sprite SectionSprite;
    public Sprite SectionSpriteExpanded;

    public AccordionLayout()
    {
        Sections = new();
        Active = null;

        SectionSprite = Sprite.Create(Sprites.AccodionSection, Vector2.Zero);
        SectionSpriteExpanded = Sprite.Create(Sprites.AccodionSectionExpanded, Vector2.Zero);
    }

    public void AddSection(string name, Layout layout)
    {
        VBox container = new();
        container.SetMargin(left: 5, top: 1);
        container.Image = SectionSprite;
        TextSprite label = new(Sprites.Font, Color.White, Color.Black, text: name);
        label.ScaleDown(0.2f);
        container.OnClick = ToggleSection;
        container.Add(label);
        container.Add(layout);
        container.SetPadding(bottom: 5);

        container.Name = name;
        layout.Name = name;
        label.OnClick = ToggleSection;

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
        InputManager.ConsumeClick(this);
        if (clicked is TextSprite)
            ToggleSection(((TextSprite)clicked).Text);
        else
            ToggleSection(((UIElement)clicked).Name);
    }

    public void ToggleSection(string name)
    {
        Layout layout = (Layout)Sections[name];
        if (Active != null && Active != layout)
        {
            Active.Image = SectionSprite;
            Active.Elements[1].Hide();
        }

        if (layout.Elements[1].Hidden)
        {
            layout.Image = SectionSpriteExpanded;
            layout.Elements[1].Unhide();
        }
        else
        {
            layout.Image = SectionSprite;
            layout.Elements[1].Hide();
        }

        Active = layout;
    }
}