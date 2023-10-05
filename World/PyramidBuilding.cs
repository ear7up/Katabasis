public class PyramidBuilding : Building
{
    public PyramidBuilding() : base()
    {
        Type = BuildingType.PYRAMID;
    }

    public override void Update()
    {
        switch ((int)(BuildProgress * 100f / 25f))
        {
            case 0: ConstructionSprite.SetNewSpriteTexture(Sprites.Pyramid25); break;
            case 1: ConstructionSprite.SetNewSpriteTexture(Sprites.Pyramid25); break;
            case 2: ConstructionSprite.SetNewSpriteTexture(Sprites.Pyramid50); break;
            case 3: ConstructionSprite.SetNewSpriteTexture(Sprites.Pyramid75); break;
        }
        base.Update();
    }
}