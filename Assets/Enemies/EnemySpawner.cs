using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Collections.Unicode;

public class EnemySpawner : NetworkBehaviour
{
    private Dictionary<EnemyType, float> enemySpawnRates = new Dictionary<EnemyType, float>();

    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    private List<NetworkObject> _spawnedCoins = new List<NetworkObject>();
    public static EnemySpawner Instance;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private GameObject jetPrefab;
    [SerializeField] private GameObject background;


    public enum SpawnLocation
    {
        Sides,
        Bottom,
        Top
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

    public void SpawnEnemy()
    {
        EnemyType enemyType = SelectEnemyTypeBasedOnSpawnRate();
        if (Runner == null)
        {
            print("runner is null in spawn enemy");
        }
        Vector2 position;
        NetworkObject spawnedEnemy;
        switch (enemyType)
        {       
            case EnemyType.Jet:
                position = GetRandomPosition(SpawnLocation.Sides);
                spawnedEnemy = Runner.Spawn(jetPrefab, position, Quaternion.identity);
                break;

            case EnemyType.Drone:
            default:
                position = GetRandomPosition(SpawnLocation.Sides);
                spawnedEnemy = Runner.Spawn(dronePrefab, position, Quaternion.identity);
                break;


        }
        _spawnedEnemies.Add(spawnedEnemy);

    }

    public void UpdateEnemySpawnRate(EnemyType type, float newSpawnRate)
    {
        if (enemySpawnRates.ContainsKey(type))
        {
            enemySpawnRates[type] = newSpawnRate;
        }
        else
        {
            enemySpawnRates.Add(type, newSpawnRate);
        }
    }

    private EnemyType SelectEnemyTypeBasedOnSpawnRate()
    {
        float totalRate = enemySpawnRates.Values.Sum();
        float randomPoint = UnityEngine.Random.value * totalRate;

        foreach (var pair in enemySpawnRates)
        {
            if (randomPoint < pair.Value)
                return pair.Key;
            randomPoint -= pair.Value;
        }

        return enemySpawnRates.Keys.Last(); // Fallback in case of rounding errors
    }

    private Vector3 GetRandomPosition(SpawnLocation location)
    {
        BoxCollider2D boxCollider2D = background.GetComponent<BoxCollider2D>();
        float width = boxCollider2D.size.x;
        float height = boxCollider2D.size.y;
        float padding = 2f;

        float x = 0;
        float y = 0;

        switch (location)
        {
            case SpawnLocation.Top:
                x = UnityEngine.Random.Range(-width / 2, width / 2);
                y = height / 2 + padding;
                break;
            case SpawnLocation.Bottom:
                x = UnityEngine.Random.Range(-width / 2, width / 2);
                y = -height / 2 - padding;
                break;
            case SpawnLocation.Sides:
                if (Random.value < 0.5) x = -width / 2 - padding; else x = width / 2 + padding;
                y = Random.Range(-height / 2, height / 2); break;
        }
        return new Vector2(x, y);

    }

    public void RPC_DespawnEverything()
    {
        foreach (NetworkObject coin in _spawnedCoins)
        {
            Runner.Despawn(coin);
        }
        foreach (NetworkObject enemy in _spawnedEnemies)
        {
            Runner.Despawn(enemy);
        }
        _spawnedEnemies.Clear();
        _spawnedCoins.Clear();
    }

    public void EnemyDefeated(Enemy enemy, Vector2 position)
    {
        if (Runner.IsServer)
        {
            _spawnedEnemies.Remove(enemy.GetComponentInParent<NetworkObject>());
            Runner.Despawn(enemy.GetComponentInParent<NetworkObject>());
            NetworkObject spawnedCoin = Runner.Spawn(coinPrefab, position, Quaternion.identity);
            _spawnedCoins.Add(spawnedCoin);
        }
    }
}
