using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEffect
{
    void SubscribeToActions(Weapon weapon);
    void EnhanceEffect();
    void ReduceEffect();
    void Unsubscribe();
}


public class PassiveItemEffectManager : MonoBehaviour
{
    private Dictionary<int, IEffect> effects;

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
        effects = new Dictionary<int, IEffect>();
    }

    public void Clear()
    {
        foreach (IEffect effect in effects.Values)
        {
            effect.Unsubscribe();
        }
        effects.Clear();
    }

    public void AddOrEnhanceEffect(int itemID)
    {
        if (!itemEffectMappings.ContainsKey(itemID))
        {
            return;
        }

        if (effects.ContainsKey(itemID))
        {
            effects[itemID].EnhanceEffect();
        }
        else
        {
            IEffect effect = itemEffectMappings[itemID]();
            effects.Add(itemID, effect);
        }
    }
}
