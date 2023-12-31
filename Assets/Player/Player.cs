using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



public class Player : NetworkBehaviour
{
    protected NetworkRigidbody2D _nrb2d;

    public float tiltAmount = 15.0f; // The amount of tilt when moving left or right

    public Projectile _projectilePrefab;
    public LayerMask enemyMask;
    private float currentTime;
    public bool isShooting = false;

    // Player Stats:
    [Networked]
    public int maxHealth { get; set; }
    [Networked]
    public float attackDamage { get; set; }
    [Networked]
    public float attackSpeed { get; set; } // Schüsse pro Sekunde
    [Networked]
    public float critChance { get; set; }
    [Networked]
    public float critDamageMultiplier { get; set; }
    [Networked]
    public float dodgeProbability { get; set; }
    [Networked]
    public float movementSpeed { get; set; }
    [Networked]
    public float luck { get; set; }
    [Networked]
    public float armor { get; set; }
    [Networked]
    public float range { get; set; }


    public int no { get; set; }

    public event Action OnStatsChanged;


    private void Awake()
    {
        _nrb2d = GetComponent<NetworkRigidbody2D>();
    }

    public void InitiallySetStats()
    {
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

        print("set stats initially");
        OnStatsChanged?.Invoke();
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
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

    }

    private void Fire(NetworkObject target)
    {
        if (target != null)
        {
            Vector3 direction = target.transform.position - _nrb2d.transform.position;

            // Calculate the rotation to look at the closest enemy
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            var projectile = Runner.Spawn(_projectilePrefab, _nrb2d.transform.position, rotation, Object.InputAuthority);

            if (projectile != null)
            {
                projectile.Fire(direction.normalized);
            }
        }
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Camera.main.GetComponent<CameraScript>().target = GetComponent<NetworkTransform>().InterpolationTarget;
            print("if id: "+no);
        }
        print("outside id: "+no);
        
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
}