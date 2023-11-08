using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostMenu : MonoBehaviour
{
    private string sessionName;
    public async void OnCreateClick()
    {
        if (!string.IsNullOrEmpty(sessionName))
        {
            await NetworkController.Instance.StartGame(Fusion.GameMode.Host, sessionName);
            UIController.Instance.ShowUIElement(UIElement.Lobby);
        }

    }

    public void GetSessionName(string input)
    {
        sessionName = input;
    }

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
