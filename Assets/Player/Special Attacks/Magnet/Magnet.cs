using Fusion;
using UnityEngine;

public class Magnet : NetworkBehaviour
{
    public void Activate(Transform playerTransform)
    {
        foreach (NetworkObject coin in EnemySpawner.Instance._spawnedCoins)
        {
            coin?.GetComponent<Coin>().SetTarget(playerTransform);
        }
    }
}
