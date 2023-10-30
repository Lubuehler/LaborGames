using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class JoinMenu : MonoBehaviour
{
    public string? selectedSession;
    public Transform elementParent;
    public GameObject template;
    public GameObject placeholder;
    void OnEnable()
    {
        for (int i = elementParent.childCount - 1; i >= 0; i--)
        {
            Transform child = elementParent.GetChild(i);
            Destroy(child.gameObject);
        }

        List<SessionInfo> sessionList = GameController.Instance.GetSessions();

        for (int i = 0; i < sessionList.Count; i++)
        {
            GameObject g = Instantiate(template, elementParent);
            g.name = sessionList[i].Name;
            g.transform.GetChild(0).GetComponent<TMP_Text>().text = sessionList[i].Name;
            g.transform.GetChild(1).GetComponent<TMP_Text>().text = sessionList[i].PlayerCount + "/" + sessionList[i].MaxPlayers;
            Button button = g.GetComponent<Button>();
            button.onClick.AddListener(() => OnButtonClick(g));
        }

        if(sessionList.Count < 1)
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
        MenuManager.Instance.ShowDialog(UIElement.DirectJoin);
    }

    public void OnJoinGameClick()
    {
        if (!string.IsNullOrEmpty(selectedSession))
        {
            GameController.Instance.JoinSession(selectedSession);
            MenuManager.Instance.ShowUIElement(UIElement.Game);
        }
    }

    public void OnBackClick()
    {
        MenuManager.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
