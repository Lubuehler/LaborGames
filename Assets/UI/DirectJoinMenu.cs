using Fusion;
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
            NetworkController.Instance.StartGame(GameMode.Client, sessionName);

            UIController.Instance.ShowUIElement(UIElement.Game);
        }

    }

    public void GetSessionName(string input)
    {
        sessionName = input;
        Debug.Log(sessionName);
    }

    public void OnCancelClick()
    {
        UIController.Instance.HideDialog(UIElement.DirectJoin);
    }
}
