using Fusion;
using UnityEngine;

public class Drone : Enemy
{
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float explosionRange = 1f;
    [SerializeField] private float explosionDamage = 40f;

    public override void Render()
    {
        base.Render();

        if (currentTarget != null && networkRigidbody2D != null)
        {
            float distanceToTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
            if (distanceToTarget <= explosionRange)
            {
                Explode();
                currentTarget.GetComponent<Player>().TakeDamage(explosionDamage);
            }
        }
    }

    private void Explode()
    {

        Runner.Spawn(deathExplosionPrefab, transform.position, transform.rotation);
        Die();
    }
}

