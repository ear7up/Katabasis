using System;

public class EscapeMenuPanel : UIElement
{
    public VBox Container;

    public EscapeMenuPanel() : base(Sprites.EscapeMenu)
    {
        Hidden = true;

        Container = new();
        Container.SetMargin(top: 25, left: 25);

        Container.Add(new TextSprite(Sprites.Font, text: "Options"));

        string[] text = new string[]{ "Options", "", "", "Exit Game", "Return to Game" };
        Action<Object>[] buttons = new Action<Object>[] { OptionsButton, Button2, Button3, ExitButton, ReturnToGameButton };

        for (int i = 0; i < buttons.Length; i++)
        {
            OverlapLayout olayout = new();

            UIElement button = new(Sprites.MenuButton, 1f, buttons[i]);
            button.HoverImage = Sprite.Create(Sprites.MenuButtonHover, Vector2.Zero);

            TextSprite buttonText = new(Sprites.Font, text: text[i]);
            buttonText.SetPadding(left: 35, top: 20);
            buttonText.ScaleDown(0.40f);

            olayout.Add(button);
            olayout.Add(buttonText);
            olayout.SetMargin(bottom: 5, left: 1);
            Container.Add(olayout);
        }
    }

    public void OptionsButton(Object clicked)
    {
        // TODO
    }

    public void Button2(Object clicked)
    {
        
    }

    public void Button3(Object clicked)
    {
        
    }

    public void ExitButton(Object clicked)
    {
        Katabasis.KatabasisGame.Instance.Exit();
    }

    public void ReturnToGameButton(Object clicked)
    {
        InputManager.Paused = false;
        Hide();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;
        base.Draw(offset);
        Container.Draw(offset);
    }

    public override void Update()
    {
        if (Hidden)
            return;
        Container.Update();
        base.Update();
    }
}