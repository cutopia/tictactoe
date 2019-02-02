using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void TestToolCallback(Dictionary<string, int> options);
public delegate void SimpleDel();
public delegate void CallbackDel(SimpleDel callback);
public struct Move
{
    public Button cell;
    public int col;
    public int row;

    public Move(Button pCell, int pCol, int pRow)
    {
        cell = pCell;
        col = pCol;
        row = pRow;
    }

    public Move(int pCol, int pRow)
    {
        col = pCol;
        row = pRow;
        cell = null;
    }
}

public class GameController : MonoBehaviour {
    [SerializeField] private GameObject gameBoard3x3;
    [SerializeField] private GameObject gameBoard4x4;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellHolderPrefab;
    [SerializeField] private GameObject undoButton;
    [SerializeField] private bool showDebugCellInfo = false;
    [SerializeField] private string[] playerSymbols = { "X", "O" };
    [SerializeField] private BigWin bigWin; 
    private GameObject _activeCellHolder;
    private string _currentTurnSymbol;
    private int[,] _gameGrid;
    private OutcomeFinder _outcomeFinder;
    private Button[,] _buttonGrid;
    private float _playbackSpeed = 0.3f;
    private int _playbackIndex = 0;
    private List<Move> _playbackMoves;
    private float _timeSinceLastCheatMove = 0;
    private SimpleDel _onUpdate;
    
    private Stack<Move> _gameMoves = new Stack<Move>();

    /**
     * Handler for 3x3 button click. Hides menu and shows/populates the 3x3 board.
     */
    public void OnSelect3x3Grid()
    {
        if (Debug.isDebugBuild)
        {
            QuickPlayTool.UpdateBoardSize(3);
        }
        gameBoard3x3.SetActive(true);
        gameBoard3x3.GetComponentInChildren<RibController>().AnimateRibs();
        gameBoard4x4.SetActive(false);
        GetComponent<MenuController>().MenuGrandExit();
        PopulateGameGrid(3, gameBoard3x3);
        StartNewGame(3);
    }

    /**
     * swap which symbol goes first
     */
    public void OnSwapSymbolClicked()
    {
        string swap = playerSymbols[0];
        playerSymbols[0] = playerSymbols[1];
        playerSymbols[1] = swap;
        GetComponent<MenuController>().UpdatePlayerSymbolSelector(playerSymbols);
    }

    /**
     * Handler for 4x4 button click. Hides menu and shows/populates the 3x3 board.
     */
    public void OnSelect4x4Grid()
    {
        if (Debug.isDebugBuild)
        {
            QuickPlayTool.UpdateBoardSize(4);
        }

        gameBoard4x4.SetActive(true);
        gameBoard4x4.GetComponentInChildren<RibController>().AnimateRibs();
        gameBoard3x3.SetActive(false);
        GetComponent<MenuController>().MenuGrandExit();
        PopulateGameGrid(4, gameBoard4x4);
        StartNewGame(4);
    }

    /**
     * handler for button allowing navigation back to title/main menu.
     */
    public void OnExitGameClicked()
    {
        // stop the exit button from doing stuff if the win fanfare is going.
        if (bigWin.gameObject.activeSelf)
        {
            return;
        }
        gameBoard3x3.SetActive(false);
        gameBoard4x4.SetActive(false);
        GetComponent<MenuController>().MenuGrandEntrance();
    }

    /**
     * since we have a stack of the moves, hooking that up to an undo button
     * this is the handler    
     */
    public void OnUndoClicked()
    {
        if (_gameMoves.Count > 0)
        {
            Move lastMove = _gameMoves.Pop();
            lastMove.cell.GetComponentInChildren<Text>().text = "";
            _currentTurnSymbol = _currentTurnSymbol == playerSymbols[0] ? playerSymbols[1] : playerSymbols[0];
            _gameGrid[lastMove.col, lastMove.row] = 0;
            lastMove.cell.interactable = true;
            if (_gameMoves.Count == 0)
            {
                DeactivateUndoButton();
            }
        }
    }

