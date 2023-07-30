using System.Collections.Generic;

public class Task
{
    // Move: go to a location
    // Go Home: Move to Home (daily)
    // Haul: Move to pos A, Take goods, Move to point B
    // Cook: Go Home, 
    // Buy: Move to a market, find seller, Haul goods Home
    // Sell: Haul goods to a market, find buyer, otherwise Haul goods Home
    // RequestGoods: add Haul task to hauling work pool
    // Work:
    //      Move to Work location
    //      If neeeded Goods are present
    //          consume goods, produce goods

    public List<SkillLevel> skillsNeeded;

    public Task()
    {

    }
}