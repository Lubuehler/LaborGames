using Fusion;
using UnityEngine;

public class EnergyExplosion : NetworkBehaviour
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
