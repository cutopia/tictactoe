using System.Collections.Generic;
using UnityEngine; // for mathf stuff

public class OutcomeFinder
{
    private delegate bool WinCheck(int[,] gameGrid, int player, int dim);
    public class Score
    {
        public bool[] winningRows;
        public bool[] winningCols;
        public bool isRowWin;
        public bool isColWin;
        public bool firstDiagWin;
        public bool secondDiagWin;
        public bool firstPlayerWins;
        public bool secondPlayerWins;
        public int[,] winGrid;
        public bool playerWon;
    };

    System.Random _rnd = new System.Random((int)Time.time);

     /// <summary>
     /// An overload of IsWinner that takes in a player symbol
     /// </summary>
     /// <returns><c>true</c>, if winner was ised, <c>false</c> otherwise.</returns>
     /// <param name="symbol">Symbol.</param>
     /// <param name="gameGrid">Game grid.</param>
     /// <param name="playerSymbols">Player symbols.</param>
    public bool IsWinner(string symbol, int[,] gameGrid, string[] playerSymbols)
    {
        int targetScore = gameGrid.GetLength(0);
        targetScore *= (symbol == playerSymbols[0] ? 1 : -1);
        int firstDiagonalTotal = 0;
        int secondDiagonalTotal = 0;
        for (int i = 0; i < gameGrid.GetLength(0); i++)
        {
            int Xtotal = 0;
            int Ytotal = 0;
            for (int j = 0; j < gameGrid.GetLength(1); j++)
            {
                Xtotal += gameGrid[i, j];
                Ytotal += gameGrid[j, i];
                if (i == j)
                {
                    firstDiagonalTotal += gameGrid[i, j];
                    secondDiagonalTotal += gameGrid[i, ((gameGrid.GetLength(1) - 1) - j)];
                }
            }
            if (Xtotal == targetScore || Ytotal == targetScore || firstDiagonalTotal == targetScore || secondDiagonalTotal == targetScore)
            {
                return true;
            }
        }
        return false;
    }

    private int[,] MakeRowGameGrid(int dim,  int player, int rowIndex)
    {
        int[,] grid = new int[dim, dim];
        // first put in the diagonal for the winner
        for (int col = 0; col < dim; col++)
        {
            grid[col, rowIndex] = player;
        }
        DoRemainingMoves(grid, player);
        return grid;
    }

    public List<Move> FindRowGameSequence(int dim, int player, int rowIndex)
    {
        int opposingPlayer = player == 1 ? -1 : 1;
        int[,] grid = MakeRowGameGrid(dim, player, rowIndex);
        // try again if the opposing player accidentally wins.
        while (IsWinner(grid, opposingPlayer).playerWon)
        {
            grid = MakeRowGameGrid(dim, player, rowIndex);
        }

        return ConvertGridToRandomMoves(grid);
    }

    private int[,] MakeColGameGrid(int dim, int player, int colIndex)
    {
        int[,] grid = new int[dim, dim];
        // first put in the diagonal for the winner
        for (int row = 0; row < dim; row++)
        {
            grid[((dim - 1) - colIndex), row] = player;
        }
        DoRemainingMoves(grid, player);
        return grid;
    }

    public List<Move> FindColGameSequence(int dim, int player, int colIndex)
    {
        int opposingPlayer = player == 1 ? -1 : 1;
        int[,] grid = MakeColGameGrid(dim, player, colIndex);
        // try again if the opposing player accidentally wins.
        while (IsWinner(grid, opposingPlayer).playerWon)
        {
            grid = MakeColGameGrid(dim, player, colIndex);
        }

        return ConvertGridToRandomMoves(grid);
    }


