using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaroGame
{
    public class GameState
    {
        public event Action<int, int> MoveMade;
        public event Action<GameResult> GameEnded;
        public event Action GameReset;

        public Player[,] GameGrid { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }

        public GameState()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
        }

        private bool CanMakeMove(int r, int c)
        {
            return !GameOver && GameGrid[r, c] == Player.None;
        }

        public void MakeMove(int r, int c)
        {
            if (!CanMakeMove(r, c)) return;

            GameGrid[r, c] = CurrentPlayer;
            TurnsPassed++;

            if (IsGameOver(r, c, out GameResult outcome))
            {
                GameOver = true;
                MoveMade?.Invoke(r, c);
                GameEnded?.Invoke(outcome);
            }
            else
            {
                PassTurn();
                MoveMade?.Invoke(r, c);
            }
        }
        public void MakeComputerMove()
        {
            if (GameOver) return;
            List<(int, int)> emptySquares = new List<(int, int)>();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (CanMakeMove(r, c))  
                    {
                        emptySquares.Add((r, c));
                    }
                }
            }
          if (emptySquares.Count > 0)
            {
                Random rand = new Random();
                var (r, c) = emptySquares[rand.Next(emptySquares.Count)];
                MakeMove(r, c);  
            }
        }

        private bool IsGameOver(int r, int c, out GameResult outcome)
        {
            if (DidMoveWin(r, c, out WinInfo winInfo))
            {
                outcome = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }
            if (IsGridFull())
            {
                outcome = new GameResult { Winner = Player.None };
                return true;
            }

            outcome = null;
            return false;
        }

        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        private bool DidMoveWin(int r, int c, out WinInfo win)
        {
            (int, int)[] row = new[] { (r, 0), (r, 1), (r, 2) };
            (int, int)[] col = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] diagonal = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] antiDiagonal = new[] { (0, 2), (1, 1), (2, 0) };

            if (CheckSquares(row, CurrentPlayer))
            {
                win = new WinInfo { Type = WinType.Row, Number = r };
                return true;
            }
            if (CheckSquares(col, CurrentPlayer))
            {
                win = new WinInfo { Type = WinType.Col, Number = c };
                return true;
            }
            if (CheckSquares(diagonal, CurrentPlayer))
            {
                win = new WinInfo { Type = WinType.Diagonal };
                return true;
            }
            if (CheckSquares(antiDiagonal, CurrentPlayer))
            {
                win = new WinInfo { Type = WinType.AntiDiagonal };
                return true;
            }

            win = null;
            return false;
        }

        private bool CheckSquares((int, int)[] squares, Player player)
        {
            foreach ((int r, int c) in squares)
            {
                if (GameGrid[r, c] != player)
                {
                    return false;
                }
            }

            return true;
        }

        private void PassTurn()
        {
            CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
        }

        public void Reset()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            GameReset?.Invoke();
        }
    }
}
