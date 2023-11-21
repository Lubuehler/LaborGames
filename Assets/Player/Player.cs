using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;



public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask coinMask;
    [SerializeField] public Projectile _projectilePrefab;
    [SerializeField] private float tiltAmount = 15.0f;
    [SerializeField] private GameObject background;

    private NetworkRigidbody2D _nrb2d;
    private float currentTime;
    public bool isShooting = false;

    [Networked] public bool isAlive { get; set; }
    [Networked(OnChanged = nameof(PlayerInfoChanged))]
    public string playerName { get; set; }

    [Networked(OnChanged = nameof(PlayerInfoChanged))]
    public bool ready { get; set; }


    // Player Stats
    [Networked] public int coins { get; set; }
    [Networked] public float maxHealth { get; set; }
    [Networked] public float attackDamage { get; set; }
    [Networked] public float attackSpeed { get; set; } // Schüsse pro Sekunde
    [Networked] public float critChance { get; set; }
    [Networked] public float critDamageMultiplier { get; set; }
    [Networked] public float dodgeProbability { get; set; }
    [Networked] public float movementSpeed { get; set; }
    [Networked] public float luck { get; set; }
    [Networked] public float armor { get; set; }
    [Networked] public float range { get; set; }
    [Networked] public float currentHealth { get; set; }

    // Actions
    public event Action OnStatsChanged;
    public event Action<float, float> OnHealthChanged;
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Camera.main.GetComponent<CameraScript>().target = this.gameObject.GetComponent<NetworkObject>();
            RPC_Configure(DataController.Instance.playerData.playerName);
            LevelController.Instance.localPlayer = this;
            LevelController.Instance.RpcPlayerSpawned(this);
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_Configure(string playerName)
    {
        this.playerName = playerName;
        this.isAlive = true;
        InitiallySetStats();
    }

    public void InitiallySetStats()
    {
        coins = 0;
        maxHealth = 100;
        attackDamage = 20;
        attackSpeed = 1;
        critChance = 0;
        critDamageMultiplier = 1;
        dodgeProbability = 0;
        movementSpeed = 5;
        luck = 0;
        armor = 0;
        range = 0;

        currentHealth = maxHealth;
        OnStatsChanged?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && LevelController.Instance.waveInProgress)
        {
            data.direction.Normalize();
            _nrb2d.Rigidbody.velocity = data.direction * movementSpeed;

            // Image Tilting
            float tilt = data.direction.x * -tiltAmount;
            transform.rotation = Quaternion.Euler(0, 0, tilt);

            //float clampedX = Mathf.Clamp(transform.position.x, minX + width / 2, maxX - width / 2);
            // float clampedY = Mathf.Clamp(transform.position.y, minY + height / 2, maxY - height / 2);
            // transform.position = new Vector2(clampedX, clampedY);
        }
        else
        {
            _nrb2d.Rigidbody.velocity = new Vector2(0, 0);
        }
    }

    private void Fire(NetworkObject target)
    {
        if (target != null)
        {
            Vector3 direction = target.transform.position - _nrb2d.transform.position;

            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            var projectile = Runner.Spawn(_projectilePrefab, _nrb2d.transform.position, rotation, Object.InputAuthority);

            if (projectile != null)
            {
                projectile.Fire(direction.normalized);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (((1 << collider.gameObject.layer) & coinMask) != 0)
        {
            Runner.Despawn(collider.gameObject.GetComponent<NetworkObject>());
            coins++;
            OnCoinsChanged?.Invoke(coins);
        }

    }

    public static void PlayerInfoChanged(Changed<Player> changed)
    {
        NetworkController.Instance.TriggerPlayerListChanged();
    }

    public override void Render()
    {
        if (isShooting)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 1f / attackSpeed)
            {
                Fire(findNearestEnemy());
                currentTime = 0f;
            }
        }

    }

    private NetworkObject findNearestEnemy()
    {
        // Calculate the bounds of the camera's view
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        Vector2 boxSize = new Vector2(width, height);
        Vector2 boxCenter = transform.position;

        // Get all colliders within the camera's view
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, Camera.main.transform.eulerAngles.z, enemyMask);

        float minDistance = float.MaxValue;
        NetworkObject closestEnemy = null;
        foreach (var enemy in hitColliders)
        {
            if (enemy != null)
            {
                Vector3 direction = enemy.transform.position - _nrb2d.transform.position;
                float distance = direction.magnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy.GetComponent<NetworkObject>();
                }
            }
        }
        return closestEnemy;
    }

    public void TakeDamage(float damage)
    {
        float prev = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            RpcDie();
            

        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        Heal(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcDie()
    {
        isAlive = false;
        Runner.Spawn(deathExplosionPrefab, transform.position, transform.rotation);
        gameObject.SetActive(false);
        LevelController.Instance.RpcPlayerDowned(this);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcRessurect()
    {
        isAlive = true;
        gameObject.SetActive(true);
        Heal(maxHealth);
    }

    public void printStats()
    {
        print("maxHealth: " + maxHealth);
        print("attackDamage: " + attackDamage);
        print("attackSpeed: " + attackSpeed);
        print("critChance: " + critChance);
        print("critDamageMultiplier: " + critDamageMultiplier);
        print("dodgeProbability: " + dodgeProbability);
        print("movementSpeed: " + movementSpeed);
        print("luck: " + luck);
        print("armor: " + armor);
        print("range: " + range);
        print("Current Health : " + currentHealth);
    }
}