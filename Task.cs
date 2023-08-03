using System;
using System.Collections.Generic;
using System.Linq;

public class TaskStatus
{
    public bool Complete;
    public bool Failed;
    public Object ReturnValue;
    public Task Task;
    public TaskStatus(Task task)
    {
        Complete = false;
        ReturnValue = null;
        Task = task;
        Failed = false;
    }

    public bool NothingToDo()
    {
        return Task == null;
    }
}

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
    public const bool DEBUG = false;
    public const int HIGH_PRIORITY = 1;
    public const int MEDIUM_PRIORITY = 2;
    public const int LOW_PRIORITY = 3;
    public const int DEFAULT_PRIORITY = 3;

    public List<SkillLevel> skillsNeeded;
    public Queue<Task> subTasks;
    public Vector2 location;
    public int currentSubtask;
    public TaskStatus Status;
    
    // completion criteria ?

    public Task()
    {
        skillsNeeded = new();
        subTasks = new();
        location = new();
        currentSubtask = 0;
        Status = new(this);
    }

    public Task Peek(Queue<Task> tasks)
    {
        if (tasks.Count == 0)
            return null;
        return tasks.Peek();
    }

    public static void Debug(String s)
    {
        if (Task.DEBUG)
            Console.WriteLine(s);
    }

    public virtual TaskStatus Execute(Person p)
    {
        Task subTask = Peek(subTasks);
        if (subTask != null)
        {
            TaskStatus subStatus = subTask.Execute(p);
            if (subStatus != null)
            {
                if (subStatus.Complete)
                {
                    if (!subStatus.Failed)
                        Debug($"  Subtask complete {subStatus.Task} returning {subStatus.ReturnValue}");
                    subTasks.Dequeue();
                }
                
                subStatus.Complete = (subTasks.Count == 0);
                
                // Make the whole task fail when a subtask fails
                if (subStatus.Failed)
                {
                    Status.Failed = true;
                    Status.Complete = true;
                }
            }
            return subStatus;
        }
        
        return null;
    }

    // Pick a random task a person swith skill level `s` can perform
    public static Task RandomUsingSkill(SkillLevel s)
    {
        // TODO: Are there non-production tasks that use skills?

        // Production tasks (e.g. use farming at a farm building to make food or animal products)
        List<int> goodsIds = GoodsProduction.GetGoodsMadeUsingSkill(s.skill);
        int index = Globals.Rand.Next(goodsIds.Count);
        int id = goodsIds[index];
        Goods goods = Goods.FromId(id);
        goods.Quantity = GoodsInfo.GetDefaultProductionQuantity(goods);
        return new TryToProduceTask(goods);
    }
}

public class FindNewHomeTask : Task
{
    public FindNewHomeTask() : base()
    {

    }

    public override TaskStatus Execute(Person p)
    {   
        Tile oldHome = p.Home;
        Tile newHome = (Tile)Tile.Find(oldHome, new TileFilterHome());

        Status.Complete = true;
        Status.ReturnValue = newHome;

        if (newHome == null)
        {
            Debug($"Failed to find a home for {p}");
            Status.Failed = true;
            return Status;
        }

        p.Home = newHome;
        newHome.Population += 1;
        oldHome.Population -= 1;
        return Status;
    }
}

public class IdleAtHomeTask : Task
{
    private Vector2 destination;
    private Vector2 direction;
    private float Duration;

    public IdleAtHomeTask() : base()
    {
        destination = Vector2.Zero;
        direction = Vector2.Zero;
        Duration = 30;
    }

    public override TaskStatus Execute(Person p)
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

        Duration -= Globals.Time;
        if (Duration <= 0)
        {
            Status.Complete = true;
        }

        return Status;
    }
}

public class FindTileByTypeTask : Task
{
    public TileType TileType;
    public FindTileByTypeTask(TileType tileType) : base()
    {
        TileType = tileType;
    }

    public override TaskStatus Execute(Person p)
    {   
        Tile found = (Tile)Tile.Find(p.Home, new TileFilterByType(TileType));
        if (found == null)
            Debug($"  {TileType} tile not found near {p.Position}");
        Status.Complete = true;
        Status.Failed = (found == null);
        Status.ReturnValue = found;
        return Status;
    }
}

public class SourceGoodsTask : Task
{
    public Goods Goods;
    public float QuantityRequired;
    public float QuantityAcquired;
    public Market Market;
    public bool FindMarket;
    public SourceGoodsTask(Goods goods) : base()
    {
        Goods = goods;
        QuantityRequired = Goods.Quantity;
        QuantityAcquired = 0;
        Market = null;
        FindMarket = true;
    }

