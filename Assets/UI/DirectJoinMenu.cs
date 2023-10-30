using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectJoinMenu : MonoBehaviour
{
    private string sessionName;
    public void OnDirectConnectClick()
    {
        if (!string.IsNullOrEmpty(sessionName))
        {
            GameController.Instance.JoinSession(sessionName);
            MenuManager.Instance.ShowUIElement(UIElement.Game);
        }

    }

    public void GetSessionName(string input)
    {
        sessionName = input;
        Debug.Log(sessionName);
    }

    public void OnCancelClick()
    {
        MenuManager.Instance.HideDialog(UIElement.DirectJoin);
    }
}
