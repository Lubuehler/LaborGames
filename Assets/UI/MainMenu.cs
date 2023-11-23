using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button multiplayerBtn;

    public Toast toast;

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

    public void TestClick()
    {
        toast.Show();
    }



    public async void OnDebugJoinClick()
    {
        await NetworkController.Instance.StartGame(Fusion.GameMode.Client, "TestSession");
        UIController.Instance.ShowUIElement(UIElement.Game);
    }

    public async void OnDebugHostClick()
    {
        await NetworkController.Instance.StartGame(Fusion.GameMode.Host, "TestSession");
        LevelController.Instance.StartLevel();
        UIController.Instance.ShowUIElement(UIElement.Game);
    }
}
