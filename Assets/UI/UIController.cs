using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject multiplayerCanvas;
    [SerializeField] private GameObject hostCanvas;
    [SerializeField] private GameObject joinCanvas;
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private GameObject playerNameCanvas;
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private GameObject spectatorCanvas;
    [SerializeField] private GameObject endscreenCanvas;

    // Dialogs
    [SerializeField] private GameObject leaveDialogCanvas;
    [SerializeField] private GameObject directJoinDialogCanvas;
    [SerializeField] private GameObject exceptionDialog;

    public Dictionary<UIElement, GameObject> canvasDict = new Dictionary<UIElement, GameObject>();


    private UIElement? activeDialog;
    private UIElement? activeUIElement;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        canvasDict[UIElement.Main] = mainCanvas;
        canvasDict[UIElement.Multiplayer] = multiplayerCanvas;
        canvasDict[UIElement.Host] = hostCanvas;
        canvasDict[UIElement.Join] = joinCanvas;
        canvasDict[UIElement.Shop] = shopCanvas;
        canvasDict[UIElement.PlayerName] = playerNameCanvas;
        canvasDict[UIElement.Lobby] = lobbyCanvas;
        canvasDict[UIElement.Game] = hudCanvas;
        canvasDict[UIElement.Spectator] = spectatorCanvas;
        canvasDict[UIElement.Endscreen] = endscreenCanvas;
        canvasDict[UIElement.Endscreen] = endscreenCanvas;

        canvasDict[UIElement.DirectJoinDialog] = directJoinDialogCanvas;
        canvasDict[UIElement.LeaveDialog] = leaveDialogCanvas;
        canvasDict[UIElement.ExceptionDialog] = exceptionDialog;

        DataController.Instance.LoadData();
        if (string.IsNullOrEmpty(DataController.Instance.playerData.playerName))
        {
            ShowUIElement(UIElement.PlayerName);
        }
        else
        {
            ShowUIElement(UIElement.Main);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(activeDialog != null)
            {
                HideDialog(activeDialog.Value);
            }
            ShowDialog(UIElement.LeaveDialog);
        }
    }


    public void ShowUIElement(UIElement targetElement)
    {
        foreach (var canvasEntry in canvasDict)
        {
            var canvas = canvasEntry.Value;
            var isActive = canvasEntry.Key == targetElement;

            if(isActive)
            {
                activeUIElement = targetElement;
            }
            
            canvas.SetActive(isActive);
        }

        if (activeDialog != null)
        {
            HideDialog(activeDialog.Value);
        }
    }

    public void ShowDialog(UIElement uIElement)
    {
        canvasDict[uIElement].SetActive(true);
        activeDialog = uIElement;
    }

    public void HideDialog(UIElement uIElement)
    {
        canvasDict[uIElement].SetActive(false);
        activeDialog = null;
    }

    public void ShowExceptionDialog(UIElement uIElement, String message)
    {
        var gameObject = canvasDict[uIElement];
        gameObject.SetActive(true);
        gameObject.GetComponent<ExceptionDialog>().SetMessage(message);
        activeDialog = uIElement;
    }
}

public enum UIElement
{
    Main,
    Multiplayer,
    Host,
    Join,
    Game,
    Shop,
    PlayerName,
    Lobby,
    Spectator,
    Endscreen,

    DirectJoinDialog,
    LeaveDialog,
    ExceptionDialog,
}
