using Fusion;
using UnityEngine;

public class Drone : Enemy
{
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float explosionRange = 1f;

    public override void Render()
    {
        base.Render();

        if (currentTarget != null && networkRigidbody2D != null)
        {
            float distanceToTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
            if (distanceToTarget <= explosionRange)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {

        if (deathExplosionPrefab != null)
        {
            NetworkObject explosion = Runner.Spawn(deathExplosionPrefab, transform.position, transform.rotation);

            ParticleSystem particleSystem = explosion.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
            else
            {
                Debug.LogWarning("Explosion prefab does not have a ParticleSystem component!");
            }
        }
        else
        {
            Debug.LogWarning("Death explosion prefab is not assigned!");
        }

        // Destroy the drone after the explosion
        
        Die();
    }
}

