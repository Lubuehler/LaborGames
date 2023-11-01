using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    public Canvas joinCanvas;

    public void OnHostClick()
    {
        MenuManager.Instance.ShowUIElement(UIElement.Host);
    }

    public void OnJoinClick()
    {
        MenuManager.Instance.ShowUIElement(UIElement.Join);
    }

    public void OnBackClick()
    {
        MenuManager.Instance.ShowUIElement(UIElement.Main);
    }
}
