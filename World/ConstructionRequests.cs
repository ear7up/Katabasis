using System.Collections.Generic;
using System.Linq;

public class ConstructionRequest
{
    public bool IsComplete { get; set; }
    public bool ReadyToHaul { get; set; }
    public bool ReadyToBuild { get; set; }
    public bool DeliveryInProgress { get; set; }
    public float TimeSpent { get; set; }
    public float HaulingMoney { get; set; }
    public float BuildingMoney { get; set; }
    public List<Goods> GoodsRequired { get; set; }

    private Building _ToBuild;
    public Building ToBuild { 
        get { return _ToBuild; }
        set { 
            _ToBuild= value;
            Requirements = (ProductionRequirements)BuildingProduction.Requirements[value.Type];
        }
    }

    public ProductionRequirements Requirements;

    public ConstructionRequest()
    {

    }

    public static ConstructionRequest Create(Tile location, Building building, List<Goods> materials, float money)
    {
        ConstructionRequest req = new();
        req.ToBuild = building;
        req.Requirements = BuildingProduction.GetRequirements(building.Type);
        req.GoodsRequired = materials;

        building.BuildProgress = 0f;

        req.HaulingMoney = 0.1f * money;
        req.BuildingMoney = money - req.HaulingMoney;

        return req;
    }

    // Check to see if all of the prerequisites to start work have been met
    public void Update()
    {
        // All ready to go
        if (ReadyToBuild)
            return;

        // Check if all goods have been purchased and are ready to be hauled to the construction site
        if (!ReadyToHaul)
        {
            bool hasAll = true;
            foreach (Goods goods in GoodsRequired)
                hasAll = hasAll && Globals.Model.Player1.Kingdom.Treasury.Has(goods);

            ReadyToHaul = hasAll;
        }
        // Check if all goods are at the work site
        else if (DeliveryInProgress)
        {
            bool hasAll = true;
            foreach (Goods goods in GoodsRequired)
                hasAll = hasAll && ToBuild.Stockpile.Has(goods);

            ReadyToBuild = hasAll;

            if (hasAll)
                DeliveryInProgress = false;
        }
    }

    // This just checks if the person meets the skill requirement, all other requirements
    // will be sorted out before the construction request is placed
    public bool SkillRequirementMetBy(Person person)
    {
        if (Requirements == null)
            return false;

        // Person must meet the skill requirement if one is set
        if (Requirements.SkillRequirement != null)
        {
            int skill = (int)Requirements.SkillRequirement.skill;
            if (person.Skills[skill].level < Requirements.SkillRequirement.level)
                return false;
        }

        return true;
    }

    public bool ToolRequirementMetBy(Person person)
    {
        if (Requirements == null)
            return false;

        // Person must have the required tool
        if (Requirements.ToolRequirement != null)
        {
            Goods tool = new Goods(
                GoodsType.TOOL, 
                (int)Requirements.ToolRequirement.Tool, 1f, 
                (int)Requirements.ToolRequirement.Material);

            if (!person.PersonalStockpile.HasSome(tool))
                return false;
        }

        return true;
    }
    public Task GetHaulTask(Person p)
    {
        Building market = (Building)Tile.Find(p.Home, new TileFilter(buildingType: BuildingType.MARKET));

        // Take goods from the market to the building and place them in the building's stockpile
        // TODO: take from Globals.Model.Player1.Person.PersonalStockpile
        HaulGoodsTask htask = new();
        htask.SetAttributes(
            $"Hauling goods to {Globals.Lowercase(ToBuild.Type.ToString())} construction site",
            GoodsRequired, 
            market.Sprite.Position, 
            ToBuild.Sprite.Position, 
            ToBuild.Stockpile,
            HaulingMoney);

        // Take all of the goods to be hauled out of the treasury
        foreach (Goods goods in GoodsRequired)
            Globals.Model.Player1.Kingdom.Treasury.Take(goods.GetId(), goods.Quantity);

        HaulingMoney = 0f;

        return htask;
    }

    public Task GetSourceToolTask(Person p)
    {
        if (Requirements == null)
            return null;

        if (Requirements.ToolRequirement != null)
        {
            Goods tool = new Goods(
                GoodsType.TOOL, 
                (int)Requirements.ToolRequirement.Tool, 1f, 
                (int)Requirements.ToolRequirement.Material);

            // Quantity must be enough to complete the project
            tool.Quantity = GoodsInfo.GetUseRate(tool);

            SourceGoodsTask task = new();
            task.SetAttributes(tool);
            return task;
        }

        return null;
    }

    public Task GetBuildTask(Person p)
    {
        BuildTask task = BuildTask.Create(this, $"Building a {Globals.Lowercase(ToBuild.Type.ToString())}");
        return task;
    }

    // First returns a HaulGoodsTask (once) that anyone can complete
    // Then returns a BuildTasks that may have skill and tool requirements
    public Task GetTask(Person p)
    {
        // Need to wait for the builder to deliver the required goods before proceeding
        if (DeliveryInProgress)
            return null;

        if (ReadyToBuild && SkillRequirementMetBy(p))
        {
            if (!ToolRequirementMetBy(p))
                return GetSourceToolTask(p);
            else
                return GetBuildTask(p);
        }

        if (ReadyToHaul && !DeliveryInProgress && !ReadyToBuild)
        {
            // TODO: what happens if the deliveryman dies before delivering?
            // maybe villagers should be unkillable during delivery tasks, then die after completing?
            DeliveryInProgress = true;
            return GetHaulTask(p);
        }

        return null;
    }

    // Spend time working on building the building
    public void Work(Person person)
    {
        if (!ReadyToBuild)
            return;

        TimeSpent += Globals.Time;

        // Any number of builders can work on the project, 
        // they get paid a wage as a percentage of the total build time
        float buildTime = BuildingInfo.GetBuildTime(ToBuild.Type);
        float wage = Globals.Time / buildTime * BuildingMoney;
        person.Money += wage;

        ToBuild.BuildProgress = TimeSpent / buildTime;

        if (TimeSpent >= buildTime)
            Complete();
    }

    public void Complete()
    {
        IsComplete = true;
        BuildingMoney = 0f;

        // Type is no longer BuildingType.UNDER_CONSTRUCTION, it can be used
        ToBuild.Type = ToBuild.Type;
    }
}

public class ConstructionRequests
{
    public List<ConstructionRequest> ConstructionQueue { get; set; }

    public ConstructionRequests()
    {
        ConstructionQueue = new();
    }

    public void AddRequest(ConstructionRequest req)
    {
        ConstructionQueue.Add(req);
    }

    public Task GetTask(Person p)
    {
        // Find the first task that the person is capable of doing
        // it may just be a SourceGoodsTask to go get a hammer, though
        foreach (ConstructionRequest req in ConstructionQueue.OrderBy(x => Globals.Rand.Next()))
        {
            Task task = req.GetTask(p);
            if (task != null)
                return task;
        }
        return null;
    }

    public void Update()
    {
        foreach (ConstructionRequest req in ConstructionQueue)
            req.Update();

        // Remove all completed requests
        ConstructionQueue.RemoveAll(x => x.IsComplete);
    }
}