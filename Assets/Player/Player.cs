using Fusion;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private LayerMask coinMask;
    [SerializeField] private float tiltAmount = 15.0f;

    BoxCollider2D backgroundCollider;

    private Weapon weapon;

    private PassiveItemEffectManager passiveItemEffectManager;

    private NetworkRigidbody2D _nrb2d;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    [SerializeField] private float movementSmoothing = .5f;

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
    [Networked] public int maxHealth { get; set; }
    [Networked] public float attackDamage { get; set; }
    [Networked] public float attackSpeed { get; set; } // Schüsse pro Sekunde
    [Networked] public float critChance { get; set; }
    [Networked] public float critDamageMultiplier { get; set; }
    [Networked] public float lifesteal { get; set; }
    [Networked] public float dodgeChance { get; set; }
    [Networked] public float movementSpeed { get; set; }
    [Networked] public float luck { get; set; }
    [Networked] public float armor { get; set; }
    [Networked] public float range { get; set; }
    [Networked] public int currentHealth { get; set; }

    //Items
    [Networked, Capacity(40)] public NetworkLinkedList<int> items { get; }

    // Actions
    public event Action OnStatsChanged;
    public event Action<float, float> OnHealthChanged;
    public event Action OnCoinsChanged;

    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        weapon = GetComponent<Weapon>();
        passiveItemEffectManager = GetComponent<PassiveItemEffectManager>();
        passiveItemEffectManager.Initialize(weapon);
        backgroundCollider = GameController.Instance.background.GetComponent<BoxCollider2D>();

    }

    public Vector2 getPosition()
    {
        return _nrb2d.transform.position;
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
        coins = 0;
        maxHealth = 100;
        attackDamage = 20;
        attackSpeed = 1;
        critChance = 0;
        critDamageMultiplier = 1;
        lifesteal = 0;
        dodgeChance = 0;
        movementSpeed = 5;
        luck = 0;
        armor = 0;
        range = 0;

        currentHealth = maxHealth;
        OnStatsChanged?.Invoke();
        RpcHealthChanged(currentHealth);
    }

    private Vector2 previousDirection;


    public void Move()
    {
        previousDirection.Normalize();
        Vector2 intendedVelocity;

        if (!isAlive)
        {
            intendedVelocity = Vector2.zero;
        }
        else if (previousDirection == Vector2.zero)
        {
            intendedVelocity = Vector2.Lerp(_nrb2d.Rigidbody.velocity, Vector2.zero, movementSmoothing * Runner.DeltaTime);
        }
        else
        {
            intendedVelocity = previousDirection * movementSpeed;
        }

        _nrb2d.Rigidbody.velocity = intendedVelocity;

        float clampedX = Mathf.Clamp(transform.position.x, (-1) * backgroundCollider.size.x / 2, backgroundCollider.size.x / 2);
        float clampedY = Mathf.Clamp(transform.position.y, (-1) * backgroundCollider.size.y / 2, backgroundCollider.size.y / 2);
        transform.position = new Vector2(clampedX, clampedY);

        // Image Tilting
        float tilt = previousDirection.x * -tiltAmount;
        transform.rotation = Quaternion.Euler(0, 0, tilt);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && LevelController.Instance.waveInProgress && isAlive)
        {
            previousDirection = data.direction;
        }
        else
        {
            previousDirection = Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (((1 << collider.gameObject.layer) & coinMask) != 0)
        {
            EnemySpawner.Instance.CoinCollected(collider.gameObject.GetComponent<Coin>());
            coins++;
            OnCoinsChanged?.Invoke();
        }

    }

    public static void PlayerInfoChanged(Changed<Player> changed)
    {
        NetworkController.Instance.TriggerPlayerListChanged();
    }


    private float flameDamageCooldown = 0.2f;
    private bool allowFlameDamage = true;
    public override void Render()
    {
        _spriteRenderer.enabled = isAlive;
        _collider.enabled = isAlive;
        Move();


        if (!allowFlameDamage)
        {
            flameDamageCooldown -= Time.deltaTime;
            if (flameDamageCooldown <= 0)
            {
                allowFlameDamage = true;
                flameDamageCooldown = 0.2f;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        print("trake dmg");
        if (!HasInputAuthority)
        {
            print("no input authority, therefore no ddmg.");
            return;
        }

        Debug.Log(weapon.shieldActive);
        if (weapon.shieldActive)
        {
            return;
        }

        double randomNumber = new System.Random().NextDouble();
        if (randomNumber < dodgeChance)
        {
            return;
        }

        RpcHealthChanged(Mathf.Clamp(currentHealth -= (int)damage, 0, maxHealth));

        if (currentHealth <= 0 && isAlive)
        {
            RemoveRandomItems();
            RpcDie();

            StartCoroutine(DelayedDeath());

        }
    }

    IEnumerator DelayedDeath()
    {

        yield return new WaitForSeconds(2f);

        LevelController.Instance.RpcPlayerDowned(this);
    }

    public void Heal(int amount)
    {
        if (amount == int.MaxValue)
        {
            RpcHealthChanged(maxHealth);
        }
        else
        {
            RpcHealthChanged(Mathf.Clamp(currentHealth + amount, 0, maxHealth));
        }

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_IncreaseMaxHealth(float amount)
    {
        maxHealth += (int)amount;
        Heal((int)amount);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcDie()
    {
        Debug.Log("Die");
        RemoveRandomItems();
        isAlive = false;
        Runner.Spawn(deathExplosionPrefab, transform.position, transform.rotation);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcRessurect()
    {
        print("ressurected " + playerName);

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
        items.Clear();
        passiveItemEffectManager.Clear();
        print("reset " + playerName);
        RPC_Configure(playerName);
        RpcRessurect();
        lobbyReady = false;
        shopReady = false;

        //Special attacks
        //selectedSpecialAttack = int.MinValue;
        //specialAttacks.Clear();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_ApplyItem(int id)
    {

        Item item = ShopSystem.Instance.allItems.FirstOrDefault(item => item.itemID == id);
        foreach (Item test in ShopSystem.Instance.allItems)
        {
            print(test.itemID);
        }
        print("item id: " + item.itemID);
        if (item != null)
        {
            foreach (StatModifier modifier in item.modifiers)
            {
                switch (modifier.statName)
                {
                    case "Max Health":
                        maxHealth += (int)modifier.value; break;
                    case "Attack Damage":
                        attackDamage += modifier.value; break;
                    case "Attack Speed":
                        attackSpeed += modifier.value; break;
                    case "Critical Strike Chance":
                        critChance += modifier.value; break;
                    case "Critical Strike Damage Factor":
                        critDamageMultiplier += modifier.value; break;
                    case "Life Steal":
                        lifesteal += modifier.value; break;
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AddItemEffect(int itemID)
    {
        passiveItemEffectManager.AddOrEnhanceEffect(itemID);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcHealthChanged(int newHealth)
    {
        currentHealth = newHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void RemoveRandomItems()
    {
        Debug.Log("RemoveRandomItems");
        int itemRemoveCount = items.Count / 3;
        while (itemRemoveCount > 0)
        {
            items.Remove(UnityEngine.Random.Range(0, items.Count));
            itemRemoveCount--;
        }

        int specialAttackRemoveCount = weapon.specialAttacks.Count / 3;
        while (specialAttackRemoveCount > 0)
        {
            weapon.specialAttacks.Remove(UnityEngine.Random.Range(0, weapon.specialAttacks.Count));
            specialAttackRemoveCount--;
        }
    }

    public void OnParticleCollision(GameObject other)
    {
        if (!allowFlameDamage) return;
        TakeDamage(5);
        allowFlameDamage = false;
    }

    public void TriggerGoldChanged()
    {
        OnCoinsChanged?.Invoke();
    }
}