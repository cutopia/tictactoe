using UnityEngine;
using UnityEngine.UI;

public class PlayerSymbolSelector : MonoBehaviour {
    [SerializeField] private Text player1SymbolText;
    [SerializeField] private Text player2SymbolText;

    // Use this for initialization
    void Start()
    {
        GrandEntrance();
    }

    public void UpdatePlayerSymbolText(string[] symbols)
    {
        player1SymbolText.text = "Player 1 is " + symbols[0];
        player2SymbolText.text = "Player 2 is " + symbols[1];
    }

    public void GrandEntrance()
    {
        GetComponent<Animation>().Play("SweepInFromBottom");
    }

    public void GrandExit()
    {
        GetComponent<Animation>().Play("SweepOutToBottom");
    }
}
