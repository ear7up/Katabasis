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
    SEWING,
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
}

public class Person : Entity
{
    public static Random rand = new Random();

    private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();
    private float[,] Demand;
    private GenderType Gender;
    public Hashtable PersonalStockpile;
    private Tile Home;
    public int Money { get; set; }
    private SkillLevel[] Skills; // inherited by children Lamarck-style?

    public enum GenderType
    {
        MALE,
        FEMALE
    }

    public Person(Vector2 position)
    {
        Position = position;
        Velocity = new Vector2(20f, 20f);
        Orientation = rand.NextFloat(0.0f, MathHelper.TwoPi);
        Scale = 0.2f;
        
        Gender = (GenderType)rand.Next(2);
        switch (Gender)
        {
            case GenderType.MALE: image = Sprites.PersonMale; break;
            case GenderType.FEMALE: image = Sprites.PersonFemale; break; 
        }

        Radius = image.Width / 2f;

        Demand = new float[Goods.NUM_GOODS_TYPES, Goods.GOODS_PER_TYPE];
        PersonalStockpile = new();

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

    public static Person CreatePerson(Vector2 position)
    {
        var person = new Person(position);
        person.AddBehaviour(person.MoveRandomly());
        person.color = new Color(rand.Next(255), rand.Next(255), rand.Next(255));
        return person;
    }

    public void ChooseNextTask()
    {
        // Check skills needed for tasks and weight the probability of choosing based on relative skill level
    }

    // Add to the Demand matrix based on what goods the person wants
    // Call once per day?
    public void UpdateGoodsDemand()
    {
        float[,] change = new float[Goods.NUM_GOODS_TYPES, Goods.GOODS_PER_TYPE];

        // Units: kg/day
        for (int i = 0; i < Goods.GOODS_PER_TYPE; i++)
        {
            change[(int)Goods.GoodsType.FOOD_ANIMAL,i] += 0.1f;
            change[(int)Goods.GoodsType.FOOD_PLANT,i] += 1f;
            change[(int)Goods.GoodsType.FOOD_PROCESSED,i] += 0.5f;
        }
    }

    public void DailyUpdate()
    {
        UpdateGoodsDemand();
    }

    // A person is willing to travel 1 tile in any direction to do work at a building

    public override void Update()
    {
        // 5% chance to change direction slightly
        if (rand.NextDouble() < 0.05)
        {
            float angle = rand.NextFloat(-MathHelper.Pi / 8f, MathHelper.Pi / 8f);
            Orientation = MathHelper.WrapAngle(Orientation + angle);
        }
        Velocity = Extensions.FromPolar(Orientation, 20f);
        Position += Velocity * Globals.Time;
    }

    public override void Draw()
    {
        
        //Globals.SpriteBatch.Draw(image, Position, null, Color.White, Orientation, Size, 1f, 0, 0);
        Globals.SpriteBatch.Draw(image, Position, null, color, 0f, Size, Scale, 0, 0);
        //base.Draw(spriteBatch);
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