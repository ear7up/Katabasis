public class BuildingPlacerPanel : CloseablePanel
{
    public TabLayout MyLayout;

    public BuildingPlacerPanel(SpriteTexture texture): base(texture)
    {
        MyLayout = new();
        MyLayout.SetMargin(left: 30, top: 60);
        Add(MyLayout);

        // Separate buildings by subcategory
        GridLayout tileImprovementLayout = BuildTileImprovementLayout();
        GridLayout buildingLayout = BuildBuildingLayout();
        GridLayout decorationLayout = BuildDecorationLayout();

        UIElement tab1 = new(Sprites.TabUnselected);
        tab1.AddSelectedImage(Sprites.TabSelected);
        MyLayout.AddTab("Tile Improvements", tab1, tileImprovementLayout);

        UIElement tab2 = new(Sprites.TabUnselected);
        tab2.AddSelectedImage(Sprites.TabSelected);
        MyLayout.AddTab("Buildings", tab2, buildingLayout);

        UIElement tab3 = new(Sprites.TabUnselected);
        tab3.AddSelectedImage(Sprites.TabSelected);
        MyLayout.AddTab("Decorations", tab3, decorationLayout);

        SetDefaultPosition(new Vector2(
            Width(), Globals.WindowSize.Y - Height() + 250));

        Hidden = true;
    }

    private GridLayout BuildTileImprovementLayout()
    {
        GridLayout tileImprovementLayout = new();
        tileImprovementLayout.SetContent(0, 0, new UIElement(Sprites.farms[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildFarm, hoverElement: new BuildingPriceDisplay(null, BuildingType.FARM)));
        tileImprovementLayout.SetContent(1, 0, new UIElement(Sprites.mines[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildMine, hoverElement: new BuildingPriceDisplay(null, BuildingType.MINE)));
        tileImprovementLayout.SetContent(2, 0, new UIElement(Sprites.ranches[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildRanch, hoverElement: new BuildingPriceDisplay(null, BuildingType.RANCH)));
        tileImprovementLayout.SetContent(3, 0, new UIElement(Sprites.gardens[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildGarden, hoverElement: new BuildingPriceDisplay(null, BuildingType.GARDEN)));

        return tileImprovementLayout;
    }

    public GridLayout BuildBuildingLayout()
    {
        GridLayout buildingLayout = new();
        buildingLayout.SetContent(0, 0, new UIElement(Sprites.markets[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildMarket, hoverElement: new BuildingPriceDisplay(null, BuildingType.MARKET)));
        buildingLayout.SetContent(1, 0, new UIElement(Sprites.houses[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildBrickHouse, hoverElement: new BuildingPriceDisplay(null, BuildingType.HOUSE, BuildingSubType.BRICK)));
        buildingLayout.SetContent(2, 0, new UIElement(Sprites.houses[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildWoodHouse, hoverElement: new BuildingPriceDisplay(null, BuildingType.HOUSE, BuildingSubType.WOOD)));
        buildingLayout.SetContent(3, 0, new UIElement(Sprites.barracks[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildBarracks, hoverElement: new BuildingPriceDisplay(null, BuildingType.BARRACKS)));
        buildingLayout.SetContent(4, 0, new UIElement(Sprites.tanneries[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildTannery, hoverElement: new BuildingPriceDisplay(null, BuildingType.TANNERY)));
        buildingLayout.SetContent(0, 1, new UIElement(Sprites.smithies[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildSmithy, hoverElement: new BuildingPriceDisplay(null, BuildingType.SMITHY)));
        buildingLayout.SetContent(1, 1, new UIElement(Sprites.temples[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildTemple, hoverElement: new BuildingPriceDisplay(null, BuildingType.TEMPLE)));
        buildingLayout.SetContent(2, 1, new UIElement(Sprites.taverns[0], scale: 0.3f, 
            onClick: Katabasis.GameManager.BuildTavern, hoverElement: new BuildingPriceDisplay(null, BuildingType.TAVERN)));
        buildingLayout.SetContent(3, 1, new UIElement(Sprites.Pyramid100, scale: 0.2f, 
            onClick: Katabasis.GameManager.BuildPyramid, hoverElement: new BuildingPriceDisplay(null, BuildingType.PYRAMID)));
        buildingLayout.SetContent(4, 1, new UIElement(Sprites.ovens[0], scale: 1.0f, 
            onClick: Katabasis.GameManager.BuildOven, hoverElement: new BuildingPriceDisplay(null, BuildingType.OVEN)));

        return buildingLayout;
    }

    public GridLayout BuildDecorationLayout()
    {
        GridLayout decorationLayout = new();
        for (int i = 0; i < Sprites.decorations.Count; i++)
        {
            UIElement element = new (Sprites.decorations[i], scale: 0.4f, 
                onClick: Katabasis.GameManager.BuildDecoration, onHover: UI.SetTooltipText, tooltip: "Decoration");
            element.UserData = i;
            decorationLayout.SetContent(i % 6, i / 6, element);
        }
        return decorationLayout;
    }
}