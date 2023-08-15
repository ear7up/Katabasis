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
    public static bool DEBUG = false;
    public const int HIGH_PRIORITY = 1;
    public const int MEDIUM_PRIORITY = 2;
    public const int LOW_PRIORITY = 3;
    public const int DEFAULT_PRIORITY = 3;

    public string Description;
    public List<SkillLevel> skillsNeeded;
    public Queue<Task> subTasks;
    public Vector2 location;
    public TaskStatus Status;
    public bool Initialized;
    public Action<Object> OnSuccess;
    public Action<Object> OnFailure;
    
    public Task(string description, Action<Object> onComplete = null, Action<Object> onFailure = null)
    {
        skillsNeeded = new();
        subTasks = new();
        location = new();
        Status = new(this);
        Initialized = false;

        Description = description;
        OnSuccess = onComplete;
        OnFailure = onFailure;
    }

    public static Task Peek(PriorityQueue2<Task, int> tasks)
    {
        if (tasks.Empty())
            return null;
        return tasks.Peek();
    }

    public static Task Peek(Queue<Task> tasks)
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
        List<int> goodsIds = GoodsProduction.GetGoodsMadeUsingSkill(s);
        int index = Globals.Rand.Next(goodsIds.Count);
        int id = goodsIds[index];
        Goods goods = Goods.FromId(id);
        goods.Quantity = GoodsInfo.GetDefaultProductionQuantity(goods);
        return new TryToProduceTask(goods);
    }

    public virtual string Describe(string extra = "")
    {
        string s = $"<{base.ToString()}> {Description} [{extra}]";
        foreach (Task subtask in subTasks)
            s += "\n" + subtask.Describe();
        return s;
    }
}

public class FindNewHomeTask : Task
{
    public FindNewHomeTask() : base("Searching for a new home")
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

    public IdleAtHomeTask() : base("Going for a walk")
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
    public FindTileByTypeTask(TileType tileType) : base("Searching for a " + Globals.Title(tileType.ToString()))
    {
        TileType = tileType;
    }

    public override TaskStatus Execute(Person p)
    {   
        Tile found = (Tile)Tile.Find(p.Home, new TileFilter(TileType));
        if (found == null)
            Debug($"  {TileType} tile not found near {p.Position}");
        Status.Complete = true;
        Status.Failed = (found == null);
        Status.ReturnValue = found;
        return Status;
    }

    public override string Describe(string extra = "")
    {
        string s = base.Describe(TileType.ToString());
        return s;
    }
}

// Tries to find goods and add to the person's invenctory
public class SourceGoodsTask : Task
{
    public Goods Goods;
    public Goods GoodsRequest;
    public float QuantityRequired;
    public float QuantityAcquired;
    public Building MarketBuilding;
    public bool FindMarket;
    public SourceGoodsTask(Goods goods) : base("Trying to find " + goods.ToString())
    {
        Goods = goods;
        GoodsRequest = new Goods(goods);
        QuantityRequired = Goods.Quantity;
        QuantityAcquired = 0;
        MarketBuilding = null;
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

        GoodsRequest.Quantity = QuantityRequired - QuantityAcquired;

        // If it exists at the house, take it immediately, then head there
        // taking goods instantly prevents another member of the household from taking it
        if (GoodsRequest.Quantity > 0 && p.House != null && p.House.Stockpile.Has(GoodsRequest))
        {
            p.House.Stockpile.Take(GoodsRequest);
            QuantityAcquired = QuantityRequired;
            subTasks.Enqueue(new GoToTask("Going home to pick up an item", p.House.Sprite.Position));
        }

        // Keep requesting more from the stockpile
        if (QuantityRequired > QuantityAcquired)
        {
            p.PersonalStockpile.Take(GoodsRequest);
            QuantityAcquired += GoodsRequest.Quantity;

            // Hack for items where quantity ~= durability
            // we'll interptret hammer quantity = 4 -> at least 4 uses of a hammer
            float useRate = GoodsInfo.GetUseRate(Goods);
            if (useRate > 0 && QuantityAcquired > useRate * QuantityRequired)
                QuantityRequired = QuantityAcquired;
        }

        // Try to find a market once
        if (FindMarket)
        {
            Building market = (Building)Tile.Find(p.Home, new TileFilter(TileType.NONE, BuildingType.MARKET));
            if (market != null)
                MarketBuilding = market;
        }

        // Only try to buy goods from the market once
        if (FindMarket && MarketBuilding != null && QuantityRequired > QuantityAcquired)
        {
            GoodsRequest.Quantity = QuantityRequired - QuantityAcquired;
            float price = Market.CheckPrice(GoodsRequest.GetId()) * GoodsRequest.Quantity;
            if (price < p.Money)
            {
                // Go to the market, place this order, and come back
                MarketOrder order = new(p, true, GoodsRequest);
                subTasks.Enqueue(new BuyFromMarketTask(MarketBuilding.Sprite.Position, order));
                FindMarket = false;
                return Status;
            }
        }

        FindMarket = false;

        // See if we can produce it
        ProductionRequirements rule = (ProductionRequirements)GoodsProduction.Requirements[Goods.GetId()];
        if (rule != null && QuantityRequired > QuantityAcquired)
        {
            GoodsRequest.Quantity = QuantityRequired - QuantityAcquired;
            SkillLevel req = rule.SkillRequirement;
            // null indicates no skill requirement to check
            if (req == null || p.Skills[(int)req.skill].level >= req.level)
            {
                // Player can try to produce the good
                subTasks.Enqueue(new TryToProduceTask(GoodsRequest));
            }
        }

        // Task is complete when all goods are acquired or we've exhuasted all possible avenues
        if (subTasks.Count == 0)
        {
            GoodsRequest.Quantity = QuantityAcquired;
            Status.Complete = true;
            Status.Failed = (QuantityAcquired < QuantityRequired);
            Status.ReturnValue = GoodsRequest;
        }
        return Status;
    }

