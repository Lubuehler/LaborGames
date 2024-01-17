using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelController : NetworkBehaviour
{
    public static LevelController Instance;


    [Networked]
    public bool gameRunning { get; set; }
    public bool initialized { get; set; } = false;

    [Networked]
    public int currentWave { get; set; }
    private float waveDuration = 30f;
    private float waveDurationIncrease = 2f;

    private float waveEndTime; // Time when the current wave will end

    //[Networked(OnChanged = nameof(ShowGame))]
    [Networked]
    public bool waveInProgress { get; set; }

    [Networked]
    public float RemainingWaveTime { get; private set; }

    public Player localPlayer;

    public List<Player> players { get; private set; } = new List<Player>();

    public event Action OnPlayerListChanged;
    public event Action OnCurrentWaveChanged;

    [Networked]
    public bool isShopping { get; set; }
    public bool isSpawning = true;

    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float enemySpawnTime = 1;

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

    public override void Spawned()
    {
        gameRunning = false;
        waveInProgress = false;
        initialized = true;

        RPC_TriggerPlayerListChanged();
    }

    [Rpc]
    public void RPC_Ready(NetworkObject networkObject, bool ready)
    {
        if (Runner.IsServer)
        {
            networkObject.GetComponent<Player>().lobbyReady = ready;


            bool allPlayersReady = true;
            foreach (Player player in players)
            {
                if (!player.lobbyReady)
                {
                    allPlayersReady = false;
                }

            }
            if (allPlayersReady)
            {
                StartLevel();
                RpcShowGame();
                Runner.SessionInfo.IsVisible = false;
                foreach (Player player in players)
                {
                    player.lobbyReady = false;
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ShopReady(NetworkObject playerObject, bool ready)
    {
        playerObject.GetComponent<Player>().shopReady = ready;


        bool allPlayersReady = true;
        foreach (Player player in players)
        {
            if (!player.shopReady)
            {
                allPlayersReady = false;
            }

        }
        if (allPlayersReady)
        {
            RpcEndShoppingPhase();

            StartNextWave();
            foreach (Player player in players)
            {
                player.shopReady = false;

            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcEndShoppingPhase()
    {

        RpcShowGame();

        isShopping = false;
        localPlayer.Heal(0);

    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartWave()
    {
        waveInProgress = true;
        currentWave++;
        OnCurrentWaveChanged?.Invoke();

        foreach (Player player in players)
        {
            player?.GetComponent<Weapon>().ResetTimer();
        }
        StartCoroutine(WaveRoutine(waveDuration));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
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

        while (RemainingWaveTime > 0 && waveInProgress && isShopping == false)
        {
            RemainingWaveTime -= 1;

            if (isSpawning)
            {
                EnemySpawner.Instance.SpawnEnemy();
            }
            yield return new WaitForSeconds(enemySpawnTime);
        }

        RpcEndWave();
        if (gameRunning && GetLivingPlayers().Count() != 0)
        {
            EnemySpawner.Instance.currentSpeed = 3f;
            RpcEnterShoppingPhase();
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEnterShoppingPhase()
    {
        isShopping = true;
        UIController.Instance.ShowUIElement(UIElement.Shop);
    }

    private void UpdateEnemyPool()
    {
        if (currentWave >= 0)
        {
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.LaserDrone, 1);
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Jet, 1);
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Drone, 5);
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Airship, 1);
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
        gameRunning = true;
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
    public void RPC_PlayerLeft(Player player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
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
        else if (localPlayer == player && !isShopping && gameRunning)
        {
            StartSpectator();
        }
    }

    [Rpc]
    public void RpcGameOver()
    {
        RpcEndWave();
        foreach (Player player in players)
        {
            player.GetComponent<Weapon>().ResetSpecialAttacks();
        }
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


    [Rpc]
    private void RpcShowGame()
    {

        if (NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>().isAlive)
        {
            UIController.Instance.ShowUIElement(UIElement.Game);
        }
        else
        {
            StartSpectator();
        }
    }

    public List<Player> GetLivingPlayers()
    {
        List<Player> livingPlayers = new List<Player>();
        foreach (Player p in players)
        {
            if (p.isAlive)

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

    public List<Enemy> FindClosestEnemies(Vector3 position, int count, float maxRange, int ignoreEnemyWithId = -1)
    {
        return FindObjectsOfType<Enemy>()
            .Where(t => t.gameObject.GetInstanceID() != ignoreEnemyWithId)
            .Where(t => (enemyMask.value & (1 << t.gameObject.layer)) != 0)
            .Where(t => Vector3.Distance(t.getPosition(), position) <= maxRange)

            .OrderBy(t => Vector3.Distance(t.getPosition(), position))
            .Take(count)
            .ToList();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_TriggerPlayerListChanged()
    {
        OnPlayerListChanged?.Invoke();
    }
}
