using System.Collections.Generic;

public class FoodInfo
{
    public static Dictionary<int, FoodInfo> Data;

    // % daily value per 100 grams
    public int Fat;
    public int VitaminA;
    public int VitaminC;
    public int Iron;
    public int Calcium;

    // Some foods improve mood when eaten
    public int Mood;

    // TODO: perhaps satiation should be moved here, will require updating places it's currently used
    public FoodInfo(int fat = 0, int vitaminA = 0, int vitaminC = 0, int iron = 0, int calcium = 0, int mood = 0)
    {
        Fat = fat;
        VitaminA = vitaminA;
        VitaminC = vitaminC;
        Iron = iron;
        Calcium = calcium;
        Mood = mood;
    }

    public static int Id(GoodsType type, int subType)
    {
        return Goods.GetId(type, subType, 0);
    }

    // Get the dietary content of the food associated with the goods id
    public static int GetContent(int id, DietReq req)
    {
        FoodInfo info = Get(id);
        if (info == null)
            return 0;
        return req switch {
            DietReq.CALCIUM => info.Calcium,
            DietReq.FAT => info.Fat,
            DietReq.IRON => info.Iron,
            DietReq.VITAMINA => info.VitaminA,
            DietReq.VITAMINC => info.VitaminC,
            _ => 0
        };
    }

    public static void Init()
    {
        Data = new();

        GoodsType t = GoodsType.FOOD_PROCESSED;
        Data[Id(t, (int)Goods.ProcessedFood.BEER)] = new FoodInfo(mood: 3);
        Data[Id(t, (int)Goods.ProcessedFood.BREAD)] = new FoodInfo(fat: 1, iron: 5, calcium: 2, mood: 1);
        Data[Id(t, (int)Goods.ProcessedFood.SALTED_MEAT)] = new FoodInfo(fat: 30, iron: 18, calcium: 1, mood: 1);
        Data[Id(t, (int)Goods.ProcessedFood.WINE)] = new FoodInfo(iron: 4, calcium: 1, mood: 3);

        t = GoodsType.FOOD_ANIMAL;
        Data[Id(t, (int)Goods.FoodAnimal.BEEF)] = new FoodInfo(fat: 30, iron: 18, calcium: 1);
        Data[Id(t, (int)Goods.FoodAnimal.DUCK)] = new FoodInfo(fat: 9, vitaminA: 3, vitaminC: 7, iron: 15, calcium: 1);
        Data[Id(t, (int)Goods.FoodAnimal.EGGS)] = new FoodInfo(fat: 6, vitaminA: 8, iron: 5, calcium: 2);
        Data[Id(t, (int)Goods.FoodAnimal.FISH)] = new FoodInfo(fat: 5, vitaminA: 6, vitaminC: 2, iron: 2, calcium: 1);
        Data[Id(t, (int)Goods.FoodAnimal.FOWL)] = new FoodInfo(fat: 14, vitaminA: 1, iron: 14, calcium: 1); // goose
        Data[Id(t, (int)Goods.FoodAnimal.GAME)] = new FoodInfo(fat: 4, iron: 25, calcium: 1); // deer
        Data[Id(t, (int)Goods.FoodAnimal.GOOSE)] = new FoodInfo(fat: 14, vitaminA: 1, iron: 14, calcium: 1);
        Data[Id(t, (int)Goods.FoodAnimal.HONEY)] = new FoodInfo(mood: 5);
        Data[Id(t, (int)Goods.FoodAnimal.MILK)] = new FoodInfo(fat: 10, vitaminA: 8, calcium: 21);
        Data[Id(t, (int)Goods.FoodAnimal.MUTTON)] = new FoodInfo(fat: 23, iron: 9, calcium: 1);
        Data[Id(t, (int)Goods.FoodAnimal.PORK)] = new FoodInfo(fat: 8, vitaminC: 1, iron: 8);
        Data[Id(t, (int)Goods.FoodAnimal.QUAIL)] = new FoodInfo(fat: 14, vitaminA: 6, vitaminC: 2, iron: 19);

        t = GoodsType.FOOD_PLANT;
        Data[Id(t, (int)Goods.FoodPlant.BARLEY)] = new FoodInfo(fat: 1, iron: 12, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.CELERY)] = new FoodInfo(vitaminA: 1, vitaminC: 1, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.CHICKPEAS)] = new FoodInfo(fat: 3, iron: 6, calcium: 2);
        Data[Id(t, (int)Goods.FoodPlant.CUCUMBER)] = new FoodInfo(vitaminC: 2, iron: 1, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.GARLIC)] = new FoodInfo(vitaminC: 1, mood: 1);
        Data[Id(t, (int)Goods.FoodPlant.GRAPES)] = new FoodInfo(vitaminA: 1, vitaminC: 19, iron: 3, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.LEEK)] = new FoodInfo(vitaminA: 8, vitaminC: 12, iron: 10, calcium: 4);
        Data[Id(t, (int)Goods.FoodPlant.LENTILS)] = new FoodInfo(fat: 17, vitaminC: 3, iron: 34, calcium: 3);
        Data[Id(t, (int)Goods.FoodPlant.LETTUCE)] = new FoodInfo(vitaminA: 2, vitaminC: 2, iron: 1, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.MELON)] = new FoodInfo(vitaminA: 5, vitaminC: 14, iron: 2, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.NUTS)] = new FoodInfo(fat: 19, iron: 2, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.OLIVE_OIL)] = new FoodInfo(fat: 17, mood: 3);
        Data[Id(t, (int)Goods.FoodPlant.ONION)] = new FoodInfo(vitaminC: 11, iron: 2, calcium: 3, mood: 1);
        Data[Id(t, (int)Goods.FoodPlant.PEAS)] = new FoodInfo(fat: 1, vitaminA: 6, vitaminC: 25, iron: 14, calcium: 3);
        Data[Id(t, (int)Goods.FoodPlant.RADISH)] = new FoodInfo(vitaminC: 19, iron: 2, calcium: 2);
        Data[Id(t, (int)Goods.FoodPlant.SCALLIONS)] = new FoodInfo(vitaminA: 6, vitaminC: 21, iron: 8, calcium: 6);
        Data[Id(t, (int)Goods.FoodPlant.SQUASH)] = new FoodInfo(vitaminA: 1, vitaminC: 21, iron: 2, calcium: 1);
        Data[Id(t, (int)Goods.FoodPlant.TURNIP)] = new FoodInfo(fat: 5, vitaminA: 4, vitaminC: 20, iron: 2, calcium: 4);
        Data[Id(t, (int)Goods.FoodPlant.WILD_EDIBLE)] = new FoodInfo(mood: -1);
    }

    public static FoodInfo Get(int goodsId)
    {
        if (!Data.ContainsKey(goodsId))
            return null;
        return Data[goodsId];
    }
}