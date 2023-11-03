using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class LevelController : NetworkBehaviour
{
    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    public GameObject dronePrefab;
    public static LevelController Instance;

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
        if (Runner.IsServer)
        {
            StartLevel();
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
