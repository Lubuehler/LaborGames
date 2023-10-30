using Fusion;
using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    Drone,
    Jet,
}

public class Enemy : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int health = 100;
    [SerializeField] private double viewingDistance = 20d;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float separationRadius = 5f;

    protected NetworkRigidbody2D networkRigidbody2D;
    protected NetworkObject currentTarget;

    public override void Spawned()
    {
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();
        if (networkRigidbody2D == null)
        {
            Debug.LogError("NetworkRigidbody2D component not found on " + gameObject.name);
        }

        UpdateTarget();
    }

    protected virtual void Move()
    {
        if (currentTarget != null && networkRigidbody2D != null)
        {
            Vector2 toTarget = (currentTarget.transform.position - transform.position);
            Vector2 separationForce = CalculateSeparationForce();

            networkRigidbody2D.Rigidbody.velocity = (toTarget.normalized + separationForce).normalized * speed;
        }
    }

    protected List<Transform> GetNearbyEnemies()
    {
        List<Transform> nearbyEnemies = new List<Transform>();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyLayerMask);
        foreach (var hit in hits)
        {
            if (hit.transform != transform)
            {
                nearbyEnemies.Add(hit.transform);
            }
        }

        return nearbyEnemies;
    }

    protected Vector2 CalculateSeparationForce()
    {
        Vector2 separationForce = Vector2.zero;
        foreach (var otherEnemy in GetNearbyEnemies())
        {
            Vector2 toOtherEnemy = transform.position - otherEnemy.position;
            if (toOtherEnemy.magnitude < separationRadius)
            {
                separationForce += toOtherEnemy.normalized / toOtherEnemy.magnitude;
            }
        }
        return separationForce;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log("Enemy died");
        Runner.Despawn(GetComponent<NetworkObject>());
    }

    public override void Render()
    {
        UpdateTarget();
        Move();
    }

    protected void UpdateTarget()
    {
        if (currentTarget != null && (currentTarget.transform.position - transform.position).magnitude < viewingDistance)
        {
            return;
        }

        currentTarget = null;
        double closestDistance = double.MaxValue;

        if (GameController.Instance?.spawnedCharacters != null)
        {
            foreach (NetworkObject player in GameController.Instance.spawnedCharacters.Values)
            {
                Vector3 playerPosition = player.transform.position;
                double distance = (playerPosition - transform.position).magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = player;
                }
            }
        }
        else
        {
            Debug.LogWarning("GameController or spawnedCharacters is null");
        }
    }
}
