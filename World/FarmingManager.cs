using System.Collections.Generic;
using System.Linq;

public enum FarmState
{
    FALLOW,
    UNPLANTED,
    SOWING,
    GROWING,
    GROWN,
}

public class Farm
{
    public const float PRODUCED_QUANTITY = 300f;
    public const float SOW_TIME = 60f;
    public const float GROW_TIME = 300f;
    public const float HARVEST_TIME = 60f;
    public const float TOTAL_WORK_TIME = SOW_TIME + HARVEST_TIME;
    
    public int PlantId { get; set; }
    public FarmState State { get; set; }
    public Building FarmBuilding { get; set; }

    public float TimeRemaining { get; set; }
    public float TimeTotal { get; set; }

    // Person ID -> Goods (quantity = time worked)
    public Dictionary<int, Goods> TimeWorked { get; set; }

    public Farm()
    {
        State = FarmState.UNPLANTED;
        PlantId = 0;
        TimeWorked = new();
    }

    public static Farm Create(Building building)
    {
        Farm farm = new();
        farm.FarmBuilding = building;
        return farm;
    }

    public string Describe()
    {
        string description = "";
        description += Globals.Title(State.ToString()) + "\n";
        description += GetPlantName() + "\n";
        return description;
    }

    public void StartSowing(int plantId, float quantity = 200f)
    {
        if (!Globals.Model.Player1.IsPlantUnlocked(plantId))
            return;

        PlantId = plantId;
        TimeRemaining = SOW_TIME;
        TimeTotal = SOW_TIME;
        State = FarmState.SOWING;
    }

    public void StartSowing(Goods.MaterialPlant plant, float quantity)
    {
        Goods g = new(GoodsType.FOOD_PLANT, (int)plant);
        PlantId = g.GetId();
        TimeRemaining = SOW_TIME;
        TimeTotal = SOW_TIME;
        State = FarmState.SOWING;
    }

    public bool Sow(Person worker)
    {
        // Another worker finished sowing, signal sowing
        if (State != FarmState.SOWING)
            return true;

        float adjustedTime = Globals.Time * GetFarmingSkillModifier(worker);

        // Extremely low chance to level up
        worker.GainExperience((int)Skill.FARMING, -99 * SkillLevel.INCREASE_CHANCE);

        if (!TimeWorked.ContainsKey(worker.Id))
            TimeWorked[worker.Id] = Goods.FromId(PlantId, quantity: 0);
        TimeWorked[worker.Id].Quantity -= adjustedTime;

        TimeRemaining -= adjustedTime;
        if (TimeRemaining > 0f)
            return false;
        State = FarmState.GROWING;
        TimeRemaining = GROW_TIME;
        TimeTotal = GROW_TIME;
        return true;
    }

    public bool Grow()
    {
        // Growing will be ~30-120% speed based on soil quality
        float adjustedTime = Globals.Time * FarmBuilding.Location.SoilQuality;

        TimeRemaining -= adjustedTime;
        if (TimeRemaining > 0f)
            return false;

        State = FarmState.GROWN;
        TimeRemaining = HARVEST_TIME;
        TimeTotal = HARVEST_TIME;
        return true;
    }

    // +0.5% speed per farming level (caps at 100 farming = +50%)
    public float GetFarmingSkillModifier(Person worker)
    {
        return 1 + (worker.Skills[(int)Skill.FARMING].level / 200f);
    }

    // Returns true when harvesting is done
    public bool Harvest(Person worker)
    {
        // Another worker finished harvesting, signal complete
        if (State != FarmState.GROWN)
            return true;

        // Working time is accelerated by the FARMING skill
        float adjustedTime = Globals.Time * GetFarmingSkillModifier(worker);

        // Get a portion of the total produced quantity
        float quantity = adjustedTime * (PRODUCED_QUANTITY / HARVEST_TIME);

        if (!TimeWorked.ContainsKey(worker.Id))
            TimeWorked[worker.Id] = Goods.FromId(PlantId, quantity: 0);
        TimeWorked[worker.Id].Quantity -= adjustedTime;

        // Extremely low chance to level up
        worker.GainExperience((int)Skill.FARMING, -99 * SkillLevel.INCREASE_CHANCE);

        TimeRemaining -= adjustedTime;
        if (TimeRemaining > 0f)
            return false;

        State = FarmState.SOWING;
        TimeRemaining = SOW_TIME;
        TimeTotal = SOW_TIME;

        // Hack: quantity will be negative until harvest is finished, then flipped to positive
        // to indicate it is ready to be collected
        foreach (Goods owed in TimeWorked.Values)
            owed.Quantity = System.Math.Abs(owed.Quantity);

        return true;
    }

