using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Main menu actions
    public void OnMultiplayerClick()
    {
        MenuManager.Instance.ShowMultiplayer();
    }
}
