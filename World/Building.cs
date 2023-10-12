using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

public enum BuildingType
{
    CITY,
    MARKET,
    HOUSE,
    BRICK_HOUSE,
    LUMBERMILL,
    FORGE,
    FARM,
    FARM_RIVER,
    RANCH,
    MINE,
    SMITHY,
    BARRACKS,
    GRANARY,
    TEMPLE,
    TANNERY,
    TAVERN,
    OVEN,
    PYRAMID,
    GARDEN,
    NONE
}

public enum BuildingSubType
{
    NONE = 0,
    GOLD_MINE,
    SILVER_MINE,
    COPPER_MINE,
    LEAD_MINE,
    MALACHITE_MINE,
    LAPIS_LAZULI_MINE,
    TIN_MINE,
    IRON_MINE,
    SALT_MINE,
    BRICK,
    WOOD
}

[JsonDerivedType(derivedType: typeof(PyramidBuilding), typeDiscriminator: "PyramidBuilding")]
public class Building : Drawable
{
    public const int MAX_BUILDING_SUBTYPES = 100;
    public const int MAX_BUILDINGS_PER_TILE = 4;

    public static int IdCounter = 0;
    public static Building SelectedBuilding = null;

    // Serialized content
    public int Id { get; set; }
    public Tile Location { get; set; }
    public BuildingType Type { get; set; }
    public BuildingSubType SubType { get; set; }
    public Sprite Sprite { get; set; }
    public Sprite ConstructionSprite { get; set; }
    public List<Sprite> Composite { get; set; }
    public List<Person> CurrentUsers { get; set; }
    public bool Selected { get; set; }
    public int MaxUsers { get; set; }
    public float BuildProgress { get; set; }

    // Only used for houses
    public Stockpile Stockpile { get; set; }
    public float Money { get; set; }

    // Hack to make the JSON deserializer populate the ybuffer
    public bool Unused {
        get { return false; }
        set { AddToYBuffer(); }
    }

    // No need to persist
    public TextSprite SelectedText;

    public static Building Random(
        BuildingType type,
        BuildingSubType subType, 
        bool temporary = false)
    {
        // Random building type (excluding NONE)
        if (type == BuildingType.NONE)
        {
            Array buildingTypes = Enum.GetValues(typeof(BuildingType));
            type = (BuildingType)Globals.Rand.Next(buildingTypes.Length - 1);
        }

        Building b = CreateBuilding(null, type, subType);
        return b;
    }

    public void SetAttributes(
        Tile location, 
        Sprite sprite, 
        Sprite constructionSprite, 
        BuildingType buildingType,
        BuildingSubType subType = BuildingSubType.NONE)
    {
        Location = location;
        Sprite = sprite;
        ConstructionSprite = constructionSprite;
        Type = buildingType;
        SubType = subType;

        MaxUsers = BuildingInfo.GetMaxUsers(buildingType, subType);
    }

    public Building()
    {
        Id = IdCounter++;
        SubType = BuildingSubType.NONE;
        Selected = false;
        SelectedText = new TextSprite(Sprites.Font);
        SelectedText.ScaleDown(0.3f);
        SelectedText.Hide();
        Globals.TextBuffer.Add(SelectedText);
        Stockpile = new();
        CurrentUsers = new();
        Composite = new();
        Money = 0f;
    }

    public int GetId()
    {
        return (int)Type * MAX_BUILDING_SUBTYPES + (int)SubType;
    }

    public static int GetId(BuildingType type, BuildingSubType subType = BuildingSubType.NONE)
    {
        return (int)type * MAX_BUILDING_SUBTYPES + (int)subType;
    }

    public static BuildingType TypeFromId(int id)
    {
        return (BuildingType)(id / MAX_BUILDING_SUBTYPES);
    }

    public static BuildingSubType SubTypeFromId(int id)
    {
        return (BuildingSubType)(id % MAX_BUILDING_SUBTYPES);
    }

