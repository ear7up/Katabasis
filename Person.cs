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
    NUM_SKILLS // last entry for easy enum size lookup
}

public class SkillLevel
{
    public Skill skill;
    public int value;
    public SkillLevel(Skill skill, int value)
    {
        this.skill = skill;
        this.value = value;
    }

    public override string ToString()
    {
        return $"SkillLevel: {skill} {value}";
    }
}

public class Person : Entity
{
    public static Random rand = new Random();
    public const float MOVE_SPEED = 60f;

    private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();
    private float[,] Demand;
    private GenderType Gender;
    public Hashtable PersonalStockpile;
    public Tile Home;
    public int Money { get; set; }
    public PriorityQueue2<Task, int> Tasks;
    private SkillLevel[] Skills; // inherited by children Lamarck-style?

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
        Skills = new SkillLevel[(int)Skill.NUM_SKILLS];
        
        for (int i = 0; i < (int)Skill.NUM_SKILLS; i++)
        {
            Skills[i] = new SkillLevel((Skill)i, rand.Next(1, 20));
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
        // Check skills needed for tasks and weight the probability of choosing based on relative skill level

        // If the Person's home is too populated, find a new home
        if (Home == null || Home.Population > Tile.MAX_POP)
        {
            Tasks.Enqueue(new FindNewHomeTask());
            return;
        }

        Tasks.Enqueue(new IdleAtHomeTask());
    }

    public void AssignPriorityTask(Task task, int priority)
    {
        Tasks.Enqueue(task, priority);
    }

    // Add to the Demand matrix based on what goods the person wants
    // Call once per day?
    public void UpdateGoodsDemand()
    {
        float[,] change = new float[Goods.NUM_GOODS_TYPES, Goods.GOODS_PER_TYPE];

        // Units: kg/day
        for (int i = 0; i < Goods.GOODS_PER_TYPE; i++)
        {
            change[(int)GoodsType.FOOD_ANIMAL,i] += 0.1f;
            change[(int)GoodsType.FOOD_PLANT,i] += 1f;
            change[(int)GoodsType.FOOD_PROCESSED,i] += 0.5f;
        }
    }

    public void DailyUpdate()
    {
        UpdateGoodsDemand();
    }

    // A person is willing to travel 1 tile in any direction to do work at a building

    public override void Update()
    {        
        if (Tasks.Empty())
        {
            ChooseNextTask();
        }

        if (Home.Population > Tile.MAX_POP)
        {
            Tasks.Enqueue(new FindNewHomeTask(), 1);
        }
        
        // Peek will grab highest priority task unless no priority is set, then it will grab the oldest assigned task
        Task current = Tasks.Peek();
        bool isCompleted = current.Execute(this);

        if (isCompleted)
        {
            Tasks.Dequeue();
        }
    }

    public override void Draw()
    {
        Globals.SpriteBatch.Draw(image, Position, null, color, 0f, Size, Scale, 0, 0);
    }

    private void AddBehaviour(IEnumerable<int> behaviour)
    {
        behaviours.Add(behaviour.GetEnumerator());
    }

    private void ApplyBehaviours()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (!behaviours[i].MoveNext())
                behaviours.RemoveAt(i--);
        }
    }

    #region Behaviours
    IEnumerable<int> Follow(float acceleration)
    {
        while (true)
        {
            // Velocity += (Target.Position - Position).ScaleTo(acceleration);
            if (Velocity != Vector2.Zero)
            {
                Orientation = 360 - (MathF.Atan2(Velocity.X, Velocity.Y) * (360 / (MathF.PI * 2)) * MathF.Sign(Velocity.X));
            }

            yield return 0;
        }
    }

    IEnumerable<int> MoveRandomly()
    {
        float direction = rand.NextFloat(0.0f, MathHelper.TwoPi);

        while (true)
        {
            direction += rand.NextFloat(-0.1f, 0.1f);
            direction = MathHelper.WrapAngle(direction);

            for (int i = 0; i < 6; i++)
            {
                Velocity += Extensions.FromPolar(direction, 0.4f);
                Orientation -= 0.05f;

                var bounds = Katabasis.KatabasisGame.Viewport.Bounds;
                bounds.Inflate(-image.Width / 2 - 1, -image.Height / 2 - 1);

                // if the person is outside the bounds, make it move away from the edge
                if (!bounds.Contains(Position.ToPoint()))
                    direction = (Katabasis.KatabasisGame.ScreenSize / 2 - Position).ToAngle() + rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

                yield return 0;
            }
        }
    }
    #endregion
}