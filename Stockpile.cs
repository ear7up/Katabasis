using System.Collections;

public class Stockpile
{
    private Hashtable _stock;

    public Stockpile()
    {
        _stock = new();
    }

    // Add quantity to stockpile if the good already exists, otherwise adds the good in the specified quantity
    public void Add(Goods goods)
    {
        Goods current = (Goods)_stock[goods.GetId()];
        if (current != null)
            current.Quantity += goods.Quantity;
        else
            _stock.Add(goods.GetId(), new Goods(goods));    
        goods.Quantity = 0;
    }

    // Takes goods from the stockpile, sets quantity to the amount taken (may be less than requested)
    public void Take(Goods goods)
    {
        Goods available = (Goods)_stock[goods.GetId()];
        if (available != null)
        {
            goods.Quantity = available.Take(goods.Quantity);
            if (available.Quantity == 0)
                _stock.Remove(available.GetId());
        }
        else
            goods.Quantity = 0;
    }

    public bool Has(Goods goods)
    {
        Goods available = (Goods)_stock[goods.GetId()];
        if (available == null || available.Quantity < goods.Quantity)
            return false;
        return true;
    }

    public void RemoveIfEmpty(Goods goods)
    {
        Goods available = (Goods)_stock[goods.GetId()];
        if (available == null || available.Quantity <= 0)
            _stock.Remove(available.GetId());
    }

    public Goods Get(Goods g)
    {
        return (Goods)_stock[g.GetId()];
    }

    public void Set(Goods g)
    {
        _stock[g.GetId()] = g;
    }

    public ICollection Values()
    {
        return _stock.Values;
    }

    public override string ToString()
    {
        string s = "[\n";
        foreach (Goods g in _stock.Values)
        {
            s += "  " + g.ToString() + "\n";
        }
        return s + "]";
    }
}