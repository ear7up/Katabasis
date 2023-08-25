// 1:1 corresponds with Skill class
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
    // ...
}

namespace ProfessionExtension
{
    public static class Extensions
    {
        public static string Describe(this ProfessionType profession)
        {
            return Globals.Title(profession.ToString());
        }
    }
}