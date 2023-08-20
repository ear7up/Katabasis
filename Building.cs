using System;

public enum BuildingType
{
    CITY,
    MARKET,
    HOUSE,
    LUMBERMILL,
    FORGE,
    FARM,
    FARM_RIVER,
    RANCH,
    MINE,
    SMITHY,
    BARRACKS,
    GRANARY,
    NONE
}

public enum BuildingSubType
{
    GOLD_MINE,
    SILVER_MINE,
    COPPER_MINE,
    LEAD_MINE,
    MALACHITE_MINE,
    LAPIS_LAZULI_MINE,
    TIN_MINE,
    IRON_MINE,
    SALT_MINE,
    NONE
}

public class Building : Drawable
{
    public const int MAX_BUILDINGS_PER_TILE = 4;

    public static int IdCounter = 0;
    public int Id;

    // Serialized content
    public Tile Location { get; set; }
    public BuildingType Type { get; set; }
    public BuildingSubType SubType { get; set; }
    public Sprite Sprite { get; set; }
    public int CurrentUsers { get; set; }
    public bool Selected { get; set; }
    public Stockpile Stockpile { get; set; }

    // No need to persist
    public int MaxUsers;
    public TextSprite SelectedText;

    public static Building Random(BuildingType type = BuildingType.NONE, bool temporary = false)
    {
        // Random building type (excluding NONE)
        if (type == BuildingType.NONE)
        {
            Array buildingTypes = Enum.GetValues(typeof(BuildingType));
            type = (BuildingType)Globals.Rand.Next(buildingTypes.Length - 1);
        }

        Building b = CreateBuilding(null, type);
        return b;
    }

    public void SetAttributes(Tile location, Sprite sprite, BuildingType buildingType = BuildingType.NONE)
    {
        Location = location;
        Sprite = sprite;
        Type = buildingType;

        MaxUsers = BuildingInfo.GetMaxUsers(buildingType);
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
    }

    public static Building CreateBuilding(Tile location, BuildingType buildingType = BuildingType.NONE)
    {
        // Try lay out the building in the empty space on the diamond based on building count
        Vector2 position = Vector2.Zero;

        if (location != null)
        {
            position.X = location.GetPosition().X;
            position.Y = location.GetPosition().Y;
        }

        // TODO: need sprites for all building types
        Sprite sprite = new Sprite(Sprites.RandomBuilding(buildingType), position);
        Building b = new Building();
        b.SetAttributes(location, sprite, buildingType);

        if (!b.IsWholeTile())
            sprite.SetScale(0.4f);

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
               Type == BuildingType.MARKET;
    }
    
    public void StartUsing()
    {
        CurrentUsers++;
    }

    public void StopUsing()
    {
        CurrentUsers--;
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
        if (building.Type == BuildingType.RANCH && location.Type != TileType.WILD_ANIMAL)
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
            building.Sprite.Texture = Sprites.GetRiverFarmSprite();
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

    public void Update()
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE)
        {
            if (InputManager.UnconsumedClick() && Sprite.Contains(InputManager.MousePos))
            {
                InputManager.ConsumeClick(this);
                Selected = true;

                if (Type == BuildingType.MARKET)
                {
                    Console.WriteLine(Market.Describe());
                    Katabasis.GameManager.ToggleMarketPanel();
                }
                else if (Type == BuildingType.HOUSE)
                    Console.WriteLine("House contents:\n" + Stockpile.ToString());
            }
            else if (Selected && InputManager.Clicked && 
                (InputManager.ClickConsumer == null || !(InputManager.ClickConsumer is UIElement))) 
            {
                if (Type == BuildingType.MARKET)
                    Katabasis.GameManager.ToggleMarketPanel();
                Selected = false;
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + " " + Sprite.Position.ToString() + " " + Type.ToString() + " " +
            $"users=({CurrentUsers}/{MaxUsers})";
    }

    public void Draw()
    {
        if (Selected)
        {
            Sprite.SpriteColor = Color.Cyan;
            Sprite.ScaleUp(0.025f);
            Sprite.Draw();

            Sprite.SpriteColor = Color.White;
            Sprite.UndoScaleUp(0.025f);

            SelectedText.Unhide();
            SelectedText.Text = $"Occupants: ({CurrentUsers}/{MaxUsers})";

            Rectangle bounds = Sprite.GetBounds();
            SelectedText.Position = new Vector2(
                bounds.X + bounds.Width / 2 - SelectedText.Width() / 2,
                bounds.Y + bounds.Height * 0.8f);
        }
        else
        {
            SelectedText.Hide();
        }

        Sprite.Draw();
    }

    public float GetMaxY()
    {
        // For perspective, let Person sprites be drawn over top of the bottom 30% of the building
        return Sprite.GetMaxY() - (Sprite.Scale.Y * Sprite.Texture.Height * 0.3f) + (Id * 0.001f);
    }

    public void DailyUpdate()
    {
        Stockpile.DailyUpdate();
    }

    public float Wealth()
    {
        return Stockpile.Wealth();
    }
}