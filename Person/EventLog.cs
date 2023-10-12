using System.Collections.Generic;

public class EventLog
{
    public const int MAX_EVENTS = 8;

    public LinkedList<string> Events { get; set; }

    public EventLog()
    {
        Events = new();
    }

    public void Add(string description)
    {
        Events.AddFirst(description);
        if (Events.Count > MAX_EVENTS)
            Events.RemoveLast();
    }

    public string Describe()
    {
        string description = "";
        foreach (string _event in Events)
            description += _event + "\n";
        return description.TrimEnd();
    }
}