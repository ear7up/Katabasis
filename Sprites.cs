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
	
	// Buildings
	private static List<Texture2D> buildings;
	private static List<Texture2D> barracks;
	private static List<Texture2D> farms;
	private static List<Texture2D> farmsRiver;
	private static List<Texture2D> granaries;
	private static List<Texture2D> houses;
	private static List<Texture2D> mines;
	private static List<Texture2D> ranches;

	// Animals
	public static Texture2D Cow;
	public static Texture2D Cat;
	public static Texture2D Donkey;
	public static Texture2D Pig;

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

		// Buildings
		buildings = LoadTextures("buildings", 48);
		barracks = LoadTextures("buildings/barracks", 4);
		farms = LoadTextures("buildings/farm", 3);
		farmsRiver = LoadTextures("buildings/farm_river", 2);
		granaries = LoadTextures("buildings/granary", 9);
		houses = LoadTextures("buildings/house", 9);
		mines = LoadTextures("buildings/mine", 4);
		ranches = LoadTextures("buildings/ranch", 5);

		// Animals
		Cow = content.Load<Texture2D>("animals/bull_copper");
		Cat = content.Load<Texture2D>("animals/cat_copper");
		Donkey = content.Load<Texture2D>("animals/donkey_copper");
		Pig = content.Load<Texture2D>("animals/pig_copper");
	}

	public static Texture2D RandomBuilding()
	{
		int i = Globals.Rand.Next(0, buildings.Count);
		return buildings[i];
	}

	public static Texture2D GetRiverFarmSprite()
	{
		int i = Globals.Rand.Next(0, farmsRiver.Count);
		return farmsRiver[i];
	}

	public static Texture2D RandomBuilding(BuildingType buildingType)
	{
		List<Texture2D> textures = null;
		switch (buildingType)
		{
			case BuildingType.MINE: textures = mines; break;
			case BuildingType.WOOD_HOUSE: textures = houses; break;
			case BuildingType.STONE_HOUSE: textures = houses; break;
			case BuildingType.RANCH: textures = ranches; break;
			case BuildingType.FARM: textures = farms; break;
			case BuildingType.FARM_RIVER: textures = farmsRiver; break;
			case BuildingType.BARRACKS: textures = barracks; break;
			case BuildingType.GRANARY: textures = granaries; break;
			default: textures = buildings; break;
		}

		int i = Globals.Rand.Next(textures.Count);
		return textures[i];
	}

	// Load path/001 through path/count and return the list of textures
	public static List<Texture2D> LoadTextures(string path, int count)
	{
		List<Texture2D> textures = new(count);
        for (int i = 1; i <= count; i++)
            textures.Add(Globals.Content.Load<Texture2D>($"{path}/{i:000}"));
		return textures;
	}
}
