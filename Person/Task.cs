using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

public enum TaskDiscriminator
{
    Task = 0,
    FindNewHomeTask = 1,
    IdleAtHomeTask = 2,
    FindTileByTypeTask = 3,
    SourceGoodsTask = 4,
    FindBuildingTask = 5,
    TryToProduceTask = 6,
    BuyFromMarketTask = 7,
    BuyTask = 8,
    SellTask = 9,
    GoToTask = 10,
    SellAtMarketTask = 11,
    EatTask = 12,
    BuildTask = 13,
    DepositInventoryTask = 14,
    CookTask = 15,
    BuyFoodFromMarketTask = 16
};

public class TaskStatus
{
    public bool Complete { get; set; }
    public bool Failed { get; set; }
    public string FailureReason { get; set; }
    public Object ReturnValue { get; set; }
    public Task Task { get; set; }

    public TaskStatus()
    {
        Complete = false;
        ReturnValue = null;
        Failed = false;
        FailureReason = "";
    }

    public void SetAttributes(Task task)
    {
        Task = task;
    }

    public bool NothingToDo()
    {
        return Task == null;
    }
}

[JsonDerivedType(derivedType: typeof(BuyTask), typeDiscriminator: "BuyTask")]
[JsonDerivedType(derivedType: typeof(EatTask), typeDiscriminator: "EatTask")]
[JsonDerivedType(derivedType: typeof(GoToTask), typeDiscriminator: "GoToTask")]
[JsonDerivedType(derivedType: typeof(CookTask), typeDiscriminator: "CookTask")]
[JsonDerivedType(derivedType: typeof(SellTask), typeDiscriminator: "SellTask")]
[JsonDerivedType(derivedType: typeof(BuildTask), typeDiscriminator: "BuildTask")]
[JsonDerivedType(derivedType: typeof(HaulGoodsTask), typeDiscriminator: "HaulGoodsTask")]
[JsonDerivedType(derivedType: typeof(IdleAtHomeTask), typeDiscriminator: "IdleAtHomeTask")]
[JsonDerivedType(derivedType: typeof(FindNewHomeTask), typeDiscriminator: "FindNewHomeTask")]
[JsonDerivedType(derivedType: typeof(SourceGoodsTask), typeDiscriminator: "SourceGoodsTask")]
[JsonDerivedType(derivedType: typeof(TryToProduceTask), typeDiscriminator: "TryToProduceTask")]
[JsonDerivedType(derivedType: typeof(FindBuildingTask), typeDiscriminator: "FindBuildingTask")]
[JsonDerivedType(derivedType: typeof(SellAtMarketTask), typeDiscriminator: "SellAtMarketTask")]
[JsonDerivedType(derivedType: typeof(BuyFromMarketTask), typeDiscriminator: "BuyFromMarketTask")]
[JsonDerivedType(derivedType: typeof(FindTileByTypeTask), typeDiscriminator: "FindTileByTypeTask")]
[JsonDerivedType(derivedType: typeof(DepositInventoryTask), typeDiscriminator: "DepositInventoryTask")]
[JsonDerivedType(derivedType: typeof(BuyFoodFromMarketTask), typeDiscriminator: "BuyFoodFromMarketTask")]
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

    // Serialized content
    //[JsonPropertyOrder(-1)]
    public TaskDiscriminator Discriminator { get; set; }
    public string Description { get; set; }
    public List<SkillLevel> skillsNeeded { get; set; }
    public Queue<Task> subTasks { get; set; }
    public TaskStatus Status { get; set; }
    public bool Initialized { get; set; }

    // Not serializable, callers need to be prepared to requeue tasks after loading a save
    // if they depend on knowing whether a task succeeded or failed
    public Action<Object> OnSuccess;
    public Action<Object> OnFailure;

    public Task()
    {
        Discriminator = TaskDiscriminator.Task;
        skillsNeeded = new();
        subTasks = new();
        Status = new();
        Status.SetAttributes(this);
        Initialized = false;
    }

    public virtual void SetAttributes(
        string description, 
        Action<Object> onComplete = null, 
        Action<Object> onFailure = null)
    {
        Description = description;
        OnSuccess = onComplete;
        OnFailure = onFailure;
    }

    public static Task Peek(PriorityQueue2<Task, int> tasks)
    {
        if (tasks.Empty())
            return null;
        return (Task)tasks.Peek();
    }

    public static Task Peek(Queue<Task> tasks)
    {
        if (tasks.Count == 0)
            return null;
        return (Task)tasks.Peek();
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
                    Status.FailureReason = subStatus.FailureReason;
                    Status.Complete = true;
                }
            }
            return subStatus;
        }
        return null;
    }

    // Pick a random task a person swith skill level `s` can perform
    public static Task RandomUsingSkill(Person p, SkillLevel s)
    {
        // TODO: Are there non-production tasks that use skills?

        // Production tasks (e.g. use farming at a farm building to make food or animal products)
        List<int> goodsIds = GoodsProduction.GetGoodsMadeUsingSkill(s);
        if (goodsIds.Count == 0)
            goodsIds = GoodsProduction.GetGoodsMadeUsingSkill(new SkillLevel() { skill = Skill.NONE, level = 100 });

        // Remove all plants that aren't unlcoked from consideration
        goodsIds.RemoveAll(id => 
            (GoodsType)Goods.TypeFromId(id) == GoodsType.FOOD_PLANT &&
            !p.Owner.IsPlantUnlocked((Goods.FoodPlant)Goods.SubTypeFromid(id))
        );

        int index = Globals.Rand.Next(goodsIds.Count);
        int id = goodsIds[index];
        Goods goods = Goods.FromId(id);
        goods.Quantity = GoodsInfo.GetDefaultProductionQuantity(goods);

        TryToProduceTask task = new();
        task.SetAttributes(goods);
        return task;
    }

    public static Task MostProfitableUsingSkill(SkillLevel s)
    {
        int id = GoodsProduction.MostProfitableUsing(s);

        if (id < 0)
            return null;

        Goods goods = Goods.FromId(id);
        goods.Quantity = GoodsInfo.GetDefaultProductionQuantity(goods);

        TryToProduceTask task = new();
        task.SetAttributes(goods);
        return task;

    }

    public virtual string Describe(string extra = "", bool debug = true, string depth = "")
    {
        string s = $"{depth}<{base.ToString()}> {Description} [{extra}]\n";
        if (!debug)
            s = $"{depth}{Description} {extra}\n";

        if (s.Trim().Length == 0)
            s = s.Trim();

        foreach (Task subtask in subTasks)
        {
            string subtaskDescription = subtask.Describe("", debug, depth + "  ");
            if (subtaskDescription.Length > 0)
                s += subtaskDescription + "\n";
        }
        return s.TrimEnd();
    }

    public virtual void Init()
    {

    }

    public virtual bool Complete(Person p)
    {
        return true;
    }
}

