using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private LayerMask coinMask;
    [SerializeField] private float tiltAmount = 15.0f;
    [SerializeField] private GameObject background;

    private NetworkRigidbody2D _nrb2d;
    private SpriteRenderer _spriteRenderer;
    private CapsuleCollider2D _capsuleCollider;

    [Networked] public bool isAlive { get; set; }
    [Networked(OnChanged = nameof(PlayerInfoChanged))]
    public string playerName { get; set; }

    [Networked(OnChanged = nameof(PlayerInfoChanged))]
    public bool lobbyReady { get; set; }


    public int lobbyNo;
    public static Player localPlayer;

    [Networked]
    public bool shopReady { get; set; }


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

    //Items
    [Networked, Capacity(20)] public NetworkLinkedList<int> items { get; }

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
        isAlive = true;
        if (HasInputAuthority)
        {
            Camera.main.GetComponent<CameraScript>().target = gameObject.GetComponent<NetworkObject>();
            RPC_Configure(DataController.Instance.playerData.playerName);
            LevelController.Instance.localPlayer = this;
        }
        LevelController.Instance.RpcPlayerSpawned(this);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_Configure(string playerName)
    {
        this.playerName = playerName;

        GetBehaviour<Weapon>().selectedSpecialAttack = int.MinValue;

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
        RpcHealthChanged();

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

            // float clampedX = Mathf.Clamp(transform.position.x, minX + width / 2, maxX - width / 2);
            // float clampedY = Mathf.Clamp(transform.position.y, minY + height / 2, maxY - height / 2);
            // transform.position = new Vector2(clampedX, clampedY);
        }
        else
        {
            _nrb2d.Rigidbody.velocity = new Vector2(0, 0);
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
        _spriteRenderer.enabled = isAlive;
        _capsuleCollider.enabled = isAlive;

    }

    public void TakeDamage(float damage)
    {
        damage = 0;
        float prev = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        RpcHealthChanged();

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

        RpcHealthChanged();
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        Heal(amount);
        RpcHealthChanged();

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
        lobbyReady = false;
        shopReady = false;

        //Special attacks
        //selectedSpecialAttack = int.MinValue;
        //specialAttacks.Clear();
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_ApplyItem(int id)
    {
        Item item = ShopSystem.Instance.allItems.FirstOrDefault(item => item.itemID == id);
        if (item != null)
        {
            foreach (StatModifier modifier in item.modifiers)
            {
                switch (modifier.statName)
                {
                    case "Max Health":
                        maxHealth += modifier.value; break;
                    case "Attack Damage":
                        attackDamage += modifier.value; break;
                    case "Attack Speed":
                        attackSpeed += modifier.value; break;
                    case "Crit Chance":
                        critChance += modifier.value; break;
                    case "Crit Damage":
                        critDamageMultiplier += modifier.value; break;
                    case "Movement Speed":
                        movementSpeed += modifier.value; break;
                    case "Luck":
                        luck += modifier.value; break;
                    case "Armor":
                        armor += modifier.value; break;
                    case "Range":
                        range += modifier.value; break;
                    default:
                        Debug.LogError("Unknown Stat Modifier Name: Check the Item Description for Spelling"); break;

                }
            }
            items.Add(item.itemID);
            OnStatsChanged?.Invoke();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

}