    public override string Describe(string extra = "")
    {
        string s = base.Describe(GoodsRequest.ToString());
        return s;
    }
}

public class FindBuildingTask : Task
{
    public BuildingType BuildingType;
    public BuildingSubType BuildingSubType;
    public TileType TileType;

    public FindBuildingTask(
        BuildingType buildingType, 
        BuildingSubType buildingSubType = BuildingSubType.NONE,
        TileType tileType = TileType.NONE) : base("Looking for a " + Globals.Title(buildingType.ToString()))
    {
        BuildingType = buildingType;
        BuildingSubType = buildingSubType;
        TileType = tileType;
    }

    public override TaskStatus Execute(Person p)
    {
        Building b = (Building)Tile.Find(p.Home, new TileFilter(TileType, BuildingType, BuildingSubType));
        Status.ReturnValue = b;
        Status.Complete = true;
        Status.Failed = (b == null);
        return Status;

        // Villagers cannot build automatically, but this is how they would
        // if (b == null)
        //     subTasks.Enqueue(new TryToBuildTask(BuildingType));
    }

    public override string Describe(string extra = "")
    {
        string s = base.Describe(BuildingType.ToString() + " " + 
            BuildingSubType.ToString() + " " + TileType.ToString());
        return s;
    }
}

public class TryToProduceTask : Task
{
    public ProductionRequirements Requirements;
    public Goods Goods;
    public int NumRequiredGoods;
    public List<Goods> GoodsRequirements;
    public List<Goods> AcquiredGoods;
    public Building Building;
    public Tile Tile;
    public Goods Tool;
    public float TimeToProduce;
    public float TimeSpent;

    public TryToProduceTask(Goods goods) : base("Trying to produce " + goods.ToString())
    {
        Goods = goods;
        Requirements = (ProductionRequirements)GoodsProduction.Requirements[goods.GetId()];
        Building = null;
        Tile = null;
        Tool = null;
        NumRequiredGoods = 0;
        GoodsRequirements = null;
        TimeToProduce = GoodsInfo.GetTime(goods) * goods.Quantity;

        TimeSpent = 0f;

        AcquiredGoods = new();
        if (Requirements.GoodsRequirement != null)
        {
            GoodsRequirements = Requirements.GoodsRequirement.ToList();
            if (Requirements.GoodsRequirement.And)
                NumRequiredGoods = GoodsRequirements.Count;
            else
                NumRequiredGoods = 1;
        }
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
                if (Building != null)
                {
                    p.BuildingUsing = null;
                    Building.StopUsing();
                }   
                return Status;
            }

