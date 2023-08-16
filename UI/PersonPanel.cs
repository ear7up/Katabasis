using System;

public class PersonPanel : UIElement
{
    Person PersonTracking;

    public VBox Layout;
    public HBox TopPart;
    public GridLayout SkillsLayout;

    public UIElement PersonIcon;
    public TextSprite TaskDescription;
    public TextSprite InventoryDescription;
    public TextSprite PersonDescription;

    public PersonPanel(Person person) : base(Sprites.TallPanel)
    {
        PersonTracking = person;

        Layout = new();
        TopPart = new();
        SkillsLayout = new();

        PersonIcon = new();
        TaskDescription = new(Sprites.Font);
        TaskDescription.ScaleDown(0.4f);
        InventoryDescription = new(Sprites.Font);
        InventoryDescription.ScaleDown(0.4f);
        PersonDescription = new(Sprites.Font);
        PersonDescription.ScaleDown(0.4f);

        int y = 0;
        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            TextSprite skillText = new TextSprite(Sprites.Font, text: Globals.Title(skill.ToString()));
            skillText.SetPadding(right: 20);
            skillText.ScaleDown(0.4f);
            SkillsLayout.SetContent(0, y, skillText);

            OverlapLayout levelDisplay = new();
            UIElement levelBar = new(Sprites.VerticalGreenBar);
            TextSprite levelText = new(Sprites.Font);
            levelText.ScaleDown(0.45f);
            levelDisplay.Add(levelBar);
            levelDisplay.Add(levelText);
            SkillsLayout.SetContent(1, y, levelDisplay);
            y++;
        }

        TopPart.Add(PersonIcon);
        TopPart.Add(PersonDescription);

        Layout.Add(TopPart);
        Layout.Add(SkillsLayout);
        Layout.Add(TaskDescription);
        Layout.Add(InventoryDescription);

        Layout.SetMargin(top: 50, left: 40);
        SkillsLayout.SetPadding(bottom: 20);
        TopPart.SetPadding(bottom: 20);
    }

    public void SetPerson(Person p)
    {
        PersonTracking = p;
    }

    public override void Update()
    {
        if (Hidden || PersonTracking == null)
            return;

        PersonIcon.Image = new Sprite(PersonTracking.image, Vector2.Zero);
        PersonIcon.Image.SetScale(0.15f);

        PersonDescription.Text = PersonTracking.Describe();

        foreach (SkillLevel s in PersonTracking.Skills)
        {
            OverlapLayout skillBarLayout = (OverlapLayout)SkillsLayout.GridContent[(int)s.skill][1];
            skillBarLayout.Elements[0].Image.SetScaleX(200f * (s.level / 100f));
            ((TextSprite)skillBarLayout.Elements[1]).Text = s.level.ToString();
        }

        TaskDescription.Text = "[ Tasks ]\n" + PersonTracking.DescribeTasks();

        InventoryDescription.Text = "[ Inventory ]\n" + PersonTracking.PersonalStockpile.ToString();

        Layout.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden || PersonTracking == null)
            return;

        base.Draw(offset);
        Layout.Draw(offset);
    }
}