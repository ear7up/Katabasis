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
    public const float SOW_TIME = 60f;
    public const float GROW_TIME = 300f;
    public const float HARVEST_TIME = 60f;
    
    public int PlantId { get; set; }
    public bool Growing { get; set; }
    public FarmState State { get; set; }
    public Building FarmBuilding { get; set; }

    public float Timer { get; set; }

    public Farm()
    {
        State = FarmState.UNPLANTED;
        PlantId = 0;
        Growing = false;
    }

    public static Farm Create(Building building)
    {
        Farm farm = new();
        farm.FarmBuilding = building;
        return farm;
    }

    public void StartSowing(Goods.FoodPlant plant, float quantity)
    {
        if (!Globals.Model.Player1.IsPlantUnlocked(plant))
            return;

        Goods g = new(GoodsType.FOOD_PLANT, (int)plant);
        PlantId = g.GetId();
        Timer = SOW_TIME;
        State = FarmState.SOWING;
    }

    public void StartSowing(Goods.MaterialPlant plant, float quantity)
    {
        Goods g = new(GoodsType.FOOD_PLANT, (int)plant);
        PlantId = g.GetId();
        Timer = SOW_TIME; 
        State = FarmState.SOWING;
    }

    public bool Sow(Person worker)
    {
        // TODO: farming skill should make this faster
        // TODO: level up farming

        Timer -= Globals.Time;
        if (Timer > 0f)
            return false;
        State = FarmState.GROWING;
        Timer = GROW_TIME;
        return true;
    }

    public bool Grow()
    {
        Timer -= Globals.Time;
        if (Timer > 0f)
            return false;

        State = FarmState.GROWN;
        Timer = HARVEST_TIME;
        return true;
    }

    public bool Harvest(Person worker)
    {
        // TODO: farming skill should make this go faster
        // TODO: level up farming
        float skillModifier = 1.0f;

        // TODO: assumes 1-to-1 relationship between time and plant quantity
        worker.PersonalStockpile.Add(PlantId, Globals.Time * skillModifier);

        Timer -= Globals.Time * skillModifier;
        if (Timer > 0f)
            return false;

        State = FarmState.UNPLANTED;
        Timer = 0f;
        return true;
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
        string plant = "ERROR";
        switch (type)
        {
            case GoodsType.MATERIAL_PLANT: plant = Globals.Title(((Goods.MaterialPlant)subType).ToString()); break;
            case GoodsType.FOOD_PLANT: plant = Globals.Title(((Goods.FoodPlant)subType).ToString()); break;
        }
        return plant;
    }

    public Task GetTask(Person p)
    {
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
    public List<Farm> Farms { get; set; }

    public FarmingManager()
    {
        Farms = new();
    }

    public void AddFarm(Building farmBuilding)
    {
        Farms.Add(Farm.Create(farmBuilding));
    }

    public Task GetTask(Person p)
    {
        // Find the first task that the person is capable of doing
        // it may just be a SourceGoodsTask to go get a hoe, though
        foreach (Farm farm in Farms.OrderBy(x => Globals.Rand.Next()))
        {
            Task task = farm.GetTask(p);
            if (task != null)
                return task;
        }
        return null;
    }

    public void Update()
    {
        foreach (Farm farm in Farms)
            farm.Update();
    }
}