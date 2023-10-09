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
	public static SpriteFont SmallFont { get; private set; }
	public static SpriteFont FontAgencyL { get; private set; }
	
	// Buildings
	public static List<SpriteTexture> buildings;
	public static List<SpriteTexture> barracks;
	public static List<SpriteTexture> barracksCon;
	public static List<SpriteTexture> farms;
	public static List<SpriteTexture> farmsCon;
	public static List<SpriteTexture> farmsRiver;
	public static List<SpriteTexture> farmsRiverCon;
	public static List<SpriteTexture> granaries;
	public static List<SpriteTexture> granariesCon;
	public static List<SpriteTexture> houses;
	public static List<SpriteTexture> housesCon;
	public static List<SpriteTexture> mines;
	public static List<SpriteTexture> minesCon;
	public static List<SpriteTexture> ranches;
	public static List<SpriteTexture> ranchesCon;
	public static List<SpriteTexture> cities;
	public static List<SpriteTexture> city1Composite;
	public static List<SpriteTexture> market1Composite;
	//public static List<SpriteTexture> citiesCon;
	public static List<SpriteTexture> markets;
	public static List<SpriteTexture> marketsCon;
	public static List<SpriteTexture> smithies;
	public static List<SpriteTexture> smithiesCon;
	public static List<SpriteTexture> temples;
	public static List<SpriteTexture> templesCon;
	public static List<SpriteTexture> ovens;
	//public static List<SpriteTexture> ovensCon;
	public static List<SpriteTexture> tanneries;
	public static List<SpriteTexture> tanneriesCon;
	public static List<SpriteTexture> taverns;
	//public static List<SpriteTexture> tavernsCon;

	// Pyramid 25% - 100% built
	public static SpriteTexture Pyramid25;
	public static SpriteTexture Pyramid50;
	public static SpriteTexture Pyramid75;
	public static SpriteTexture Pyramid100;

	public static List<SpriteTexture> decorations;

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

	// Plants
	public static SpriteTexture Barley;
	public static SpriteTexture Celery;
	public static SpriteTexture Chickpeas;
	public static SpriteTexture Cucumber;
	public static SpriteTexture Garlic;
	public static SpriteTexture Gourd;
	public static SpriteTexture Grapes;
	public static SpriteTexture Leeks;
	public static SpriteTexture Lentils;
	public static SpriteTexture Lettuce;
	public static SpriteTexture Melon;
	public static SpriteTexture OliveOil;
	public static SpriteTexture Onion;
	public static SpriteTexture Peas;
	public static SpriteTexture Radishes;
	public static SpriteTexture Scallions;
	public static SpriteTexture Turnips;
	public static SpriteTexture Wheat;

	// UI
	public static SpriteTexture Clock;
	public static SpriteTexture ClockHand;
	public static SpriteTexture BottomLeftPanel;
	public static SpriteTexture BottomPanel;
	public static SpriteTexture TallPanel;
	public static SpriteTexture BigPanel;
	public static SpriteTexture SmallPanel;
	public static List<SpriteTexture> BottomLeftButtons;
	public static List<SpriteTexture> BottomLeftButtonsHover;
	public static SpriteTexture Tooltip;
	public static SpriteTexture VerticalGreenBar;
	public static SpriteTexture VerticalBar;
	public static SpriteTexture TabBackground;
	public static SpriteTexture TabSelected;
	public static SpriteTexture TabUnselected;
	public static SpriteTexture EscapeMenu;
	public static SpriteTexture MenuButton;
	public static SpriteTexture MenuButtonHover;
	public static SpriteTexture XButton;
	public static SpriteTexture XButtonHover;
	public static SpriteTexture AccodionSection;
	public static SpriteTexture AccodionSectionExpanded;
	public static SpriteTexture RightClickMenu;
	public static SpriteTexture MenuItem;
	public static SpriteTexture MenuItemHover;
	public static SpriteTexture CropIcon;
	public static SpriteTexture ArrowLeft;
	public static SpriteTexture ArrowRight;
	public static SpriteTexture DarkPanel;
	public static SpriteTexture DarkPanelTop;
	public static SpriteTexture DarkPanelTopDarker;
	public static SpriteTexture DarkPanelBottom;

	// Market UI
	public static SpriteTexture Buy1;
	public static SpriteTexture Buy10;
	public static SpriteTexture Buy100;

	// Temple UI (Senet)
	public static SpriteTexture SenetBoard;
	public static SpriteTexture SenetPiece1;
	public static SpriteTexture SenetPiece2;
	public static SpriteTexture Anubis;
	public static SpriteTexture AnubisHover;
	public static SpriteTexture Horus;
	public static SpriteTexture HorusHover;
	public static SpriteTexture Isis;
	public static SpriteTexture IsisHover;
	public static SpriteTexture Osiris;
	public static SpriteTexture OsirisHover;
	public static SpriteTexture Ra;
	public static SpriteTexture RaHover;
	public static SpriteTexture Set;
	public static SpriteTexture SetHover;
	public static SpriteTexture Thoth;
	public static SpriteTexture ThothHover;
	public static SpriteTexture SenetStick;
	public static SpriteTexture SenetRoll;
	public static SpriteTexture SenetRollHover;

	// Map
	public static List<SpriteTexture> desertTextures;
	public static List<SpriteTexture> desertHillTextures;
	public static List<SpriteTexture> desertVegetationTextures;
	public static List<SpriteTexture> desertBedouinTextures;
	public static List<SpriteTexture> desertForestTextures;
	public static List<SpriteTexture> desertRiverTextures;
	public static List<SpriteTexture> fogTextures;
	public static SpriteTexture pavedTexture;

	// Misc
	public static SpriteTexture Coin;
	public static SpriteTexture Cursor;

	// Resusable sprites
	public static Sprite Paved;
	//public static Sprite BuildingShadow;

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
		SmallFont = content.Load<SpriteFont>("agencyr_small");
		FontAgencyL = content.Load<SpriteFont>("agencyr_large");

		// Map textures
        desertTextures = Sprites.LoadTextures("desert/flat", 18);
        desertHillTextures = Sprites.LoadTextures("desert/hills", 7);
        desertVegetationTextures = Sprites.LoadTextures("desert/vegetation", 12);
        desertBedouinTextures = Sprites.LoadTextures("desert/bedouin_camps", 5);
		desertForestTextures = Sprites.LoadTextures("desert/forest", 5);
		desertRiverTextures = Sprites.LoadTextures("desert/river", 16);
		fogTextures = Sprites.LoadTextures("fog", 13);

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
		city1Composite = LoadTextures("buildings/city/composite", 6);
		markets = LoadTextures("buildings/market", 7);
		market1Composite = LoadTextures("buildings/market/composite", 4);
		smithies = LoadTextures("buildings/smithy", 4);
		temples = LoadTextures("buildings/temple", 1);
		ovens = LoadTextures("buildings/oven", 3);
		tanneries = LoadTextures("buildings/tannery", 1);
		taverns = LoadTextures("buildings/tavern", 2);

		Pyramid25 = LoadTexture("buildings/pyramid/pyramid_25");
		Pyramid50 = LoadTexture("buildings/pyramid/pyramid_50");
		Pyramid75 = LoadTexture("buildings/pyramid/pyramid_75");
		Pyramid100 = LoadTexture("buildings/pyramid/pyramid_100");

		pavedTexture = LoadTexture("buildings/paved");
		Paved = Sprite.Create(pavedTexture, Vector2.Zero);
		//BuildingShadow = Sprite.Create(LoadTexture("buildings/buildingShadow"), Vector2.Zero);

		barracksCon = LoadTextures("buildings/barracks/construction", 1);
		farmsCon = LoadTextures("buildings/farm/construction", 1);
		farmsRiverCon = LoadTextures("buildings/farm_river/construction", 1);
		granariesCon = LoadTextures("buildings/granary/construction", 1);
		housesCon = LoadTextures("buildings/house/construction", 1);
		minesCon = LoadTextures("buildings/mine/construction", 1);
		ranchesCon = LoadTextures("buildings/ranch/construction", 1);
		//citiesCon = LoadTextures("buildings/city/construction", 1);
		marketsCon = LoadTextures("buildings/market/construction", 1);
		smithiesCon = LoadTextures("buildings/smithy/construction", 1);
		templesCon = LoadTextures("buildings/temple/construction", 1);
		//ovensCon = LoadTextures("buildings/oven/construction", 1);
		tanneriesCon = LoadTextures("buildings/tannery/construction", 1);

		decorations = LoadTextures("decorations", 10);

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

		// Plants
		Barley = LoadTexture("plant_icons/barley");
		Celery = LoadTexture("plant_icons/celery");
		Chickpeas = LoadTexture("plant_icons/chickpeas");
		Cucumber = LoadTexture("plant_icons/cucumber");
		Garlic = LoadTexture("plant_icons/garlic");
		Gourd = LoadTexture("plant_icons/gourd");
		Grapes = LoadTexture("plant_icons/grapes");
		Leeks = LoadTexture("plant_icons/leeks");
		Lentils = LoadTexture("plant_icons/lentils");
		Lettuce = LoadTexture("plant_icons/lettuce");
		Melon = LoadTexture("plant_icons/watermelon");
		OliveOil = LoadTexture("plant_icons/olive_oil");
		Onion = LoadTexture("plant_icons/onion");
		Peas = LoadTexture("plant_icons/peas");
		Radishes = LoadTexture("plant_icons/radishes");
		Scallions = LoadTexture("plant_icons/scallions");
		Turnips = LoadTexture("plant_icons/turnips");
		Wheat = LoadTexture("plant_icons/wheat");

		// UI
		Clock = LoadTexture("UI_maybe/clock");
		ClockHand = LoadTexture("UI_maybe/clock_hand");
		BottomLeftPanel = LoadTexture("UI_maybe/panel1");
		BottomPanel = LoadTexture("UI_maybe/bottom_panel");
		TallPanel = LoadTexture("UI_maybe/tall_panel");
		BigPanel = LoadTexture("UI_maybe/bigPanel");
		SmallPanel = LoadTexture("UI_maybe/small_panel");
		BottomLeftButtons = LoadTextures("UI_maybe/buttons", 8);
		BottomLeftButtonsHover = LoadTextures("UI_maybe/buttons/hover", 8);
		Tooltip = LoadTexture("UI_maybe/tooltip");
		VerticalGreenBar = LoadTexture("UI_maybe/vertical_green_bar");
		VerticalBar = LoadTexture("UI_maybe/vertical_bar");
		TabBackground = LoadTexture("UI_maybe/tab_background");
		TabSelected = LoadTexture("UI_maybe/tab_selected");
		TabUnselected = LoadTexture("UI_maybe/tab_unselected");
		EscapeMenu = LoadTexture("UI_maybe/escapeMenuPanel");
		MenuButton = LoadTexture("UI_maybe/buttons/menuButton");
		MenuButtonHover = LoadTexture("UI_maybe/buttons/menuButtonHover");
		XButton = LoadTexture("UI_maybe/buttons/xbutton");
		XButtonHover = LoadTexture("UI_maybe/buttons/xbuttonHover");
		AccodionSection = LoadTexture("UI_maybe/accordionSection");
		AccodionSectionExpanded = LoadTexture("UI_maybe/accordionSectionExpanded");
		RightClickMenu = LoadTexture("UI_maybe/rightClickMenu");
		MenuItem = LoadTexture("UI_maybe/menuItem");
		MenuItemHover = LoadTexture("UI_maybe/menuItemHover");
		CropIcon = LoadTexture("UI_maybe/cropIcon");
		ArrowLeft = LoadTexture("UI_maybe/arrowLeft");
		ArrowRight = LoadTexture("UI_maybe/arrowRight");
		DarkPanel = LoadTexture("UI_maybe/darkPanel");
		DarkPanelTop = LoadTexture("UI_maybe/darkPanelTop");
		DarkPanelTopDarker = LoadTexture("UI_maybe/darkPanelTopDarker");
		DarkPanelBottom = LoadTexture("UI_maybe/darkPanelBottom");

		// UI - Market
		Buy1 = LoadTexture("UI_maybe/buttons/buy1");
		Buy10 = LoadTexture("UI_maybe/buttons/buy10");
		Buy100 = LoadTexture("UI_maybe/buttons/buy100");

		// UI - Temple (Senet)
		SenetBoard = LoadTexture("UI_maybe/senet/board");
		SenetPiece1 = LoadTexture("UI_maybe/senet/piece1");
		SenetPiece2 = LoadTexture("UI_maybe/senet/piece2");
		Anubis = LoadTexture("UI_maybe/senet/anubis");
		AnubisHover = LoadTexture("UI_maybe/senet/anubisHover");
		Horus = LoadTexture("UI_maybe/senet/horus");
		HorusHover = LoadTexture("UI_maybe/senet/horusHover");
		Isis = LoadTexture("UI_maybe/senet/isis");
		IsisHover = LoadTexture("UI_maybe/senet/isisHover");
		Osiris = LoadTexture("UI_maybe/senet/osiris");
		OsirisHover = LoadTexture("UI_maybe/senet/osirisHover");
		Ra = LoadTexture("UI_maybe/senet/ra");
		RaHover = LoadTexture("UI_maybe/senet/raHover");
		Set = LoadTexture("UI_maybe/senet/set");
		SetHover = LoadTexture("UI_maybe/senet/setHover");
		Thoth = LoadTexture("UI_maybe/senet/thoth");
		ThothHover = LoadTexture("UI_maybe/senet/thothHover");
		SenetStick = LoadTexture("UI_maybe/senet/stick");
		SenetRoll = LoadTexture("UI_maybe/senet/rollButton");
		SenetRollHover = LoadTexture("UI_maybe/senet/rollButtonHover");

		// Misc
		Coin = LoadTexture("misc/coin");
		Cursor = LoadTexture("misc/cursor");
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

	public static SpriteTexture RandomBuilding(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
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
			case BuildingType.TEMPLE: textures = temples; break;
			case BuildingType.OVEN: textures = ovens; break;
			case BuildingType.TANNERY: textures = tanneries; break;
			case BuildingType.TAVERN: textures = taverns; break;
			default: textures = buildings; break;
		}

		return Random(textures);
	}

	public static SpriteTexture RandomConstruction(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
	{
		List<SpriteTexture> textures = null;
		switch (buildingType)
		{
			case BuildingType.MINE: textures = minesCon; break;
			case BuildingType.HOUSE: textures = housesCon; break;
			case BuildingType.RANCH: textures = ranchesCon; break;
			case BuildingType.FARM: textures = farmsCon; break;
			case BuildingType.FARM_RIVER: textures = farmsRiverCon; break;
			case BuildingType.BARRACKS: textures = barracksCon; break;
			case BuildingType.GRANARY: textures = granariesCon; break;
			case BuildingType.MARKET: textures = marketsCon; break;
			case BuildingType.CITY: textures = cities; break; // No con textures
			case BuildingType.SMITHY: textures = smithiesCon; break;
			case BuildingType.OVEN: textures = ovens; break; // No con texture yet
			case BuildingType.TEMPLE: textures = templesCon; break;
			case BuildingType.TANNERY: textures = tanneriesCon; break;
			case BuildingType.TAVERN: textures = taverns; break; // No con texture yet
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

	public static SpriteTexture RandomFog()
	{
		return Random(fogTextures);	
	}

	public static SpriteTexture RandomDecoration()
	{
		return Random(decorations);
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
