using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject empPrefab;
    [SerializeField] private GameObject teleportPrefab;
    [SerializeField] private GameObject shield;
    [SerializeField] private Animator shieldAnimator;


    private NetworkRigidbody2D _nrb2d;
    private Player player;
    private float currentTime;


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
        if (LevelController.Instance.waveInProgress)
        {
            if (player.isAlive)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= 1f / player.attackSpeed)
                {
                    Fire(findNearestEnemy());
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
    }

    private void Fire(NetworkObject target)
    {
        if (target != null)
        {
            Vector3 direction = target.transform.position - _nrb2d.transform.position;

            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            var projectile = Runner.Spawn(_projectilePrefab, _nrb2d.transform.position, rotation, Object.InputAuthority);

            projectile?.Fire(direction.normalized);
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
