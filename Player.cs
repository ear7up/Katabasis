public class Player
{
    public Kingdom Kingdom;
    
    public Player(Tile startTile)
    {
        Kingdom = new(this, startTile);
    }

    public void Update()
    {
        Kingdom.Update();
    }

    public void DailyUpdate()
    {
        Kingdom.DailyUpdate();
    }
}