    public List<Vector2Int> GetWinningCells(int[,] gameGrid, int player)
    {
        var cellList = new List<Vector2Int>();
        int targetScore = gameGrid.GetLength(0) * player;
        int firstDiagonalTotal = 0;
        int secondDiagonalTotal = 0;
        var firstDiagCells = new List<Vector2Int>();
        var secondDiagCells = new List<Vector2Int>();
        for (int i = 0; i < gameGrid.GetLength(0); i++)
        {
            int Xtotal = 0;
            int Ytotal = 0;
            for (int j = 0; j < gameGrid.GetLength(1); j++)
            {
                Xtotal += gameGrid[i, j];
                Ytotal += gameGrid[j, i];
                if (i == j)
                {
                    firstDiagonalTotal += gameGrid[i, j];
                    firstDiagCells.Add(new Vector2Int(i, j));
                    secondDiagonalTotal += gameGrid[i, ((gameGrid.GetLength(1) - 1) - j)];
                    secondDiagCells.Add(new Vector2Int(i, ((gameGrid.GetLength(1) - 1) - j)));
                }
            }
            if (Xtotal == targetScore)
            {
                for (int k = 0; k < gameGrid.GetLength(1); k++)
                {
                    cellList.Add(new Vector2Int(i, k));
                }
            }
            if (Ytotal == targetScore)
            {
                for (int k = 0; k < gameGrid.GetLength(0); k++)
                {
                    cellList.Add(new Vector2Int(k, i));
                }
            }

        }
        if (firstDiagonalTotal == targetScore)
        {
            cellList.AddRange(firstDiagCells);
        }
        if (secondDiagonalTotal == targetScore)
        {
            cellList.AddRange(secondDiagCells);
        }
        return cellList;
    }

    private int[,] GetRandomGrid(int dim)
    {
        int[] gridArray = new int[dim * dim];
        int[,] finalGrid = new int[dim, dim];
        int player = 1;
        int i;
        // initialize a filled game grid.
        for (i = 0; i < gridArray.Length; i++)
        {
            gridArray[i] = player;
            player *= -1;
        }
        // shuffle the grid.
        for (i = 0; i < gridArray.Length; i++)
        {
            int tmp = gridArray[i];
            int r = _rnd.Next(gridArray.Length);
            gridArray[i] = gridArray[r];
            gridArray[r] = tmp;
        }
        for (i = 0; i < gridArray.Length; i++)
        {
            finalGrid[i / dim, i % dim] = gridArray[i];
        }
        return finalGrid;
    }

