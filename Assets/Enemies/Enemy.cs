using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

public enum EnemyType
{
    Drone,
    Jet,
    LaserDrone
}

public class Enemy : NetworkBehaviour
{
    //[SerializeField] protected float speed = 3f;
    [SerializeField] protected int health = 100;
    [SerializeField] protected double viewingDistance = 20d;
    [SerializeField] protected LayerMask enemyLayerMask;
    [SerializeField] protected LayerMask playerLayerMask;
    [SerializeField] protected float separationRadius = 5f;
    [SerializeField] protected float explosionRange = 1f;
    [SerializeField] protected float explosionDamage = 40f;
    [SerializeField] protected GameObject deathExplosionPrefab;


    protected NetworkRigidbody2D networkRigidbody2D;
    protected NetworkObject currentTarget;

    protected float movementSmoothing = .5f;

    protected bool movementDisabled = false;


    public override void Spawned()
    {
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();
        if (networkRigidbody2D == null)
        {
            Debug.LogError("NetworkRigidbody2D component not found on " + gameObject.name);
        }

        UpdateTarget();
    }

    public Vector2 getPosition()
    {
        return networkRigidbody2D.ReadPosition();
    }
    
    public Vector2 getVelocity()
    {
        return networkRigidbody2D.ReadVelocity();
    }

    public Transform getTransform()
    {
        return networkRigidbody2D.transform;
    }


    protected virtual void Move()
    {
        if (!movementDisabled)
        {
            if (currentTarget == null || !currentTarget.GetComponent<Player>().isAlive)
            {
                networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, Vector2.zero, Runner.DeltaTime * movementSmoothing);
                return;
            }
            Vector2 toTarget = currentTarget.transform.position - transform.position;
            Vector2 separationForce = CalculateSeparationForce();

            var speed = EnemySpawner.Instance.speed;

            Vector2 desiredVelocity = (toTarget.normalized + separationForce).normalized * speed;
            networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, desiredVelocity, Runner.DeltaTime * movementSmoothing);
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
        List<Transform> nearbyEnemies = GetNearbyEnemies();

        foreach (var otherEnemy in nearbyEnemies)
        {
            Vector2 toOtherEnemy = transform.position - otherEnemy.position;
            if (toOtherEnemy.sqrMagnitude < separationRadius * separationRadius)
            {
                separationForce += toOtherEnemy.normalized / (toOtherEnemy.sqrMagnitude + 0.1f); // Add a small value to prevent division by zero
            }
        }

        float maxSeparationForce = .4f;
        if (separationForce.magnitude > maxSeparationForce)
        {
            separationForce = separationForce.normalized * maxSeparationForce;
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



    public override void Render()
    {
        UpdateTarget();
        Move();
        DoSomething();
    }

    protected virtual void DoSomething() { }

    protected void UpdateTarget()
    {
        if (currentTarget == null ||
            !currentTarget.GetComponent<Player>().isAlive ||
            (currentTarget.transform.position - transform.position).magnitude < viewingDistance)
        {
            currentTarget = GetClosestPlayer();
        }
    }

    private NetworkObject GetClosestPlayer()
    {
        NetworkObject closestPlayer = null;
        double closestDistance = double.MaxValue;
        foreach (PlayerRef playerRef in Runner.ActivePlayers)
        {
            NetworkObject playerObject = NetworkController.Instance.GetPlayerAvatar(playerRef);
            if (!playerObject.GetComponent<Player>().isAlive)
            {
                continue;
            }
            Vector3 playerPosition = playerObject.transform.position;
            double distance = (playerPosition - transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = playerObject;
            }
        }
        return closestPlayer;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((playerLayerMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(explosionDamage);
            Explode();
        }
    }

    private void Explode()
    {

        Runner.Spawn(deathExplosionPrefab, transform.position, transform.rotation);
        Die();
    }

    protected virtual void Die()
    {
        EnemySpawner.Instance.EnemyDefeated(this, transform.position);
    }

    public void EMPHit()
    {
        movementDisabled = true;
        networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, Vector2.zero, Runner.DeltaTime * movementSmoothing);
        StartCoroutine(EnableMovementAfterDelay(5f));
    }

    private IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        movementDisabled = false;
    }
}
