using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{

  public List<SessionInfo> sessionList;

  private NetworkRunner _runner;

  public async Task<StartGameResult> JoinLobby()
  {
    _runner = GetComponent<NetworkRunner>();
    if (_runner == null)
    {
      _runner = gameObject.AddComponent<NetworkRunner>();
      _runner.ProvideInput = true;
    }
    return await _runner.JoinSessionLobby(SessionLobby.ClientServer);
  }

  public async void StartGame(GameMode mode, string sessionName)
  {
    // Create the Fusion runner and let it know that we will be providing user input
    _runner = GetComponent<NetworkRunner>();
    if (_runner == null)
    {
      _runner = gameObject.AddComponent<NetworkRunner>();
      _runner.ProvideInput = true;
    }

    // Start or join (depends on gamemode) a session with a specific name
    await _runner.StartGame(new StartGameArgs()
    {
      GameMode = mode,
      SessionName = sessionName,
      Scene = SceneManager.GetActiveScene().buildIndex,
      SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
      PlayerCount = 4
    });
    GameController.Instance.GameInitialized();
  }

  public PlayerRef GetLocalPlayer()
  {

    return _runner.LocalPlayer;
  }

  [SerializeField] private NetworkPrefabRef _playerPrefab;


  public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
  {
    if (runner.IsServer)
    {
      // Create a unique position for the player
      Vector2 spawnPosition = new Vector2(0, 0);
      Debug.Log("spawn: server");
      NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
      // Keep track of the player avatars so we can remove it when they disconnect
      GameController.Instance.AddPlayerChar(player, networkPlayerObject);
    }
  }

  public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
  {
    // Find and remove the players avatar
    NetworkObject networkObject = GameController.Instance.GetPlayerAvatar(player);
    if (networkObject != null)
    {
      runner.Despawn(networkObject);
      GameController.Instance.RemovePlayerChar(player);
    }
  }

  public void OnInput(NetworkRunner runner, NetworkInput input)
  {
    var data = new NetworkInputData();

    if (Input.GetKey(KeyCode.W))
      data.direction += Vector2.up;

    if (Input.GetKey(KeyCode.S))
      data.direction += Vector2.down;

    if (Input.GetKey(KeyCode.A))
      data.direction += Vector2.left;

    if (Input.GetKey(KeyCode.D))
      data.direction += Vector2.right;

    input.Set(data);
  }

  public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
  public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
  {
    Debug.Log(shutdownReason);
  }
  public void OnConnectedToServer(NetworkRunner runner) { }
  public void OnDisconnectedFromServer(NetworkRunner runner) { }
  public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
  //public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
  public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
  public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
  {
    this.sessionList = sessionList;
    Debug.Log("Session list size: " + sessionList.Count);
    for (int i = 0; i < sessionList.Count; i++)
    {
      Debug.Log(i + " " + sessionList[i]);
    }

  }
  public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
  public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
  //public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
  public void OnSceneLoadDone(NetworkRunner runner) { }
  public void OnSceneLoadStart(NetworkRunner runner) { }

  public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
  {
    throw new NotImplementedException();
  }

  public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
  {
    throw new NotImplementedException();
  }
}

public struct NetworkInputData : INetworkInput
{
  public Vector2 direction;
}