public class FindNewHomeTask : Task
{
    public FindNewHomeTask()
    {
        Discriminator = TaskDiscriminator.FindNewHomeTask;
        SetAttributes("Searching for a new home");
    }

    public override TaskStatus Execute(Person p)
    {   
        Tile oldHome = p.Home;
        Tile newHome = (Tile)Tile.Find(oldHome, new TileFilterHome());

        Status.Complete = true;
        Status.ReturnValue = newHome;

        if (newHome == null)
        {
            Status.Failed = true;
            Status.FailureReason = "Can't find a house";
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
    // Serialized content
    public Vector2 destination { get; set; }
    public Vector2 direction { get; set; }
    public float Duration { get; set; }

    public IdleAtHomeTask()
    {
        Discriminator = TaskDiscriminator.IdleAtHomeTask;
        destination = Vector2.Zero;
        direction = Vector2.Zero;
        Duration = 30;
        SetAttributes("Going for a walk");
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
    // Serialized content
    public TileType TileType { get; set; }

    public FindTileByTypeTask()
    {
        Discriminator = TaskDiscriminator.FindTileByTypeTask;
    }

    public void SetAttributes(
        TileType tileType,
        Action<object> onComplete = null, 
        Action<object> onFailure = null)
    {
        base.SetAttributes("Searching for a " + Globals.Title(tileType.ToString()), onComplete, onFailure);
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
}

// Tries to find goods and add to the person's invenctory
public class SourceGoodsTask : Task
{
    // Serialized content
    public Goods GoodsRequest { get; set; }

    public SourceGoodsTask()
    {
        Discriminator = TaskDiscriminator.SourceGoodsTask;
    }

    public void SetAttributes(Goods goods)
    {
        base.SetAttributes("Trying to find " + goods.ToString());
        GoodsRequest = new(goods);
    }

    public override TaskStatus Execute(Person p)
    {   
        if (!Initialized)
            return Init(p);

        TaskStatus subStatus = base.Execute(p);

        if (subStatus != null && subStatus.Failed)
            return subStatus;

        if (subTasks.Count > 0)
            return Status;

        Status.Complete = Complete(p);
        return Status;
    }

    public TaskStatus Init(Person p)
    {
        Initialized = true;

        int id = GoodsRequest.GetId();
        
        // Goods already in the person's inventory (accept partial for tools)
        if (GoodsRequest.IsTool())
        {
            ToolMaterial material = p.PersonalStockpile.HasToolOrBetter(GoodsRequest);
            if (material != ToolMaterial.NONE)
            {
                Status.Complete = true;
                return Status;
            }
        }
        else if (p.PersonalStockpile.Has(GoodsRequest))
        {
            Status.Complete = true;
            return Status;
        }

        // TODO: support taking tool or better version from house or market

        // Remove goods from house and add to personal inventory
        if (p.House != null && p.House.Stockpile.Has(GoodsRequest))
        {
            // Take the goods immediately so that they aren't taken before the villager arrives
            p.House.Stockpile.Take(id, GoodsRequest.Quantity);
            p.PersonalStockpile.Add(id, GoodsRequest.Quantity);

            GoToTask go = new();
            go.SetAttributes("Going home to pick up item", p.House.Sprite.Position);
            subTasks.Enqueue(go);
            return Status;
        }

        // Try to buy the goods from the market
        Building market = (Building)Tile.Find(p.Home, new TileFilter(TileType.NONE, BuildingType.MARKET));
        if (market != null && Globals.Model.Market.AttemptTransact(MarketOrder.Create(p, true, new Goods(GoodsRequest))))
        {
            GoToTask go = new();
            go.SetAttributes("Buying from market", market.Sprite.Position);
            subTasks.Enqueue(go);
            return Status;
        }

        // Try to produce the goods
        ProductionRequirements rule = (ProductionRequirements)GoodsProduction.Requirements[id];
        if (rule == null)
        {
            Status.Failed = true;
            return Status;
        }

        // null indicates no skill requirement to check
        SkillLevel req = rule.SkillRequirement;
        if (req == null || p.Skills[(int)req.skill].level >= req.level)
        {
            TryToProduceTask task = new();
            task.SetAttributes(new Goods(GoodsRequest));
            subTasks.Enqueue(task);
        }
        else
        {
            Status.Failed = true;
        }

        return Status;
    }
}

public class FindBuildingTask : Task
{
    // Serialized content
    public BuildingType BuildingType { get; set; }
    public BuildingSubType BuildingSubType { get; set; }
    public TileType TileType { get; set; }

    public FindBuildingTask()
    {
        Discriminator = TaskDiscriminator.FindBuildingTask;
    }

    public void SetAttributes(
        BuildingType buildingType, 
        BuildingSubType buildingSubType = BuildingSubType.NONE,
        TileType tileType = TileType.NONE)
    {
        base.SetAttributes("Looking for a " + Globals.Title(buildingType.ToString()));
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
}

public class TryToProduceTask : Task
{
    // Serialized content
    public List<Goods> RequiredGoods { get; set; }
    public float TimeToProduce { get; set; }
    public Building ReqBuilding { get; set; }
    public Tile ReqTile { get; set; }
    public float TimeSpent { get; set; }

    private Goods _Goods;
    public Goods Goods { 
        get { return _Goods; }
        set { 
            _Goods = value;
            Requirements = (ProductionRequirements)GoodsProduction.Requirements[value.GetId()];
        }
    }

    public ProductionRequirements Requirements;

    public TryToProduceTask()
    {
        Discriminator = TaskDiscriminator.TryToProduceTask;
        
        RequiredGoods = null;
        TimeToProduce = 0f;
        ReqBuilding = null;
        TimeSpent = 0f;
        RequiredGoods = new();
    }

    public void SetAttributes(Goods goods)
    {
        base.SetAttributes("Trying to produce " + goods.ToString());

        Goods = new Goods(goods);
        Requirements = (ProductionRequirements)GoodsProduction.Requirements[goods.GetId()];
    }

    public override TaskStatus Execute(Person p)
    {
        if (!Initialized)
            return Init(p);

        TaskStatus subStatus = base.Execute(p);

        if (subStatus != null && subStatus.Failed)
            return subStatus;

        if (subTasks.Count > 0)
            return Status;

        // Mark the building as in-use
        if (TimeSpent == 0f && ReqBuilding != null)
            ReqBuilding.StartUsing(p);

        // Add progress
        TimeSpent += Globals.Time;
        if (TimeSpent >= TimeToProduce)
        {
            Status.Complete = Complete(p);
        }

        return Status;
    }

    // Logic to run when the task completes,
    // adds goods to the person's stockpile, always returns true
    public override bool Complete(Person p)
    {
        if (Requirements == null)
        {
            Console.Write("Failed to complete production task, requirements lookup null for " + Goods.ToString());
            return true;
        }

        // If the task required a skill, give it a chance to increase the skill by 1
        if (Requirements.SkillRequirement != null)
        {
            float r = Globals.Rand.NextFloat(0f, 1f);
            int skill = (int)Requirements.SkillRequirement.skill;
            float experience = GoodsInfo.GetExperience(Goods) * Goods.Quantity;
            p.GainExperience(skill, experience);
        }

        // Reduce the quantity of minerals by mining
        // Reduce soil quality by farming
        if (ReqBuilding != null)
        {
            if (ReqBuilding.Type == BuildingType.MINE)
                ReqBuilding.Location.TakeResource();
            else if (ReqBuilding.Type == BuildingType.FARM)
                ReqBuilding.Location.Farm();
        }

        // Reduce the number of trees when cutting wood (but not when farming honey)
        // Reduce the number of wild animals when hunting
        if (ReqTile != null)
        {
            if (ReqTile.Type == TileType.FOREST && Requirements.ToolRequirement.Tool == Goods.Tool.AXE)
                ReqTile.TakeResource();
            else if (ReqTile.Type == TileType.WILD_ANIMAL || ReqTile.Type == TileType.ELEPHANT)
                ReqTile.TakeResource();
        }

        // Remove the goods used to produce the item
        float minQty = Goods.Quantity;
        foreach (Goods g in RequiredGoods)
        {
            // HasSome because ingredients like raw meat may have spoiled slightly
            if (!p.PersonalStockpile.HasSome(g))
                Console.WriteLine("Missing requirement producing " + Goods.ToString());
            p.PersonalStockpile.Take(g.GetId(), g.Quantity);

            minQty = Math.Min(minQty, g.Quantity);
        }

        // If we lost a few ingredients, reduce the amount produced
        Goods.Quantity = minQty;

        // If a tool was used, decrement its durability
        if (Requirements.ToolRequirement != null)
            p.PersonalStockpile.UseTool(Requirements.ToolRequirement.Tool, Requirements.ToolRequirement.Material);

        // Finish by adding the completed goods to the person's stockpile
        p.PersonalStockpile.Add(Goods.GetId(), Goods.Quantity);
        if (ReqBuilding != null)
            ReqBuilding.StopUsing(p);
        return true;
    }

    public TaskStatus Init(Person p)
    {
        if (Requirements == null)
        {
            Console.Write("Requirements lookup failed for " + Goods.ToString());
            Status.Failed = true;
            return Status;
        }

        // Checks if the good is available, e.g. some crops must be unlocked
        if (!p.Owner.CanProduce(Goods))
        {
            Status.Failed = true;
            return Status;
        }

        Initialized = true;

        BuildingType bReq = Requirements.BuildingRequirement;
        BuildingSubType bReq2 = Requirements.BuildingSubTypeRequirement;
        TileType tReq = Requirements.TileRequirement;

        // Queue up subtasks to find all the necessary prerequisites to produce the good
        if (Requirements.ToolRequirement != null)
        {
            SourceGoodsTask task = new SourceGoodsTask();
            task.SetAttributes(new Goods(GoodsType.TOOL, (int)Requirements.ToolRequirement.Tool, 1, (int)Requirements.ToolRequirement.Material));
            subTasks.Enqueue(task);
        }

        // Fail if building or tile requirement cannot be satisfied
        TileFilter filter = new();
        filter.FindResource = Tile.CanHaveResource(tReq);

        if (tReq != TileType.NONE)
            filter.FilterTileType = tReq;
        if (bReq != BuildingType.NONE)
            filter.FilterBuildingType = bReq;
        if (bReq2 != BuildingSubType.NONE)
            filter.FilterBuildingSubType = bReq2;

        // Only search if there's a requirement to do so
        Object found = null;
        if (tReq != TileType.NONE || bReq != BuildingType.NONE)
        {
            found = Tile.Find(p.Home, filter);
            if (found == null)
            {
                Status.Failed = true;
                Status.FailureReason = "Failed to find " + filter.Describe();
                return Status;
            }
        }

        if (Requirements.GoodsRequirement != null)
        {
            // If it's an 'AND' requirement, source all of the goods
            if (Requirements.GoodsRequirement.And)
            {
                foreach (Goods g in Requirements.GoodsRequirement.Options.Values)
                {
                    Goods req = Goods.FromId(g.GetId(), Goods.Quantity);
                    SourceGoodsTask task = new SourceGoodsTask();
                    task.SetAttributes(req);
                    subTasks.Enqueue(task);
                    RequiredGoods.Add(new Goods(req));
                }
            }
            // Otherwise, just pick one good to source
            else
            {
                List<Goods> reqs = Requirements.GoodsRequirement.ToList();
                Goods req = new Goods(reqs[Globals.Rand.Next(reqs.Count)]);
                req.Quantity = Goods.Quantity;

                SourceGoodsTask task = new();
                task.SetAttributes(req);
                subTasks.Enqueue(task);
                RequiredGoods.Add(new Goods(req));
            }
        }
        
        // Queue up task to go to the required location after the task to source goods has completed
        if (found == null)
        {
            // No destination required
        }
        else if (found is Building)
        {
            Building b = (Building)found;
            ReqBuilding = b;

            GoToTask go = new();
            go.SetAttributes("Going to " + Globals.Title(b.Type.ToString()), b.Sprite.Position);
            subTasks.Enqueue(go);
        }
        else if (found is Tile)
        {
            Tile t = (Tile)found;
            ReqTile = t;

            Rectangle bounds = t.BaseSprite.GetBounds();

            // Randomize the destination slightly
            Vector2 destination = new Vector2(
                bounds.X + bounds.Width * Globals.Rand.NextFloat(0.2f, 0.8f),
                bounds.Y + bounds.Height * Globals.Rand.NextFloat(0.2f, 0.8f));

            GoToTask go = new();
            go.SetAttributes("Going to " + Globals.Title(t.Type.ToString()), destination);
            subTasks.Enqueue(go);
        }

        // Calculate how long it will take to produce the goods
        TimeToProduce = GoodsInfo.GetTime(Goods) * Goods.Quantity;

        // Reduce time to produce by 0.5% per skill level
        if (Requirements.SkillRequirement != null)
        {
            int skill = (int)Requirements.SkillRequirement.skill;
            //TimeToProduce *= (200 - p.Skills[skill].level) / 200f;
            TimeToProduce = GoodsProduction.CalculateTimeToProduce(Goods.GetId(), Goods.Quantity, p.Skills[skill].level);
        }

        // Modify harvest yield time by soil quality (better near rivers)
        if (found != null && found is Tile && Goods.Type == GoodsType.FOOD_PLANT)
            TimeToProduce /= ((Tile)found).SoilQuality;

        return Status;
    }
}

public class BuyFromMarketTask : Task
{
    public BuyFromMarketTask()
    {
        Discriminator = TaskDiscriminator.BuyFromMarketTask;
    }

    public void SetAttributes(Vector2 marketPosition, MarketOrder order)
    {
        base.SetAttributes("");

        GoToTask go = new();
        go.SetAttributes("Going to the market", marketPosition);
        subTasks.Enqueue(go);

        BuyTask buy = new();
        buy.SetAttributes(order);
        subTasks.Enqueue(buy);
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
    // Serialized content
    public MarketOrder Order { get; set; }

    public BuyTask()
    {
        Discriminator = TaskDiscriminator.BuyTask;
    }

    public void SetAttributes(MarketOrder order)
    {
        base.SetAttributes("Buying " + order.Goods.ToString());
        Order = order;
    }

    public override TaskStatus Execute(Person p)
    {
        Status.Complete = true;
        if (Globals.Model.Market.AttemptTransact(Order))
            Status.ReturnValue = Order.Goods;
        return Status;
    }
}

public class HaulGoodsTask : Task
{
    public List<Goods> Hauling { get; set; }
    public Vector2 From { get; set; }
    public Vector2 To { get; set; }
    public Stockpile ToStockpile { get; set; }
    public float Salary { get; set; }

    public HaulGoodsTask() : base()
    {
        Hauling = new();
    }

    public void SetAttributes(string description, List<Goods> hauling, Vector2 from, Vector2 to, Stockpile toStockpile, float salary)
    {
        Description = description;
        ToStockpile = toStockpile;
        foreach (Goods goods in hauling)
            Hauling.Add(new Goods(goods));
        From = from;
        To = to;
        Salary = salary;
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
            Status.Complete = Complete(p);

        return Status;
    }

    public void Init(Person p)
    {
        Initialized = true;

        GoToTask goFrom = new();
        goFrom.SetAttributes("Going to pickup materials", From);
        subTasks.Enqueue(goFrom);

        GoToTask goTo = new();
        goTo.SetAttributes("Delivering materials", To);
        subTasks.Enqueue(goTo);
    }

    public override bool Complete(Person p)
    {
        foreach (Goods goods in Hauling)
            ToStockpile.Add(goods);
        p.Money += Salary;
        return true;
    }
}

public class SellTask : Task
{
    // Serialized content
    public List<Goods> Goods { get; set; }

    public SellTask()
    {
        Discriminator = TaskDiscriminator.SellTask;
    }

    public void SetAttributes(List<Goods> goods)
    {
        Goods = goods;
        string description = "Selling ";
        foreach (Goods g in goods)
            description += g.ToString() + ", ";
        base.SetAttributes(description);
    }

    public override TaskStatus Execute(Person p)
    {
        foreach (Goods g in Goods)
            Globals.Model.Market.PlaceSellOrder(MarketOrder.Create(p, false, g));
        Status.Complete = true;
        return Status;
    }
}

public class GoToTask : Task
{
    // Serialized content
    public Vector2 Destination { get; set; }
    public Vector2 direction2 { get; set; }

    // TODO: this struct won't work properly when made serializable with getter/setter
    public Vector2 Direction;

    public GoToTask()
    {
        Discriminator = TaskDiscriminator.GoToTask;
        Direction = Vector2.Zero;
    }

    public void SetAttributes(string description, Vector2 destination)
    {
        base.SetAttributes(description);
        Destination = destination;
    }

    public override TaskStatus Execute(Person p)
    {
        if (Direction == Vector2.Zero)
        {
            Direction = Destination - p.Position;
            Direction.Normalize();
            direction2 = Direction;
        }

        // Move in the direction of the destination at default movespeed scaled by time elapsed
        p.Position += Direction * (Person.MOVE_SPEED * Globals.Time);

        // If we're within 1/8th of the tile's height from the destination, 
        // choose a new one as the home tile's origin plus or minus half the width/height
        float distance = Vector2.Distance(p.Position, Destination);

        if (distance < Map.TileSize.Y / 8.0f)
        {
            Status.Complete = true;
        }
        return Status;
    }
}

public class SellAtMarketTask : Task
{
    public SellAtMarketTask()
    {
        Discriminator = TaskDiscriminator.SellAtMarketTask;
        SetAttributes("Selling goods at the market");
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
                GoToTask go = new();
                go.SetAttributes("Going to the market", market.Sprite.Position);
                subTasks.Enqueue(go);

                SellTask sell = new();
                sell.SetAttributes(toSell);
                subTasks.Enqueue(sell);
            }
        }
        Initialized = true;
    }
}

public class BuyFoodFromMarketTask : Task
{
    public BuyFoodFromMarketTask()
    {
        SetAttributes("Buying food from the market");
    }

    public override TaskStatus Execute(Person p)
    {
        if (!Initialized)
            return Init(p);

        // Try to complete subtasks first
        TaskStatus subStatus = base.Execute(p);
        if (subStatus != null && !subStatus.Complete)
            return Status;

        // But only when all subtasks are done
        if (subTasks.Count == 0)
            Status.Complete = true;

        return Status;
    }

    public TaskStatus Init(Person p)
    {
        Initialized = true;

        // Try to buy the cheapest, most filling food from the market if hungry
        Building market = (Building)Tile.Find(p.Home, 
            new TileFilter(buildingType: BuildingType.MARKET));

        if (market != null && p.Hunger > Person.DAILY_HUNGER)
        {
            BuyFromMarketTask buyTask = new();
            MarketOrder foodOrder = Globals.Model.Market.CheapestFood(p.Hunger);

            if (foodOrder != null)
            {
                foodOrder.Requestor = p;

                // Don't order more than you can afford (max 80% of money)
                float price = Globals.Model.Market.GetPrice(foodOrder.Goods.GetId());
                foodOrder.Goods.Quantity = Math.Min(p.Money * 0.8f / price, foodOrder.Goods.Quantity);
                buyTask.SetAttributes(market.Sprite.Position, foodOrder);
                
                // Cook right after buying (if raw)
                subTasks.Enqueue(buyTask);
                if (foodOrder.Goods.IsCookable())
                {
                    List<Goods> rawFood = new();
                    rawFood.Add(new Goods(foodOrder.Goods));
                    subTasks.Enqueue(CookTask.Create(rawFood));
                }

                // Eat right after buying
                subTasks.Enqueue(new EatTask());
            }
        }
        return Status;
    }
}

// Eat any food  you're holding until you're no longer hungry
public class EatTask : Task
{
    public EatTask()
    {
        Discriminator = TaskDiscriminator.EatTask;
        SetAttributes("Eating food");
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

public class BuildTask : Task
{
    // Serialized content
    public ConstructionRequest Request { get; set; }

    public BuildTask() 
    {
           
    }

    public static BuildTask Create(ConstructionRequest request, string description)
    {
        BuildTask task = new() { 
            Description = description,
            Request = request 
        };
        return task;
    }

    public override TaskStatus Execute(Person p)
    {
        if (!Initialized)
            return Init(p);

        // Try to complete subtasks first
        TaskStatus subStatus = base.Execute(p);
        if (subStatus != null && !subStatus.Complete)
            return Status;
    
        // Work until the request is complete
        // If large building tasks are added, set a time limit for a single BuildTask
        if (subTasks.Count == 0)
            Request.Work(p);

        if (Request.IsComplete)
            Complete(p);

        return Status;
    }

    public TaskStatus Init(Person p)
    {
        Initialized = true;

        GoToTask go = new();
        go.SetAttributes("Going to construction site to build", Request.ToBuild.Sprite.Position);
        subTasks.Enqueue(go);

        return Status;
    }

    public override bool Complete(Person p)
    {
        // Subtract durability from the tool used in construction
        ProductionRequirements reqs = BuildingProduction.GetRequirements(Request.ToBuild.Type);
        if (reqs != null && reqs.ToolRequirement != null)
            p.PersonalStockpile.UseTool(reqs.ToolRequirement.Tool, reqs.ToolTypeRequirement);

        Status.Complete = true;

        return true;
    }
}

public class DepositInventoryTask : Task
{
    public DepositInventoryTask()
    {
        Discriminator = TaskDiscriminator.DepositInventoryTask;
        SetAttributes("Depositing inventory at home");
    }

    public override TaskStatus Execute(Person p)
    {
        if (p.House == null)
        {
            Status.Complete = true;
            Status.Failed = true;
            return Status;
        }

        p.PersonalStockpile.DepositIntoExcludingProfessionTool(p.House.Stockpile, p.Profession);

        // Share household money
        p.House.Money += p.Money;
        p.Money = p.House.Money / p.House.CurrentUsers.Count;
        p.House.Money -= p.Money;

        Status.Complete = true;
        return Status;
    }
}

public class CookTask : Task
{
    // Serialized content
    public float TimeToProduce { get; set; }
    public float TimeSpent { get; set; }
    public Queue<Goods> ToCook { get; set; }

    public CookTask()
    {
        Discriminator = TaskDiscriminator.CookTask;
        TimeSpent = 0f;
        TimeToProduce = 0f;
        ToCook = new();
        SetAttributes("Cooking");
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

    public static CookTask Create(List<Goods> toCook)
    {
        CookTask task = new();
        foreach (Goods goods in toCook)
            task.ToCook.Enqueue(goods);
        return task;
    }

    public void Init(Person p)
    {
        // Task may be created with a list of goods ready to cook
        if (ToCook.Count > 0)
        {
            Initialized = true;
            return;
        }
        
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
            // No need to cook if food is already available, this food will be used by EatTask that follows
            if (g.IsEdible())
            {
                int satiation = GoodsInfo.GetSatiation(g);

                Goods req = new(g);
                req.Quantity = (limit - current) / satiation;
                p.House.Stockpile.Take(req);
                current += req.Quantity * satiation;
            }
            else if (g.IsCookable())
            {
                // Try to take as much as needed to reach limit
                Goods cooked = new(g);
                cooked.Cook();
                int satiation = GoodsInfo.GetSatiation(cooked);

                Goods req = new(g);
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

    public override string Describe(string extra = "", bool debug = true, string depth = "")
    {
        string food = "(preparing)";
        if (ToCook.Count > 0)
            food = new Goods(ToCook.Peek()).Cook().ToString();
        return base.Describe(food, debug, depth);
    }
}