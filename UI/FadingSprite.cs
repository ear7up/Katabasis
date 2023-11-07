public class FadingSprite : Sprite
{
    public float FadeTime { get; set; }
    public float TimeSpent { get; set; }

    public FadingSprite(
        SpriteTexture spriteTexture = null,
        float fadeTime = 2f) : base()
    {
        SetNewSpriteTexture(spriteTexture);
        FadeTime = fadeTime;
    }

    public void Update()
    {
        TimeSpent += Globals.Time;
        Transparency = 1 - TimeSpent / FadeTime;
    }

    public override void Draw()
    {
        if (Texture != null && TimeSpent < FadeTime)
            base.Draw();
    }

    public void Reset()
    {
        TimeSpent = 0f;
        Transparency = 1f;
    }
}