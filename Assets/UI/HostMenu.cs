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
            GameController.Instance.StartSession(sessionName);
            MenuManager.Instance.ShowUIElement(UIElement.Game);
        }

    }

    public void GetSessionName(string input)
    {
        sessionName = input;
        Debug.Log(sessionName);
    }

    public void OnBackClick()
    {
        MenuManager.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
