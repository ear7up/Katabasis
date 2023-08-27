using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProfessionExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public enum PersonType
{
    CITIZEN,
    SLAVE,
    SOLDIER
}

public enum GenderType
{
    MALE,
    FEMALE
}

public class Person : Entity, Drawable
{
    // We will attempt to model real causes of death (hunger, disease, conflict)
    // that bring down the average age at death, this will be the age at which people
    // start to die due to age-related afflictions independent of these other causes
    public const int OLD_AGE = 70;

    public const float MOVE_SPEED = 60f;

    public const int DAILY_HUNGER = 35;
    public const int STARVING = 210;
    public const int STARVED_TO_DEATH = 350;

    public static int IdCounter = 0;
    public readonly int Id;    

    // Serialized content
    public Tile Home { get; set; }
    public Building House { get; set; }
    public Building BuildingUsing { get; set; }
    public Player Owner { get; set; }
    public PersonType Type { get; set; }
    public string Name { get; set; }
    public GenderType Gender { get; set; }
    public float Age { get; set; }
    public int Hunger { get; set; }
    public float Money { get; set; }
    public ProfessionType Profession { get; set; }
    private float[,] Demand { get; set; }
    public Stockpile PersonalStockpile { get; set; }
    // Tasks are being serialized with only the Task fields, not the proper subclass fields
    // Needs a JSON converter attribute or something like that? 
    public PriorityQueue2<Task, int> Tasks { get; set; }
    public WeightedList<SkillLevel> Skills { get; set; }

    // Don't serialize, we need to requeue this task if we saved and loaded in the middle
    public bool SearchingForHouse;

    public Person()
    {
        Id = IdCounter++;
        Owner = null;
        Velocity = new Vector2(20f, 20f);
        Orientation = Globals.Rand.NextFloat(0.0f, MathHelper.TwoPi);
        Scale = 0.2f;
        
        SpriteTexture image = null;

        Gender = (GenderType)Globals.Rand.Next(2);
        switch (Gender)
        {
            case GenderType.MALE: image = Sprites.ManC; break;
            case GenderType.FEMALE: image = Sprites.WomanC; break; 
        }

        SetImage(image);
        Name = NameGenerator.Random(Gender);
        Age = Globals.Rand.Next(10, 50);
        Profession = ProfessionType.NONE;
        House = null;
        SearchingForHouse = false;
        BuildingUsing = null;
        
        Tasks = new();
        PersonalStockpile = new();
        Demand = new float[Goods.NUM_GOODS_TYPES, Goods.GOODS_PER_TYPE];

        // New person starts with each skill assigned randomly between 1-20 (they go up to 100 with experience)
        Skills = new();
        
        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            // Random skills, older people have more
            int level = (int)(Globals.Rand.Next(10, 35) * ((Age + 90) / 100));
            int weight = level;

            // Bias people toward cooking
            if (skill == Skill.COOKING)
                weight *= 4;
            
            Skills.Add(SkillLevel.Create(skill, level), weight);
        }

