using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public Canvas mainCanvas;
    public Canvas multiplayerCanvas;
    public Canvas hostCanvas;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        mainCanvas.enabled = true;
        multiplayerCanvas.enabled = false;
        hostCanvas.enabled = false;
    }

    public void ShowMain()
    {
        mainCanvas.enabled = true;
        multiplayerCanvas.enabled = false;
        hostCanvas.enabled = false;
    }

    public void ShowMultiplayer()
    {
        multiplayerCanvas.enabled = true;
        mainCanvas.enabled = false;
        hostCanvas.enabled = false;
    }

    public void ShowHost()
    {
        hostCanvas.enabled = true;
        mainCanvas.enabled = false;
        multiplayerCanvas.enabled = false;
    }
}
