using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EMP : NetworkBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    public override void Render()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 40f, Vector2.zero, Mathf.Infinity, enemyLayer);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                hit.collider.gameObject.GetComponent<Enemy>().EMPHit();
            }
        }
    }

    void OnParticleSystemStopped()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}
