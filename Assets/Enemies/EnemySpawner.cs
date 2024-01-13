using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    private Dictionary<EnemyType, float> enemySpawnRates = new Dictionary<EnemyType, float>();

    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    private List<NetworkObject> _spawnedCoins = new List<NetworkObject>();
    public List<GameObject> spawnedObjects = new List<GameObject>();
    public static EnemySpawner Instance;

    [SerializeField] private GameObject coinPrefab;

    // Enemies
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private GameObject jetPrefab;
    [SerializeField] private GameObject airshipPrefab;


    [SerializeField] public float speed = 3f;


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
            Destroy(gameObject);
        }
    }

    public void SpawnEnemy()
    {

        EnemyType enemyType = SelectEnemyTypeBasedOnSpawnRate();
        Vector2 position;
        NetworkObject spawnedEnemy;
        switch (enemyType)
        {
            case EnemyType.Airship:
                position = GetRandomPosition(SpawnLocation.Sides);
                spawnedEnemy = Runner.Spawn(airshipPrefab, position, Quaternion.identity);
                break;

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
        float randomPoint = Random.value * totalRate;

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
        BoxCollider2D boxCollider2D = GameController.Instance.background.GetComponent<BoxCollider2D>();
        float width = boxCollider2D.size.x;
        float height = boxCollider2D.size.y;
        float padding = 2f;

        float x = 0;
        float y = 0;

        switch (location)
        {
            case SpawnLocation.Top:
                x = Random.Range(-width / 2, width / 2);
                y = height / 2 + padding;
                break;
            case SpawnLocation.Bottom:
                x = Random.Range(-width / 2, width / 2);
                y = -height / 2 - padding;
                break;
            case SpawnLocation.Sides:
                if (Random.value < 0.5) x = -width / 2 - padding; else x = width / 2 + padding;
                y = Random.Range(-height / 2, height / 2); break;
        }
        return new Vector2(x, y);

    }

    public void RegisterObject(GameObject gameObject)
    {
        spawnedObjects.Add(gameObject);
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
        foreach (GameObject gameObject in spawnedObjects)
        {
            Destroy(gameObject);
        }
        _spawnedEnemies.Clear();
        _spawnedCoins.Clear();
        spawnedObjects.Clear();
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

    public void CoinCollected(Coin coin)
    {
        _spawnedCoins.Remove(coin.GetComponent<NetworkObject>());
        Runner.Despawn(coin.GetComponent<NetworkObject>());
    }
}
