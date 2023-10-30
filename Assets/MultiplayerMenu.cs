using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    public void OnHostClick()
    {
        MenuManager.Instance.ShowHost();
        Debug.Log("Host clicked");
    }

    public void OnJoinClick()
    {
        Debug.Log("Join clicked");
    }

    public void OnBackClick()
    {
        MenuManager.Instance.ShowMain();
        Debug.Log("Back clicked");
    }
}
