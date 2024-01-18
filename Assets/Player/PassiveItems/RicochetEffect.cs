using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RicochetEffect : IEffect
{
    private Weapon weapon;
    private int maxJumps = 1;
    private float jumpRange = 7;
    private const int increasePerItem = 1;
    private Dictionary<int, int> ricochetsRemainingPerShot = new Dictionary<int, int>();

    public RicochetEffect(Weapon weapon)
    {
        ((IEffect)this).SubscribeToActions(weapon);
    }

    private void HandleOnHitTarget(Vector2 position, int targetID, int shotID)
    {
        if (!ricochetsRemainingPerShot.ContainsKey(shotID))
        {
            ricochetsRemainingPerShot[shotID] = maxJumps;
        }
        if (ricochetsRemainingPerShot[shotID] <= 0)
        {
            ricochetsRemainingPerShot.Remove(shotID);
            return;
        }

        PerformRicochet(position, targetID, shotID);

    }

    private void PerformRicochet(Vector2 position, int targetID, int shotID)
    {
        ricochetsRemainingPerShot[shotID]--;
        Enemy nextEnemy = LevelController.Instance.FindClosestEnemies(position, 1, jumpRange, ignoreEnemyWithId: targetID).FirstOrDefault();
        if (nextEnemy != null)
        {
            weapon.ReleaseBullet(nextEnemy, shotID, origin: position);
        }

    }

    void IEffect.SubscribeToActions(Weapon weapon)
    {
        this.weapon = weapon;
        weapon.OnHitTarget += HandleOnHitTarget;
    }

    public void Unsubscribe()
    {
        weapon.OnHitTarget -= HandleOnHitTarget;
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
