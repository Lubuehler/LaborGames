using System.Collections;
using UnityEngine;
using Fusion;
using System;

public class ProjectileJet : NetworkBehaviour
{
    private NetworkRigidbody2D _rigidbody;
    public LayerMask collisionLayers;

    float maxDistance = 50f;

    private Jet firedBy;

    public void Fire(Vector2 direction, float speed, Jet jet)
    {
        _rigidbody.Rigidbody.velocity = direction * speed;
        this.firedBy = jet;
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(5);
        Runner.Despawn(this.GetComponent<NetworkObject>());
    }

    protected void Awake()
    {
        _rigidbody = GetComponent<NetworkRigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collisionLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            if (HasStateAuthority)
            {

                firedBy.OnProjectileHit(collision.gameObject.GetComponent<Player>());
                Destroy(gameObject);
            }
        }
    }
}
