using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QuickPlayTool : EditorWindow {
    private static TestToolCallback _registeredCallback;
    private static CallbackDel _registedBigWinCallback;
    private static int toolbarInt;
    private static string[] toolbarStrings = new string[] { "Row", "Column", "Diagonal", "Draw" };
    private static int diagonalDirectionInt;
    private static string[] diagonalStrings = new string[] { "\\", "/" };
    private static int whichPlayerWinsInt;
    private static string[] playerStrings = new string[] { "Player1", "Player2" };
    private static int boardDim;
    private static int boardSizeInt;
    private static string[] boardSizeStrings = new string[] { "3x3", "4x4" };
    private static string[] rowColStrings = new string[] { "1", "2", "3" };
    private static int rowSelectInt;
    private static int colSelectInt;

    public static void ShowWindow(TestToolCallback callback, CallbackDel bigWinCallback)
    {
        var f = EditorWindow.focusedWindow;
        _registeredCallback = callback;
        _registedBigWinCallback = bigWinCallback;
        var window = EditorWindow.GetWindow(typeof(QuickPlayTool), true, "Quick Play Tool");
        window.minSize = new Vector2(270, 400);
        f.Focus();
    }

    public static void HideWindow()
    {
        EditorWindow.GetWindow(typeof(QuickPlayTool)).Close();
    }

    public static void UpdateBoardSize(int dim)
    {
        boardDim = dim;
        if (boardDim == 4)
        {
            rowColStrings = new string[] { "1", "2", "3", "4" };
            boardSizeInt = 1;
        }
        else
        {
            rowColStrings = new string[] { "1", "2", "3" };
            if (rowSelectInt > 2)
            {
                rowSelectInt = 2;
            }
            if (colSelectInt > 2)
            {
                colSelectInt = 2;
            }
            boardSizeInt = 0;
        }
    }

    public static int GetBoardSize()
    {
        return boardDim;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Choose desired game outcome:", EditorStyles.boldLabel);
        toolbarInt = GUI.Toolbar(new Rect(10, 30, 250, 30), toolbarInt, toolbarStrings);
        if (toolbarInt == 0)
        {
            SetupRowWinGui();
        }
        else if (toolbarInt == 1)
        {
            SetupColWinGui();
        }
        else if (toolbarInt == 2)
        {
            SetupDiagonalWinGui();
        }
        GUI.Label(new Rect(10, 190, 200, 20), "Choose board size", EditorStyles.boldLabel);
        boardSizeInt = GUI.Toolbar(new Rect(10, 210, 125, 30), boardSizeInt, boardSizeStrings);
        if (boardSizeInt == 1 && boardDim != 4)
        {
            UpdateBoardSize(4);
        }
        else if (boardSizeInt == 0 && boardDim != 3)
        {
            UpdateBoardSize(3);
        }

        GUI.Label(new Rect(10, 310, 230, 20), "Click button to play the selected game:", EditorStyles.boldLabel);
        if (GUI.Button(new Rect(10, 330, 125, 30), "Start Quick Play Now"))
        {
            Dictionary<string, int> opts = new Dictionary<string, int>();
            opts.Add("winType", toolbarInt);
            opts.Add("diagonalDir", diagonalDirectionInt);
            opts.Add("winIndex", 0);
            opts.Add("whoWins", whichPlayerWinsInt);
            opts.Add("col", colSelectInt);
            opts.Add("row", rowSelectInt);
            _registeredCallback(opts);
        }
        if (GUI.Button(new Rect(140, 330, 125, 30), "Demo Big Win"))
        {
            _registedBigWinCallback(null);
        }
    }

    private void SetupDiagonalWinGui()
    {
        GUI.Label(new Rect(10, 240, 200, 20), "Which player wins?", EditorStyles.boldLabel);
        whichPlayerWinsInt = GUI.Toolbar(new Rect(10, 260, 125, 30), whichPlayerWinsInt, playerStrings);

        GUI.Label(new Rect(10, 80, 200, 20), "Choose diagonal direction for win:", EditorStyles.boldLabel);
        diagonalDirectionInt = GUI.Toolbar(new Rect(10, 100, 125, 30), diagonalDirectionInt, diagonalStrings);
    }

    private void SetupColWinGui()
    {
        GUI.Label(new Rect(10, 80, 200, 20), "Choose column for win:", EditorStyles.boldLabel);
        colSelectInt = GUI.Toolbar(new Rect(10, 100, 125, 30), colSelectInt, rowColStrings);


        GUI.Label(new Rect(10, 240, 200, 20), "Which player wins?", EditorStyles.boldLabel);
        whichPlayerWinsInt = GUI.Toolbar(new Rect(10, 260, 125, 30), whichPlayerWinsInt, playerStrings);
    }

    private void SetupRowWinGui()
    {
        GUI.Label(new Rect(10, 80, 200, 20), "Choose row for win:", EditorStyles.boldLabel);
        rowSelectInt = GUI.Toolbar(new Rect(10, 100, 125, 30), rowSelectInt, rowColStrings);


        GUI.Label(new Rect(10, 240, 200, 20), "Which player wins?", EditorStyles.boldLabel);
        whichPlayerWinsInt = GUI.Toolbar(new Rect(10, 260, 125, 30), whichPlayerWinsInt, playerStrings);
    }
}
