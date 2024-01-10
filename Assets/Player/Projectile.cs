using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class Projectile : NetworkBehaviour
{
    private NetworkRigidbody2D _rigidbody;
    public float speed = 20.0f;
    public int damage = 50;
    public LayerMask collisionLayers;

    private Func<GameObject, bool> onHit;

    public void Fire(Vector2 direction, Func<GameObject, bool> onHit)
    {
        _rigidbody.Rigidbody.velocity = direction * speed;
        this.onHit = onHit;
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

    // TODO change to OverlapBox
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision involves the specified layers.
        if ((collisionLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            onHit(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
