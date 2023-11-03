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
        await NetworkController.Instance.JoinLobby();
        UIController.Instance.ShowUIElement(UIElement.Multiplayer);

    }



    public void OnDebugJoinClick()
    {
        NetworkController.Instance.StartGame(Fusion.GameMode.Client, "TestSession");
        UIController.Instance.ShowUIElement(UIElement.Game);
    }

    public void OnDebugHostClick()
    {
        NetworkController.Instance.StartGame(Fusion.GameMode.Host, "TestSession");
        UIController.Instance.ShowUIElement(UIElement.Game);
    }
}
