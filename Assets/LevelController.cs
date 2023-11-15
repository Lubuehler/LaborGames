using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public class LevelController : NetworkBehaviour
{
    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();

    public static LevelController Instance;


    [Networked(OnChanged = nameof(GameStarted))]
    public bool gameStarted { get; set; }
    private static void GameStarted(Changed<LevelController> changed)
    {
        if(changed.Behaviour.gameStarted)
        {
            UIController.Instance.ShowUIElement(UIElement.Game);
        }
    }

    private int currentWave = 0;
    private float waveDuration = 30f;
    private float waveDurationIncrease = 2f;

    private float waveEndTime; // Time when the current wave will end
    private bool waveInProgress = false;

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

    public IEnumerator DelayedStartRoutine()
    {
        print("delayed start");
        yield return new WaitForSeconds(.5f);
        StartNextWave();
    }

    public override void Spawned()
    {
        gameStarted = false;
        if (!Runner.IsServer) return;

        enemySpawner = GetComponent<EnemySpawner>();
        enemySpawner.UpdateEnemySpawnRate(EnemyType.Drone, 1);

    }

    [Rpc]
    public void RPC_CheckReady()
    {
        bool allPlayersReady = true;
        List<PlayerRef> players = new List<PlayerRef>(Runner.ActivePlayers);
        foreach (PlayerRef player in players)
        {
            if (!Runner.GetPlayerObject(player).GetComponent<Player>().ready)
            {
                allPlayersReady = false;
            }
        }
        if (allPlayersReady && Runner.IsServer)
        {
            StartLevel();
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;
            livingPlayers = new List<Player>();
            foreach (PlayerRef player in players)
            {
                livingPlayers.Add(Runner.GetPlayerObject(player).GetComponent<Player>());
            }
        }
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartWave()
    {
        print("starting wave no: " + currentWave);
        currentWave++;
        StartCoroutine(WaveRoutine(waveDuration));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEndWave()
    {
        foreach(NetworkObject enemy in _spawnedEnemies)
        {
            Runner.Despawn(enemy);
        }
        EnterShoppingPhase();
    }


    private IEnumerator WaveRoutine(float duration)
    {
        if (!Runner.IsServer) yield break;

        Debug.Log($"Wave {currentWave} started.");

        RemainingWaveTime = duration;

        while (RemainingWaveTime > 0 )
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
        Debug.Log("Shop phase started. Player can buy stuff now.");
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
        if(livingPlayers.Count == 0) {
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
