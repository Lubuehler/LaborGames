using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public class LevelController : NetworkBehaviour
{
    public GameObject dronePrefab;
    public static LevelController Instance;

    public GameObject dronePrefab;

    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    public List<NetworkObject> _spawnedCoins = new List<NetworkObject>();




    [Networked]
    public bool gameStarted { get; set; }

    [Networked]
    public int currentWave { get; set; }
    private float waveDuration = 30f;
    private float waveDurationIncrease = 2f;

    private float waveEndTime; // Time when the current wave will end

    [Networked(OnChanged = nameof(ShowGame))]
    public bool waveInProgress { get; set; }

    [Networked]
    public float RemainingWaveTime { get; private set; }

    public event Action onStartGame;

    public Player localPlayer;
    private List<Player> livingPlayers;
    private EnemySpawner enemySpawner;
    

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
        this.gameStarted = false;
        this.waveInProgress = false;
        if (!Runner.IsServer) return;

        enemySpawner = GetComponent<EnemySpawner>();
        enemySpawner.UpdateEnemySpawnRate(EnemyType.Drone, 1);

    }

    public IEnumerator DelayedStartRoutine()
    {
        yield return new WaitForSeconds(.5f);
        StartNextWave();
    }

    [Rpc]
    public void RPC_Ready(NetworkObject networkObject, bool ready)
    {
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
                if (!gameStarted)
                {
                    StartLevel();
                    Runner.SessionInfo.IsVisible = false;
                }
                else
                {
                    StartNextWave();
                }

                livingPlayers = new List<Player>();
                foreach (PlayerRef playerRef in players)
                {
                    Player player = Runner.GetPlayerObject(playerRef).GetComponent<Player>();
                    player.ready = false;
                    livingPlayers.Add(player);
                }

            }
        }
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartWave()
    {
        print("starting wave no: " + currentWave);
        waveInProgress = true;
        currentWave++;
        StartCoroutine(WaveRoutine(waveDuration));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEndWave()
    {
        foreach (NetworkObject coin in _spawnedCoins)
        {
            Runner.Despawn(coin);
        }
        foreach (NetworkObject enemy in _spawnedEnemies)
        {
            Runner.Despawn(enemy);
        }
        waveInProgress = false;
        EnterShoppingPhase();
    }


    private IEnumerator WaveRoutine(float duration)
    {
        if (!Runner.IsServer) yield break;

        Debug.Log($"Wave {currentWave} started.");

        RemainingWaveTime = duration;

        while (RemainingWaveTime > 0)
        {
            // Assuming the game is not paused and you're counting down
            RemainingWaveTime -= 1;

            enemySpawner.SpawnEnemy();
            yield return new WaitForSeconds(1);
        }

        RpcEndWave();
    }



    private void EnterShoppingPhase()
    {
        UIController.Instance.ShowUIElement(UIElement.Shop);
    }

    private void UpdateEnemyPool()
    {
        if (currentWave >= 0)
        {
            enemySpawner.UpdateEnemySpawnRate(EnemyType.Jet, 2);
            enemySpawner.UpdateEnemySpawnRate(EnemyType.Drone, 3);

        }
    }

    public void StartNextWave()
    {
        if (!Object.HasStateAuthority) return;

        UpdateEnemyPool();
        waveDuration += currentWave; // Increase wave duration based on current wave
        RpcStartWave();
    }

    public void StartLevel()
    {
        StartNextWave();
        this.gameStarted = true;
    }

    public void EnemyDefeated(Enemy enemy)
    {
        if (Runner.IsServer)
        {
            _spawnedEnemies.Remove(enemy.GetComponentInParent<NetworkObject>());
            Runner.Despawn(enemy.GetComponentInParent<NetworkObject>());
            CheckForLevelCompletion();
        }
    }

    public void PlayerDowned(Player player)
    {
        livingPlayers.Remove(player);
        if (livingPlayers.Count == 0)
        {
            RpcPauseGame();
        }
    }

    private void CheckForLevelCompletion()
    {
        if (_spawnedEnemies.Count == 0)
        {
            Debug.Log("Level Completed!");
            // Handle level completion logic here
        }
    }


    private static void ShowGame(Changed<LevelController> changed)
    {
        if (changed.Behaviour.waveInProgress)
        {
            UIController.Instance.ShowUIElement(UIElement.Game);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPauseGame()
    {
        Time.timeScale = 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUnpauseGame()
    {
        Time.timeScale = 1;
    }
}
