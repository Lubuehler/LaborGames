using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTargetAttackEffect : IEffect
{
    private Weapon weapon;
    private int additionalAttacks = 2;
    private const int increasePerItem = 1;

    public MultiTargetAttackEffect(Weapon weapon)
    {
        this.weapon = weapon;
        weapon.OnAttack += HandleOnAttack;
    }


    private void HandleOnAttack(Transform weapon, Transform target)
    {
        List<Enemy> enemies = LevelController.Instance.FindClosestEnemies(target.position, additionalAttacks, 5f);
        foreach (Enemy enemy in enemies)
        {
            this.weapon.ReleaseBullet(enemy.getTransform());
        }
    }

    void IEffect.ReduceEffect()
    {
        if (additionalAttacks == 0) { return; }
        additionalAttacks -= increasePerItem;
    }

    void IEffect.EnhanceEffect()
    {
        additionalAttacks += increasePerItem;
    }


    ~MultiTargetAttackEffect()
    {
        weapon.OnAttack -= HandleOnAttack;
    }
}