    public static void BuildCityComposite(Building building)
    {
        List<SpriteTexture> sprites = Sprites.city1Composite;

        // TODO: fixed offsets differ based on building scale
        Rectangle bounds = building.Sprite.GetBounds();
        Vector2 pos = new Vector2(bounds.X, bounds.Y + 25) + building.Sprite.Origin * 0.25f;
        building.Composite.Add(Sprite.Create(sprites[0], pos + new Vector2(98, 100)));
        building.Composite.Add(Sprite.Create(sprites[1], pos + new Vector2(63, 66)));
        building.Composite.Add(Sprite.Create(sprites[2], pos + new Vector2(242, 170)));
        building.Composite.Add(Sprite.Create(sprites[3], pos + new Vector2(250, 125)));
        building.Composite.Add(Sprite.Create(sprites[4], pos + new Vector2(267, 70)));
        building.Composite.Add(Sprite.Create(sprites[5], pos + new Vector2(151, 3)));
    }

    public static void BuildMarketComposite(Building building)
    {
        List<SpriteTexture> sprites = Sprites.market1Composite;

        Rectangle bounds = building.Sprite.GetBounds();
        Vector2 pos = new Vector2(bounds.X, bounds.Y + 25) + building.Sprite.Origin * 0.25f;
        building.Composite.Add(Sprite.Create(sprites[0], pos + new Vector2(172, 155)));
        building.Composite.Add(Sprite.Create(sprites[1], pos + new Vector2(217, 100)));
        building.Composite.Add(Sprite.Create(sprites[2], pos + new Vector2(260, 121)));
        building.Composite.Add(Sprite.Create(sprites[3], pos + new Vector2(190, 80)));
    }

    public void AddToYBuffer()
    {
        if (!IsWholeTile()) 
            Globals.Ybuffer.Add(this); 

        foreach (Sprite part in Composite)
            Globals.Ybuffer.Add(part);
    }

    public static Building CreateBuilding(
        Tile location, 
        BuildingType buildingType,
        BuildingSubType subType = BuildingSubType.NONE)
    {
        Vector2 position = Vector2.Zero;

        if (location != null)
        {
            position.X = location.GetPosition().X;
            position.Y = location.GetPosition().Y;
        }

        SpriteTexture spriteTexture = null;
        if (buildingType == BuildingType.MARKET)
            spriteTexture = Sprites.markets[5];
        else if (buildingType == BuildingType.PYRAMID)
            spriteTexture = Sprites.Pyramid100;
        else
            spriteTexture = Sprites.RandomBuilding(buildingType, subType);

        Sprite sprite = Sprite.Create(spriteTexture, position);
        Sprite conSprite = Sprite.Create(Sprites.RandomConstruction(buildingType, subType), position);

        Building b = null;
        if (buildingType == BuildingType.PYRAMID)
            b = new PyramidBuilding();
        else
            b = new Building();

        b.SetAttributes(location, sprite, conSprite, buildingType, subType);

        if (buildingType == BuildingType.CITY)
            BuildCityComposite(b);
        if (buildingType == BuildingType.MARKET)
            BuildMarketComposite(b);

        float scale = 0.4f;
        if (buildingType == BuildingType.OVEN)
            scale = 1.0f;

        if (!b.IsWholeTile())
        {
            sprite.SetScale(scale);
            conSprite?.SetScale(scale);
        }

        if (location != null)
            location.AddBuilding(b);

        return b;    
    }

    public bool IsWholeTile()
    {
        return Type == BuildingType.FARM || 
               Type == BuildingType.FARM_RIVER || 
               Type == BuildingType.MINE || 
               Type == BuildingType.RANCH || 
               Type == BuildingType.CITY ||
               Type == BuildingType.MARKET || 
               Type == BuildingType.GARDEN;
    }
    
    public void StartUsing(Person p)
    {
        CurrentUsers.Add(p);
    }

    public void StopUsing(Person p)
    {
        CurrentUsers.Remove(p);
    }

