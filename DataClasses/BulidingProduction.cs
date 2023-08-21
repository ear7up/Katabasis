using System.Collections;

public class BuildingProduction
{
    public static Hashtable Requirements;

    public static void Init()
    {
        Requirements = new();
        
        // building: cedar + saw + vegetation -> farm
        Requirements.Add(BuildingType.FARM, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 40)),
            toolRequirement: Goods.Tool.SAW,
            tileRequirement: TileType.VEGETATION,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: cedar + saw + animals -> ranch
        Requirements.Add(BuildingType.RANCH, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 40)),
            toolRequirement: Goods.Tool.SAW,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: stone + furnace -> forge
        Requirements.Add(BuildingType.FORGE, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60)),
            toolRequirement: Goods.Tool.FURNACE,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 20)));

        // building: [bricks or wood] -> house
        Requirements.Add(BuildingType.HOUSE, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 30),
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 30),
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.EBONY, 30)),
            toolRequirement: Goods.Tool.SAW,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 10)));

        // building: cedar + saw -> lumbermill
        Requirements.Add(BuildingType.LUMBERMILL, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 30)),
            toolRequirement: Goods.Tool.SAW,
            tileRequirement: TileType.FOREST,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // building: saw + [cedar AND linen] -> market
        Requirements.Add(BuildingType.MARKET, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 50),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LINEN, 20), 
                and: true),
            toolRequirement: Goods.Tool.SAW,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // building: cedar + shovel -> mine
        Requirements.Add(BuildingType.MINE, new ProductionRequirements(
            //goodsRequirement: new GoodsRequirement(
            //    new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 30)),
            toolRequirement: Goods.Tool.SHOVEL,
            tileRequirement: TileType.HILLS,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 30)));

        // building: stone + furnace -> smithy
        Requirements.Add(BuildingType.SMITHY, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60)),
            toolRequirement: Goods.Tool.FURNACE,
            levelRequirement: SkillLevel.Create(Skill.BUILDING, 20)));
    }
}
