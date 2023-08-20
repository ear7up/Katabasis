using System.IO;
using System.Text.Json;

public class Player
{
    public Kingdom Kingdom { get; set; }
    
    public Player(Tile startTile)
    {
        Kingdom = new(this, startTile);
    }

    public void Save(FileStream fileStream)
    {
        JsonSerializer.Serialize(fileStream, this, Globals.JsonOptions);
    }

    public void Load()
    {
        
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