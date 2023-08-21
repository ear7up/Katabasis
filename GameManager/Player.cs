using System.IO;
using System.Text.Json;

public class Player
{
    // Serialized content
    public Kingdom Kingdom { get; set; }
    
    public Player()
    {
        
    }

    public static Player Create(Tile startTile)
    {
        Player player = new();
        player.SetAttributes(startTile);
        return player;
    }

    public void SetAttributes(Tile startTile)
    {
        Kingdom = Kingdom.Create(this, startTile);
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