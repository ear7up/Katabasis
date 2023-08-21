using System;
using System.Collections;
using System.Collections.Generic;

public class Stockpile
{
    // Serialized content
    public Dictionary<int, Goods> _stock { get; set; }

    public Stockpile()
    {
        _stock = new();
    }

    private Goods Get(int id)
    {
        if (_stock.ContainsKey(id))
            return _stock[id];
        return null;
    }

    // Add quantity to stockpile if the good already exists, otherwise adds the good in the specified quantity
    public void Add(Goods goods)
    {
        Goods current = Get(goods.GetId());
        if (current != null)
            current.Quantity += goods.Quantity;
        else
            _stock.Add(goods.GetId(), new Goods(goods));    
        goods.Quantity = 0;
    }

    public void Add(int goodsId, float quantity)
    {
        Goods current = Get(goodsId);
        if (current != null)
            current.Quantity += quantity;
        else
            _stock.Add(goodsId, Goods.FromId(goodsId, quantity));
    }

    // Add goods from another stockpile to this one
    public void Sum(Stockpile other)
    {
        foreach (Goods g in other.Values())
        {
            Goods current = Get(g.GetId());
            if (current != null)
                current.Quantity += g.Quantity;
            else
                _stock.Add(g.GetId(), new Goods(g));
        }
    }

    // Takes goods from the stockpile, sets quantity to the amount taken (may be less than requested)
    public void Take(Goods goods)
    {
        Goods available = Get(goods.GetId());
        if (available != null)
            goods.Quantity = available.Take(goods.Quantity);
        else
            goods.Quantity = 0;
    }

    public float Take(int goodsId, float quantity)
    {
        Goods available = Get(goodsId);
        if (available != null)
            return available.Take(quantity);
        return 0f;
    }

    // Apply decay rates ("daily" rather than continuous)
    public void DailyUpdate()
    {
        foreach (Goods g in _stock.Values)
            g.Quantity *= (1 - GoodsInfo.GetDecayRate(g));

        // Remove goods with zero or near-zero quantiy
        List<int> toRemove = new();
        foreach (Goods g in _stock.Values)
            if (g.Quantity <= 0.001f)
                toRemove.Add(g.GetId());

        foreach (int gid in toRemove)
            _stock.Remove(gid);
    }

    // Takes goods from the stockpile, sets quantity to the amount taken (may be less than requested)
    public void Borrow(Goods goods)
    {
        Goods available = Get(goods.GetId());
        if (available != null)
            goods.Quantity = available.Borrow(goods.Quantity);
        else
            goods.Quantity = 0;
    }

    public float Borrow(int goodsId, float quantity)
    {
        Goods available = Get(goodsId);
        if (available != null)
            return available.Borrow(quantity);
        return 0f;
    }

    public bool Has(Goods goods)
    {
        Goods available = Get(goods.GetId());
        if (available == null || available.Quantity < goods.Quantity)
            return false;
        return true;
    }

    public bool HasSome(Goods goods)
    {
        Goods available = Get(goods.GetId());
        if (available == null || available.Quantity <= 0.01f)
            return false;
        return true;
    }

    public void RemoveIfEmpty(Goods goods)
    {
        Goods available = Get(goods.GetId());
        if (available == null || available.Quantity <= 0)
            _stock.Remove(available.GetId());
    }

    public ICollection Values()
    {
        return _stock.Values;
    }

    public float Wealth()
    {
        float wealth = 0f;
        foreach (Goods g in _stock.Values)
            wealth += g.Value();
        return wealth;
    }

    public override string ToString()
    {
        string s = "";
        foreach (Goods g in _stock.Values)
            if (g.Quantity >= 0.1f)
                s += "  " + g.ToString() + "\n";
        return s + "";
    }

    public void DepositInto(Stockpile other)
    {
        foreach (Goods g in _stock.Values)
            other.Add(g);
        _stock.Clear();
    }

    public void DepositIntoExcludingFoodAndTools(Stockpile other)
    {
        List<Goods> keep = new();
        foreach (Goods g in _stock.Values)
            if (g.IsEdible() || g.IsTool())
                keep.Add(g);
            else
                other.Add(g);

        _stock.Clear();
        foreach (Goods g in keep)
            Add(g);
    }

    public float TotalSatiation()
    {
        float satiation = 0f;
        foreach (Goods g in _stock.Values)
            satiation += GoodsInfo.GetSatiation(g) * g.Quantity;
        return satiation;
    }

    // To allow foreach over Stockpile goods
    public IEnumerator GetEnumerator()
    {
        return _stock.Values.GetEnumerator();
    }

    public void UseTool(Goods.Tool toolType)
    {
        Goods g = Get(Goods.GetId(GoodsType.TOOL, (int)toolType));
        if (g != null)
            g.Use();
        else
            Console.WriteLine("Failed to use tool " + toolType.ToString());
    }
}