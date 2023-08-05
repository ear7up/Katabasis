using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

static class Sprites
{
	public static Texture2D ManC { get; private set; }
	public static Texture2D ManS { get; private set; }
	public static Texture2D ManG { get; private set; }
	public static Texture2D WomanC { get; private set; }
	public static Texture2D WomanS { get; private set; }
	public static Texture2D WomanG { get; private set; }
	public static Sprite Circle { get; private set; }
	public static SpriteFont Font { get; private set; }
	public static SpriteFont Font2 { get; private set; }
	
	private static Random r = new Random();
	private static List<Texture2D> buildings;

	public static void Load(ContentManager content)
	{
		// folder/file
		ManC = content.Load<Texture2D>("person/man_copper");
		ManS = content.Load<Texture2D>("person/man_silver");
		ManG = content.Load<Texture2D>("person/man_gold");
		WomanC = content.Load<Texture2D>("person/woman_copper");
		WomanS = content.Load<Texture2D>("person/woman_silver");
		WomanG = content.Load<Texture2D>("person/woman_gold");

		Circle = new Sprite(content.Load<Texture2D>("circle"), Vector2.Zero);
		
		Font = content.Load<SpriteFont>("Font");
		Font2 = content.Load<SpriteFont>("Gladius-z8AV3");

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

	// Load path/001 through path/count and return the list of textures
	public static List<Texture2D> LoadTextures(string path, int count)
	{
		List<Texture2D> textures = new();
        for (int i = 1; i <= count; i++)
        {
            textures.Add(Globals.Content.Load<Texture2D>($"{path}/{i:000}"));
        }
		return textures;
	}
}