    public void Collect(Person worker, Goods toCollect)
    {
        worker.PersonalStockpile.Add(toCollect.GetId(), toCollect.Quantity);
    }

    public void Update()
    {
        switch (State)
        {
            case FarmState.GROWING: Grow(); break;
            case FarmState.GROWN: Rot(); break;
        }
    }

    public void Rot()
    {
        // TODO: After a certain amount of time, grown foods should rot if unharvested
    }

    // This just checks if the person meets the skill requirement, all other requirements
    // will be sorted out before the construction request is placed
    public bool SkillRequirementMetBy(Person person)
    {
        ProductionRequirements Requirements = (ProductionRequirements)GoodsProduction.Requirements[PlantId];
        if (Requirements == null)
            return true;

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
        ProductionRequirements Requirements = (ProductionRequirements)GoodsProduction.Requirements[PlantId];
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

    public Task GetSourceToolTask(Person p)
    {
        ProductionRequirements Requirements = (ProductionRequirements)GoodsProduction.Requirements[PlantId];
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

    public string GetPlantName()
    {
        GoodsType type = (GoodsType)Goods.TypeFromId(PlantId);
        int subType = Goods.SubTypeFromid(PlantId);
        string plant = "";
        switch (type)
        {
            case GoodsType.MATERIAL_PLANT: plant = Globals.Title(((Goods.MaterialPlant)subType).ToString()); break;
            case GoodsType.FOOD_PLANT: plant = Globals.Title(((Goods.FoodPlant)subType).ToString()); break;
        }
        return plant;
    }

    public Task GetTask(Person p)
    {
        // If the person has done work and the crops have been harvested (positive quantity)
        // assign a task to come to the farm and collect their share
        if (TimeWorked.ContainsKey(p.Id) && TimeWorked[p.Id].Quantity > 0)
        {
            Goods toCollect = TimeWorked[p.Id];
            toCollect.Quantity = (TimeWorked[p.Id].Quantity / TOTAL_WORK_TIME) * PRODUCED_QUANTITY;
            Task task = CollectTask.Create("Collecting harvest", this, toCollect);
            TimeWorked.Remove(p.Id);
            return task;
        }

        if (!SkillRequirementMetBy(p))
            return null;

        if (!ToolRequirementMetBy(p))
            return GetSourceToolTask(p);

        if (PlantId == 0)
            return null;

        string plant = GetPlantName();
        
        // Don't use the farm when fallow
        // When unplanted, sow seeds (multiple people can sow)
        // When grown, harvest (multiple people can harvest)
        return State switch
        {
            FarmState.FALLOW => null,
            FarmState.UNPLANTED => SowTask.Create($"Sowing {plant}", this),
            FarmState.SOWING => SowTask.Create($"Sowing {plant}", this),
            FarmState.GROWING => null,
            FarmState.GROWN => HarvestTask.Create($"Harvesting {plant}", this),
            _ => null,
        };
    }
}

public class FarmingManager
{
    public Dictionary<int, Farm> Farms { get; set; }

    public FarmingManager()
    {
        Farms = new();
    }

    public void AddFarm(Building farmBuilding)
    {
        Farms[farmBuilding.Id] = Farm.Create(farmBuilding);
    }

    public Task GetTask(Person p)
    {
        // Find the first task that the person is capable of doing
        // it may just be a SourceGoodsTask to go get a hoe, though
        foreach (KeyValuePair<int, Farm> kv in Farms.OrderBy(x => Globals.Rand.Next()))
        {
            Task task = kv.Value.GetTask(p);
            if (task != null)
                return task;
        }
        return null;
    }

    public void Update()
    {
        foreach (KeyValuePair<int, Farm> kv in Farms)
            kv.Value.Update();
    }

    public Farm GetFarm(Building building)
    {
        if (Farms.ContainsKey(building.Id))
            return Farms[building.Id];
        return null;
    }
}