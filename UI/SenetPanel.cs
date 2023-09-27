using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public enum GameState
{
    PLAYING,
    WON,
    LOST
}

public class GamePiece
{
    [JsonIgnore]
    public UIElement Element;

    private int _Team;
    public int Team { 
        get { return _Team; }
        set { SetTeam(value); }
    }
    public int X { get; set; }
    public int Y { get; set; }

    public float ClearTimer { get; set; }

    public GamePiece()
    {
        Element = new();
    }

    public static GamePiece Create(int team, int x, int y)
    {
        GamePiece piece = new() {
            X = x,
            Y = y
        };
        piece.SetTeam(team);
        return piece;
    }

    public void SetTeam(int team)
    {
        _Team = team;
        switch (_Team)
        {
            case 1: Element.Image = Sprite.Create(Sprites.SenetPiece1, Vector2.Zero); break;
            case 2: Element.Image = Sprite.Create(Sprites.SenetPiece2, Vector2.Zero); break;
            default: Element.Image = null; break;
        }

        Element.Image?.SetScale(0.5f);
    }

    public void Clear()
    {
        SetTeam(0);
    }

    public void SetClearTimer()
    {
        ClearTimer = 1.5f;
    }

    public void Update(bool active)
    {
        Element.Update();

        if (ClearTimer > 0f)
        {
            ClearTimer = Math.Max(0f, ClearTimer - Globals.Time);
            if (ClearTimer <= 0f)
                Clear();
        }

        if (Element.Image == null)
            return;

        if (active && Team == 1)
            Element.Image.SpriteColor = Color.Orange;
        else if (active && Team == 2)
            Element.Image.SpriteColor = Color.Blue;
        else
            Element.Image.SpriteColor = Color.White;
    }

    public void Draw(Vector2 offset)
    {
        // Each row starts farther to the left and has more space between elements
        int adjustOffsetX = Y * 35;
        float adjustPerspectiveX = 1 + (Y * 0.14f);
        Vector2 gridOffset = new(245 - adjustOffsetX + (X * 54 * adjustPerspectiveX), 115 + (Y * 58));
        Element.Draw(offset + gridOffset);
    }

    public void DrawAtPos(Vector2 offset)
    {
        Element.Draw(Element.Position);
    }

    public Point CalcNewTilePosition(int n)
    {
        int x = X;
        int y = Y;

        if (Y % 2 == 0)
        {
            x += n;
            if (x > 9)
            {
                y++;
                x = 9 - (x % 10);
            }
        }
        else
        {
            x -= n;
            if (x < 0)
            {
                y++;
                x = -x - 1;
            }
        }

        return new Point(x, y);
    }

    public int CalcNewTileNumber(int n)
    {
        return (Y * 10) + X + n + 1;
    }

    public int MoveForward(GamePiece[][] board, int n)
    {
        int tile = CalcNewTileNumber(n);
        Point newPos = CalcNewTilePosition(n);

        // Off the board
        if (newPos.Y > 2)
        {
            SetClearTimer();
            return tile;
        }

        // Swap with piece at move location
        GamePiece whereLanding = board[newPos.Y][newPos.X];
        board[Y][X]= whereLanding;
        whereLanding.X = X;
        whereLanding.Y = Y;
        whereLanding.Element.SetAnimation(Element.Position, 20f, 10f);

        board[newPos.Y][newPos.X] = this;
        X = newPos.X;
        Y = newPos.Y;

        Element.SetAnimation(whereLanding.Element.Position, 20f, 10f);
        if (tile > 30 || (tile >= (int)SenetGame.SenetSpecialTiles.ELIM1 && tile <= (int)SenetGame.SenetSpecialTiles.ELIM2))
            SetClearTimer();

        return tile;
    }
}

public class SenetGame
{
    public int Turn { get; set; }
    public int AiChoices { get; set; }
    public GameState State { get; set; }
    public GamePiece[][] Board { get; set; }
    public GamePiece Selected { get; set; }
    public GamePiece[] Player1Pieces { get; set; }
    public GamePiece[] Player2Pieces { get; set; }
    public int Player1MovingIndex { get; set; }
    public int Player2MovingIndex { get; set; }

    public static int _HiddenPosition;
    public int HiddenPosition { 
        get { return _HiddenPosition; } 
        set { _HiddenPosition = value; } 
    }

    public bool Unused {
        get { return false; }
        set { SetOnClick(); }
    }

    public SenetGame()
    {
        State = GameState.PLAYING;

        // Init empty board
        Board = new GamePiece[3][];
        for (int y = 0; y < 3; y++)
        {
            Board[y] = new GamePiece[10];
            for (int x = 0; x < 10; x++)
                Board[y][x] = GamePiece.Create(0, x, y);
        }

        Player1Pieces = new GamePiece[5];
        Player2Pieces = new GamePiece[5];
    }

