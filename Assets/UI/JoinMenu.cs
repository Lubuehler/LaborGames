using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class JoinMenu : MonoBehaviour
{
    public string selectedSession = "";
    public Transform elementParent;
    public GameObject template;
    public GameObject placeholder;
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
    }



    public void OnDirectConnectClick()
    {
        UIController.Instance.ShowDialog(UIElement.DirectJoin);
    }

    public async void OnJoinGameClick()
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

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
