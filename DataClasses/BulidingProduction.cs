using System.Collections;

// Every buliding MUST require goods to be built
public class BuildingProduction
{
    // Building ID -> ProductionRequirements
    public static Hashtable Requirements;

    public static void Init()
    {
        Requirements = new();
        
        // building: wood + saw + vegetation -> farm
        Requirements.Add(Building.GetId(BuildingType.FARM), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 40)),
            toolRequirement: new ToolRequirement(Goods.Tool.SAW),
            tileRequirement: TileType.VEGETATION,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: wood + saw + vegetation -> farm river
        Requirements.Add(Building.GetId(BuildingType.FARM_RIVER), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 40)),
            toolRequirement: new ToolRequirement(Goods.Tool.SAW),
            tileRequirement: TileType.VEGETATION,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: wood + saw + animals -> ranch
        Requirements.Add(Building.GetId(BuildingType.RANCH), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 40)),
            toolRequirement: new ToolRequirement(Goods.Tool.SAW),
            tileRequirement: TileType.ANIMAL,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: stone + furnace -> forge
        Requirements.Add(Building.GetId(BuildingType.FORGE), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60)),
            toolRequirement: new ToolRequirement(Goods.Tool.FURNACE),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 20)));

        // building: brick -> house (brick)
        Requirements.Add(Building.GetId(BuildingType.HOUSE, BuildingSubType.BRICK), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 30)),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: wood -> house (wood)
        Requirements.Add(Building.GetId(BuildingType.HOUSE, BuildingSubType.WOOD), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 30)),
            toolRequirement: new ToolRequirement(Goods.Tool.SAW),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: wood + saw -> lumbermill
        Requirements.Add(Building.GetId(BuildingType.LUMBERMILL), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 30)),
            toolRequirement: new ToolRequirement(Goods.Tool.SAW),
            tileRequirement: TileType.FOREST,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // Tannery building made of bricks, with clay vats for holding the hides and liquid
        // building: bricks + clay + hammer -> tannery
        Requirements.Add(Building.GetId(BuildingType.TANNERY), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 30),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.CLAY, 40),
                and: true),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // Tavern building made of bricks
        // building: bricks + hammer -> tavern
        Requirements.Add(Building.GetId(BuildingType.TAVERN), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 60)),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // building: saw + [wood AND linen] -> market
        Requirements.Add(Building.GetId(BuildingType.MARKET), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 50),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LINEN, 20), 
                and: true),
            toolRequirement: new ToolRequirement(Goods.Tool.SAW),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // building: wood + shovel -> mine
        Requirements.Add(Building.GetId(BuildingType.MINE), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 30)),
            toolRequirement: new ToolRequirement(Goods.Tool.SHOVEL),
            tileRequirement: TileType.HILLS,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // building: stone + furnace -> smithy
        Requirements.Add(Building.GetId(BuildingType.SMITHY), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60),
                new Goods(GoodsType.TOOL, (int)Goods.Tool.FURNACE, 1),
                and: true),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 20)));

        // building: bricks -> oven
        Requirements.Add(Building.GetId(BuildingType.OVEN), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 20)),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: sandstone + chisel -> pyramid
        Requirements.Add(Building.GetId(BuildingType.PYRAMID), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.SANDSTONE, 20/*00*/)),
            toolRequirement: new ToolRequirement(Goods.Tool.CHISEL),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 40)));

        // building: stone -> temple
        Requirements.Add(Building.GetId(BuildingType.TEMPLE), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60)),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 20)));

        // building: stone + wood -> granary
        Requirements.Add(Building.GetId(BuildingType.GRANARY), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 20),
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 50),
                and: true),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 20)));

        // building: bricks + wood -> barracks
        Requirements.Add(Building.GetId(BuildingType.BARRACKS), new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 80),
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.WOOD, 50),
                and: true),
            toolRequirement: new ToolRequirement(Goods.Tool.HAMMER),
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));
    }

    public static ProductionRequirements GetRequirements(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
    {
        return (ProductionRequirements)Requirements[Building.GetId(buildingType, subType)];
    }

    public static ProductionRequirements GetRequirements(int id)
    {
        return (ProductionRequirements)Requirements[id];
    }
}
