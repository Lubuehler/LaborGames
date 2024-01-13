using UnityEngine;

public class AoEDamageEffect : IEffect
{
    private Weapon weapon;
    private float damageRadius = 4;
    private GameObject visualEffectPrefab;
    private LayerMask enemyLayer;
    private float radiusIncrease = 4;

    public AoEDamageEffect(Weapon weapon, GameObject visualEffectPrefab, LayerMask enemyLayer)
    {
        ((IEffect)this).SubscribeToActions(weapon);
        this.visualEffectPrefab = visualEffectPrefab;

        this.enemyLayer = enemyLayer;
    }

    private void HandleOnHitTarget(Vector2 position, int targetID, int shotID)
    {
        Debug.Log("HandleOnHit AOE");
        if (visualEffectPrefab != null)
        {
            GameObject effect = GameObject.Instantiate(visualEffectPrefab, position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * damageRadius;
            GameObject.Destroy(effect, 2.0f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(position, damageRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.GetInstanceID() == targetID)
            {
                continue;
            }
            weapon.OnBulletHit(hitCollider.GetComponent<Enemy>(), shotID);

        }

    }

    public void EnhanceEffect()
    {
        damageRadius += radiusIncrease;
    }

    public void ReduceEffect()
    {
        damageRadius -= radiusIncrease;
    }

    public void SubscribeToActions(Weapon weapon)
    {
        this.weapon = weapon;
        weapon.OnHitTarget += HandleOnHitTarget;

    }

    ~AoEDamageEffect()
    {
        if (weapon != null)
        {
            weapon.OnHitTarget -= HandleOnHitTarget;
        }
    }
}