    public void SelectPiece(Object clicked)
    {
        GamePiece piece = (GamePiece)((UIElement)clicked).UserData;
        if (Turn % 2 == 0 && piece.Team == 1)
            Selected = piece;
    }

    public void Update(SenetPanel panel)
    {
        if (InputManager.UnconsumedKeypress(Keys.R))
            ResetBoard(panel);

        if (Turn % 2 == 1 && Selected == null || Selected.Team == 0)
            AiSelectBestPiece();

        foreach (GamePiece[] row in Board)
        {
            foreach (GamePiece piece in row)
            {
                bool active = (piece == Selected);
                piece.Update(active);
            }
        }

        if (State != GameState.PLAYING)
            return;

        if (InputManager.UnconsumedKeypress(Keys.N))
            Move(panel);
    }

    public void Draw(Vector2 offset)
    {
        foreach (GamePiece[] row in Board)
        {
            foreach (GamePiece piece in row)
            {
                if (piece.Element.AnimationDestination != Vector2.Zero)
                    piece.DrawAtPos(offset);
                else
                    piece.Draw(offset);
            }
        }
    }

    public void ResetBoard(SenetPanel panel)
    {
        Turn = 0;
        AiChoices = 7;
        State = GameState.PLAYING;
        HiddenPosition = Globals.Rand.Next(0, 7);

        Player1MovingIndex = 0;
        Player2MovingIndex = 0;

        foreach (GamePiece[] row in Board)
        {
            foreach(GamePiece piece in row)
            {
                piece.Clear();
                piece.Element.AnimationDestination = Vector2.Zero;
            }
        }

        if (panel != null)
            foreach (UIElement godButton in panel.godButtons.Elements)
                godButton.Image.SpriteColor = Color.White;

        SetPieces();
        SetOnClick();
    }

    public void Guess(Object clicked)
    {
        int index = (int)((UIElement)clicked).UserData;
        if (index == HiddenPosition)
            Win();
        else
            Lose();
    }

    public void Win()
    {
        // Draw some fancy graphics and reveal the hidden god
        State = GameState.WON;
        
    }

    public void Lose()
    {
        // Lock out input until game is reset
        State = GameState.LOST;
    }

    public void SetPieces()
    {
        Array.Clear(Player1Pieces);
        Array.Clear(Player2Pieces);

        // Place ten pawns, alternating team
        for (int i = 0; i < 10; i++)
        {
            GamePiece piece = Board[i / 10][i % 10];
            piece.SetTeam((i % 2) + 1);

            if (i % 2 == 0)
                Player1Pieces[4 - i / 2] = piece;
            else
                Player2Pieces[4 - i / 2] = piece;
        }

        Selected = Player1Pieces[Player1MovingIndex];
    }

    public void SetOnClick()
    {
        foreach (GamePiece[] row in Board)
        {
            foreach(GamePiece piece in row)
            {
                piece.Element.OnClick = SelectPiece;
                piece.Element.UserData = piece;
            }
        }
    }

    public int Roll(SenetPanel panel)
    {
        int sum = 0;
        for (int i = 0; i < 4; i++)
            sum += Globals.Rand.Next(0, 2);

        for (int i = 0; i < panel.Sticks.Elements.Count; i++)
        {
            if (i < sum)
                panel.Sticks.Elements[i].Image.SpriteColor = Color.DarkRed;
            else if (panel.Sticks.Elements[i].Image != null)
                panel.Sticks.Elements[i].Image.SpriteColor = Color.White;
        }

        if (sum == 0)
            sum = 5;

        panel.RollNumber.Text = $"{sum}";

        return sum;
    }

    public void Move(SenetPanel panel)
    {
        // Can't move pieces once they're off the board or waiting to be cleared
        if (Selected.Team == 0 || Selected.ClearTimer != 0f)
            return;

        SoundEffects.Play(SoundEffects.DiceSound);
        if (Turn % 2 == 0)
            MovePlayer1(panel);
        else
            MovePlayer2(panel);
        Turn++;
    }

    public enum SenetSpecialTiles
    {
        ELIM1 = 26,
        RANDOMIZE = 27,
        ELIM3 = 28,
        ELIM2 = 29
    }

