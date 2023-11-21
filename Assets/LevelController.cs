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

    public Player localPlayer;
    public List<Player> livingPlayers = new List<Player>();
    public List<Player> deadPlayers = new List<Player>();


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

        EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Drone, 1);

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

        if (livingPlayers.Count > 0)
        {
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
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Jet, 2);
            EnemySpawner.Instance.UpdateEnemySpawnRate(EnemyType.Drone, 3);
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
        this.gameStarted = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcPlayerSpawned(Player player)
    {
        livingPlayers.Add(player);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcPlayerDowned(Player player)
    {
        print(livingPlayers);
        if (livingPlayers.Contains(player))
        {
            livingPlayers.Remove(player);
            deadPlayers.Add(player);
            if (livingPlayers.Count == 0)
            {
                RpcEndWave();
                UIController.Instance.ShowUIElement(UIElement.Endscreen);
            }
            else if (localPlayer == player)
            {
                Player selectedPlayer = livingPlayers.FirstOrDefault(player => player.GetComponent<Player>().isAlive);

                UIController.Instance.ShowUIElement(UIElement.Spectator);
                Camera.main.GetComponent<CameraScript>().target = selectedPlayer.GetComponent<NetworkObject>();
            }
        } else
        {
            print("Cant remove Player from 'Living Players list' as does not exist");
        }
        
    }

    public void RessurectPlayers()
    {
        foreach (Player player in deadPlayers)
        {
            player.RpcRessurect();
        }
    }


    private static void ShowGame(Changed<LevelController> changed)
    {
        if (changed.Behaviour.waveInProgress)
        {
            UIController.Instance.ShowUIElement(UIElement.Game);
        }
    }
}