    public static bool EquivalentType(BuildingType a, BuildingType b)
    {
        if (a == b)
            return true;
        if (a == BuildingType.FARM && b == BuildingType.FARM_RIVER)
            return true;
        if (a == BuildingType.FARM_RIVER && b == BuildingType.FARM)
            return true;
        return false;
    }

    public static bool ValidPlacement(Building building, Tile location)
    {
        if (location == null)
            return false;
        if (building.Type == BuildingType.MINE && location.Type != TileType.HILLS)
            return false;
        if (building.Type == BuildingType.FARM_RIVER && location.Type != TileType.RIVER)
            return false;
        if (building.Type == BuildingType.RANCH && !TileAnimal.Domesticateable(location))
            return false;
        if (location.Buildings.Count >= MAX_BUILDINGS_PER_TILE)
            return false;
        if (building.IsWholeTile() && location.Buildings.Count != 0)
            return false;
        if (location.Owner == null)
            return false;
        if (location.Type == TileType.CAMP)
            return false;
        foreach (Building b in location.Buildings)
            if (b.IsWholeTile())
                return false;
        return true;
    }

    public static bool ConfirmBuilding(Building building, Tile location)
    {
        if (location == null || building == null)
            return false;

        // Force farms to be river type on rivers
        if (location.Type == TileType.RIVER && building.Type == BuildingType.FARM)
        {
            building.Type = BuildingType.FARM_RIVER;
            building.Sprite.SetNewSpriteTexture(Sprites.GetRiverFarmSprite());
        }

        if (ValidPlacement(building, location))
        {
            location.AddBuilding(building);
            building.Location = location;

            if (building.Type == BuildingType.MINE)
            {
                switch (location.Minerals)
                {
                    case MineralType.IRON: building.SubType = BuildingSubType.IRON_MINE; break;
                    case MineralType.COPPER: building.SubType = BuildingSubType.COPPER_MINE; break;
                    case MineralType.GOLD: building.SubType = BuildingSubType.GOLD_MINE; break;
                    case MineralType.LAPIS_LAZULI: building.SubType = BuildingSubType.LAPIS_LAZULI_MINE; break;
                    case MineralType.LEAD: building.SubType = BuildingSubType.LEAD_MINE; break;
                    case MineralType.MALACHITE: building.SubType = BuildingSubType.MALACHITE_MINE; break;
                    case MineralType.SILVER: building.SubType = BuildingSubType.SILVER_MINE; break;
                    case MineralType.TIN: building.SubType = BuildingSubType.TIN_MINE; break;
                    case MineralType.SALT: building.SubType = BuildingSubType.SALT_MINE; break;
                }
            }
            return true;
        }
        return false;
    }

