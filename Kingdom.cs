using System.Collections.Generic;

public class Kingdom
{
    public const int START_MAX_TILES = 25;

    public int Day;
    public Player Owner;
    public Tile StartTile;
    public int MaxTiles;
    public List<Tile> OwnedTiles;
    public List<Person> People;
    public List<Person> Deceased;
    public Stockpile Treasury;
    public float Money;
    public float TaxRate;
    public int StarvationDeaths;

    public Kingdom(Player owner, Tile startTile)
    {
        Day = 1;
        Owner = owner;
        StartTile = startTile;
        MaxTiles = START_MAX_TILES;
        OwnedTiles = new();
        People = new();
        Deceased = new();
        Treasury = new();
        Money = 1000f;
        TaxRate = 0.1f;
        StarvationDeaths = 0;

        // Start with 25 tiles centered around the start tile, which will contain a market
        AcquireTilesAround(startTile, distance: 2);
        Building city = Building.CreateBuilding(startTile, BuildingType.CITY);
        Building market = Building.CreateBuilding(startTile.Neighbors[0], BuildingType.MARKET);
        //market.Sprite.Scale = 0.5f;

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
        return true;
    }

    // Be careful not to add the same tile twice to the OwnedTiles list
    public bool AcquireTilesAround(Tile tile, int distance = 1)
    {
        AcquireTile(tile);
        int num_tiles = (2 * distance + 1) * (2 * distance + 1);

        Tile current = tile;

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
                AcquireTile(current);
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
        return Treasury.Wealth() + Money;
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
    public string PrivateGoods()
    {
        string s = "Privately Owned Goods\n====================\n";
        Stockpile total = new();

        // Sum all goods stored in villager inventories
        foreach (Person p in People)
            total.Sum(p.PersonalStockpile);

        // Sum all goods stored in villager houses
        foreach (Tile t in OwnedTiles)
            foreach (Building b in t.Buildings)
                total.Sum(b.Stockpile);

        s += total.ToString();
        return s;
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
                totalWealth += p.House.Wealth() / p.House.CurrentUsers;
            totalHunger += p.Hunger;
            totalAge +=  p.Age;
            foreach (SkillLevel s in p.Skills)
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