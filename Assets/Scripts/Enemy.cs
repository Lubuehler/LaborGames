using Fusion;
using UnityEngine;

public enum EnemyType
{
    Drone,
    Jet,
}

public class Enemy : NetworkBehaviour
{
    public float speed = 3f;
    public int health = 100;
    public double viewingDistance = 20d;
    protected NetworkRigidbody2D networkRigidbody2D;
    protected NetworkObject currentTarget;

    public override void Spawned()
    {
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();
        UpdateTarget();
    }


    protected virtual void Move()
    {
        if (currentTarget != null)
        {
            networkRigidbody2D.Rigidbody.velocity = (currentTarget.transform.position - networkRigidbody2D.transform.position).normalized * speed;
        }
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
        // Default die implementation
        Debug.Log("Enemy died");
        Destroy(gameObject);
    }
    public override void Render()
    {
        UpdateTarget();
        Move();
    }
    protected void UpdateTarget()
    {
        if (currentTarget != null && (currentTarget.transform.position - networkRigidbody2D.transform.position).magnitude < viewingDistance)
        {
            return;
        }
        currentTarget = null;
        double closestDistance = double.MaxValue;
        foreach (NetworkObject player in GameController.Instance.spawnedCharacters.Values)
        {
            Vector3 playerPosition = player.transform.position;
            double distance = (playerPosition - networkRigidbody2D.transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = player;
            }
        }
    }
}