    public void Eliminate(SenetPanel panel, int n)
    {
        // Only eliminate on the player side, not the CPU
        if (Turn % 2 != 0)
        {
            AiEliminate(n);
            return;
        }

        List<UIElement> candidates = new();

        int i = 0;
        foreach (UIElement button in panel.godButtons.Elements)
        {
            if (i != HiddenPosition && button.Image.SpriteColor == Color.White)
                candidates.Add(button);
            i++;
        }

        int eliminated = 0;
        foreach (UIElement button in candidates.OrderBy(x => Globals.Rand.Next()))
        {
            // Make the button transparent
            button.Image.SpriteColor = Color.White * 0f;
            eliminated++;

            if (eliminated >= n)
                break;
        }
    }

    public void AiEliminate(int n)
    {
        AiChoices -= n;

        // AI knows which panels is correct
        if (AiChoices <= 1)
            Lose();
    }

    public void Randomize(SenetPanel panel)
    {
        if (Turn % 2 != 0)
        {
            AiChoices = 7;
            return;
        }

        HiddenPosition = Globals.Rand.Next(0, 7);
        foreach (UIElement button in panel.godButtons.Elements)
            button.Image.SpriteColor = Color.White;
    }

    public void Movement(SenetPanel panel, GamePiece piece, int tile)
    {
        switch ((SenetSpecialTiles)tile)
        {
            case SenetSpecialTiles.ELIM1: SoundEffects.Play(SoundEffects.ElimSound); Eliminate(panel, 1); break;
            case SenetSpecialTiles.ELIM2: SoundEffects.Play(SoundEffects.ElimSound); Eliminate(panel, 2); break;
            case SenetSpecialTiles.ELIM3: SoundEffects.Play(SoundEffects.ElimSound); Eliminate(panel, 3); break;
            case SenetSpecialTiles.RANDOMIZE: 
            {
                SoundEffects.Play(SoundEffects.DripSound);
                Randomize(panel); 
                break;
            }
            default: return;
        }

        CheckVictoryCondition();
    }

    // One victory condition is to clear all pieces
    public void CheckVictoryCondition()
    {
        bool p1win = true;
        foreach (GamePiece piece in Player1Pieces)
            if (piece.Team != 0 && piece.ClearTimer == 0f)
                p1win = false;
        if (p1win)
            Win();

        bool p2win = true;
        foreach (GamePiece piece in Player2Pieces)
            if (piece.Team != 0 && piece.ClearTimer == 0f)
                p2win = false;
        if (p2win)
            Lose();
    }

    public void MovePlayer1(SenetPanel panel)
    {
        Movement(panel, Selected, Selected.MoveForward(Board, Roll(panel)));

        for (int i = 0; i < Player2Pieces.Length; i++)
        {
            Selected = Player2Pieces[i];
            if (Selected.Team != 0 && Selected.ClearTimer == 0f)
                break;
        }

        AiSelectBestPiece();
    }

    public void AiSelectBestPiece()
    {
        int count = 0;
        foreach (GamePiece piece in Player2Pieces)
            if (piece.Team != 0 && piece.ClearTimer == 0f)
                count++;

        GamePiece choice = null;
        foreach (GamePiece piece in Player2Pieces)
        {
            if (piece.Team == 0 || piece.ClearTimer != 0f)
                continue;

            // Moving 2 steps is a 6/16 chance, try to 
            Point newPos = piece.CalcNewTilePosition(2);
            int tile = piece.CalcNewTileNumber(2);

            // Avoid moving pieces likely to hit the randomize tile
            if (tile == (int)SenetSpecialTiles.RANDOMIZE)
                continue;

            // Max y coordinate, max x coordinate for even rows, min for the odd row (middle goes backwards)
            if (choice == null || piece.Y > choice.Y ||
                (piece.Y == choice.Y && piece.Y != 1 && piece.X > choice.X) ||
                (piece.Y == choice.Y && piece.Y == 1 && piece.X < choice.X))
            {
                choice = piece;
            }

            // Aggressively try to swap with the opponent's pieces
            if (Board[newPos.Y][newPos.X].Team == 1)
            {
                choice = piece; 
                break;
            }
        }

        if (choice != null)
            Selected = choice;
        else
            CheckVictoryCondition();
    }

    public void MovePlayer2(SenetPanel panel)
    {
        Movement(panel, Selected, Selected.MoveForward(Board, Roll(panel)));
        
        for (int i = 0; i < Player1Pieces.Length; i++)
        {
            Selected = Player1Pieces[i];
            if (Selected.Team != 0 && Selected.ClearTimer == 0f)
                break;
        }
    }
}

public class SenetPanel : CloseablePanel
{
    public const int ROWS = 4;
    public const int COLUMNS = 6;
    public const int PROGRESS_BAR_WIDTH = 300;

    public VBox MyLayout;
    public HBox godButtons;
    public HBox Sticks;
    public TextSprite WinLoss;
    public TextSprite RollNumber;

