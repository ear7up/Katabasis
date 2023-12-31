using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonTest
{
    public void Test()
    {
        TileExample t1 = new() { Type = "Desert", Neighbor = null };
        TileExample t2 = new() { Type = "Vegetation", Neighbor = t1 };

        PersonExample p1 = new() { Home = t2 };

        MapExample f = new();
        f.x = 4;
        f.tiles.Add(t1);
        f.tiles.Add(t2);
        f.p1 = p1;

        System.Console.WriteLine(f.ToString() + "\n");

        string jsonText = JsonSerializer.Serialize(f, Globals.JsonOptionsS);
        Console.WriteLine(jsonText + "\n");

        MapExample f2 = JsonSerializer.Deserialize<MapExample>(jsonText, Globals.JsonOptionsS);
        Console.WriteLine(f2.ToString());

        // P1 home is a reference to the second tile in the map
        Console.WriteLine("References honored? " + (f2.p1.Home == f2.tiles[1]).ToString());

        Task task = new EatTask();
        jsonText = JsonSerializer.Serialize(task, Globals.JsonOptionsS);
        Console.WriteLine(jsonText + "\n");

        Task task2 = JsonSerializer.Deserialize<Task>(jsonText, Globals.JsonOptionsD);
        string jsonText2 = JsonSerializer.Serialize(task2, Globals.JsonOptionsS);

        if (jsonText == jsonText2)
            Console.WriteLine("Task roundtrip serialization success!");
        else
            Console.WriteLine("Task roundtrip serialization failure!\n" + jsonText2);

        Vector2 v1 = new Vector2(2f, 3f);
        jsonText = JsonSerializer.Serialize(v1, Globals.JsonOptionsS);
        Console.WriteLine(jsonText + "\n");
        Vector2 v2 = JsonSerializer.Deserialize<Vector2>(jsonText, Globals.JsonOptionsD);
        if (v1 == v2)
            Console.WriteLine("Vector2 round trip success!");
        else
            Console.WriteLine("Vector2 round trip failure!");
    }
}

// Json root class
public class MapExample
{
    public int x { get; set; }
    public List<TileExample> tiles { get; set; }
    public int y;
    public PersonExample p1 { get; set; }

    // Constructors must be parameterless
    public MapExample()
    {
        // This causes an error, references cannot be preserved if constructors take params
        // this.x = x;
        x = 0;
        tiles = new();
        y = 4;
    }

    public override string ToString()
    {
        string txt = $"MapExample: x = {x} y = {y}\np1 = {p1}\ntiles = [\n";
        foreach (TileExample tile in tiles)
            txt += tile.ToString() + "\n";
        return txt + "]";
    }
}

public class TileExample
{
    public string Type { get; set; }
    public TileExample Neighbor { get; set; }

    public override string ToString()
    {
        return $"TileExample: Type = {Type} Neighbor = [{Neighbor}]";
    }
}

public class PersonExample
{
    // Person references the a tile in the MapExample's tile list
    public TileExample Home { get; set; }

    public override string ToString()
    {
        return $"PersonExample: Home = [{Home}]";
    }
}