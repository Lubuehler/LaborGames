using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class Drone : Enemy
{
    [SerializeField] private GameObject smokeGrenadePrefab;
    [SerializeField] private const float throwCooldown = 30f;

    private float lastThrowTime = -30f;
    protected override void DoSomething()
    {
        if (Time.time - lastThrowTime >= throwCooldown)
        {
            if (Vector3.Distance(getPosition(), currentTarget.GetComponent<Player>().getPosition()) < 10)
            {
                RPC_throwSmokeGrenade();
                lastThrowTime = Time.time;
            }
        }
    }

    [Rpc]
    public void RPC_throwSmokeGrenade()
    {
        GameObject grenade = Instantiate(smokeGrenadePrefab, getPosition(), Quaternion.identity);
        grenade.GetComponent<SmokeGrenade>().ThrowToTargetPosition(currentTarget.GetComponent<Player>().getPosition());
    }
}