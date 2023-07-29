using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

class Person : Entity
{
    public static Random rand = new Random();

    private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();

    public Person(Texture2D image, Vector2 position)
    {
        this.image = image;
        Position = position;
        Radius = image.Width / 2f;
        Velocity = new Vector2(20f, 20f);
        //color = Color.Transparent;
        Orientation = rand.NextFloat(0.0f, MathHelper.TwoPi);
        Scale = 0.2f;
    }

    public static Person CreatePerson(Vector2 position)
    {
        var person = new Person(Sprites.Person, position);
        person.AddBehaviour(person.MoveRandomly());
        person.color = new Color(rand.Next(255), rand.Next(255), rand.Next(255));
        return person;
    }

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
        //Console.WriteLine("Moving at angle " + angle.ToString() + " vector " + Velocity.ToString());

        //ApplyBehaviours();
        //Position += Velocity;
        //Position = Vector2.Clamp(Position, Size / 2, Katabasis.KatabasisGame.ScreenSize - Size / 2);
        //Velocity *= 0.8f;
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