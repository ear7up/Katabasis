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

    public float Wealth()
    {
        float wealth = 0f;
        foreach (Goods g in Property.Values())
        {
            // TODO: lookup value of good once values exist
            wealth += g.Quantity; /* Market.GetValue(g.GetId()); */
        }
        return wealth;
    }
}