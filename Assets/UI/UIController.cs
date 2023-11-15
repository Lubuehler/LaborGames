using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject mainCanvas;
    public GameObject multiplayerCanvas;
    public GameObject hostCanvas;
    public GameObject joinCanvas;
    public GameObject directJoinCanvas;
    public GameObject shopCanvas;
    public GameObject playerNameCanvas;
    public GameObject lobbyCanvas;
    public GameObject hudCanvas;
    public GameObject spectatorCanvas;
    public GameObject endscreenCanvas;

    public Dictionary<UIElement, GameObject> canvasDict = new Dictionary<UIElement, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        canvasDict[UIElement.Main] = mainCanvas;
        canvasDict[UIElement.Multiplayer] = multiplayerCanvas;
        canvasDict[UIElement.Host] = hostCanvas;
        canvasDict[UIElement.Join] = joinCanvas;
        canvasDict[UIElement.DirectJoin] = directJoinCanvas;
        canvasDict[UIElement.Shop] = shopCanvas;
        canvasDict[UIElement.PlayerName] = playerNameCanvas;
        canvasDict[UIElement.Lobby] = lobbyCanvas;
        canvasDict[UIElement.Game] = hudCanvas;
        canvasDict[UIElement.Spectator] = spectatorCanvas;
        canvasDict[UIElement.Endscreen] = endscreenCanvas;

        DataController.Instance.LoadData();
        if(string.IsNullOrEmpty(DataController.Instance.playerData.playerName))
        {   
            ShowUIElement(UIElement.PlayerName);
        } 
        else 
        {
            ShowUIElement(UIElement.Main);
        }
    }

    public void ShowUIElement(UIElement uIElement)
    {
        foreach (var canvas in canvasDict)
        {

            if (canvas.Key == uIElement)
            {
                canvas.Value.SetActive(true);
            }
            else
            {
                canvas.Value.SetActive(false);
            }

        }
    }

    public void ShowDialog(UIElement uIElement)
    {
        canvasDict[uIElement].SetActive(true);

    }

    public void HideDialog(UIElement uIElement)
    {
        canvasDict[uIElement].SetActive(false);

    }
}

public enum UIElement
{
    Main,
    Multiplayer,
    Host,
    Join,
    DirectJoin,
    Game,
    Shop,
    PlayerName,
    Lobby,
    Spectator,
    Endscreen
}
