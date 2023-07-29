using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

class Person : Entity
{
    public static Random rand = new Random();

    private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();
    private int timeUntilStart = 60;
    public bool IsActive { get { return timeUntilStart <= 0; } }
    public int PointValue { get; private set; }

    public Person(Texture2D image, Vector2 position)
    {
        this.image = image;
        Position = position;
        Radius = image.Width / 2f;
        color = Color.Transparent;
        PointValue = 1;
    }

    public static Person CreatePerson(Vector2 position)
    {
        var person = new Person(Sprites.Person, position);
        person.AddBehaviour(person.MoveRandomly());
        return person;
    }

    public override void Update()
    {
        if (timeUntilStart <= 0)
        {
            ApplyBehaviours();
        }
        else
        {
            timeUntilStart--;
            color = Color.White * (1 - timeUntilStart / 60f);
        }

        Position += Velocity;
        Position = Vector2.Clamp(Position, Size / 2, Katabasis.KatabasisGame.ScreenSize - Size / 2);

        Velocity *= 0.8f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (timeUntilStart > 0)
        {
            // Draw an expanding, fading-out version of the sprite as part of the spawn-in effect.
            float factor = timeUntilStart / 60f;	// decreases from 1 to 0 as the enemy spawns in
            spriteBatch.Draw(image, Position, null, Color.White * factor, Orientation, Size / 2f, 2 - factor, 0, 0);
        }

        base.Draw(spriteBatch);
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

    public void HandleCollision(Person other)
    {
        var d = Position - other.Position;
        Velocity += 10 * d / (d.LengthSquared() + 1);
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

                // if the enemy is outside the bounds, make it move away from the edge
                if (!bounds.Contains(Position.ToPoint()))
                    direction = (Katabasis.KatabasisGame.ScreenSize / 2 - Position).ToAngle() + rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

                yield return 0;
            }
        }
    }
    #endregion
}