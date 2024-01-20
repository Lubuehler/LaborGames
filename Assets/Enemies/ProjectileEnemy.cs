using Fusion;
using System.Collections;
using UnityEngine;

public class ProjectileEnemy : NetworkBehaviour
{
    private NetworkRigidbody2D _rigidbody;
    [SerializeField] private LayerMask collisionLayers;
    private ParticleSystem trail;

    [SerializeField] private GameObject hitExplosionPrefab;


    private Jet firedBy;

    public void Fire(Vector2 direction, float speed, Jet jet, ParticleSystem trail)
    {

        _rigidbody.Rigidbody.velocity = direction * speed;
        this.firedBy = jet;
        this.trail = trail;
        trail.Play();
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(5);
        Destroy(trail.gameObject);
        Runner.Despawn(GetComponent<NetworkObject>());
    }

    protected void Awake()
    {
        _rigidbody = GetComponent<NetworkRigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collisionLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            Vector3 directionToShooter = (transform.position - firedBy.getTransform().position).normalized;
            float offsetDistance = 0.2f;
            Vector3 explosionPosition = transform.position - (directionToShooter * offsetDistance);

            Instantiate(hitExplosionPrefab, explosionPosition, Quaternion.identity);
            if (HasStateAuthority)
            {
                firedBy.OnProjectileHit(collision.gameObject.GetComponent<Player>());
                Destroy(trail.gameObject);
                Runner.Despawn(GetComponent<NetworkObject>());
            }
        }
    }
}
