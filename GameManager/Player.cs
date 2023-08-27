using System;
using System.IO;
using System.Text.Json;

public class Player
{
    // Serialized content
    public Kingdom Kingdom { get; set; }
    public bool[] UnlockedPlants { get; set; }
    
    public Player()
    {
        int num_plants = Enum.GetValues(typeof(Goods.FoodPlant)).Length;
        UnlockedPlants = new bool[num_plants];
        UnlockPlant(Goods.FoodPlant.WHEAT);
        UnlockPlant(Goods.FoodPlant.BARLEY);
        UnlockPlant(Goods.FoodPlant.WILD_EDIBLE);
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
    }

    public void UnlockPlant(Goods.FoodPlant plant)
    {
        UnlockedPlants[(int)plant] = true;
    }

    public bool IsPlantUnlocked(Goods.FoodPlant plant)
    {
        return UnlockedPlants[(int)plant];
    }

    public bool CanProduce(Goods goods)
    {
        // Plant type not unlocked, cannot be produced
        if (goods.Type == GoodsType.FOOD_PLANT && !IsPlantUnlocked((Goods.FoodPlant)goods.SubType))
            return false;
        return true;
    }

    public void Update()
    {
        Kingdom.Update();
    }

    public void DailyUpdate()
    {
        Kingdom.DailyUpdate();
    }
}