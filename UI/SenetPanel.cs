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
        ClearTimer = 3f;
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

    public int MoveForward(GamePiece[][] board, int n)
    {
        int x = X;
        int y = Y;
        int tile = (Y * 10) + X + n + 1;

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

        // Off the board
        if (y > 2)
            return tile;

        // Swap with piece at move location
        GamePiece whereLanding = board[y][x];
        board[Y][X]= whereLanding;
        whereLanding.X = X;
        whereLanding.Y = Y;
        whereLanding.Element.SetAnimation(Element.Position, 20f, 10f);

        board[y][x] = this;
        X = x;
        Y = y;

        Element.SetAnimation(whereLanding.Element.Position, 20f, 10f);
        if (tile >= (int)SenetGame.SenetSpecialTiles.ELIM1 && tile <= (int)SenetGame.SenetSpecialTiles.ELIM2)
            Element.AnimationOnComplete = SetClearTimer;

        return tile;
    }
}

public class SenetGame
{
    public int Turn { get; set; }
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

        if (State != GameState.PLAYING)
            return;

        foreach (GamePiece[] row in Board)
        {
            foreach (GamePiece piece in row)
            {
                bool active = (piece == Selected);
                piece.Update(active);
            }
        }

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

    public int Roll()
    {
        int sum = 0;
        for (int i = 0; i < 4; i++)
            sum += Globals.Rand.Next(0, 2);
        if (sum == 0)
            sum = 5;
        return sum;
    }

    public void Move(SenetPanel panel)
    {
        if (Selected.Team == 0)
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
            return;

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

    public void Randomize(SenetPanel panel)
    {
        // TODO: seperate randomize/eliminate for CPU and player
        if (Turn % 2 != 0)
            return;

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

        if (tile > 30)
        {
            piece.Clear();
            CheckVictoryCondition();
        }
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
        Movement(panel, Selected, Selected.MoveForward(Board, Roll()));

        for (int i = 0; i < Player2Pieces.Length; i++)
        {
            Selected = Player2Pieces[i];
            if (Selected.Team != 0 && Selected.ClearTimer == 0f)
                break;
        }
    }

    public void MovePlayer2(SenetPanel panel)
    {
        Movement(panel, Selected, Selected.MoveForward(Board, Roll()));
        
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

    public SenetPanel() : base(Sprites.SenetBoard)
    {
        MyLayout = new();
        MyLayout.SetMargin(top: 373, left: 93);
        MyLayout.OnClick = null;
        Add(MyLayout);

        Draggable = false;

        int i = 0;
        godButtons = new();

        UIElement osirisButton = new(Sprites.Osiris, hoverImage: Sprites.OsirisHover, onClick: Globals.Model.Senet.Guess);
        osirisButton.UserData = i++;
        osirisButton.SetPadding(right: 5);
        godButtons.Add(osirisButton);

        UIElement setButton = new(Sprites.Set, hoverImage: Sprites.SetHover, onClick: Globals.Model.Senet.Guess);
        setButton.UserData = i++;
        setButton.SetPadding(right: 15);
        godButtons.Add(setButton);

        UIElement raButton = new(Sprites.Ra, hoverImage: Sprites.RaHover, onClick: Globals.Model.Senet.Guess);
        raButton.UserData = i++;
        raButton.SetPadding(right: 13);
        godButtons.Add(raButton);

        UIElement thothButton = new(Sprites.Thoth, hoverImage: Sprites.ThothHover, onClick: Globals.Model.Senet.Guess);
        thothButton.UserData = i++;
        thothButton.SetPadding(right: 8);
        godButtons.Add(thothButton);

        UIElement isisButton = new(Sprites.Isis, hoverImage: Sprites.IsisHover, onClick: Globals.Model.Senet.Guess);
        isisButton.UserData = i++;
        isisButton.SetPadding(right: 10);
        godButtons.Add(isisButton);

        UIElement anubisButton = new(Sprites.Anubis, hoverImage: Sprites.AnubisHover, onClick: Globals.Model.Senet.Guess);
        anubisButton.UserData = i++;
        anubisButton.SetPadding(right: 8);
        godButtons.Add(anubisButton);

        UIElement horusButton = new(Sprites.Horus, hoverImage: Sprites.HorusHover, onClick: Globals.Model.Senet.Guess);
        horusButton.UserData = i++;
        godButtons.Add(horusButton);

        MyLayout.Add(godButtons);

        SetDefaultPosition(new Vector2(Globals.WindowSize.X / 2 - Width() / 2, 50f));
    }

    public void Update(Building building)
    {
        if (building == null)
            Hide();
        else
            Unhide();

        if (Hidden)
            return;

        SetDraggable();

        Globals.Model.Senet.Update(this);
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