using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

static class Extensions
{
	public static float ToAngle(this Vector2 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public static Vector2 ScaleTo(this Vector2 vector, float length)
	{
		return vector * (length / vector.Length());
	}

	public static Point ToPoint(this Vector2 vector)
	{
		return new Point((int)vector.X, (int)vector.Y);
	}

	public static float NextFloat(this Random rand, float minValue, float maxValue)
	{
		return (float)rand.NextDouble() * (maxValue - minValue) + minValue;
	}

	public static Vector2 NextVector2(this Random rand, float minLength, float maxLength)
	{
		double theta = rand.NextDouble() * 2 * Math.PI;
		float length = rand.NextFloat(minLength, maxLength);
		return new Vector2(length * (float)Math.Cos(theta), length * (float)Math.Sin(theta));
	}

    public static Vector2 FromPolar(float angle, float magnitude)
    {
        return magnitude * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
}