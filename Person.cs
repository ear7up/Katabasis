using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

public enum Skill
{
    FARMING = 0,
    BUILDING,
    HUNTING,
    FISHING,
    COOKING,
    CRAFTING,
    SMITHING,
    FIGHTING,
    MINING,
    FORESTRY,
    NONE
}

public class SkillLevel
{
    public Skill skill;
    public int level;
    public SkillLevel(Skill skill, int level)
    {
        this.skill = skill;
        this.level = level;
    }

    public override string ToString()
    {
        return $"SkillLevel: {skill} {level}";
    }
}

public class Person : Entity
{
    public static Random rand = new Random();
    public const float MOVE_SPEED = 60f;
    public const int DAILY_HUNGER = 10;
    public const int STARVING = 100;

    private float[,] Demand;
    private GenderType Gender;
    public Stockpile PersonalStockpile;
    public Tile Home;
    public float Money { get; set; }
    public int Hunger { get; set; }
    public PriorityQueue2<Task, int> Tasks;
    public WeightedList<SkillLevel> Skills; // inherited by children Lamarck-style?

    public enum GenderType
    {
        MALE,
        FEMALE
    }

    private Person(Vector2 position)
    {
        Position = position;
        Velocity = new Vector2(20f, 20f);
        Orientation = rand.NextFloat(0.0f, MathHelper.TwoPi);
        Scale = 0.2f;
        
        Gender = (GenderType)rand.Next(2);
        switch (Gender)
        {
            case GenderType.MALE: image = Sprites.ManC; break;
            case GenderType.FEMALE: image = Sprites.WomanC; break; 
        }

        Radius = image.Width / 2f;

        Demand = new float[Goods.NUM_GOODS_TYPES, Goods.GOODS_PER_TYPE];
        PersonalStockpile = new();

        Home = null;
        Tasks = new();

        // New person starts with each skill assigned randomly between 1-20 (they go up to 100 with experience)
        Skills = new(Globals.Rand);
        
        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            int level = rand.Next(5, 30);
            Skills.Add(new SkillLevel(skill, level), level);
        }
    }

    public override string ToString()
    {
        string gender = Gender == GenderType.MALE ? "Male" : "Female";
        return $"Person(gender={gender}, money={Money})";
    }

    public static Person CreatePerson(Vector2 position, Tile home)
    {
        Person person = new Person(position);
        person.Scale = 0.05f;
        person.Home = home;
        home.Population += 1;
        return person;
    }

    public void ChooseNextTask()
    {
        // If the Person's home is too populated, find a new home
        if (Home == null || Home.Population > Tile.MAX_POP)
        {
            Tasks.Enqueue(new FindNewHomeTask());
            return;
        }

        float r = Globals.Rand.NextFloat(0f, 1f);

        if (r < 1f)
        {
            // Pick a skill, biased toward high-level skills, then pick a task that uses that skill
            SkillLevel weightedRandomChoice = Skills.Next();
            Task task = Task.RandomUsingSkill(weightedRandomChoice);
            Tasks.Enqueue(task);
        }
        else
        {
            Tasks.Enqueue(new IdleAtHomeTask());
        }
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
        Hunger += DAILY_HUNGER;
        Tasks.Enqueue(new EatTask());
        UpdateGoodsDemand();
        GoToMarket();
    }

    // A person is willing to travel 1 tile in any direction to do work at a building

    public override void Update()
    {        
        if (Tasks.Empty())
        {
            ChooseNextTask();
        }

        if (Hunger >= STARVING)
        {
            AssignPriorityTask(new EatTask(), Task.HIGH_PRIORITY);
        }

        if (Home != null && Home.Population > Tile.MAX_POP)
        {
            Tasks.Enqueue(new FindNewHomeTask(), 1);
        }
        
        // Peek will grab highest priority task unless no priority is set, then it will grab the oldest assigned task
        Task current = Tasks.Peek();
        TaskStatus currentStatus = current.Execute(this);

        if (Task.DEBUG)
            Console.WriteLine($"Doing task {current} status: {currentStatus.Complete} value: {currentStatus.ReturnValue}");

        if (currentStatus.Complete)
        {
            if (currentStatus.Failed)
                Console.WriteLine($"Task {currentStatus.Task} failed");
            Tasks.Dequeue();
        }
    }

    public override void Draw()
    {
        Globals.SpriteBatch.Draw(image, Position, null, color, 0f, Size, Scale, 0, 0);
    }
}