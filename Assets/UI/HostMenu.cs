using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostMenu : MonoBehaviour
{
    private string sessionName;
    public void OnCreateClick()
    {
        if (!string.IsNullOrEmpty(sessionName))
        {
            NetworkController.Instance.StartGame(Fusion.GameMode.Host, sessionName);
            UIController.Instance.ShowUIElement(UIElement.Game);
        }

    }

    public void GetSessionName(string input)
    {
        sessionName = input;
        Debug.Log(sessionName);
    }

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
