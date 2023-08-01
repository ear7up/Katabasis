using System;
using System.Collections.Generic;
using System.Linq;

public class Task
{
    // Move: go to a location
    // Go Home: Move to Home (daily)
    // Haul: Move to pos A, Take goods, Move to point B
    // Cook: Go Home, 
    // Buy: Move to a market, find seller, Haul goods Home
    // Sell: Haul goods to a market, find buyer, otherwise Haul goods Home
    // RequestGoods: add Haul task to hauling work pool
    // Work:
    //      Move to Work location
    //      If neeeded Goods are present
    //          consume goods, produce goods
    public const int DEFAULT_PRIORITY = 5;

    public List<SkillLevel> skillsNeeded;
    public Queue<Task> subTasks;
    public Vector2 location;
    public int currentSubtask;
    // completion criteria ?

    public Task()
    {
        skillsNeeded = new();
        subTasks = new();
        location = new();
        currentSubtask = 0;
    }

    public virtual bool Execute(Person p)
    {
        Task subTask = subTasks.Peek();
        if (subTask != null)
        {
            bool complete = subTask.Execute(p);
            if (complete)
            {
                subTasks.Dequeue();
            }
            return subTasks.Count == 0;
        }

        // Return true when the task is completed
        return true;
    }
}

public class FindNewHomeTask : Task
{
    public FindNewHomeTask() : base()
    {

    }

    public override bool Execute(Person p)
    {   
        Tile oldHome = p.Home;
        Tile newHome = (Tile)Tile.Find(oldHome, new TileFilterHome());

        if (newHome == null)
        {
            Console.WriteLine($"Failed to find a home for {p}");
            return false;
        }

        p.Home = newHome;
        newHome.Population += 1;
        oldHome.Population -= 1;
        return true;
    }
}

public class IdleAtHomeTask : Task
{
    private Vector2 destination;
    private Vector2 direction;

    public IdleAtHomeTask() : base()
    {
        destination = Vector2.Zero;
        direction = Vector2.Zero;
    }

    public override bool Execute(Person p)
    {   
        Vector2 homeAbsOrigin = p.Home.GetPosition() + p.Home.GetOrigin();

        if (destination == Vector2.Zero)
        {
            destination = homeAbsOrigin;
            direction = new Vector2(destination.X - p.Position.X, destination.Y - p.Position.Y);
            direction.Normalize();
        }

        // Move in the direction of the destination at default movespeed scaled by time elapsed
        p.Position +=  direction * (Person.MOVE_SPEED * Globals.Time);

        // If we're within 1/8th of the tile's height from the destination, 
        // choose a new one as the home tile's origin plus or minus half the width/height
        float distanceFromHome = Vector2.Distance(p.Position, destination);

        if (distanceFromHome < Map.TileSize.Y / 8.0f)
        {
            destination = new Vector2(
                homeAbsOrigin.X + Globals.Rand.NextFloat(-Map.TileSize.X / 2f, Map.TileSize.X / 2f),
                homeAbsOrigin.Y + Globals.Rand.NextFloat(-Map.TileSize.Y / 2f, Map.TileSize.Y / 2f));
            direction = new Vector2(destination.X - p.Position.X, destination.Y - p.Position.Y);
            direction.Normalize();
        }

        // This task never ends?
        return false;
    }
}

public class SourceGoodsTask : Task
{
    public Goods Goods;
    public int QuantityRequired;
    public int QuantityAcquired;
    public SourceGoodsTask(Goods goods) : base()
    {
        Goods = goods;
        QuantityRequired = Goods.Quantity;
        QuantityAcquired = 0;
    }

    public override bool Execute(Person p)
    {   
        // Try to complete subtasks first
        if (!base.Execute(p))
            return false;

        // Keep requesting more from the stockpile
        Goods.Quantity = QuantityRequired - QuantityAcquired;
        p.Home.TakeFromStockpile(Goods);
        QuantityAcquired += Goods.Quantity;

        if (QuantityAcquired >= QuantityRequired)
        {
            Goods.Quantity = QuantityAcquired;
            return true;
        }

        // See if we can buy it
        Market market = (Market)Tile.Find(p.Home, new TileFilterMarket());

        if (market != null)
        {
            float price = market.CheckPrice(Goods);
            if (price < p.Money)
            {
                // TODO: Go to the market and buy goods
                //subTasks.Enqueue(new BuyFromMarketTask());
            }
        }

        // TODO: See if we can produce it

        return false;
    }
}