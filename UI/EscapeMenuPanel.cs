using System;
using Katabasis;

public class EscapeMenuPanel : UIElement
{
    public VBox Container;

    public EscapeMenuPanel() : base(Sprites.EscapeMenu)
    {
        Hidden = true;

        Container = new();
        Container.SetMargin(top: 25, left: 25);

        Container.Add(new TextSprite(Sprites.Font, text: "Options"));

        string[] text = new string[]{ "Options", "Save", "Load", "Exit Game", "Return to Game" };
        Action<Object>[] buttons = new Action<Object>[] { OptionsButton, SaveButton, LoadButton, ExitButton, ReturnToGameButton };

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
        // TODO: close this menu and unhide the options menu
        // do not unpause the game
    }

    public void SaveButton(Object clicked)
    {
        // TODO: save menu (name the save, choose to overwrite, etc.)
        Katabasis.KatabasisGame.Instance.Save(KatabasisGame.CurrentSaveName);
    }

    public void LoadButton(Object clicked)
    {
        // TODO: load menu (choose which save to load, display last modified time, etc.)
        Katabasis.KatabasisGame.Instance.Load(KatabasisGame.CurrentSaveName);
    }

    public void ExitButton(Object clicked)
    {
        // TODO: should we always exit without saving?
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