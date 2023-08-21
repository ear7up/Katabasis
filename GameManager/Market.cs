using System;
using System.Collections;
using System.Collections.Generic;

public class MarketOrder
{
    public Person requestor { get; set; }
    public bool buying { get; set; }
    public Goods goods { get; set; }

    public MarketOrder(Person requestor, bool buying, Goods goods)
    {
        this.requestor = requestor;
        this.buying = buying;
        this.goods = goods;
    }

    public override string ToString()
    {
        return $"MarketOrder(requestor={requestor.Name}, buying={buying}, goods={goods})";
    }
}

public class Market
{
    // Serialized parameters

    // BuyOrders: Goods.ID -> List<MarketOrder>
    public Dictionary<int, List<MarketOrder>> BuyOrders { get; set; }

    // SellOrders: Goods.ID -> List<MarketOrder>
    public Dictionary<int, List<MarketOrder>> SellOrders { get; set; }

    // Keep a tabulated list of market prices where each index is the id of a good
    public float[] Prices { get; set; }

    // Net quantity attempted to purchase vs amount being sold
    public float[] Demand { get; set; }

    public Kingdom Kingdom { get; set; }

    public Market()
    {
        BuyOrders = new();
        SellOrders = new();

        Array goodsTypes = Enum.GetValues(typeof(GoodsType));
        int num_goods = goodsTypes.Length * Goods.MAX_GOODS_PER_CATEGORY;

        Prices = new float[num_goods];
        for (int g = 0; g < Prices.Length; g++)
            Prices[g] = GoodsInfo.GetDefaultPrice(g);

        Demand = new float[num_goods];
    }

    public List<MarketOrder> GetBuyOrders(int goodsId)
    {
        if (BuyOrders.ContainsKey(goodsId))
            return BuyOrders[goodsId];
        return null;
    }

    public List<MarketOrder> GetSellOrders(int goodsId)
    {
        if (SellOrders.ContainsKey(goodsId))
            return SellOrders[goodsId];
        return null;
    }

    public void SetAttributes(Kingdom kingdom)
    {
        Kingdom = kingdom;
    }

    public void Update()
    {
        // Increase or decrease prices proportiontely to supply and demand
        for (int i = 0; i < Prices.Length; i++)
        {
            // No change if demand is 0
            // Price changes by (0.01 * d) percent per second where d is net quantity surplus/excess
            // E.g. an outstanding request for 50 units will increase price by 0.05% per second (3%/min)
            float demand = Demand[i];
            Prices[i] *= 1 + (0.00001f * demand * Globals.Time);

            // Limit price fluctuations so they don't get too crazy
            // Also, this prevents the UI price bars from stretching out too far
            float defaultPrice = GoodsInfo.GetDefaultPrice(i);
            Prices[i] = MathHelper.Clamp(Prices[i], defaultPrice * 0.25f, defaultPrice * 4f);
        }
    }

    public string Describe()
    {
        string buyOrders = "";
        foreach (List<MarketOrder> list in BuyOrders.Values)
            foreach (MarketOrder buy in list)
                buyOrders += "    " + buy.ToString() + "\n";

        string sellOrders = "";
        foreach (List<MarketOrder> list in SellOrders.Values)
            foreach (MarketOrder sell in list)
                sellOrders += "    " + sell.ToString() + "\n";

        return $"Market(\n  buying=\n{buyOrders}\n  selling=\n{sellOrders})";
    }

