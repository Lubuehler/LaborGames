using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject loading;
    [SerializeField] private Button multiplayerBtn;

    void OnEnable()
    {
        loading.SetActive(false);
        multiplayerBtn.interactable = true;
    }

    // Main menu actions
    public async void OnMultiplayerClick()
    {
        multiplayerBtn.interactable = false;
        loading.SetActive(true);

        GameController.Instance.CreateNetworkController();
        await NetworkController.Instance.JoinLobby();

        UIController.Instance.ShowUIElement(UIElement.Multiplayer);
    }

    public void OnSettingsClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Settings);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }


    // Debug
    public async void OnDebugJoinClick()
    {
        loading.SetActive(true);
        GameController.Instance.CreateNetworkController();
        await NetworkController.Instance.StartGame(Fusion.GameMode.Client, "TestSession");
        UIController.Instance.ShowUIElement(UIElement.Game);
    }

    public async void OnDebugHostClick()
    {
        loading.SetActive(true);
        GameController.Instance.CreateNetworkController();
        await NetworkController.Instance.StartGame(Fusion.GameMode.Host, "TestSession");
        LevelController.Instance.StartLevel();
        UIController.Instance.ShowUIElement(UIElement.Game);
    }
}
