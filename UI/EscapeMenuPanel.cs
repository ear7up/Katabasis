using System;
using Katabasis;

public class EscapeMenuPanel : CloseablePanel
{
    public VBox Container;

    public EscapeMenuPanel() : base(Sprites.EscapeMenu)
    {
        Container = new();
        Container.SetMargin(top: 35, left: 25);

        XButton.ScaleDown(0.5f);

        string[] text = new string[]{ 
            "Options", "Save", "Load", "Exit Game", "Save and Exit", "Return to Game" };
        Action<Object>[] buttons = new Action<Object>[] { 
            OptionsButton, SaveButton, LoadButton, ExitButton, SaveAndExitButton, ReturnToGameButton };

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

        SetDefaultPosition(new Vector2(
            Globals.WindowSize.X / 2 - Width() / 2, 
            Globals.WindowSize.Y / 2 - Height() / 2));

        Hidden = true;
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

    public void SaveAndExitButton(Object clicked)
    {
        Katabasis.KatabasisGame.Instance.Save();
        Katabasis.KatabasisGame.Instance.Exit();
    }

    public void ReturnToGameButton(Object clicked)
    {
        InputManager.Paused = false;
        Hide();
    }

    public override void ClosePanel(Object clicked)
    {
        InputManager.Paused = false;
        base.ClosePanel(clicked);
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;
        base.Draw(offset);
        Container.Draw(offset);
    }

    public override void Hide()
    {
        InputManager.Paused = false;
        base.Hide();
    }

    public override void Unhide()
    {
        InputManager.Paused = true;
        base.Unhide();
    }

    public override void Update()
    {
        if (InputManager.UnconsumedKeypress(Keys.Escape))
        {
            GameManager.TogglePanel(this);
            InputManager.ConsumeKeypress(Keys.Escape, this);
        }

        if (Hidden)
            return;

        Container.Update();
        base.Update();
    }
}