using Fusion;
using System;
using System.Linq;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private Projectile _projectilePrefab;

    // Special attacks
    [SerializeField] private GameObject emp;
    [SerializeField] private GameObject teleport;
    [SerializeField] private GameObject magnet;
    [SerializeField] private GameObject dash;
    [SerializeField] private GameObject shield;
    [SerializeField] private GameObject slowness;


    private NetworkRigidbody2D _nrb2d;
    private Player player;

    private float currentTime { get; set; }

    [Networked] public int shotCounter { get; set; } = 0;


    // ###############################################
    public bool shieldActive = false;
    // ###############################################


    [Networked, Capacity(20)] public NetworkLinkedList<int> specialAttacks { get; }
    [Networked(OnChanged = nameof(OnSpecialAttackChanged))] public int selectedSpecialAttack { get; set; }


    public bool specialAttackAvailable;
    public float specialAttackTimer;
    public float cooldown = 2f;


    public event Action<Transform, int> OnAttack;
    public event Action<Vector2, int, int> OnHitTarget;
    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();
        specialAttackAvailable = false;
        specialAttackTimer = cooldown;
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
            var pressed = data.buttons.GetPressed(ButtonsPrevious);
            var released = data.buttons.GetReleased(ButtonsPrevious);
            ButtonsPrevious = data.buttons;

            if (specialAttackAvailable && selectedSpecialAttack != int.MinValue && LevelController.Instance.waveInProgress)
            {
                if (pressed.IsSet(MyButtons.SpecialAttack) && !released.IsSet(MyButtons.SpecialAttack))
                {
                    DeploySpecialAttack();
                    ResetTimer();
                }
            }
        }


    }

    public static void OnSpecialAttackChanged(Changed<Weapon> changed)
    {
        changed.Behaviour.OnSpecialAttackChanged(changed.Behaviour.selectedSpecialAttack);
    }

    private void OnSpecialAttackChanged(int itemID)
    {
        emp.SetActive(false);
        teleport.SetActive(false);
        shield.SetActive(false);
        slowness.SetActive(false);
        magnet.SetActive(false);
        dash.SetActive(false);

        switch (itemID)
        {
            case 3:
                emp.SetActive(true);
                break;
            case 4:
                teleport.SetActive(true);
                break;
            case 5:
                shield.SetActive(true);
                break;
            case 6:
                slowness.SetActive(true);
                break;
            case 7:
                magnet.SetActive(true);
                break;
            case 8:
                dash.SetActive(true);
                break;
            default:
                break;
        }
    }

    private void DeploySpecialAttack()
    {
        switch (selectedSpecialAttack)
        {
            case 3:
                emp.GetComponent<EMP>().Activate(_nrb2d);
                break;
            case 4:
                teleport.GetComponent<Teleport>().Activate();
                break;
            case 5:
                shield.GetComponent<Shield>().Activate();
                break;
            case 6:
                slowness.GetComponent<Slowness>().Activate();
                break;
            case 7:
                magnet.GetComponent<Magnet>().Activate(gameObject.transform);
                break;
            case 8:
                dash.GetComponent<Dash>().Activate(_nrb2d);
                break;
            default:
                break;
        }
    }



    public override void Render()
    {
        if (!LevelController.Instance.waveInProgress)
        {
            return;
        }

        // Shooting
        if (player.isAlive)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 1f / player.attackSpeed)
            {
                Shoot(LevelController.Instance.FindClosestEnemies(player.getPosition(), 1, 10f).FirstOrDefault(), shotCounter++);
                currentTime = 0f;
            }
        }

        // Special attack cooldown
        if (!specialAttackAvailable)
        {
            specialAttackTimer -= Time.deltaTime;
            if (specialAttackTimer <= 0)
            {
                specialAttackAvailable = true;
            }

        }
    }

    private void Shoot(Enemy target, int shotID)
    {
        if (target != null)
        {
            ReleaseBullet(target, shotID);
            OnAttack?.Invoke(target.getTransform(), shotID);
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
        print(shotID);
        print(enemy);
        print(enemy.gameObject);
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
        specialAttackTimer = cooldown;
    }
}