    public override TaskStatus Execute(Person p)
    {   
        // Try to complete subtasks first
        TaskStatus subStatus = base.Execute(p);
        if (subStatus != null)
        {
            if (!subStatus.Complete)
            {
                return Status;
            }
            else if (subStatus.ReturnValue != null && subStatus.ReturnValue is Goods)
            {
                // Add the acquired goods from the subtask to our total
                Goods g = (Goods)subStatus.ReturnValue;
                QuantityAcquired += g.Quantity;
            }
        }

        // Keep requesting more from the stockpile
        if (QuantityRequired > QuantityAcquired)
        {
            Goods.Quantity = QuantityRequired - QuantityAcquired;
            p.PersonalStockpile.Take(Goods);
            QuantityAcquired += Goods.Quantity;
        }

        // Try to find a market once
        if (FindMarket)
        {
            Market = (Market)Tile.Find(p.Home, new TileFilterBuliding(BuildingType.MARKET));
            FindMarket = false;
        }

        // Try to buy goods if still needed
        if (Market != null && QuantityRequired > QuantityAcquired)
        {
            Goods.Quantity = QuantityRequired - QuantityAcquired;
            float price = Market.CheckPrice(Goods);
            if (price < p.Money)
            {
                MarketOrder order = new(p, true, Goods, price / Goods.Quantity);
                subTasks.Enqueue(new BuyFromMarketTask(Market.Sprite.Position, Market, order));
                return Status;
            }
        }

        // See if we can produce it
        ProductionRequirements rule = (ProductionRequirements)GoodsProduction.Requirements[Goods.GetId()];
        if (rule != null && QuantityRequired > QuantityAcquired)
        {
            Goods.Quantity = QuantityRequired - QuantityAcquired;
            SkillLevel req = rule.SkillRequirement;
            // null indicates no skill requirement to check
            if (req == null || p.Skills[(int)req.skill].level >= req.level)
            {
                // Player can try to produce the good
                subTasks.Enqueue(new TryToProduceTask(Goods));
            }
        }

        // Task is complete when all goods are acquired or we've exhuasted all possible avenues
        if (QuantityAcquired >= QuantityRequired || subTasks.Count == 0)
        {
            Goods.Quantity = QuantityAcquired;
            Status.Complete = true;
            Status.Failed = (QuantityAcquired < QuantityRequired);
            Status.ReturnValue = Goods;
        }

        return Status;
    }
}

public class FindBuildingTask : Task
{
    public BuildingType BuildingType;
    public FindBuildingTask(BuildingType buildingType)
    {
        BuildingType = buildingType;
    }
    public override TaskStatus Execute(Person p)
    {
        Building b = (Building)Tile.Find(p.Home, new TileFilterBuliding(BuildingType));
        Status.ReturnValue = b;
        Status.Complete = true;
        Status.Failed = (b == null);
        return Status;
    }
}

public class TryToProduceTask : Task
{
    public ProductionRequirements Requirements;
    public Goods Goods;
    public List<Goods> GoodsRequirements;
    public List<Goods> AcquiredGoods;
    public Building Building;
    public Tile Tile;
    public Goods Tool;
    public float TimeToProduce;
    public float TimeSpent;

    public TryToProduceTask(Goods goods)
    {
        Goods = goods;
        Requirements = (ProductionRequirements)GoodsProduction.Requirements[goods.GetId()];
        Building = null;
        Tile = null;
        Tool = null;
        GoodsRequirements = null;
        TimeToProduce = GoodsInfo.GetTime(goods) * goods.Quantity;
        TimeSpent = 0f;
        AcquiredGoods = new();
        if (Requirements.GoodsRequirement != null)
            GoodsRequirements = Requirements.GoodsRequirement.ToList();
    }

