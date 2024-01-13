using Fusion;
using UnityEngine;

public class Magnet : NetworkBehaviour
{
    [SerializeField] private LayerMask coinLayer;
    [SerializeField] private float range = 20f;
    public void Activate(Transform playerTransform)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, Vector2.zero, Mathf.Infinity, coinLayer);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                hit.collider.gameObject.GetComponent<Coin>().SetTarget(playerTransform);
            }
        }
    }
}
