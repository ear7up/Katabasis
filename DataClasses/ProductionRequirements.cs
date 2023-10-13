public class ProductionRequirements
{
    public SkillLevel SkillRequirement;
    public ToolRequirement ToolRequirement;
    public TileType TileRequirement;
    public BuildingType BuildingRequirement;
    public BuildingSubType BuildingSubTypeRequirement;
    public GoodsRequirement GoodsRequirement;
    public SecondaryGoods Secondary;

    public ProductionRequirements(
        SkillLevel levelRequirement = null, 
        ToolRequirement toolRequirement = null,
        TileType tileRequirement = TileType.NONE,
        BuildingType buildingRequirement = BuildingType.NONE,
        BuildingSubType buildingSubTypeRequirement = BuildingSubType.NONE,
        GoodsRequirement goodsRequirement = null,
        SecondaryGoods secondary = null)
    {
        SkillRequirement = levelRequirement;
        ToolRequirement = toolRequirement;
        TileRequirement = tileRequirement;
        BuildingRequirement = buildingRequirement;
        BuildingSubTypeRequirement = buildingSubTypeRequirement;
        GoodsRequirement = goodsRequirement;
        Secondary = secondary;
    }

    public override string ToString()
    {
        return $"ProductionRequirements(\n" +
            $"  {SkillRequirement}\n" + 
            $"  {ToolRequirement}\n" +
            $"  {TileRequirement}\n" +
            $"  {BuildingRequirement}\n" +
            $"  {GoodsRequirement}\n" +
            ")";
    }
}

public class SecondaryGoods
{
    public int Id;
    public float Ratio;

    public SecondaryGoods(int id, float ratio)
    {
        Id = id;
        Ratio = ratio;
    }
}