using System;

public class MarketTests
{
    public static void RunTests()
    {
        // TEMPORARY
        Person seller1 = Person.CreatePerson(Vector2.Zero, null);
        Person seller2 = Person.CreatePerson(Vector2.Zero, null);
        Person seller3 = Person.CreatePerson(Vector2.Zero, null);
        Goods iron1 = new(GoodsType.SMITHED, (int)Goods.Smithed.IRON, 10);
        Goods iron2 = new(GoodsType.SMITHED, (int)Goods.Smithed.IRON, 20);
        Goods iron3 = new(GoodsType.SMITHED, (int)Goods.Smithed.IRON, 40);

        // Iron is selling for 1.3 - 1.5 each
        Market market = new(null, null);
        market.PlaceSellOrder(new(seller1, false, iron1, 1.5f));
        market.PlaceSellOrder(new(seller2, false, iron2, 1.4f));
        market.PlaceSellOrder(new(seller3, false, iron3, 1.3f));

        // Buyer is willing to pay 2.0
        Person buyer = Person.CreatePerson(Vector2.Zero, null);
        buyer.Money = 100; 
        Goods iron0 = new(GoodsType.MATERIAL_NATURAL, (int)Goods.Smithed.IRON, 50);
        market.PlaceBuyOrder(new(buyer, true, iron0, 2.0f));

        // Seller3 sells out 40/40 @ 1.3 ea = 52
        // Seller 2 sells 10/20 @ 1.4 ea = 14 (still selling 10)
        // Seller 3 sells 0//10 @ 1.5 ea = 0 (still selling 10)
        Console.WriteLine(market.ToString());
        Console.WriteLine($"Buyer money leftover: {buyer.Money}");
        Console.WriteLine($"Seller1 money: {seller1.Money}"); 
        Console.WriteLine($"Seller2 money: {seller2.Money}"); 
        Console.WriteLine($"Seller3 money: {seller3.Money}"); 
    }
}