    public SenetPanel() : base(Sprites.SenetBoard)
    {
        MyLayout = new();
        //MyLayout.SetMargin(top: 373, left: 93);
        MyLayout.SetMargin(top: 29, left: 25);
        MyLayout.OnClick = null;
        Add(MyLayout);

        Draggable = false;

        HBox topPart = new();
        VBox rollLayout = new();

        // Roll button in the top-left
        UIElement rollButton = new(Sprites.SenetRoll, hoverImage: Sprites.SenetRollHover, onClick: Move);
        rollLayout.Add(rollButton);
        rollButton.SetPadding(bottom: 10);

        // Sticks to show the roll below the button
        Sticks = new();
        for (int s = 0; s < 4; s++)
        {
            UIElement stick = new(Sprites.SenetStick);
            stick.SetPadding(right: 5);
            Sticks.Add(stick);
        }
        
        Sticks.Elements[3].SetPadding(right: 18);
        RollNumber = new(Sprites.FontAgencyL, hasDropShadow: false);
        Sticks.Add(RollNumber);

        Sticks.SetPadding(bottom: 192);
        rollLayout.Add(Sticks);
        rollLayout.SetPadding(right: 50);

        WinLoss = new(Sprites.FontAgencyL, Color.White, Color.Black);

        //topPart.Add(rollLayout);
        //topPart.Add(WinLoss);
        MyLayout.Add(rollLayout);
        WinLoss.SetPadding(left: 400);
        MyLayout.Add(WinLoss);

        // god buttons below the roll panel, aligned with the background image
        int i = 0;
        godButtons = new();

        UIElement osirisButton = new(Sprites.Osiris, hoverImage: Sprites.OsirisHover, onClick: Guess);
        osirisButton.UserData = i++;
        osirisButton.SetPadding(left: 65, right: 5);
        godButtons.Add(osirisButton);

        UIElement setButton = new(Sprites.Set, hoverImage: Sprites.SetHover, onClick: Guess);
        setButton.UserData = i++;
        setButton.SetPadding(right: 15);
        godButtons.Add(setButton);

        UIElement raButton = new(Sprites.Ra, hoverImage: Sprites.RaHover, onClick: Guess);
        raButton.UserData = i++;
        raButton.SetPadding(right: 13);
        godButtons.Add(raButton);

        UIElement thothButton = new(Sprites.Thoth, hoverImage: Sprites.ThothHover, onClick: Guess);
        thothButton.UserData = i++;
        thothButton.SetPadding(right: 8);
        godButtons.Add(thothButton);

        UIElement isisButton = new(Sprites.Isis, hoverImage: Sprites.IsisHover, onClick: Guess);
        isisButton.UserData = i++;
        isisButton.SetPadding(right: 10);
        godButtons.Add(isisButton);

        UIElement anubisButton = new(Sprites.Anubis, hoverImage: Sprites.AnubisHover, onClick: Guess);
        anubisButton.UserData = i++;
        anubisButton.SetPadding(right: 8);
        godButtons.Add(anubisButton);

        UIElement horusButton = new(Sprites.Horus, hoverImage: Sprites.HorusHover, onClick: Guess);
        horusButton.UserData = i++;
        godButtons.Add(horusButton);

        MyLayout.Add(godButtons);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X / 2 - Width() / 2, 50f));
    }

    public void Guess(Object clicked)
    {
        Globals.Model.Senet.Guess(clicked);
    }

    public void Move(Object clicked)
    {
        Globals.Model.Senet.Move(this);
    }

    public void Update(Building building)
    {
        Sticks.SetPadding(bottom: 192 - WinLoss.Height());

        if (building == null)
            Hide();
        else
            Unhide();

        if (Hidden)
            return;

        SetDraggable();

        Globals.Model.Senet.Update(this);

        switch (Globals.Model.Senet.State)
        {
            case GameState.PLAYING: WinLoss.Text = ""; break;
            case GameState.WON: WinLoss.Text = "Victory!"; break;
            case GameState.LOST: WinLoss.Text = "Defeat."; break;
        }

        base.Update();
    }

    public void SetDraggable()
    {
        Draggable = true;
        foreach (GamePiece[] row in Globals.Model.Senet.Board)
        {
            foreach (GamePiece piece in row)
            {
                if (piece.Element.AnimationDestination != Vector2.Zero)
                {
                    Draggable = false;
                    break;
                }
            }
        }
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;
        base.Draw(offset);
        Globals.Model.Senet.Draw(offset);
    }

    public override void ClosePanel(Object clicked)
    {
        if (Building.SelectedBuilding != null)
        {
            Building.SelectedBuilding.Selected = false;
            Building.SelectedBuilding = null;
        }
        base.ClosePanel(clicked);
    }
}