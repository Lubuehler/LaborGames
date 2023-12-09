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
    [SerializeField] private GameObject empPrefab;

    private NetworkRigidbody2D _nrb2d;
    private SpriteRenderer _spriteRenderer;
    private CapsuleCollider2D _capsuleCollider;
    private float currentTime;
    public bool isShooting = false;

    [Networked] public bool isAlive { get; set; }
    [Networked(OnChanged = nameof(PlayerInfoChanged))]
    public string playerName { get; set; }

    [Networked(OnChanged = nameof(PlayerInfoChanged))]
    public bool ready { get; set; }
    public int lobbyNo;
    public static Player localPlayer;


    // Player Stats
    [Networked] public int coins { get; set; }
    [Networked] public float maxHealth { get; set; }
    [Networked] public float attackDamage { get; set; }
    [Networked] public float attackSpeed { get; set; } // Schüsse pro Sekunde
    [Networked] public float critChance { get; set; }
    [Networked] public float critDamageMultiplier { get; set; }
    [Networked] public float dodgeChance { get; set; }
    [Networked] public float movementSpeed { get; set; }
    [Networked] public float luck { get; set; }
    [Networked] public float armor { get; set; }
    [Networked] public float range { get; set; }
    [Networked] public float currentHealth { get; set; }

    //Items & SpecialAttacks
    [Networked] public NetworkLinkedList<Guid> items { get; }

    [Networked] public NetworkLinkedList<Guid> specialAttacks { get; }
    [Networked] public Guid selectedSpecialAttack { get; set; }

    // Actions
    public event Action OnStatsChanged;
    public event Action<float, float> OnHealthChanged;
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    public override void Spawned()
    {
        this.isAlive = true;
        if (HasInputAuthority)
        {
            Camera.main.GetComponent<CameraScript>().target = this.gameObject.GetComponent<NetworkObject>();
            RPC_Configure(DataController.Instance.playerData.playerName);
            LevelController.Instance.localPlayer = this;
        }
        LevelController.Instance.RpcPlayerSpawned(this);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_Configure(string playerName)
    {
        this.playerName = playerName;
        selectedSpecialAttack = Guid.Empty;

        int margin = 2;
        _nrb2d.TeleportToPosition(new Vector2((GetComponentInChildren<SpriteRenderer>().size.x + margin) * lobbyNo, 0));
        InitiallySetStats();
    }

    public void InitiallySetStats()
    {
        coins = 200;
        maxHealth = 100;
        attackDamage = 20;
        attackSpeed = 1;
        critChance = 0;
        critDamageMultiplier = 1;
        dodgeChance = 0;
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
        if (GetInput(out NetworkInputData data) && LevelController.Instance.waveInProgress && isAlive)
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
        if (isShooting && isAlive)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 1f / attackSpeed)
            {
                Fire(findNearestEnemy());
                currentTime = 0f;
            }
        }
        _spriteRenderer.enabled = isAlive;
        _capsuleCollider.enabled = isAlive;

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

        if (currentHealth <= 0 && isAlive)
        {
            RpcDie();
            if (HasInputAuthority)
            {
                StartCoroutine(DelayedDeath());
            }
        }
    }

    IEnumerator DelayedDeath()
    {

        yield return new WaitForSeconds(2f);

        LevelController.Instance.RpcPlayerDowned(this);
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
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcRessurect()
    {
        isAlive = true;
        Heal(maxHealth);

        if (HasInputAuthority)
        {
            Camera.main.GetComponent<CameraScript>().target = GetComponent<NetworkObject>();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcReset()
    {
        RPC_Configure(playerName);
        RpcRessurect();
        ready = false;

        //Special attacks
        selectedSpecialAttack = Guid.Empty;
        specialAttacks.Clear();
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ApplyItem(Guid id)
    {
        Item item = ShopSystem.Instance.items.FirstOrDefault(item => item.id == id);
        print(item);
        if (item.itemType == ItemType.SpecialAttack)
        {
            specialAttacks.Add(item.id);
        }
        else if (item.itemType == ItemType.Item)
        {
            foreach (StatModifier modifier in item.modifiers)
            {
                switch (modifier.statName)
                {
                    case "Max Health":
                        this.maxHealth += modifier.value; break;
                    case "Attack Damage":
                        this.attackDamage += modifier.value; break;
                    case "Attack Speed":
                        this.attackSpeed += modifier.value; break;
                    case "Crit Chance":
                        this.critChance += modifier.value; break;
                    case "Crit Damage":
                        this.critDamageMultiplier += modifier.value; break;
                    case "Movement Speed":
                        this.movementSpeed += modifier.value; break;
                    case "Luck":
                        this.luck += modifier.value; break;
                    case "Armor":
                        this.armor += modifier.value; break;
                    case "Range":
                        this.range += modifier.value; break;
                    default:
                        Debug.LogError("Unknown Stat Modifier Name: Check the Item Description for Spelling"); break;

                }
            }
            OnStatsChanged?.Invoke();
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SetSelectedSpecialAttack(Guid id)
    {
        this.selectedSpecialAttack = id;
        Debug.Log(this.selectedSpecialAttack);
    }

    public void DeployEMP()
    {
        Runner.Spawn(empPrefab, transform.position, transform.rotation);
    }
}