using System.Collections.Generic;

public class Kingdom
{
    public Player Owner;
    public Tile StartTile;
    public List<Tile> OwnedTiles;
    public List<Person> People;
    public List<Person> Deceased;

    public Kingdom(Player owner, Tile startTile)
    {
        Owner = owner;
        StartTile = startTile;
        OwnedTiles = new();
        OwnedTiles.Add(startTile);
        People = new();
        Deceased = new();
    }

    // Checks if tile is adjacent to one owned by the player
    public bool TryToAcquireTile(Tile tile)
    {
        foreach (Tile neighbor in tile.Neighbors)
            if (neighbor.Owner == Owner)
                return AcquireTile(tile);
        return false;
    }

    public bool AcquireTile(Tile tile)
    {
        OwnedTiles.Add(tile);
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
        // Remove every person who died
        foreach (Person p in Deceased)
            People.Remove(p);

        Deceased.Clear();

        foreach (Person p in People)
            p.Update();
    }
}