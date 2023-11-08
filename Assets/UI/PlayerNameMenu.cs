using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNameMenu : MonoBehaviour
{
    private string playerName;
    public void OnCreateClick()
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            DataController.Instance.playerData.playerName = playerName;
            DataController.Instance.SaveData();
            UIController.Instance.ShowUIElement(UIElement.Main);
        }

    }

    public void GetPlayerName(string input)
    {
        playerName = input;
    }
}
