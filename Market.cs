using System;
using System.Collections;
using System.Collections.Generic;

public class MarketOrder
{
    public Person requestor;
    public bool buying;
    public Goods goods;

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

static class Market
{
    // BuyOrders: Goods.ID -> List<MarketOrder>
    private static Hashtable BuyOrders;

    // SellOrders: Goods.ID -> List<MarketOrder>
    private static Hashtable SellOrders;

    // Keep a tabulated list of market prices where each index is the id of a good
    public static float[] Prices;

    public static Kingdom Kingdom;

    public static void Init(Kingdom kingdom)
    {
        Kingdom = kingdom;
        BuyOrders = new();
        SellOrders = new();

        Array goodsTypes = Enum.GetValues(typeof(GoodsType));
        int num_goods = goodsTypes.Length * Goods.MAX_GOODS_PER_CATEGORY;

        Prices = new float[num_goods];
        for (int g = 0; g < Prices.Length; g++)
            Prices[g] = GoodsInfo.GetDefaultPrice(g);
    }

    // Large numbers of villagers will attemp to place buy orders, but won't actually
    // place them, use the number of requests to increase prices of in-demand goods
    public static void Update()
    {
        // Increase or decrease prices proportiontely to supply and demand
        // E.g. 10 buy orders, 3 sell orders for wheat, increase prices by 0.7%/s
        for (int i = 0; i < Prices.Length; i++)
        {
            // TODO: this currently doesn't work because buyers don't wait around
            // they just AttemptTransact and bail if they can't buy immediately
            // so buying always equals null

            // Consider capping prices at a multiple of the default price
            // this may be unnecessary as long as villagers prioritize the most profitable work
            // AND they need to cancel orders when work becomes unprofitable
            List<MarketOrder> buying = (List<MarketOrder>)BuyOrders[i];
            List<MarketOrder> selling = (List<MarketOrder>)SellOrders[i]; 
            if (buying == null || selling == null)
                continue;
            Prices[i] *= (1 + 0.001f * (buying.Count - selling.Count)) * Globals.Time;
        }
    }

    public static string Describe()
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
    public static bool AttemptTransact(MarketOrder o)
    {
        Hashtable orders = SellOrders;
        if (!o.buying)
            orders = BuyOrders;

        // Trying to buy, can't afford it
        float cost = GetPrice(o.goods.GetId()) * o.goods.Quantity;
        if (o.buying && o.requestor.Money < cost)
            return false;
        
        // Trying to sell, don't have it
        if (!o.buying && !o.requestor.PersonalStockpile.Has(o.goods))
            return false;

        List<MarketOrder> trades = (List<MarketOrder>)orders[o.goods.GetId()];

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
    public static bool PlaceBuyOrder(MarketOrder o)
    {
        float unitPrice = GetPrice(o.goods.GetId());

        // Requestor cannot afford his order
        if (o.requestor.Money < o.goods.Quantity * unitPrice)
            return false;
        
        if (!AttemptTransact(o))
        {
            List<MarketOrder> orders = (List<MarketOrder>)BuyOrders[o.goods.GetId()];

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

    public static void CancelBuyOrder(Person p, int goodsId)
    {
        List<MarketOrder> orders = (List<MarketOrder>)BuyOrders[goodsId];
        orders.RemoveAll(order => order.requestor == p);
    }

    // Get the price of one unit of the given good, plus taxes
    public static float GetPrice(int goodsId)
    {
        return Prices[goodsId] * (1 + Kingdom.TaxRate);
    }

    // Get just the tax amount from the total price including tax
    public static float GetTax(float price)
    {
        return price * (1f / (1f + Kingdom.TaxRate));
    }

    // Tries to execute the order first, then adds it if note complete
    // Returns false if the order could not be added (currently always added)
    public static bool PlaceSellOrder(MarketOrder o)
    {
        if (!AttemptTransact(o))
        {
            List<MarketOrder> orders = (List<MarketOrder>)SellOrders[o.goods.GetId()];

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

    public static void CancelSellOrder(Person p, int goodsId)
    {
        List<MarketOrder> orders = (List<MarketOrder>)SellOrders[goodsId];
        orders.RemoveAll(order => order.requestor == p);
    }
}