    public virtual void Update()
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE)
        {
            if (InputManager.UnconsumedClick() && Sprite.Contains(InputManager.MousePos))
            {
                InputManager.ConsumeClick(this);
                Selected = !Selected;

                if (Selected)
                {
                    SelectedBuilding = this;
                    if (BuildProgress < 1f)
                        SoundEffects.Play(SoundEffects.BuildingSound);
                }

                if (Type == BuildingType.MARKET)
                    Katabasis.GameManager.ToggleMarketPanel();

                else if (Type == BuildingType.HOUSE)
                    Console.WriteLine("House contents:\n" + Stockpile.ToString());
            }
            else if (Selected && InputManager.Clicked && 
                (InputManager.ClickConsumer == null || !(InputManager.ClickConsumer is UIElement))) 
            {
                Selected = false;
                if (SelectedBuilding == this)
                    SelectedBuilding = null;
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + " " + Sprite.Position.ToString() + " " + Type.ToString() + " " +
            $"users=({CurrentUsers.Count}/{MaxUsers})";
    }

    public string Name()
    {
        string name = Globals.Title(Type.ToString());
        if (SubType != BuildingSubType.NONE)
            name += $"({Globals.Title(SubType.ToString())})";
        return name;
    }

    public string Describe()
    {
        string description = "";

        if (SubType != BuildingSubType.NONE)
            description += Globals.Title(SubType.ToString()) + " ";
        description += $"{Globals.Title(Type.ToString())}\n";

        description += $"Occupants: ({CurrentUsers.Count}/{MaxUsers})\n";

        if (BuildProgress < 1f)
            description += $"Build Progress: {(int)(100 * BuildProgress)}%\n";

        return description;
    }

    public Sprite GetSprite()
    {
        if (BuildProgress < 1f && Location != null)
            return ConstructionSprite;
        return Sprite;
    }

    public SpriteTexture GetSpriteTexture()
    {
        return new SpriteTexture(Sprite.TexturePathSerial, Sprite.Texture);
    }

    public void DrawSelected()
    {
        Sprite active = GetSprite();
        active.SpriteColor = Color.Cyan;
        active.ScaleUp(0.025f);
        active.Draw();

        active.SpriteColor = Color.White;
        active.UndoScaleUp(0.025f);
    }

    public void Draw()
    {
        /*
        if (Type != BuildingType.FARM && Type != BuildingType.MINE)
        {
            Sprites.BuildingShadow.Position = Sprite.Position;
            Sprites.BuildingShadow.Scale = Sprite.Scale;
            Sprites.BuildingShadow.Draw();
        }
        */

        if (Selected)
            DrawSelected();

        if (Selected || (BuildProgress < 1f && Location != null))
        {
            SelectedText.Unhide();

            if (BuildProgress < 1f)
                SelectedText.Text = $"Build Progress: ({(int)(100 * BuildProgress)}%)";
            else if (MaxUsers < 9999)
                SelectedText.Text = $"Occupants: ({CurrentUsers.Count}/{MaxUsers})";

            Rectangle bounds = Sprite.GetBounds();
            SelectedText.Position = new Vector2(
                bounds.X + bounds.Width / 2 - SelectedText.Width() / 2,
                bounds.Y + bounds.Height * 0.8f);
        }
        else
        {
            SelectedText.Hide();
        }

        GetSprite().Draw();

        if (Location == null)
            foreach (Sprite part in Composite)
                part.Draw();
    }

    // Keep in mind this is non-deterministic when there are multiple options for building materials
    public List<Goods> GetMaterials()
    {
        return BuildingProduction.GetRequirements(Type, SubType).GoodsRequirement.ToList();
    }

    // Calculate how expensive it will be to produce this building including materials and labor
    public static float MaterialCost(List<Goods> materials)
    {
        float materialCost = 0f;
        foreach (Goods goods in materials)
            materialCost += Globals.Model.Market.GetPrice(goods.GetId()) * goods.Quantity;
        return materialCost;
    }

    public static float LaborCost(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
    {
        // building cost $1.5/second
        float timeToProduce = BuildingInfo.GetBuildTime(buildingType, subType);
        float buildingCost = timeToProduce * 1.5f;

        // 10% of labor cost for hauling
        float haulingCost = buildingCost * 0.1f;

        return buildingCost + haulingCost;
    }

    public float GetMaxY()
    {
        // For perspective, let Person sprites be drawn over top of the bottom 30% of the building
        return Sprite.GetMaxY() - (Sprite.Scale.Y * Sprite.Texture.Height * 0.3f) + (Id * 0.001f);
    }

    public void DailyUpdate()
    {
        Stockpile.DailyUpdate();
        CurrentUsers.RemoveAll(x => x.IsDead);
    }

    public float Wealth()
    {
        return Stockpile.Wealth();
    }

    public void SetPosition(Vector2 newPosition)
    {
        // If there are composite sprites, update their positions
        // so that they maintain the same offset from the base sprite position
        Rectangle bounds = Sprite.GetBounds();
        Vector2 pos = Sprite.Position;

        foreach (Sprite part in Composite)
        {
            Vector2 offset = part.Position - pos;
            part.Position = newPosition + offset;
        }
        Sprite.Position = newPosition;

        if (ConstructionSprite != null)
            ConstructionSprite.Position = newPosition;
    }
}