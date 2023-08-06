public class Player
{
    public Stockpile Property;
    public Kingdom Kingdom;
    
    public Player(Tile startTile)
    {
        Property = new();
        Kingdom = new(this, startTile);
    }

    public void Update()
    {
        Property.Update();
        Kingdom.Update();
    }
}