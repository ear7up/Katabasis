public class FadingTextSprite : TextSprite
{
    public float FadeTime { get; set; }
    public float TimeSpent { get; set; }

    public FadingTextSprite(
        SpriteFont font,
        string text,
        Vector2 startPosition, 
        float scale, 
        float fadeTime) : base(font, true, text)
    {
        Scale = new Vector2(scale, scale);
        FadeTime = fadeTime;
        SetDefaultPosition(startPosition);
    }

    public void StartAnimation()
    {
        float speed = 10f;
        Vector2 destination = DefaultPosition + new Vector2(0, -speed * FadeTime);
        base.SetAnimation(destination, speed, acceleration: 0f);
    }

    public override void ExecuteAnimation()
    {
        base.ExecuteAnimation();
        Transparency = 1 - TimeSpent / FadeTime;
    }

    public override void Update()
    {
        base.Update();
        TimeSpent += Globals.Time;
        //if (TimeSpent >= FadeTime)
        //    Reset();
    }

    public void Reset()
    {
        Position = DefaultPosition;
        TimeSpent = 0f;
        Transparency = 1f;
    }
}