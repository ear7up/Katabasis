using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class Player
{
    // Serialized content
    public Person Person { get; set; }
    public Kingdom Kingdom { get; set; }
    public Dictionary<int, bool> UnlockedPlants { get; set; }
    
    public Player()
    {
        Person = new();
        Person.Money = 1000f;

        // By default, players will be able to farm wheat, barley, and flax
        // villagers may also gather wild edible plants from tiles with vegetation
        UnlockedPlants = new();
        UnlockPlant(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.WHEAT);
        UnlockPlant(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.BARLEY);
        UnlockPlant(GoodsType.FOOD_PLANT, (int)Goods.FoodPlant.WILD_EDIBLE);
        UnlockPlant(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.FLAX);
    }

    public static Player Create(Tile startTile)
    {
        Player player = new();
        player.SetAttributes(startTile);
        return player;
    }

    public void SetAttributes(Tile startTile)
    {
        Kingdom = Kingdom.Create(this, startTile);

        Person.PersonalStockpile = new();
    }

    public void UnlockPlant(int id)
    {
        UnlockPlant((GoodsType)Goods.TypeFromId(id), Goods.SubTypeFromid(id));
    }

    public void UnlockPlant(GoodsType type, int subType)
    {
        UnlockedPlants[Goods.GetId(type, subType, 0)] = true;
    }

    public bool IsPlantUnlocked(int id)
    {
        return UnlockedPlants.ContainsKey(id);
    }

    public bool IsPlantUnlocked(GoodsType type, int subType)
    {
        return UnlockedPlants.ContainsKey(Goods.GetId(type, subType, 0));
    }

    public bool CanProduce(Goods goods)
    {
        // Plant type not unlocked, cannot be produced
        if (goods.Type == GoodsType.FOOD_PLANT && !IsPlantUnlocked(goods.Type, goods.SubType))
            return false;
        return true;
    }

    public void Update()
    {
        Kingdom.Update();
        Person.PersonalStockpile.DepositInto(Kingdom.Treasury);
    }

    public void DailyUpdate()
    {
        Kingdom.DailyUpdate();
    }
}