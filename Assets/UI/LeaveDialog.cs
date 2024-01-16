using Fusion;
using UnityEngine;

public class LeaveDialog : MonoBehaviour
{
    public void OnLeaveClick()
    {
        NetworkController.Instance?.GetComponent<NetworkRunner>().Shutdown(destroyGameObject: true);

        UIController.Instance.ShowUIElement(UIElement.Main);
        UIController.Instance.HideDialog(UIElement.LeaveDialog);
    }

    public void OnStayClick()
    {
        UIController.Instance.HideDialog(UIElement.LeaveDialog);
    }
}
