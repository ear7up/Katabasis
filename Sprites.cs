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
	public static List<Texture2D> buildings;
	public static List<Texture2D> barracks;
	public static List<Texture2D> farms;
	public static List<Texture2D> farmsRiver;
	public static List<Texture2D> granaries;
	public static List<Texture2D> houses;
	public static List<Texture2D> mines;
	public static List<Texture2D> ranches;
	public static List<Texture2D> cities;
	public static List<Texture2D> markets;
	public static List<Texture2D> smithies;

	// Animals
	public static Texture2D Cow;
	public static Texture2D Cat;
	public static Texture2D Donkey;
	public static Texture2D Pig;
	public static Texture2D Elephant;
	public static Texture2D Gazelle;
	public static Texture2D Duck;
	public static Texture2D Fowl;
	public static Texture2D Quail;
	public static Texture2D Goat;
	public static Texture2D Sheep;
	public static Texture2D Goose;

	// UI
	public static Texture2D Clock;
	public static Texture2D ClockHand;
	public static Texture2D BottomLeftPanel;
	public static Texture2D BottomPanel;
	public static Texture2D TallPanel;
	public static List<Texture2D> BottomLeftButtons;
	public static Texture2D Tooltip;

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
		cities = LoadTextures("buildings/city", 2);
		markets = LoadTextures("buildings/market", 7);
		smithies = LoadTextures("buildings/smithy", 4);

		// Animals
		Cow = content.Load<Texture2D>("animals/bull_copper");
		Cat = content.Load<Texture2D>("animals/cat_copper");
		Donkey = content.Load<Texture2D>("animals/donkey_copper");
		Pig = content.Load<Texture2D>("animals/pig_copper");
		Elephant = content.Load<Texture2D>("animals/elephant_copper");
		Gazelle = content.Load<Texture2D>("animals/gazelle_copper");
		Duck = content.Load<Texture2D>("animals/duck_copper");
		Fowl = content.Load<Texture2D>("animals/fowl_copper");
		Quail = content.Load<Texture2D>("animals/quail_copper");
		Goat = content.Load<Texture2D>("animals/goat_copper");
		Sheep = content.Load<Texture2D>("animals/sheep_copper");
		Goose = content.Load<Texture2D>("animals/goose_copper");

		// UI
		Clock = content.Load<Texture2D>("UI_maybe/clock");
		ClockHand = content.Load<Texture2D>("UI_maybe/clock_hand");
		BottomLeftPanel = content.Load<Texture2D>("UI_maybe/panel1");
		BottomPanel = content.Load<Texture2D>("UI_maybe/bottom_panel");
		TallPanel = content.Load<Texture2D>("UI_maybe/tall_panel");
		BottomLeftButtons = LoadTextures("UI_maybe/buttons", 8);
		Tooltip = content.Load<Texture2D>("UI_maybe/tooltip");
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
			case BuildingType.HOUSE: textures = houses; break;
			case BuildingType.RANCH: textures = ranches; break;
			case BuildingType.FARM: textures = farms; break;
			case BuildingType.FARM_RIVER: textures = farmsRiver; break;
			case BuildingType.BARRACKS: textures = barracks; break;
			case BuildingType.GRANARY: textures = granaries; break;
			case BuildingType.MARKET: textures = markets; break;
			case BuildingType.CITY: textures = cities; break;
			case BuildingType.SMITHY: textures = smithies; break;
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
