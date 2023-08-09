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
        Kingdom.Update();
    }

    public void DailyUpdate()
    {
        Property.DailyUpdate();
    }
}