using System;
using System.Collections;
using System.Collections.Generic;

public class MarketOrder
{
    public Person Requestor { get; set; }
    public bool Buying { get; set; }
    public Goods Goods { get; set; }

    public MarketOrder()
    {

    }

    public static MarketOrder Create(Person requestor, bool buying, Goods goods)
    {
        MarketOrder order = new()
        {
            Requestor = requestor,
            Buying = buying,
            Goods = goods
        };
        return order;
    }

    public override string ToString()
    {
        return $"MarketOrder(requestor={Requestor.Name}, buying={Buying}, goods={Goods})";
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
    public int CheapestFoodId { get; set; }

    public Market()
    {
        BuyOrders = new();
        SellOrders = new();
        CheapestFoodId = 0;

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

    public void DailyUpdate()
    {
        // Calculate the current cheapest food
        CheapestFoodUncached(Person.DAILY_HUNGER);
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
        if (o.Buying)
            Demand[o.Goods.GetId()]++;
        else
            Demand[o.Goods.GetId()]--;

        // Trying to buy, can't afford it
        float cost = GetPrice(o.Goods.GetId()) * o.Goods.Quantity;
        if (o.Buying && o.Requestor.Money < cost)
            return false;
        
        // Trying to sell, don't have it
        if (!o.Buying && !o.Requestor.PersonalStockpile.Has(o.Goods))
            return false;

        List<MarketOrder> trades = null;
        if (o.Buying)
            trades = GetSellOrders(o.Goods.GetId());
        else
            trades = GetBuyOrders(o.Goods.GetId());

        // Quit if no one is buying/selling the good being requested
        if (trades == null || trades.Count == 0)
            return false;
        
        // Try to buy as much of the order as possible
        float amountBoughtOrSold = 0f;
        for (int i = 0; o.Goods.Quantity > 0 && i < trades.Count; i++)
        {
            MarketOrder trade = trades[i];

            // Can't buy or sell more than each individual trader is offering
            float sale_quantity = MathHelper.Min(o.Goods.Quantity, trade.Goods.Quantity);
            cost = sale_quantity * GetPrice(o.Goods.GetId());

            float tax = GetTax(cost);

            // Trying to buy, can't afford it (price probably went up, defer until later)
            if (o.Buying && o.Requestor.Money < cost)
                break;

            // Trying to sell, buyer can't afford it (skip to next one)
            if (!o.Buying && trade.Requestor.Money < cost)
                continue;

            if (o.Buying)
            {    
                o.Requestor.Money -= cost;
                trade.Requestor.Money += (cost - tax);
                Kingdom.Owner.Person.Money += tax;

                Goods purchased = new Goods(o.Goods);
                purchased.Quantity = sale_quantity;
                o.Requestor.PersonalStockpile.Add(purchased);
            }
            else if (!o.Buying)
            {
                o.Requestor.Money += (cost - tax);
                trade.Requestor.Money -= cost;
                Kingdom.Owner.Person.Money += tax;

                Goods sold = new Goods(o.Goods);
                sold.Quantity = sale_quantity;
                trade.Requestor.PersonalStockpile.Add(sold.GetId(), sold.Quantity);
                o.Requestor.PersonalStockpile.Take(sold);
            }

            amountBoughtOrSold += sale_quantity;

            // E.g. reduce buying(5) to buying(3) if 2 were bought
            // it's the same for selling(5) to selling(3) if 2 were sold
            trade.Goods.Quantity -= sale_quantity;
            o.Goods.Quantity -= sale_quantity;
        }

        // Remove all fulfilled orders
        trades.RemoveAll(s => s.Goods.Quantity <= 0.001f);
        
        if (amountBoughtOrSold < o.Goods.Quantity)
            return false;
        return true;
    }

    // Returns false if the order could not be placed (e.g. not enough money)
    public bool PlaceBuyOrder(MarketOrder o)
    {
        float unitPrice = GetPrice(o.Goods.GetId());

        // Requestor cannot afford his order
        if (o.Requestor.Money < o.Goods.Quantity * unitPrice)
            return false;
        
        if (!AttemptTransact(o))
        {
            List<MarketOrder> orders = GetBuyOrders(o.Goods.GetId());

            // If there are no orders for the good, add it
            if (orders == null)
            {
                orders = new();
                BuyOrders[o.Goods.GetId()] = orders;
            }
            orders.Add(o);
        }
        return true;
    }

    public void CancelBuyOrder(Person p, int goodsId)
    {
        List<MarketOrder> orders = GetBuyOrders(goodsId);
        orders.RemoveAll(order => order.Requestor == p);
    }

    // Get the price of one unit of the given good, plus taxes
    public float GetPrice(int goodsId)
    {
        return Prices[goodsId] * (1 + Kingdom.TaxRate);
    }

    // Get just the tax amount from the total price including tax
    public float GetTax(float price)
    {
        return price - (price / (1f + Kingdom.TaxRate));
    }

    // Tries to execute the order first, then adds it if note complete
    // Returns false if the order could not be added (currently always added)
    public bool PlaceSellOrder(MarketOrder o)
    {
        if (!AttemptTransact(o))
        {
            List<MarketOrder> orders = GetSellOrders(o.Goods.GetId());

            // Remove the goods from the seller's inventory
            o.Requestor.PersonalStockpile.Take(o.Goods);

            // If there are no orders for the good, add it
            if (orders == null)
            {
                orders = new();
                SellOrders[o.Goods.GetId()] = orders;
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
        orders.RemoveAll(order => order.Requestor == p);
    }

    public float GetQuantitySold(int id)
    {
        if (!SellOrders.ContainsKey(id))
            return 0f;

        float sum = 0f;
        List<MarketOrder> orders = SellOrders[id];
        foreach (MarketOrder order in orders)
            sum += order.Goods.Quantity;
        return sum;
    }

    public MarketOrder CheapestFood(int hunger)
    {
        // First time lookup
        if (CheapestFoodId == 0)
            return CheapestFoodUncached(hunger);

        // Cheapest food out of stock
        float quantity = GetQuantitySold(CheapestFoodId);
        if (quantity == 0)
            return CheapestFoodUncached(hunger);

        // Buy enough to satisfy hunger, or whatever is available if there's not enough
        MarketOrder order = new();
        order.Buying = true;
        order.Goods = Goods.FromId(CheapestFoodId);

        int satiation = GoodsInfo.GetSatiation(order.Goods);
        if (satiation == 0f)
        {
            Goods uncooked = new Goods(order.Goods);
            satiation = GoodsInfo.GetSatiation(uncooked.Cook());
        }

        order.Goods.Quantity = Math.Min(hunger / satiation, quantity);

        return order;
    }

    // Returns the cheapest food being sold in sufficient quantity to fulfill hunger
    // Will also buy raw meat by checking the price of the raw version against the satiation of the cooked version
    public MarketOrder CheapestFoodUncached(int hunger)
    {
        int minId = -1;
        float reqQuantity = 0f;
        float min = 9999f;
        Goods lookup = new();
        lookup.Type = GoodsType.FOOD_PROCESSED;

        GoodsType[] goodsTypes = { 
            GoodsType.FOOD_PROCESSED, GoodsType.FOOD_ANIMAL, GoodsType.FOOD_PLANT, GoodsType.RAW_MEAT };
        Type[] subTypeEnums = new Type[] { 
            typeof(Goods.ProcessedFood), typeof(Goods.FoodAnimal), typeof(Goods.FoodPlant), typeof(Goods.RawMeat) };

        for (int i = 0; i < goodsTypes.Length; i++)
        {
            foreach (int subType in Enum.GetValues(subTypeEnums[i]))
            {
                lookup.Type = goodsTypes[i];
                lookup.SubType = subType;
                int id = lookup.GetId();

                if (lookup.IsCookable())
                    lookup.Cook();

                // Skip goods not being sold
                float quantity = GetQuantitySold(id);
                int satiation = GoodsInfo.GetSatiation(lookup);

                // Not enough, skip this one
                if (quantity * satiation < hunger)
                    continue;

                // Check if most efficient satiation per cost
                float price = Prices[id];
                if (satiation / price < min)
                {
                    min = satiation / price;
                    minId = id;
                    reqQuantity = hunger / satiation;
                }
            }
        }

        if (minId == -1)
            return null;

        // Cache the id of the cheapest food
        CheapestFoodId = minId;

        MarketOrder buyOrder = new();
        buyOrder.Goods = Goods.FromId(minId, reqQuantity);
        buyOrder.Buying = true;

        return buyOrder;
    }
}