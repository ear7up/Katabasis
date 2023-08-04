using System.Collections;

public class BulidingProduction
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
            levelRequirement: new SkillLevel(Skill.BUILDING, 10)));

        // building: cedar + saw + animals -> ranch
        Requirements.Add(BuildingType.RANCH, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 40)),
            toolRequirement: Goods.Tool.SAW,
            tileRequirement: TileType.ANIMAL,
            levelRequirement: new SkillLevel(Skill.BUILDING, 10)));

        // building: stone + furnace -> forge
        Requirements.Add(BuildingType.FORGE, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60)),
            toolRequirement: Goods.Tool.FURNACE,
            levelRequirement: new SkillLevel(Skill.BUILDING, 20)));

        // building: cedar + saw -> wood house
        Requirements.Add(BuildingType.WOOD_HOUSE, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 30)),
            toolRequirement: Goods.Tool.SAW,
            levelRequirement: new SkillLevel(Skill.BUILDING, 10)));

        // building: [stone or sandstone] -> house
        // TODO: add stone/sandstone bricks and use those instead?
        Requirements.Add(BuildingType.STONE_HOUSE, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 30),
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.SANDSTONE, 30),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.BRICKS, 30)),
            toolRequirement: Goods.Tool.CHISEL,
            levelRequirement: new SkillLevel(Skill.BUILDING, 10)));

        // building: cedar + saw -> lumbermill
        Requirements.Add(BuildingType.LUMBERMILL, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 30)),
            toolRequirement: Goods.Tool.SAW,
            tileRequirement: TileType.FOREST,
            levelRequirement: new SkillLevel(Skill.BUILDING, 30)));

        // building: saw + [cedar AND linen] -> market
        Requirements.Add(BuildingType.MARKET, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 50),
                new Goods(GoodsType.CRAFT_GOODS, (int)Goods.Crafted.LINEN, 20), 
                and: true),
            toolRequirement: Goods.Tool.SAW,
            levelRequirement: new SkillLevel(Skill.BUILDING, 30)));

        // building: cedar + shovel -> mine
        Requirements.Add(BuildingType.MINE, new ProductionRequirements(
            //goodsRequirement: new GoodsRequirement(
            //    new Goods(GoodsType.MATERIAL_PLANT, (int)Goods.MaterialPlant.CEDAR, 30)),
            toolRequirement: Goods.Tool.SHOVEL,
            tileRequirement: TileType.HILLS,
            levelRequirement: new SkillLevel(Skill.BUILDING, 30)));

        // building: stone + furnace -> smithy
        Requirements.Add(BuildingType.SMITHY, new ProductionRequirements(
            goodsRequirement: new GoodsRequirement(
                new Goods(GoodsType.MATERIAL_NATURAL, (int)Goods.MaterialNatural.STONE, 60)),
            toolRequirement: Goods.Tool.FURNACE,
            levelRequirement: new SkillLevel(Skill.BUILDING, 20)));
    }
}
