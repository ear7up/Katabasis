public class SowTask : Task
{
    public Farm TargetFarm;

    public SowTask() : base()
    {

    }

    public void SetAttributes(string description, Farm farm)
    {
        Description = description;
        TargetFarm = farm;
    }

    public static SowTask Create(string description, Farm farm)
    {
        SowTask task = new();
        task.SetAttributes(description, farm);
        return task;
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
        {
            bool complete = TargetFarm.Sow(p);
            if (complete)
                Status.Complete = Complete(p);
        }

        return Status;
    }

    public override TaskStatus Init(Person p)
    {
        Initialized = true;

        GoToTask go = new();
        go.SetAttributes("Going to sow seeds", TargetFarm.FarmBuilding.Location.GetPosition());
        subTasks.Enqueue(go);

        return Status;
    }

    public override bool Complete(Person p)
    {
        return true;
    }
}

public class HarvestTask : Task
{
    public Farm TargetFarm;

    public HarvestTask() : base()
    {

    }

    public void SetAttributes(string description, Farm farm)
    {
        Description = description;
        TargetFarm = farm;
    }

    public static SowTask Create(string description, Farm farm)
    {
        SowTask task = new();
        task.SetAttributes(description, farm);
        return task;
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
        {
            bool complete = TargetFarm.Harvest(p);
            if (complete)
                Status.Complete = Complete(p);
        }

        return Status;
    }

    public override TaskStatus Init(Person p)
    {
        Initialized = true;

        GoToTask go = new();
        go.SetAttributes("Going to harvest", TargetFarm.FarmBuilding.Location.GetPosition());
        subTasks.Enqueue(go);

        return Status;
    }

    public override bool Complete(Person p)
    {
        return true;
    }
}