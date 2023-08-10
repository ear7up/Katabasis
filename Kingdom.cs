using System.Collections.Generic;

public class Kingdom
{
    public const int START_MAX_TILES = 20;

    public Player Owner;
    public Tile StartTile;
    public int MaxTiles;
    public List<Tile> OwnedTiles;
    public List<Person> People;
    public List<Person> Deceased;
    public Stockpile Treasury;

    public Kingdom(Player owner, Tile startTile)
    {
        Owner = owner;
        StartTile = startTile;
        MaxTiles = START_MAX_TILES;
        OwnedTiles = new();
        AcquireTilesAround(startTile);
        People = new();
        Deceased = new();
        Treasury = new();
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
        OwnedTiles.Add(tile);
        tile.Highlight();
        tile.Owner = Owner;
        return true;
    }

    // Be careful not to add the same tile twice to the OwnedTiles list
    public bool AcquireTilesAround(Tile tile)
    {
        AcquireTile(tile);
        foreach (Tile neighbor in tile.Neighbors)
            if (neighbor != null)
                AcquireTile(neighbor);
        return true;
    }

    public void AddPerson(Person person)
    {
        person.Owner = Owner;
        People.Add(person);
    }

    public void PersonDied(Person person)
    {
        Deceased.Add(person);
    }

    public void Update()
    {
        // For now, 1 tile allowed per 1000 wealth
        MaxTiles = (int)(START_MAX_TILES + PublicWealth() * 0.001f);

        // Remove every person who died
        foreach (Person p in Deceased)
            People.Remove(p);

        Deceased.Clear();

        foreach (Person p in People)
            p.Update();
    }

    public void DailyUpdate()
    {
        // TODO: Disable decay for food in granary?
        Treasury.DailyUpdate();
    }

    public float TotalWealth()
    {
        return PublicWealth() + PrivateWealth();
    }

    public float PublicWealth()
    {
        return Treasury.Wealth();
    }

    public float PrivateWealth()
    {
        float wealth = 0f;
        foreach (Person p in People)
            wealth += p.Wealth();
        foreach (Tile t in OwnedTiles)
            wealth += t.Wealth();
        return wealth;
    }

    // Add up all the goods in all citizen's stockpiles, plus the tile stockpiles
    // and return a detailed list of goods, quantities, and values
    public string PrivateGoods()
    {
        string s = "Privately Owned Goods\n====================\n";
        Stockpile total = new();
        foreach (Person p in People)
            total.Sum(p.PersonalStockpile);
        foreach (Tile t in OwnedTiles)
            total.Sum(t.Stockpile);
        s += total.ToString();
        return s;
    }
}