using System;
using System.Collections.Generic;

public class Military
{
    public const int MIN_AGE = 14;
    public const int MAX_AGE = 40;

    public Kingdom MyKingdom { get; set; }
    public List<Person> Soldiers { get; set; }

    public Military()
    {
        Soldiers = new();
    }

    public static Military Create(Kingdom kingdom)
    {
        Military military = new() {
            MyKingdom = kingdom
        };
        return military;
    }

    public void Add(Person person)
    {
        Soldiers.Add(person);
        person.Profession = ProfessionType.SOLDIER;
    }

    public void Dismiss(Person person)
    {
        if (person == null)
            return;

        foreach (Task task in person.Tasks) 
            if (task is DeploymentTask)
                task.Complete(person);

        Soldiers.Remove(person);
        person.ResetProfession();
    }

    // Dismiss the most recent `n` soldiers to have joined
    public void DismissN(int n)
    {
        n = Math.Min(n, Soldiers.Count);

        for (int i = 0; i < n; i++)
        {
            Soldiers[^1].ResetProfession();
            Soldiers.RemoveAt(Soldiers.Count - 1);
        }
    }

    public void Recruit(Person person)
    {
        person.Profession = ProfessionType.SOLDIER;
        Soldiers.Add(person);
    }

    public bool ValidCandidate(Person person)
    {
        if (person.Profession == ProfessionType.SOLDIER || person.Profession == ProfessionType.SCRIBE)
            return false;
        if (person.Gender == GenderType.FEMALE)
            return false;
        if (person.Age < MIN_AGE || person.Age > MAX_AGE)
            return false;
        return true;
    }

    // Recruit N people that meet the enlistment criteria in ValidateCandidate
    public void ConscriptN(int n)
    {
        int i = 0;
        int recruited = 0;
        while (recruited < n)
        {
            if (i >= MyKingdom.People.Count)
                break;

            Person person = MyKingdom.People[i++];
            if (ValidCandidate(person))
            {
                Recruit(person);
                recruited++;
            }
        }
    }

    public void Deploy(Vector2 pos)
    {
        CancelDeployment();
        foreach (Person soldier in Soldiers)
            soldier.Tasks.Enqueue(DeploymentTask.Create(pos));
    }

    public void CancelDeployment()
    {
        foreach (Person soldier in Soldiers)
            foreach (Task task in soldier.Tasks) 
                if (task is DeploymentTask)
                    task.Complete(soldier);
    }
}