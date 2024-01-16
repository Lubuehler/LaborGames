using Fusion;
using UnityEngine;

public class EnergyExplosion : NetworkBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float range = 40f;
    [SerializeField] private float duration = 5.0f;

    public override void Render()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, Vector2.zero, Mathf.Infinity, enemyLayer);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                hit.collider.gameObject.GetComponent<Enemy>().EMPHit(duration);
            }
        }
    }

    void OnParticleSystemStopped()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}
