public class DarkPanel : VBox
{
    public TextSprite HeaderText;
    public VBox ContentLayout;

    public DarkPanel(string headerText = "") : base()
    {
        SpriteTexture topTexture = headerText.Length > 0 ? Sprites.DarkPanelTopDarker : Sprites.DarkPanelTop;
        HeaderText = new(Sprites.SmallFont, Color.White, Color.Black, text: headerText);
        HeaderText.SetPadding(left: 15);

        OverlapLayout top = new();
        top.Add(new UIElement(topTexture));
        top.Add(HeaderText);
        Add(top);
        ContentLayout = new VBox(Sprites.DarkPanel);
        ContentLayout.SetMargin(top: 5, left: 5);
        Add(ContentLayout);
        Add(new UIElement(Sprites.DarkPanelBottom));
    }

    public void AddContent(UIElement content)
    {
        ContentLayout.Add(content);
    }

    public override void Update()
    {
        int imageHeight = ContentLayout.Image.Texture.Height;
        int contentHeight = ContentLayout.Height();

        if (contentHeight > imageHeight)
            ContentLayout.Image.SetScaleY((float)contentHeight / imageHeight);
        else
            ContentLayout.Image.SetScaleY(1f);

        base.Update();
    }
}