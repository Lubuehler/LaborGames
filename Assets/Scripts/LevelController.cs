using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class LevelController : NetworkBehaviour
{
    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    public GameObject dronePrefab;

    private NetworkRunner _networkRunner;

    private void SpawnEnemy(Vector3 position, EnemyType enemyType)
    {
        if (enemyType == EnemyType.Drone)
        {
            NetworkObject spawnedEnemy = _networkRunner.Spawn(dronePrefab, position, Quaternion.identity);
            _spawnedEnemies.Add(spawnedEnemy);
        }
    }

    public void StartLevel()
    {
        _networkRunner = FindObjectOfType<NetworkRunner>();
        print("Started Level");

        for (int i = 0; i < 5; i++)
        {
            Vector2 spawnPosition = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
            SpawnEnemy(spawnPosition, EnemyType.Drone);
        }
    }

    public void EnemyDefeated(Enemy enemy)
    {
        if (_networkRunner.IsServer)
        {
            _spawnedEnemies.Remove(enemy.GetComponentInParent<NetworkObject>());
            _networkRunner.Despawn(enemy.GetComponentInParent<NetworkObject>());
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
