using System.Collections.Generic;

// Priority queue wrapper that lets you maintain FIFO order for non-priority elements
public class PriorityQueue2<TElement, TPriority>
{
    public PriorityQueue<TElement, TPriority> pq { get; set; }
    public Queue<TElement> q { get; set; }

    public PriorityQueue2()
    {
        pq = new();
        q = new();
    }

    public void Enqueue(TElement e)
    {
        q.Enqueue(e);
    }

    public void Enqueue(TElement e, TPriority p)
    {
        pq.Enqueue(e, p);
    }

    public TElement Peek()
    {
        if (pq.Count > 0)
            return pq.Peek();
        return q.Peek();
    }

    public void Dequeue()
    {
        if (pq.Count > 0)
            pq.Dequeue();
        else if (q.Count > 0)
            q.Dequeue();
    }

    public bool Empty()
    {
        return pq.Count == 0 && q.Count == 0;
    }

    public Queue<TElement>.Enumerator GetEnumerator()
    {
        return q.GetEnumerator();
    }
}