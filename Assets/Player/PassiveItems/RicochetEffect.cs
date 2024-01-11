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
        this.jumpRange = 200;
        weapon.OnHitTarget += HandleOnHitTarget;
    }

    private void HandleOnHitTarget(Transform hitTarget)
    {
        Enemy enemy = hitTarget.GetComponent<Enemy>();
        if (enemy != null)
        {
            PerformRicochet(enemy, maxJumps);

        }
    }

    private void PerformRicochet(Enemy firstTarget, int jumpsRemaining)
    {
        Enemy currentTarget = firstTarget;
        while (jumpsRemaining > 0)
        {
            Enemy nextEnemy = LevelController.Instance.FindClosestEnemies(currentTarget.getPosition(), 1, jumpRange, ignoreEnemy: currentTarget).First();
            if (nextEnemy != null)
            {
                // Simulate an attack on the next target
                this.weapon.ReleaseBullet(nextEnemy.getTransform(), origin: currentTarget.getTransform());
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
