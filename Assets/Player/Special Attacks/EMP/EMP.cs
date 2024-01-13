using UnityEngine;
using Fusion;

public class EMP : NetworkBehaviour
{
    [SerializeField] private GameObject energyExplosionPrefab;
    public void Activate(NetworkRigidbody2D rigidbody)
    {
        Runner.Spawn(energyExplosionPrefab, rigidbody.transform.position, rigidbody.transform.rotation);
    }
}