    // Returns true if the transaction completed, false if it did not get completed
    // o.goods.Quantity reflects the amount sold if partially completed
    // automatically adds quantity purchased to the requestor's stockpile
    // automatically deducts money from requestor for purchases
    public bool AttemptTransact(MarketOrder o)
    {
        if (o.buying)
            Demand[o.goods.GetId()]++;
        else
            Demand[o.goods.GetId()]--;

        // Trying to buy, can't afford it
        float cost = GetPrice(o.goods.GetId()) * o.goods.Quantity;
        if (o.buying && o.requestor.Money < cost)
            return false;
        
        // Trying to sell, don't have it
        if (!o.buying && !o.requestor.PersonalStockpile.Has(o.goods))
            return false;

        List<MarketOrder> trades = null;
        if (o.buying)
            trades = GetSellOrders(o.goods.GetId());
        else
            trades = GetBuyOrders(o.goods.GetId());

        // Quit if no one is buying/selling the good being requested
        if (trades == null || trades.Count == 0)
            return false;
        
        // Try to buy as much of the order as possible
        float amountBoughtOrSold = 0f;
        for (int i = 0; o.goods.Quantity > 0 && i < trades.Count; i++)
        {
            MarketOrder trade = trades[i];

            // Can't buy or sell more than each individual trader is offering
            float sale_quantity = MathHelper.Min(o.goods.Quantity, trade.goods.Quantity);
            cost = sale_quantity * GetPrice(o.goods.GetId());

            float tax = GetTax(cost);

            // Trying to buy, can't afford it (price probably went up, defer until later)
            if (o.buying && o.requestor.Money < cost)
                break;

            // Trying to sell, buyer can't afford it (skip to next one)
            if (!o.buying && trade.requestor.Money < cost)
                continue;

            if (o.buying)
            {    
                o.requestor.Money -= cost;
                trade.requestor.Money += (cost - tax);
                Kingdom.Money += tax;

                Goods purchased = new Goods(o.goods);
                purchased.Quantity = sale_quantity;
                o.requestor.PersonalStockpile.Add(purchased);
            }
            else if (!o.buying)
            {
                o.requestor.Money += (cost - tax);
                trade.requestor.Money -= cost;
                Kingdom.Money += tax;

                Goods sold = new Goods(o.goods);
                sold.Quantity = sale_quantity;
                o.requestor.PersonalStockpile.Take(sold);
            }

            amountBoughtOrSold += sale_quantity;

            // E.g. reduce buying(5) to buying(3) if 2 were bought
            // it's the same for selling(5) to selling(3) if 2 were sold
            trade.goods.Quantity -= sale_quantity;
            o.goods.Quantity -= sale_quantity;
        }

        // Remove all fulfilled orders
        trades.RemoveAll(s => s.goods.Quantity <= 0.001f);
        
        if (amountBoughtOrSold < o.goods.Quantity)
            return false;
        return true;
    }

    // Returns false if the order could not be placed (e.g. not enough money)
    public bool PlaceBuyOrder(MarketOrder o)
    {
        float unitPrice = GetPrice(o.goods.GetId());

        // Requestor cannot afford his order
        if (o.requestor.Money < o.goods.Quantity * unitPrice)
            return false;
        
        if (!AttemptTransact(o))
        {
            List<MarketOrder> orders = GetBuyOrders(o.goods.GetId());

            // If there are no orders for the good, add it
            if (orders == null)
            {
                orders = new();
                BuyOrders[o.goods.GetId()] = orders;
            }
            orders.Add(o);
        }
        return true;
    }

    public void CancelBuyOrder(Person p, int goodsId)
    {
        List<MarketOrder> orders = GetBuyOrders(goodsId);
        orders.RemoveAll(order => order.requestor == p);
    }

    // Get the price of one unit of the given good, plus taxes
    public float GetPrice(int goodsId)
    {
        return Prices[goodsId] * (1 + Kingdom.TaxRate);
    }

    // Get just the tax amount from the total price including tax
    public float GetTax(float price)
    {
        return price * (1f / (1f + Kingdom.TaxRate));
    }

    // Tries to execute the order first, then adds it if note complete
    // Returns false if the order could not be added (currently always added)
    public bool PlaceSellOrder(MarketOrder o)
    {
        if (!AttemptTransact(o))
        {
            List<MarketOrder> orders = GetSellOrders(o.goods.GetId());

            // Remove the goods from the seller's inventory
            o.requestor.PersonalStockpile.Take(o.goods);

            // If there are no orders for the good, add it
            if (orders == null)
            {
                orders = new();
                SellOrders[o.goods.GetId()] = orders;
            }
            orders.Add(o);
        }
        return true;
    }

    public void CancelSellOrder(Person p, int goodsId)
    {
        List<MarketOrder> orders = GetSellOrders(goodsId);
        if (orders == null)
            return;
        orders.RemoveAll(order => order.requestor == p);
    }
}