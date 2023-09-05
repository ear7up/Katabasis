using System;

public class PersonPanel : CloseablePanel
{
    Person PersonTracking;

    public VBox MyLayout;
    public HBox TopPart;
    public GridLayout SkillsLayout;

    public UIElement PersonIcon;
    public TextSprite TaskDescription;
    public TextSprite InventoryDescription;
    public TextSprite PersonDescription;

    public PersonPanel(Person person) : base(Sprites.TallPanel)
    {
        PersonTracking = person;

        MyLayout = new();
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
            string skillName = Globals.Title(skill.ToString());
            if (skillName == "None")
                skillName = "General";
            TextSprite skillText = new TextSprite(Sprites.Font, text: skillName);
            skillText.SetPadding(right: 20);
            skillText.ScaleDown(0.4f);
            SkillsLayout.SetContent(0, y, skillText);

            OverlapLayout levelDisplay = new();
            levelDisplay.SetMargin(top: 1);
            
            UIElement startLevelBar = new(Sprites.VerticalGreenBar);
            UIElement levelBar = new(Sprites.VerticalGreenBar);
            levelBar.Image.SpriteColor = Color.Goldenrod;
            TextSprite levelText = new(Sprites.SmallFont);
            levelDisplay.Add(levelBar);
            levelDisplay.Add(startLevelBar);
            levelDisplay.Add(levelText);
            SkillsLayout.SetContent(1, y, levelDisplay);
            y++;
        }

        TopPart.Add(PersonIcon);
        TopPart.Add(PersonDescription);

        MyLayout.Add(TopPart);
        MyLayout.Add(SkillsLayout);
        MyLayout.Add(TaskDescription);
        MyLayout.Add(InventoryDescription);

        MyLayout.SetMargin(top: 50, left: 40);
        SkillsLayout.SetPadding(bottom: 20);
        TopPart.SetPadding(bottom: 20);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X - Width(), 50f));
    }

    public void SetPerson(Person p)
    {
        PersonTracking = p;
        if (p == null)
            Hide();
        else
            Unhide();
    }

    public override void Update()
    {
        if (Hidden || PersonTracking == null)
            return;

        PersonIcon.Image = Sprite.Create(PersonTracking.GetSpriteTexture(), Vector2.Zero);
        PersonIcon.Image.SetScale(0.15f);

        PersonDescription.Text = PersonTracking.Describe();

        foreach (SkillLevel s in PersonTracking.Skills._list)
        {
            OverlapLayout levelDisplay = (OverlapLayout)SkillsLayout.GridContent[(int)s.skill][1];
            levelDisplay.Elements[0].Image.SetScaleX(200f * (s.level / 100f));
            levelDisplay.Elements[1].Image.SetScaleX(200f * (s.startLevel / 100f));
            ((TextSprite)levelDisplay.Elements[2]).Text = s.level.ToString();
        }

        TaskDescription.Text = "[ Tasks ]\n" + PersonTracking.DescribeTasks();

        InventoryDescription.Text = "[ Inventory ]\n" + PersonTracking.PersonalStockpile.ToString();

        XButton.Update();
        MyLayout.Update();
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden || PersonTracking == null)
            return;

        base.Draw(offset);
        MyLayout.Draw(offset);
    }

    // Override to disable camera following when the panel closes
    public override void ClosePanel(object clicked)
    {
        PersonTracking = null;
        Globals.Model.GameCamera.Unfollow();
        base.ClosePanel(clicked);
    }
}