using System;

public class TasksTest
{
    public static void RunTests(Map map)
    {
        // Create the test person
        Person p1 = Person.CreatePerson(map.Origin, map.GetOriginTile());

        // Add some goods clay to the person's stockpile
        Goods clay = new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY, 100);
        p1.PersonalStockpile.Add(clay);

        // Make the next tile a river (clay can actually be found in any desert tile)
        map.tiles[1].Type = TileType.RIVER;

        // Set the first tile as the person's home
        p1.Home = map.tiles[0];

        // Try to make 20 clay bricks...
        TryToProduceTask task = new();
        task.SetAttributes(new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 20));
        p1.AssignPriorityTask(task, 2);

        for (int i = 0; i < 10; i++)
        {
            p1.Update();
            Globals.Time += 5;
        }

        Console.WriteLine("\nP1 stockpile:");
        foreach (Goods g in p1.PersonalStockpile.Values())
        {
            Console.WriteLine("  " + g);
        }

        // Success! P1 converted 20 clay from his personal stockpile to 20 bricks
        //     P1 stockpile:
        //     Goods(type=CRAFT_GOODS, subType=BRICKS, quantity=20)
        //     Goods(type=MATERIAL_NATURAL, subType=CLAY, quantity=80)
    }
}