using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class Kingdom
{
    public const int START_MAX_TILES = 25;

    // Serialized content
    public int Day { get; set; }
    public Player Owner { get; set; }
    public Tile StartTile { get; set; }
    public int MaxTiles { get; set; }
    public float TaxRate { get; set; }
    public int StarvationDeaths {get; set; }
    public List<Tile> OwnedTiles { get; set; }
    public List<Person> People { get; set; }
    public List<Person> Deceased { get; set; }
    public Stockpile Treasury { get; set; }
    public Military Army { get; set; }

    public Kingdom()
    {
        Day = 1;
        MaxTiles = START_MAX_TILES;
        OwnedTiles = new();
        People = new();
        Deceased = new();
        Treasury = new();
        Army = Military.Create(this);
        TaxRate = 0.1f;
        StarvationDeaths = 0;
    }

    public static Kingdom Create(Player owner, Tile startTile)
    {
        Kingdom kingdom = new();
        kingdom.SetAttributes(owner, startTile);
        return kingdom;
    }

    public void SetAttributes(Player owner, Tile startTile)
    {
        Owner = owner;
        StartTile = startTile;
    }

    public void Init()
    {
        // Start with 25 tiles centered around the start tile, which will contain a market
        AcquireTilesAround(StartTile, distance: 2);
        ExploreTilesAround(StartTile, distance: 7);

        Building city = Building.CreateBuilding(StartTile, BuildingType.CITY);
        city.BuildProgress = 1f;

        Building market = Building.CreateBuilding(StartTile.Neighbors[0], BuildingType.MARKET);
        market.BuildProgress = 1f;

        Treasury.Add(Goods.GetId(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 0), 200f);
        Treasury.Add(Goods.GetId(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 0), 100f);
    }

    // Checks if tile is adjacent to one owned by the player
    public bool TryToAcquireTile(Tile tile)
    {
        if (OwnedTiles.Count >= MaxTiles)
            return false;
        if (tile == null || tile.Owner != null)
            return false;

        foreach (Tile neighbor in tile.Neighbors)
            if (neighbor != null && neighbor.Owner == Owner)
                return AcquireTile(tile);
        return false;
    }

    public bool AcquireTile(Tile tile)
    {
        if (tile == null)
            return false;
        OwnedTiles.Add(tile);
        tile.Highlight();
        tile.Owner = Owner;

        if (tile.PlantId != 0)
            Owner.UnlockPlant(tile.PlantId);

        return true;
    }

    public void ExploreTilesAround(Tile tile, int distance = 1, bool circular = true)
    {
        ActOnTilesAround(ExploreTile, tile, distance, circular);
    }

    public void ExploreTile(Tile tile)
    {
        tile.Explore();
    }

    public void AcquireTileVoid(Tile tile)
    {
        AcquireTile(tile);
    }

    // Be careful not to add the same tile twice to the OwnedTiles list
    public bool AcquireTilesAround(Tile tile, int distance = 1)
    {
        return ActOnTilesAround(AcquireTileVoid, tile, distance);
    }

    public bool ActOnTilesAround(Action<Tile> action, Tile tile, int distance = 1, bool circular = false)
    {
        action(tile);
        int num_tiles = (2 * distance + 1) * (2 * distance + 1);

        Tile current = tile;
        Rectangle bounds = tile.BaseSprite.GetBounds();

        // Take 1 step NE, then 1 step SE, then 2 steps SW, 2 steps NW, and so on
        // to complete a ring around the starting tile. Keep making larger rings
        // until all tiles within the requested radius have been claimed
        int i = 1;
        float steps = 1f;
        int direction = (int)Cardinal.NE;
        while (i < num_tiles)
        {
            for (int j = 0; j < (int)steps && current != null; j++)
            {
                current = current.Neighbors[direction];

                if (current == null)
                    break;

                if (!circular)
                {
                    action(current);
                } 
                else
                {
                    Vector2 tileOrigin = tile.GetPosition() + tile.GetOrigin();
                    Vector2 currentOrigin = current.GetPosition() + tile.GetOrigin();

                    if (Math.Abs(tileOrigin.X - currentOrigin.X) < bounds.Width * (distance - 1.5) &&
                        Math.Abs(tileOrigin.Y - currentOrigin.Y) < bounds.Height * (distance - 2.4))
                    {
                        action(current);
                    }
                }
                    

                if (++i >= num_tiles)
                    break;
            }
            direction = (int)Tile.GetNextDirection(direction);
            steps += 0.5f;

            if (current == null)
                break;
        }

        return true;
    }

    public void AddPerson(Person person)
    {
        person.Owner = Owner;
        People.Add(person);
    }

    public void PersonDied(Person person)
    {
        if (person.Hunger >= Person.STARVED_TO_DEATH)
            StarvationDeaths++;
        Deceased.Add(person);
    }

    public void Update()
    {
        // For now, 1 tile allowed per 100 wealth
        MaxTiles = (int)(START_MAX_TILES + PublicWealth() * 0.01f);

        // Remove every person who died
        foreach (Person p in Deceased)
            People.Remove(p);

        Deceased.Clear();

        foreach (Person p in People)
            p.Update();
    }

    public void DailyUpdate()
    {
        Day++;
        Treasury.DailyUpdate();
    }

    public float TotalWealth()
    {
        return PublicWealth() + PrivateWealth();
    }

    public float PublicWealth()
    {
        return Owner.Person.Money + Treasury.Wealth();
    }

    public float PrivateWealth()
    {
        float wealth = 0f;
        foreach (Person p in People)
            wealth += p.Wealth();
        return wealth;
    }

    // Add up all the goods in all citizen's stockpiles, plus the tile stockpiles
    // and return a detailed list of goods, quantities, and values
    public Stockpile PrivateGoods()
    {
        Stockpile total = new();

        // Sum all goods stored in villager inventories
        foreach (Person p in People)
            total.Sum(p.PersonalStockpile);

        // Sum all goods stored in villager houses
        foreach (Tile t in OwnedTiles)
            foreach (Building b in t.Buildings)
                total.Sum(b.Stockpile);

        return total;
    }

    public string Statistics()
    {
        float totalHunger = 0f;
        float totalAge = 0f;
        float totalWealth = 0f;
        int totalLevel = 0;
        int homeless = 0;
        foreach (Person p in People)
        {
            totalWealth += p.Wealth();
            if (p.House == null)
                homeless++;
            else
                totalWealth += p.House.Wealth() / p.House.CurrentUsers.Count;
            totalHunger += p.Hunger;
            totalAge +=  p.Age;
            foreach (SkillLevel s in p.Skills._list)
                totalLevel += s.level;
        }

        return 
            $"Day: {Day}\n" + 
            $"Number of People: {People.Count}\n" + 
            $"Homelessness: {homeless}/{People.Count}\n" + 
            $"Average Hunger: {totalHunger / People.Count}/{Person.STARVED_TO_DEATH}\n" + 
            $"Starvation Deaths: {StarvationDeaths}\n" + 
            $"Average Age: {totalAge / People.Count}\n" + 
            $"Average Wealth: {totalWealth / People.Count}\n" +
            $"Average Skill Total: {totalLevel / People.Count}";
    }
}