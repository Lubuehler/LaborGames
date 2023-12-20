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

    private NetworkRigidbody2D _nrb2d;
    private Player player;
    private float currentTime;


    // Special attack
    [Networked, Capacity(20)] public NetworkLinkedList<int> specialAttacks { get; }
    [Networked] public int selectedSpecialAttack { get; set; }
    public bool specialAttackAvailable;
    public float specialAttackTimer;
    public float animationDuration = 2f;

    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();

        specialAttackAvailable = false;
    }

    public override void Spawned()
    {
        player = GetBehaviour<Player>();
        selectedSpecialAttack = int.MinValue;
    }

    [Networked] public NetworkButtons ButtonsPrevious { get; set; }

    public override void FixedUpdateNetwork()
    {
        if(GetInput<NetworkInputData>(out var data) == false) return;

        if(LevelController.Instance.waveInProgress && player.isAlive)
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
                    DeployEMP();
                }
            }
        }
    }

    private void DeployEMP()
    {
        Runner.Spawn(empPrefab, _nrb2d.transform.position, transform.rotation);
        ResetTimer();
    }

    public override void Render()
    {
        if (LevelController.Instance.waveInProgress)
        {
            if (!specialAttackAvailable)
            {
                specialAttackTimer -= Time.deltaTime;
                if (specialAttackTimer <= 0)
                {
                    specialAttackAvailable = true;
                }
            }

            if (player.isAlive)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= 1f / player.attackSpeed)
                {
                    Fire(findNearestEnemy());
                    currentTime = 0f;
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