    private void ShuffleMoveList(List<Move> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Move tmp = list[i];
            int r = _rnd.Next(list.Count);
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    private int[,] GenerateDrawGrid(int dim)
    {
        int attemptCountdown = 2000;

        while ((attemptCountdown--) > 0)
        {
            int[,] gameGrid = GetRandomGrid(dim);
            if (FindWins(gameGrid) == null)
            {
                return gameGrid;
            }
        }
        return null;
    }

    public List<Move> FindDrawGameSequence(int dim)
    {
        int[,] grid = GenerateDrawGrid(dim);
        if (grid == null)
        {
            return null;
        }
        return ConvertGridToRandomMoves(grid);
    }

    /// <summary>
    /// Makes a game grid containing a diagonal win for the specified player.
    /// However, it's theoretically possible for the other player to win first
    /// so that should be checked and those grids thrown out.
    /// </summary>
    /// <returns>The first diagonal grid.</returns>
    /// <param name="dim">Dim.</param>
    /// <param name="player">Player.</param>

    private int[,] MakeFirstDiagonalGrid(int dim, int player)
    {
        int[,] grid = new int[dim,dim];
        // first put in the diagonal for the winner
        for (int col = 0; col < dim; col++)
        {
            grid[col, (dim - 1) - col] = player;
        }
        DoRemainingMoves(grid, player);
        return grid;
    }

    private void DoRemainingMoves(int[,] grid, int player)
    {
        int dim = grid.GetLength(0);
        // then put in some other moves for the loser.
        List<Vector2Int> remainingCells = GetShuffledListOfAvailableCells(grid);
        int numberOfOpponentTurns = player == 1 ? (dim - 1) : dim;
        for (int i = 0; i < numberOfOpponentTurns; i++)
        {
            Vector2Int move = remainingCells[0];
            remainingCells.RemoveAt(0);
            grid[move.x, move.y] = player == 1 ? -1 : 1;
        }
    }

    public List<Move> FindFirstDiagonalGameSequence(int dim, int player)
    {
        int opposingPlayer = player == 1 ? -1 : 1;
        int[,] grid = MakeFirstDiagonalGrid(dim, player);
        // try again if the opposing player accidentally wins.
        while (IsWinner(grid, opposingPlayer).playerWon)
        {
            grid = MakeFirstDiagonalGrid(dim, player);
        }

        return ConvertGridToRandomMoves(grid);
    }

    private List<Vector2Int> GetShuffledListOfAvailableCells(int[,] grid)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int col = 0; col < grid.GetLength(0); col++)
        {
            for (int row = 0; row < grid.GetLength(1); row++)
            {
                if (grid[col,row] == 0)
                {
                    cells.Add(new Vector2Int(col, row));
                }
            }
        }
        // shuffle the list
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int tmp = cells[i];
            int r = _rnd.Next(cells.Count);
            cells[i] = cells[r];
            cells[r] = tmp;
        }
        return cells;
    }

    private int[,] MakeSecondDiagonalGrid(int dim, int player)
    {
        int[,] grid = new int[dim, dim];
        // first put in the diagonal for the winner
        for (int col = 0; col < dim; col++)
        {
            grid[col, col] = player;

        }
        DoRemainingMoves(grid, player);
        return grid;
    }

    public List<Move> FindSecondDiagonalGameSequence(int dim, int player)
    {
        int opposingPlayer = player == 1 ? -1 : 1;
        int[,] grid = MakeSecondDiagonalGrid(dim, player);
        // try again if the opposing player accidentally wins.
        while (IsWinner(grid, opposingPlayer).playerWon)
        {
            grid = MakeSecondDiagonalGrid(dim, player);
        }

        return ConvertGridToRandomMoves(grid);
    }

    /// <summary>
    /// Take a game grid and spit out a randomized list of the moves to recreate it step by step.
    /// </summary>
    /// <returns>The list of moves to make to recreate the grid</returns>
    /// <param name="grid">a tic tac toe game board with -1 or +1 representing moves for each cell</param>
    private List<Move> ConvertGridToRandomMoves(int[,] grid)
    {
        List<Move> movesList = new List<Move>();
        List<Move> player1Moves = new List<Move>();
        List<Move> player2Moves = new List<Move>();
        for (int col = 0; col < grid.GetLength(0); col++)
        {
            for (int row = 0; row < grid.GetLength(1); row++)
            {
                if (grid[col,row] == 1)
                {
                    player1Moves.Add(new Move(col, row));
                } else if (grid[col, row] == -1)
                {
                    player2Moves.Add(new Move(col, row));
                }
            }
        }
        ShuffleMoveList(player1Moves);
        ShuffleMoveList(player2Moves);
        while (player1Moves.Count > 0 || player2Moves.Count > 0)
        {
            if (player1Moves.Count > 0)
            {
                Move m = player1Moves[0];
                player1Moves.RemoveAt(0);
                movesList.Add(m);
            }
            if (player2Moves.Count > 0)
            {
                Move m = player2Moves[0];
                player2Moves.RemoveAt(0);
                movesList.Add(m);
            }
        }
        return movesList;
    }

     /// <summary>
     /// Look for winning patterns for either player and build out a score object to describe them.
     /// </summary>
     /// <returns>Scoring object describing wins for both players</returns>
     /// <param name="gameGrid">Game grid.</param>
    private static Score FindWins(int[,] gameGrid)
    {
        int targetScore = gameGrid.GetLength(0);
        int firstDiagonalTotal = 0;
        int secondDiagonalTotal = 0;
        Score outcome = new Score();
        outcome.winningCols = new bool[targetScore];
        outcome.winningRows = new bool[targetScore];
        for (int i = 0; i < gameGrid.GetLength(0); i++)
        {
            int Xtotal = 0;
            int Ytotal = 0;
            for (int j = 0; j < gameGrid.GetLength(1); j++)
            {
                Xtotal += gameGrid[i, j];
                Ytotal += gameGrid[j, i];
                if (i == j)
                {
                    firstDiagonalTotal += gameGrid[i, j];
                    secondDiagonalTotal += gameGrid[i, ((gameGrid.GetLength(1) - 1) - j)];
                }
            }
            if (Xtotal == targetScore ||
                Ytotal == targetScore ||
                firstDiagonalTotal == targetScore ||
                secondDiagonalTotal == targetScore)
            {
                outcome.firstPlayerWins = true;
                outcome.isColWin = outcome.isColWin || (Ytotal == targetScore);
                outcome.isRowWin = outcome.isRowWin || (Xtotal == targetScore);
                outcome.firstDiagWin = outcome.firstDiagWin || (firstDiagonalTotal == targetScore);
                outcome.secondDiagWin = outcome.secondDiagWin || (secondDiagonalTotal == targetScore);
                outcome.winningRows[i] = outcome.winningRows[i] || (Xtotal == targetScore);
                outcome.winningCols[i] = outcome.winningCols[i] || (Ytotal == targetScore);
            }
            if (Xtotal == -targetScore ||
                Ytotal == -targetScore ||
                firstDiagonalTotal == -targetScore ||
                secondDiagonalTotal == -targetScore)
            {
                outcome.secondPlayerWins = true;
                outcome.isColWin = outcome.isColWin || (Ytotal == -targetScore);
                outcome.isRowWin = outcome.isRowWin || (Xtotal == -targetScore);
                outcome.firstDiagWin = outcome.firstDiagWin || (firstDiagonalTotal == -targetScore);
                outcome.secondDiagWin = outcome.secondDiagWin || (secondDiagonalTotal == -targetScore);
                outcome.winningRows[i] = outcome.winningRows[i] || (Xtotal == -targetScore);
                outcome.winningCols[i] = outcome.winningCols[i] || (Ytotal == -targetScore);
            }
        }
        outcome.winGrid = gameGrid;
        if (outcome.firstPlayerWins || outcome.secondPlayerWins)
        {
            return outcome;
        }
        return null;
    }

     /// <summary>
     /// Evaluate if the given player is the winner of the grid.
     /// </summary>
     /// <returns>Scoring object describing game outcome relative to player</returns>
     /// <param name="gameGrid">Game grid.</param>
     /// <param name="player">Player.</param>
    private static Score IsWinner(int[,] gameGrid, int player)
    {
        int targetScore = gameGrid.GetLength(0) * player;
        Score outcome = new Score();
        outcome.winningCols = new bool[gameGrid.GetLength(0)];
        outcome.winningRows = new bool[gameGrid.GetLength(1)];
        int firstDiagonalTotal = 0;
        int secondDiagonalTotal = 0;
        for (int i = 0; i < gameGrid.GetLength(0); i++)
        {
            int Xtotal = 0;
            int Ytotal = 0;
            for (int j = 0; j < gameGrid.GetLength(1); j++)
            {
                Xtotal += gameGrid[i, j];
                Ytotal += gameGrid[j, i];
                if (i == j)
                {
                    firstDiagonalTotal += gameGrid[i, j];
                    secondDiagonalTotal += gameGrid[i, ((gameGrid.GetLength(1) - 1) - j)];
                }
            }
            if (Xtotal == targetScore ||
                Ytotal == targetScore ||
                firstDiagonalTotal == targetScore ||
                secondDiagonalTotal == targetScore)
            {
                outcome.playerWon = true;
                outcome.isColWin = outcome.isColWin || (Ytotal == targetScore);
                outcome.isRowWin = outcome.isRowWin || (Xtotal == targetScore);
                outcome.firstDiagWin = outcome.firstDiagWin || (firstDiagonalTotal == targetScore);
                outcome.secondDiagWin = outcome.secondDiagWin || (secondDiagonalTotal == targetScore);
                outcome.winningRows[i] = outcome.winningRows[i] || (Xtotal == targetScore);
                outcome.winningCols[i] = outcome.winningCols[i] || (Ytotal == targetScore);
            }
        }
        outcome.winGrid = gameGrid;
        return outcome;
    }
}
