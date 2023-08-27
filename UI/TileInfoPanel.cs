using System.Security.Cryptography.X509Certificates;

public class TileInfoPanel : UIElement
{
    public const float MAX_BAR_SIZE = 100f;

    public Tile MyTile;
    public VBox Container;
    public UIElement TileImage;
    public TextSprite TileDesc;

    public UIElement SoilQualityBaseBar;
    public UIElement SoilQualityBar;
    public TextSprite SoilQualityPercent;

    public VBox ResourceLayout;
    public UIElement ResourceQuantityBar;
    public UIElement ResourceQuantityBaseBar;
    public TextSprite ResourceQuantityPercent;

    public TileInfoPanel() : base(Sprites.SmallPanel)
    {
        MyTile = null;
        Hidden = true;

        Container = new();
        Container.SetMargin(top: 50, left: 50);
        TileImage = new();

        TileDesc = new(Sprites.Font);
        TileDesc.ScaleDown(0.4f);

        HBox topRow = new();
        topRow.Add(TileImage);
        topRow.Add(TileDesc);
        
        // 3 layers, a reference bar, the base quality, and current quality
        OverlapLayout soilQualityLayout = new();
        
        UIElement soilQualityMaxBar = new(Sprites.VerticalBar);
        soilQualityMaxBar.Image.SetScaleX(MAX_BAR_SIZE);
        soilQualityMaxBar.Image.SpriteColor = Color.PaleGreen;
        soilQualityLayout.Add(soilQualityMaxBar);

        SoilQualityBaseBar = new(Sprites.VerticalBar);
        SoilQualityBaseBar.Image.SpriteColor = Color.Red;
        soilQualityLayout.Add(SoilQualityBaseBar);

        SoilQualityBar = new(Sprites.VerticalBar);
        SoilQualityBar.Image.SpriteColor = Color.Green;
        soilQualityLayout.Add(SoilQualityBar);

        SoilQualityPercent = new(Sprites.Font);
        SoilQualityPercent.ScaleDown(0.45f);
        soilQualityLayout.Add(SoilQualityPercent);

        Container.Add(topRow);
        topRow.SetPadding(bottom: 10);
        Container.Add(new TextSprite(Sprites.Font, text: "Soil Quality"));
        Container.Add(soilQualityLayout);

        // Show current resource quantity (hide ResourceLayout if tile has no exhaustible resource)
        ResourceLayout = new();
        ResourceLayout.SetMargin(left: 1);
        ResourceLayout.Add(new TextSprite(Sprites.Font, text: "Resource Quantity"));

        OverlapLayout resourceBarLayout = new();
        
        ResourceQuantityBaseBar = new(Sprites.VerticalBar);
        ResourceQuantityBaseBar.Image.SpriteColor = Color.Red;
        ResourceQuantityBaseBar.Image.SetScaleX(MAX_BAR_SIZE);

        ResourceQuantityBar = new(Sprites.VerticalBar);
        ResourceQuantityBar.Image.SpriteColor = Color.Green;

        ResourceQuantityPercent = new TextSprite(Sprites.Font);
        ResourceQuantityPercent.ScaleDown(0.45f);

        resourceBarLayout.Add(ResourceQuantityBaseBar);
        resourceBarLayout.Add(ResourceQuantityBar);
        resourceBarLayout.Add(ResourceQuantityPercent);

        ResourceLayout.Add(resourceBarLayout);

        Container.Add(ResourceLayout);
    }

    public void UpdateTileData(Tile tile)
    {
        if (tile == null)
        {
            MyTile = null;
            Hidden = true;
            return;
        }
        else
        {
            Hidden = false;
        }

        // Update image when switching tiles
        if (tile != MyTile)
        {
            TileImage.Image = new Sprite();
            TileImage.Image.SetNewSpriteTexture(new SpriteTexture(
                tile.BaseSprite.TexturePathSerial, tile.BaseSprite.Texture));
            TileImage.Image.ScaleDown(0.6f);
        }

        // Update tile text description
        TileDesc.Text = tile.Describe();

        // Update soil quality graph
        float qualityPercent = tile.SoilQuality / Tile.GetMaxSoilQuality();
        float baseQualityPercent = tile.BaseSoilQuality / Tile.GetMaxSoilQuality();

        SoilQualityBar.Image.SetScaleX(MAX_BAR_SIZE * qualityPercent);
        SoilQualityBaseBar.Image.SetScaleX(MAX_BAR_SIZE * baseQualityPercent);

        SoilQualityPercent.Text = $"{(int)(100 * qualityPercent)}/{(int)(100 * baseQualityPercent)}%";

        // Update resource quantity graph
        if (tile.BaseResourceQuantity > 0f)
        {
            if (ResourceLayout.Hidden)
                ResourceLayout.Unhide();

            float resourcePercent = tile.CurrentResourceQuantity / tile.BaseResourceQuantity;
            ResourceQuantityBar.Image.SetScaleX(MAX_BAR_SIZE * resourcePercent);
            ResourceQuantityPercent.Text = $"{(int)(100 * resourcePercent)}%";
        }
        else
        {
            ResourceLayout.Hide();
        }

        MyTile = tile;

        Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden || MyTile == null)
            return;
        base.Draw(offset);
        Container.Draw(offset);
    }

    public override void Update()
    {
        if (Hidden || MyTile == null)
            return;
        Container.Update();
        base.Update();
    }
}