    private void DeactivateUndoButton()
    {
        undoButton.GetComponent<Button>().interactable = false;
        undoButton.GetComponent<CanvasGroup>().alpha = 0.6f;
    }

    private void ActivateUndoButton()
    {
        undoButton.GetComponent<Button>().interactable = true;
        undoButton.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    /**
     * Handler for game grid cells being clicked
     * places the appropriate symbol in the cell, checks for win, and then switches to next turn's symbol.
     * @param cell (Button) the cell that was clicked
     * @param col (int) column in game grid for this cell
     * @param row (int) row in game grid for this cell
     */
    public void OnCellClicked(Button cell, int col, int row)
    {
        ActivateUndoButton();

        Text cellText = cell.GetComponentInChildren<Text>();
        cellText.text = _currentTurnSymbol;
        cellText.GetComponent<Animation>().Play("CellTextAnim");

        cell.interactable = false;
        // update game grid with new symbol then check for win
        _gameMoves.Push(new Move(cell, col, row));
        _gameGrid[col, row] = _currentTurnSymbol == playerSymbols[0] ? 1 : -1;
        if (_outcomeFinder.IsWinner(_currentTurnSymbol, _gameGrid, playerSymbols))
        {
            HandleWinFanfare();
        }
        else
        {
            // see if the game is a draw
            if (_gameMoves.Count == _gameGrid.Length)
            {
                GetComponent<MenuController>().ShowEndOfGameMenu("The game ended in a draw.");
            }
            // if we don't have a winner yet, swap to next symbol.
            _currentTurnSymbol = _currentTurnSymbol == playerSymbols[0] ? playerSymbols[1] : playerSymbols[0];
        }
    }

    private void HandleWinFanfare()
    {
        foreach (Button cell in _buttonGrid)
        {
            cell.interactable = false;
        }
        List<Vector2Int> winningGridLocations = _outcomeFinder.GetWinningCells(_gameGrid, _currentTurnSymbol == playerSymbols[0] ? 1 : -1);
        List<Text> winCells = new List<Text>();
        foreach (Vector2Int l in winningGridLocations)
        {
            Text cellText = _buttonGrid[l.x, l.y].GetComponentInChildren<Text>();
            cellText.GetComponent<Animation>().Play("WinCellEffect");
            winCells.Add(cellText);
        }
        StartCoroutine(onComplete(winCells[0].GetComponent<Animation>(), () => bigWin.PlayBigWinAnimation(() => GetComponent<MenuController>().ShowEndOfGameMenu("CONGRATULATIONS\n" + _currentTurnSymbol + " WINS THE GAME!"))));
    }

    IEnumerator onComplete(Animation anim, SimpleDel callback)
    {
        while (anim.isPlaying)
        {
            yield return null;
        }

        callback();
    }

    /**
     * Creates a holder element for the cells and then evenly plops dim by dim cells into it.
     * Attaches listeners to each cell (button) 
     * @param dim (int) specifies how many cells in one dimension of the square game board
     * @param board (GameObject) the element to build our gameboard into. 
     */
    private void PopulateGameGrid(int dim, GameObject board)
    {
        // clear the board if we already have one from previous game.
        if (_activeCellHolder)
        {
            Destroy(_activeCellHolder);
        }
        // create and position a container for the game cells that is the proper dimension of the baord.
        _activeCellHolder = Instantiate(cellHolderPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
        _activeCellHolder.transform.SetParent(board.transform);
        _activeCellHolder.transform.SetAsFirstSibling();
        _activeCellHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        _activeCellHolder.transform.localScale = new Vector3(1f, 1f, 1f);
        _buttonGrid = new Button[dim, dim];
        float width = _activeCellHolder.GetComponent<RectTransform>().rect.width / dim;
        float height = _activeCellHolder.GetComponent<RectTransform>().rect.height / dim;
        float xAdjust = (dim % 2 == 0) ? width * 0.5f : 0f;
        float yAdjust = (dim % 2 == 0) ? height * 0.5f : 0f;
        // create each game cell and give it a click listener
        for (int row = 0; row < dim; row++)
        {
            for (int col = 0; col < dim; col++)
            {
                GameObject instance = Instantiate(cellPrefab) as GameObject;
                instance.transform.SetParent(_activeCellHolder.transform);

                instance.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
                instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(width + xAdjust + (col * -width), height + yAdjust + (row * -height));
                instance.transform.localScale = new Vector3(1f, 1f, 1f);
                int cellCol = col;
                int cellRow = row;

                if (showDebugCellInfo && Debug.isDebugBuild)
                {
                    GameObject debugContainer = new GameObject("debugInfo");
                    Text debugText = debugContainer.AddComponent<Text>();
                    debugContainer.transform.SetParent(instance.transform);
                    debugContainer.transform.localPosition = new Vector3(0f, 0f, 0f);
                    debugText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                    debugText.text = "col: " + col + ", row: " + row;
                    debugText.color = Color.white;
                }
                _buttonGrid[cellCol, cellRow] = instance.GetComponent<Button>();
                instance.GetComponent<Button>().onClick.AddListener(() => OnCellClicked(instance.GetComponent<Button>(), cellCol, cellRow));
            }
        }
    }

    private void CheatHandler(Dictionary<string, int> options)
    {
        Debug.Log("CheatHandler called");
        if (QuickPlayTool.GetBoardSize() == 4)
        {
            OnSelect4x4Grid();
        }
        else
        {
            OnSelect3x3Grid();
        }
        int player = options["whoWins"] == 0 ? 1 : -1;

        // start appropriate sequence of moves for selected game outcome.
        if (options["winType"] == 3)
        {
            _playbackMoves = _outcomeFinder.FindDrawGameSequence(_gameGrid.GetLength(0));
        } else if (options["winType"] == 2)
        {
            if (options["diagonalDir"] == 0)
            {
                _playbackMoves = _outcomeFinder.FindFirstDiagonalGameSequence(_gameGrid.GetLength(0), player);
            }
            else
            {
                _playbackMoves = _outcomeFinder.FindSecondDiagonalGameSequence(_gameGrid.GetLength(0), player);
            }
        } else if (options["winType"] == 0)
        {
            // row win
            _playbackMoves = _outcomeFinder.FindRowGameSequence(_gameGrid.GetLength(0), player, options["row"]);
        } else if (options["winType"] == 1)
        {
            // col win
            _playbackMoves = _outcomeFinder.FindColGameSequence(_gameGrid.GetLength(0), player, options["col"]);
        }
        _playbackIndex = 0;
        _onUpdate += DoCheatMove;
    }

    /**
     * Take care of setting the game state back to the game beginning.
     */
    private void StartNewGame(int dim)
    {
        _currentTurnSymbol = playerSymbols[0];
        _gameGrid = new int[dim, dim];
        _gameMoves.Clear();
        DeactivateUndoButton();
    }

    private void Update()
    {
        if (_onUpdate != null)
        {
            _onUpdate();
        }
    }

    private void DoCheatMove()
    {
        _timeSinceLastCheatMove += Time.deltaTime;
        if (_timeSinceLastCheatMove > _playbackSpeed)
        {
            if (_playbackMoves == null)
            {
                _onUpdate -= DoCheatMove;
                return;
            }
            _timeSinceLastCheatMove = 0.0f;
            int col = _playbackMoves[_playbackIndex].col;
            int row = _playbackMoves[_playbackIndex].row;
            Button cell = _buttonGrid[col, row];
            OnCellClicked(cell, col, row);
            _playbackIndex++;
            if (_playbackIndex >= _playbackMoves.Count)
            {
                _onUpdate -= DoCheatMove;
            }
        }
    }

    private void OnDisable()
    {
        if (Debug.isDebugBuild)
        {
            QuickPlayTool.HideWindow();
        }
    }

    private void Awake()
    {
        _outcomeFinder = new OutcomeFinder();
        if (Debug.isDebugBuild)
        {
            QuickPlayTool.ShowWindow(CheatHandler, bigWin.PlayBigWinAnimation);
        }
        GetComponent<MenuController>().UpdatePlayerSymbolSelector(playerSymbols);
        gameBoard3x3.SetActive(false);
        gameBoard4x4.SetActive(false);
    }
}
