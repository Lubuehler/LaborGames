using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;


public class GameController : NetworkBehaviour
{
    public static GameController Instance;
    private bool _gameStarted = false;
    public GameObject networkController;
    public LevelController levelController;
    public Camera MainCamera;
    public Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnGUI()
    {
        if (!_gameStarted)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                networkController.GetComponent<BasicSpawner>().StartGame(GameMode.Host);
                _gameStarted = true;

                return;
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                networkController.GetComponent<BasicSpawner>().StartGame(GameMode.Client);

            }
        }
    }

    public void GameInitialized()
    {
        levelController.StartLevel();
        _gameStarted = true;
        print("game started");
    }


    public void AddPlayerChar(PlayerRef player, NetworkObject networkPlayerObject)
    {
        spawnedCharacters.Add(player, networkPlayerObject);
    }
    public void RemovePlayerChar(PlayerRef player)
    {
        spawnedCharacters.Remove(player);
    }
    public NetworkObject GetPlayerAvatar(PlayerRef player)
    {
        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            return networkObject;
        }
        else
        {
            return null;
        }

    }
}
