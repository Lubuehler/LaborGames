using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public class LevelController : NetworkBehaviour
{
    public static LevelController Instance;


    [Networked]
    public bool gameRunning { get; set; }
    public bool initialized { get; set; } = false;

    [Networked]
    public int currentWave { get; set; }
    private float waveDuration = 10f;
    private float waveDurationIncrease = 2f;

    private float waveEndTime; // Time when the current wave will end

    [Networked(OnChanged = nameof(ShowGame))]
    public bool waveInProgress { get; set; }

    [Networked]
    public float RemainingWaveTime { get; private set; }

    public Player localPlayer;

    public List<Player> players {get; private set; } = new List<Player>();

    public event Action OnPlayerListChanged;


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

    public override void Spawned()
    {
        this.gameRunning = false;
        this.waveInProgress = false;
        initialized = true;
        if (!Runner.IsServer) return;
        EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Drone, 1);
        

        List<PlayerRef> playerRefs = new List<PlayerRef>(Runner.ActivePlayers);
        foreach (PlayerRef player in playerRefs)
        {
            if (Runner.GetPlayerObject(player) == null) {  continue; }
            players.Add(Runner.GetPlayerObject(player).GetComponent<Player>());
        }
        OnPlayerListChanged?.Invoke();
    }

    [Rpc]
    public void RPC_Ready(NetworkObject networkObject, bool ready)
    {
        //if (!gameRunning)
        //{
        //    networkObject.GetComponent<Player>().RpcReset();
        //    networkObject.GetComponent<Player>().ready = ready;

        //}
        if (Runner.IsServer)
        {
            networkObject.GetComponent<Player>().ready = ready;
            

            bool allPlayersReady = true;
            List<PlayerRef> players = new List<PlayerRef>(Runner.ActivePlayers);
            foreach (PlayerRef player in players)
            {
                if (!Runner.GetPlayerObject(player).GetComponent<Player>().ready)
                {
                    allPlayersReady = false;
                }
                
            }
            if (allPlayersReady)
            {
                if (!gameRunning)
                {
                    
                    StartLevel();
                    Runner.SessionInfo.IsVisible = false;

                    foreach (PlayerRef player in players)
                    {
                        Runner.GetPlayerObject(player).GetComponent<Player>().ready = false;
                        
                    }
                }
                else
                {
                    StartNextWave();
                }
            }
        }
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartWave()
    {
        waveInProgress = true;
        currentWave++;
        StartCoroutine(WaveRoutine(waveDuration));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEndWave()
    {
        waveInProgress = false;
        EnemySpawner.Instance.RPC_DespawnEverything();
    }


    private IEnumerator WaveRoutine(float duration)
    {
        if (!Runner.IsServer) yield break;

        Debug.Log($"Wave {currentWave} started.");

        RemainingWaveTime = duration;

        while (RemainingWaveTime > 0 && waveInProgress)
        {
            // Assuming the game is not paused and you're counting down
            RemainingWaveTime -= 1;

            EnemySpawner.Instance.SpawnEnemy();
            yield return new WaitForSeconds(1);
        }

        RpcEndWave();
        print("wave ended");
        if (gameRunning)
        {
            print("shopping now");
            RpcEnterShoppingPhase();
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEnterShoppingPhase()
    {
        UIController.Instance.ShowUIElement(UIElement.Shop);
    }

    private void UpdateEnemyPool()
    {
        if (currentWave >= 0)
        {
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Jet, 1);
            //EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Drone, 3);
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Drone, 10);
        }
    }

    public void StartNextWave()
    {
        if (!Runner.IsServer) return;

        UpdateEnemyPool();
        waveDuration += currentWave; // Increase wave duration based on current wave
        RpcStartWave();
    }

    public void StartLevel()
    {
        StartNextWave();
        this.gameRunning = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcPlayerSpawned(Player player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            OnPlayerListChanged?.Invoke();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcPlayerDowned(Player player)
    {
        if (GetLivingPlayers().Count == 0)
        {
            RpcGameOver();
        }
        else if (localPlayer == player)
        {
           StartSpectator();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGameOver()
    {
        RpcEndWave();
        UIController.Instance.ShowUIElement(UIElement.Endscreen);
        print("Game Over");
        StopGame();
    }

    public void StartSpectator()
    {
        Player selectedPlayer = players.FirstOrDefault(player => player.GetComponent<Player>().isAlive);

        UIController.Instance.ShowUIElement(UIElement.Spectator);
        Camera.main.GetComponent<CameraScript>().target = selectedPlayer.GetComponent<NetworkObject>();
        print("Spectating");
    }

    public void StopGame()
    {
        foreach (var player in players)
        {
            player.RpcReset();
        }
        gameRunning = false;
    }

    public void RessurectPlayers()
    {
        foreach (Player player in players)
        {
            if (!player.isAlive)
            {
                player.RpcRessurect();
            }
        }
    }


    private static void ShowGame(Changed<LevelController> changed)
    {
        if (changed.Behaviour.waveInProgress)
        {
            if (NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>().isAlive)
            {
                UIController.Instance.ShowUIElement(UIElement.Game);
            }
            else
            {
                LevelController.Instance.StartSpectator();
            }
        }
    }

    public List<Player> GetLivingPlayers()
    {
        List<Player> livingPlayers = new List<Player>();
        foreach (Player p in players)
        {
            if ( p.isAlive )

            {
                livingPlayers.Add((Player)p);
            }
        }
        return livingPlayers;
    }

    public List<Player> GetDeadPlayers()
    {
        List<Player> deadPlayers = new List<Player>();
        foreach (Player p in players)
        {
            if (!p.isAlive)

            {
                deadPlayers.Add((Player)p);
            }
        }
        return deadPlayers;
    }
}