            // Get sourced requirements from subtask
            if (subStatus.Task is FindBuildingTask)
            {
                Building = (Building)subStatus.ReturnValue;
                Building.StartUsing();
                p.BuildingUsing = Building;
                Tile = Building.Location;
                subTasks.Enqueue(new GoToTask(
                    "Going to " + Globals.Title(Building.Type.ToString()), 
                    Building.Sprite.Position));
            }
            else if (subStatus.Task is SourceGoodsTask)
            {
                Goods goods = (Goods)subStatus.ReturnValue;
                if (goods.Type == GoodsType.TOOL)
                {
                    // Add the tool quantity back, we're just borrowing it to use
                    // TODO: this is bugged, if the tool was sourced from the PersonalStockpile, this dupes it
                    Tool = goods;
                    p.PersonalStockpile.Add(Tool);
                }
                else if (goods.Quantity > 0)
                    AcquiredGoods.Add(goods);
            }
            else if (Tile == null && subStatus.Task is FindTileByTypeTask)
            {
                // Go to the tile
                Tile = (Tile)subStatus.ReturnValue;
                subTasks.Enqueue(new GoToTask(
                    "Going to " + Globals.Title(Tile.Type.ToString()), Tile.GetPosition()));
            }
        }

        BuildingType bReq = Requirements.BuildingRequirement;
        BuildingSubType bReq2 = Requirements.BuildingSubTypeRequirement;
        TileType tReq = Requirements.TileRequirement;

        // Queue up subtasks to find all the necessary prerequisites to produce the good
        if (Requirements.ToolRequirement != Goods.Tool.NONE && Tool == null)
            subTasks.Enqueue(new SourceGoodsTask(new Goods(GoodsType.TOOL, (int)Requirements.ToolRequirement, 1)));
        else if (bReq != BuildingType.NONE && Building == null)
            subTasks.Enqueue(new FindBuildingTask(bReq, bReq2, tReq));
        else if (tReq != TileType.NONE && Tile == null)
            subTasks.Enqueue(new FindTileByTypeTask(tReq));
        else if (GoodsRequirements != null && AcquiredGoods.Count < NumRequiredGoods)
        {
            // If it's an 'AND' requirement, source all of the goods
            if (Requirements.GoodsRequirement.And)
            {
                foreach (Goods g in GoodsRequirements)
                {
                    Goods req = new Goods(g);
                    req.Quantity = Goods.Quantity;
                    subTasks.Enqueue(new SourceGoodsTask(req));
                }
            }
            // Otherwise, just pick one good to source
            else
            {
                Goods req = new Goods(GoodsRequirements[Globals.Rand.Next(GoodsRequirements.Count)]);
                req.Quantity = Goods.Quantity;
                subTasks.Enqueue(new SourceGoodsTask(req));
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
                //p.PersonalStockpile.Add(Tool);
            }

            float productionTime = TimeToProduce;

            // Reduce time to produce by 0.5% per skill level
            if (Requirements.SkillRequirement != null)
            {
                int skill = (int)Requirements.SkillRequirement.skill;
                productionTime *= (200 - p.Skills[skill].level) / 200f;
            }

            if (TimeSpent >= productionTime)
            {
                Status.ReturnValue = Goods;
                Status.Complete = true;

                // Modify harvest yield by soil quality (better near rivers)
                if (Goods.Type == GoodsType.FOOD_PLANT && Tile != null)
                    Goods.Quantity *= Tile.SoilQuality;

                // If the task required a skill, give it a chance to increase the skill by 1
                if (Requirements.SkillRequirement != null)
                {
                    float r = Globals.Rand.NextFloat(0f, 1f);
                    int skill = (int)Requirements.SkillRequirement.skill;
                    float experience = GoodsInfo.GetExperience(Goods) * Goods.Quantity;

                    // E.g. if making 20 units of 1xp goods, chance is 10% + 20% = 30% to gain a level
                    if (r < SkillLevel.INCREASE_CHANCE + experience / 100)
                        p.Skills[skill].level++;
                }

                float q = Goods.Quantity;
                p.PersonalStockpile.Add(Goods);
                Goods.Quantity = q;
                if (Building != null)
                    Building.StopUsing();
                Task.Debug($"Successfully produced {Goods}");
            }
        }
        return Status;
    }

    public override string Describe(string extra = "")
    {
        string s = base.Describe(Goods.ToString());
        return s;
    }
}

