using System;
using System.Collections;
using System.Collections.Generic;

public class MarketOrder
{
    public Person requestor;
    public bool buying;
    public Goods goods;
    public float unitPrice;

    public MarketOrder(Person requestor, bool buying, Goods goods, float unitPrice)
    {
        this.requestor = requestor;
        this.buying = buying;
        this.goods = goods;
        this.unitPrice = unitPrice;
    }

    public override string ToString()
    {
        return $"MarketOrder(requestor={requestor}, buying={buying}, goods={goods}, unitPrice={unitPrice})";
    }
}

public class Market : Building
{
    // BuyOrders: Goods.ID -> List<MarketOrder> (sorted by unitPrice DESC)
    private Hashtable BuyOrders;
    // SellOrders: Goods.ID -> List<MarketOrder> (sorted by unitPrice ASC)
    private Hashtable SellOrders;

    public Market(Tile tile, Sprite sprite) : base(tile, sprite)
    {
        BuyOrders = new();
        SellOrders = new();
    }

    public override string ToString()
    {
        string buyOrders = "";
        foreach (List<MarketOrder> list in BuyOrders.Values)
        {
            foreach (MarketOrder buy in list)
            {
                buyOrders += "    " + buy.ToString() + "\n";
            }
        }

        string sellOrders = "";
        foreach (List<MarketOrder> list in SellOrders.Values)
        {
            foreach (MarketOrder sell in list)
            {
                sellOrders += "    " + sell.ToString() + "\n";
            }
        }

        return $"Market(\n  buying=\n{buyOrders}\n  selling=\n{sellOrders})";
    }

    // Returns true if the transaction needs to be placed (could not be completed)
    private bool AttemptTransact(MarketOrder o)
    {
        Hashtable orders = SellOrders;
        if (!o.buying)
        {
            orders = BuyOrders;
        }

        List<MarketOrder> trades = (List<MarketOrder>)orders[o.goods.GetId()];
        bool completedOrders = false;

        // Quit if no one is buying/selling the good being requested
        if (trades == null || trades.Count == 0)
        {
            return true;
        }
        
        // Try to buy order from the cheapest sellers
        for (int i = 0; o.goods.Quantity > 0 && i < trades.Count; i++)
        {
            MarketOrder trade = trades[i];

            // If buying and the cheapest seller's price is too high, quit (sellers sorted by price asc)
            if (o.buying && trade.unitPrice > o.unitPrice)
            {
                //Console.WriteLine($"Too expensive: {trade}");
                break;
            }
            // If selling and the highest buyer's price is too low, quit (buyers sorted by price desc)
            else if (!o.buying && trade.unitPrice < o.unitPrice)
            {
                //Console.WriteLine($"Too cheap: {trade}");
                break;
            }
            else
            {
                // Can't buy or sell more than each individual trader is offering
                int sale_quantity = MathHelper.Min(o.goods.Quantity, trade.goods.Quantity);
                int cost = (int)(sale_quantity * trade.unitPrice);

                //Console.WriteLine($"Executing trade: {trade}");
                //Console.WriteLine($"Sale quantity: {sale_quantity}");

                // Tranfer ownership of goods and money 
                o.goods.Quantity -= sale_quantity;
                trade.goods.Quantity -= sale_quantity;

                // Remember, the buyer already paid to place the order
                if (o.buying)
                {    
                    trade.requestor.Money += cost;
                }
                else
                {
                    o.requestor.Money += cost;
                }

                // All of the goods in this order have been bought/sold, flag that we need to remove some trades
                if (trade.goods.Quantity == 0)
                {
                    completedOrders = true;
                }
            }
        }

        // Remove all fulfilled orders
        if (completedOrders)
        {
            trades.RemoveAll(s => s.goods.Quantity == 0);
        }

        // If not all goods were bought/sold at the requested price, return false to indicate the order must be placed
        return o.goods.Quantity != 0;
    }

    public int PlaceBuyOrder(MarketOrder o)
    {
        // Requestor cannot afford his order
        if (o.requestor.Money < o.goods.Quantity * o.unitPrice)
        {
            return -1;
        }

        o.requestor.Money -= (int)(o.goods.Quantity * o.unitPrice);
        
        if (AttemptTransact(o))
        {
            List<MarketOrder> orders = (List<MarketOrder>)BuyOrders[o.goods.GetId()];

            // If there are no orders for the good, add it
            if (orders == null)
            {
                orders = new();
                BuyOrders[o.goods.GetId()] = orders;
                orders.Add(o);
            }
            else
            {
                for (int i = 0; i < orders.Count + 1; i++)
                {
                    // Insert in order by unit price, priciest buyers first
                    if (i == orders.Count || o.unitPrice > orders[i].unitPrice)
                    {
                        orders.Insert(i, o);
                        return i;
                    }
                }
            }
        }
        return -1;
    }

    public void CancelBuyOrder(Person p, Goods g, int orderId)
    {
        List<MarketOrder> orders = (List<MarketOrder>)BuyOrders[g.GetId()];
        if (orders != null && orderId >= 0 && orderId < orders.Count && orders[orderId].requestor == p)
        {
            // Give the person his money back and remove the order
            p.Money += (int)(orders[orderId].goods.Quantity * orders[orderId].unitPrice);
            orders.RemoveAt(orderId);
        }
    }

    // Check if anyone is selling a particular type of goods
    public float CheckPrice(Goods g)
    {
        List<MarketOrder> sellers = (List<MarketOrder>)SellOrders[g.GetId()];
        int quantity = g.Quantity;
        float price = 0f;
        if (sellers == null || sellers.Count == 0)
            return 0f;
        
        foreach (MarketOrder seller in sellers)
        {
            int saleQuantity = Math.Min(quantity, seller.goods.Quantity);
            price += saleQuantity * seller.unitPrice;
            quantity -= saleQuantity;
            if (quantity <= 0)
                break;
        }
        
        // Only return price if all can be bought
        if (quantity > 0)
            return 0f;
        return price;
    }

    public int PlaceSellOrder(MarketOrder o)
    {
        if (AttemptTransact(o))
        {
            List<MarketOrder> orders = (List<MarketOrder>)SellOrders[o.goods.GetId()];

            // If there are no orders for the good, add it
            if (orders == null)
            {
                orders = new();
                SellOrders[o.goods.GetId()] = orders;
                orders.Add(o);
            }
            else
            {
                for (int i = 0; i <= orders.Count; i++)
                {
                    // Insert in order by unit price, cheapest sellers first
                    if (i == orders.Count || o.unitPrice < orders[i].unitPrice)
                    {
                        orders.Insert(i, o);
                        return i;
                    }
                }
            }
        }
        return -1;
    }

    public void CancelSellOrder(Person p, Goods g, int orderId)
    {
        List<MarketOrder> orders = (List<MarketOrder>)SellOrders[g.GetId()];
        if (orders != null && orderId >= 0 && orderId < orders.Count && orders[orderId].requestor == p)
        {
            // Give the seller back his goods
            ((Goods)p.PersonalStockpile[g.GetId()]).Quantity += orders[orderId].goods.Quantity;
            orders.RemoveAt(orderId);
        }
    }
}