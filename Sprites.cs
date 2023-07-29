using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

static class Sprites
{
	public static Texture2D Person { get; private set; }
	public static SpriteFont Font { get; private set; }
	private static Random r = new Random();
	private static List<Texture2D> buildings;

	public static void Load(ContentManager content)
	{
		// folder/file
		Person = content.Load<Texture2D>("ball");
		Font = content.Load<SpriteFont>("Font");

		const int NUM_BUILDINGS = 48;
		buildings = new List<Texture2D>(NUM_BUILDINGS);
		for (int i = 1; i <= NUM_BUILDINGS; i++)
		{
			buildings.Add(content.Load<Texture2D>($"buildings/{i:000}"));
		}
	}

	public static Texture2D RandomBuilding()
	{
		int i = r.Next(0, buildings.Count);
		return buildings[i];
	}
}
