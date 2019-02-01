using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {
    [SerializeField] GameObject mainMenuElements;
    [SerializeField] GameObject winScreenElements;
    [SerializeField] PlayerSymbolSelector playerSymbolSelector;
    [SerializeField] GameObject darkPanel;
    [SerializeField] Text winMessage;

    public void MenuGrandExit()
    {
        if (!winScreenElements.activeSelf && !mainMenuElements.activeSelf)
        {
            // nothing to do so let's exit
            return;
        }
        if (winScreenElements.activeSelf)
        {
            winScreenElements.GetComponent<MenuGroupFader>().Hide();
        }
        if (mainMenuElements.activeSelf)
        {
            mainMenuElements.GetComponent<MenuGroupFader>().Hide();
        }
        playerSymbolSelector.GrandExit();
        darkPanel.GetComponent<MenuGroupFader>().Hide();
    }

    public void MenuGrandEntrance()
    {
        darkPanel.GetComponent<MenuGroupFader>().Show();
        mainMenuElements.GetComponent<MenuGroupFader>().Show();
        playerSymbolSelector.GrandEntrance();
    }

    /// <summary>
    /// show a message about the result of the game and display menu items to start a new one.
    /// </summary>
    /// <param name="message">Message.</param>
    public void ShowEndOfGameMenu(string message)
    {
        darkPanel.GetComponent<MenuGroupFader>().Show();
        winMessage.text = message;
        winScreenElements.GetComponent<MenuGroupFader>().Show();
        playerSymbolSelector.GrandEntrance();
    }

    public void UpdatePlayerSymbolSelector(string[] symbols)
    {
        playerSymbolSelector.UpdatePlayerSymbolText(symbols);
    }
}
