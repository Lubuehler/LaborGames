using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private Transform elementParent;
    [SerializeField] private GameObject cell;
    [SerializeField] private TMP_Text header;
    [SerializeField] private TMP_Text playerCount;
    [SerializeField] private GameObject readyButton;

    private bool ready;

    void OnEnable()
    {
        ready = false;
        readyButton.GetComponent<Button>().interactable = true;
        readyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Ready";

        header.text = NetworkController.Instance.currentSession.Name;
        NetworkController.Instance.OnPlayerListChanged += UpdatePlayerList;
        buttonEnabled = true;
        UpdatePlayerList();
    }

    private void OnDisable()
    {
        NetworkController.Instance.OnPlayerListChanged -= UpdatePlayerList;
    }

    public void UpdatePlayerList()
    {
        while (elementParent.childCount > 0)
        {
            DestroyImmediate(elementParent.transform.GetChild(0).gameObject);
        }

        NetworkRunner _runner = NetworkController.Instance.GetComponent<NetworkRunner>();
        List<PlayerRef> players = new List<PlayerRef>(_runner.ActivePlayers);

        foreach (PlayerRef player in players)
        {
            NetworkObject networkObject = _runner.GetPlayerObject(player);
            
            GameObject g = Instantiate(cell, elementParent);
            g.transform.GetChild(0).GetChild(0).GetComponentInChildren<TMP_Text>().text = networkObject.GetComponent<Player>().playerName;

            // Checkmark
            var lobbyReady = networkObject.GetComponent<Player>().lobbyReady;
            g.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(lobbyReady);

            // Self player indicator
            var playerIsLocal = player.Equals(_runner.LocalPlayer);
            g.transform.GetChild(0).GetChild(1).gameObject.SetActive(playerIsLocal);
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
            StartCoroutine(EnableButtonAfterDelay(1.0f));
            LevelController.Instance.RPC_Ready(NetworkController.Instance.GetLocalPlayerObject(), ready);
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
        UIController.Instance.ShowDialog(UIElement.LeaveDialog);
    }
}
