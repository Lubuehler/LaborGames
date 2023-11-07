using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class LevelController : NetworkBehaviour
{
    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    public GameObject dronePrefab;
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
        gameStarted = false;
        // if (Runner.IsServer)
        // {
        //     StartLevel();
        // }
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
            this.gameStarted = true;
        }
    }

    private void SpawnEnemy(Vector3 position, EnemyType enemyType)
    {
        if (enemyType == EnemyType.Drone)
        {
            if (Runner == null)
            {
                print("runner is null in spawn enemy");
            }
            NetworkObject spawnedEnemy = Runner.Spawn(dronePrefab, position, Quaternion.identity);
            _spawnedEnemies.Add(spawnedEnemy);
        }
        else if (enemyType == EnemyType.Jet)
        {

        }
    }

    public void StartLevel()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 spawnPosition = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
            SpawnEnemy(spawnPosition, EnemyType.Drone);
        }
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

    private void CheckForLevelCompletion()
    {
        if (_spawnedEnemies.Count == 0)
        {
            Debug.Log("Level Completed!");
            // Handle level completion logic here
        }
    }
}
