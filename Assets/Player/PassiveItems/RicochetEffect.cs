using UnityEngine;
using System.Linq;

public class RicochetEffect : IEffect
{
    private Weapon weapon;
    private int maxJumps;
    private float jumpRange;
    private const int increasePerItem = 1;

    public RicochetEffect(Weapon weapon)
    {
        this.weapon = weapon;
        this.maxJumps = 1;
        this.jumpRange = 20;
        weapon.OnHitTarget += HandleOnHitTarget;
    }

    private void HandleOnHitTarget(Transform hitTarget)
    {
        PerformRicochet(hitTarget, maxJumps);
    }

    private void PerformRicochet(Transform currentTarget, int jumpsRemaining)
    {
        Transform nextTarget = currentTarget;

        while (jumpsRemaining > 0)
        {
            GameObject nextTargetObj = LevelController.Instance.FindClosestEnemies(nextTarget, 1, jumpRange).FirstOrDefault();
            if (nextTargetObj != null)
            {
                // Simulate an attack on the next target
                this.weapon.ReleaseBullet(nextTargetObj.transform);
            }
            else
            {
                // No more targets within range
                break;
            }

            jumpsRemaining--;
        }
    }

    void IEffect.ReduceEffect()
    {
        if (maxJumps > 0) maxJumps -= increasePerItem;
    }

    void IEffect.EnhanceEffect()
    {
        maxJumps += increasePerItem;
    }

    ~RicochetEffect()
    {
        weapon.OnHitTarget -= HandleOnHitTarget;
    }
}
