using System;
using System.Collections.Generic;

public class GamePiece : UIElement
{
    public int Team;
    public int X;
    public int Y;

    public GamePiece(int team, int x, int y)
    {
        X = x;
        Y = y;
        SetTeam(team);
    }

    public void SetTeam(int team)
    {
        Team = team;
        switch (Team)
        {
            case 1: Image = Sprite.Create(Sprites.SenetPiece1, Vector2.Zero); break;
            case 2: Image = Sprite.Create(Sprites.SenetPiece2, Vector2.Zero); break;
            default: Image = null; break;
        }

        Image?.SetScale(0.5f);
    }

    public void Update(bool active)
    {
        base.Update();

        if (Image == null)
            return;

        if (active && Team == 1)
            Image.SpriteColor = Color.Orange;
        else if (active && Team == 2)
            Image.SpriteColor = Color.Blue;
        else
            Image.SpriteColor = Color.White;
    }

    public override void Draw(Vector2 offset)
    {
        if (Image == null)
            return;

        // Each row starts farther to the left and has more space between elements
        int adjustOffsetX = Y * 35;
        float adjustPerspectiveX = 1 + (Y * 0.14f);
        Vector2 gridOffset = new(245 - adjustOffsetX + (X * 54 * adjustPerspectiveX), 115 + (Y * 58));
        base.Draw(offset + gridOffset);
    }

    public void MoveForward(GamePiece[][] board, int n)
    {
        int x = (X + n) % 10;
        int y = Y + (X + n) / 10;

        // Off the board
        if (y > 2)
            return;

        // Swap with piece at move location
        GamePiece whereLanding = board[y][x];
        board[Y][X]= whereLanding;
        whereLanding.X = X;
        whereLanding.Y = Y;

        board[y][x] = this;
        X = x;
        Y = y;
    }
}

public class SenetGame
{
    public int Turn;
    public GamePiece[][] Board;

    public GamePiece[] Player1Pieces;
    public GamePiece[] Player2Pieces;
    public int Player1MovingIndex;
    public int Player2MovingIndex;

    public SenetGame()
    {
        // Init empty board
        Board = new GamePiece[3][];
        for (int y = 0; y < 3; y++)
        {
            Board[y] = new GamePiece[10];
            for (int x = 0; x < 10; x++)
                Board[y][x] = new GamePiece(0, x, y);
        }

        Player1Pieces = new GamePiece[7];
        Player2Pieces = new GamePiece[7];

        SetPieces();
    }

    public void Update()
    {
        foreach (GamePiece[] row in Board)
        {
            foreach (GamePiece piece in row)
            {
                bool active = 
                    Turn % 2 == 0 && piece == Player1Pieces[Player1MovingIndex] ||
                    Turn % 2 == 1 && piece == Player2Pieces[Player2MovingIndex] ;
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
            foreach (GamePiece piece in row)
                piece.Draw(offset);
    }

    public void ResetBoard()
    {
        Turn = 0;
        Player1MovingIndex = 0;
        Player2MovingIndex = 0;

        foreach (GamePiece[] row in Board)
            foreach(GamePiece piece in row)
                piece.SetTeam(0);
        SetPieces();
    }

    public void SetPieces()
    {
        Array.Clear(Player1Pieces);
        Array.Clear(Player2Pieces);

        // Place fourteen pawns, alternating team
        for (int i = 0; i < 14; i++)
        {
            GamePiece piece = Board[i / 10][i % 10];
            piece.SetTeam((i % 2) + 1);

            if (i % 2 == 0)
                Player1Pieces[6 - i / 2] = piece;
            else
                Player2Pieces[6 - i / 2] = piece;
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
        Player1Pieces[Player1MovingIndex].MoveForward(Board, Roll());
        Player1MovingIndex = (Player1MovingIndex + 1) % Player1Pieces.Length;
    }

    public void MovePlayer2()
    {
        Player2Pieces[Player2MovingIndex].MoveForward(Board, Roll());
        Player2MovingIndex = (Player2MovingIndex + 1) % Player2Pieces.Length;
    }
}

public class SenetPanel : CloseablePanel
{
    public const int ROWS = 4;
    public const int COLUMNS = 6;
    public const int PROGRESS_BAR_WIDTH = 300;

    public VBox MyLayout;
    public SenetGame Game;

    public SenetPanel() : base(Sprites.SenetBoard)
    {
        MyLayout = new();
        MyLayout.SetMargin(top: 55, left: 40);
        MyLayout.OnClick = null;
        Add(MyLayout);

        Game = new();

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

        Game.Update();
        base.Update();
    }

    public override void Draw(Vector2 offset)
    {
        if (Hidden)
            return;
        base.Draw(offset);
        Game.Draw(offset);
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