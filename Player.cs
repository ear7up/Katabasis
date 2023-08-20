using System.IO;
using System.Text.Json;

public class Player
{
    // Serialized content
    public Kingdom Kingdom { get; set; }
    
    // TODO: remove constructor params
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