public class BuyFromMarketTask : Task
{
    public BuyFromMarketTask(Vector2 marketPosition, MarketOrder order)
        : base("")
    {
        subTasks.Enqueue(new GoToTask("Going to the market", marketPosition));
        subTasks.Enqueue(new BuyTask(order));
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
    public MarketOrder Order;
    public BuyTask(MarketOrder order) : base("Buying " + order.goods.ToString())
    {
        Order = order;
    }
    public override TaskStatus Execute(Person p)
    {
        Status.Complete = true;
        if (Market.AttemptTransact(Order))
            Status.ReturnValue = Order.goods;
        return Status;
    }
}

public class SellTask : Task
{
    public List<Goods> Goods;
    public SellTask(List<Goods> goods) : base("Selling ")
    {
        foreach (Goods g in goods)
            Description += g.ToString() + ", ";
        Goods = goods;
    }
    public override TaskStatus Execute(Person p)
    {
        foreach (Goods g in Goods)
            Market.PlaceSellOrder(new MarketOrder(p, false, g));
        Status.Complete = true;
        return Status;
    }
}

public class GoToTask : Task
{
    public Vector2 destination;
    public Vector2 direction;
    public GoToTask(string description, Vector2 position) : base(description)
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

    public override string Describe(string extra = "")
    {
        string s = base.Describe(Description + " " + destination.ToString());
        return s;
    }
}

public class SellAtMarketTask : Task
{
    public SellAtMarketTask() : base("Selling goods at the market")
    {
      
    }

    public override TaskStatus Execute(Person p)
    {
        if (!Initialized)
            Init(p);

        // Try to complete subtasks first
        TaskStatus subStatus = base.Execute(p);
        if (subStatus != null && !subStatus.Complete)
            return Status;

        // But only when all subtasks are done
        if (subTasks.Count == 0)
            Status.Complete = true;

        return Status;
    }

    public void Init(Person p)
    {
        // Figure out what to take to market and sell
        Building market = (Building)Tile.Find(p.Home, 
            new TileFilter(buildingType: BuildingType.MARKET));

        if (market != null)
        {    
            List<Goods> toSell = p.FigureOutWhatToSell();
            if (toSell.Count > 0)
            {
                subTasks.Enqueue(new GoToTask("Going to the market", market.Sprite.Position));
                subTasks.Enqueue(new SellTask(toSell));
            }
        }
        Initialized = true;
    }
}

// Eat any food  you're holding until you're no longer hungry
public class EatTask : Task
{
    public EatTask() : base("Eating food")
    {

    }

    public override TaskStatus Execute(Person p)
    {
        foreach (Goods g in p.PersonalStockpile.Values())
        {
            if (p.Hunger <= 0)
                break;

            if (g.IsEdible())
            {
                while (p.Hunger > 0 && g.Quantity > 0)
                {
                    g.Use();
                    p.Hunger = Math.Max(0, p.Hunger - GoodsInfo.GetSatiation(g));
                }
            }
        }
        Status.Complete = true;
        return Status;
    }
}

public class TryToBuildTask : Task
{
    public Goods Tool;
    public Tile DestTile;
    public float TimeSpent;
    public bool ToolBorrowed;
    public BuildingType BuildingType;

    public TryToBuildTask(BuildingType buildingType) : base("Trying to build")
    {
        Tool = null;
        DestTile = null;
        TimeSpent = 0f;
        ToolBorrowed = false;
        BuildingType = buildingType;
        ProductionRequirements reqs = (ProductionRequirements)BuildingProduction.Requirements[BuildingType];

        if (reqs.ToolRequirement != Goods.Tool.NONE)
            subTasks.Enqueue(new SourceGoodsTask(new Goods(GoodsType.TOOL, (int)reqs.ToolRequirement)));

        if (reqs.GoodsRequirement != null)
            foreach (Goods g in reqs.GoodsRequirement.ToList())
                subTasks.Enqueue(new SourceGoodsTask(g));

        // TODO: additional constraint Tile.MAX_BUILDINGS
        if (reqs.TileRequirement != TileType.NONE)
            subTasks.Enqueue(new FindTileByTypeTask(reqs.TileRequirement));
    }

