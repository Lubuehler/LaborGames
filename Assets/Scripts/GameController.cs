using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using ExitGames.Client.Photon.StructWrapping;
using System.Threading.Tasks;


public class GameController : NetworkBehaviour
{

  public static GameController Instance;

  private bool _gameStarted = false;
  public GameObject networkController;
  public LevelController levelController;
  public Camera MainCamera;
  public Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

  public List<NetworkObject> dummyEnemies;
  public Player localPlayer;

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

    public async Task<StartGameResult> JoinLobby()
    {
        return await networkController.GetComponent<BasicSpawner>().JoinLobby();
    }

    public void StartSession(string sessionName)
    {
        if (!_gameStarted)
        {
            networkController.GetComponent<BasicSpawner>().StartGame(GameMode.Host, sessionName);
            _gameStarted = true;
        }
    }

    public void JoinSession(string sessionName)
    {
        if (!_gameStarted)
        {
            networkController.GetComponent<BasicSpawner>().StartGame(GameMode.Client, sessionName);
        }
    }

    public List<SessionInfo> GetSessions()
    {
        return networkController.GetComponent<BasicSpawner>().sessionList;
    }

    public void GameInitialized()
    {
       // levelController.StartLevel();
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
