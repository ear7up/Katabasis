public enum DietReq
{
    FAT,
    VITAMINA,
    VITAMINC,
    IRON,
    CALCIUM
}

public class Health
{
    public const int MIN = 0;
    public const int MAX = 100;

    // Mood should probably be an object with an array of modifiers
    public int Mood { get; set; }
    public int Fat { get; set; }
    public int VitaminA { get; set; }
    public int VitaminC { get; set; }
    public int Iron { get; set; }
    public int Calcium { get; set; }

    public Health()
    {
        Mood = 50;
        Fat = 100;
        VitaminA = 100;
        VitaminC = 100;
        Iron = 100;
        Calcium = 100;
    }

    public void DailyUpdate()
    {
        int change = -10;
        Fat = MathHelper.Clamp(Fat + change, MIN, MAX);
        VitaminA = MathHelper.Clamp(VitaminA + change, MIN, MAX);
        VitaminC = MathHelper.Clamp(VitaminC + change, MIN, MAX);
        Iron = MathHelper.Clamp(Iron + change, MIN, MAX);
        Calcium = MathHelper.Clamp(Calcium + change, MIN, MAX);

        float average = (Fat + VitaminA + VitaminC + Iron + Calcium) / 5f;
        Mood = MathHelper.Clamp(Mood + ((average > 0.5f) ? +1 : -1), MIN, MAX);
    }

    public void EatFood(int goodsId)
    {
        FoodInfo info = FoodInfo.Get(goodsId);
        if (info == null)
            return;
        Fat = MathHelper.Clamp(Fat + info.Fat, MIN, MAX);
        VitaminA = MathHelper.Clamp(VitaminA + info.VitaminA * 3, MIN, MAX);
        VitaminC = MathHelper.Clamp(VitaminC + info.VitaminC * 2, MIN, MAX);
        Iron = MathHelper.Clamp(Iron + info.Iron, MIN, MAX);
        Calcium = MathHelper.Clamp(Calcium+ info.Calcium * 4, MIN, MAX);
        Mood = MathHelper.Clamp(Mood + info.Mood, MIN, MAX);
    }

    public DietReq GetDietReq()
    {
        int min = Fat;
        DietReq req = DietReq.FAT;
        if (VitaminA < min)
        {
            min = VitaminA;
            req = DietReq.VITAMINA;
        }

        if (VitaminC < min)
        {
            min = VitaminC;
            req = DietReq.VITAMINC;
        }

        if (Iron < min)
        {
            min = Iron;
            req = DietReq.IRON;
        }

        if (Calcium < min)
        {
            min = Calcium;
            req = DietReq.CALCIUM;
        }

        return req;
    }
}