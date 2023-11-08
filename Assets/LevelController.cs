using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Collections;

public class LevelController : NetworkBehaviour
{
    private List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();
    public GameObject dronePrefab;
    public static LevelController Instance;

    private int currentWave = 0;
    private float waveDuration = 30f;
    private float waveDurationIncrease = 2f;

    private float waveEndTime; // Time when the current wave will end
    private bool waveInProgress = false;

    public float RemainingWaveTime { get; private set; }

    private List<EnemyType> currentEnemyPool;

    public event Action onStartGame;
    public GameObject background;
    public Player localPlayer;
    private List<Player> livingPlayers;

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
        if (!Runner.IsServer) return;

        currentEnemyPool = new List<EnemyType> { EnemyType.Drone };

    }

    private void SpawnRandomEnemy()
    {
        Vector3 position = GetRandomPosition(); // Implement this method based on your game's logic
        EnemyType enemyType = currentEnemyPool[UnityEngine.Random.Range(0, currentEnemyPool.Count)];
        SpawnEnemy(position, enemyType);
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
            /// ...
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

        while (RemainingWaveTime > 0)
        {
            // Assuming the game is not paused and you're counting down
            RemainingWaveTime -= 1;

            SpawnRandomEnemy();
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
        // Increase Difficulty
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

    public void PlayerDowned()
    {

    }

    private void CheckForLevelCompletion()
    {
        if (_spawnedEnemies.Count == 0)
        {
            Debug.Log("Level Completed!");
            // Handle level completion logic here
        }
    }

    private Vector3 GetRandomPosition()
    {
        // Assuming 'background' is a reference to the background GameObject with a SpriteRenderer
        SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();

        Vector2 size = spriteRenderer.size;

        // Calculate a random position within the sprite bounds
        float x = UnityEngine.Random.Range(-size.x / 2, size.x / 2);
        float y = UnityEngine.Random.Range(-size.y / 2, size.y / 2);

        // Adjust for the position of the background GameObject if it's not centered at the world origin
        Vector3 offsetPosition = new Vector3(x, y, 0) + spriteRenderer.gameObject.transform.position;

        return offsetPosition;

    }
}
