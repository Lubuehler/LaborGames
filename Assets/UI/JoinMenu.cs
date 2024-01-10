using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class JoinMenu : MonoBehaviour
{
    [SerializeField] private Transform elementParent;
    [SerializeField] private GameObject template;
    [SerializeField] private GameObject placeholder;

    private string selectedSession = "";
    private bool isDoubleClick = false;
    private float doubleClickTime = 0.5f;
    void OnEnable()
    {
        NetworkController.Instance.OnSessionListChanged += UpdateSessionList;
        UpdateSessionList();
    }

    private void OnDisable()
    {
        NetworkController.Instance.OnSessionListChanged -= UpdateSessionList;
    }

    public void UpdateSessionList()
    {
        for (int i = elementParent.childCount - 1; i >= 0; i--)
        {
            Transform child = elementParent.GetChild(i);
            Destroy(child.gameObject);
        }

        List<SessionInfo> sessionList = NetworkController.Instance.sessionList;

        for (int i = 0; i < sessionList.Count; i++)
        {
            GameObject g = Instantiate(template, elementParent);
            g.name = sessionList[i].Name;
            g.transform.GetChild(0).GetComponent<TMP_Text>().text = sessionList[i].Name;
            g.transform.GetChild(1).GetComponent<TMP_Text>().text = sessionList[i].PlayerCount + "/" + sessionList[i].MaxPlayers;
            Button button = g.GetComponent<Button>();
            button.onClick.AddListener(() => OnButtonClick(g));
        }

        if (sessionList.Count < 1)
        {
            placeholder.SetActive(true);
        }
        else
        {
            placeholder.SetActive(false);
        }
    }

    public void OnButtonClick(GameObject clickedButton)
    {
        selectedSession = clickedButton.name;
        if (!isDoubleClick)
        {
            isDoubleClick = true;
            Invoke(nameof(ResetDoubleClick), doubleClickTime);
        }
        else
        {
            JoinGame();
        }
    }

    private void ResetDoubleClick()
    {
        isDoubleClick = false;
    }

    private async void JoinGame()
    {
        if (!string.IsNullOrEmpty(selectedSession))
        {
            StartGameResult result = await NetworkController.Instance.StartGame(GameMode.Client, selectedSession);
            if (result.Ok)
            {
                UIController.Instance.ShowUIElement(UIElement.Lobby);
            }
        }
    }

    public void OnDirectConnectClick()
    {
        UIController.Instance.ShowDialog(UIElement.DirectJoin);
    }

    public void OnJoinGameClick()
    {
        JoinGame();
    }

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
