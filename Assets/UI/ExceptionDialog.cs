using System;
using TMPro;
using UnityEngine;

public class ExceptionDialog : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public void SetMessage(String message)
    {
        text.text = message;
    }

    public void OnOkClick()
    {
        UIController.Instance.HideDialog(UIElement.ExceptionDialog);
    }
}
