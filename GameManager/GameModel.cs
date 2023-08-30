using System.Collections.Generic;
using Katabasis;

public class GameModel
{
    // Serialized content
    public float TimeOfDay { get; set; }
    public Camera GameCamera { get; set; }
    public Player Player1 { get; set; }
    public Market Market { get; set; }
    public Map TileMap { get; set; }

    public GameModel()
    {
        
    }

    // Called when creating a new game, adds people to the world
    public void InitNew()
    {
        TileMap = new();
        TileMap.Generate();
        GameCamera = Camera.Create(KatabasisGame.Viewport, TileMap.Origin);

        Player1 = Player.Create(TileMap.GetOriginTile());
        Player1.Kingdom.Init();

        // Only one market will exist at any time
        Market = new();
        Market.SetAttributes(Player1.Kingdom);

        const int NUM_PEOPLE = 100;
        for (int i = 0 ; i < NUM_PEOPLE; i++)
        {
            Person person = Person.CreatePerson(TileMap.Origin, TileMap.GetOriginTile());
            person.Money = Globals.Rand.Next(20, 50);
            Player1.Kingdom.AddPerson(person);
        }

        Globals.Model = this;
    }

    // If loading from a save file, set static variables and calculate unserialized content
    public void InitLoaded()
    {
        TileMap.ComputeNeighbors();
        Globals.Model = this;
    }
}