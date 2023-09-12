public class Health
{
    public const int MIN = 0;
    public const int MAX = 0;

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
        int change = -100;
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
        Fat += info.Fat;
        VitaminA += info.VitaminA;
        VitaminC += info.VitaminC;
        Iron += info.Iron;
        Calcium += info.Calcium;
        Mood = MathHelper.Clamp(Mood + info.Mood, MIN, MAX);
    }
}