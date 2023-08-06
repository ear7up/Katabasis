public enum Skill
{
    FARMING = 0,
    BUILDING,
    HUNTING,
    FISHING,
    COOKING,
    CRAFTING,
    SMITHING,
    FIGHTING,
    MINING,
    FORESTRY,
    NONE
}

public class SkillLevel
{
    public const float INCREASE_CHANCE = 0.1f;

    public Skill skill;
    public int level;
    public SkillLevel(Skill skill, int level)
    {
        this.skill = skill;
        this.level = level;
    }

    public override string ToString()
    {
        return $"{skill}:{level}";
    }
}