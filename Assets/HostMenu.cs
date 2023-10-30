using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostMenu : MonoBehaviour
{
    public void OnCreateClick()
    {
        MenuManager.Instance.ShowHost();
    }

    public void OnBackClick()
    {
        MenuManager.Instance.ShowMultiplayer();
        Debug.Log("Back clicked");
    }
}
