using System;

public class PersonPanel : CloseablePanel
{
    Person PersonTracking;

    public TabLayout MyLayout;
    public HBox TopPart;
    public GridLayout SkillsLayout;
    public VBox HealthLayout;

    public UIElement PersonIcon;
    public TextSprite TaskDescription;
    public TextSprite InventoryDescription;
    public TextSprite PersonDescription;

    public PersonPanel(Person person) : base(Sprites.TallPanel)
    {
        PersonTracking = person;
        MyLayout = new();
        MyLayout.SetMargin(top: 40, left: 30);

        VBox overviewLayout = new();

        TopPart = new();
        TopPart.SetPadding(bottom: 10);

        PersonIcon = new();
        TaskDescription = new(Sprites.SmallFont, Color.White, Color.Black);
        InventoryDescription = new(Sprites.SmallFont, Color.White, Color.Black);
        PersonDescription = new(Sprites.SmallFont);

        TopPart.Add(PersonIcon);
        TopPart.Add(PersonDescription);

        DarkPanel taskLayout = new("Tasks");
        taskLayout.AddContent(TaskDescription);
        taskLayout.SetPadding(bottom: 15);

        DarkPanel inventoryLayout = new("Inventory");
        inventoryLayout.AddContent(InventoryDescription);

        overviewLayout.Add(TopPart);
        overviewLayout.Add(taskLayout);
        overviewLayout.Add(inventoryLayout);

        BuildSkillsLayout();
        BuildHealthLayout();

        MyLayout.AddTab("Overview", overviewLayout);
        MyLayout.AddTab("Skills", SkillsLayout);
        MyLayout.AddTab("Health", HealthLayout);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X - Width(), 50f));
    }

    // TODO
    public void BuildHealthLayout()
    {
        HealthLayout = new();
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
        HealthLayout.Add(new TextSprite(Sprites.SmallFont));
    }

    public void UpdateHealthDisplay()
    {
        int row = 0;
        Health h = PersonTracking.HealthStatus;
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Mood: {h.Mood}";
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Hunger: {PersonTracking.Hunger}";
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Fat: {h.Fat}";
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Vitamin A: {h.VitaminA}";
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Vitamin C: {h.VitaminC}";
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Calcium: {h.Calcium}";
        ((TextSprite)HealthLayout.Elements[row++]).Text = $"Iron: {h.Iron}";
    }

    public void BuildSkillsLayout()
    {
        SkillsLayout = new();
        SkillsLayout.SetPadding(bottom: 20);

        int y = 0;
        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            string skillName = Globals.Title(skill.ToString());
            if (skillName == "None")
                skillName = "General";
            TextSprite skillText = new TextSprite(Sprites.SmallFont, text: skillName);
            skillText.SetPadding(right: 20);
            SkillsLayout.SetContent(0, y, skillText);

            OverlapLayout levelDisplay = new();
            levelDisplay.SetMargin(top: 1);
            
            UIElement startLevelBar = new(Sprites.VerticalGreenBar);
            startLevelBar.Image.SetScaleY(1.1f);
            UIElement levelBar = new(Sprites.VerticalGreenBar);
            levelBar.Image.SetScaleY(1.1f);
            levelBar.Image.SpriteColor = Color.Goldenrod;
            TextSprite levelText = new(Sprites.SmallFont, Color.White, Color.Black);
            levelDisplay.Add(levelBar);
            levelDisplay.Add(startLevelBar);
            levelDisplay.Add(levelText);
            SkillsLayout.SetContent(1, y, levelDisplay);
            y++;
        }
    }

    public void UpdateSkillsDisplay()
    {
        foreach (SkillLevel s in PersonTracking.Skills._list)
        {
            OverlapLayout levelDisplay = (OverlapLayout)SkillsLayout.GridContent[(int)s.skill][1];
            levelDisplay.Elements[0].Image.SetScaleX(200f * (s.level / 100f));
            levelDisplay.Elements[1].Image.SetScaleX(200f * (s.startLevel / 100f));
            ((TextSprite)levelDisplay.Elements[2]).Text = s.level.ToString();
        }
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

        TaskDescription.Text = PersonTracking.DescribeTasks();
        InventoryDescription.Text = PersonTracking.PersonalStockpile.ToString();

        UpdateHealthDisplay();
        UpdateSkillsDisplay();

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