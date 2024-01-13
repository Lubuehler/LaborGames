using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private GameObject empPrefab;
    [SerializeField] private GameObject teleportPrefab;
    [SerializeField] private GameObject shield;
    [SerializeField] private Animator shieldAnimator;


    private NetworkRigidbody2D _nrb2d;
    private Player player;

    private float currentTime { get; set; }

    [Networked] public int shotCounter { get; set; } = 0;


    // Special attack
    [Networked, Capacity(20)] public NetworkLinkedList<int> specialAttacks { get; }
    [Networked] public int selectedSpecialAttack { get; set; }
    public bool specialAttackAvailable;
    public float specialAttackTimer;
    public float animationDuration = 10f;


    // Teleport
    private static int maxLength = 8; // 4 times per second -> 2 seconds
    private Queue<Vector3> positions = new Queue<Vector3>(maxLength + 1);
    private Queue<Quaternion> rotations = new Queue<Quaternion>(maxLength + 1);
    private float positionTimer = 0f;
    private float interval = 0.25f;

    // Shield
    private float shieldTime = 5.0f;
    public bool shieldActive = false;
    private bool animationTriggered = false;

    // Slowness 

    private float slownessTime = 5.0f;
    public bool slownessActive = false;


    public event Action<Transform, int> OnAttack;
    public event Action<Vector2, int, int> OnHitTarget;
    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();
        specialAttackAvailable = false;
        specialAttackTimer = animationDuration;
    }

    public override void Spawned()
    {
        player = GetBehaviour<Player>();
        selectedSpecialAttack = int.MinValue;
    }

    [Networked] public NetworkButtons ButtonsPrevious { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<NetworkInputData>(out var data) == false) return;

        if (LevelController.Instance.waveInProgress && player.isAlive)
        {
            // compute pressed/released state
            var pressed = data.buttons.GetPressed(ButtonsPrevious);
            var released = data.buttons.GetReleased(ButtonsPrevious);

            // store latest input as 'previous' state we had
            ButtonsPrevious = data.buttons;

            if (specialAttackAvailable && selectedSpecialAttack != int.MinValue && LevelController.Instance.waveInProgress)
            {
                if (pressed.IsSet(MyButtons.SpecialAttack) && !released.IsSet(MyButtons.SpecialAttack))
                {
                    switch (selectedSpecialAttack)
                    {
                        case 3:
                            DeployEMP();
                            break;
                        case 4:
                            Teleport();
                            break;
                        case 5:
                            DeployShield();
                            break;
                        case 6:
                            DeploySlowness();
                            break;
                        default:
                            break;
                    }
                    ResetTimer();
                }
            }
        }


    }

    private void DeployEMP()
    {
        Runner.Spawn(empPrefab, _nrb2d.transform.position, transform.rotation);
    }

    private void DeployShield()
    {
        shield.SetActive(true);
        shieldActive = true;
    }

    private void DeploySlowness()
    {
        EnemySpawner.Instance.speed = 1.5f;
        slownessActive = true;
    }

    void StorePosition()
    {
        positions.Enqueue(_nrb2d.transform.position);
        rotations.Enqueue(_nrb2d.transform.rotation);
        if (positions.Count > maxLength)
        {
            positions.Dequeue();
            rotations.Dequeue();
        }
    }

    void Teleport()
    {
        Runner.Spawn(teleportPrefab, _nrb2d.transform.position);
        _nrb2d.TeleportToPosition(positions.Peek());
        _nrb2d.TeleportToRotation(rotations.Peek());
        Runner.Spawn(teleportPrefab, _nrb2d.transform.position);
    }

    public override void Render()
    {
        if (!LevelController.Instance.waveInProgress)
        {
            return;
        }

        if (player.isAlive)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= 1f / player.attackSpeed)
            {
                Shoot(LevelController.Instance.FindClosestEnemies(transform.position, 1, 10f).FirstOrDefault(), shotCounter++);
                currentTime = 0f;
            }
        }

        if (!specialAttackAvailable)
        {
            specialAttackTimer -= Time.deltaTime;
            if (specialAttackTimer <= 0)
            {
                specialAttackAvailable = true;
            }
        }

        if (selectedSpecialAttack == 4)
        {
            positionTimer += Time.deltaTime;

            if (positionTimer >= interval)
            {
                StorePosition();
                positionTimer = 0f;
            }
        }

        if (selectedSpecialAttack == 5 && shieldActive)
        {
            shieldTime -= Time.deltaTime;

            if (shieldTime <= 2.0f && !animationTriggered)
            {
                shieldAnimator.SetTrigger("ShieldEnd");
                animationTriggered = true;
            }

            if (shieldTime <= 0.0f)
            {
                shieldActive = false;
                shield.SetActive(false);
                shieldTime = 5.0f;
                animationTriggered = false;
            }
        }

        if (selectedSpecialAttack == 6 && slownessActive)
        {
            slownessTime -= Time.deltaTime;

            if (slownessTime <= 0.0f)
            {
                slownessActive = false;
                slownessTime = 5.0f;
                EnemySpawner.Instance.speed = 3;
            }
        }
    }

    private void Shoot(Enemy target, int shotID)
    {
        if (target != null)
        {
            OnAttack?.Invoke(target.getTransform(), shotID);
            ReleaseBullet(target, shotID);
        }
    }

    public void ReleaseBullet(Enemy target, int shotID, Vector2? origin = null)
    {
        if (target == null)
        { return; }
        if (origin == null)
        {
            origin = player.getPosition();
        }

        Projectile projectile;
        Vector2 direction;
        direction = target.getPosition() - (Vector2)origin;

        projectile = Instantiate(_projectilePrefab, (Vector2)origin, Quaternion.LookRotation(Vector3.forward, direction));

        projectile.Fire(direction.normalized, this, shotID);

    }

    public void OnBulletHit(Enemy enemy, int shotID)
    {
        if (enemy == null || enemy.gameObject == null) { return; }
        OnHitTarget?.Invoke(enemy.getPosition(), enemy.gameObject.GetInstanceID(), shotID);

        if (HasStateAuthority)
        {
            enemy.TakeDamage(CalculateDamage());
        }
    }

    private int CalculateDamage()
    {
        double randomNumber = new System.Random().NextDouble();
        if (randomNumber < player.critChance)
        {
            return (int)player.attackDamage;
        }
        else
        {
            return (int)(player.attackDamage * player.critDamageMultiplier);
        }
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetSelectedSpecialAttack(int itemID)
    {
        selectedSpecialAttack = itemID;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AddSpecialAttack(int itemID)
    {
        selectedSpecialAttack = itemID;
        specialAttacks.Add(itemID);
    }

    private void ResetTimer()
    {
        specialAttackAvailable = false;
        specialAttackTimer = animationDuration;
    }
}