    public override TaskStatus Execute(Person p)
    {
        // Wait for subtasks to complete, use their return value
        TaskStatus subStatus = base.Execute(p);
        if (subStatus == null)
        {
            subStatus = null;
        }
        else if (subStatus.Failed || !subStatus.Complete)
        {
            return subStatus;
        }
        else if (subStatus.Task is SourceGoodsTask)
        {
            Goods g = (Goods)subStatus.ReturnValue;
            if (g.Type == GoodsType.TOOL)
            {
                Tool = g;
                //ToolBorrowed = ((SourceGoodsTask)subStatus.Task).FromInventory;
            }
        }
        else if (subStatus.Task is FindTileByTypeTask)
        {
            DestTile = (Tile)subStatus.ReturnValue;
            subTasks.Enqueue(new GoToTask("Going to build site", DestTile.GetPosition()));
        }

        // All queued tasks complete, add build time
        if (subTasks.Count == 0)
        {
            // When enough time has passed, finish the building,
            // return the tool if borrowed, and add the building to the tile
            TimeSpent += Globals.Time;
            if (TimeSpent >= BuildingInfo.GetBuildTime(BuildingType))
            {
                // TODO; Why is this failing to get a tool? Not being produced
                //Tool.Use();
                //if (ToolBorrowed)
                //    p.PersonalStockpile.Add(Tool);
                
                // No tile type requirement - place it at home tile
                if (DestTile == null)
                    DestTile = p.Home;

                Building.CreateBuilding(DestTile, BuildingType);
                Status.Complete = true;
            }
        }

        return Status;
    }

    public override string Describe(string extra = "")
    {
        string s = base.Describe(BuildingType.ToString());
        return s;
    }
}

public class DepositInventoryTask : Task
{
    public DepositInventoryTask() : base("Depositing inventory at home")
    {

    }

    public override TaskStatus Execute(Person p)
    {
        if (p.House == null)
        {
            Status.Complete = true;
            Status.Failed = true;
            return Status;
        }
        p.PersonalStockpile.DepositIntoExcludingFoodAndTools(p.House.Stockpile);
        Status.Complete = true;
        return Status;
    }
}

public class CookTask : Task
{
    public float TimeToProduce;
    public float TimeSpent;
    public Queue<Goods> ToCook;

    public CookTask() : base("Cooking food")
    {
        TimeSpent = 0f;
        TimeToProduce = 0f;
        ToCook = new();
    }

    public override TaskStatus Execute(Person p)
    {
        if (!Initialized)
        {
            Init(p);
            Initialized = true;
            return Status;
        }

        if (ToCook.Count > 0)
        {
            Goods current = ToCook.Peek();
            TimeToProduce = GoodsInfo.GetTime(current) * current.Quantity;
            
            TimeSpent += Globals.Time;
            if (TimeSpent >= TimeToProduce)
            {
                ToCook.Dequeue();
                current.Cook();
                p.PersonalStockpile.Add(current);
                TimeSpent = 0f;
            }
        }

        if (ToCook.Count == 0)
            Status.Complete = true;
        return Status;
    }

    public void Init(Person p)
    {
        if (p.House == null)
        {
            Status.Complete = true;
            Status.Failed = true;
            return;
        }

        // Try to cook as much as you want to eat, or two days worth if not very hungry
        float current = 0;
        float limit = Math.Max(p.Hunger, Person.DAILY_HUNGER * 2f);
        foreach (Goods g in p.House.Stockpile)
        {
            if (g.IsCookable())
            {
                // Try to take as much as needed to reach limit
                Goods cooked = new Goods(g);
                cooked.Cook();
                int satiation = GoodsInfo.GetSatiation(cooked);

                Goods req = new Goods(g);
                req.Quantity = (limit - current) / satiation;
                p.House.Stockpile.Take(req);
                current += req.Quantity * satiation;

                if (req.Quantity > 0f)
                    ToCook.Enqueue(req);
            }

            if (current >= limit)
                break;
        }
    }

    public override string Describe(string extra = "")
    {
        string food = "planning";
        if (ToCook.Count > 0)
            food = ToCook.Peek().ToString();
        return base.Describe(food);
    }
}