using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DirectJoinMenu : MonoBehaviour
{
    private string sessionName;

    public TMP_InputField inputField;

    void OnEnable()
    {
        sessionName = "";
        inputField.Select();
        inputField.text = "";
    }
    public async void OnDirectConnectClick()
    {
        if (!string.IsNullOrEmpty(sessionName))
        {
            StartGameResult result = await NetworkController.Instance.StartGame(GameMode.Client, sessionName);
            if (result.Ok)
            {
                UIController.Instance.ShowUIElement(UIElement.Lobby);
            }
            UIController.Instance.HideDialog(UIElement.DirectJoinDialog);
        }
    }

    public void GetSessionName(string input)
    {
        sessionName = input;
    }

    public void OnCancelClick()
    {
        UIController.Instance.HideDialog(UIElement.DirectJoinDialog);
    }
}
