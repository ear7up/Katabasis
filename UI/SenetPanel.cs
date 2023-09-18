using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    public void Update(bool active)
    {
        Element.Update();

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

    public void MoveForward(GamePiece[][] board, int n)
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

        // Off the board
        if (y > 2)
        {
            SetTeam(0);
            return;
        }

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
    }
}

public class SenetGame
{
    public int Turn { get; set; }
    public GamePiece[][] Board { get; set; }
    public GamePiece Selected { get; set; }

    public GamePiece[] Player1Pieces { get; set; }
    public GamePiece[] Player2Pieces { get; set; }
    public int Player1MovingIndex { get; set; }
    public int Player2MovingIndex { get; set; }

    public bool Unused {
        get { return false; }
        set { SetOnClick(); }
    }

    public SenetGame()
    {
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

        SetPieces();
        SetOnClick();
    }

    public void SelectPiece(Object clicked)
    {
        GamePiece piece = (GamePiece)((UIElement)clicked).UserData;
        if (Turn % 2 == 0 && piece.Team == 1)
            Selected = piece;
    }

    public void Update()
    {
        foreach (GamePiece[] row in Board)
        {
            foreach (GamePiece piece in row)
            {
                bool active = (piece == Selected);
                piece.Update(active);
            }
        }

        if (InputManager.UnconsumedKeypress(Keys.N))
            Move();
        if (InputManager.UnconsumedKeypress(Keys.R))
            ResetBoard();
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

    public void ResetBoard()
    {
        Turn = 0;
        Player1MovingIndex = 0;
        Player2MovingIndex = 0;

        foreach (GamePiece[] row in Board)
        {
            foreach(GamePiece piece in row)
            {
                piece.SetTeam(0);
                piece.Element.AnimationDestination = Vector2.Zero;
            }
        }
        SetPieces();
        SetOnClick();
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

    public void Move()
    {
        SoundEffects.Play(SoundEffects.DiceSound);
        if (Turn % 2 == 0)
            MovePlayer1();
        else
            MovePlayer2();
        Turn++;
    }

    public void MovePlayer1()
    {
        Selected.MoveForward(Board, Roll());
        Player1MovingIndex = (Player1MovingIndex + 1) % Player1Pieces.Length;
        Selected = Player2Pieces[Player2MovingIndex];
    }

    public void MovePlayer2()
    {
        Selected.MoveForward(Board, Roll());
        Player2MovingIndex = (Player2MovingIndex + 1) % Player2Pieces.Length;
        Selected = Player1Pieces[Player1MovingIndex];
    }
}

public class SenetPanel : CloseablePanel
{
    public const int ROWS = 4;
    public const int COLUMNS = 6;
    public const int PROGRESS_BAR_WIDTH = 300;

    public VBox MyLayout;

    public SenetPanel() : base(Sprites.SenetBoard)
    {
        MyLayout = new();
        MyLayout.SetMargin(top: 55, left: 40);
        MyLayout.OnClick = null;
        Add(MyLayout);

        Draggable = false;

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

        Globals.Model.Senet.Update();
        base.Update();
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