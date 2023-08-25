public class ProductionRequirements
{
    public SkillLevel SkillRequirement;
    public ToolRequirement ToolRequirement;
    public ToolMaterial ToolTypeRequirement;
    public TileType TileRequirement;
    public BuildingType BuildingRequirement;
    public BuildingSubType BuildingSubTypeRequirement;
    public GoodsRequirement GoodsRequirement;

    public ProductionRequirements(
        SkillLevel levelRequirement = null, 
        ToolRequirement toolRequirement = null,
        TileType tileRequirement = TileType.NONE,
        BuildingType buildingRequirement = BuildingType.NONE,
        BuildingSubType buildingSubTypeRequirement = BuildingSubType.NONE,
        GoodsRequirement goodsRequirement = null)
    {
        SkillRequirement = levelRequirement;
        ToolRequirement = toolRequirement;
        TileRequirement = tileRequirement;
        BuildingRequirement = buildingRequirement;
        BuildingSubTypeRequirement = buildingSubTypeRequirement;
        GoodsRequirement = goodsRequirement;
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