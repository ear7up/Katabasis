using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

static class Sprites
{
	public static Hashtable Loaded = new();

	public static SpriteTexture Sky { get; private set; }

	public static SpriteTexture ManC { get; private set; }
	public static SpriteTexture ManS { get; private set; }
	public static SpriteTexture ManG { get; private set; }
	public static SpriteTexture WomanC { get; private set; }
	public static SpriteTexture WomanS { get; private set; }
	public static SpriteTexture WomanG { get; private set; }
	public static Sprite Circle { get; private set; }

	public static SpriteFont Font { get; private set; }
	public static SpriteFont Font2 { get; private set; }
	
	// Buildings
	public static List<SpriteTexture> buildings;
	public static List<SpriteTexture> barracks;
	public static List<SpriteTexture> farms;
	public static List<SpriteTexture> farmsRiver;
	public static List<SpriteTexture> granaries;
	public static List<SpriteTexture> houses;
	public static List<SpriteTexture> mines;
	public static List<SpriteTexture> ranches;
	public static List<SpriteTexture> cities;
	public static List<SpriteTexture> markets;
	public static List<SpriteTexture> smithies;

	// Animals
	public static SpriteTexture Cow;
	public static SpriteTexture Cat;
	public static SpriteTexture Donkey;
	public static SpriteTexture Pig;
	public static SpriteTexture Elephant;
	public static SpriteTexture Gazelle;
	public static SpriteTexture Duck;
	public static SpriteTexture Fowl;
	public static SpriteTexture Quail;
	public static SpriteTexture Goat;
	public static SpriteTexture Sheep;
	public static SpriteTexture Goose;

	// UI
	public static SpriteTexture Clock;
	public static SpriteTexture ClockHand;
	public static SpriteTexture BottomLeftPanel;
	public static SpriteTexture BottomPanel;
	public static SpriteTexture TallPanel;
	public static List<SpriteTexture> BottomLeftButtons;
	public static SpriteTexture Tooltip;
	public static SpriteTexture VerticalGreenBar;
	public static SpriteTexture VerticalBar;
	public static SpriteTexture TabBackground;
	public static SpriteTexture TabSelected;
	public static SpriteTexture TabUnselected;

	// Map
	public static List<SpriteTexture> desertTextures;
	public static List<SpriteTexture> desertHillTextures;
	public static List<SpriteTexture> desertVegetationTextures;
	public static List<SpriteTexture> desertBedouinTextures;
	public static List<SpriteTexture> desertForestTextures;
	public static List<SpriteTexture> desertRiverTextures;

	public static void Load(ContentManager content)
	{
		// folder/file
		Sky = LoadTexture("sky");

		ManC = LoadTexture("person/man_copper");
		ManS = LoadTexture("person/man_silver");
		ManG = LoadTexture("person/man_gold");
		WomanC = LoadTexture("person/woman_copper");
		WomanS = LoadTexture("person/woman_silver");
		WomanG = LoadTexture("person/woman_gold");

		Circle = Sprite.Create(LoadTexture("circle"), Vector2.Zero);
		
		Font = content.Load<SpriteFont>("Font");
		Font2 = content.Load<SpriteFont>("Gladius-z8AV3");

		// Map textures
        desertTextures = Sprites.LoadTextures("desert/flat", 18);
        desertHillTextures = Sprites.LoadTextures("desert/hills", 7);
        desertVegetationTextures = Sprites.LoadTextures("desert/vegetation", 12);
        desertBedouinTextures = Sprites.LoadTextures("desert/bedouin_camps", 5);
		desertForestTextures = Sprites.LoadTextures("desert/forest", 5);
		desertRiverTextures = Sprites.LoadTextures("desert/river", 16);

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
		Cow = LoadTexture("animals/bull_copper");
		Cat = LoadTexture("animals/cat_copper");
		Donkey = LoadTexture("animals/donkey_copper");
		Pig = LoadTexture("animals/pig_copper");
		Elephant = LoadTexture("animals/elephant_copper");
		Gazelle = LoadTexture("animals/gazelle_copper");
		Duck = LoadTexture("animals/duck_copper");
		Fowl = LoadTexture("animals/fowl_copper");
		Quail = LoadTexture("animals/quail_copper");
		Goat = LoadTexture("animals/goat_copper");
		Sheep = LoadTexture("animals/sheep_copper");
		Goose = LoadTexture("animals/goose_copper");

		// UI
		Clock = LoadTexture("UI_maybe/clock");
		ClockHand = LoadTexture("UI_maybe/clock_hand");
		BottomLeftPanel = LoadTexture("UI_maybe/panel1");
		BottomPanel = LoadTexture("UI_maybe/bottom_panel");
		TallPanel = LoadTexture("UI_maybe/tall_panel");
		BottomLeftButtons = LoadTextures("UI_maybe/buttons", 8);
		Tooltip = LoadTexture("UI_maybe/tooltip");
		VerticalGreenBar = LoadTexture("UI_maybe/vertical_green_bar");
		VerticalBar = LoadTexture("UI_maybe/vertical_bar");
		TabBackground = LoadTexture("UI_maybe/tab_background");
		TabSelected = LoadTexture("UI_maybe/tab_selected");
		TabUnselected = LoadTexture("UI_maybe/tab_unselected");
	}

	public static SpriteTexture RandomBuilding()
	{
		int i = Globals.Rand.Next(0, buildings.Count);
		return buildings[i];
	}

	public static SpriteTexture GetRiverFarmSprite()
	{
		int i = Globals.Rand.Next(0, farmsRiver.Count);
		return farmsRiver[i];
	}

	public static SpriteTexture RandomBuilding(BuildingType buildingType)
	{
		List<SpriteTexture> textures = null;
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

		return Random(textures);
	}

	public static SpriteTexture RandomDesert()
	{
		return Random(desertTextures);
	}

	public static SpriteTexture RandomHills()
	{
		return Random(desertHillTextures);
	}

	public static SpriteTexture RandomVegetation()
	{
		return Random(desertVegetationTextures);
	}

	public static SpriteTexture RandomCamp()
	{
		return Random(desertBedouinTextures);
	}

	public static SpriteTexture RandomForest()
	{
		return Random(desertForestTextures);
	}

	public static SpriteTexture RandomRiver()
	{
		return Random(desertRiverTextures);
	}

	public static SpriteTexture Random(List<SpriteTexture> textures)
	{
		int i = Globals.Rand.Next(textures.Count);
		return textures[i];
	}

	public static Texture2D GetTexture(string path)
	{
		return (Texture2D)Loaded[path];
	}

	// Load path/001 through path/count and return the list of textures
	public static List<SpriteTexture> LoadTextures(string path, int count)
	{
		List<SpriteTexture> textures = new(count);
        for (int i = 1; i <= count; i++)
		{
			string texturePath = $"{path}/{i:000}";
			Texture2D texture = Globals.Content.Load<Texture2D>(texturePath);
			Loaded[texturePath] = texture;
            textures.Add(new SpriteTexture(texturePath, texture));
		}
		return textures;
	}

	public static SpriteTexture LoadTexture(string path)
	{
		Texture2D texture = Globals.Content.Load<Texture2D>(path);
		Loaded[path] = texture;
		return new SpriteTexture(path, texture);
	}
}
