using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    public Transform elementParent;
    public GameObject row;
    public TMP_Text header;
    public TMP_Text playerCount;

    private bool ready;
    public GameObject readyButton;

    void OnEnable()
    {
        ready = false;
        readyButton.GetComponent<Button>().interactable = true;
        readyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Ready";

        header.text = NetworkController.Instance.currentSession.Name;
        NetworkController.Instance.OnPlayerListChanged += UpdatePlayerList;
        UpdatePlayerList();
    }

    private void OnDisable()
    {
        NetworkController.Instance.OnPlayerListChanged -= UpdatePlayerList;
    }

    public void UpdatePlayerList()
    {
        for (int i = elementParent.childCount - 1; i >= 0; i--)
        {
            Transform child = elementParent.GetChild(i);
            Destroy(child.gameObject);
        }

        NetworkRunner _runner = NetworkController.Instance.GetComponent<NetworkRunner>();
        List<PlayerRef> players = new List<PlayerRef>(_runner.ActivePlayers);


        foreach (PlayerRef player in players)
        {
            NetworkObject networkObject = _runner.GetPlayerObject(player);
            GameObject g = Instantiate(row, elementParent);
            g.transform.GetChild(0).GetComponent<TMP_Text>().text = networkObject.GetComponent<Player>().playerName;
            if (networkObject.GetComponent<Player>().ready)
            {
                g.transform.GetChild(1).gameObject.SetActive(true);
            }
        }

        playerCount.text = players.Count + "/4";
    }

    private bool buttonEnabled = true;

    public void OnReadyClick()
    {
        if (buttonEnabled)
        {
            buttonEnabled = false;
            readyButton.GetComponent<Button>().interactable = false;
            if (ready)
            {
                ready = false;
                readyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Ready";
            }
            else
            {
                ready = true;
                readyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Unready";
            }
            LevelController.Instance.RPC_Ready(NetworkController.Instance.GetLocalPlayerObject(), ready);
            StartCoroutine(EnableButtonAfterDelay(1.0f));
        }
    }

    private IEnumerator EnableButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        readyButton.GetComponent<Button>().interactable = true;
        buttonEnabled = true;
    }

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Multiplayer);
    }
}
