using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public static int IdCounter = 0;

    public const float MOVE_SPEED = 60f;
    public const int DAILY_HUNGER = 35;
    public const int STARVING = 210;
    public const int STARVED_TO_DEATH = 350;

    public Tile Home;
    public Player Owner;
    public PersonType Type;

    public readonly int Id;
    public readonly float Age;
    public readonly string Name;
    public readonly GenderType Gender;

    public int Hunger;
    public float Money;
    private float[,] Demand;

    public Stockpile PersonalStockpile;
    public PriorityQueue2<Task, int> Tasks;
    public WeightedList<SkillLevel> Skills; // inherited by children Lamarck-style?

    private Person(Vector2 position)
    {
        Id = IdCounter++;
        Owner = null;
        Position = position;
        Velocity = new Vector2(20f, 20f);
        Orientation = Globals.Rand.NextFloat(0.0f, MathHelper.TwoPi);
        Scale = 0.2f;
        
        Texture2D image = null;

        Gender = (GenderType)Globals.Rand.Next(2);
        switch (Gender)
        {
            case GenderType.MALE: image = Sprites.ManC; break;
            case GenderType.FEMALE: image = Sprites.WomanC; break; 
        }

        SetImage(image);
        Name = NameGenerator.Random(Gender);
        Age = Globals.Rand.Next(10, 50);
        Home = null;
        
        Tasks = new();
        PersonalStockpile = new();
        Demand = new float[Goods.NUM_GOODS_TYPES, Goods.GOODS_PER_TYPE];

        // New person starts with each skill assigned randomly between 1-20 (they go up to 100 with experience)
        Skills = new(Globals.Rand);
        
        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            // Random skills, older people have more
            int level = (int)(Globals.Rand.Next(5, 25) * ((Age + 90) / 100));
            Skills.Add(new SkillLevel(skill, level), level);
        }
    }

    public override string ToString()
    {
        string skills = "[";
        foreach (SkillLevel s in Skills)
            skills += s.ToString() + ", ";
        skills = skills.Substring(0, skills.Length - 1);
        skills += "]";

        string task = "Idle";
        if (Task.Peek(Tasks) != null)
            task  = Tasks.Peek().ToString();

        return $"Person('{Name}' ({Age}) hunger={Hunger}\n" +
               $"Task=[{task}]\n" +
               $"Skill={skills}\n" +
               $"Items={PersonalStockpile})";
    }

    public static Person CreatePerson(Vector2 position, Tile home)
    {
        Person person = new Person(position);
        person.Scale = 0.05f;
        person.Home = home;
        home.Population += 1;
        Globals.Ybuffer.Add(person);
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

        // TODO: build a house

        float r = Globals.Rand.NextFloat(0f, 1f);

        if (r < 1f)
        {
            // TODO: Perhaps try random using inventory?

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
        if (Hunger >= STARVED_TO_DEATH)
        {
            Die();
            return;
        }

        Tasks.Enqueue(new EatTask());
        UpdateGoodsDemand();
        GoToMarket();
    }

    public bool CheckIfClicked()
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE && InputManager.Clicked &&
            GetBounds().Contains(InputManager.MousePos))
        {
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

        if (Hunger >= STARVING)
        {
            // Hunt if you have a spear, otherwise scavenge for wild plants
            if (PersonalStockpile.Has(new Goods(GoodsType.TOOL, (int)Goods.Tool.SPEAR)))
                AssignPriorityTask(new SourceGoodsTask(
                    new Goods(GoodsType.FOOD_ANIMAL, (int)Goods.FoodAnimal.GAME)), 
                    Task.HIGH_PRIORITY);
            else
                AssignPriorityTask(new SourceGoodsTask(
                    new Goods(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.WILD_EDIBLE)), 
                    Task.HIGH_PRIORITY);
        }

        // FindNewHomeTask pathfinds relative to home tile, so it can't be null
        if (Home != null && Home.Population > Tile.MAX_POP)
            Tasks.Enqueue(new FindNewHomeTask(), 1);
        
        // Peek will grab highest priority task unless no priority is set, then it will grab the oldest assigned task
        Task current = Task.Peek(Tasks);
        if (current != null)
        {
            TaskStatus currentStatus = current.Execute(this);

            // This happens often if the task prerequisites cannot be fulfilled
            if (currentStatus.Complete || currentStatus.Failed)
                Tasks.Dequeue();
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
        Owner.Kingdom.RemovePerson(this);
        Globals.Ybuffer.Remove(this);
    }
}