    public override TaskStatus Execute(Person p)
    {
        // Try to complete subtasks first
        TaskStatus subStatus = base.Execute(p);

        if (subStatus != null)
        {
            if (!subStatus.Complete)
                return subStatus;

            if (subStatus.Failed)
            {
                Status.Complete = true;
                Status.Failed = true;
                return Status;
            }

            // Get sourced requirements from subtask
            if (subStatus.Task is FindBuildingTask)
            {
                Building = (Building)subStatus.ReturnValue;
                subTasks.Enqueue(new GoToTask(Building.Sprite.Position));
            }
            else if (subStatus.Task is SourceGoodsTask)
            {
                Goods goods = (Goods)subStatus.ReturnValue;
                if (goods.Type == GoodsType.TOOL)
                    Tool = goods;
                else
                    AcquiredGoods.Add(goods);
            }
            else if (subStatus.Task is FindTileByTypeTask)
            {
                // Go to the tile
                Tile = (Tile)subStatus.ReturnValue;
                subTasks.Enqueue(new GoToTask(Tile.GetPosition()));
            }
        }

        // Queue up subtasks to find all the necessary prerequisites to produce the good
        if (Requirements.BuildingRequirement != BuildingType.NONE && Building == null)
            subTasks.Enqueue(new FindBuildingTask(Requirements.BuildingRequirement));
        else if (Requirements.TileRequirement != TileType.NONE && Tile == null)
            subTasks.Enqueue(new FindTileByTypeTask(Requirements.TileRequirement));
        else if (Requirements.ToolRequirement != Goods.Tool.NONE && Tool == null)
            subTasks.Enqueue(new SourceGoodsTask(new Goods(GoodsType.TOOL, (int)Requirements.ToolRequirement, 1)));
        else if (Requirements.GoodsRequirement != null && AcquiredGoods.Count == 0)
        {
            foreach (Goods g in GoodsRequirements)
            {
                // TODO: 1-to-1 ratio assumed, e.g. 1 clay => 1 bricks
                g.Quantity = Goods.Quantity;
                subTasks.Enqueue(new SourceGoodsTask(g));
            }
        }
        else if (subTasks.Count == 0)
        {
            //Status.Complete = true;
            // TODO: possibly modify quantity produced

            // Update the progress on the goods production
            TimeSpent += Globals.Time;
 
            if (Tool != null)
            {
                Tool.Use();
            }

            if (TimeSpent >= TimeToProduce)
            {
                p.PersonalStockpile.Add(Goods);
                Status.ReturnValue = Goods;
                Status.Complete = true;
                Task.Debug($"Successfully produced {Goods}");
            }
        }

        return Status;
    }
}

public class BuyFromMarketTask : Task
{
    public BuyFromMarketTask(Vector2 startPosition, Market market, MarketOrder order) : base()
    {
        subTasks.Enqueue(new GoToTask(market.Sprite.Position));
        subTasks.Enqueue(new BuyTask(market, order));
        subTasks.Enqueue(new GoToTask(startPosition));
    }

    public override TaskStatus Execute(Person p)
    {
        // Try to complete subtasks first
        TaskStatus subStatus = base.Execute(p);
        if (subStatus != null && !subStatus.Complete)
            return Status;

        // Return the purchased goods from the subtask BuyTask
        if (subStatus != null && subStatus.ReturnValue is Goods)
            Status.ReturnValue = subStatus.ReturnValue;

        // But only when all subtasks are done
        if (subTasks.Count == 0)
            Status.Complete = true;

        return Status;
    }
}

// Attempts to buy as much of the order quantity as is available
public class BuyTask : Task
{
    public Market Market;
    public MarketOrder Order;
    public BuyTask(Market market, MarketOrder order) : base()
    {
        Market = market;
        Order = order;
    }
    public override TaskStatus Execute(Person p)
    {
        Market.AttemptTransact(Order);
        Status.Complete = true;
        Status.ReturnValue = Order.goods;
        return Status;
    }
}

public class GoToTask : Task
{
    public Vector2 destination;
    public Vector2 direction;
    public GoToTask(Vector2 position) : base()
    {
        destination = position;
        direction = Vector2.Zero;
    }
    public override TaskStatus Execute(Person p)
    {
        if (direction == Vector2.Zero)
        {
            direction = destination - p.Position;
            direction.Normalize();
            Debug($"  Moving to position {destination}");
        }

        // Move in the direction of the destination at default movespeed scaled by time elapsed
        p.Position += direction * (Person.MOVE_SPEED * Globals.Time);

        // If we're within 1/8th of the tile's height from the destination, 
        // choose a new one as the home tile's origin plus or minus half the width/height
        float distance = Vector2.Distance(p.Position, destination);

        if (distance < Map.TileSize.Y / 8.0f)
        {
            Status.Complete = true;
        }
        return Status;
    }
}

// Eat any food  you're holding until you're no longer hungry
public class EatTask : Task
{
    public override TaskStatus Execute(Person p)
    {
        foreach (Goods g in p.PersonalStockpile.Values())
        {
            if (p.Hunger <= 0)
                break;

            if (g.IsEdible())
            {
                g.Use();
                p.Hunger = Math.Max(0, p.Hunger - GoodsInfo.GetSatiation(g));
            }
        }
        Status.Complete = true;
        return Status;
    }
}