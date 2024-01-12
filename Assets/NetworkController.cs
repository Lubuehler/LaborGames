using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkController Instance;
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private GameObject levelControllerPrefab;

    // Session stuff
    public List<SessionInfo> sessionList;
    public SessionInfo currentSession;

    // Actions
    public event Action OnSessionListChanged;
    public event Action OnPlayerListChanged;

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

    public async Task<StartGameResult> StartGame(GameMode mode, string sessionName)
    {
        _runner = GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        StartGameResult result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 4
        });
        currentSession = _runner.SessionInfo;
        return result;
    }

    private int playerCounter = 0;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            _runner.Spawn(levelControllerPrefab);
        }
        if (runner.IsServer)
        {
            Vector2 spawnPosition = new Vector2(0, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            networkPlayerObject.GetComponent<Player>().lobbyNo = playerCounter++;

            _runner.SetPlayerObject(player, networkPlayerObject);

            OnPlayerListChanged?.Invoke();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        runner.Disconnect(player);
        runner.Despawn(GetPlayerAvatar(player));
        OnPlayerListChanged?.Invoke();
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

        data.buttons.Set(MyButtons.SpecialAttack, Input.GetKey(KeyCode.Space));

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log(shutdownReason);
        if(runner.IsClient)
        {
            UIController.Instance.ShowUIElement(UIElement.Main);
            var message = "ShutdownReason: " + shutdownReason;
            UIController.Instance.ShowExceptionDialog(UIElement.ExceptionDialog, message);
        }
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        this.sessionList = sessionList;
        OnSessionListChanged?.Invoke();
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { print("scene load done"); }
    public void OnSceneLoadStart(NetworkRunner runner) { print("scene load started"); }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public NetworkObject GetLocalPlayerObject()
    {
        return GetPlayerAvatar(_runner.LocalPlayer);
    }

    public NetworkObject GetPlayerAvatar(PlayerRef player)
    {
        if (_runner.TryGetPlayerObject(player, out NetworkObject networkObject))
        {
            return networkObject;
        }
        else
        {
            return null;
        }
    }

    public void TriggerPlayerListChanged()
    {
        OnPlayerListChanged?.Invoke();
    }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 direction;
    public NetworkButtons buttons;
}

enum MyButtons
{
    SpecialAttack = 0,
}