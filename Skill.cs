using System;

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
    public const float INCREASE_CHANCE = 0.5f;

    public Skill skill { get; set; }
    public int level { get; set; }
    public int startLevel { get; set; }
    
    public SkillLevel(Skill skill, int level)
    {
        this.skill = skill;
        this.level = level;
        this.startLevel = level;
    }

    public override string ToString()
    {
        return $"{skill}:{level}";
    }

    public void GainExperience(float xp)
    {
        float r = Globals.Rand.NextFloat(0f, 1f);

        // E.g. if making 20 units of 1xp goods, chance is 10% + 20% = 30% to gain a level
        if (r < SkillLevel.INCREASE_CHANCE + (xp / 100))
            level = Math.Min(level + 1, 100);
    }
}