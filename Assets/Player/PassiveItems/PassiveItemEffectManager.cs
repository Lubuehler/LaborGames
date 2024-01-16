using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEffect
{
    void SubscribeToActions(Weapon weapon);
    void EnhanceEffect();
    void ReduceEffect();
}


public class PassiveItemEffectManager : MonoBehaviour
{
    private Dictionary<Type, IEffect> effects = new Dictionary<Type, IEffect>();

    [SerializeField] private GameObject aoeEffectExplosionPrefab;
    [SerializeField] private LayerMask enemyLayer;

    private Dictionary<int, Func<IEffect>> itemEffectMappings;

    public void Initialize(Weapon weapon)
    {
        itemEffectMappings = new Dictionary<int, Func<IEffect>>()
        {
            {3085, () => new MultiTargetAttackEffect(weapon) },
            {3074, () => new AoEDamageEffect(weapon, aoeEffectExplosionPrefab, enemyLayer) },
            {3087, () => new RicochetEffect(weapon) }
        };
    }

    public void AddOrEnhanceEffect(int itemID)
    {
        if (!itemEffectMappings.ContainsKey(itemID))
        {
            return;
        }
        IEffect effect = itemEffectMappings[itemID]();
        Type effectType = effect.GetType();
        if (effects.ContainsKey(effectType))
        {
            effects[effectType].EnhanceEffect();
        }
        else
        {
            effects.Add(effectType, effect);
        }
    }
}
