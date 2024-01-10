using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEDamageEffect : IEffect
{
    private Weapon weapon;
    private float damageRadius;
    private float damageAmount = 10;
    private GameObject visualEffectPrefab;
    private LayerMask enemyLayer;
    private float damageIncrease = 10;


    public AoEDamageEffect(Weapon weapon, GameObject visualEffectPrefab, LayerMask enemyLayer)
    {
        this.weapon = weapon;
        this.visualEffectPrefab = visualEffectPrefab;

        weapon.OnHitTarget += HandleOnHitTarget;
        this.enemyLayer = enemyLayer;
    }

    private void HandleOnHitTarget(Transform target)
    {
        // Deal damage in a circle around the target
        Collider[] hitColliders = Physics.OverlapSphere(target.position, damageRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            ApplyDamage(hitCollider.gameObject);
        }

        // Display the visual effect
        if (visualEffectPrefab != null)
        {
            GameObject effect = GameObject.Instantiate(visualEffectPrefab, target.position, Quaternion.identity);
            GameObject.Destroy(effect, 2.0f); // Destroy the effect after 2 seconds
        }
    }

    private void ApplyDamage(GameObject enemyObject)
    {
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.TakeDamage(((int)damageAmount));
    }

    public void EnhanceEffect()
    {
        damageAmount += damageIncrease;
    }

    public void ReduceEffect()
    {
        damageAmount -= damageIncrease;
    }

    ~AoEDamageEffect()
    {
        if (weapon != null)
        {
            weapon.OnHitTarget -= HandleOnHitTarget;
        }
    }
}