using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;


public class GameController : MonoBehaviour
{

    public static GameController Instance;

    private bool _gameStarted = false;
    public GameObject networkController;
    public Camera MainCamera;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public List<NetworkObject> dummyEnemies;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(this.gameObject);
        }
    }

    public List<NetworkObject> GetEnemies()
    {
        return dummyEnemies;
    }


    public void OnGUI()
    {
        if (!_gameStarted)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                networkController.GetComponent<BasicSpawner>().StartGame(GameMode.Host, this);
                _gameStarted = true;
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                networkController.GetComponent<BasicSpawner>().StartGame(GameMode.Client, this);
                _gameStarted = true;
            }
        }
    }

    public void AddPlayerChar(PlayerRef player, NetworkObject networkPlayerObject)
    {
        _spawnedCharacters.Add(player, networkPlayerObject);
    }
    public void RemovePlayerChar(PlayerRef player)
    {
        _spawnedCharacters.Remove(player);
    }
    public NetworkObject GetPlayerAvatar(PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            return networkObject;
        }
        else
        {
            return null;
        }

    }
}
