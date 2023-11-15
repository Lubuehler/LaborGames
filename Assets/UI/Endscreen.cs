using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endscreen : MonoBehaviour
{
    public void OnReturnButtonClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Lobby);
    }

}
