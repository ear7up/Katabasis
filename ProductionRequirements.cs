public class ProductionRequirements
{
    public SkillLevel SkillRequirement;
    public Goods.Tool ToolRequirement;
    public TileType TileRequirement;
    public BuildingType BuildingRequirement;
    public GoodsRequirement GoodsRequirement;

    public ProductionRequirements(
        SkillLevel levelRequirement = null, 
        Goods.Tool toolRequirement = Goods.Tool.NONE,
        TileType tileRequirement = TileType.NONE,
        BuildingType buildingRequirement = BuildingType.NONE,
        GoodsRequirement goodsRequirement = null)
    {
        SkillRequirement = levelRequirement;
        ToolRequirement = toolRequirement;
        TileRequirement = tileRequirement;
        BuildingRequirement = buildingRequirement;
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