        Globals.Ybuffer.Add(this);
    }

    public void SetAttributes(Vector2 position, Tile home)
    {
        Position = position;
        Home = home;
    }

    public override string ToString()
    {
        string skills = "[";
        foreach (SkillLevel s in Skills._list)
            skills += s.ToString() + ", ";
        skills = skills.Substring(0, skills.Length - 1);
        skills += "]";

        string task = "Idle";
        if (Task.Peek(Tasks) != null)
            task  = Task.Peek(Tasks).Describe();

        string house = "homeless";
        if (House != null)
            house = House.Sprite.Position.ToString();

        return $"Person('{Name}' ({Age}) hunger={Hunger}\n" +
               $"Task=[{task}]\n" +
               $"Skill={skills}\n" +
               $"Items={PersonalStockpile})\n" + 
               $"Position={Position}\n" + 
               $"House={house}";
    }

    public string Describe()
    {
        string description = 
            $"Name: {Name}\n" + 
            $"Age: {Age:0.0}\n" + 
            $"Profession: {Profession.Describe()}\n" + 
            $"Money: ${Money:0.0}\n" + 
            $"Hunger: {Hunger}";
        return description;
    }

    public string DescribeTasks()
    {
        string description = "";
        foreach (Task task in Tasks)
            description += task.Describe(debug: false) + "\n";
        return description;
    }

    public static Person CreatePerson(Vector2 position, Tile home)
    {
        Person person = new();
        person.SetAttributes(position, home);
        person.Scale = 0.05f;
        home.Population += 1;
        return person;
    }

    public void SetHouse(Object x)
    {
        House = (Building)x;
        House.StartUsing();

        Home.Population -= 1;
        Home = House.Location;
        Home.Population += 1;
    }

    public void NoHouseFound(Object x)
    {
        SearchingForHouse = false;
    }

    public void ChooseNextTask()
    {
        // If the Person's home is too populated, find a new home
        if (Home != null && Home.Population > Tile.MAX_POP)
        {
            Tasks.Enqueue(new FindNewHomeTask());
            return;
        }

        float r = Globals.Rand.NextFloat(0f, 1f);

        if (r < 1f)
        {
            // Pick a skill, biased toward high-level skills, then pick a task that uses that skill
            AdjustSkillWeights();
            SkillLevel weightedRandomChoice = Skills.Next();

            Task task = null;
            
            const float PROFIT_SEEKING_CHANCE = 0.3f;
            if (r < PROFIT_SEEKING_CHANCE)
                task = Task.MostProfitableUsingSkill(weightedRandomChoice);

            // Some skills may have no completable tasks, so have a chance to not get stuck in a loop
            if (task == null || r >= PROFIT_SEEKING_CHANCE)
                task = Task.RandomUsingSkill(this, weightedRandomChoice);

            Tasks.Enqueue(task);
        }
        else
        {
            Tasks.Enqueue(new IdleAtHomeTask());
        }
    }

    public void AdjustSkillWeights()
    {
        // Increase chance to pick a cooking task according to Hunger
        // scales from 1x to 4x cooking level based on hunger
        SkillLevel cooking = Skills[(int)Skill.COOKING];
        Skills.SetWeightAtIndex((int)Skill.COOKING, cooking.level * (1 + (3 * Hunger / STARVED_TO_DEATH)));

        // 2x likelihood of choosing a task related to the skill used by your profession
        if ((int)Profession <= (int)Skill.NONE)
        {
            SkillLevel profession = Skills[(int)Profession];
            Skills.SetWeightAtIndex((int)profession.skill, 3 * profession.level);
        }

        // Recalculate after adjusting weights, including from level-ups
        Skills.Recalculate();
    }

    public void GainExperience(int skillId, float exp)
    {
        // If skill leveled up, update weights but do not Recalculate (unnecessary)
        if (Skills[skillId].GainExperience(exp) > 0)
            Skills.SetWeightAtIndex(skillId, Skills[skillId].level);
    }

    public void AssignPriorityTask(Task task, int priority)
    {
        Tasks.Enqueue(task, priority);
    }

    // Add to the Demand matrix based on what goods the person wants
    // Call once per day?
    public void UpdateGoodsDemand()
    {
        // Hunger coefficient, food demand increases more if the person is hungry
        int h = (Hunger / DAILY_HUNGER);

        // Units: kg/day
        for (int i = 0; i < Goods.GOODS_PER_TYPE; i++)
        {
            // Food demand tends to increase each day so that people won't starve
            // Rationale: expected change [0.4, 0.8, 0.3] = satiation([4, 4, 2.4]) = 10.4 > daily hunger 10
            Demand[(int)GoodsType.FOOD_ANIMAL,i] += Globals.Rand.NextFloat(0f, 0.8f) * h;
            Demand[(int)GoodsType.FOOD_PLANT,i] += Globals.Rand.NextFloat(0f, 1.6f) * h;
            Demand[(int)GoodsType.FOOD_PROCESSED,i] += Globals.Rand.NextFloat(0f, 0.6f) * h;

            // Will occasionally want to buy craft goods
            Demand[(int)GoodsType.CRAFT_GOODS,i] += Globals.Rand.NextFloat(0f, 0.005f);
        }
    }

    public void GoToMarket()
    {
        // TODO: Go to the market, figure out what you want most, check prices, and buy important things
        // try to buy goods where Demand matrix > 1
    }

    public void DailyUpdate()
    {
        PersonalStockpile.DailyUpdate();

        // A year is 10 days, chance to die every 1/10th of a year after hitting old age
        Age += 0.1f;
        if (OldAgeCheck())
            return;

        // Guaranteed death after hunger >= Person.STARVED_TO_DEATH
        Hunger += DAILY_HUNGER;
        if (HungerCheck())
            return;

        // Change sprites based on total level (simply cosmetic)
        TotalLevelCheck();

        // Change profession based on skills
        ProfessionCheck();

        // If you have a house, go there, desposit your inventory, then try to cook
        if (House != null && Home != null)
            DailyHomeTasks();

        // Eat until you run out of food or are no longer hungry
        AssignPriorityTask(new EatTask(), 1);

        // Randomly add demand for diffrent types of consumer goods
        UpdateGoodsDemand();

        if (House == null && !SearchingForHouse)
            FindHouse();
    }

    public void DailyHomeTasks()
    {
        GoToTask go = new();
        go.SetAttributes("Going home for the day", House.Sprite.Position);
        Tasks.Enqueue(go);
        Tasks.Enqueue(new DepositInventoryTask());
        Tasks.Enqueue(new CookTask());
        Tasks.Enqueue(new EatTask());
        Tasks.Enqueue(new SellAtMarketTask());
    }

    public void FindHouse()
    {
        FindBuildingTask find = new();
        find.SetAttributes(BuildingType.HOUSE);
        find.OnSuccess = SetHouse;
        find.OnFailure = NoHouseFound;
        Tasks.Enqueue(find);
        SearchingForHouse = true;
    }

    public bool OldAgeCheck()
    {
        if (Age >= OLD_AGE && Globals.Rand.NextFloat(0f, 1f) <= 0.01f)
        {
            Die();
            return true;
        }
        return false;
    }

    public bool HungerCheck()
    {
        if (Hunger >= STARVED_TO_DEATH)
        {
            Die();
            return true;
        }
        return false;
    }

    public void TotalLevelCheck()
    {
        int sum = 0;
        foreach (SkillLevel skill in Skills._list)
            sum += skill.level;

        if (sum >= 350)
            SetImage((Gender == GenderType.MALE) ? Sprites.ManG : Sprites.WomanG);
        else if (sum >= 300)
            SetImage((Gender == GenderType.MALE) ? Sprites.ManS : Sprites.WomanS);
    }

    // Pick the highest skill as the person's profession
    public void ProfessionCheck()
    {
        SkillLevel max = Skills[0];
        foreach (SkillLevel skillLevel in Skills._list)
            if (skillLevel.level > max.level)
                max = skillLevel;
        
        Profession = (ProfessionType)max.skill;
    }

    // Takes excess goods from person's home and adds to their personal stockpile
    public List<Goods> FigureOutWhatToSell()
    {
        List<Goods> extras = new();

        // Try to keep enough food to feed everyone in the household for 3 days
        float totalSatiation = House.Stockpile.TotalSatiation();
        float keepSatiation = House.CurrentUsers * Person.DAILY_HUNGER * 3;

        foreach (Goods g in House.Stockpile)
        {
            // Try to keep twice the production quantity of the good lying around
            float keepAmount = GoodsInfo.GetDefaultProductionQuantity(g) * 2f;

            // However, for food, just try to keep enough to feed everyone
            float satiation = GoodsInfo.GetSatiation(g);

            if (satiation > 0f)
            {
                float qtyToTake = Math.Max(0f, (totalSatiation - keepSatiation) / satiation);
                qtyToTake = Math.Min(qtyToTake, g.Quantity);
                
                float qty = House.Stockpile.Take(g.GetId(), qtyToTake);
                if (qty > 0f)
                {
                    PersonalStockpile.Add(new Goods(g, qty));
                    extras.Add(new Goods(g, qty));
                }
            }
            else if (g.Quantity > keepAmount)
            {
                float qty = House.Stockpile.Take(g.GetId(), g.Quantity - keepAmount);
                if (qty > 0f)
                {
                    PersonalStockpile.Add(new Goods(g, qty));
                    extras.Add(new Goods(g, qty));
                }
            }
        }
        return extras;
    }

    public bool CheckIfClicked()
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE && InputManager.UnconsumedClick() &&
            GetBounds().Contains(InputManager.MousePos))
        {
            InputManager.ConsumeClick(this);
            return true;
        }
        return false;
    }

    // A person is willing to travel 1 tile in any direction to do work at a building
    public override void Update()
    {        
        if (Tasks.Empty())
            ChooseNextTask();

        float r = Globals.Rand.NextFloat(0f, 1f);
        
        // Peek will grab highest priority task unless no priority is set, then it will grab the oldest assigned task
        Task current = Task.Peek(Tasks);
        if (current != null)
        {
            TaskStatus currentStatus = current.Execute(this);

            if (currentStatus != null)
            {
                // This happens often if the task prerequisites cannot be fulfilled
                if (currentStatus.Complete || currentStatus.Failed)
                    Tasks.Dequeue();

                if (currentStatus.Complete && !currentStatus.Failed && current.OnSuccess != null)
                    current.OnSuccess(currentStatus.ReturnValue);
                else if (currentStatus.Complete && currentStatus.Failed && current.OnFailure != null)
                    current.OnFailure(currentStatus.ReturnValue);
            }
        }
    }

    public override void Draw()
    {
        // Why did I override this and the origin with Size instead of Size/2 and set Orientation to 0?
        Globals.SpriteBatch.Draw(image, Position, null, color, 0f, Size / 2f, Scale, 0, 0);
    }

    public float GetMaxY()
    {
        // Each person has a unique id, use it as a small constant to prevent YBuffer flickering
        return Position.Y + (Scale * image.Height) + (Id * 0.000001f);
    }

    public void Die()
    {
        if (Home != null)
            Home.Population--;
        if (House != null)
            House.StopUsing();
        if (BuildingUsing != null)
            BuildingUsing.StopUsing();
        Owner.Kingdom.PersonDied(this);
        Globals.Ybuffer.Remove(this);
    }

    public float Wealth()
    {
        float wealth = Money + PersonalStockpile.Wealth();
        return wealth;
    }
}