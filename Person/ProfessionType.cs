// 1:1 corresponds with Skill class
using System.ComponentModel;

public enum ProfessionType
{
    // Begin 1:1 skill correspondence
    FARMER,
    MASON,
    HUNTER,
    FISHERMAN,
    COOK,
    CRAFTSMAN,
    SMITH,
    MERCENARY,
    MINER,
    WOODSMAN,
    NONE,

    // Additional professions not tied to a single skill
    SOLDIER,
    SCRIBE
}

namespace ProfessionExtension
{
    public static class Extensions
    {
        public static string Describe(this ProfessionType profession)
        {
            return Globals.Title(profession.ToString());
        }

        public static Goods.Tool GetTool(this ProfessionType professionType)
        {
            return professionType switch
            {
                ProfessionType.FARMER => Goods.Tool.HOE,
                ProfessionType.FISHERMAN => Goods.Tool.FISHING_NET,
                ProfessionType.HUNTER => Goods.Tool.SPEAR,
                ProfessionType.MASON => Goods.Tool.HAMMER,
                ProfessionType.MINER => Goods.Tool.PICKAXE,
                ProfessionType.SMITH => Goods.Tool.HAMMER,
                ProfessionType.WOODSMAN => Goods.Tool.AXE,
                _ => Goods.Tool.NONE,
            };
        }
    }
}