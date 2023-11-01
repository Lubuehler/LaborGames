using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button multiplayerBtn;

    void OnEnable()
    {
        multiplayerBtn.interactable = true;
    }

    // Main menu actions
    public async void OnMultiplayerClick()
    {
        multiplayerBtn.interactable = false;
        await GameController.Instance.JoinLobby();
        MenuManager.Instance.ShowUIElement(UIElement.Multiplayer);
    }



    public void OnDebugJoinClick()
    {
        GameController.Instance.JoinSession("TestSession");
        MenuManager.Instance.ShowUIElement(UIElement.Game);
    }

    public void OnDebugHostClick()
    {
        GameController.Instance.StartSession("TestSession");
        MenuManager.Instance.ShowUIElement(UIElement.Game);
    }
}
