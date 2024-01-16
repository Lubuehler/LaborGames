using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{

    public void OnHostClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Host);
    }

    public void OnJoinClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Join);
    }

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Main);

    }
}
